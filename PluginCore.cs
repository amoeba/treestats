using System;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    [FriendlyName("TreeStats")]
    public class PluginCore : PluginBase
    {
        public static PluginHost MyHost;
        public static CoreManager MyCore;

        // Establish base URL for all queries
        public static string urlBase = "http://treestats.net/";

        protected override void Startup()
        {
            try
            {
                string dir = setupPluginDir();
                Logging.Init(dir + "\\messages.txt", dir + "\\errors.txt");

                MigrateLegacyPluginDirLocation(dir);

                MyHost = Host;
                MyCore = Core;

                // Plugin setup
                Account.Init(MyCore);
                Character.Init(MyCore, MyHost);
                Settings.Init(dir + "\\settings.txt");
                Util.Init(MyHost);
                Settings.Load();
                MainView.ViewInit(Path.ToString() + "//icon.bmp");

                // Bind events
                Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
                Core.CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(Core_CommandLineText);
                Core.EchoFilter.ServerDispatch += new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        protected override void Shutdown()
        {
            try
            {
                MyHost = null;
                MyCore = null;

                Account.Destroy();
                Character.Destroy();
                Util.Destroy();
                Settings.Destroy();
                MainView.ViewDestroy();

                // Unbind events
                Core.CharacterFilter.LoginComplete -= new EventHandler(CharacterFilter_LoginComplete);
                Core.EchoFilter.ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
                Core.CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(Core_CommandLineText);
                
                Logging.Destroy();
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        /**
         * Attempt to find a suitable place to store log and setting files and ensure
         * that directory exists.
         * 
         * Falls back to the same folder as the TreeStats DLL.
         */
        string setupPluginDir()
        {
            string dir = null;

            try {
                dir = string.Format(@"{0}\{1}\{2}",
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Decal Plugins",
                    "TreeStats");

                Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                // noop
            }
            finally
            {
                if (dir == null)
                {
                    dir = Path;
                }
            }

            try
            {
                if (!Directory.Exists(dir))
                {
                    dir = Path;
                }
            } catch (Exception ex)
            {
                // noop
            }

            return dir;
        }
        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                Logging.LogMessage("LoginComplete");
                Logging.LogMessage("  Server:" + Core.CharacterFilter.Server);
                Logging.LogMessage("  Character: " + Core.CharacterFilter.Name);
                Logging.LogMessage("  ShouldSend() : " + Settings.ShouldSendCharacter(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name).ToString());
                Logging.LogMessage("  ShouldLogin() " + Account.ShouldLogin().ToString());

                // Log in (if applicable)
                if (Account.ShouldLogin())
                {
                    Account.Login(Settings.accountName, Settings.accountPassword);
                }
                else
                {
                    // Then upload (if we should)
                    if (Settings.ShouldSendCharacter(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name))
                    {
                        Character.DoUpdate();
                    }
                }
                }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        void Core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                string text = e.Text.ToLower();

                if (text.StartsWith("@treestats") || text.StartsWith("/treestats"))
                {
                    e.Eat = true;
                    string[] tokens = text.Split(' ');

                    if (tokens.Length <= 1)
                    {
                        Settings.ShowHelp();
                    }
                    else if (tokens.Length >= 2)
                    {
                        string command = tokens[1];

                        if (command == "help")
                        {
                            Settings.ShowHelp();
                        }
                        else if (command == "send")
                        {
                            Character.TryUpdate(true);
                        }
                        else if (command == "mode")
                        {
                            Settings.ToggleMode();
                            Settings.Save();
                        }
                        else if (command == "add")
                        {
                            Settings.AddCharacter(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name);
                            Settings.Save();
                        }
                        else if (command == "rem")
                        {
                            Settings.RemoveCharacter(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name);
                            Settings.Save();
                        }
                        else if (command == "account")
                        {
                            // @treestats login name password
                            if (tokens.Length < 5)
                            {
                                Settings.ShowHelp();

                                return;
                            }

                            string subcommand = tokens[2];

                            if (subcommand == "create")
                            {
                                Account.Create(tokens[3], tokens[4]);

                                return;
                            } 
                            else if (subcommand == "login")
                            {
                                Account.Login(tokens[3], tokens[4]);
                                Settings.useAccount = true;

                                return;

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        void EchoFilter_ServerDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                if (e.Message.Type == 0xF7B0) // Game Event
                {
                    if ((int)e.Message["event"] == 0x0029) // Titles list
                    {
                        Character.ProcessTitlesMessage(e);
                    }
                    else if ((int)e.Message["event"] == 0x0013) // Login Character
                    {
                        Character.ProcessCharacterPropertyData(e);
                    }
                    else if ((int)e.Message["event"] == 0x0020) // Allegiance info
                    {
                        Character.ProcessAllegianceInfoMessage(e);
                    }
                    else if ((int)e.Message["event"] == 0x002b) // Set title
                    {
                        Character.ProcessSetTitleMessage(e);
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        private void MigrateLegacyPluginDirLocation(string pluginDir)
        {
            string oldSettingsPath = Path + "\\settings.txt";
            string currentSettingsPath = pluginDir + "\\settings.txt";

            if (File.Exists(oldSettingsPath) && !File.Exists(currentSettingsPath))
            {
                try
                {
                    File.Copy(oldSettingsPath, currentSettingsPath);
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                }
            }
        }
    }
}
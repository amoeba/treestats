using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    [FriendlyName("TreeStats")]
    public class PluginCore : PluginBase
    {
        public static PluginHost MyHost;
        public static CoreManager MyCore;

        protected override void Startup()
        {
            try
            {
                Logging.Init(Path.ToString() + "\\messages.txt", Path.ToString() + "\\errors.txt");

                MyHost = Host;
                MyCore = Core;

                // Plugin setup
                Character.Init(MyCore, MyHost);
                Settings.Init(Path.ToString() + "\\settings.txt");
                Util.Init(MyHost);

                // Load settings
                Settings.Load();

                // Load View
                MainView.ViewInit();

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

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                Logging.LogMessage("LoginComplete");
                Logging.LogMessage("  Server:" + Core.CharacterFilter.Server);
                Logging.LogMessage("  Character: " + Core.CharacterFilter.Name);

                // Log in (if applicable)
                if (Account.ShouldLogin())
                {
                    Account.Login(Settings.accountName, Settings.accountPassword);
                }
                else
                {

                    // Upload (if applicable)
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
                    string[] tokens = text.Split(' ');

                    if (tokens.Length <= 1)
                    {
                        Settings.ShowHelp();
                    }
                    else if (tokens.Length == 2)
                    {
                        string command = tokens[1];

                        Util.WriteToChat("Command is " + command);

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
    }
}
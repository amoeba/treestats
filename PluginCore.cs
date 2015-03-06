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
        public static bool isLoggedIn;

        protected override void Startup()
        {
            try
            {
                // Plugin setup
                TreeStats2.Init(Core, Host);
                Settings.Init(Path.ToString() + "\\settings.txt");
                Logging.Init(Path.ToString() + "\\messages.txt", Path.ToString() + "\\errors.txt");

                isLoggedIn = false;

                // Load settings
                Settings.Load();

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
                // Unbind events
                Core.CharacterFilter.LoginComplete -= new EventHandler(CharacterFilter_LoginComplete);
                Core.EchoFilter.ServerDispatch -=new EventHandler<NetworkMessageEventArgs>(EchoFilter_ServerDispatch);
                Core.CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(Core_CommandLineText);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            if (Settings.ShouldSend(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name))
            {
                TreeStats.GetPlayerInfo();
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
                    else if(tokens.Length == 2)
                    {
                        string command = tokens[1];

                        Util.WriteToChat("Command is " + command);

                        if (command == "help")
                        {
                            Settings.ShowHelp();
                        }
                        else if (command == "send")
                        {
                            TreeStats.GetPlayerInfo();
                        }
                        else if (command == "mode")
                        {
                            Util.WriteToChat("mode command");

                            Settings.ToggleMode();
                            Settings.Save();
                        }
                        else if (command == "add")
                        {
                            Util.WriteToChat("add command");

                            Settings.AddChar(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name);
                            Settings.Save();
                        }
                        else if (command == "rem")
                        {
                            Util.WriteToChat("rem command");

                            Settings.RemoveChar(Core.CharacterFilter.Server + "-" + Core.CharacterFilter.Name);
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
            if (e.Message.Type == 0xF7B0) // Game Event
            {
                if ((int)e.Message["event"] == 0x0029) // Titles list
                {
                    TreeStats2.ProcessTitlesMessage(e);
                }
                else if ((int)e.Message["event"] == 0x0013) // Augmentation info
                {
                    TreeStats2.ProcessAugmentationsMessage(e);
                }
                else if ((int)e.Message["event"] == 0x0020) // Allegiance info
                {
                    TreeStats2.ProcessAllegianceInfoMessage(e);
                }
            }
        }
    }
}

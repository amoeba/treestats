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
                Logging.Init(Path.ToString() + "\\messages.txt", Path.ToString() + "\\errors.txt");

                // Bind events
                Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
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
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            TreeStats2.GetPlayerInfo();
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

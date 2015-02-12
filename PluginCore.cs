using System;
using System.Collections.Generic;
using System.Text;
using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats2
{
    [FriendlyName("TreeStats2")]
    public class PluginCore : PluginBase
    {
        protected override void Startup()
        {
            try
            {
                
                TreeStats2.Init(Core, Host);
                Logging.Init(Path.ToString() + "\\messages.txt", Path.ToString() + "\\errors.txt");

                Core.CharacterFilter.LoginComplete += new EventHandler(CharacterFilter_LoginComplete);
                
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
                Core.CharacterFilter.LoginComplete -= new EventHandler(CharacterFilter_LoginComplete);
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

    }
}

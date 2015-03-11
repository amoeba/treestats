using System;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Util
    {
        internal static PluginHost MyHost;

        public static void Init(PluginHost host)
        {
            MyHost = host;
        }

        public static void Destroy()
        {
            MyHost = null;
        }
        
        public static void WriteToChat(string message)
        {
            try
            {
                MyHost.Actions.AddChatText("[TreeStats] " + message, 1);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }
    }
}

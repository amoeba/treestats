using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public delegate void QueuedAction();

    public static class Util
    {
        public static PluginHost MyHost;

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
            MyHost.Actions.AddChatText(message, 1, 1);
        }
    }
}

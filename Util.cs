﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
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
            if (Settings.silent)
            {
                return;
            }

            MyHost.Actions.AddChatText("[TreeStats] " + message, 1, 1);
        }
    }
}

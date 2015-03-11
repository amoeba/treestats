using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Settings
    {
        public static string settingsFile { get; set; }

        // Tracked settings
        public static bool autoMode;
        public static List<string> trackedCharacters;


        internal static void Init(string _settingsFileName)
        {
            settingsFile = _settingsFileName;

            autoMode = false;
            trackedCharacters = new List<string>();
        }

        internal static void Destroy()
        {
            trackedCharacters.Clear();
            trackedCharacters = null;
        }

        internal static void Save()
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(settingsFile, false);

            // Write auto mode
            sw.WriteLine("auto:" + autoMode.ToString());

            if(trackedCharacters.Count > 0)
            {
                // Write character list
                string characters = "";

                foreach(string key in trackedCharacters)
                {
                    characters += key + "#";
                }

                characters.Remove(characters.Length - 1); // Remove last #
                sw.WriteLine(characters);
            }

            sw.Close();
        }

        internal static void Load()
        {
            if (!File.Exists(settingsFile))
            {
                return;
            } 
            
            System.IO.StreamReader sr = new System.IO.StreamReader(settingsFile, true);

            // Read in lines from the file
            string line = "";
            int i = 0;

            while (!sr.EndOfStream)
            {
                switch (i)
                {
                    case 0: // Line 1 is auto mode setting, True | False
                        line = sr.ReadLine();
                        string[] tokens = line.Split(':');

                        if (tokens.Length == 2 && tokens[0] == "auto")
                        {
                            // Try to grab token[1] as a bool

                            if (tokens[1] == "True")
                            {
                                autoMode = true;
                            }
                            else if (tokens[1] == "False")
                            {
                                autoMode = false;
                            }
                        }
                        break;
                    case 1: // Line 2 is character keys, #-delimited

                        line = sr.ReadLine();

                        string[] keys = line.Split('#');

                        foreach (string key in keys)
                        {
                            if(key.Length > 0)
                            {
                                AddChar(key);
                            }
                        }

                        break;
                    default:
                        break;
                }

                i += 1;
            }
        }

        internal static void ToggleMode()
        {
            try
            {
                if (autoMode == true)
                {
                    Util.WriteToChat("Setting mode to manual. You must now either use @treestats send or @treestats add to send characters.");

                    autoMode = false;
                }
                else
                {
                    Util.WriteToChat("Setting mode to automatic. Any characters you log into will be sent automatically.");

                    autoMode = true;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void AddChar(string key)
        {
            try
            {
                Util.WriteToChat("Now tracking " + key);

                if (!trackedCharacters.Exists(k => k == key))
                {
                    trackedCharacters.Add(key);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void RemoveChar(string key)
        {
            try
            {
                if(trackedCharacters.Exists(k => k == key))
                {
                    Util.WriteToChat("No longer tracking " + key);

                    trackedCharacters.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static bool ShouldSend(string key)
        {
            return Settings.autoMode || Settings.trackedCharacters.Exists(k => k == key);
        }

        internal static void ShowHelp()
        {
            Util.WriteToChat("Treestats help:");
            Util.WriteToChat("Available commands: help, send, mode, addchar, removechar");
            Util.WriteToChat("");
            Util.WriteToChat("help: Shows this message.");
            Util.WriteToChat("send: Sends the currently logged-in character (Max once per minute), no matter what other settings you have set.");
            Util.WriteToChat("mode: Toggle mode between automatic and manual tracking.");
            Util.WriteToChat("     auto: Automatically track new characters.");
            Util.WriteToChat("     manual: Only track characters you manually add. See add/rem");
            Util.WriteToChat("add: Enables tracking for the currently logged in character.");
            Util.WriteToChat("rem: Removes tracking for the currently logged in character.");
        }
    }
}
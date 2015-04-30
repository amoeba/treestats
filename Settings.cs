using System;
using System.Collections.Generic;
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
            try
            {
                settingsFile = _settingsFileName;

                autoMode = false;
                trackedCharacters = new List<string>();
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void Destroy()
        {
            trackedCharacters.Clear();
            trackedCharacters = null;
        }

        internal static void Save()
        {
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(settingsFile, false);

                // Write auto mode
                sw.WriteLine("auto:" + autoMode.ToString());

                if (trackedCharacters.Count > 0)
                {
                    // Write character list
                    string characters = "";

                    foreach (string key in trackedCharacters)
                    {
                        characters += key + "#";
                    }

                    characters = characters.Remove(characters.Length - 1); // Remove last #

                    sw.WriteLine(characters);
                }

                sw.Close();
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void Load()
        {
            try
            {
                Logging.LogMessage("Load()"); 
                
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
                                if (key.Length > 0)
                                {
                                    AddCharacter(key);
                                }
                            }

                            break;
                        default:
                            break;
                    }

                    i += 1;
                }

            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void SetAutoMode(bool state)
        {
            try
            {
                if (state == true)
                {
                    Util.WriteToChat("Setting mode to automatic. Any characters you log into will be uploaded automatically.");
                    autoMode = true;
                }
                else
                {
                    Util.WriteToChat("Setting mode to manual. Only characters you have explicitly added will be uploaded automatically.");
                    autoMode = false;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void ToggleMode()
        {
            try
            {
                if (autoMode == true)
                {
                    SetAutoMode(true);
                }
                else
                {
                    SetAutoMode(false);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void AddCharacter(string key)
        {
            try
            {
                if (!trackedCharacters.Exists(k => k == key))
                {
                    Util.WriteToChat("Now tracking " + key + ".");

                    trackedCharacters.Add(key);
                }
                else
                {
                    Util.WriteToChat("Already tracking " + key + ".");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void RemoveCharacter(string key)
        {
            try
            {
                if (trackedCharacters.Exists(k => k == key))
                {
                    Util.WriteToChat("No longer tracking " + key + ".");

                    trackedCharacters.Remove(key);
                }
                else
                {
                    Util.WriteToChat("Not already tracking " + key + ".");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static bool ShouldSend(string key)
        {
            try
            {
                return Settings.autoMode || Settings.trackedCharacters.Exists(k => k == key);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }

            return false;
        }

        internal static void ShowHelp()
        {
            try
            {
                Util.WriteToChat("Treestats help:");
                Util.WriteToChat("Available commands: help, send, mode, addchar, removechar");
                Util.WriteToChat("");
                Util.WriteToChat("help: Shows this message.");
                Util.WriteToChat("send: Sends the currently logged-in character (Max once per minute), no matter what other settings you have set.");
                Util.WriteToChat("mode: Toggle mode between automatic and manual tracking.");
                Util.WriteToChat("     auto: Automatically upload characters.");
                Util.WriteToChat("     manual: Only upload characters you manually add. See add/rem.");
                Util.WriteToChat("add: Enables tracking for the currently logged in character.");
                Util.WriteToChat("rem: Removes tracking for the currently logged in character.");
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }
    }
}
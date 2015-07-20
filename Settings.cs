using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Settings
    {
        // Accessor for settings file
        public static string settingsFile { get; set; }

        // Tracked settings
        public static bool autoMode;
        public static List<string> trackedCharacters;
        public static bool useAccount;
        public static string accountName;
        public static string accountPassword;
        public static bool isLoggedIn;

        internal static void Init(string _settingsFileName)
        {
            try
            {
                settingsFile = _settingsFileName;

                autoMode = false;
                trackedCharacters = new List<string>();
                useAccount = false;
                accountName = "";
                accountPassword = "";
                isLoggedIn = false;
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void Destroy()
        {
            settingsFile = null;
            trackedCharacters.Clear();
            trackedCharacters = null;
            accountName = null;
            accountPassword = null;
        }

        internal static void Save()
        {
            try
            {
                Logging.LogMessage("Settings::Save()");

                System.IO.StreamWriter sw = new System.IO.StreamWriter(settingsFile, false);

                // Write auto mode
                sw.WriteLine("auto:" + autoMode.ToString());

                // Write account info if present
                sw.WriteLine("use_account:" + useAccount.ToString());

                if (accountName.Length > 0 && accountPassword.Length > 0)
                {
                    sw.WriteLine("account:" + accountName + "#" + accountPassword);
                }

                // Write tracked characters
                if (trackedCharacters.Count > 0)
                {
                    // Write character list
                    StringBuilder characters = new StringBuilder();
                    characters.Append("characters:");

                    foreach (string key in trackedCharacters)
                    {
                        characters.AppendFormat("{0}#", key);
                    }

                    characters = characters.Remove(characters.Length - 1, 1); // Remove last #

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
                Logging.LogMessage("Settings::Load()"); 
                
                if (!File.Exists(settingsFile))
                {
                    return;
                }

                System.IO.StreamReader sr = new System.IO.StreamReader(settingsFile, true);

                // Read in lines from the file
                string line = String.Empty;
                string[] tokens;
                string[] keys;

                while ((line = sr.ReadLine()) != null)
                {
                    tokens = line.Split(':');

                    switch (tokens[0])
                    {
                        case "auto": // Auto mode setting, True | False
                            Logging.LogMessage("Reading auto mode setting with stored value of " + tokens[1]);

                            // Try to grab token[1] as a bool
                            if (tokens[1] == "True")
                            {
                                autoMode = true;
                            }
                            else if (tokens[1] == "False")
                            {
                                autoMode = false;
                            }

                            break;
                        case "characters": // Character keys, #-delimited
                            Logging.LogMessage("Reading characters setting with stored value of " + tokens[1]);

                            keys = tokens[1].Split('#');

                            foreach (string key in keys)
                            {
                                if (key.Length > 0)
                                {
                                    AddCharacter(key);
                                }
                            }

                            break;
                        case "use_account": // Line 3 is whether to use an account
                            Logging.LogMessage("Reading use_account setting with stored value of " + tokens[1]);

                            // Try to grab token[1] as a bool
                            if (tokens[1] == "True")
                            {
                                useAccount = true;
                            }
                            else if (tokens[1] == "False")
                            {
                                useAccount = false;
                            }

                            break;
                        case "account": // Line 4 is account info, #-separated
                            Logging.LogMessage("Reading account setting with stored value of " + tokens[1]);
                            keys = tokens[1].Split('#');

                            if (keys.Length != 2 && keys[0].Length > 0 && keys[1].Length > 0)
                            {
                                break;
                            }

                            accountName = keys[0];
                            accountPassword = keys[1];

                            break;
                        default:
                            /* Handle legacy support of old settings file format:
                             * 
                             * The first settings file format had no key name, 
                             * i.e. 'characters:Kolthar al Magus-WintersEbb'
                             * and was instead just 'Kolthar al Magus-WintersEbb'.
                             * This default route should catch old settings files.
                             */

                            keys = line.Split('#');
                            Logging.LogMessage("Found " + keys.Length.ToString() + " characters.");

                            foreach (string key in keys)
                            {
                                Logging.LogMessage("  " + key);

                                if (key.Length > 0)
                                {
                                    AddCharacter(key);
                                }
                            }

                            break;
                    }
                }

                sr.Close();
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

        internal static bool ShouldSendCharacter(string key)
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
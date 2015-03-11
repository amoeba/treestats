using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Threading;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Character
    {
        public static CoreManager Core { get; set; }
        public static PluginHost Host { get; set; }


        // Throttle sending to once per minute
        public static DateTime lastSend;

        // Store latest message 
        public static string lastMessage;

        // Make an area to store information
        // These are stored for later because we get some of
        // this information before we login and are ready to 
        // send the character update.

        public static string character;
        public static string server;
        public static string allegianceName;
        public static int currentTitle;
        public static List<Int32> titlesList;
        public static Int64 luminance_earned;
        public static Int64 luminance_total;

        // Store character properties from GameEvent/Login Character message
        public static Dictionary<Int32, Int32> characterProperties;


        // DWORD Blacklist
        // We're going to grab all the Character Property DWORD values
        // becuase we haven't figured all of the ones we want.
        // But there are some we know we never want and we'll blacklist those.

        public static List<Int32> dwordBlacklist;

        // Resources
        // http://www.immortalbob.com/phpBB3/viewtopic.php?f=24&t=100&start=10
        // http://pastebin.com/X05rYnYU
        // http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/VTClassic%20Shared/Constants.cs

        internal static void Init(CoreManager _core, PluginHost _host)
        {
            Logging.loggingState = true;

            Core = _core;
            Host = _host;

            lastMessage = null;

            // General character info
            currentTitle = -1;
            titlesList = new List<Int32>();

            allegianceName = "";

            luminance_earned = -1;
            luminance_total = -1;

            // Store all returned character properties from the Login Player event
            characterProperties = new Dictionary<Int32, Int32>();

            // A list of dwords we know we don't want to save
            dwordBlacklist = new List<Int32>()
            {
                 2,5,7,10,17,19,20,24,25,26,28,30,33,35,36,38,43,45,86,87,88,89,90,91,
                 92,98,105,106,107,108,109,110,111,113,114,115,117,125,129,131,134,158,
                 159,160,166,170,171,172,174,175,176177,178,179,188,193,270,271,272,293
            };
        }

        internal static void Destroy()
        {
            lastMessage = null;

            characterProperties.Clear();
            characterProperties = null;
            
            dwordBlacklist.Clear();
            dwordBlacklist = null;
        }

        internal static void DoUpdate()
        {
            GetCharacterInfo();
            SendCharacterInfo(lastMessage);
        }

        /* GetPlayerInfo()
         * Gets player information
         *  
         * This method builds a JSON request string which is later encrypted
         * It would be really nice to use a propert data structure for
         * this and using Json.NET to serialize that data structure.
         * 
         * Some of the stuff here is stored beforehand from other messages
         * Most of it is taken from CharacterFilter once login is completed
         * 
         * Saves the concatenated JSON string to class variable lastMessage
         */

        internal static void GetCharacterInfo()
        {
            try
            {
                // One long string stores the entire POST request body
                string json = "{";

                // Declare the fileservice for later use
                Decal.Adapter.Wrappers.CharacterFilter cf = Core.CharacterFilter;
                Decal.Filters.FileService fs = CoreManager.Current.FileService as Decal.Filters.FileService;

                // Update character and server for later
                character = cf.Name;
                server = cf.Server;

                // Exit if we can't get Name and Server from CharacterFilter
                if (character.Length <= 0 && server.Length <= 0)
                {
                    return;
                }


                // General attributes
                json += "\"version\":\"1\",";
                json += "\"name\":\"" + cf.Name + "\",";       
                json += "\"race\":\"" + cf.Race + "\",";
                json += "\"gender\":\"" + cf.Gender + "\",";
                json += "\"level\":" + cf.Level + ",";

                // Add allegiance name if we've gotten it in a message
                if (allegianceName.Length > 0)
                {
                    json += "\"allegiance_name\":\"" + allegianceName + "\",";
                }

                json += "\"rank\":" + cf.Rank + ",";
                json += "\"followers\":" + cf.Followers.ToString() + ",";
                json += "\"server\":\"" + cf.Server + "\",";
                json += "\"server_population\":" + cf.ServerPopulation.ToString() + ",";
                json += "\"deaths\":" + cf.Deaths.ToString() + ",";
                json += "\"birth\":\"" + cf.Birth + "\",";
                json += "\"total_xp\":" + cf.TotalXP.ToString() + ",";
                json += "\"unassigned_xp\":" + cf.UnassignedXP.ToString() + ",";
                json += "\"skill_credits\":" + cf.SkillPoints.ToString() + ",";
                

                // Luminance XP

                if (luminance_earned != -1)
                {
                    json += "\"luminance_earned\":" + luminance_earned.ToString() + ",";
                }

                if (luminance_total != -1)
                {
                    json += "\"luminance_total\":" + luminance_total.ToString() + ",";
                }

                // Attributes
                json += "\"attribs\":{";

                foreach (var attr in Core.CharacterFilter.Attributes)
                {
                    json += "\"" + attr.Name.ToLower() + "\":{\"name\":\"" + attr.Name + "\",\"base\":" + attr.Base.ToString() + ",\"creation\":" + attr.Creation.ToString() + "},";
                   
                }

                json = json.Remove(json.Length - 1);
                json += "},";


                // Vitals
                json += "\"vitals\":{";

                foreach (var vital in Core.CharacterFilter.Vitals)
                {
                    json += "\"" + vital.Name.ToLower() + "\":{\"name\":\"" + vital.Name + "\",\"base\":" + vital.Base.ToString() + "},";
                }

                json = json.Remove(json.Length - 1);
                json += "},";


                // Skills
                string skill_text = "";
                Decal.Interop.Filters.SkillInfo skillinfo = null;

                json += "\"skills\":{";

                for (int i = 0; i < fs.SkillTable.Length; ++i)
                {
                    try
                    {
                        skillinfo = Core.CharacterFilter.Underlying.get_Skill((Decal.Interop.Filters.eSkillID)fs.SkillTable[i].Id);
                        string name = skillinfo.Name.ToLower().Replace(" ", "_");
                        skill_text += name + "&" + skillinfo.Training.ToString() + "&" + skillinfo.Base.ToString() + "#";
                        json += "\""+ name + "\":{\"name\":\"" + name + "\",\"base\":" + skillinfo.Base + ",\"training\":\"" + skillinfo.Training.ToString().Substring(6) + "\"},";
                    }
                    finally
                    {
                        if (skillinfo != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(skillinfo);
                            skillinfo = null;
                        }
                    }
                }

                skill_text = skill_text.Remove(skill_text.Length - 1);

                json = json.Remove(json.Length - 1);
                json += "},";
                

                // Monarch & Patron Information
                // We wrap in try/catch because AllegianceInfoWrapper behaves oddly (Is not null when it should be? Not sure on this.)

                try
                {
                    json += "\"monarch\":{\"name\":\"" + cf.Monarch.Name + "\",\"race\":" + cf.Monarch.Race + ",\"rank\":" + cf.Monarch.Rank + ",\"gender\":" + cf.Monarch.Gender + ",\"followers\":" + cf.MonarchFollowers + "},";
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                }

                try
                {
                    json += "\"patron\":{\"name\":\"" + cf.Patron.Name + "\",\"race\":" + cf.Patron.Race + ",\"rank\":" + cf.Patron.Rank + ",\"gender\":" + cf.Patron.Gender + "},";
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                }


                // Vassals
                if (cf.Vassals != null && cf.Vassals.Count > 0)
                {
                    json += "\"vassals\":[";

                    
                    foreach (AllegianceInfoWrapper vassal in cf.Vassals)
                    {
                        json += "{\"name\":\"" + vassal.Name + "\",\"race\":" + vassal.Race + ",\"rank\":" + vassal.Rank + ",\"gender\":" + vassal.Gender + "},";
                    }
                    
                    json = json.Remove(json.Length - 1);
                    json += "],";
                }


                // Titles
                // Add titles to message if we have them
                if (currentTitle != -1)
                {
                    json += "\"current_title\":" + currentTitle.ToString() + ",";
                    json += "\"titles\":[";

                    //foreach(int titleId in titlesList)
                    for(int i = 0; i < titlesList.Count; i++)
                    {
                        //json += titleId.ToString() + ",";
                        json += titlesList[i].ToString() + ",";
                    }

                    // Remove final trailing comma
                    json = json.Remove(json.Length - 1);
                    json += "],";
                }


                // Character Properties
                if(characterProperties.Count > 0)
                {
                    json += "\"properties\":{";

                    foreach (var kvp in characterProperties)
                    {
                        json += "\"" + kvp.Key + "\":" + kvp.Value.ToString() + ",";
                    }

                    json = json.Remove(json.Length - 1);
                    json += "},";
                }

                // Remove final trailing comma
                json = json.Remove(json.Length - 1);
                
                // Add closing bracket
                json += "}";

                // Encrypt POST request
                lastMessage = Encryption.encrypt(json);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void ProcessTitlesMessage(NetworkMessageEventArgs e)
        {
            // Save current title
            currentTitle = e.Message.Value<Int32>("current");
            
            MessageStruct titles = e.Message.Struct("titles");

            for (int i = 0; i < titles.Count; i++)
            {
                // Add title to list

                // Check if exists first so multiple firings of the event don't make 
                // duplicate titles

                Int32 titleId = titles.Struct(i).Value<Int32>("title");

                if (!titlesList.Contains(titleId))
                {
                    titlesList.Add(titleId);
                }
            }
        }

        internal static void ProcessCharacterPropertyData(NetworkMessageEventArgs e)
        {
            try
            {
                MessageStruct props = e.Message.Struct("properties");
                MessageStruct dwords = props.Struct("dwords");
                MessageStruct qwords = props.Struct("qwords");

                MessageStruct tmpStruct;

                Int32 tmpKey;
                Int32 tmpValue;

                // Process DWORDS
                for (int i = 0; i < dwords.Count; i++)
                {
                    tmpStruct = dwords.Struct(i);

                    tmpKey = tmpStruct.Value<Int32>("key");
                    tmpValue = tmpStruct.Value<Int32>("value");

                    if (!dwordBlacklist.Contains(tmpKey))
                    {
                        characterProperties.Add(tmpKey, tmpValue);
                    }
                }


                // Process QWORDS
                Int64 qwordKey;
                Int64 qwordValue;

                for (int i = 0; i < qwords.Count; i++)
                {
                    tmpStruct = qwords.Struct(i);

                    qwordKey = tmpStruct.Value<Int64>("key");
                    qwordValue = tmpStruct.Value<Int64>("value");

                    if (qwordKey == 6)
                    {
                        luminance_earned = qwordValue;
                    }
                    else if (qwordKey == 7)
                    {
                        luminance_total = qwordValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void ProcessAllegianceInfoMessage(NetworkMessageEventArgs e)
        {
            try
            {
                allegianceName = e.Message.Value<string>("allegianceName");
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void ProcessSetTitleMessage(NetworkMessageEventArgs e)
        {
            Int32 title = e.Message.Value<Int32>("title");
            bool active = e.Message.Value<bool>("active");

            if (active)
            {
                currentTitle = title;
            }
        }

        internal static void SendCharacterInfo(string message)
        {
            try
            {
                //if(lastSend != DateTime.MinValue)
                //{
                //    TimeSpan diff = DateTime.Now - lastSend;

                //    if (diff.Minutes <= 1)
                //    {
                //        Util.WriteToChat("Failed to send character: Please wait " + (60 - diff.Seconds).ToString() + "s before sending again. Thanks.");
                //        return;
                //    }
                //}

                Logging.LogMessage(message);

                // If we got this far, we can send. Update last send DateTime and send
                lastSend = DateTime.Now;

                Util.WriteToChat("Sending character update.");
                
                // Do the sending
                Uri endpoint = new Uri("http://floating-meadow-8649.herokuapp.com/");

                using (var client = new WebClient())
                {
                    client.UploadStringCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                            Logging.LogError(e.Error);
                        else
                            Util.WriteToChat(e.Result);
                    };

                    client.UploadStringAsync(endpoint, "POST", message);
                    
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }
    }
}

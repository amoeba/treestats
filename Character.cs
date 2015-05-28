using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using WindowsTimer = System.Windows.Forms.Timer;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Character
    {
        public const string TreestatsURL = "http://treestats.herokuapp.com/";
        public static Uri endpoint;

        public static CoreManager MyCore { get; set; }
        public static PluginHost MyHost { get; set; }

        // Updates
        public static DateTime lastSend; // Throttle sending to once per minute
        public static WindowsTimer updateTimer; // Automatically send updates every hour
        public static bool sentServerPopulation; // Only send server pop the first time (after login)

        // Store latest message 
        public static string lastMessage = null;
        public static StringBuilder req;

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

        internal static void Init(CoreManager core, PluginHost host)
        {
            try
            {
                endpoint = new Uri(TreestatsURL);

                Logging.loggingState = true;

                MyCore = core;
                MyHost = host;

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


                // Set up timed updates
                updateTimer = new WindowsTimer();
                updateTimer.Interval = 1000 * 60 * 60; // One hour
                updateTimer.Tick += new EventHandler(updateTimer_Tick);
                updateTimer.Start();

                sentServerPopulation = false;
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void Destroy()
        {
            try
            {
                endpoint = null;

                lastMessage = null;

                characterProperties.Clear();
                characterProperties = null;

                dwordBlacklist.Clear();
                dwordBlacklist = null;

                if (updateTimer != null)
                {
                    updateTimer.Stop();
                    updateTimer.Tick -= updateTimer_Tick;
                    updateTimer.Dispose();
                    updateTimer = null;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void updateTimer_Tick(object sender, EventArgs e)
        {
            Logging.LogMessage("updateTimer_Tick");

            TryUpdate();
        }


        /* DoUpdate()
         * 
         * Sends an update without checking for the minimum send interval
         */

        internal static void DoUpdate()
        {
            try
            {
                GetCharacterInfo();
                SendCharacterInfo(lastMessage);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }


        /* TryUpdate()
         * 
         * Sends an update via DoUpdate() after checking for the minimum time interval
         */

        internal static void TryUpdate()
        {
            if (lastSend != DateTime.MinValue) // Null check: Can't do null checks on DateTimes, so we do this
            {
                TimeSpan diff = DateTime.Now - lastSend;

                if (diff.Minutes < 1) // Hard-coded one minute interval
                {
                    Util.WriteToChat("Failed to send character: Please wait " + (60 - diff.Seconds).ToString() + "s before sending again. Thanks.");

                    return;
                }
            }

            DoUpdate();
        }


        /* GetCharacterInfo()
         * 
         * Gets player information
         *  
         * This method builds a JSON request string which is later encrypted
         * It would be really nice to use a proper data structure for
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
                if (MyCore.CharacterFilter.Name == null)
                {
                    return;
                }

                req = new StringBuilder();

                // One long string stores the entire POST request body
                req.Append("{");

                // Declare the fileservice for later use
                Decal.Adapter.Wrappers.CharacterFilter cf = MyCore.CharacterFilter;
                Decal.Filters.FileService fs = CoreManager.Current.FileService as Decal.Filters.FileService;

                // Update character and server for later
                character = cf.Name;
                server = cf.Server;

                // General attributes
                req.AppendFormat("\"version\":\"{0}\",", 1);
                req.AppendFormat("\"name\":\"{0}\",", cf.Name);
                req.AppendFormat("\"race\":\"{0}\",", cf.Race);
                req.AppendFormat("\"gender\":\"{0}\",", cf.Gender);
                req.AppendFormat("\"level\":{0},", cf.Level);

                // Add allegiance name if we've gotten it in a message
                if (allegianceName.Length > 0)
                {
                    req.AppendFormat("\"allegiance_name\":\"{0}\",", allegianceName);
                }

                req.AppendFormat("\"rank\":{0},", cf.Rank);
                req.AppendFormat("\"followers\":{0},", cf.Followers);
                req.AppendFormat("\"server\":\"{0}\",", cf.Server);

                /* Only append server population if it hasn't been sent yet (we just logged in).
                / Character Filter only receives this value from the server and login, instead
                / of continuously. If we sent this each time we'd be reporting inaccurate server
                 * populations.
                */
                if (!sentServerPopulation)
                {
                    req.AppendFormat("\"server_population\":{0},", cf.ServerPopulation);
                    sentServerPopulation = true;
                }


                req.AppendFormat("\"deaths\":{0},", cf.Deaths);
                req.AppendFormat("\"birth\":\"{0}\",", cf.Birth);
                req.AppendFormat("\"total_xp\":{0},", cf.TotalXP);
                req.AppendFormat("\"unassigned_xp\":{0},", cf.UnassignedXP);
                req.AppendFormat("\"skill_credits\":{0},", cf.SkillPoints);


                // Luminance XP

                if (luminance_earned != -1)
                {
                    req.AppendFormat("\"luminance_earned\":{0},", luminance_earned);
                }

                if (luminance_total != -1)
                {

                    req.AppendFormat("\"luminance_total\":{0},", luminance_total);
                }

                // Attributes
                req.Append("\"attribs\":{");
                string attribs_format = "\"{0}\":{{\"name\":\"{1}\",\"base\":{2},\"creation\":{3}}},";

                foreach (var attr in MyCore.CharacterFilter.Attributes)
                {
                    req.AppendFormat(attribs_format, attr.Name.ToLower(), attr.Name, attr.Base, attr.Creation);
                }

                req.Remove(req.Length - 1, 1);
                req.Append("},");


                // Vitals
                req.Append("\"vitals\":{");
                string vitals_format = "\"{0}\":{{\"name\":\"{1}\",\"base\":{2}}},";

                foreach (var vital in MyCore.CharacterFilter.Vitals)
                {
                    req.AppendFormat(vitals_format, vital.Name.ToLower(), vital.Name, vital.Base);
                }

                req.Remove(req.Length - 1, 1);
                req.Append("},");


                // Skills
                Decal.Interop.Filters.SkillInfo skillinfo = null;

                req.Append("\"skills\":{");
                string skill_format = "\"{0}\":{{\"name\":\"{1}\",\"base\":{2},\"training\":\"{3}\"}},";

                string name;
                string training;

                for (int i = 0; i < fs.SkillTable.Length; ++i)
                {
                    try
                    {
                        skillinfo = MyCore.CharacterFilter.Underlying.get_Skill((Decal.Interop.Filters.eSkillID)fs.SkillTable[i].Id);

                        name = skillinfo.Name.ToLower().Replace(" ", "_");
                        training = skillinfo.Training.ToString().Substring(6);

                        req.AppendFormat(skill_format, name, name, skillinfo.Base, training);
                    }
                    finally
                    {
                        if (skillinfo != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(skillinfo);
                            skillinfo = null;
                        }

                        name = null;
                        training = null;
                    }
                }


                req.Remove(req.Length - 1, 1);
                req.Append("},");


                // Monarch & Patron Information
                // We wrap in try/catch because AllegianceInfoWrapper behaves oddly (Is not null when it should be? Not sure on this.)

                try
                {
                    if (cf.Monarch != null)
                    {
                        req.AppendFormat("\"monarch\":{{\"name\":\"{0}\",\"race\":{1},\"rank\":{2},\"gender\":{3},\"followers\":{4}}},", cf.Monarch.Name, cf.Monarch.Race, cf.Monarch.Rank, cf.Monarch.Gender, cf.MonarchFollowers);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                }

                try
                {
                    if (cf.Patron != null)
                    {
                        req.AppendFormat("\"patron\":{{\"name\":\"{0}\",\"race\":{1},\"rank\":{2},\"gender\":{3}}},", cf.Patron.Name, cf.Patron.Race, cf.Patron.Rank, cf.Patron.Gender);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError(ex);
                }


                // Vassals
                if (cf.Vassals != null && cf.Vassals.Count > 0)
                {
                    req.Append("\"vassals\":[");
                    string vassal_format = "{{\"name\":\"{0}\",\"race\":{1},\"rank\":{2},\"gender\":{3}}},";

                    foreach (AllegianceInfoWrapper vassal in cf.Vassals)
                    {
                        req.AppendFormat(vassal_format, vassal.Name, vassal.Race, vassal.Rank, vassal.Gender);
                    }

                    req.Remove(req.Length - 1, 1);
                    req.Append("],");
                }


                // Titles
                // Add titles to message if we have them
                if (currentTitle != -1)
                {
                    req.AppendFormat("\"current_title\":{0},", currentTitle);
                    req.Append("\"titles\":[");

                    //foreach(int titleId in titlesList)
                    for (int i = 0; i < titlesList.Count; i++)
                    {
                        req.AppendFormat("{0},", titlesList[i]);
                    }

                    // Remove final trailing comma
                    req.Remove(req.Length - 1, 1);
                    req.Append("],");
                }


                // Character Properties
                if (characterProperties.Count > 0)
                {
                    req.Append("\"properties\":{");

                    string property_format = "\"{0}\":{1},";

                    foreach (var kvp in characterProperties)
                    {
                        req.AppendFormat(property_format, kvp.Key, kvp.Value);
                    }

                    req.Remove(req.Length - 1, 1);
                    req.Append("},");
                }

                req.Remove(req.Length - 1, 1);
                req.Append("}");

                // Encrypt POST request
                
                lastMessage = Encryption.encrypt(req.ToString());
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void SendCharacterInfo(string message)
        {
            try
            {
                lastSend = DateTime.Now;

                if (message == null || message.Length < 1)
                {
                    return;
                }

                Util.WriteToChat("Sending character update.");

                // Do the sending
                using (var client = new WebClient())
                {
                    client.UploadStringCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            Util.WriteToChat("Upload Error: " + e.Error.Message);
                        }
                        else
                        {
                            Util.WriteToChat(e.Result);
                        }
                    };

                    client.UploadStringAsync(endpoint, "POST", message);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void ProcessTitlesMessage(NetworkMessageEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Logging.LogError(ex);
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
            try
            {
                Int32 title = e.Message.Value<Int32>("title");
                bool active = e.Message.Value<bool>("active");

                if (active)
                {
                    currentTitle = title;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }
    }
}

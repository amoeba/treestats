using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats2
{
    public static class TreeStats2
    {
        public static CoreManager Core { get; set; }
        public static PluginHost Host { get; set; }

        internal static void Init(CoreManager _core, PluginHost _host)
        {
            Core = _core;
            Host = _host;
        }

        internal static void GetPlayerInfo()
        {
            try
            {
                // One long string stores the entire POST request body
                string json = "{";

                // Declare the fileservice for later use
                Decal.Adapter.Wrappers.CharacterFilter cf = Core.CharacterFilter;
                Decal.Filters.FileService fs = CoreManager.Current.FileService as Decal.Filters.FileService;

               
                // Get constants for Gender and Race so we can send names instead of IDs
                Dictionary<int, string> genderTable = new Dictionary<int, string>();
                Dictionary<int, string> heritageTable = new Dictionary<int, string>();

                // Manually specify allegiance rank mappings
                Dictionary<string, string> rankNames = new Dictionary<string, string>();

                // Aluvian
                rankNames.Add("Aluvian-0-Male", "");
                rankNames.Add("Aluvian-1-Male", "Yeoman");
                rankNames.Add("Aluvian-2-Male", "Baronet");
                rankNames.Add("Aluvian-3-Male", "Baron");
                rankNames.Add("Aluvian-4-Male", "Reeve");
                rankNames.Add("Aluvian-5-Male", "Thane");
                rankNames.Add("Aluvian-6-Male", "Ealdor");
                rankNames.Add("Aluvian-7-Male", "Duke");
                rankNames.Add("Aluvian-8-Male", "Aethling");
                rankNames.Add("Aluvian-9-Male", "King");
                rankNames.Add("Aluvian-10-Male", "High King");

                rankNames.Add("Aluvian-0-Female", "");
                rankNames.Add("Aluvian-1-Female", "Yeoman");
                rankNames.Add("Aluvian-2-Female", "Baronet");
                rankNames.Add("Aluvian-3-Female", "Baroness");
                rankNames.Add("Aluvian-4-Female", "Reeve");
                rankNames.Add("Aluvian-5-Female", "Thane");
                rankNames.Add("Aluvian-6-Female", "Ealdor");
                rankNames.Add("Aluvian-7-Female", "Duchess");
                rankNames.Add("Aluvian-8-Female", "Aethling");
                rankNames.Add("Aluvian-9-Female", "Queen");
                rankNames.Add("Aluvian-10-Female", "High Queen");

                // Gharundim (without proper apostrophe)
                rankNames.Add("Gharundim-0-Male", "");
                rankNames.Add("Gharundim-1-Male", "Sayyid");
                rankNames.Add("Gharundim-2-Male", "Shayk");
                rankNames.Add("Gharundim-3-Male", "Maulan");
                rankNames.Add("Gharundim-4-Male", "Mu'allim");
                rankNames.Add("Gharundim-5-Male", "Naqib");
                rankNames.Add("Gharundim-6-Male", "Qadi");
                rankNames.Add("Gharundim-7-Male", "Mushir");
                rankNames.Add("Gharundim-8-Male", "Amir");
                rankNames.Add("Gharundim-9-Male", "Malik");
                rankNames.Add("Gharundim-10-Male", "Sultan");

                rankNames.Add("Gharundim-0-Female", "");
                rankNames.Add("Gharundim-1-Female", "Sayyida");
                rankNames.Add("Gharundim-2-Female", "Shayka");
                rankNames.Add("Gharundim-3-Female", "Mualana");
                rankNames.Add("Gharundim-4-Female", "Mu'allima");
                rankNames.Add("Gharundim-5-Female", "Naqiba");
                rankNames.Add("Gharundim-6-Female", "Qadiya");
                rankNames.Add("Gharundim-7-Female", "Mushira");
                rankNames.Add("Gharundim-8-Female", "Amira");
                rankNames.Add("Gharundim-9-Female", "Malika");
                rankNames.Add("Gharundim-10-Female", "Sultana");

                // Gharu'ndim
                rankNames.Add("Gharu'ndim-0-Male", "");
                rankNames.Add("Gharu'ndim-1-Male", "Sayyid");
                rankNames.Add("Gharu'ndim-2-Male", "Shayk");
                rankNames.Add("Gharu'ndim-3-Male", "Maulan");
                rankNames.Add("Gharu'ndim-4-Male", "Mu'allim");
                rankNames.Add("Gharu'ndim-5-Male", "Naqib");
                rankNames.Add("Gharu'ndim-6-Male", "Qadi");
                rankNames.Add("Gharu'ndim-7-Male", "Mushir");
                rankNames.Add("Gharu'ndim-8-Male", "Amir");
                rankNames.Add("Gharu'ndim-9-Male", "Malik");
                rankNames.Add("Gharu'ndim-10-Male", "Sultan");

                rankNames.Add("Gharu'ndim-0-Female", "");
                rankNames.Add("Gharu'ndim-1-Female", "Sayyida");
                rankNames.Add("Gharu'ndim-2-Female", "Shayka");
                rankNames.Add("Gharu'ndim-3-Female", "Mualana");
                rankNames.Add("Gharu'ndim-4-Female", "Mu'allima");
                rankNames.Add("Gharu'ndim-5-Female", "Naqiba");
                rankNames.Add("Gharu'ndim-6-Female", "Qadiya");
                rankNames.Add("Gharu'ndim-7-Female", "Mushira");
                rankNames.Add("Gharu'ndim-8-Female", "Amira");
                rankNames.Add("Gharu'ndim-9-Female", "Malika");
                rankNames.Add("Gharu'ndim-10-Female", "Sultana");

                // Sho
                rankNames.Add("Sho-0-Male", "");
                rankNames.Add("Sho-1-Male", "Jinin");
                rankNames.Add("Sho-2-Male", "Jo-Chueh");
                rankNames.Add("Sho-3-Male", "Nan-Chueh");
                rankNames.Add("Sho-4-Male", "Shi-Chueh");
                rankNames.Add("Sho-5-Male", "Ta-Chueh");
                rankNames.Add("Sho-6-Male", "Kun-Chueh");
                rankNames.Add("Sho-7-Male", "Kou");
                rankNames.Add("Sho-8-Male", "Taikou");
                rankNames.Add("Sho-9-Male", "Ou");
                rankNames.Add("Sho-10-Male", "Koutei");

                rankNames.Add("Sho-0-Female", "");
                rankNames.Add("Sho-1-Female", "Jinin");
                rankNames.Add("Sho-2-Female", "Jo-Chueh");
                rankNames.Add("Sho-3-Female", "Nan-Chueh");
                rankNames.Add("Sho-4-Female", "Shi-Chueh");
                rankNames.Add("Sho-5-Female", "Ta-Chueh");
                rankNames.Add("Sho-6-Female", "Kun-Chueh");
                rankNames.Add("Sho-7-Female", "Kou");
                rankNames.Add("Sho-8-Female", "Taikou");
                rankNames.Add("Sho-9-Female", "Jo-Ou");
                rankNames.Add("Sho-10-Female", "Koutei");

                // Viamontian
                rankNames.Add("Viamontian-0-Male", "");
                rankNames.Add("Viamontian-1-Male", "Squire");
                rankNames.Add("Viamontian-2-Male", "Banner");
                rankNames.Add("Viamontian-3-Male", "Baron");
                rankNames.Add("Viamontian-4-Male", "Viscount");
                rankNames.Add("Viamontian-5-Male", "Count");
                rankNames.Add("Viamontian-6-Male", "Marquis");
                rankNames.Add("Viamontian-7-Male", "Duke");
                rankNames.Add("Viamontian-8-Male", "Grand Duke");
                rankNames.Add("Viamontian-9-Male", "King");
                rankNames.Add("Viamontian-10-Male", "High King");

                rankNames.Add("Viamontian-0-Female", "");
                rankNames.Add("Viamontian-1-Female", "Dame");
                rankNames.Add("Viamontian-2-Female", "Banner");
                rankNames.Add("Viamontian-3-Female", "Baroness");
                rankNames.Add("Viamontian-4-Female", "Vicountess");
                rankNames.Add("Viamontian-5-Female", "Countess");
                rankNames.Add("Viamontian-6-Female", "Marquise");
                rankNames.Add("Viamontian-7-Female", "Duchess");
                rankNames.Add("Viamontian-8-Female", "Grand Duchess");
                rankNames.Add("Viamontian-9-Female", "Queen");
                rankNames.Add("Viamontian-10-Female", "High Queen");

                // Undead 
                rankNames.Add("Umbraen-0-Male", "");
                rankNames.Add("Umbraen-1-Male", "Tenebrous");
                rankNames.Add("Umbraen-2-Male", "Shade");
                rankNames.Add("Umbraen-3-Male", "Squire");
                rankNames.Add("Umbraen-4-Male", "Knight");
                rankNames.Add("Umbraen-5-Male", "Void Knight");
                rankNames.Add("Umbraen-6-Male", "Void Lord");
                rankNames.Add("Umbraen-7-Male", "Duke");
                rankNames.Add("Umbraen-8-Male", "Archduke");
                rankNames.Add("Umbraen-9-Male", "Highborn");
                rankNames.Add("Umbraen-10-Male", "King");

                rankNames.Add("Umbraen-0-Female", "");
                rankNames.Add("Umbraen-1-Female", "Tenebrous");
                rankNames.Add("Umbraen-2-Female", "Shade");
                rankNames.Add("Umbraen-3-Female", "Squire");
                rankNames.Add("Umbraen-4-Female", "Knight");
                rankNames.Add("Umbraen-5-Female", "Void Knight");
                rankNames.Add("Umbraen-6-Female", "Viod Lady");
                rankNames.Add("Umbraen-7-Female", "Duchess");
                rankNames.Add("Umbraen-8-Female", "Archduchess");
                rankNames.Add("Umbraen-9-Female", "Highborn");
                rankNames.Add("Umbraen-10-Female", "Queen");

                // Penumbraen
                rankNames.Add("Penumbraen-0-Male", "");
                rankNames.Add("Penumbraen-1-Male", "Tenebrous");
                rankNames.Add("Penumbraen-2-Male", "Shade");
                rankNames.Add("Penumbraen-3-Male", "Squire");
                rankNames.Add("Penumbraen-4-Male", "Knight");
                rankNames.Add("Penumbraen-5-Male", "Void Knight");
                rankNames.Add("Penumbraen-6-Male", "Void Lord");
                rankNames.Add("Penumbraen-7-Male", "Duke");
                rankNames.Add("Penumbraen-8-Male", "Archduke");
                rankNames.Add("Penumbraen-9-Male", "Highborn");
                rankNames.Add("Penumbraen-10-Male", "King");

                rankNames.Add("Penumbraen-0-Female", "");
                rankNames.Add("Penumbraen-1-Female", "Tenebrous");
                rankNames.Add("Penumbraen-2-Female", "Shade");
                rankNames.Add("Penumbraen-3-Female", "Squire");
                rankNames.Add("Penumbraen-4-Female", "Knight");
                rankNames.Add("Penumbraen-5-Female", "Void Knight");
                rankNames.Add("Penumbraen-6-Female", "Viod Lady");
                rankNames.Add("Penumbraen-7-Female", "Duchess");
                rankNames.Add("Penumbraen-8-Female", "Archduchess");
                rankNames.Add("Penumbraen-9-Female", "Highborn");
                rankNames.Add("Penumbraen-10-Female", "Queen");

                // Gear Knight
                rankNames.Add("Gear Knight-0-Male", "");
                rankNames.Add("Gear Knight-1-Male", "Tribunus");
                rankNames.Add("Gear Knight-2-Male", "Praefectus");
                rankNames.Add("Gear Knight-3-Male", "Optio");
                rankNames.Add("Gear Knight-4-Male", "Centurion");
                rankNames.Add("Gear Knight-5-Male", "Principes");
                rankNames.Add("Gear Knight-6-Male", "Legatus");
                rankNames.Add("Gear Knight-7-Male", "Consul");
                rankNames.Add("Gear Knight-8-Male", "Dux");
                rankNames.Add("Gear Knight-9-Male", "Secondus");
                rankNames.Add("Gear Knight-10-Male", "Primus");

                rankNames.Add("Gear Knight-0-Female", "");
                rankNames.Add("Gear Knight-1-Female", "Tribunus");
                rankNames.Add("Gear Knight-2-Female", "Praefectus");
                rankNames.Add("Gear Knight-3-Female", "Optio");
                rankNames.Add("Gear Knight-4-Female", "Centurion");
                rankNames.Add("Gear Knight-5-Female", "Principes");
                rankNames.Add("Gear Knight-6-Female", "Legatus");
                rankNames.Add("Gear Knight-7-Female", "Consul");
                rankNames.Add("Gear Knight-8-Female", "Dux");
                rankNames.Add("Gear Knight-9-Female", "Secondus");
                rankNames.Add("Gear Knight-10-Female", "Primus");

                // Undead
                rankNames.Add("Undead-0-Male", "");
                rankNames.Add("Undead-1-Male", "Neophyte");
                rankNames.Add("Undead-2-Male", "Acolyte");
                rankNames.Add("Undead-3-Male", "Adept");
                rankNames.Add("Undead-4-Male", "Esquire");
                rankNames.Add("Undead-5-Male", "Squire");
                rankNames.Add("Undead-6-Male", "Knight");
                rankNames.Add("Undead-7-Male", "Count");
                rankNames.Add("Undead-8-Male", "Viscount");
                rankNames.Add("Undead-9-Male", "Highness");
                rankNames.Add("Undead-10-Male", "Annointed");

                rankNames.Add("Undead-0-Female", "");
                rankNames.Add("Undead-1-Female", "Neophyte");
                rankNames.Add("Undead-2-Female", "Acolyte");
                rankNames.Add("Undead-3-Female", "Adept");
                rankNames.Add("Undead-4-Female", "Esquire");
                rankNames.Add("Undead-5-Female", "Squire");
                rankNames.Add("Undead-6-Female", "Knight");
                rankNames.Add("Undead-7-Female", "Countess");
                rankNames.Add("Undead-8-Female", "Viscountess");
                rankNames.Add("Undead-9-Female", "Highness");
                rankNames.Add("Undead-10-Female", "Annointed");

                // Empyrean
                rankNames.Add("Empyrean-0-Male", "");
                rankNames.Add("Empyrean-1-Male", "Ensign");
                rankNames.Add("Empyrean-2-Male", "Corporal");
                rankNames.Add("Empyrean-3-Male", "Lieutenant");
                rankNames.Add("Empyrean-4-Male", "Commander");
                rankNames.Add("Empyrean-5-Male", "Commodore");
                rankNames.Add("Empyrean-6-Male", "Admiral");
                rankNames.Add("Empyrean-7-Male", "Commodore");
                rankNames.Add("Empyrean-8-Male", "Warlord");
                rankNames.Add("Empyrean-9-Male", "Ipharsin");
                rankNames.Add("Empyrean-10-Male", "Aulin");

                rankNames.Add("Empyrean-0-Female", "");
                rankNames.Add("Empyrean-1-Female", "Ensign");
                rankNames.Add("Empyrean-2-Female", "Corporal");
                rankNames.Add("Empyrean-3-Female", "Lieutenant");
                rankNames.Add("Empyrean-4-Female", "Commander");
                rankNames.Add("Empyrean-5-Female", "Captain");
                rankNames.Add("Empyrean-6-Female", "Commodore");
                rankNames.Add("Empyrean-7-Female", "Admiral");
                rankNames.Add("Empyrean-8-Female", "Warlord");
                rankNames.Add("Empyrean-9-Female", "Ipharsia");
                rankNames.Add("Empyrean-10-Female", "Aulia");

                // Tumerok
                rankNames.Add("Tumerok-0-Male", "");
                rankNames.Add("Tumerok-1-Male", "Xutua");
                rankNames.Add("Tumerok-2-Male", "Tuona");
                rankNames.Add("Tumerok-3-Male", "Ona");
                rankNames.Add("Tumerok-4-Male", "Nuona");
                rankNames.Add("Tumerok-5-Male", "Turea");
                rankNames.Add("Tumerok-6-Male", "Rea");
                rankNames.Add("Tumerok-7-Male", "Nurea");
                rankNames.Add("Tumerok-8-Male", "Kauh");
                rankNames.Add("Tumerok-9-Male", "Sutah");
                rankNames.Add("Tumerok-10-Male", "Tah");

                rankNames.Add("Tumerok-0-Female", "");
                rankNames.Add("Tumerok-1-Female", "Xutua");
                rankNames.Add("Tumerok-2-Female", "Tuona");
                rankNames.Add("Tumerok-3-Female", "Ona");
                rankNames.Add("Tumerok-4-Female", "Nuona");
                rankNames.Add("Tumerok-5-Female", "Turea");
                rankNames.Add("Tumerok-6-Female", "Rea");
                rankNames.Add("Tumerok-7-Female", "Nurea");
                rankNames.Add("Tumerok-8-Female", "Kauh");
                rankNames.Add("Tumerok-9-Female", "Sutah");
                rankNames.Add("Tumerok-10-Female", "Tah");

                // Lugian
                rankNames.Add("Lugian-0-Male", "");
                rankNames.Add("Lugian-1-Male", "Laigus");
                rankNames.Add("Lugian-2-Male", "Raigus");
                rankNames.Add("Lugian-3-Male", "Amploth");
                rankNames.Add("Lugian-4-Male", "Arintoth");
                rankNames.Add("Lugian-5-Male", "Obeloth");
                rankNames.Add("Lugian-6-Male", "Lithos");
                rankNames.Add("Lugian-7-Male", "Kantos");
                rankNames.Add("Lugian-8-Male", "Gigas");
                rankNames.Add("Lugian-9-Male", "Extas");
                rankNames.Add("Lugian-10-Male", "Tiatus");

                rankNames.Add("Lugian-0-Female", "");
                rankNames.Add("Lugian-1-Female", "Laigus");
                rankNames.Add("Lugian-2-Female", "Raigus");
                rankNames.Add("Lugian-3-Female", "Amploth");
                rankNames.Add("Lugian-4-Female", "Arintoth");
                rankNames.Add("Lugian-5-Female", "Obeloth");
                rankNames.Add("Lugian-6-Female", "Lithos");
                rankNames.Add("Lugian-7-Female", "Kantos");
                rankNames.Add("Lugian-8-Female", "Gigas");
                rankNames.Add("Lugian-9-Female", "Extas");
                rankNames.Add("Lugian-10-Female", "Tiatus");

                for (int i = 1; i < fs.HeritageTable.Length; ++i)
                {
                    try
                    {
                        Decal.Filters.HeritageTable heritages = fs.HeritageTable;

                        int heritageId = heritages[i].Id;
                        string heritageName = heritages[i].Name;

                        if (heritageName != "Invalid")
                        {
                            heritageTable.Add(heritageId, heritageName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                    }
                }

                for (int i = 1; i < fs.GenderTable.Length; ++i)
                {
                    try
                    {
                        Decal.Filters.GenderTable genders = fs.GenderTable;

                        int genderId = genders[i].Id;
                        string genderName = genders[i].Name;

                        if (genderName != "Invalid")
                        {
                            genderTable.Add(genderId, genderName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(ex);
                    }

                }

                // General attributes
                json += "\"version\":\"1\",";
                json += "\"name\":\"" + cf.Name + "\",";       
                json += "\"race\":\"" + cf.Race + "\",";
                json += "\"gender\":\"" + cf.Gender + "\",";
                json += "\"class_template\":\"" + cf.ClassTemplate + "\",";
                json += "\"level\":" + cf.Level + ",";
                
                json += "\"rank\":" + cf.Rank + ",";
                json += "\"title\":\"" + rankNames[cf.Race + "-" + cf.Rank.ToString() + "-" + cf.Gender] + "\",";
                json += "\"followers\":" + cf.Followers.ToString() + ",";
                
                json += "\"server\":\"" + cf.Server + "\",";
                json += "\"server_population\":" + cf.ServerPopulation.ToString() + ",";
                json += "\"deaths\":" + cf.Deaths.ToString() + ",";
                json += "\"birth\":\"" + cf.Birth + "\",";
                json += "\"total_xp\":" + cf.TotalXP.ToString() + ",";
                json += "\"unassigned_xp\":" + cf.UnassignedXP.ToString() + ",";
                json += "\"skill_credits\":" + cf.SkillPoints.ToString() + ",";

                // Attributes

                json += "\"attribs\":{";

                foreach (var attr in Core.CharacterFilter.Attributes)
                {
                    json += "\"" + attr.Name.ToLower() + "\":{\"name\":\"" + attr.Name + "\", \"base\":" + attr.Base.ToString() + ", \"creation\":" + attr.Creation.ToString() + "},";
                   
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


                // Get skills
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
                // We wrap in try/catch because AllegianceInfoWrapper behaves badly (Is not null when it should be? Not sure on this.)
                try
                {
                    json += "\"monarch\":{\"name\":\"" + cf.Monarch.Name + "\",\"race\":\"" + heritageTable[cf.Monarch.Race] + "\",\"rank\":" + cf.Monarch.Rank + ",\"title\":\"" + rankNames[heritageTable[cf.Monarch.Race] + "-" + cf.Monarch.Rank.ToString() + "-" + genderTable[cf.Monarch.Gender]] + "\",\"gender\":\"" + genderTable[cf.Monarch.Gender] + "\",\"followers\":" + cf.MonarchFollowers + "},";
                }
                catch (Exception ex)
                {
                }

                try
                {
                    json += "\"patron\":{\"name\":\"" + cf.Patron.Name + "\",\"race\":\"" + heritageTable[cf.Patron.Race] + "\",\"rank\":" + cf.Patron.Rank + ",\"title\":\"" + rankNames[heritageTable[cf.Patron.Race] + "-" + cf.Patron.Rank.ToString() + "-" + genderTable[cf.Patron.Gender]] + "\",\"gender\":\"" + genderTable[cf.Patron.Gender] + "\",\"xp\":" + cf.Patron.XP + "},";
                }
                catch (Exception ex)
                {
                }

                // Vassals
 
                if (cf.Vassals != null && cf.Vassals.Count > 0)
                {
                    json += "\"vassals\":[";

                    
                    foreach (AllegianceInfoWrapper vassal in cf.Vassals)
                    {
                        json += "{\"name\":\"" + vassal.Name + "\",\"race\":\"" + heritageTable[vassal.Race] + "\",\"rank\":" + vassal.Rank + ", \"title\":\"" + rankNames[heritageTable[vassal.Race] + "-" + vassal.Rank.ToString() + "-" + genderTable[vassal.Gender]] + "\",\"gender\":\"" + genderTable[vassal.Gender] + "\"},";
                    }
                    
                    json = json.Remove(json.Length - 1);
                   
                    json += "],";
                }

                // Remove final trailing comma
                json = json.Remove(json.Length - 1);
                
                // Add closing bracket
                json += "}";

                // Send POST request
                SendData(Encryption.encrypt(json));
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void SendData(string message)
        {
            try
            {
                Util.WriteToChat("Sending character update.");
                
                Uri endpoint = new Uri("http://floating-meadow-8649.herokuapp.com/");
  
                using (var client = new WebClient())
                {
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

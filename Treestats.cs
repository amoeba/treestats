﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class TreeStats2
    {
        public static CoreManager Core { get; set; }
        public static PluginHost Host { get; set; }

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

        public static List<Int32> ratingValues;
        public static List<Int32> augmentationValues;
        public static List<Int32> auraValues;

        // Store character properties from GameEvent/Login Character message
        public static Dictionary<Int32, Int32> characterProperties;


        // Store constants for Gender, Race, and Rank so we can send names instead of IDs
        public static Dictionary<int, string> genderTable;
        public static Dictionary<int, string> heritageTable;
        public static Dictionary<string, string> rankNames;
        public static Dictionary<Int32, string> characterPropertyData;



        // Resources
        // http://www.immortalbob.com/phpBB3/viewtopic.php?f=24&t=100&start=10
        // http://pastebin.com/X05rYnYU
       //  http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/VTClassic%20Shared/Constants.cs

        internal enum CharProps
        {
            Species = 2,
            Burden = 5,
            Equipped_Slots = 0x0A,
            Rare_ID = 0x11,
            Value = 0x13,
            Total_Pyreals = 0x14
        }

        // Quad props
        /*
         * /vt mexec getcharquadprop[1] = total regular xp
/vt mexec getcharquadprop[2] = unassigned xp
/vt mexec getcharquadprop[6] = total lum xp
         */

        internal static void Init(CoreManager _core, PluginHost _host)
        {
            Logging.loggingState = false;

            Core = _core;
            Host = _host;
            
            currentTitle = -1;
            titlesList = new List<Int32>();
            allegianceName = "";
            luminance_earned = -1;
            luminance_total = -1;

            // Store all returned character properties from the Login Player event
            characterProperties = new Dictionary<Int32, Int32>();

            // Set up gender, hertiage, and rank translation
            genderTable = new Dictionary<int, string>();
            heritageTable = new Dictionary<int, string>();
            rankNames = new Dictionary<string, string>();

            characterPropertyData = new Dictionary<int, string>();

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

            
            // Store names of CharacterPropertyData
            characterPropertyData.Add(0xB5, "Chess Rank");
            characterPropertyData.Add(0xC0, "Fishing Skill");

            characterPropertyData.Add(0xCC, "Elemental Damage Bonus");

            characterPropertyData.Add(0xDA, "Augmentation: Reinforcement of the Lugians");
            characterPropertyData.Add(0xDB, "Augmentation: Bleeargh's Fortitude");
            characterPropertyData.Add(0xDC, "Augmentation: Oswald's Enhancement");
            characterPropertyData.Add(0xDD, "Augmentation: Siraluun's Blessing");
            characterPropertyData.Add(0xDE, "Augmentation: Enduring Calm");
            characterPropertyData.Add(0xDF, "Augmentation: Steadfast Will");
            characterPropertyData.Add(0xE0, "Augmentation: Ciandra's Essence");
            characterPropertyData.Add(0xE1, "Augmentation: Yoshi's Essence");
            characterPropertyData.Add(0xE2, "Augmentation: Jibril's Essence");
            characterPropertyData.Add(0xE3, "Augmentation: Celdiseth's Essence");
            characterPropertyData.Add(0xE4, "Augmentation: Koga's Essence");
            characterPropertyData.Add(0xE5, "Augmentation: Shadow of the Seventh Mule");
            characterPropertyData.Add(0xE6, "Augmentation: Might of the Seventh Mule");
            characterPropertyData.Add(0xE7, "Augmentation: Clutch of the Miser");
            characterPropertyData.Add(0xE8, "Augmentation: Enduring Enchantment");
            characterPropertyData.Add(0xE9, "Augmentation: Critical Protection");
            characterPropertyData.Add(0xEA, "Augmentation: Quick Learner");
            characterPropertyData.Add(0xEB, "Augmentation: Ciandra's Fortune");
            characterPropertyData.Add(0xEC, "Augmentation: Charmed Smith");
            characterPropertyData.Add(0xED, "Augmentation: Innate Renewal");
            characterPropertyData.Add(0xEE, "Augmentation: Archmage's Endurance");
            characterPropertyData.Add(0xF0, "Augmentation: Enchancement of the Blade ;Turner");
            characterPropertyData.Add(0xF1, "Augmentation: Enchancement of the Arrow ;Turner");
            characterPropertyData.Add(0xF2, "Augmentation: Enchancement of the Mace ;Turner");
            characterPropertyData.Add(0xF3, "Augmentation: Caustic Enhancement");
            characterPropertyData.Add(0xF4, "Augmentation: Fiery Enchancement");
            characterPropertyData.Add(0xF5, "Augmentation: Icy Enchancement");
            characterPropertyData.Add(0xF6, "Augmentation: Storm's Enhancement");
            characterPropertyData.Add(0x300, "Augmentation: Master of the Steel Circle");
            characterPropertyData.Add(0x302, "Augmentation: Master of the Four-Fold Path");
            characterPropertyData.Add(0x310, "Augmentation: Iron Skin of the Invincible");

            augmentationValues = new List<Int32>()
            {
                0xDA,0xDB,0xDC,0xDD,0xDE,0xDF,0xE0,0xE1,0xE2,0xE3,0xE4,0xE5,0xE6,0xE7,0xE8,
                0xE9,0xEA,0xEB,0xEC,0xED,0xEE,0xF0,0xF1,0xF2,0xF3,0xF4,0xF5,0xF6,0x300,0x302,0x310
            };

            auraValues = new List<Int32>()
            {
                0x00
            };

            ratingValues = new List<Int32>()
            {
                0x00
            };
 


            //getcharintprop: Var 243	Acid Resist
//getcharintprop: Var 244- 2"	Fire Resist
//getcharintprop: Var 245	Lightning Resist
//getcharintprop: Var 246	Pierce Resist
            // 287 is the number of ribbons turned in to society (1001 is master)

            // Ratings
            /*
             * Full list from the wiki (+My codes)
             * Damage (370)
             * Critical Damage (374)
             * Damage Resistance (371)
             * Critical Damage Resistance (375)
             * Damage over Time Resistance
             * Health Drain Resistance
             * Healing Boost
             * Aetheria Surge
             * Manage Charge
             * Mana Reduction
             * Damage Reduction
             * Healing Reduction (376)
             * Damage Resistance Reduction
             * Vitality (379)
             */ 
            characterPropertyData.Add(370, "Rating: Damage");
            characterPropertyData.Add(371, "Rating: Damage Resistance");
            characterPropertyData.Add(372, "Rating: Critical");
            characterPropertyData.Add(373, "Rating: Crit Resist");
            characterPropertyData.Add(374, "Rating: Critical Damage");
            characterPropertyData.Add(375, "Rating: Critical Damage Resistance");
            characterPropertyData.Add(376, "Rating: Heal Boost");
            characterPropertyData.Add(379, "Rating: Vitality");

            // Missing DoT Resist
            // Health Drain Resist
            // Aetheria Surge Rating
            // Mana Charge Rating
            // Mana Reduction
            // Healing Reduction


            characterPropertyData.Add(353, "Weapon Master Category");
        }


        /*  
         * Get player information
         *  
         * This method builds a JSON request string which is later encrypted
         * It would be really nice to use a propert data structure for
         * this and using Json.NET to serialize that data structure.
         * 
         * Some of the stuff here is stored beforehand from other messages
         * Most of it is taken from CharacterFilter once login is completed
         */
        internal static void GetPlayerInfo()
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

                // Populate Heritage table at login time
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

                // Populate Gender table at login time
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
                json += "\"level\":" + cf.Level + ",";

                // Add allegiance name if we've gotten it in a message
                if (allegianceName.Length > 0)
                {
                    Logging.LogMessage("Adding allegiance name to request, " + allegianceName);
                    json += "\"allegiance_name\":\"" + allegianceName + "\",";
                }

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
                    json += "\"monarch\":{\"name\":\"" + cf.Monarch.Name + "\",\"race\":\"" + heritageTable[cf.Monarch.Race] + "\",\"rank\":" + cf.Monarch.Rank + ",\"title\":\"" + rankNames[heritageTable[cf.Monarch.Race] + "-" + cf.Monarch.Rank.ToString() + "-" + genderTable[cf.Monarch.Gender]] + "\",\"gender\":\"" + genderTable[cf.Monarch.Gender] + "\",\"followers\":" + cf.MonarchFollowers + "},";
                }
                catch (Exception ex)
                {
                }

                try
                {
                    json += "\"patron\":{\"name\":\"" + cf.Patron.Name + "\",\"race\":\"" + heritageTable[cf.Patron.Race] + "\",\"rank\":" + cf.Patron.Rank + ",\"title\":\"" + rankNames[heritageTable[cf.Patron.Race] + "-" + cf.Patron.Rank.ToString() + "-" + genderTable[cf.Patron.Gender]] + "\",\"gender\":\"" + genderTable[cf.Patron.Gender] + "\"},";
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
                // This includes chess, fishing, augs, ratings, etc

                // Things I want:
                /* augs, ratings, elemental damage bonus
                 * chess rank
                 * fishing skill
                 */
 
                if(characterProperties.Count > 0)
                {
                    Logging.LogMessage("Adding characterProperties to message");

                    json += "\"properties\":{";

                    foreach (var kvp in characterProperties)
                    {
                        json += "\"" + kvp.Key + "\":" + kvp.Value.ToString() + ",";
                    }

                    json = json.Remove(json.Length - 1);

                    json += "},";
                }

                // XP Augmentations

                //foreach (var kvp in characterProperties)
                //{
                //    // Check what it is
                //    if(augmentationValues.Exists(v => v ==  kvp.Key))
                //    {
                //        Logging.LogMessage("key: " + kvp.Key + " is in aug list");

                //    }
                //}

                // Lum Augs (Auras)

                // Ratings



                // Remove final trailing comma
                json = json.Remove(json.Length - 1);
                
                // Add closing bracket
                json += "}";

                // Send POST request
                string post = Encryption.encrypt(json);
                Logging.LogMessage(post);
                
                SendData(post);
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

        internal static void ProcessAugmentationsMessage(NetworkMessageEventArgs e)
        {
            try
            {
                Logging.LogMessage("---------- AUGMENTIONS ---------- ");

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

                    characterProperties.Add(tmpKey, tmpValue); 
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
               
                Logging.LogMessage("---------- AUGMENTIONS ---------- ");
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

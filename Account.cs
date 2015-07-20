using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace TreeStats
{
    public static class Account
    {
        public static DateTime lastSend;
        public static int minimumSendInterval = 5;

        public static void Init()
        {
            lastSend = DateTime.MinValue;
        }

        public static void Destroy()
        {
            lastSend = DateTime.MinValue;
        }

        public static void Create(string name, string password)
        {
            try
            {
                if (lastSend != DateTime.MinValue) // Null check: Can't do null checks on DateTimes, so we do this
                {
                    TimeSpan diff = DateTime.Now - lastSend;

                    if (diff.Seconds < minimumSendInterval)
                    {
                        Util.WriteToChat("Failed to create TreeStats account: Please wait " + (minimumSendInterval - diff.Seconds).ToString() + "s before sending again. Thanks.");

                        return;
                    }
                }

                string message = "{\"name\":\"" + name + "\",\"password\":\"" + password + "\"}";

                Logging.LogMessage("Account::Create(): " + message);

                using (var client = new WebClient())
                {
                    client.UploadStringCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            Util.WriteToChat("Account Creation Error: " + e.Error.Message);
                        }
                        else
                        {
                            Util.WriteToChat(e.Result);

                            if (e.Result.ToString() == "Account successfully created.")
                            {
                                Logging.LogMessage("Account was created, player is now logged in");
                                
                                Settings.isLoggedIn = true;
                                Settings.accountName = name;
                                Settings.accountPassword = password;
                                Settings.Save();

                                // Force an upload now that we're logged in
                                Character.DoUpdate();
                            }
                            else
                            {
                                Settings.isLoggedIn = false;
                            }
                        }
                    };

                    lastSend = DateTime.Now;

                    client.UploadStringAsync(new Uri(PluginCore.urlBase + "account/create"), "POST", message);
                }

                
            
            }
            catch (Exception ex) 
            {
                Logging.LogError(ex);
            }
        }

        public static void Login(string name, string password)
        {
            try
            {
                if (lastSend != DateTime.MinValue) // Null check: Can't do null checks on DateTimes, so we do this
                {
                    TimeSpan diff = DateTime.Now - lastSend;

                    if (diff.Seconds < minimumSendInterval)
                    {
                        Util.WriteToChat("Failed to login: Please wait " + (minimumSendInterval - diff.Seconds).ToString() + "s before sending again. Thanks.");

                        return;
                    }
                }

                if (name.Length < 1 || password.Length < 1)
                {
                    Logging.LogMessage("Account name and/or password were length zero. Login aborted.");
                    Util.WriteToChat("Account name and password must both be filled in.");

                    return;
                }

                string message = "{\"name\":\"" + name + "\",\"password\":\"" + password + "\"}";

                Logging.LogMessage("Account::Login(): message = \"" + message + "\"");

                using (var client = new WebClient())
                {
                    client.UploadStringCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            Util.WriteToChat("Account Login Error: " + e.Error.Message);
                        }
                        else
                        {
                            if (e.Result.ToString() == "You are now logged in.") // Message body from web server
                            {
                                

                                Settings.isLoggedIn = true;
                                Settings.accountName = name;
                                Settings.accountPassword = password;
                                Settings.Save();

                                Util.WriteToChat("You are now logged into TreeStats as " + Settings.accountName + ".");

                                // Force an update now that we're logged in.
                                Character.DoUpdate();
                            }
                            else
                            {
                                Settings.isLoggedIn = false;
                                Util.WriteToChat("Login failed.");
                                Logging.LogMessage("Not logged in.");
                            }
                        }
                    };

                    lastSend = DateTime.Now;

                    client.UploadStringAsync(new Uri(PluginCore.urlBase + "account/login"), "POST", message);
                }



            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static bool ShouldLogin()
        {
            try
            {
                if (Settings.useAccount == true &&
                    Settings.accountName.Length > 0 &&
                    Settings.accountPassword.Length > 0
                    )
                {
                    return true;
                }
            }
            catch (Exception ex) 
            {
                Logging.LogError(ex);
            }

            return false;
        }
    }
}
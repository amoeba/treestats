using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

// This function is very lightly modified from Virindi

namespace TreeStats
{
    public static class Logging
    {
        public static bool loggingState { get; set; }
        public static string messagesFile { get; set; }
        public static string errorLogFile { get; set; }

        internal static void Init(string _messages, string _errors)
        {
            loggingState = true;
            messagesFile = _messages;
            errorLogFile = _errors;
        }

        internal static void Destroy()
        {
            return;
        }

        internal static void LogMessage(string message)
        {
            try
            {

                if (loggingState == false)
                {
                    return;
                }

                System.IO.StreamWriter sw = new System.IO.StreamWriter(messagesFile, true);
                sw.WriteLine("[" + DateTime.Now.ToString() + "] " + message);
                sw.Close();
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

        internal static void LogError(Exception ex)
        {
            if (loggingState == false)
            {
                return;
            }

            System.IO.StreamWriter sw = new System.IO.StreamWriter(errorLogFile, true);
            sw.WriteLine("============================================================================");
            sw.WriteLine(DateTime.Now.ToString());
            sw.WriteLine("Error: " + ex.Message);
            sw.WriteLine("Source: " + ex.Source);
            sw.WriteLine("Stack: " + ex.StackTrace);
            if (ex.InnerException != null)
            {
                sw.WriteLine("Inner: " + ex.InnerException.Message);
                sw.WriteLine("Inner Stack: " + ex.InnerException.StackTrace);
            }
            sw.WriteLine("============================================================================");
            sw.WriteLine("");
            sw.Close();
        }
    }
}

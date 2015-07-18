using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

// This function is very lightly modified from Virindi (virindi.net)

namespace TreeStats
{
    public static class Logging
    {
        public static bool loggingState { get; set; }
        public static string messagesFile { get; set; }
        public static string errorLogFile { get; set; }
        public static int fileSizeLimit = 102400; // 100 KB

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

                bool shouldAppend = true; // Default to appending log message
                
                // Check file size and decide whether to append or not
                FileInfo info = new FileInfo(messagesFile);

                if (info.Length > fileSizeLimit)
                {
                    shouldAppend = false;
                }


                System.IO.StreamWriter sw = new System.IO.StreamWriter(messagesFile, shouldAppend);

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

            bool shouldAppend = true; // Default to appending log message

            // Check file size and decide whether to append or not
            FileInfo info = new FileInfo(messagesFile);

            if (info.Length > fileSizeLimit)
            {
                shouldAppend = false;
            }


            System.IO.StreamWriter sw = new System.IO.StreamWriter(errorLogFile, shouldAppend);

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

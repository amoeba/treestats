//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Net;

//namespace TreeStats
//{
//    public static class Messaging
//    {
//        public static void SendEncrypted(string original)
//        {
//            Logging.LogMessage("SendEncrypted() Called");

//            // Take in message
//            // Verify it is a string with a closing }
//            // Encrypt it
//            // Send it

//            int curly_position = original.LastIndexOf("}");
//            Logging.LogMessage("Curly position is " + curly_position);

//            if (curly_position == original.Length - 1)
//            {
//                Logging.LogMessage("position is at length - 1");
//            }
//            else
//            {
//                Logging.LogMessage("position is not at length -1");
//            }

//            string encrypted = Encryption.encrypt(original);

//            Logging.LogMessage("Encrypted string...");
//            Logging.LogMessage(encrypted);

//            Logging.LogMessage("Sending encrypted message");

//            using (var client = new WebClient())
//            {
//                client.UploadStringCompleted += (s, e) =>
//                {
//                    if (e.Error != null)
//                    {
//                        Util.WriteToChat("Upload Error: " + e.Error.Message);
//                    }
//                    else
//                    {
//                        Util.WriteToChat(e.Result);
//                    }
//                };

//                client.UploadStringAsync(new Uri("http://treestats-staging.herokuapp.com/message"), "POST", encrypted);
//            }
//        }
//    }
//}

using System;

namespace TreeStats
{
    public static class Util
    {
        public static void WriteToChat(string message)
        {
            try
            {
                TreeStats2.Host.Actions.AddChatText("[TreeStats2] " + message, 1);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }

    }
}

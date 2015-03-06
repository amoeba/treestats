using System;

namespace TreeStats
{
    public static class Util
    {
        public static void WriteToChat(string message)
        {
            try
            {
                TreeStats.Host.Actions.AddChatText("[TreeStats] " + message, 1);
            }
            catch (Exception ex)
            {
                Logging.LogError(ex);
            }
        }
    }
}

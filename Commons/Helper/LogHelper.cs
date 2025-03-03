using System;

namespace Helper
{
    public class LogHelper
    {
        /// <summary>
        /// Method to log integration events
        /// </summary>
        /// <param name="logText"></param>
        public static void Log(string logText)
        {
#if (DEBUG)
            Console.WriteLine(logText);
#endif
        }
    }
}

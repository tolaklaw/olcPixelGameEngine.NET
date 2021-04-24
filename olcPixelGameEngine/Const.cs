using System;
using System.Collections.Generic;
using System.Text;

namespace olc
{
    public class Const
    {
        public const byte nDefaultAlpha = 0xFF;
        public const uint nDefaultPixel = 0xFF0000; //nDefaultAlpha << 24 ;
        public const byte nMouseButton = 5;
        public enum rcode { FAIL = 0, OK = 1, NO_FILE = -1 };
        public static Renderer renderer;
        public static Platform platform;
        public static Dictionary<int, byte> mapKeys;
        public const byte nMouseButtons = 5;

        private static Dictionary<string, int> logcounter = new Dictionary<string, int>();
        public static void Log(string s)
        {
            if (!logcounter.ContainsKey(s)) logcounter.Add(s, 0);
            logcounter[s]++;
        }

        public static string outputlog()
        {
            string s = "";
            foreach (var counter in logcounter)
            {
                s += $"[{counter.Key}={counter.Value}]";
            }
            return s;
        }
    }
}

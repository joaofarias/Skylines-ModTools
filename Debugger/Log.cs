using UnityEngine;

namespace ModTools
{
    public static class Log
    {
        public static void Message(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Log, false);
            }
        }

        public static void Error(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Error, false);
            }
        }

        public static void Warning(string s)
        {
            if (ModTools.Instance.console != null)
            {
                ModTools.Instance.console.AddMessage(s, LogType.Warning, false);
            }
        }
    }
}

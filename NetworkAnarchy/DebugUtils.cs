using ColossalFramework;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    public class DebugUtils
    {
        public static readonly string modPrefix = "[Network Anarchy " + ModInfo.Version + "] ";

        public static SavedBool hideDebugMessages = new SavedBool("hideDebugMessages", NetworkAnarchy.settingsFileName, true, true);

        public static void Log(string message)
        {
            if (hideDebugMessages.value)
            {
                return;
            }

            if (message == m_lastLog)
            {
                m_duplicates++;
            }
            else if (m_duplicates > 0)
            {
                Debug.Log(modPrefix + m_lastLog + "(x" + (m_duplicates + 1) + ")");
                Debug.Log(modPrefix + message);
                m_duplicates = 0;
            }
            else
            {
                Debug.Log(modPrefix + message);
            }
            m_lastLog = message;
        }

        public static void Warning(string message)
        {
            if (message != m_lastWarning)
            {
                Debug.LogWarning(modPrefix + "Warning: " + message);
            }
            m_lastWarning = message;
        }

        public static void LogException(Exception e)
        {
            Debug.LogError(modPrefix + "Intercepted exception (not game breaking):");
            Debug.LogException(e);
        }

        private static string m_lastWarning;
        private static string m_lastLog;
        private static int m_duplicates = 0;
    }
}

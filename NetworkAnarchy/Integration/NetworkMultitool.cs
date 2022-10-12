using ColossalFramework;
using QCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NetworkAnarchy.Mods
{
    internal class NetworkMultitool
    {
        private static bool IsEnabled;
        private static Assembly ModAssembly;
        private static Type toolClass ;

        internal static void Initialise()
        {
            ModAssembly = QCommon.GetAssembly("mod", "zoningadjuster");
            if (ModAssembly == null)
            {
                NetworkAnarchy.Log.Info($"NetworkAnarchy: Network Multitool not found", "[NA04]");
                IsEnabled = false;
                return;
            }

            toolClass = ModAssembly.GetType("NetworkMultitool.NetworkMultitoolTool");
            if (toolClass == null)
            {
                NetworkAnarchy.Log.Info($"NetworkAnarchy: Network Multitool failed loading", "[NA05]");
                IsEnabled = false;
                return;
            }

            NetworkAnarchy.Log.Info($"NetworkAnarchy: Network Multitool loaded", "[NA06]");
            IsEnabled = true;
        }

        internal static bool IsToolActive()
        {
            if (!IsEnabled) return false;

            return Singleton<ToolController>.instance.CurrentTool.GetType() == toolClass;
        }
    }
}

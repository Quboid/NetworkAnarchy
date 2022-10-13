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
    internal class ZoningAdjuster
    {
        private static bool IsEnabled;
        private static Assembly ModAssembly;
        private static Type toolClass;

        internal static void Initialise()
        {
            ModAssembly = QCommon.GetAssembly("mod", "zoningadjuster");
            if (ModAssembly == null)
            {
                ModInfo.Log.Info($"NetworkAnarchy: Zoning Adjuster not found", "[NA01]");
                IsEnabled = false;
                return;
            }

            toolClass = ModAssembly.GetType("ZoningAdjuster.ZoningTool");

            if (toolClass == null)
            {
                ModInfo.Log.Info($"NetworkAnarchy: Zoning Adjuster failed loading", "[NA02]");
                IsEnabled = false;
                return;
            }

            ModInfo.Log.Info($"NetworkAnarchy: Zoning Adjuster loaded", "[NA03]");
            IsEnabled = true;
        }

        internal static bool IsToolActive()
        {
            if (!IsEnabled) return false;

            return Singleton<ToolController>.instance.CurrentTool.GetType() == toolClass;
        }
    }
}

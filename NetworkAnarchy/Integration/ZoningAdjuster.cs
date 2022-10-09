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
                DebugUtils.Log($"NetworkAnarchy: Zoning Adjuster not found [NA01]");
                IsEnabled = false;
                return;
            }

            toolClass = ModAssembly.GetType("ZoningAdjuster.ZoningTool");

            if (toolClass == null)
            {
                DebugUtils.Log($"NetworkAnarchy: Zoning Adjuster failed loading [NA02]");
                IsEnabled = false;
                return;
            }

            DebugUtils.Log($"NetworkAnarchy: Zoning Adjuster loaded [NA03]");
            IsEnabled = true;
        }

        internal static bool IsToolActive()
        {
            if (!IsEnabled) return false;

            return Singleton<ToolController>.instance.CurrentTool.GetType() == toolClass;
        }
    }
}

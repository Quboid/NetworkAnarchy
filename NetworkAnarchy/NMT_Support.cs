using ColossalFramework;
using QCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NetworkAnarchy
{
    internal class NMT_Support
    {
        private static bool IsEnabled;
        private static Assembly NMTAssembly;
        private static Type tNMTClass ;

        internal static void Initialise()
        {
            NMTAssembly = QCommon.GetAssembly("mod", "networkmultitool");
            if (NMTAssembly == null)
            {
                IsEnabled = false;
                return;
            }

            tNMTClass = NMTAssembly.GetType("NetworkMultitool.NetworkMultitoolTool");
            Debug.Log($"class:{tNMTClass}");
            if (tNMTClass == null)
            {
                IsEnabled = false;
                return;
            }

            IsEnabled = true;
        }

        internal static bool IsNMTToolActive()
        {
            if (!IsEnabled) return false;

            return Singleton<ToolController>.instance.CurrentTool.GetType() == tNMTClass;
        }
    }
}

using HarmonyLib;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    [HarmonyPatch(typeof(BuildingTool), "CheckSpace")]
    public class BT_CheckSpace
    {
        public static bool Prefix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = ToolBase.ToolErrors.None;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(BuildingTool), "CheckCollidingBuildings")]
    public class BT_CheckCollidingBuildings
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
using ColossalFramework.Math;
using HarmonyLib;

namespace NetworkAnarchy.Patches
{
    // OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, ItemClass.Layer layers, ushort ignoreBuilding, ushort ignoreNode1, ushort ignoreNode2, ulong[] buildingMask)
    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("OverlapQuad")]
    [HarmonyPatch(new[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.CollisionType), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) })]
    public class BM_OverlapQuad
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
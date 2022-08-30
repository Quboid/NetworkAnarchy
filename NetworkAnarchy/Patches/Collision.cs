using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;

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
            if (!NetworkAnarchy.Collision)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("OverlapQuad")]
    [HarmonyPatch(new[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.CollisionType), typeof(ItemClass.Layer), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) })]
    public class NM_OverlapQuad
    {
        public static bool Prefix(ref bool __result)
        {
            if (!NetworkAnarchy.Collision)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetNode))]
    [HarmonyPatch("TestNodeBuilding")]
    public class NN_TestNodeBuilding
    {
        public static bool Prefix(ref bool __result)
        {
            if (!NetworkAnarchy.Collision)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    // void CalculateImplementation1(ushort blockID, ushort segmentID, ref NetSegment data, ref ulong valid, float minX, float minZ, float maxX, float maxZ)
    [HarmonyPatch(typeof(ZoneBlock))]
    [HarmonyPatch("CalculateImplementation1")]
    [HarmonyPatch(
        new[] { typeof(ushort), typeof(ushort), typeof(NetSegment), typeof(ulong), typeof(float), typeof(float), typeof(float), typeof(float) },
        new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
    public class ZB_CalculateImplementation1
    {
        public static bool Prefix(ZoneBlock __instance, ushort blockID, ushort segmentID, ref NetSegment data, ref ulong valid, float minX, float minZ, float maxX, float maxZ)
        {
            if (data.Info.m_flattenTerrain)
            {
                return true;
            }

            return false;
        }
    }
}

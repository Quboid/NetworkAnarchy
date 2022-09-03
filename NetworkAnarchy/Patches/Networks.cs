using HarmonyLib;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    // For straight slope
    [HarmonyPatch(typeof(NetAI), "LinearMiddleHeight")]
    class NAI_LinearMiddleHeight
    {
        public static bool Prefix(ref bool __result, NetAI __instance)
        {
            if (NetworkAnarchy.instance.StraightSlope)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetAI), "BuildOnWater")]
    class NAI_BuildOnWater
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetAI), "GetMinSegmentLength")]
    class NAI_GetMinSegmentLength
    {
        public static void Postfix(ref float __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result /= 42;
            }
        }
    }

    [HarmonyPatch(typeof(NetInfo), "GetMinNodeDistance")]
    class NI_GetMinNodeDistance
    {
        public static void Postfix(ref float __result)
        {
            if (!NetworkAnarchy.NodeSnapping)
            {
                __result = 3f;
            }
        }
    }

    [HarmonyPatch(typeof(NetTool), "CheckStartAndEnd")]
    class NT_CheckStartAndEnd
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "CanAddSegment")]
    class NT_CanAddSegment
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    //bool CanAddNode(ushort segmentID, Vector3 position, Vector3 direction, bool checkDirection, ulong[] collidingSegmentBuffer)
    [HarmonyPatch(typeof(NetTool), "CanAddNode")]
    [HarmonyPatch(new[] { typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(ulong[]) })]
    class NT_CanAddNode
    {
        public static bool Prefix(ref bool __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NetTool), "CheckCollidingSegments")]
    class NT_CheckCollidingSegments
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

    // ToolBase.ToolErrors CanCreateSegment(NetInfo segmentInfo, ushort startNode, ushort startSegment, ushort endNode, ushort endSegment,
    // ushort upgrading, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, ulong[] collidingSegmentBuffer, bool testEnds)
    [HarmonyPatch(typeof(NetTool), "CanCreateSegment")]
    [HarmonyPatch(new[] { typeof(NetInfo), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(ulong[]), typeof(bool) })]
    class NT_CanCreateSegment
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

    [HarmonyPatch(typeof(NetTool), "TestNodeBuilding")]
    class NT_TestNodeBuilding
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

    [HarmonyPatch(typeof(NetTool), "CheckNodeHeights")]
    class NT_CheckNodeHeights
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            if (NetworkAnarchy.Anarchy)
            {
                __result &= ~ToolBase.ToolErrors.SlopeTooSteep;
            }
        }
    }
}
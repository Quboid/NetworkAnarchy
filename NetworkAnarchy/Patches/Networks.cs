using HarmonyLib;

namespace NetworkAnarchy.Patches
{
    // For straight slope
    [HarmonyPatch(typeof(NetAI), "LinearMiddleHeight")]
    class NAI_LinearMiddleHeight
    {
        public static bool Prefix(ref bool __result, NetAI __instance)
        {
            if (NetworkAnarchy.instance.StraightSlope && !(__instance is WaterPipeAI || __instance is FlightPathAI))
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
}
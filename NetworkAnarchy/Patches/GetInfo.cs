using HarmonyLib;
using QCommonLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    // NetInfo GetInfo(float minElevation, float maxElevation, float length, bool incoming, bool outgoing, bool curved, bool enableDouble, ref ToolBase.ToolErrors errors)
    [HarmonyPatch(typeof(RoadAI), "GetInfo")]
    class RAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            NetInfo info = __result;
            __result = GetInfoUtils.GetResult(__result, ref errors);
            ModInfo.s_debugPanel?.Text($"AAA {info.name} -> {__result.name}");
        }
    }

    [HarmonyPatch(typeof(PedestrianPathAI), "GetInfo")]
    class PPAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            __result = GetInfoUtils.GetResult(__result, ref errors);
        }
    }

    [HarmonyPatch(typeof(PedestrianWayAI), "GetInfo")]
    class PWAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            __result = GetInfoUtils.GetResult(__result, ref errors);
        }
    }

    [HarmonyPatch(typeof(TrainTrackAI), "GetInfo")]
    class TTAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            __result = GetInfoUtils.GetResult(__result, ref errors);
        }
    }

    [HarmonyPatch(typeof(MetroTrackAI), "GetInfo")]
    class MTAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            __result = GetInfoUtils.GetResult(__result, ref errors);
        }
    }

    internal static class GetInfoUtils
    {
        internal static NetInfo GetResult(NetInfo result, ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.instance == null || !NetworkAnarchy.instance.IsActive) { return result; }
            if (NetworkAnarchy.instance.IsBuildingIntersection()) { return result; }
            NetTool netTool = NetworkAnarchy.ToolNet;
            NetAI ai = netTool.m_prefab.m_netAI;

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            return Get(ai, result);
        }

        internal static NetInfo Get(NetAI ai, NetInfo normal)
        {
            NetAIWrapper wrapper = new NetAIWrapper(ai);

            switch (NetworkAnarchy.instance.mode)
            {
                case Modes.Ground:
                    return ai.m_info;

                case Modes.Elevated:
                    return wrapper.Elevated ?? wrapper.Info;

                case Modes.Bridge:
                    return wrapper.Bridge ?? wrapper.Info;

                case Modes.Tunnel:
                    return wrapper.Tunnel ?? wrapper.Info;
            }
            return normal;
        }
    }
}

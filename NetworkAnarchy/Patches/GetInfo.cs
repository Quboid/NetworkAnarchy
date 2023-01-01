using HarmonyLib;
using QCommonLib;
using System.Collections.Generic;
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
            NetTool netTool = NetworkAnarchy.ToolNet;
            RoadAI ai = netTool.m_prefab.m_netAI as RoadAI;
            Log.Debug($"RAI_GetInfo1 \"{__result.name}\" ({netTool.m_prefab.name}) - {GetInfo.Get(ai, __result)}");

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            __result = GetInfo.Get(ai, __result);
        }
    }

    [HarmonyPatch(typeof(PedestrianPathAI), "GetInfo")]
    class PPAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            NetTool netTool = NetworkAnarchy.ToolNet;
            PedestrianPathAI ai = netTool.m_prefab.m_netAI as PedestrianPathAI;

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            __result = GetInfo.Get(ai, __result);
        }
    }

    [HarmonyPatch(typeof(PedestrianWayAI), "GetInfo")]
    class PWAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            NetTool netTool = NetworkAnarchy.ToolNet;
            PedestrianWayAI ai = netTool.m_prefab.m_netAI as PedestrianWayAI;

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            __result = GetInfo.Get(ai, __result);
        }
    }

    [HarmonyPatch(typeof(TrainTrackAI), "GetInfo")]
    class TTAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            NetTool netTool = NetworkAnarchy.ToolNet;
            TrainTrackAI ai = netTool.m_prefab.m_netAI as TrainTrackAI;

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            __result = GetInfo.Get(ai, __result);
        }
    }

    [HarmonyPatch(typeof(MetroTrackAI), "GetInfo")]
    class MTAI_GetInfo
    {
        public static void Postfix(ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            NetTool netTool = NetworkAnarchy.ToolNet;
            MetroTrackAI ai = netTool.m_prefab.m_netAI as MetroTrackAI;

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            __result = GetInfo.Get(ai, __result);
        }
    }

    internal static class GetInfo
    {
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

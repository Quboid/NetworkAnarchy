using HarmonyLib;

namespace NetworkAnarchy.Patches
{
    // NetInfo GetInfo(float minElevation, float maxElevation, float length, bool incoming, bool outgoing, bool curved, bool enableDouble, ref ToolBase.ToolErrors errors)
    [HarmonyPatch(typeof(RoadAI), "GetInfo")]
    class RAI_GetInfo
    {
        public static bool Prefix(ref RoadAI __instance, ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            return !GetInfoUtils.GetEarlyResult(ref __result, __instance.m_info, ref errors);
        }

        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            GetInfoUtils.GetLateResult(ref errors);
        }
    }

    [HarmonyPatch(typeof(PedestrianPathAI), "GetInfo")]
    class PPAI_GetInfo
    {
        public static bool Prefix(ref PedestrianPathAI __instance, ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            return !GetInfoUtils.GetEarlyResult(ref __result, __instance.m_info, ref errors);
        }

        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            GetInfoUtils.GetLateResult(ref errors);
        }
    }

    [HarmonyPatch(typeof(PedestrianWayAI), "GetInfo")]
    class PWAI_GetInfo
    {
        public static bool Prefix(ref PedestrianWayAI __instance, ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            return !GetInfoUtils.GetEarlyResult(ref __result, __instance.m_info, ref errors);
        }

        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            GetInfoUtils.GetLateResult(ref errors);
        }
    }

    [HarmonyPatch(typeof(TrainTrackAI), "GetInfo")]
    class TTAI_GetInfo
    {
        public static bool Prefix(ref TrainTrackAI __instance, ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            return !GetInfoUtils.GetEarlyResult(ref __result, __instance.m_info, ref errors);
        }

        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            GetInfoUtils.GetLateResult(ref errors);
        }
    }

    [HarmonyPatch(typeof(MetroTrackAI), "GetInfo")]
    class MTAI_GetInfo
    {
        public static bool Prefix(ref MetroTrackAI __instance, ref NetInfo __result, ref ToolBase.ToolErrors errors)
        {
            return !GetInfoUtils.GetEarlyResult(ref __result, __instance.m_info, ref errors);
        }

        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            GetInfoUtils.GetLateResult(ref errors);
        }
    }



    internal static class GetInfoUtils
    {
        internal static bool GetEarlyResult(ref NetInfo output, NetInfo input, ref ToolBase.ToolErrors errors)
        {
            output = input;
            if (NetworkAnarchy.instance == null || !NetworkAnarchy.instance.IsActive) { return false; }
            if (NetworkAnarchy.instance.IsBuildingGroundIntersection()) { return true; }
            NetAIWrapper wrapper = new NetAIWrapper(input.m_netAI);

            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }

            switch (NetworkAnarchy.instance.mode)
            {
                case Modes.Normal:
                    return false;

                case Modes.Ground:
                    output = wrapper.Info;
                    return true;

                case Modes.Elevated:
                    output = wrapper.Elevated ?? wrapper.Info;
                    return true;

                case Modes.Bridge:
                    output = wrapper.Bridge ?? wrapper.Info;
                    return true;

                case Modes.Tunnel:
                    output = wrapper.Tunnel ?? wrapper.Info;
                    return true;
            }

            return false;
        }

        internal static void GetLateResult(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
}

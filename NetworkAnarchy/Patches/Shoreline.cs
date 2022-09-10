using ColossalFramework.PlatformServices;
using HarmonyLib;

namespace NetworkAnarchy.Patches
{
    class Utils
    {
        internal static ToolBase.ToolErrors YeetLimits(ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy)
            {
                errors &= ~ToolBase.ToolErrors.CanalTooClose;
                errors &= ~ToolBase.ToolErrors.CannotBuildOnWater;
                errors &= ~ToolBase.ToolErrors.HeightTooHigh;
                errors &= ~ToolBase.ToolErrors.ShoreNotFound;
                errors &= ~ToolBase.ToolErrors.SlopeTooSteep;
                errors &= ~ToolBase.ToolErrors.WaterNotFound;
            }
            return errors;
        }
    }

    [HarmonyPatch(typeof(QuayAI))]
    [HarmonyPatch("GetInfo")]
    class QAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            errors = Utils.YeetLimits(errors);
        }
    }

    [HarmonyPatch(typeof(ShipPathAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class SPAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }

    [HarmonyPatch(typeof(QuayAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class QAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }


    [HarmonyPatch(typeof(FloodWallAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class FWAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }

    [HarmonyPatch(typeof(FloodWallAI))]
    [HarmonyPatch("GetInfo")]
    class FWAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            errors = Utils.YeetLimits(errors);
        }
    }


    [HarmonyPatch(typeof(HarborAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class HAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }


    [HarmonyPatch(typeof(CargoHarborAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class CHAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }

    [HarmonyPatch(typeof(FishingHarborAI))]
    [HarmonyPatch("CheckBuildPosition")]
    class FHAI_CheckBuildPosition
    {
        public static void Postfix(ref ToolBase.ToolErrors __result)
        {
            __result = Utils.YeetLimits(__result);
        }
    }

    [HarmonyPatch(typeof(CanalAI))]
    [HarmonyPatch("GetInfo")]
    class CAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            errors = Utils.YeetLimits(errors);
        }
    }
}

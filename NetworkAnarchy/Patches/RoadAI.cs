using ColossalFramework;
using HarmonyLib;
using QCommonLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkAnarchy.Patches
{
    #region RoadAI
    // void GetElevationLimits(out int min, out int max)
    [HarmonyPatch(typeof(RoadAI), "GetElevationLimits")]
    class RAI_GetElevationLimits
    {
        public static bool Prefix(out int min, out int max)
        {
            min = -3;
            max = 0;
            if (NetworkAnarchy.Anarchy)
            {
                min = int.MinValue / 256;
                max = int.MaxValue / 256;

                return false;
            }

            return true;
        }

        public static void Postfix(ref int min)
        {
            // Override minimum height for tunnels in map editor
            if (QCommon.Scene == QCommon.SceneTypes.MapEditor)
            {
                if (NetworkAnarchy.Anarchy)
                {
                    min = int.MinValue / 256;
                }
                else
                {
                    min = 0;
                }
            }
        }
    }

    // ToolBase.ToolErrors CheckBuildPosition(bool test, bool visualize, bool overlay, bool autofix, ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint, out BuildingInfo ownerBuilding, out Vector3 ownerPosition, out Vector3 ownerDirection, out int productionRate)
    [HarmonyPatch(typeof(RoadAI), "CheckBuildPosition")]
    class RAI_CheckBuildPosition
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

    // NetInfo GetInfo(float minElevation, float maxElevation, float length, bool incoming, bool outgoing, bool curved, bool enableDouble, ref ToolBase.ToolErrors errors)
    [HarmonyPatch(typeof(RoadAI), "GetInfo")]
    class RAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
    #endregion

    #region PedestrianPathAI
    [HarmonyPatch(typeof(PedestrianPathAI), "GetElevationLimits")]
    class PPAI_GetElevationLimits
    {
        public static bool Prefix(out int min, out int max)
        {
            min = -3;
            max = 0;
            if (NetworkAnarchy.Anarchy)
            {
                min = int.MinValue / 256;
                max = int.MaxValue / 256;

                return false;
            }

            return true;
        }

        public static void Postfix(ref int min)
        {
            // Override minimum height for tunnels in map editor
            if (QCommon.Scene == QCommon.SceneTypes.MapEditor)
            {
                if (NetworkAnarchy.Anarchy)
                {
                    min = int.MinValue / 256;
                }
                else
                {
                    min = 0;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PedestrianPathAI), "CheckBuildPosition")]
    class PPAI_CheckBuildPosition
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

    [HarmonyPatch(typeof(PedestrianPathAI), "GetInfo")]
    class PPAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
    #endregion

    #region PedestrianWayAI
    [HarmonyPatch(typeof(PedestrianWayAI), "GetElevationLimits")]
    class PWAI_GetElevationLimits
    {
        public static bool Prefix(out int min, out int max)
        {
            min = -3;
            max = 0;
            if (NetworkAnarchy.Anarchy)
            {
                min = int.MinValue / 256;
                max = int.MaxValue / 256;

                return false;
            }

            return true;
        }

        public static void Postfix(ref int min)
        {
            // Override minimum height for tunnels in map editor
            if (QCommon.Scene == QCommon.SceneTypes.MapEditor)
            {
                if (NetworkAnarchy.Anarchy)
                {
                    min = int.MinValue / 256;
                }
                else
                {
                    min = 0;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PedestrianWayAI), "CheckBuildPosition")]
    class PWAI_CheckBuildPosition
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

    [HarmonyPatch(typeof(PedestrianWayAI), "GetInfo")]
    class PWAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
    #endregion

    #region TrainTrackAI
    [HarmonyPatch(typeof(TrainTrackAI), "GetElevationLimits")]
    class TTAI_GetElevationLimits
    {
        public static bool Prefix(out int min, out int max)
        {
            min = -3;
            max = 0;
            if (NetworkAnarchy.Anarchy)
            {
                min = int.MinValue / 256;
                max = int.MaxValue / 256;

                return false;
            }

            return true;
        }

        public static void Postfix(ref int min)
        {
            // Override minimum height for tunnels in map editor
            if (QCommon.Scene == QCommon.SceneTypes.MapEditor)
            {
                if (NetworkAnarchy.Anarchy)
                {
                    min = int.MinValue / 256;
                }
                else
                {
                    min = 0;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TrainTrackAI), "CheckBuildPosition")]
    class TTAI_CheckBuildPosition
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

    [HarmonyPatch(typeof(TrainTrackAI), "GetInfo")]
    class TTAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
    #endregion

    #region DecorationWallAI
    [HarmonyPatch(typeof(DecorationWallAI), "CheckBuildPosition")]
    class DWAI_CheckBuildPosition
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

    [HarmonyPatch(typeof(DecorationWallAI), "GetInfo")]
    class DWAI_GetInfo
    {
        public static void Postfix(ref ToolBase.ToolErrors errors)
        {
            if (NetworkAnarchy.Anarchy && (errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
            {
                errors ^= ToolBase.ToolErrors.HeightTooHigh;
            }
        }
    }
    #endregion
}

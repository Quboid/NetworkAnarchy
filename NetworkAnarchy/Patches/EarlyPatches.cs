using HarmonyLib;
using QCommonLib;
using System;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    public static class EarlyPatches
    {
        public static void Deploy()
        {
            Type self = typeof(EarlyPatches);
            try
            {
                Patcher.Instance.Patch(typeof(NetInfo).GetMethod("InitializePrefab"), postfix: new HarmonyMethod(self.GetMethod("NetInfo_InitializePrefab_Postfix")));
                Patcher.Instance.Patch(typeof(TransportInfo).GetMethod("InitializePrefab"), postfix: new HarmonyMethod(self.GetMethod("TransportInfo_InitializePrefab_Postfix")));
                Patcher.Instance.Patch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix")));
                Debug.Log($"NetworkAnarchy: Deployed early patches [NA13]");
            }
            catch (Exception e)
            {
                Debug.Log($"NetworkAnarchy: Failed to deploy early patches [NA14]\n{e}");
            }
        }

        public static void Revert()
        {
            try
            { 
                Type self = typeof(EarlyPatches);
                Patcher.Instance.Unpatch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport"), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                Patcher.Instance.Unpatch(typeof(TransportInfo).GetMethod("InitializePrefab"), self.GetMethod("TransportInfo_InitializePrefab_Postfix"));
                Patcher.Instance.Unpatch(typeof(NetInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Debug.Log($"NetworkAnarchy: Reverted early patches [NA15]");
            }
            catch (Exception e)
            {
                Debug.Log($"NetworkAnarchy: Failed to revert early patches [NA16]\n{e}");
            }
        }

        /// <summary>
        /// Make ship and airplane paths available in-game. Unneeded?
        /// </summary>
        public static void NetInfo_InitializePrefab_Postfix(ref NetInfo __instance)
        {
            if (QCommon.Scene != QCommon.SceneTypes.Game) return;

            if (__instance.name == "Ship Path" || __instance.name == "Airplane Path")
            {
                __instance.m_availableIn |= ItemClass.Availability.Game;
            }
        }

        /// <summary>
        /// Make Airplane paths visible
        /// </summary>
        public static void TransportInfo_InitializePrefab_Postfix(ref TransportInfo __instance)
        {
            if (QCommon.Scene != QCommon.SceneTypes.Game) return;

            if (__instance.name == "Airplane")
            {
                __instance.m_pathVisibility |= ItemClass.Availability.Game;
            }
        }

        /// <summary>
        /// Enable the ship and airplane path buttons in-game
        /// </summary>
        public static bool PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix(ref PublicTransportPanel __instance, ref bool __result, NetInfo info)
        {
            if (__instance.category == "PublicTransportShip")
            {
                __result = (info.m_vehicleTypes & (VehicleInfo.VehicleType.Ferry | VehicleInfo.VehicleType.Ship)) != VehicleInfo.VehicleType.None;
                return false;
            }
            if (__instance.category == "PublicTransportPlane")
            {
                __result = (info.m_vehicleTypes & (VehicleInfo.VehicleType.Helicopter | VehicleInfo.VehicleType.Blimp | VehicleInfo.VehicleType.Plane)) != VehicleInfo.VehicleType.None;
                return false;
            }
            return true;
        }
    }
}

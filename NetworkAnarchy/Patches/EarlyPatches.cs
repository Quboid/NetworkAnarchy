using HarmonyLib;
using QCommonLib;
using System;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    public static class EarlyPatches
    {
        public static void Deploy(QPatcher Patcher)
        {
            Type self = typeof(EarlyPatches);
            try
            {
                Patcher.Postfix(typeof(NetInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Patcher.Postfix(typeof(TransportInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Patcher.Prefix(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                //Patcher.Instance.Patch(typeof(NetInfo).GetMethod("InitializePrefab"), postfix: new HarmonyMethod(self.GetMethod("NetInfo_InitializePrefab_Postfix")));
                //Patcher.Instance.Patch(typeof(TransportInfo).GetMethod("InitializePrefab"), postfix: new HarmonyMethod(self.GetMethod("TransportInfo_InitializePrefab_Postfix")));
                //Patcher.Instance.Patch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), prefix: new HarmonyMethod(self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix")));
                ModInfo.Log.Info($"NetworkAnarchy: Deployed early patches", "[NA13]");
            }
            catch (Exception e)
            {
                ModInfo.Log.Warning($"NetworkAnarchy: Failed to deploy early patches\n{e}", "[NA14]");
            }
        }

        public static void Revert(QPatcher Patcher)
        {
            Type self = typeof(EarlyPatches);
            try
            { 
                Patcher.Unpatch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                Patcher.Unpatch(typeof(TransportInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Patcher.Unpatch(typeof(NetInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                //Patcher.Instance.Unpatch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                //Patcher.Instance.Unpatch(typeof(TransportInfo).GetMethod("InitializePrefab"), self.GetMethod("TransportInfo_InitializePrefab_Postfix"));
                //Patcher.Instance.Unpatch(typeof(NetInfo).GetMethod("InitializePrefab"), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                ModInfo.Log.Info($"NetworkAnarchy: Reverted early patches", "[NA15]");
            }
            catch (Exception e)
            {
                ModInfo.Log.Warning($"NetworkAnarchy: Failed to revert early patches\n{e}", "[NA16]");
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

using QCommonLib;
using System;
using System.Reflection;

namespace NetworkAnarchy.Patches
{
    public static class EarlyPatches
    {
        public static void Deploy(QPatcher Patcher)
        {
            Type self = typeof(EarlyPatches);
            try
            {
                Patcher.Postfix(typeof(NetInfo).GetMethod("InitializePrefab", BindingFlags.Public | BindingFlags.Instance), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Patcher.Postfix(typeof(TransportInfo).GetMethod("InitializePrefab", BindingFlags.Public | BindingFlags.Instance), self.GetMethod("TransportInfo_InitializePrefab_Postfix"));
                Patcher.Prefix(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                Log.Info($"NetworkAnarchy: Deployed early patches", "[NA13]");
            }
            catch (Exception e)
            {
                Log.Warning($"NetworkAnarchy: Failed to deploy early patches\n{e}", "[NA14]");
            }
        }

        public static void Revert(QPatcher Patcher)
        {
            Type self = typeof(EarlyPatches);
            try
            { 
                Patcher.Unpatch(typeof(PublicTransportPanel).GetMethod("IsRoadEligibleToPublicTransport", BindingFlags.NonPublic | BindingFlags.Instance), self.GetMethod("PublicTransportPanel_IsRoadEligibleToPublicTransport_Prefix"));
                Patcher.Unpatch(typeof(TransportInfo).GetMethod("InitializePrefab", BindingFlags.Public | BindingFlags.Instance), self.GetMethod("TransportInfo_InitializePrefab_Postfix"));
                Patcher.Unpatch(typeof(NetInfo).GetMethod("InitializePrefab", BindingFlags.Public | BindingFlags.Instance), self.GetMethod("NetInfo_InitializePrefab_Postfix"));
                Log.Info($"NetworkAnarchy: Reverted early patches", "[NA15]");
            }
            catch (Exception e)
            {
                Log.Warning($"NetworkAnarchy: Failed to revert early patches\n{e}", "[NA16]");
            }
        }

        /// <summary>
        /// Allow any road to make an outside connection, and make ship and airplane paths available in-game
        /// </summary>
        public static void NetInfo_InitializePrefab_Postfix(ref NetInfo __instance)
        {
            AnyRoadOutsideConnection.Apply(__instance);

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

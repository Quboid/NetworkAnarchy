using UnityEngine;

namespace NetworkAnarchy.Patches
{
    /// <summary>
    /// Make Airplane paths visible
    /// </summary>
    public static class EarlyPatches
    {
        public static void TransportInfo_InitializePrefab_Postfix(ref TransportInfo __instance)
        {
            if (!NetworkAnarchy.InGame()) return;

            if (__instance.name == "Airplane")
            {
                __instance.m_pathVisibility |= ItemClass.Availability.Game;
                Debug.Log($"NetworkAnarchy: {__instance.name} is {__instance.m_pathVisibility}");
            }
        }
    }
}

using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace NetworkAnarchy.Patches
{
    [HarmonyPatch(typeof(NetTool))]
    [HarmonyPatch("RenderSegment")]
    class PatchRenderSegment
    {
        static void Prefix(NetInfo info, NetSegment.Flags flags, ref Vector3 startPosition, ref Vector3 endPosition, ref Vector3 startDirection, ref Vector3 endDirection)
        {
            if (!isLHT())
            {
                return;
            }

            // Tunnel entrances shouldn't be inverted
            if (info.m_netAI is RoadTunnelAI || info.m_netAI is TrainTrackTunnelAI || info.m_netAI is PedestrianTunnelAI)
            {
                return;
            }

            // Don't flip Sully's Water Park station networks
            if (info.m_netAI is TrainTrackBridgeAI && info.name.Contains("Water Slide FT - ST"))
            {
                return;
            }

            Vector3 buffer;

            buffer = endPosition;
            endPosition = startPosition;
            startPosition = buffer;

            buffer = endDirection;
            endDirection = -startDirection;
            startDirection = -buffer;
        }

        static bool isLHT()
        {
            return Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
        }
    }
}

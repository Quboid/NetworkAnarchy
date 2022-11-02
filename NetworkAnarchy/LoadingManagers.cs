using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NetworkAnarchy
{
    // Adapted from AnyRoadOutsideConnectionsRevisited by Mbyron26
    internal class AnyRoadOutsideConnection
    {
        private static BuildingInfo connectionTemplate;
        private static readonly Queue<RoadBaseAI> InfoQueue = new Queue<RoadBaseAI>();
        private static int count = 0;

        /// <summary>
        /// Clear the queue/template, needed for second-loads
        /// </summary>
        public static void Initialise()
        {
            connectionTemplate = null;
            InfoQueue.Clear();
        }

        /// <summary>
        /// Finish up the road AI conversion after the level has loaded
        /// </summary>
        internal static void Finalise()
        { 
            ModInfo.Log.Info($"Enabled outside connections for {count} roads", "[NA57]");
        }

        /// <summary>
        /// Apply the conversion to roads that don't have the outsideConnection building set
        /// </summary>
        /// <param name="info">The network to check and, if needed, attached outside connection</param>
        internal static void Apply(NetInfo info)
        {
            if (info == null) 
            {
                return;
            }
            if (info.m_netAI is RoadBaseAI ai)
            {
                if (ai.m_outsideConnection == null && connectionTemplate == null)
                { // If no outsideConnection is set but the template building hasn't been found, queue for later
                    InfoQueue.Enqueue(ai);
                }
                else
                {
                    if (ai.m_outsideConnection == null)
                    {
                        ai.m_outsideConnection = connectionTemplate;
                        count++;
                    }
                    else
                    {
                        if (connectionTemplate == null)
                        {
                            connectionTemplate = ai.m_outsideConnection;
                        }
                    }
                    while (InfoQueue.Count > 0)
                    {
                        InfoQueue.Dequeue().m_outsideConnection = connectionTemplate;
                        count++;
                    }
                }
            }
        }
    }

    internal class NetworkTiling
    {
        /// <summary>
        /// Search network prefabs for any which have "NetworkTiling" in their segment or node material name
        /// Code by Ronyx69 (https://gist.github.com/ronyx69/4f06181c8082188418cd0c224f630a09)
        /// </summary>
        internal static void Apply()
        {
            int cSeg = 0, cNode = 0;

            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetLoaded(i);
                if (prefab == null) continue;

                if (prefab.m_segments != null)
                {
                    for (uint j = 0; j < prefab.m_segments.Length; j++)
                    {
                        if (prefab.m_segments[j].m_material == null) continue;

                        if (prefab.m_segments[j].m_material.name.Contains("NetworkTiling"))
                        {
                            cSeg++;
                            string[] floats = prefab.m_segments[j].m_material.name.Split(' ');
                            var tiling = Convert.ToSingle(floats[1]);
                            prefab.m_segments[j].m_material.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_segments[j].m_segmentMaterial != null) prefab.m_segments[j].m_segmentMaterial.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_segments[j].m_lodMaterial != null) prefab.m_segments[j].m_lodMaterial.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_segments[j].m_combinedLod.m_material != null) prefab.m_segments[j].m_combinedLod.m_material.mainTextureScale = new Vector2(1, Math.Abs(tiling));
                        }
                    }
                }

                if (prefab.m_nodes != null)
                {
                    for (uint j = 0; j < prefab.m_nodes.Length; j++)
                    {
                        if (prefab.m_nodes[j].m_material == null) continue;

                        if (prefab.m_nodes[j].m_material.name.Contains("NetworkTiling"))
                        {
                            cNode++;
                            string[] floats = prefab.m_nodes[j].m_material.name.Split(' ');
                            var tiling = Convert.ToSingle(floats[1]);
                            prefab.m_nodes[j].m_material.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_nodes[j].m_nodeMaterial != null) prefab.m_nodes[j].m_nodeMaterial.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_nodes[j].m_lodMaterial != null) prefab.m_nodes[j].m_lodMaterial.mainTextureScale = new Vector2(1, tiling);
                            if (prefab.m_nodes[j].m_combinedLod.m_material != null) prefab.m_nodes[j].m_combinedLod.m_material.mainTextureScale = new Vector2(1, Math.Abs(tiling));
                        }
                    }
                }
            }

            ModInfo.Log.Info($"Applying Network Tiling ({cSeg} segments, {cNode} nodes)", "[NA56]");
        }
    }

    internal class UpdateCatenaries
    {
        /// <summary>
        /// Set the number of catenaries based on the player's choice
        /// </summary>
        internal static void Apply()
        {
            int probability = NetworkAnarchy.reduceCatenary.value ? 0 : 100;

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null)
                {
                    continue;
                }

                for (int j = 0; j < info.m_lanes.Length; j++)
                {
                    if (info.m_lanes[j] != null && info.m_lanes[j].m_laneProps != null)
                    {
                        NetLaneProps.Prop[] props = info.m_lanes[j].m_laneProps.m_props;
                        if (props == null)
                        {
                            continue;
                        }

                        for (int k = 0; k < props.Length; k++)
                        {
                            if (props[k] != null && props[k].m_prop != null && props[k].m_segmentOffset == 0f && props[k].m_prop.name.ToLower().Contains("powerline"))
                            {
                                props[k].m_probability = probability;
                            }
                        }
                    }
                }
            }
        }
    }
}

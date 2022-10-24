using ColossalFramework;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        internal void ApplyNetworkTiling()
        {
            int cSeg = 0, cNode = 0;

            // Code by Ronyx69 (https://gist.github.com/ronyx69/4f06181c8082188418cd0c224f630a09)
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

            Log.Info($"Applying Network Tiling ({cSeg} segments, {cNode} nodes)", "[NA56]");
        }

        private void FixNodes()
        {
            m_stopWatch.Reset();
            m_stopWatch.Start();

            NetNode[] nodes = NetManager.instance.m_nodes.m_buffer;
            //string msg = "";

            uint max = NetManager.instance.m_nodes.m_size;
            for (int i = m_fixNodesCount; i < max; i++)
            {
                if (nodes[i].m_flags == NetNode.Flags.None || (nodes[i].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.Untouchable)
                {
                    continue;
                }

                if (m_stopWatch.ElapsedMilliseconds >= 1 && i > m_fixNodesCount + 16)
                {
                    m_fixNodesCount = i;
                    return;
                }

                NetInfo info = nodes[i].Info;
                if (info == null || info.m_netAI == null)
                {
                    continue;
                }

                var prefab = NetPrefab.GetPrefab(info);
                if ((nodes[i].m_flags & NetNode.Flags.Underground) == NetNode.Flags.Underground)
                {
                    if (prefab == null)
                    {
                        continue;
                    }

                    if ((info.m_setVehicleFlags & Vehicle.Flags.Underground) == 0 && info != prefab.netAI.Tunnel && info != prefab.netAI.Slope && !info.m_netAI.IsUnderground())
                    {
                        // Fix by Algernon
                        int tempI = i;
                        var tempPrefab = prefab;
                        Singleton<SimulationManager>.instance.AddAction(() =>
                        {
                            Log.Debug($"Fixing node {tempI} {info.m_setVehicleFlags} (underground:{(info.m_setVehicleFlags & Vehicle.Flags.Underground) != 0}, elevation:{nodes[tempI].m_elevation}, flags:{nodes[tempI].m_flags}", "[NA54]");
                            nodes[tempI].m_elevation = 0;
                            nodes[tempI].m_flags = nodes[tempI].m_flags & ~NetNode.Flags.Underground;

                            if (info != prefab.netAI.Elevated && info != tempPrefab.netAI.Bridge)
                            {
                                nodes[tempI].m_flags = nodes[tempI].m_flags | NetNode.Flags.OnGround;
                            }
                            // Updating terrain
                            try
                            {
                                TerrainModify.UpdateArea(nodes[tempI].m_bounds.min.x, nodes[tempI].m_bounds.min.z, nodes[tempI].m_bounds.max.x, nodes[tempI].m_bounds.max.z, true, true, false);
                            }
                            catch { }
                        });
                    }
                    //msg += $"\n  Node {i} is underground (info:{info}){nodes[i].m_flags}";
                }
                else if ((info != prefab.netAI.Elevated && info != prefab.netAI.Bridge) || ((nodes[i].m_flags & (NetNode.Flags.Transition | NetNode.Flags.End)) != 0 && nodes[i].m_elevation == 0))
                {
                    if ((nodes[i].m_flags & NetNode.Flags.OnGround) == 0)
                    {
                        //msg += $"\n  Node {i} is ground (info:{info}) {nodes[i].m_flags}";
                        nodes[i].m_flags = nodes[i].m_flags | NetNode.Flags.OnGround;
                    }
                }
                else if ((nodes[i].m_flags & NetNode.Flags.OnGround) != 0)
                {
                    //msg += $"\n  Node {i} is not ground (info:{info}) {nodes[i].m_flags}";
                    nodes[i].m_flags = nodes[i].m_flags & ~NetNode.Flags.OnGround;
                }
            }
            //if (msg != "")
            //{
            //    Log.Debug(msg, "[NA55]");
            //}

            m_fixNodesCount = 0;
        }

        private void FixTunnels()
        {
            m_stopWatch.Reset();
            m_stopWatch.Start();

            NetNode[] nodes = NetManager.instance.m_nodes.m_buffer;
            NetSegment[] segments = NetManager.instance.m_segments.m_buffer;

            uint max = NetManager.instance.m_segments.m_size;
            for (ushort i = m_fixTunnelsCount; i < max; i++)
            {
                if (segments[i].m_flags == NetSegment.Flags.None || (segments[i].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.Untouchable)
                {
                    continue;
                }

                if (m_stopWatch.ElapsedMilliseconds >= 1 && i > m_fixTunnelsCount + 16)
                {
                    m_fixTunnelsCount = i;
                    return;
                }

                NetInfo info = segments[i].Info;

                ushort startNode = segments[i].m_startNode;
                ushort endNode = segments[i].m_endNode;

                var prefab = NetPrefab.GetPrefab(info);
                if (prefab == null)
                {
                    continue;
                }

                // Is it a tunnel?
                if (info == prefab.netAI.Tunnel)
                {
                    nodes[startNode].m_flags = nodes[startNode].m_flags & ~NetNode.Flags.OnGround;
                    // Make sure tunnels have underground flag
                    if ((nodes[startNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                    {
                        nodes[startNode].m_flags = nodes[startNode].m_flags | NetNode.Flags.Underground;
                    }

                    if ((nodes[endNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                    {
                        nodes[endNode].m_flags = nodes[endNode].m_flags | NetNode.Flags.Underground;
                    }

                    if (prefab.netAI.Slope == null)
                    {
                        continue;
                    }

                    // Convert tunnel entrance?
                    if (IsEndTunnel(ref nodes[startNode]))
                    {
                        // Oops wrong way! Invert the segment
                        segments[i].m_startNode = endNode;
                        segments[i].m_endNode = startNode;

                        Vector3 dir = segments[i].m_startDirection;
                        segments[i].m_startDirection = segments[i].m_endDirection;
                        segments[i].m_endDirection = dir;

                        segments[i].m_flags = segments[i].m_flags ^ NetSegment.Flags.Invert;

                        segments[i].CalculateSegment(i);

                        // Make it a slope
                        segments[i].Info = prefab.netAI.Slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[startNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[startNode].m_flags = nodes[startNode].m_flags & ~NetNode.Flags.Underground;
                        }
                    }
                    else if (IsEndTunnel(ref nodes[endNode]))
                    {
                        // Make it a slope
                        segments[i].Info = prefab.netAI.Slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[endNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[endNode].m_flags = nodes[endNode].m_flags & ~NetNode.Flags.Underground;
                        }
                    }
                }
                // Is it a slope?
                else if (info == prefab.netAI.Slope)
                {
                    if (prefab.netAI.Tunnel == null)
                    {
                        continue;
                    }

                    // Convert to tunnel?
                    if (!IsEndTunnel(ref nodes[startNode]) && !IsEndTunnel(ref nodes[endNode]))
                    {
                        if ((nodes[startNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[startNode].m_flags = nodes[startNode].m_flags | NetNode.Flags.Underground;
                        }

                        if ((nodes[endNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[endNode].m_flags = nodes[endNode].m_flags | NetNode.Flags.Underground;
                        }

                        // Make it a tunnel
                        segments[i].Info = prefab.netAI.Tunnel;
                        segments[i].UpdateBounds(i);

                        // Updating terrain
                        TerrainModify.UpdateArea(segments[i].m_bounds.min.x, segments[i].m_bounds.min.z, segments[i].m_bounds.max.x, segments[i].m_bounds.max.z, true, true, false);

                        NetManager.instance.UpdateSegment(i);
                    }

                    // Is tunnel wrong way?
                    if (IsEndTunnel(ref nodes[startNode]))
                    {
                        // Oops wrong way! Invert the segment
                        segments[i].m_startNode = endNode;
                        segments[i].m_endNode = startNode;

                        Vector3 dir = segments[i].m_startDirection;

                        segments[i].m_startDirection = segments[i].m_endDirection;
                        segments[i].m_endDirection = dir;

                        segments[i].m_flags = segments[i].m_flags ^ NetSegment.Flags.Invert;

                        segments[i].CalculateSegment(i);
                    }
                }
            }

            m_fixTunnelsCount = 0;
        }

        private bool FixControlPoint(int point)
        {
            if (m_controlPoints == null)
            {
                return false;
            }

            NetInfo info = m_current;

            // Pulling from a node?
            if (m_controlPoints[point].m_node != 0)
            {
                info = NetManager.instance.m_nodes.m_buffer[m_controlPoints[point].m_node].Info;
                if (info == null)
                {
                    info = m_current;
                }
            }
            // Pulling from a segment?
            else if (m_controlPoints[point].m_segment != 0)
            {
                info = NetManager.instance.m_segments.m_buffer[m_controlPoints[point].m_segment].Info;
                if (info == null)
                {
                    info = m_current;
                }
            }
            else
            {
                return false;
            }

            float pointElevation = m_controlPoints[point].m_position.y - NetSegment.SampleTerrainHeight(info, m_controlPoints[point].m_position, false, 0f);
            float diff = pointElevation - m_controlPoints[point].m_elevation;

            // Are we off?
            if (diff <= -1f || diff >= 1f)
            {
                m_controlPoints[point].m_elevation = pointElevation;
                m_cachedControlPoints[point].m_elevation = pointElevation;
            }

            return true;
        }

        private static bool IsEndTunnel(ref NetNode node)
        {
            if ((node.m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.Untouchable && (node.m_flags & NetNode.Flags.Underground) == NetNode.Flags.Underground)
            {
                return false;
            }

            int count = 0;

            for (int i = 0; i < 8; i++)
            {
                int segment = node.GetSegment(i);
                if (segment == 0 || (NetManager.instance.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.Created)
                {
                    continue;
                }

                NetInfo info = NetManager.instance.m_segments.m_buffer[segment].Info;

                var prefab = NetPrefab.GetPrefab(info);
                if (prefab == null)
                {
                    return true;
                }

                if (info != prefab.netAI.Tunnel && info != prefab.netAI.Slope)
                {
                    return true;
                }

                count++;
            }

            if (TerrainManager.instance.SampleRawHeightSmooth(node.m_position) > node.m_position.y + 8f)
            {
                return false;
            }

            return count == 1;
        }

        public void UpdateCatenary()
        {
            int probability = reduceCatenary.value ? 0 : 100;

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

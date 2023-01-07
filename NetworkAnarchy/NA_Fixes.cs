using ColossalFramework;
using QCommonLib;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        private void FixNodes()
        {
            QTimer timer = new QTimer();

            NetNode[] nodes = NetManager.instance.m_nodes.m_buffer;
            NetNode.Flags fUnder = NetNode.Flags.Underground;
            NetNode.Flags fUntouch = NetNode.Flags.Untouchable;
            //string msg = "";

            bool singleMode = NetPrefab.SingleMode;
            NetPrefab.SingleMode = false;

            uint max = NetManager.instance.m_nodes.m_size;
            for (int i = m_fixNodesCount; i < max; i++)
            {
                if (nodes[i].m_flags == NetNode.Flags.None)
                {
                    continue;
                }

                NetInfo info = nodes[i].Info;
                if (info == null || info.m_netAI == null)
                {
                    continue;
                }

                // Fix for underground metro stations
                if ((nodes[i].m_flags & fUnder) != fUnder && (info.m_setVehicleFlags & Vehicle.Flags.Underground) == Vehicle.Flags.Underground && info.m_netAI.IsUnderground() && (nodes[i].m_flags & fUntouch) == fUntouch)
                {
                    nodes[i].m_flags |= fUnder;
                    nodes[i].m_flags &= ~NetNode.Flags.OnGround;
                    Log.Debug($"AAA Found bugged node #{i} {info.name}:{nodes[i].m_flags}");
                }

                if ((nodes[i].m_flags & fUntouch) == fUntouch)
                {
                    continue;
                }

                if (timer.MS >= 1 && i > m_fixNodesCount + 16)
                {
                    m_fixNodesCount = i;
                    NetPrefab.SingleMode = singleMode;
                    return;
                }

                var prefab = NetPrefab.Factory(info);
                if (prefab == null)
                {
                    continue;
                }

                if ((nodes[i].m_flags & fUnder) == fUnder)
                {
                    if ((info.m_setVehicleFlags & Vehicle.Flags.Underground) == 0 && info != prefab.NetAI.Tunnel && info != prefab.NetAI.Slope && !info.m_netAI.IsUnderground())
                    {
                        // Fix by Algernon
                        int tempI = i;
                        var tempPrefab = prefab;
                        Singleton<SimulationManager>.instance.AddAction(() =>
                        {
                            Log.Debug($"Fixing node {tempI} {info.m_setVehicleFlags} (underground:{(info.m_setVehicleFlags & Vehicle.Flags.Underground) != 0}, elevation:{nodes[tempI].m_elevation}, flags:{nodes[tempI].m_flags}", "[NA54]");
                            nodes[tempI].m_elevation = 0;
                            nodes[tempI].m_flags = nodes[tempI].m_flags & ~fUnder;

                            if (info != prefab.NetAI.Elevated && info != tempPrefab.NetAI.Bridge)
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
                else if ((info != prefab.NetAI.Elevated && info != prefab.NetAI.Bridge) || ((nodes[i].m_flags & (NetNode.Flags.Transition | NetNode.Flags.End)) != 0 && nodes[i].m_elevation == 0))
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

            NetPrefab.SingleMode = singleMode;
            m_fixNodesCount = 0;
        }

        private void FixTunnels()
        {
            QTimer timer = new QTimer();

            NetNode[] nodes = NetManager.instance.m_nodes.m_buffer;
            NetSegment[] segments = NetManager.instance.m_segments.m_buffer;

            bool singleMode = NetPrefab.SingleMode;
            NetPrefab.SingleMode = false;

            uint max = NetManager.instance.m_segments.m_size;
            for (ushort i = m_fixTunnelsCount; i < max; i++)
            {
                if (segments[i].m_flags == NetSegment.Flags.None || (segments[i].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.Untouchable)
                {
                    continue;
                }

                if (timer.MS >= 1 && i > m_fixTunnelsCount + 16)
                {
                    m_fixTunnelsCount = i;
                    NetPrefab.SingleMode = singleMode;
                    return;
                }

                NetInfo info = segments[i].Info;

                ushort startNode = segments[i].m_startNode;
                ushort endNode = segments[i].m_endNode;

                var prefab = NetPrefab.Factory(info);
                if (prefab == null)
                {
                    continue;
                }

                // Is it a tunnel?
                if (info == prefab.NetAI.Tunnel)
                {
                    Log.Debug($"AAA Tunnel {i}:{info.name}");
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

                    if (prefab.NetAI.Slope == null)
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
                        segments[i].Info = prefab.NetAI.Slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[startNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[startNode].m_flags = nodes[startNode].m_flags & ~NetNode.Flags.Underground;
                        }
                    }
                    else if (IsEndTunnel(ref nodes[endNode]))
                    {
                        // Make it a slope
                        segments[i].Info = prefab.NetAI.Slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[endNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[endNode].m_flags = nodes[endNode].m_flags & ~NetNode.Flags.Underground;
                        }
                    }
                }
                // Is it a slope?
                else if (info == prefab.NetAI.Slope)
                {
                    Log.Debug($"AAA Slope {i}:{info.name}");
                    if (prefab.NetAI.Tunnel == null)
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
                        segments[i].Info = prefab.NetAI.Tunnel;
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

            NetPrefab.SingleMode = singleMode;
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

                var prefab = NetPrefab.Factory(info);
                if (prefab == null)
                {
                    return true;
                }

                if (info != prefab.NetAI.Tunnel && info != prefab.NetAI.Slope)
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
    }
}

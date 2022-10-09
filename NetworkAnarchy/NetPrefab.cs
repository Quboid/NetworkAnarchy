using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy
{
    public enum Mode
    {
        Normal,
        Ground,
        Elevated,
        Bridge,
        Tunnel,
        Single
    }

    public class NetPrefab
    {
        private NetInfo m_prefab;
        private NetInfo m_elevated;
        private NetInfo m_bridge;
        private NetInfo m_slope;
        private NetInfo m_tunnel;

        private NetAIWrapper m_netAI;
        private Mode m_mode;
        private bool m_hasElevation;

        private float m_defaultMaxTurnAngle;

        public static Dictionary<NetInfo, NetPrefab> m_netPrefabs;
        private static bool m_singleMode;

        private NetPrefab(NetInfo prefab)
        {
            m_netAI = new NetAIWrapper(prefab.m_netAI);

            m_prefab = prefab;

            if (m_hasElevation = m_netAI.HasElevation)
            {
                m_elevated = m_netAI.Elevated;
                m_bridge = m_netAI.Bridge;
                m_slope = m_netAI.Slope;
                m_tunnel = m_netAI.Tunnel;
            }
        }

        public static void Initialize()
        {
            m_netPrefabs = new Dictionary<NetInfo, NetPrefab>();

            string prefabsAdded = "";

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;

                NetPrefab prefab = new NetPrefab(info);
                if (prefab.m_hasElevation && prefab.isValid() && !m_netPrefabs.ContainsKey(info))
                {
                    prefabsAdded += info.name + "\n";
                    m_netPrefabs.Add(info, prefab);

                    if (info.m_flattenTerrain &&
                        !info.m_netAI.IsUnderground() &&
                        !prefab.m_netAI.IsInvisible() &&
                        info != prefab.netAI.Elevated &&
                        info != prefab.netAI.Bridge &&
                        info != prefab.netAI.Slope &&
                        info != prefab.netAI.Tunnel)
                    {
                        info.m_followTerrain = false;
                    }

                    if (prefab.m_netAI.Elevated != null && !m_netPrefabs.ContainsKey(prefab.m_netAI.Elevated))
                        m_netPrefabs.Add(prefab.m_netAI.Elevated, prefab);
                    if (prefab.m_netAI.Bridge != null && !m_netPrefabs.ContainsKey(prefab.m_netAI.Bridge))
                        m_netPrefabs.Add(prefab.m_netAI.Bridge, prefab);
                    if (prefab.m_netAI.Slope != null && !m_netPrefabs.ContainsKey(prefab.m_netAI.Slope))
                        m_netPrefabs.Add(prefab.m_netAI.Slope, prefab);
                    if (prefab.m_netAI.Tunnel != null && !m_netPrefabs.ContainsKey(prefab.m_netAI.Tunnel))
                        m_netPrefabs.Add(prefab.m_netAI.Tunnel, prefab);

                    prefab.m_defaultMaxTurnAngle = info.m_maxTurnAngle;
                }
            }

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;

                if (!m_netPrefabs.ContainsKey(info))
                {
                    prefabsAdded += info.name + "\n";
                    m_netPrefabs.Add(info, new NetPrefab(info));

                    if (info.m_flattenTerrain && !info.m_netAI.IsUnderground())
                    {
                        info.m_followTerrain = false;
                    }
                }
            }

            DebugUtils.Log($"Registered roads: {m_netPrefabs.Count}\n{prefabsAdded}");
        }

        public static NetPrefab GetPrefab(NetInfo info)
        {
            if (info != null && m_netPrefabs.ContainsKey(info)) return m_netPrefabs[info];

            return null;
        }

        public static bool SingleMode
        {
            get { return m_singleMode; }
            set
            {
                if (value == m_singleMode) return;
                m_singleMode = false;

                foreach (NetPrefab prefab in m_netPrefabs.Values)
                {
                    if (value)
                    {
                        prefab.mode = Mode.Single;
                        prefab.Update();
                    }
                    else
                    {
                        prefab.Restore(false);
                    }
                }

                m_singleMode = value;
            }
        }

        public static void SetMaxTurnAngle(float angle)
        {
            if (m_netPrefabs == null) return;

            foreach (NetPrefab road in m_netPrefabs.Values)
            {
                if ((road.prefab.m_connectGroup & (NetInfo.ConnectGroup.CenterTram | NetInfo.ConnectGroup.NarrowTram | NetInfo.ConnectGroup.SingleTram | NetInfo.ConnectGroup.WideTram)) != NetInfo.ConnectGroup.None)
                {
                    //DebugUtils.Log("SetMaxTurnAngle on " + road.prefab.name);

                    road.prefab.m_maxTurnAngle = road.m_defaultMaxTurnAngle;
                    road.prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * road.m_defaultMaxTurnAngle);

                    if (road.m_defaultMaxTurnAngle > angle)
                    {
                        road.prefab.m_maxTurnAngle = angle;
                        road.prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * angle);
                    }
                }
            }
        }

        public static void ResetMaxTurnAngle()
        {
            if (m_netPrefabs == null) return;

            foreach (NetPrefab road in m_netPrefabs.Values)
            {
                road.prefab.m_maxTurnAngle = road.m_defaultMaxTurnAngle;
                road.prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * road.m_defaultMaxTurnAngle);
            }
        }

        public bool isValid()
        {
            return m_slope != null || m_tunnel == null;
        }

        public void Restore(bool revertDetour)
        {
            if (m_prefab == null) return;

            if (m_singleMode)
            {
                SingleMode = false;
                return;
            }

            if (m_hasElevation)
            {
                m_netAI.Info = m_prefab;
                m_netAI.Elevated = m_elevated;
                m_netAI.Bridge = m_bridge;
                m_netAI.Slope = m_slope;
                m_netAI.Tunnel = m_tunnel;
            }
        }

        public Mode mode
        {
            get { return m_mode; }
            set
            {
                if (m_prefab == null || !m_hasElevation) return;

                m_mode = value;
            }
        }

        public NetInfo prefab
        {
            get { return m_prefab; }
        }

        public NetAIWrapper netAI
        {
            get { return m_netAI; }
        }

        public bool hasElevation
        {
            get { return m_hasElevation; }
        }

        public bool hasVariation
        {
            get { return m_elevated != null || m_bridge != null || m_slope != null || m_tunnel != null; }
        }

        public bool LinearMiddleHeight()
        {
            return true;
        }

        public void Update()
        {
            if (m_prefab == null) return;

            Restore(!NetworkAnarchy.instance.StraightSlope);

            NetSkins_Support.ForceUpdate();

            if (!hasElevation) return;

            switch (m_mode)
            {
                case Mode.Ground:
                    if (m_prefab.m_flattenTerrain)
                    {
                        m_netAI.Elevated = m_prefab;
                        m_netAI.Bridge = null;
                        m_netAI.Slope = null;
                        m_netAI.Tunnel = m_prefab;
                    }
                    break;
                case Mode.Elevated:
                    if (m_elevated != null)
                    {
                        m_netAI.Info = m_elevated;
                        m_netAI.Elevated = m_elevated;
                        m_netAI.Bridge = null;
                    }
                    break;
                case Mode.Bridge:
                    if (m_bridge != null)
                    {
                        m_netAI.Info = m_bridge;
                        m_netAI.Elevated = m_bridge;
                    }
                    break;
                case Mode.Tunnel:
                    if (m_tunnel != null && m_slope != null)
                    {
                        m_netAI.Info = m_tunnel;
                        m_netAI.Elevated = m_tunnel;
                        m_netAI.Bridge = null;
                        m_netAI.Slope = m_tunnel;
                    }
                    break;
                case Mode.Single:
                    m_netAI.Elevated = null;
                    m_netAI.Bridge = null;
                    m_netAI.Slope = null;
                    m_netAI.Tunnel = null;
                    break;
            }
        }
    }
}

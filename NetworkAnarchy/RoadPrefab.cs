using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using NetworkAnarchy.Redirection;

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

    public class RoadPrefab
    {
        private NetInfo m_prefab;
        private NetInfo m_elevated;
        private NetInfo m_bridge;
        private NetInfo m_slope;
        private NetInfo m_tunnel;

        private RoadAIWrapper m_roadAI;
        private Mode m_mode;
        private bool m_hasElevation;
        private bool m_detoured;

        private float m_defaultMaxTurnAngle;

        private Dictionary<MethodInfo, RedirectCallsState> m_redirections = new Dictionary<MethodInfo, RedirectCallsState>();

        public static Dictionary<NetInfo, RoadPrefab> m_roadPrefabs;
        private static bool m_singleMode;
        private static MethodInfo m_LinearMiddleHeight = typeof(RoadPrefab).GetMethod("LinearMiddleHeight");

        private RoadPrefab(NetInfo prefab)
        {
            m_roadAI = new RoadAIWrapper(prefab.m_netAI);

            m_prefab = prefab;

            if (m_hasElevation = m_roadAI.hasElevation)
            {
                m_elevated = m_roadAI.elevated;
                m_bridge = m_roadAI.bridge;
                m_slope = m_roadAI.slope;
                m_tunnel = m_roadAI.tunnel;
            }

            HashSet<Type> types = new HashSet<Type>();

            try
            {
                if (m_prefab != null) types.Add(m_prefab.m_netAI.GetType().GetMethod("LinearMiddleHeight").DeclaringType);
                if (m_elevated != null) types.Add(m_elevated.m_netAI.GetType().GetMethod("LinearMiddleHeight").DeclaringType);
                if (m_bridge != null) types.Add(m_bridge.m_netAI.GetType().GetMethod("LinearMiddleHeight").DeclaringType);
                if (m_slope != null) types.Add(m_slope.m_netAI.GetType().GetMethod("LinearMiddleHeight").DeclaringType);
                if (m_tunnel != null) types.Add(m_tunnel.m_netAI.GetType().GetMethod("LinearMiddleHeight").DeclaringType);

                foreach (Type type in types)
                {
                    m_redirections[type.GetMethod("LinearMiddleHeight")] = default(RedirectCallsState);
                }
            }
            catch(Exception e)
            {
                DebugUtils.Log("Getting RoadPrefab LinearMiddleHeight redirection failed: " + prefab.name);
                DebugUtils.LogException(e);
            }
        }

        public static void Initialize()
        {
            m_roadPrefabs = new Dictionary<NetInfo, RoadPrefab>();

            //string prefabsAdded = "";

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;

                RoadPrefab prefab = new RoadPrefab(info);
                if (prefab.m_hasElevation && prefab.isValid() && !m_roadPrefabs.ContainsKey(info))
                {
                    //prefabsAdded += info.name + "\n";
                    m_roadPrefabs.Add(info, prefab);

                    if (info.m_flattenTerrain &&
                        !info.m_netAI.IsUnderground() &&
                        !prefab.m_roadAI.IsInvisible() &&
                        info != prefab.roadAI.elevated &&
                        info != prefab.roadAI.bridge &&
                        info != prefab.roadAI.slope &&
                        info != prefab.roadAI.tunnel)
                    {
                        info.m_followTerrain = false;
                    }

                    if (prefab.m_roadAI.elevated != null && !m_roadPrefabs.ContainsKey(prefab.m_roadAI.elevated))
                        m_roadPrefabs.Add(prefab.m_roadAI.elevated, prefab);
                    if (prefab.m_roadAI.bridge != null && !m_roadPrefabs.ContainsKey(prefab.m_roadAI.bridge))
                        m_roadPrefabs.Add(prefab.m_roadAI.bridge, prefab);
                    if (prefab.m_roadAI.slope != null && !m_roadPrefabs.ContainsKey(prefab.m_roadAI.slope))
                        m_roadPrefabs.Add(prefab.m_roadAI.slope, prefab);
                    if (prefab.m_roadAI.tunnel != null && !m_roadPrefabs.ContainsKey(prefab.m_roadAI.tunnel))
                        m_roadPrefabs.Add(prefab.m_roadAI.tunnel, prefab);

                    prefab.m_defaultMaxTurnAngle = info.m_maxTurnAngle;
                }
            }

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;

                if (!m_roadPrefabs.ContainsKey(info))
                {
                    m_roadPrefabs.Add(info, new RoadPrefab(info));

                    if (info.m_flattenTerrain && !info.m_netAI.IsUnderground())
                    {
                        info.m_followTerrain = false;
                    }
                }
            }

            //DebugUtils.Log("Registered roads:\n" + prefabsAdded);
        }

        public static RoadPrefab GetPrefab(NetInfo info)
        {
            if (info != null && m_roadPrefabs.ContainsKey(info)) return m_roadPrefabs[info];

            return null;
        }

        public static bool singleMode
        {
            get { return m_singleMode; }
            set
            {
                if (value == m_singleMode) return;
                m_singleMode = false;

                foreach (RoadPrefab prefab in m_roadPrefabs.Values)
                {
                    if (value)
                    {
                        prefab.mode = Mode.Single;
                        prefab.Update(prefab.m_detoured);
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
            if (m_roadPrefabs == null) return;

            foreach (RoadPrefab road in m_roadPrefabs.Values)
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
            if (m_roadPrefabs == null) return;

            foreach (RoadPrefab road in m_roadPrefabs.Values)
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

            if (m_detoured && revertDetour)
            {
                m_detoured = false;

                foreach (KeyValuePair<MethodInfo, RedirectCallsState> current in m_redirections)
                {
                    RedirectionHelper.RevertRedirect(current.Key, current.Value);
                }
            }

            if (m_singleMode)
            {
                singleMode = false;
                return;
            }

            if (m_hasElevation)
            {
                m_roadAI.info = m_prefab;
                m_roadAI.elevated = m_elevated;
                m_roadAI.bridge = m_bridge;
                m_roadAI.slope = m_slope;
                m_roadAI.tunnel = m_tunnel;
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

        public RoadAIWrapper roadAI
        {
            get { return m_roadAI; }
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

        public void Update(bool straightSlope)
        {
            if (m_prefab == null) return;

            Restore(!straightSlope);

            if (straightSlope && !m_detoured)
            {
                List<MethodInfo> methods = new List<MethodInfo>(m_redirections.Keys);
                foreach (MethodInfo from in methods)
                {
                    m_redirections[from] = RedirectionHelper.RedirectCalls(from, m_LinearMiddleHeight);
                }

                m_detoured = true;
            }

            NetSkins_Support.ForceUpdate();

            if (!hasElevation) return;

            switch (m_mode)
            {
                case Mode.Ground:
                    if (m_prefab.m_flattenTerrain)
                    {
                        m_roadAI.elevated = m_prefab;
                        m_roadAI.bridge = null;
                        m_roadAI.slope = null;
                        m_roadAI.tunnel = m_prefab;
                    }
                    break;
                case Mode.Elevated:
                    if (m_elevated != null)
                    {
                        m_roadAI.info = m_elevated;
                        m_roadAI.elevated = m_elevated;
                        m_roadAI.bridge = null;
                    }
                    break;
                case Mode.Bridge:
                    if (m_bridge != null)
                    {
                        m_roadAI.info = m_bridge;
                        m_roadAI.elevated = m_bridge;
                    }
                    break;
                case Mode.Tunnel:
                    if (m_tunnel != null && m_slope != null)
                    {
                        m_roadAI.info = m_tunnel;
                        m_roadAI.elevated = m_tunnel;
                        m_roadAI.bridge = null;
                        m_roadAI.slope = m_tunnel;
                    }
                    break;
                case Mode.Single:
                    m_roadAI.elevated = null;
                    m_roadAI.bridge = null;
                    m_roadAI.slope = null;
                    m_roadAI.tunnel = null;
                    break;
            }
        }
    }
}

using QCommonLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkAnarchy
{
    public enum Modes
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
        private readonly NetInfo m_prefab;
        private readonly NetInfo m_elevated;
        private readonly NetInfo m_bridge;
        private readonly NetInfo m_slope;
        private readonly NetInfo m_tunnel;

        private readonly NetAIWrapper m_netAI;
        private readonly bool m_hasElevation;

        private float m_defaultMaxTurnAngle;

        internal static QLogger Log;

        /// <summary>
        /// Dictionary of base-game prefab to Network Anarchy wrapper
        /// </summary>
        public static Dictionary<NetInfo, NetPrefab> m_netPrefabs;

        private NetPrefab(NetInfo prefab)
        {
            Log = ModInfo.Log;

            m_netAI = new NetAIWrapper(prefab.m_netAI);

            m_prefab = prefab;

            m_hasElevation = m_netAI.HasElevation;
            if (m_hasElevation)
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
                    prefabsAdded += info.name;
                    m_netPrefabs.Add(info, prefab);

                    if (info.m_flattenTerrain &&
                        !info.m_netAI.IsUnderground() &&
                        !prefab.m_netAI.IsInvisible() &&
                        info != prefab.NetAI.Elevated &&
                        info != prefab.NetAI.Bridge &&
                        info != prefab.NetAI.Slope &&
                        info != prefab.NetAI.Tunnel)
                    {
                        prefabsAdded += " (FollowTerrain off)";
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
                    prefabsAdded += "\n";
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

            ModInfo.Log.Info($"Registered networks: {m_netPrefabs.Count}", "[NA36]");
#if DEBUG
            ModInfo.Log.Debug($"\n{prefabsAdded}", "[NA37]");
#endif
        }

        public static NetPrefab GetPrefab(NetInfo info)
        {
            if (info != null && m_netPrefabs.ContainsKey(info)) return m_netPrefabs[info];

            return null;
        }

        /// <summary>
        /// Is the player placing a single object (rather than a draggable network)?
        /// </summary>
        private static bool m_singleMode;
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
                        prefab.Mode = Modes.Single;
                        prefab.Update();
                    }
                    else
                    {
                        prefab.Restore();
                    }
                }

                m_singleMode = value;
            }
        }

        /// <summary>
        /// The placement mode for this specific prefab
        /// </summary>
        private Modes m_mode;
        public Modes Mode
        {
            get { return m_mode; }
            set
            {
                //if (prefab.name == "Highway") Log.Debug($"Set Mode {NetworkAnarchy.instance.IsBuildingIntersection()}/{Mode}", "[T01]");
                if (m_prefab == null || !m_hasElevation) return;

                m_mode = value;
            }
        }

        /// <summary>
        /// The base-game prefab
        /// </summary>
        public NetInfo Prefab
        {
            get { return m_prefab; }
        }

        /// <summary>
        /// The base-game prefab's AI
        /// </summary>
        public NetAIWrapper NetAI
        {
            get { return m_netAI; }
        }

        /// <summary>
        /// Does network have placement modes other than on-ground?
        /// </summary>
        public bool HasElevation
        {
            get { return m_hasElevation; }
        }

        /// <summary>
        /// Does network have placement modes other than on-ground?
        /// </summary>
        public bool HasVariation
        {
            get { return m_elevated != null || m_bridge != null || m_slope != null || m_tunnel != null; }
        }

        //public bool LinearMiddleHeight()
        //{
        //    return true;
        //}

        public static void SetMaxTurnAngle(float angle)
        {
            if (m_netPrefabs == null) return;

            foreach (NetPrefab road in m_netPrefabs.Values)
            {
                if ((road.Prefab.m_connectGroup & (NetInfo.ConnectGroup.CenterTram | NetInfo.ConnectGroup.NarrowTram | NetInfo.ConnectGroup.SingleTram | NetInfo.ConnectGroup.WideTram)) != NetInfo.ConnectGroup.None)
                {
                    //DebugUtils.Log("SetMaxTurnAngle on " + road.prefab.name);

                    road.Prefab.m_maxTurnAngle = road.m_defaultMaxTurnAngle;
                    road.Prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * road.m_defaultMaxTurnAngle);

                    if (road.m_defaultMaxTurnAngle > angle)
                    {
                        road.Prefab.m_maxTurnAngle = angle;
                        road.Prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * angle);
                    }
                }
            }
        }

        public static void ResetMaxTurnAngle()
        {
            if (m_netPrefabs == null) return;

            foreach (NetPrefab road in m_netPrefabs.Values)
            {
                road.Prefab.m_maxTurnAngle = road.m_defaultMaxTurnAngle;
                road.Prefab.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * road.m_defaultMaxTurnAngle);
            }
        }

        public bool isValid()
        {
            return m_slope != null || m_tunnel == null;
        }

        /// <summary>
        /// Restore prefab wrapper to original settings
        /// </summary>
        public void Restore()
        {
            if (m_prefab == null) return;
            //if (prefab.name == "Highway") Log.Debug($"Restore {NetworkAnarchy.instance.IsBuildingIntersection()}/{Mode}", "[T04]");
            if (NetworkAnarchy.instance.IsBuildingGroundIntersection()) return;

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

        /// <summary>
        /// Set the prefab wrapper to the selected placement mode
        /// </summary>
        public void Update()
        {
            //ModInfo.Log.Debug($"Update {m_netAI},{m_prefab} intersection:{NetworkAnarchy.instance.IsBuildingIntersection()}");
            if (m_prefab == null) return;

            Restore();

            //ModInfo.Log.Debug($"{m_prefab},{m_netAI} hasElevation:{hasElevation} flatten:{m_prefab.m_flattenTerrain} mode:{m_mode}\n  Ground:{m_netAI.Info}\n  Elevated:{m_netAI.Elevated}\n  Bridge:{m_netAI.Bridge}\n  Tunnel:{m_netAI.Tunnel}");

            //if (prefab.name == "Highway") Log.Debug($"Update {NetworkAnarchy.instance.IsBuildingIntersection()}/{Mode} hasEl:{hasElevation}", "[T03]");

            Mods.NetworkSkins.ForceUpdate();

            if (!HasElevation) return;

            //if (NetworkAnarchy.instance.IsBuildingIntersection())
            //{
            //    m_netAI.Elevated = null;
            //    m_netAI.Bridge = null;
            //    m_netAI.Slope = null;
            //    m_netAI.Tunnel = null;
            //    return;
            //}

            switch (Mode)
            {
                case Modes.Ground:
                    {
                        m_netAI.Elevated = m_prefab;
                        m_netAI.Bridge = null;
                        m_netAI.Slope = null;
                        m_netAI.Tunnel = m_prefab;
                    }
                    break;
                case Modes.Elevated:
                    if (m_elevated != null)
                    {
                        m_netAI.Info = m_elevated;
                        m_netAI.Elevated = m_elevated;
                        m_netAI.Bridge = null;
                    }
                    break;
                case Modes.Bridge:
                    if (m_bridge != null)
                    {
                        m_netAI.Info = m_bridge;
                        m_netAI.Elevated = m_bridge;
                    }
                    break;
                case Modes.Tunnel:
                    if (m_tunnel != null && m_slope != null)
                    {
                        m_netAI.Info = m_tunnel;
                        m_netAI.Elevated = m_tunnel;
                        m_netAI.Bridge = null;
                        m_netAI.Slope = m_tunnel;
                    }
                    break;
                case Modes.Single:
                    m_netAI.Elevated = null;
                    m_netAI.Bridge = null;
                    m_netAI.Slope = null;
                    m_netAI.Tunnel = null;
                    break;
            }
        }
    }
}

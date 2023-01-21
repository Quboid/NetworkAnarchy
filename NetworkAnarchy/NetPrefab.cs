using QCommonLib;
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
        public static Dictionary<NetInfo, NetInfo> s_toGroundMap = new Dictionary<NetInfo, NetInfo>();

        protected readonly bool m_hasElevation;
        protected readonly NetInfo m_prefab;
        protected readonly NetInfo m_elevated;
        protected readonly NetInfo m_bridge;
        protected readonly NetInfo m_slope;
        protected readonly NetInfo m_tunnel;

        protected NetInfo netInfo;
        protected NetAIWrapper netAI;
        public NetAIWrapper NetAI => netAI;

        // Needs to check if it's elevated/etc and pick ground version
        public NetPrefab(NetInfo info)
        {
            netInfo = GetOnGround(info);
            netAI = new NetAIWrapper(netInfo.m_netAI);

            m_hasElevation = netAI.HasElevation;
            if (m_hasElevation)
            {
                m_elevated = netAI.Elevated;
                m_bridge = netAI.Bridge;
                m_slope = netAI.Slope;
                m_tunnel = netAI.Tunnel;
            }
        }

        public virtual bool HasVariation => (m_elevated != null || m_bridge != null || m_slope != null || m_tunnel != null);

        public static NetPrefab Factory(NetInfo info)
        {
            return new NetPrefab(info);
        }

        public override string ToString()
        {
            return netInfo.name;
        }


        // Static Prefab managers

        private static readonly Dictionary<NetInfo, float> DefaultMaxAngles = new Dictionary<NetInfo, float>();
        private static readonly Dictionary<NetInfo, float> DefaultMaxAnglesCos = new Dictionary<NetInfo, float>();
        public static void SetMaxTurnAngle(float angle)
        {
            ResetMaxTurnAngle();
            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;
                if ((info.m_connectGroup & (NetInfo.ConnectGroup.CenterTram | NetInfo.ConnectGroup.NarrowTram | NetInfo.ConnectGroup.SingleTram | NetInfo.ConnectGroup.WideTram)) != NetInfo.ConnectGroup.None)
                {
                    DefaultMaxAngles.Add(info, info.m_maxTurnAngle);
                    DefaultMaxAnglesCos.Add(info, Mathf.Cos(Mathf.Deg2Rad * info.m_maxTurnAngle));

                    if (info.m_maxTurnAngle > angle)
                    {
                        info.m_maxTurnAngle = angle;
                        info.m_maxTurnAngleCos = Mathf.Cos(Mathf.Deg2Rad * angle);
                    }
                }
            }
        }

        public static void ResetMaxTurnAngle()
        {
            foreach (var pair in DefaultMaxAngles)
            {
                pair.Key.m_maxTurnAngle = pair.Value;
                pair.Key.m_maxTurnAngleCos = DefaultMaxAnglesCos[pair.Key];
            }
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
                m_singleMode = value;
            }
        }

        public static void CreateToGroundMap()
        {
            s_toGroundMap = new Dictionary<NetInfo, NetInfo>();
            //string msg = $"NetInfo: {PrefabCollection<NetInfo>.PrefabCount()}\n";

            HashSet<NetInfo> allNetInfos = new HashSet<NetInfo>();
            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                allNetInfos.Add(PrefabCollection<NetInfo>.GetPrefab(i));
            }
            HashSet<NetInfo> groundOnly = new HashSet<NetInfo>(allNetInfos);
            foreach (NetInfo info in allNetInfos)
            {
                if (info == null || info.m_netAI == null)
                {
                    groundOnly.Remove(info);
                    continue;
                }
                NetAIWrapper ai = new NetAIWrapper(info.m_netAI);

                // Stop on-ground segments that aren't at terrain height from being bumpy at nodes
                if (info.m_flattenTerrain && !info.m_netAI.IsUnderground() && !ai.IsInvisible() &&
                    info != ai.Elevated && info != ai.Bridge && info != ai.Slope && info != ai.Tunnel)
                {
                    info.m_followTerrain = false;
                }

                if (ai.Tunnel != null) groundOnly.Remove(ai.Tunnel);
                if (ai.Slope != null) groundOnly.Remove(ai.Slope);
                if (ai.Elevated != null) groundOnly.Remove(ai.Elevated);
                if (ai.Bridge != null) groundOnly.Remove(ai.Bridge);
            }

            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
                if (info == null) continue;
                if (info.m_netAI == null) continue;
                if (!groundOnly.Contains(info)) continue;
                NetAIWrapper ai = new NetAIWrapper(info.m_netAI);

                if (ai.Tunnel != null && !s_toGroundMap.ContainsKey(ai.Tunnel)) s_toGroundMap.Add(ai.Tunnel, info);
                if (ai.Slope != null && !s_toGroundMap.ContainsKey(ai.Slope)) s_toGroundMap.Add(ai.Slope, info);
                if (ai.Elevated != null && !s_toGroundMap.ContainsKey(ai.Elevated)) s_toGroundMap.Add(ai.Elevated, info);
                if (ai.Bridge != null && !s_toGroundMap.ContainsKey(ai.Bridge)) s_toGroundMap.Add(ai.Bridge, info);

                //if (info.name.Contains("Pedestrian") && !info.name.Contains("Street")) msg += $"  {info.name}\n    T:{ai.Tunnel != null}, S:{ai.Slope != null}, E:{ai.Elevated != null}, B:{ai.Bridge != null} ({ai.Elevated?.name})\n";
            }

            //Log.Debug($"groundOnly: {groundOnly.Count}, " + msg);
        }

        public static NetInfo GetOnGround(NetInfo info)
        {
            if (NetworkAnarchy.instance.IsBuildingIntersection())
            {
                if (!NetworkAnarchy.instance.IsBuildingGroundIntersection())
                {
                    NetAIWrapper ai = new NetAIWrapper(s_toGroundMap.ContainsKey(info) ? s_toGroundMap[info].m_netAI : info.m_netAI);
                    if (info == ai.Info)
                    { // Switch ground nodes to elevated
                        info = ai.Elevated ?? ai.Info;
                    }
                }
                // Building ground intersection, don't alter info
            }
            else if (QCommon.Scene == QCommon.SceneTypes.AssetEditor && ToolManager.instance.m_properties.m_editPrefabInfo is NetInfo editPrefab)
            {
                NetAIWrapper editAI = new NetAIWrapper(editPrefab.m_netAI);
                if (info == editPrefab || info == editAI.Elevated || info == editAI.Bridge || info == editAI.Tunnel || info == editAI.Slope)
                {
                    info = editPrefab;
                }
            }
            else if (s_toGroundMap.ContainsKey(info))
            {
                info = s_toGroundMap[info];
            }
            return info;
        }
    }
}

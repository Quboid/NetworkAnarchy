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
        protected readonly bool m_hasElevation;
        protected readonly NetInfo m_prefab;
        protected readonly NetInfo m_elevated;
        protected readonly NetInfo m_bridge;
        protected readonly NetInfo m_slope;
        protected readonly NetInfo m_tunnel;

        protected NetInfo netInfo;
        protected NetAIWrapper netAI;
        public NetAIWrapper NetAI => netAI;

        public NetPrefab(NetInfo info)
        {
            netInfo = info;
            netAI = new NetAIWrapper(info.m_netAI);

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


        // Static Prefab managers

        private static readonly Dictionary<NetInfo, float> DefaultMaxAngles = new Dictionary<NetInfo, float>();
        private static readonly Dictionary<NetInfo, float> DefaultMaxAnglesCos = new Dictionary<NetInfo, float>();
        public static void SetMaxTurnAngle(float angle)
        {
            ResetMaxTurnAngle();
            for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); i++)
            {
                NetInfo info = PrefabCollection<NetInfo>.GetPrefab(i);
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
    }
}

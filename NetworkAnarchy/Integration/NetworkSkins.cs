using ColossalFramework.UI;
using System;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy.Mods
{
    internal class NetworkSkins
    {
        private static object m_instance;

        private static FieldInfo m_selectedPrefab;
        private static MethodInfo m_update;

        private static Type m_UINetworkSkinsPanel = Type.GetType("NetworkSkins.UI.UINetworkSkinsPanel , NetworkSkins");

        private static int m_tries;

        public static void ForceUpdate()
        {
            try
            {
                if (modExists && InstanceFound())
                {
                    m_selectedPrefab.SetValue(m_instance, null);
                    m_update.Invoke(m_instance, null);
                }
            }
            catch (Exception e)
            {
                m_UINetworkSkinsPanel = null;
                m_instance = null;
                Log.Error(e, "[NA45]");
            }
        }

        public static bool modExists
        {
            get
            {
                return m_UINetworkSkinsPanel != null;
            }
        }

        public static void Init()
        {
            try
            {
                m_tries = 0;
                m_instance = null;
                m_UINetworkSkinsPanel = Type.GetType("NetworkSkins.UI.UINetworkSkinsPanel, NetworkSkins");

                if (m_UINetworkSkinsPanel != null)
                {
                    m_selectedPrefab = m_UINetworkSkinsPanel.GetField("_selectedPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
                    m_update = m_UINetworkSkinsPanel.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);

                    if (m_selectedPrefab != null && m_update != null)
                    {
                        Log.Info("NetworkAnarchy: Network Skins loaded", "[NA07]");
                    }
                    else
                    {
                        m_UINetworkSkinsPanel = null;
                        m_selectedPrefab = null;
                        m_update = null;
                        Log.Info("NetworkAnarchy: Network Skins not loaded", "[NA08]");
                    }
                }
                else
                {
                    Log.Info("NetworkAnarchy: Network Skins not found", "[NA09]");
                }
            }
            catch (Exception e)
            {
                m_UINetworkSkinsPanel = null;
                Log.Error(e, "[NA34]");
            }
        }

        private static bool InstanceFound()
        {
            try
            {
                if (m_instance == null && m_tries++ < 10)
                {
                    m_instance = GameObject.FindObjectOfType(m_UINetworkSkinsPanel);

                    if (m_tries >= 10)
                    {
                        m_UINetworkSkinsPanel = null;
                    }
                }

                return m_instance != null && ((UIComponent)m_instance).enabled;
            }
            catch (Exception e)
            {
                m_UINetworkSkinsPanel = null;
                m_instance = null;
                Log.Error(e, "[NA35]");
                return false;
            }
        }
    }
}

using ColossalFramework;
using ColossalFramework.UI;

using ICities;
using NetworkAnarchy.Detours;
using NetworkAnarchy.Redirection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        public const string settingsFileName = "NetworkAnarchy";

        public static SavedBool reduceCatenary = new SavedBool("reduceCatenary", settingsFileName, true, true);

        public static SavedBool changeMaxTurnAngle = new SavedBool("_changeMaxTurnAngle", settingsFileName, false, true);
        public static SavedFloat maxTurnAngle = new SavedFloat("_maxTurnAngle", settingsFileName, 90, true);
        public static SavedBool saved_smoothSlope = new SavedBool("smoothSlope", settingsFileName, false, true);
        public static SavedBool saved_anarchy = new SavedBool("anarchy", settingsFileName, false, true);
        public static SavedBool saved_bending = new SavedBool("bending", settingsFileName, true, true);
        public static SavedBool saved_nodeSnapping = new SavedBool("nodeSnapping", settingsFileName, true, true);
        public static SavedBool saved_collision = new SavedBool("collision", settingsFileName, true, true);

        private int m_elevation = 0;
        private readonly SavedInt m_elevationStep = new SavedInt("elevationStep", settingsFileName, 3, true);

        private NetTool m_netTool;
        private BulldozeTool m_bulldozeTool;
        private BuildingTool m_buildingTool;

        #region Reflection to private field/methods
        private FieldInfo m_elevationField;
        private FieldInfo m_elevationUpField;
        private FieldInfo m_elevationDownField;
        private FieldInfo m_buildingElevationField;
        private FieldInfo m_controlPointCountField;
        private FieldInfo m_upgradingField;
        private FieldInfo m_placementErrorsField;
        #endregion

        private bool m_keyDisabled;

        private NetInfo m_current;
        private InfoManager.InfoMode m_infoMode = (InfoManager.InfoMode) (-1);

        private Mode m_mode;
        private bool m_straightSlope = saved_smoothSlope;

        private bool m_buttonExists;
        private bool m_activated;
        private bool m_toolEnabled;
        private bool m_buttonInOptionsBar;
        private bool m_inEditor;

        private int m_fixNodesCount = 0;
        private ushort m_fixTunnelsCount = 0;
        private readonly Stopwatch m_stopWatch = new Stopwatch();

        private int m_segmentCount;
        private int m_controlPointCount;
        private NetTool.ControlPoint[] m_controlPoints;
        private NetTool.ControlPoint[] m_cachedControlPoints;

        public static NetworkAnarchy instance;

        private static UIToolOptionsButton m_toolOptionButton;
        private static UIButton m_upgradeButtonTemplate;

        public static FastList<NetInfo> bendingPrefabs = new FastList<NetInfo>();

        public static UIButton chirperButton;
        public static UITextureAtlas chirperAtlasAnarchy;
        public static UITextureAtlas chirperAtlasNormal;

        public bool singleMode
        {
            get => RoadPrefab.singleMode;
            set => RoadPrefab.singleMode = value;
        }

        public Mode mode
        {
            get => m_mode;
            set {
                if (value != m_mode)
                {
                    m_mode = value;

                    var prefab = RoadPrefab.GetPrefab(m_current);
                    if (prefab == null)
                    {
                        return;
                    }

                    prefab.mode = m_mode;
                    prefab.Update(straightSlope);
                    m_toolOptionButton.UpdateInfo();
                }
            }
        }

        public int elevationStep
        {
            get => m_elevationStep.value;
            set => m_elevationStep.value = Mathf.Clamp(value, 1, 12);
        }

        public int elevation => Mathf.RoundToInt(m_elevation / 256f * 12f);

        public bool isActive => m_activated;

        public bool straightSlope
        {
            get => m_straightSlope;

            set {
                if (value != m_straightSlope)
                {
                    m_straightSlope = value;

                    m_toolOptionButton.UpdateInfo();

                    var prefab = RoadPrefab.GetPrefab(m_current);
                    if (prefab == null)
                    {
                        return;
                    }

                    prefab.Update(straightSlope);

                    saved_smoothSlope.value = value;
                }
            }
        }

        public void Start()
        {
            NetSkins_Support.Init();

            // Getting NetTool
            m_netTool = GameObject.FindObjectsOfType<NetTool>().Where(x => x.GetType() == typeof(NetTool)).FirstOrDefault();
            if (m_netTool == null)
            {
                DebugUtils.Warning("NetTool not found.");
                enabled = false;
                return;
            }

            // Getting BulldozeTool
            m_bulldozeTool = GameObject.FindObjectsOfType<BulldozeTool>().Where(x => x.GetType() == typeof(BulldozeTool)).FirstOrDefault();
            if (m_bulldozeTool == null)
            {
                DebugUtils.Warning("BulldozeTool not found.");
                enabled = false;
                return;
            }

            // Getting BuildingTool
            m_buildingTool = GameObject.FindObjectsOfType<BuildingTool>().Where(x => x.GetType() == typeof(BuildingTool)).FirstOrDefault();
            if (m_buildingTool == null)
            {
                DebugUtils.Warning("BuildingTool not found.");
                enabled = false;
                return;
            }

            // Getting NetTool private fields
            m_elevationField = m_netTool.GetType().GetField("m_elevation", BindingFlags.NonPublic | BindingFlags.Instance);
            m_elevationUpField = m_netTool.GetType().GetField("m_buildElevationUp", BindingFlags.NonPublic | BindingFlags.Instance);
            m_elevationDownField = m_netTool.GetType().GetField("m_buildElevationDown", BindingFlags.NonPublic | BindingFlags.Instance);
            m_buildingElevationField = m_buildingTool.GetType().GetField("m_elevation", BindingFlags.NonPublic | BindingFlags.Instance);
            m_controlPointCountField = m_netTool.GetType().GetField("m_controlPointCount", BindingFlags.NonPublic | BindingFlags.Instance);
            m_upgradingField = m_netTool.GetType().GetField("m_upgrading", BindingFlags.NonPublic | BindingFlags.Instance);
            m_placementErrorsField = m_buildingTool.GetType().GetField("m_placementErrors", BindingFlags.NonPublic | BindingFlags.Instance);

            if (m_elevationField == null || m_elevationUpField == null || m_elevationDownField == null || m_buildingElevationField == null || m_controlPointCountField == null || m_upgradingField == null || m_placementErrorsField == null)
            {
                DebugUtils.Warning("NetTool fields not found");
                m_netTool = null;
                enabled = false;
                return;
            }

            bendingPrefabs.Clear();
            int count = PrefabCollection<NetInfo>.PrefabCount();
            for (uint i = 0; i < count; i++)
            {
                NetInfo prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if (prefab != null)
                {
                    if (prefab.m_enableBendingSegments)
                    {
                        bendingPrefabs.Add(prefab);
                    }
                }
            }

            Redirector<NetInfoDetour>.Deploy();
            collision = (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) == ItemClass.Availability.None;

            if (chirperAtlasAnarchy == null)
            {
                LoadChirperAtlas();
            }

            chirperButton = UIView.GetAView().FindUIComponent<UIButton>("Zone");

            // Getting Upgrade button template
            try
            {
                m_upgradeButtonTemplate = GameObject.Find("RoadsSmallPanel").GetComponent<GeneratedScrollPanel>().m_OptionsBar.Find<UIButton>("Upgrade");
            }
            catch
            {
                DebugUtils.Warning("Upgrade button template not found");
            }

            // Creating UI
            CreateToolOptionsButton();

            // Store segment count
            m_segmentCount = NetManager.instance.m_segmentCount;

            // Getting control points
            try
            {
                m_controlPoints = m_netTool.GetType().GetField("m_controlPoints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(m_netTool) as NetTool.ControlPoint[];
                m_cachedControlPoints = m_netTool.GetType().GetField("m_cachedControlPoints", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(m_netTool) as NetTool.ControlPoint[];
            }
            catch
            {
                DebugUtils.Warning("ControlPoints not found");
            }

            // Init dictionary
            RoadPrefab.Initialize();

            m_inEditor = (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None;
            RoadPrefab.singleMode = m_inEditor;

            if (changeMaxTurnAngle.value)
            {
                RoadPrefab.SetMaxTurnAngle(maxTurnAngle.value);
            }

            // Update Catenary
            UpdateCatenary();

            // Fix nodes
            FixNodes();

            // Load Anarchy saved settings
            anarchy = saved_anarchy.value;
            bending = saved_bending.value;
            snapping = saved_nodeSnapping.value;
            collision = saved_collision.value;

            OptionsKeymapping.RegisterUUIHotkeys();

            DebugUtils.Log("Initialized");
        }

        public void Update()
        {
            if (m_netTool == null)
            {
                return;
            }

            try
            {
                // Getting selected prefab
                NetInfo prefab = m_netTool.enabled || m_bulldozeTool.enabled ? m_netTool.m_prefab : null;

                // Has the prefab/tool changed?
                if (prefab != m_current || m_toolEnabled != m_netTool.enabled)
                {
                    m_toolEnabled = m_netTool.enabled;

                    if (prefab == null)
                    {
                        Deactivate();
                    }
                    else
                    {
                        Activate(prefab);
                    }

                    if (m_toolOptionButton != null)
                    {
                        m_toolOptionButton.isVisible = m_activated || !m_buttonInOptionsBar;
                    }
                }

                // Plopping intersection?
                if (m_buildingTool.enabled)
                {
                    if (!RoadPrefab.singleMode)
                    {
                        int elevation = (int) m_buildingElevationField.GetValue(m_buildingTool);
                        RoadPrefab.singleMode = (elevation == 0);
                    }
                }
                else
                {
                    RoadPrefab.singleMode = m_inEditor && !UIView.HasModalInput() && !m_netTool.enabled && !m_bulldozeTool.enabled;
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Update failed");
                DebugUtils.LogException(e);

                try
                {
                    Deactivate();
                    RoadPrefab.singleMode = false;
                }
                catch { }
            }
        }

        public void OnDisable()
        {
            Deactivate();
            RoadPrefab.singleMode = false;
        }

        public class AfterSimulationTick : ThreadingExtensionBase
        {
            public override void OnAfterSimulationTick()
            {
                if (NetworkAnarchy.instance == null || !NetworkAnarchy.instance.enabled)
                {
                    return;
                }

                try
                {
                    NetworkAnarchy.instance.OnAfterSimulationTick();
                }
                catch (Exception e)
                {
                    DebugUtils.Log("OnAfterSimulationTick failed");
                    DebugUtils.LogException(e);
                }
            }
        }

        public virtual void OnAfterSimulationTick()
        {
            if (m_buildingTool == null)
            {
                return;
            }

            // Removes HeightTooHigh & TooShort errors
            if (m_buildingTool.enabled)
            {
                var errors = (ToolBase.ToolErrors) m_placementErrorsField.GetValue(m_buildingTool);
                if ((errors & ToolBase.ToolErrors.HeightTooHigh) == ToolBase.ToolErrors.HeightTooHigh)
                {
                    errors = errors & ~ToolBase.ToolErrors.HeightTooHigh;
                    m_placementErrorsField.SetValue(m_buildingTool, errors);
                }

                if ((errors & ToolBase.ToolErrors.TooShort) == ToolBase.ToolErrors.TooShort)
                {
                    errors = errors & ~ToolBase.ToolErrors.TooShort;
                    m_placementErrorsField.SetValue(m_buildingTool, errors);
                }
            }

            // Resume fixes
            if (m_fixNodesCount != 0 || m_fixTunnelsCount != 0)
            {
                var prefab = RoadPrefab.GetPrefab(m_current);
                if (prefab != null)
                {
                    prefab.Restore(false);
                }

                if (m_fixTunnelsCount != 0)
                {
                    FixTunnels();
                }

                if (m_fixNodesCount != 0)
                {
                    FixNodes();
                }

                if (prefab != null)
                {
                    prefab.Update(false);
                }
            }

            if (!isActive && !m_bulldozeTool.enabled)
            {
                return;
            }

            // Check if segment have been created/deleted/updated
            if (m_segmentCount != NetManager.instance.m_segmentCount || (bool) m_upgradingField.GetValue(m_netTool))
            {
                m_segmentCount = NetManager.instance.m_segmentCount;

                var prefab = RoadPrefab.GetPrefab(m_current);
                if (prefab != null)
                {
                    prefab.Restore(false);
                }

                m_fixTunnelsCount = 0;
                m_fixNodesCount = 0;

                FixTunnels();
                FixNodes();

                if (prefab != null)
                {
                    prefab.Update(straightSlope);
                }
            }

            if (!isActive)
            {
                return;
            }

            // Fix first control point elevation
            int count = (int) m_controlPointCountField.GetValue(m_netTool);
            if (count != m_controlPointCount && m_controlPointCount == 0 && count == 1)
            {
                if (FixControlPoint(0))
                {
                    m_elevation = Mathf.RoundToInt(Mathf.RoundToInt(m_controlPoints[0].m_elevation / elevationStep) * elevationStep * 256f / 12f);
                    UpdateElevation();
                    if (m_toolOptionButton != null)
                    {
                        m_toolOptionButton.UpdateInfo();
                    }
                }
            }
            // Fix last control point elevation
            else if (count == ((m_netTool.m_mode == NetTool.Mode.Curved || m_netTool.m_mode == NetTool.Mode.Freeform) ? 2 : 1))
            {
                FixControlPoint(count);
            }
            m_controlPointCount = count;
        }

        private void Activate(NetInfo info)
        {
            if (info == null)
            {
                return;
            }

            var prefab = RoadPrefab.GetPrefab(m_current);
            if (prefab != null)
            {
                prefab.Restore(true);
            }

            m_current = info;
            prefab = RoadPrefab.GetPrefab(info);

            AttachToolOptionsButton(prefab);

            // Is it a valid prefab?
            m_current.m_netAI.GetElevationLimits(out int min, out int max);

            if ((m_bulldozeTool.enabled || (min == 0 && max == 0)) && !m_buttonExists)
            {
                Deactivate();
                return;
            }

            DisableDefaultKeys();
            m_elevation = (int) m_elevationField.GetValue(m_netTool);
            if (prefab != null)
            {
                prefab.mode = m_mode;
                prefab.Update(straightSlope);
            }
            else
            {
                DebugUtils.Log("Selected prefab not registered");
            }

            m_segmentCount = NetManager.instance.m_segmentCount;
            m_controlPointCount = 0;

            m_activated = true;
            m_toolOptionButton.isVisible = true;
            m_toolOptionButton.UpdateInfo();
        }

        private void Deactivate()
        {
            if (!isActive)
            {
                return;
            }

            var prefab = RoadPrefab.GetPrefab(m_current);
            if (prefab != null)
            {
                prefab.Restore(true);
            }

            m_current = null;

            RestoreDefaultKeys();

            m_activated = false;

            DebugUtils.Log($"Deactivated \n {new StackTrace().ToString()}");
        }

        private void DisableDefaultKeys()
        {
            if (m_keyDisabled)
            {
                return;
            }

            var emptyKey = new SavedInputKey("", Settings.gameSettingsFile);

            m_elevationUpField.SetValue(m_netTool, emptyKey);
            m_elevationDownField.SetValue(m_netTool, emptyKey);

            m_keyDisabled = true;
        }

        private void RestoreDefaultKeys()
        {
            if (!m_keyDisabled)
            {
                return;
            }

            m_elevationUpField.SetValue(m_netTool, OptionsKeymapping.elevationUp);
            m_elevationDownField.SetValue(m_netTool, OptionsKeymapping.elevationDown);

            m_keyDisabled = false;
        }

        private void UpdateElevation()
        {
            m_current.m_netAI.GetElevationLimits(out int min, out int max);

            m_elevation = Mathf.Clamp(m_elevation, min * 256, max * 256);
            if (elevationStep < 3)
            {
                m_elevation = Mathf.RoundToInt(Mathf.RoundToInt(m_elevation / (256f / 12f)) * (256f / 12f));
            }

            if ((int) m_elevationField.GetValue(m_netTool) != m_elevation)
            {
                m_elevationField.SetValue(m_netTool, m_elevation);
                m_toolOptionButton.UpdateInfo();
            }
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
        public void OnDestroy()
        {
            Redirector<NetInfoDetour>.Revert();
            anarchy = false;
        }

        public static bool anarchy
        {
            get
            {
                return Redirector<NetToolDetour>.IsDeployed();
            }

            set
            {
                if (anarchy != value)
                {
                    if (value)
                    {
                        DebugUtils.Log("Enabling anarchy");
                        Redirector<NetToolDetour>.Deploy();
                        Redirector<BuildingToolDetour>.Deploy();
                        Redirector<RoadAIDetour>.Deploy();
                        Redirector<PedestrianPathAIDetour>.Deploy();
                        Redirector<TrainTrackAIDetour>.Deploy();
                        Redirector<NetAIDetour>.Deploy();

                        if (chirperButton != null && chirperAtlasAnarchy != null)
                        {
                            chirperAtlasNormal = chirperButton.atlas;
                            chirperButton.atlas = chirperAtlasAnarchy;
                        }
                    }
                    else
                    {
                        DebugUtils.Log("Disabling anarchy");
                        Redirector<NetToolDetour>.Revert();
                        Redirector<BuildingToolDetour>.Revert();
                        Redirector<RoadAIDetour>.Revert();
                        Redirector<PedestrianPathAIDetour>.Revert();
                        Redirector<TrainTrackAIDetour>.Revert();
                        Redirector<NetAIDetour>.Revert();

                        if (chirperButton != null && chirperAtlasNormal != null)
                        {
                            chirperButton.atlas = chirperAtlasNormal;
                        }
                    }

                    saved_anarchy.value = value;
                }
            }
        }

        public static bool bending
        {
            get
            {
                return bendingPrefabs.m_size > 0 && bendingPrefabs.m_buffer[0].m_enableBendingSegments;
            }

            set
            {
                if (bending != value)
                {
                    for (int i = 0; i < bendingPrefabs.m_size; i++)
                    {
                        bendingPrefabs.m_buffer[i].m_enableBendingSegments = value;
                    }

                    saved_bending.value = value;
                }
            }
        }

        private static bool _snapping = saved_nodeSnapping.value;
        public static bool snapping
        {
            get
            {
                return _snapping;
            }

            set
            {
                if (_snapping != value)
                {
                    _snapping = value;

                    saved_nodeSnapping.value = value;
                }
            }
        }

        private static bool _collision = saved_collision.value;
        public static bool collision
        {
            get
            {
                return _collision;// !Redirector<CollisionNetNodeDetour>.IsDeployed();
            }

            set
            {
                if (value != _collision)
                {
                    if (value)
                    {
                        DebugUtils.Log("Enabling collision");
                        Redirector<CollisionBuildingManagerDetour>.Revert();
                        //Redirector<CollisionNetManagerDetour>.Revert();
                        //Redirector<CollisionNetNodeDetour>.Revert();
                        CollisionZoneBlockDetour.Revert();
                    }
                    else
                    {
                        DebugUtils.Log("Disabling collision");
                        Redirector<CollisionBuildingManagerDetour>.Deploy();
                        //Redirector<CollisionNetManagerDetour>.Deploy();
                        //Redirector<CollisionNetNodeDetour>.Deploy();
                        CollisionZoneBlockDetour.Deploy();
                    }

                    _collision = value;
                    saved_collision.value = value;
                }
            }
        }

        public static bool grid
        {
            get
            {
                return (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None;
            }

            set
            {
                if (value)
                {
                    ToolManager.instance.m_properties.m_mode = ToolManager.instance.m_properties.m_mode | ItemClass.Availability.AssetEditor;
                }
                else
                {
                    ToolManager.instance.m_properties.m_mode = ToolManager.instance.m_properties.m_mode & ~ItemClass.Availability.AssetEditor;
                }
            }
        }
    }
}

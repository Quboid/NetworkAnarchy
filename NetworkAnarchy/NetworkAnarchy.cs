using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace NetworkAnarchy
{
    public class NetworkAnarchyLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (NetworkAnarchy.instance == null)
            {
                // Creating the instance
                NetworkAnarchy.instance = new GameObject("NetworkAnarchy").AddComponent<NetworkAnarchy>();

                // Don't destroy it
                GameObject.DontDestroyOnLoad(NetworkAnarchy.instance);
            }
            else
            {
                NetworkAnarchy.instance.Start();
                NetworkAnarchy.instance.enabled = true;
            }
        }

        public override void OnLevelUnloading()
        {
            if (NetworkAnarchy.instance != null)
            {
                NetworkAnarchy.instance.enabled = false;
            }
        }
    }

    public class NetworkAnarchy : MonoBehaviour
    {
        public const string settingsFileName = "NetworkAnarchy";

        public static SavedBool reduceCatenary = new SavedBool("reduceCatenary", settingsFileName, true, true);

        public static SavedBool changeMaxTurnAngle = new SavedBool("_changeMaxTurnAngle", settingsFileName, false, true);
        public static SavedFloat maxTurnAngle = new SavedFloat("_maxTurnAngle", settingsFileName, 90, true);

        public static SavedBool saved_smoothSlope = new SavedBool("smoothSlope", settingsFileName, false, true);

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

        public void OnGUI()
        {
            try
            {
                Event e = Event.current;

                if (m_buildingTool.enabled && RoadPrefab.singleMode)
                {
                    // Checking key presses
                    if (OptionsKeymapping.elevationUp.IsPressed(e) || OptionsKeymapping.elevationDown.IsPressed(e))
                    {
                        RoadPrefab.singleMode = false;
                        BuildingInfo info = m_buildingTool.m_prefab;
                        if (info != null)
                        {
                            // Reset cached value
                            FieldInfo cachedMaxElevation = info.m_buildingAI.GetType().GetField("m_cachedMaxElevation", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (cachedMaxElevation != null)
                            {
                                cachedMaxElevation.SetValue(info.m_buildingAI, -1);
                            }

                            info.m_buildingAI.GetElevationLimits(out int min, out int max);

                            int elevation = (int) m_buildingElevationField.GetValue(m_buildingTool);
                            elevation += OptionsKeymapping.elevationUp.IsPressed(Event.current) ? 1 : -1;

                            m_buildingElevationField.SetValue(m_buildingTool, Mathf.Clamp(elevation, min, max));
                        }
                        e.Use();
                    }
                    return;
                }
                else if (m_buildingTool.enabled && OptionsKeymapping.elevationReset.IsPressed(e))
                {
                    m_buildingElevationField.SetValue(m_buildingTool, 0);
                }

                if (!isActive)
                {
                    return;
                }

                // Updating the elevation
                if (m_elevation >= 0 || m_elevation <= -256)
                {
                    int currentElevation = (int) m_elevationField.GetValue(m_netTool);
                    if (m_elevation != currentElevation)
                    {
                        m_elevation = currentElevation;
                        m_toolOptionButton.UpdateInfo();
                    }
                }
                else
                {
                    UpdateElevation();
                }

                // Checking key presses
                if (OptionsKeymapping.elevationUp.IsPressed(e))
                {
                    m_elevation += Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                }
                else if (OptionsKeymapping.elevationDown.IsPressed(e))
                {
                    m_elevation -= Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                }
                else if (OptionsKeymapping.elevationStepUp.IsPressed(e))
                {
                    if (elevationStep < 12)
                    {
                        elevationStep++;
                        m_toolOptionButton.UpdateInfo();
                    }
                }
                else if (OptionsKeymapping.elevationStepDown.IsPressed(e))
                {
                    if (elevationStep > 1)
                    {
                        elevationStep--;
                        m_toolOptionButton.UpdateInfo();
                    }
                }
                else if (OptionsKeymapping.modesCycleRight.IsPressed(e))
                {
                    if (m_mode < Mode.Tunnel)
                    {
                        mode++;
                    }
                    else
                    {
                        mode = Mode.Normal;
                    }

                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.modesCycleLeft.IsPressed(e))
                {
                    if (m_mode > Mode.Normal)
                    {
                        mode--;
                    }
                    else
                    {
                        mode = Mode.Tunnel;
                    }

                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.elevationReset.IsPressed(e))
                {
                    m_elevation = 0;
                    UpdateElevation();
                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.toggleStraightSlope.IsPressed(e))
                {
                    straightSlope = !m_straightSlope;
                    m_toolOptionButton.UpdateInfo();
                }

                if (m_mode == Mode.Tunnel && InfoManager.instance.CurrentMode != InfoManager.InfoMode.Traffic)
                {
                    if (m_infoMode == (InfoManager.InfoMode) (-1))
                    {
                        m_infoMode = InfoManager.instance.CurrentMode;
                    }

                    InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Traffic, InfoManager.SubInfoMode.Default);
                }
                else if (m_mode != Mode.Tunnel && m_infoMode != (InfoManager.InfoMode) (-1))
                {
                    InfoManager.instance.SetCurrentMode(m_infoMode, InfoManager.SubInfoMode.Default);
                    m_infoMode = (InfoManager.InfoMode) (-1);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
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

        private void FixNodes()
        {
            m_stopWatch.Reset();
            m_stopWatch.Start();

            NetNode[] nodes = NetManager.instance.m_nodes.m_buffer;

            bool singleMode = RoadPrefab.singleMode;
            RoadPrefab.singleMode = false;

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
                    RoadPrefab.singleMode = singleMode;
                    return;
                }

                NetInfo info = nodes[i].Info;
                if (info == null || info.m_netAI == null)
                {
                    continue;
                }

                var prefab = RoadPrefab.GetPrefab(info);
                if ((nodes[i].m_flags & NetNode.Flags.Underground) == NetNode.Flags.Underground)
                {
                    if (prefab == null)
                    {
                        continue;
                    }

                    if ((info.m_setVehicleFlags & Vehicle.Flags.Underground) == 0 && info != prefab.roadAI.tunnel && info != prefab.roadAI.slope && !info.m_netAI.IsUnderground())
                    {
                        nodes[i].m_elevation = 0;
                        nodes[i].m_flags = nodes[i].m_flags & ~NetNode.Flags.Underground;
                        if (info != prefab.roadAI.elevated && info != prefab.roadAI.bridge)
                        {
                            nodes[i].m_flags = nodes[i].m_flags | NetNode.Flags.OnGround;
                        }
                        // Updating terrain
                        try
                        {
                            TerrainModify.UpdateArea(nodes[i].m_bounds.min.x, nodes[i].m_bounds.min.z, nodes[i].m_bounds.max.x, nodes[i].m_bounds.max.z, true, true, false);
                        }
                        catch { }
                    }
                }
                else if ( (info != prefab.roadAI.elevated && info != prefab.roadAI.bridge) || ((nodes[i].m_flags & (NetNode.Flags.Transition | NetNode.Flags.End)) != 0 && nodes[i].m_elevation == 0) )
                {
                    nodes[i].m_flags = nodes[i].m_flags | NetNode.Flags.OnGround;
                }
                else
                {
                    nodes[i].m_flags = nodes[i].m_flags & ~NetNode.Flags.OnGround;
                }
            }

            RoadPrefab.singleMode = singleMode;
            m_fixNodesCount = 0;
        }

        private void FixTunnels()
        {
            m_stopWatch.Reset();
            m_stopWatch.Start();

            bool singleMode = RoadPrefab.singleMode;
            RoadPrefab.singleMode = false;

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
                    RoadPrefab.singleMode = singleMode;
                    return;
                }

                NetInfo info = segments[i].Info;

                ushort startNode = segments[i].m_startNode;
                ushort endNode = segments[i].m_endNode;

                var prefab = RoadPrefab.GetPrefab(info);
                if (prefab == null)
                {
                    continue;
                }

                // Is it a tunnel?
                if (info == prefab.roadAI.tunnel)
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

                    if (prefab.roadAI.slope == null)
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
                        segments[i].Info = prefab.roadAI.slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[startNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[startNode].m_flags = nodes[startNode].m_flags & ~NetNode.Flags.Underground;
                        }
                    }
                    else if (IsEndTunnel(ref nodes[endNode]))
                    {
                        // Make it a slope
                        segments[i].Info = prefab.roadAI.slope;
                        NetManager.instance.UpdateSegment(i);

                        if ((nodes[endNode].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                        {
                            nodes[endNode].m_flags = nodes[endNode].m_flags & ~NetNode.Flags.Underground;                            
                        }
                    }
                }
                // Is it a slope?
                else if (info == prefab.roadAI.slope)
                {
                    if (prefab.roadAI.tunnel == null)
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
                        segments[i].Info = prefab.roadAI.tunnel;
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

            RoadPrefab.singleMode = singleMode;
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

                var prefab = RoadPrefab.GetPrefab(info);
                if (prefab == null)
                {
                    return true;
                }

                if (info != prefab.roadAI.tunnel && info != prefab.roadAI.slope)
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

        private void CreateToolOptionsButton()
        {
            if (m_toolOptionButton != null)
            {
                return;
            }

            try
            {
                m_toolOptionButton = UIView.GetAView().AddUIComponent(typeof(UIToolOptionsButton)) as UIToolOptionsButton;

                if (m_toolOptionButton == null)
                {
                    DebugUtils.Log("Couldn't create label");
                    return;
                }

                m_toolOptionButton.autoSize = false;
                m_toolOptionButton.size = new Vector2(36, 36);
                m_toolOptionButton.position = Vector2.zero;
                m_toolOptionButton.isVisible = false;
            }
            catch (Exception e)
            {
                enabled = false;
                DebugUtils.Log("CreateToolOptionsButton failed");
                DebugUtils.LogException(e);
            }
        }

        private void AttachToolOptionsButton(RoadPrefab prefab)
        {
            m_buttonExists = false;

            RoadsOptionPanel[] panels = GameObject.FindObjectsOfType<RoadsOptionPanel>();

            foreach (RoadsOptionPanel panel in panels)
            {
                // Find the visible RoadsOptionPanel
                if (panel.component.isVisible)
                {
                    UIComponent button = panel.component.Find<UIComponent>("ElevationStep");
                    if (button == null)
                    {
                        continue;
                    }

                    // Put the main button in ElevationStep
                    m_toolOptionButton.transform.SetParent(button.transform);
                    m_buttonInOptionsBar = false;
                    button.tooltip = null;
                    m_buttonExists = true;

                    // Add Upgrade button if needed
                    var list = new List<NetTool.Mode>(panel.m_Modes);
                    if (m_upgradeButtonTemplate != null && prefab != null && prefab.hasVariation && !list.Contains(NetTool.Mode.Upgrade))
                    {
                        UITabstrip toolMode = panel.component.Find<UITabstrip>("ToolMode");
                        if (toolMode != null)
                        {
                            list.Add(NetTool.Mode.Upgrade);
                            panel.m_Modes = list.ToArray();

                            toolMode.AddTab("Upgrade", m_upgradeButtonTemplate, false);

                            DebugUtils.Log("Upgrade button added.");
                        }
                    }

                    return;
                }
            }

            // No visible RoadsOptionPanel found. Put the main button in OptionsBar instead
            UIPanel optionBar = UIView.Find<UIPanel>("OptionsBar");

            if (optionBar == null)
            {
                DebugUtils.Log("OptionBar not found!");
                return;
            }
            m_toolOptionButton.transform.SetParent(optionBar.transform);
            m_buttonInOptionsBar = true;
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

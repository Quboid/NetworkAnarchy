using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using QCommonLib;
using System.Diagnostics;
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
        public static SavedInt saved_segmentLength = new SavedInt("saved_segmentLength", settingsFileName, 96, true);
        public static SavedBool showDebugMessages = new SavedBool("showDebugMessages", settingsFileName, false, true);

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
        private ToolBase m_wasToolBase;
        private InfoManager.InfoMode m_infoMode = (InfoManager.InfoMode) (-1);

        private Mode m_mode;

        /// <summary>
        /// Does the vanilla elevation button exist?
        /// </summary>
        private bool DoesVanillaElevationButtonExist
        {
            get
            {
                return _doesVanillaElevationButtonExist;
            }
            set
            {
                _doesVanillaElevationButtonExist = value;
            }
        }
        private bool _doesVanillaElevationButtonExist;

        /// <summary>
        /// Is NA active (valid NetTool prefab selected)
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _activated;
            }
            set
            {
                _activated = value;
            }
        }
        private bool _activated;

        /// <summary>
        /// Is the vanilla NetTool active? To check if it has been toggled next tick
        /// </summary>
        //private bool IsNetToolEnabled
        //{
        //    get
        //    {
        //        return _isNetToolEnabled;
        //    }
        //    set
        //    {
        //        _isNetToolEnabled = value;
        //    }
        //}

        /// <summary>
        /// Is the NA button in the options bar instead of attached to the vanilla elevation button?
        /// </summary>
        internal bool IsButtonInOptionsBar
        {
            get
            {
                return _isButtonInOptionsBar;
            }
            set
            {
                _isButtonInOptionsBar = value;
            }
        }
        private bool _isButtonInOptionsBar;

        private int m_fixNodesCount = 0;
        private ushort m_fixTunnelsCount = 0;
        private readonly Stopwatch m_stopWatch = new Stopwatch();

        private int m_segmentCount;
        private int m_controlPointCount;
        private NetTool.ControlPoint[] m_controlPoints;
        private NetTool.ControlPoint[] m_cachedControlPoints;

        public static NetworkAnarchy instance;

        internal static UIToolOptionsButton m_toolOptionButton;
        private static UIButton m_upgradeButtonTemplate;

        public static FastList<NetInfo> bendingPrefabs = new FastList<NetInfo>();

        public static ChirperManager chirperManager;

        internal static QLogger Log;

        // Max Segment Length settings
        internal const int SegmentLengthFloor = 4;
        internal const int SegmentLengthCeiling = 256;
        internal const int SegmentLengthInterval = 2;

        private int? m_maxSegmentLength = null;
        public int MaxSegmentLength
        {
            get => m_maxSegmentLength == null ? saved_segmentLength : (int)m_maxSegmentLength;
            set => m_maxSegmentLength = Mathf.Clamp(value, SegmentLengthFloor, SegmentLengthCeiling);
        }

        /// <summary>
        /// The selected placement mode, regardless of prefab
        /// </summary>
        public Mode mode
        {
            get => m_mode;
            set {
                if (value != m_mode)
                {
                    m_mode = value;

                    var prefab = NetPrefab.GetPrefab(m_current);
                    if (prefab == null)
                    {
                        return;
                    }

                    prefab.mode = m_mode;
                    prefab.Update();
                    m_toolOptionButton.UpdateButton();
                }
            }
        }

        public int elevationStep
        {
            get => m_elevationStep.value;
            set => m_elevationStep.value = Mathf.Clamp(value, 1, 12);
        }

        public int elevation => Mathf.RoundToInt(m_elevation / 256f * 12f);

        private bool _straightSlope = saved_smoothSlope;
        public bool StraightSlope
        {
            get => _straightSlope;

            set
            {
                if (value != _straightSlope)
                {
                    Log.Debug($"Setting StraightSlope to {(value ? "enabled" : "disabled")}", "[NA47]");

                    _straightSlope = value;

                    m_toolOptionButton.UpdateButton();

                    var prefab = NetPrefab.GetPrefab(m_current);
                    if (prefab == null)
                    {
                        return;
                    }

                    prefab.Update();

                    saved_smoothSlope.value = value;
                }
            }
        }

        private static bool _anarchy = saved_anarchy.value;
        public static bool Anarchy
        {
            get
            {
                return _anarchy;
            }

            set
            {
                if (_anarchy != value)
                {
                    Log.Debug($"Setting Anarchy to {(value ? "enabled" : "disabled")}", "[NA40]");

                    _anarchy = value;
                    saved_anarchy.value = value;
                    ChirperManager.UpdateAtlas();
                }
            }
        }

        private static bool _bending = saved_bending.value;
        public static bool Bending
        {
            get
            {
                return _bending;// bendingPrefabs.m_size > 0 && bendingPrefabs.m_buffer[0].m_enableBendingSegments;
            }

            set
            {
                if (_bending != value)
                {
                    Log.Debug($"Setting Bending to {(value ? "enabled" : "disabled")}", "[NA41]");

                    for (int i = 0; i < bendingPrefabs.m_size; i++)
                    {
                        bendingPrefabs.m_buffer[i].m_enableBendingSegments = value;
                    }

                    _bending = value;
                    saved_bending.value = value;
                }
            }
        }

        private static bool _snapping = saved_nodeSnapping.value;
        public static bool NodeSnapping
        {
            get
            {
                return _snapping;
            }

            set
            {
                if (_snapping != value)
                {
                    Log.Debug($"Setting NodeSnapping to {(value ? "enabled" : "disabled")}", "[NA42]");

                    _snapping = value;
                    saved_nodeSnapping.value = value;
                }
            }
        }

        private static bool _collision = saved_collision.value;
        public static bool Collision
        {
            get
            {
                return _collision;
            }

            set
            {
                if (value != _collision)
                {
                    Log.Debug($"Setting Collision to {(value ? "enabled" : "disabled")}", "[NA43]");

                    _collision = value;
                    saved_collision.value = value;
                    ChirperManager.UpdateAtlas();
                }
            }
        }

        public static bool Grid
        {
            get
            {
                return (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None;
            }

            set
            {
                Log.Debug($"Setting Editor Grid to {(value ? "enabled" : "disabled")}", "[NA44]");
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

        public void ToggleAnarchy()
        {
            Anarchy = !Anarchy;
            UpdateAnarchyButton(m_toolOptionButton.m_anarchyBtn, "Anarchy", Anarchy);
        }
        public void ToggleBending()
        {
            Bending = !Bending;
            UpdateAnarchyButton(m_toolOptionButton.m_bendingBtn, "Bending", Bending);
        }
        public void ToggleSnapping()
        {
            NodeSnapping = !NodeSnapping;
            UpdateAnarchyButton(m_toolOptionButton.m_snappingBtn, "Snapping", NodeSnapping);
        }
        public void ToggleCollision()
        {
            Collision = !Collision;
            UpdateAnarchyButton(m_toolOptionButton.m_collisionBtn, "Collision", Collision);
        }
        public void ToggleStraightSlope()
        {
            StraightSlope = !StraightSlope;
            UpdateAnarchyButton(m_toolOptionButton.m_straightSlopeBtn, "StraightSlope", StraightSlope);
        }

        public void ToggleGrid() {
            Grid = !Grid;
            m_toolOptionButton.UpdateButton();
        }
    }
}

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using NetworkAnarchy.Localization;
using NetworkAnarchy.Patches;
using QCommonLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Error code max: 67

namespace NetworkAnarchy
{
    public class ModInfo : LoadingExtensionBase, IUserMod 
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                if (GameSettings.FindSettingsFileByName(NetworkAnarchy.settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = NetworkAnarchy.settingsFileName } });
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load/create the setting file.");
                Debug.LogException(e);
            }
        }

        public string Name => "Network Anarchy " + QVersion.Version();
        public string Description => Str.mod_Description;

        internal static GameObject DebugGO;
        internal static DebugPanel s_debugPanel;

        //internal static QLogger Log;
        internal static QPatcher Patcher;
        internal string HarmonyId = "quboid.csl_mods.networkanarchy";

        internal static CultureInfo Culture => QCommon.GetCultureInfo();

        public void OnSettingsUI(UIHelperBase helper)
        {
            LocaleManager.eventLocaleChanged -= LocaleChanged;
            LocaleChanged();
            LocaleManager.eventLocaleChanged += LocaleChanged;

            try
            {
                var group = helper.AddGroup(Name) as UIHelper;
                var panel = group.self as UIPanel;

                var checkBox = (UICheckBox)group.AddCheckbox(Str.options_showLabels, UIToolOptionsButton.showLabels.value, (b) =>
                {
                    UIToolOptionsButton.showLabels.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                    }
                });
                checkBox.tooltip = Str.options_showLabelsTooltip;

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_showElevationStepSlider, UIToolOptionsButton.showElevationSlider.value, (b) =>
                {
                    UIToolOptionsButton.showElevationSlider.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                    }
                });
                checkBox.tooltip = Str.options_showElevationStepSliderTooltip;

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_showMaxSegmentLengthSlider, UIToolOptionsButton.showMaxSegmentLengthSlider.value, (b) =>
                {
                    UIToolOptionsButton.showMaxSegmentLengthSlider.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                    }
                });
                checkBox.tooltip = Str.options_showMaxSegmentLengthSliderTooltip;

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox(Str.options_reduceCatenaries, NetworkAnarchy.reduceCatenary.value, (b) =>
                 {
                     NetworkAnarchy.reduceCatenary.value = b;
                     if (NetworkAnarchy.instance != null)
                     {
                         UpdateCatenaries.Apply();
                     }
                 });
                checkBox.tooltip = Str.options_reduceCatenariesTooltip;

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox(Str.options_tramMaxTurnAngle, NetworkAnarchy.changeMaxTurnAngle.value, (b) =>
                 {
                     NetworkAnarchy.changeMaxTurnAngle.value = b;

                     if (b)
                     {
                         NetPrefab.SetMaxTurnAngle(NetworkAnarchy.maxTurnAngle);
                     }
                     else
                     {
                         NetPrefab.ResetMaxTurnAngle();
                     }
                 });
                checkBox.tooltip = Str.options_tramMaxTurnAngleTooltip;

                group.AddTextfield(Str.options_maxTurnAngle + ": ", NetworkAnarchy.maxTurnAngle.ToString(), (f) => { },
                    (s) =>
                    {
                        float.TryParse(s, out var f);

                        NetworkAnarchy.maxTurnAngle.value = Mathf.Clamp(f, 0f, 180f);

                        if (NetworkAnarchy.changeMaxTurnAngle.value)
                        {
                            NetPrefab.SetMaxTurnAngle(NetworkAnarchy.maxTurnAngle.value);
                        }
                    });

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                group.AddButton(Str.options_resetToolWindowPosition, () =>
                {
                    UIToolOptionsButton.savedWindowX.Delete();
                    UIToolOptionsButton.savedWindowY.Delete();

                    if (UIToolOptionsButton.toolOptionsPanel)
                    {
                        UIToolOptionsButton.toolOptionsPanel.absolutePosition = new Vector3(-1000, -1000);
                    }
                });

                group.AddSpace(10);

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_enableDebugLogging, NetworkAnarchy.showDebugMessages.value, (b) =>
                {
                    NetworkAnarchy.showDebugMessages.value = b;
#if !DEBUG
                    Log.IsDebug = b;
#endif
                });
                checkBox.tooltip = Str.options_enableDebugLoggingTooltip;
            }
            catch (Exception e)
            {
                Debug.Log("NetworkAnarchy OnSettingsUI failed [NA20]");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Executes at main menu
        /// </summary>
        public void OnEnabled()
        {
#if DEBUG
            Patcher = new QPatcher(HarmonyId, EarlyPatches.Deploy, EarlyPatches.Revert, true);
            Log.IsDebug = true;
#else
            //Log = new QLogger(true);// NetworkAnarchy.showDebugMessages); 
            Patcher = new QPatcher(HarmonyId, EarlyPatches.Deploy, EarlyPatches.Revert);
            Log.IsDebug = true; // Always log stuff for now
#endif

            AnyRoadOutsideConnection.Initialise();



            // Game loaded to main menu
            if (UIView.GetAView() == null)
            {
                LoadingManager.instance.m_introLoaded += CheckIncompatibleMods;
            }
            else
            {
                CheckIncompatibleMods();
            }

            // Hot reload
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                InitializeMod();
            }
        }

        public void OnDisabled()
        {
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                DestroyMod();
            }

            //Log = null;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            InitializeMod();
        }

        public override void OnLevelUnloading()
        {
            DestroyMod();
        }

        public void InitializeMod()
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

            Patcher.PatchAll();
            if (QCommon.Scene != QCommon.SceneTypes.AssetEditor)
            {
                Mods.CrossTheLine.Initialise(Patcher);
            }
            else
            {
                GameAreaManager.instance.m_maxAreaCount = GameAreaManager.AREAGRID_RESOLUTION * GameAreaManager.AREAGRID_RESOLUTION;
                for (int i = 0; i < GameAreaManager.instance.m_maxAreaCount; i++)
                {
                    GameAreaManager.instance.m_areaGrid[i] = i + 1;
                }
                GameAreaManager.instance.m_areaCount = GameAreaManager.instance.m_maxAreaCount;
            }

            //DebugGO = new GameObject("MIT_DebugPanel");
            //DebugGO.AddComponent<DebugPanel>();
            //s_debugPanel = DebugGO.GetComponent<DebugPanel>();
        }

        public void DestroyMod()
        {
            Mods.CrossTheLine.Deactivate();
            Patcher.UnpatchAll();
            NetworkAnarchy.instance.RestoreDefaultKeys();

            if (s_debugPanel != null)
            {
                GameObject.Destroy(s_debugPanel);
                s_debugPanel = null;
            }

            if (NetworkAnarchy.instance != null)
            {
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton.m_toolOptionsPanel);
                NetworkAnarchy.m_toolOptionButton.m_toolOptionsPanel = null;
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton);
                NetworkAnarchy.m_toolOptionButton = null;
                NetworkAnarchy.instance.enabled = false;
                GameObject.Destroy(NetworkAnarchy.instance);
                NetworkAnarchy.instance = null;
            }

            LocaleManager.eventLocaleChanged -= LocaleChanged;
        }

        public void CheckIncompatibleMods()
        {
            Dictionary<ulong, string> incompatbleMods = new Dictionary<ulong, string>
            {
                { 1844442251,   "Fine Road Tool" },
                { 1844440354,   "Fine Road Anarchy" },
                { 2847163882,   "Any Road Outside Connections Revisited" },
                { 883332136,    "Any Road Outside Connections" },
                { 2558311605,   "Left-Hand Network Fix" },
                { 1274199764,   "Network Tiling" },
                { 2085018096,   "Node Spacer" },
                { 650436109,    "Quay Anarchy" },
                { 707759735,    "Ship Path Anarchy" },
                { 1586027591,   "Tiny Segments - Extra Anarchy" },
            };

            QIncompatible checker = new QIncompatible(incompatbleMods, Log.instance);
        }

        internal static void LocaleChanged()
        {
            if (NetworkAnarchy.instance != null)
            {
                if (NetworkAnarchy.m_toolOptionButton != null)
                {
                    NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                }
            }

            Log.Info($"Network Anarchy Locale changed {Str.Culture?.Name}->{Culture.Name}", "[NA48]");
            Str.Culture = Culture;
        }

        internal static string GetString(object o)
        {
            if (o == null) return "<null>";
            return o.ToString();
        }
    }

    internal class DebugPanel : MonoBehaviour
    {
        internal UIPanel m_panel;
        internal UILabel m_label;

        internal DebugPanel()
        {
            m_panel = UIView.GetAView().AddUIComponent(typeof(UIPanel)) as UIPanel;
            m_panel.name = "NetworkAnarchy_DebugPanel";
            m_panel.atlas = QTextures.GetAtlas("Ingame");
            m_panel.backgroundSprite = "SubcategoriesPanel";
            m_panel.size = new Vector2(400, 200);
            m_panel.absolutePosition = new Vector3(400, 50);
            m_panel.clipChildren = true;
            m_panel.isVisible = true;

            m_label = m_panel.AddUIComponent<UILabel>();
            m_label.text = "Debug";
            m_label.relativePosition = new Vector3(5, 5);
            m_label.size = m_panel.size - new Vector2(10, 10);
        }

        internal void Text(string text)
        {
            Singleton<SimulationManager>.instance.m_ThreadingWrapper.QueueMainThread(() => {
                if (m_label != null)
                {
                    m_label.text = text;
                }
            });
        }
    }

    public class Log : QLoggerStatic { }
}

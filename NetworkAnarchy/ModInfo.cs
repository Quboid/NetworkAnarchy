using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using NetworkAnarchy.Localization;
using NetworkAnarchy.Patches;
using QCommonLib;
using System;
using System.Globalization;
using UnityEngine;

// Error code max: 65

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

        public void OnEnabled()
        {
#if DEBUG
            //Log = new QLogger(true);
            Patcher = new QPatcher(HarmonyId, EarlyPatches.Deploy, EarlyPatches.Revert, true);
#else
            //Log = new QLogger(true);// NetworkAnarchy.showDebugMessages); // Always log stuff while mod is in beta
            Patcher = new QPatcher(HarmonyId, EarlyPatches.Deploy, EarlyPatches.Revert);
            Log.IsDebug = true;
#endif

            AnyRoadOutsideConnection.Initialise();
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
            //if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            //{
            //    return;
            //}

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
            Mods.CrossTheLine.Initialise(Patcher);
        }

        public void DestroyMod()
        {
            Mods.CrossTheLine.Deactivate();
            Patcher.UnpatchAll();
            NetworkAnarchy.instance.RestoreDefaultKeys();

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

    public class Log : QLoggerStatic { }

}

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using NetworkAnarchy.Localization;
using System;
using System.Globalization;
using UnityEngine;
using System.Linq;
using QCommonLib;

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
                DebugUtils.Log("Couldn't load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public string Name => "Network Anarchy " + Version;
        public string Description => Str.mod_Description;

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
                         NetworkAnarchy.instance.UpdateCatenary();
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

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_disableDebugLogging, DebugUtils.hideDebugMessages.value, (b) =>
                {
                    DebugUtils.hideDebugMessages.value = b;
                });
                checkBox.tooltip = Str.options_disableDebugLoggingTooltip;
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public static string MinorVersion => MajorVersion + "." + typeof(ModInfo).Assembly.GetName().Version.Build;
        public static string MajorVersion => typeof(ModInfo).Assembly.GetName().Version.Major + "." + typeof(ModInfo).Assembly.GetName().Version.Minor;
        public static string FullVersion => MinorVersion + " r" + typeof(ModInfo).Assembly.GetName().Version.Revision;

        public static string Version
        {
            get
            {
                if (typeof(ModInfo).Assembly.GetName().Version.Minor == 0 && typeof(ModInfo).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(ModInfo).Assembly.GetName().Version.Major.ToString() + ".0";
                }
                if (typeof(ModInfo).Assembly.GetName().Version.Build > 0)
                {
                    return MinorVersion;
                }
                else
                {
                    return MajorVersion;
                }
            }
        }

        public void OnEnabled()
        {
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

            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void DestroyMod()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.UnpatchAll());

            if (NetworkAnarchy.instance != null)
            {
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton.m_toolOptionsPanel);
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton);
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

            Debug.Log($"Network Anarchy Locale changed {Str.Culture?.Name}->{ModInfo.Culture.Name}");
            Str.Culture = ModInfo.Culture;
        }
    }

    public static class Patcher
    {
        private const string HarmonyId = "quboid.csl_mods.networkanarchy";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            patched = true;
            var harmony = new Harmony(HarmonyId);
#if DEBUG
            //Harmony.DEBUG = true;
#endif
            harmony.PatchAll();
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }
    }
}

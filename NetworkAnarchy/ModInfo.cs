using ColossalFramework;
using ColossalFramework.UI;
using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

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

        public string Description => "More tool options for roads and other networks";

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                var group = helper.AddGroup(Name) as UIHelper;
                var panel = group.self as UIPanel;

                var checkBox = (UICheckBox)group.AddCheckbox("Show elevation step slider", UIToolOptionsButton.showElevationSlider.value, (b) =>
                {
                    UIToolOptionsButton.showElevationSlider.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                    }
                });
                checkBox.tooltip = "Show slider for changing the elevation step, from 1m to 12m.\n";

                checkBox = (UICheckBox)group.AddCheckbox("Show max segment length slider", UIToolOptionsButton.showMaxSegmentLengthSlider.value, (b) =>
                {
                    UIToolOptionsButton.showMaxSegmentLengthSlider.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        NetworkAnarchy.m_toolOptionButton.CreateOptionPanel(true);
                    }
                });
                checkBox.tooltip = "Show slider for changing the maximum segment length, from 4m to 256m (default is 96m).\n";

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox("Reduce rail catenary masts", NetworkAnarchy.reduceCatenary.value, (b) =>
                 {
                     NetworkAnarchy.reduceCatenary.value = b;
                     if (NetworkAnarchy.instance != null)
                     {
                         NetworkAnarchy.instance.UpdateCatenary();
                     }
                 });
                checkBox.tooltip = "Reduce the number of catenary mast of rail lines from 3 to 1 per segment.\n";

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox("Change max turn angle for more realistic tram tracks turns", NetworkAnarchy.changeMaxTurnAngle.value, (b) =>
                 {
                     NetworkAnarchy.changeMaxTurnAngle.value = b;

                     if (b)
                     {
                         RoadPrefab.SetMaxTurnAngle(NetworkAnarchy.maxTurnAngle);
                     }
                     else
                     {
                         RoadPrefab.ResetMaxTurnAngle();
                     }
                 });
                checkBox.tooltip = "Change all roads with tram tracks max turn angle by the value below if current value is higher";

                group.AddTextfield("Max turn angle: ", NetworkAnarchy.maxTurnAngle.ToString(), (f) => { },
                    (s) =>
                    {
                        float.TryParse(s, out var f);

                        NetworkAnarchy.maxTurnAngle.value = Mathf.Clamp(f, 0f, 180f);

                        if (NetworkAnarchy.changeMaxTurnAngle.value)
                        {
                            RoadPrefab.SetMaxTurnAngle(NetworkAnarchy.maxTurnAngle.value);
                        }
                    });

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                group.AddButton("Reset tool window position", () =>
                {
                    UIToolOptionsButton.savedWindowX.Delete();
                    UIToolOptionsButton.savedWindowY.Delete();

                    if (UIToolOptionsButton.toolOptionsPanel)
                    {
                        UIToolOptionsButton.toolOptionsPanel.absolutePosition = new Vector3(-1000, -1000);
                    }
                });

                group.AddSpace(10);

                checkBox = (UICheckBox)group.AddCheckbox("Disable debug messages logging", DebugUtils.hideDebugMessages.value, (b) =>
                {
                    DebugUtils.hideDebugMessages.value = b;
                });
                checkBox.tooltip = "If checked, debug messages won't be logged.";
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
            get {
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
            Debug.Log($"OnEnabled");
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                InitializeMod();
            }
        }

        public void OnDisabled()
        {
            Debug.Log($"OnDisabled");
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                DestroyMod();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Debug.Log($"OnLevelLoaded");
            //if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            //{
            //    return;
            //}

            InitializeMod();
        }

        public override void OnLevelUnloading()
        {
            Debug.Log($"OnLevelUnloading");
            DestroyMod();
        }

        public void InitializeMod()
        {
            Debug.Log($"InitializeMod");
            if (NetworkAnarchy.instance == null)
            {
                Debug.Log($"InitializeMod IsNull");
                // Creating the instance
                NetworkAnarchy.instance = new GameObject("NetworkAnarchy").AddComponent<NetworkAnarchy>();

                // Don't destroy it
                GameObject.DontDestroyOnLoad(NetworkAnarchy.instance);
            }
            else
            {
                Debug.Log($"InitializeMod NotNull");
                NetworkAnarchy.instance.Start();
                NetworkAnarchy.instance.enabled = true;
            }

            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void DestroyMod()
        {
            Debug.Log($"DestroyMod");
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.UnpatchAll());

            if (NetworkAnarchy.instance != null)
            {
                Debug.Log($"DestroyMod NotNull");
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton.m_toolOptionsPanel);
                GameObject.Destroy(NetworkAnarchy.m_toolOptionButton);
                NetworkAnarchy.instance.enabled = false;
                GameObject.Destroy(NetworkAnarchy.instance);
                NetworkAnarchy.instance = null;
            }
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
            Harmony.DEBUG = true;
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

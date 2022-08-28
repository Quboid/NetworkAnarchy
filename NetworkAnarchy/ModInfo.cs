using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy
{
    public class ModInfo : IUserMod
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

                var checkBox = (UICheckBox) group.AddCheckbox("Disable debug messages logging", DebugUtils.hideDebugMessages.value, (b) =>
                 {
                     DebugUtils.hideDebugMessages.value = b;
                 });
                checkBox.tooltip = "If checked, debug messages won't be logged.";

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
                    return typeof(ModInfo).Assembly.GetName().Version.Major.ToString();
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
    }
}

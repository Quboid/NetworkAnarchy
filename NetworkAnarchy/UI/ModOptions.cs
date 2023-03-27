using ColossalFramework.UI;
using ICities;
using NetworkAnarchy.Lang;
using System;
using UnityEngine;

namespace NetworkAnarchy.UI
{
    public class ModOptions
    {
        public ModOptions(UIHelperBase helper, string name)
        {
            try
            {
                var group = helper.AddGroup(name) as UIHelper;
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

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_reduceCatenaries, NetworkAnarchy.reduceCatenary.value, (b) =>
                {
                    NetworkAnarchy.reduceCatenary.value = b;
                    if (NetworkAnarchy.instance != null)
                    {
                        UpdateCatenaries.Apply();
                    }
                });
                checkBox.tooltip = Str.options_reduceCatenariesTooltip;

                group.AddSpace(10);

                checkBox = (UICheckBox)group.AddCheckbox(Str.options_tramMaxTurnAngle, NetworkAnarchy.changeMaxTurnAngle.value, (b) =>
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
    }
}

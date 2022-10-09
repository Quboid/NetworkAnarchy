﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        //KeyCode wasKeyCode = KeyCode.None;
        //bool wasControl = false, wasAlt = false, wasShift = false;

        public void OnGUI()
        {
            try
            {
                Event e = Event.current;

                //if (!DebugUtils.hideDebugMessages && e.keyCode != KeyCode.None && (e.keyCode != wasKeyCode || e.control != wasControl || e.alt != wasAlt || e.shift != wasShift))
                //{
                //    if (e.control || e.alt || e.shift || !(e.keyCode == KeyCode.W || e.keyCode == KeyCode.A || e.keyCode == KeyCode.S || e.keyCode == KeyCode.D))
                //    {
                //        Debug.Log($"Key: {e.keyCode} (c:{e.control}, a:{e.alt}, s:{e.shift})");
                //        wasKeyCode = e.keyCode;
                //        wasControl = e.control;
                //        wasAlt = e.alt;
                //        wasShift = e.shift;
                //    }
                //}

                //Debug.Log($"AAA {e.keyCode} Ctrl:{e.control}, Alt:{e.alt}, Shift:{e.shift}\n" +
                //    $"Ctrl:{OptionsKeymapping.toggleAnarchy.Control}, key:{OptionsKeymapping.toggleAnarchy.Key}, up:{OptionsKeymapping.toggleAnarchy.IsKeyUp()}\n" +
                //    $"pressed:{OptionsKeymapping.toggleAnarchy.IsPressed()}, pressedE:{OptionsKeymapping.toggleAnarchy.IsPressed(e)}, pressedEv:{OptionsKeymapping.toggleAnarchy.IsPressed(Event.current)}");

                // Allow Anarchy and Collision shortcuts even if the panel isn't visible
                if (!UIView.HasModalInput() && OptionsKeymapping.toggleAnarchy.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleAnarchy (was:{Anarchy})");
                    ToggleAnarchy();
                }
                else if (!UIView.HasModalInput() && OptionsKeymapping.toggleCollision.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleCollision (was:{Collision})");
                    ToggleCollision();
                }

                if (m_buildingTool.enabled && NetPrefab.SingleMode)
                {
                    // Checking key presses
                    if (OptionsKeymapping.elevationUp.IsPressed(e) || OptionsKeymapping.elevationDown.IsPressed(e))
                    {
                        NetPrefab.SingleMode = false;
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

                            int elevation = (int)m_buildingElevationField.GetValue(m_buildingTool);
                            elevation += OptionsKeymapping.elevationUp.IsPressed(Event.current) ? 1 : -1;

                            m_buildingElevationField.SetValue(m_buildingTool, Mathf.Clamp(elevation, min, max));
                        }
                        e.Use();
                    }
                    return;
                }
                else if (m_buildingTool.enabled && OptionsKeymapping.elevationReset.IsPressed())
                {
                    m_buildingElevationField.SetValue(m_buildingTool, 0);
                }

                if (!IsActive)
                {
                    return;
                }

                // Updating the elevation
                if (m_elevation >= 0 || m_elevation <= -256)
                {
                    int currentElevation = (int)m_elevationField.GetValue(m_netTool);
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
                    DebugUtils.Log($"Hotkey: elevationUp (was:{m_elevation})");
                    m_elevation += Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                }
                else if (OptionsKeymapping.elevationDown.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: elevationDown (was:{m_elevation})");
                    m_elevation -= Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                }
                else if (OptionsKeymapping.elevationStepUp.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: elevationStepUp (was:{elevationStep})");
                    if (elevationStep < 12)
                    {
                        elevationStep++;
                        m_toolOptionButton.UpdateInfo();
                    }
                }
                else if (OptionsKeymapping.elevationStepDown.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: elevationStepDown (was:{elevationStep})");
                    if (elevationStep > 1)
                    {
                        elevationStep--;
                        m_toolOptionButton.UpdateInfo();
                    }
                }
                else if (OptionsKeymapping.modesCycleRight.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: modesCycleRight (was:{m_mode})");
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
                    DebugUtils.Log($"Hotkey: modesCycleLeft (was:{m_mode})");
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
                    DebugUtils.Log($"Hotkey: elevationReset (was:{m_elevation})");
                    m_elevation = 0;
                    UpdateElevation();
                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.toggleStraightSlope.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleStraightSlope (was:{StraightSlope})");
                    StraightSlope = !StraightSlope;
                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.toggleBending.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleBending (was:{Bending})");
                    ToggleBending();
                }
                else if (OptionsKeymapping.toggleSnapping.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleSnapping (was:{NodeSnapping})");
                    ToggleSnapping();
                }
                else if (OptionsKeymapping.toggleGrid.IsPressed(e))
                {
                    DebugUtils.Log($"Hotkey: toggleGrid (was:{Grid})");
                    ToggleGrid();
                }

                if (m_mode == Mode.Tunnel && InfoManager.instance.CurrentMode != InfoManager.InfoMode.Traffic)
                {
                    if (m_infoMode == (InfoManager.InfoMode)(-1))
                    {
                        m_infoMode = InfoManager.instance.CurrentMode;
                    }

                    InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Traffic, InfoManager.SubInfoMode.Default);
                }
                else if (m_mode != Mode.Tunnel && m_infoMode != (InfoManager.InfoMode)(-1))
                {
                    InfoManager.instance.SetCurrentMode(m_infoMode, InfoManager.SubInfoMode.Default);
                    m_infoMode = (InfoManager.InfoMode)(-1);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        private void UpdateAnarchyButton(UICheckBox checkBox, string name, bool enabled)
        {
            UIButton button = checkBox.Find<UIButton>("NA_" + name);
            if (button == null)
            {
                DebugUtils.Log($"Failed to find anarchy button \"{name}\"!");
                return;
            }
            if (enabled)
            {
                button.normalBgSprite = "OptionBaseFocused";
                button.normalFgSprite = name + "Focused";
            }
            else
            {
                button.normalBgSprite = "OptionBase";
                button.normalFgSprite = name;
            }
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

        private void AttachToolOptionsButton(NetPrefab prefab)
        {
            DoesVanillaElevationButtonExist = false;

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
                    IsButtonInOptionsBar = false;
                    button.tooltip = null;
                    DoesVanillaElevationButtonExist = true;

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
            IsButtonInOptionsBar = true;
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }
    }
}

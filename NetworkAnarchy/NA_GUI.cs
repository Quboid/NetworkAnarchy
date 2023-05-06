using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class NetworkAnarchy : MonoBehaviour
    {
        public void OnGUI()
        {
            try
            {
                Event e = Event.current;

                //if (DebugUtils.showDebugMessages && e.keyCode != KeyCode.None && (e.keyCode != wasKeyCode || e.control != wasControl || e.alt != wasAlt || e.shift != wasShift))
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

                //Debug.Log($"{e.keyCode} Ctrl:{e.control}, Alt:{e.alt}, Shift:{e.shift}\n" +
                //    $"Ctrl:{OptionsKeymapping.toggleAnarchy.Control}, key:{OptionsKeymapping.toggleAnarchy.Key}, up:{OptionsKeymapping.toggleAnarchy.IsKeyUp()}\n" +
                //    $"pressed:{OptionsKeymapping.toggleAnarchy.IsPressed()}, pressedE:{OptionsKeymapping.toggleAnarchy.IsPressed(e)}, pressedEv:{OptionsKeymapping.toggleAnarchy.IsPressed(e)}");

                // Allow Anarchy and Collision shortcuts even if the panel isn't visible (not handled by UUI)
                if (OptionsKeymapping.toggleAnarchy.IsPressed(e)) {
                    var was = Anarchy;
                    ToggleAnarchy();
                    Log.Debug($"Hotkey: toggleAnarchy {Anarchy} (was:{was})");
                    e.Use();
                }
                else if (OptionsKeymapping.toggleCollision.IsPressed(e))
                {
                    var was = Collision;
                    ToggleCollision();
                    Log.Debug($"Hotkey: toggleCollision (was:{Collision})");
                    e.Use();
                }

                if (IsBuildingIntersection())
                {
                    // Checking key presses
                    if (OptionsKeymapping.elevationUp.IsPressed(e) || OptionsKeymapping.elevationDown.IsPressed(e))
                    {
                        int delta = OptionsKeymapping.elevationUp.IsPressed(e) ? 1 : -1;
                        NetPrefab.SingleMode = false;
                        BuildingInfo info = m_buildingTool.m_prefab;

                        // Reset cached value
                        FieldInfo cachedMaxElevation = info.m_buildingAI.GetType().GetField("m_cachedMaxElevation", BindingFlags.NonPublic | BindingFlags.Instance);
                        cachedMaxElevation?.SetValue(info.m_buildingAI, -1);

                        info.m_buildingAI.GetElevationLimits(out int min, out int max);
                        if (Anarchy)
                        {
                            min = -999;
                            max = 999;
                        }

                        int elevation = (int)m_buildingElevationField.GetValue(m_buildingTool);
                        elevation += delta;

                        m_buildingElevationField.SetValue(m_buildingTool, Mathf.Clamp(elevation, min, max));
                        e.Use();
                        Log.Debug($"Intersection El: {(delta == 1 ? "Up" : "Down")} {elevation - delta}->{elevation} ({m_buildingElevationField.GetValue(m_buildingTool)})");
                    }
                    return;
                }
                else if (IsBuildingToolEnabled() && OptionsKeymapping.elevationReset.IsPressed())
                {
                    m_buildingElevationField.SetValue(m_buildingTool, 0);
                    e.Use();
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
                        m_toolOptionButton.UpdateButton();
                    }
                }
                else
                {
                    UpdateElevation();
                }

                // Checking key presses
                if (OptionsKeymapping.elevationUp.IsPressed(e)) {
                    var was = m_elevation;
                    m_elevation += Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                    //Log.Debug($"Hotkey: elevationUp {m_elevation} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.elevationDown.IsPressed(e)) {
                    var was = m_elevation;
                    m_elevation -= Mathf.RoundToInt(256f * elevationStep / 12f);
                    UpdateElevation();
                    //Log.Debug($"Hotkey: elevationDown {m_elevation} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.elevationStepUp.IsPressed(e)) {
                    var was = elevationStep;
                    if (elevationStep < 12) {
                        elevationStep++;
                        m_toolOptionButton.UpdateButton();
                    }
                    //Log.Debug($"Hotkey: elevationStepUp {elevationStep} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.elevationStepDown.IsPressed(e)) {
                    var was = elevationStep;
                    if (elevationStep > 1) {
                        elevationStep--;
                        m_toolOptionButton.UpdateButton();
                    }
                    //Log.Debug($"Hotkey: elevationStepDown {elevationStep} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.modesCycleRight.IsPressed(e)) {
                    var was = m_mode;
                    if (m_mode < Modes.Tunnel) {
                        mode++;
                    } else {
                        mode = Modes.Normal;
                    }

                    m_toolOptionButton.UpdateButton();
                    //Log.Debug($"Hotkey: modesCycleRight {m_mode} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.modesCycleLeft.IsPressed(e)) {
                    var was = m_mode;
                    if (m_mode > Modes.Normal) {
                        mode--;
                    } else {
                        mode = Modes.Tunnel;
                    }

                    m_toolOptionButton.UpdateButton();
                    //Log.Debug($"Hotkey: modesCycleLeft {m_mode} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.elevationReset.IsPressed(e)) {
                    var was = m_elevation;
                    m_elevation = 0;
                    UpdateElevation();
                    m_toolOptionButton.UpdateButton();
                    //Log.Debug($"Hotkey: elevationReset {m_elevation} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.toggleBending.IsPressed(e)) {
                    var was = Bending;
                    ToggleBending();
                    //Log.Debug($"Hotkey: toggleBending {Bending} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.toggleSnapping.IsPressed(e)) {
                    var was = NodeSnapping;
                    ToggleSnapping();
                    //Log.Debug($"Hotkey: toggleSnapping {NodeSnapping} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.toggleStraightSlope.IsPressed(e)) {
                    var was = StraightSlope;
                    ToggleStraightSlope();
                    //Log.Debug($"Hotkey: toggleStraightSlope {StraightSlope} (was:{was})");
                    e.Use();
                } else if (OptionsKeymapping.toggleGrid.IsPressed(e)) {
                    var was = Grid;
                    ToggleGrid();
                    //Log.Debug($"Hotkey: toggleGrid {Grid} (was:{was})");
                    e.Use();
                }

                if (m_mode == Modes.Tunnel && InfoManager.instance.CurrentMode != InfoManager.InfoMode.Traffic)
                {
                    if (m_infoMode == (InfoManager.InfoMode)(-1))
                    {
                        m_infoMode = InfoManager.instance.CurrentMode;
                    }

                    InfoManager.instance.SetCurrentMode(InfoManager.InfoMode.Traffic, InfoManager.SubInfoMode.Default);
                }
                else if (m_mode != Modes.Tunnel && m_infoMode != (InfoManager.InfoMode)(-1))
                {
                    InfoManager.instance.SetCurrentMode(m_infoMode, InfoManager.SubInfoMode.Default);
                    m_infoMode = (InfoManager.InfoMode)(-1);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "[NA33]");
            }
        }

        private void UpdateAnarchyButton(UICheckBox checkBox, string name, bool enabled)
        {
            UIButton button = checkBox.Find<UIButton>("NA_" + name);
            if (button == null)
            {
                Log.Warning($"Failed to find anarchy button \"{name}\"!", "[NA32]");
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
                UIPanel optionBar = UIView.Find<UIPanel>("OptionsBar");

                if (optionBar == null)
                {
                    Log.Warning("OptionBar not found!", "[NA31.1]");
                    return;
                }

                optionBar.autoLayout = true;

                // Save existing optionsBar components, clear list
                IList<UIComponent> components = new List<UIComponent>();
                foreach (UIComponent comp in optionBar.components)
                {
                    components.Add(comp);
                }
                optionBar.components.Clear();

                // Add NA button to empty list
                m_toolOptionButton = optionBar.AddUIComponent(typeof(UIToolOptionsButton)) as UIToolOptionsButton;

                // Re-add components after NA button
                foreach (UIComponent comp in components)
                {
                    optionBar.components.Add(comp);
                }

                if (m_toolOptionButton == null)
                {
                    Log.Warning("Couldn't create label", "[NA31.2]");
                    return;
                }

                // Configure NA button
                m_toolOptionButton.autoSize = false;
                m_toolOptionButton.size = new Vector2(36, 36);
                m_toolOptionButton.position = Vector2.zero;
                m_toolOptionButton.isVisible = false;

                // Iterate networks' option panels
                RoadsOptionPanel[] panels = optionBar.GetComponentsInChildren<RoadsOptionPanel>();
                foreach (RoadsOptionPanel panel in panels)
                {
                    // Remove vanilla elevation button
                    ((UIPanel)panel.component).autoLayout = true;
                    UIComponent button = panel.component.Find<UIComponent>("ElevationStep");
                    if (button == null)
                    {
                        continue;
                    }
                    button.size = Vector2.zero;
                    button.isVisible = false;
                    button.enabled = false;

                    //// Add Upgrade button if needed (e.g. powerlines)
                    //var list = new List<NetTool.Mode>(panel.m_Modes);
                    //if (m_upgradeButtonTemplate != null && !list.Contains(NetTool.Mode.Upgrade))
                    //{
                    //    UITabstrip toolMode = panel.component.Find<UITabstrip>("ToolMode");
                    //    if (toolMode != null)
                    //    {
                    //        list.Add(NetTool.Mode.Upgrade);
                    //        panel.m_Modes = list.ToArray();

                    //        toolMode.AddTab("Upgrade", m_upgradeButtonTemplate, false);

                    //        Log.Debug($"Upgrade button added for {panel.name}.", "[NA17]");
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                enabled = false;
                Log.Error(e, "[NA32]");
            }
        }

        //private void CheckElevationStep()
        //{
        //    RoadsOptionPanel[] panels = GameObject.FindObjectsOfType<RoadsOptionPanel>();

        //    foreach (RoadsOptionPanel panel in panels)
        //    {
        //        // Find the visible RoadsOptionPanel
        //        if (panel.component.isVisible)
        //        {
        //            UIComponent button = panel.component.Find<UIComponent>("ElevationStep");
        //            if (button == null)
        //            {
        //                continue;
        //            }
        //            button.isVisible = false;
        //        }
        //    }
        //}

        //private void AttachToolOptionsButton(NetPrefab prefab)
        //{
        //    return;
        //    DoesVanillaElevationButtonExist = false;

        //    RoadsOptionPanel[] panels = GameObject.FindObjectsOfType<RoadsOptionPanel>();

        //    foreach (RoadsOptionPanel panel in panels)
        //    {
        //        // Find the visible RoadsOptionPanel
        //        if (panel.component.isVisible)
        //        {
        //            UIComponent button = panel.component.Find<UIComponent>("ElevationStep");
        //            if (button == null)
        //            {
        //                continue;
        //            }

        //            button.isVisible = false;

        //            // Put the main button in ElevationStep
        //            //m_toolOptionButton.transform.SetParent(button.transform);

        //            //IsButtonInOptionsBar = false;
        //            //button.tooltip = null;
        //            //DoesVanillaElevationButtonExist = true;

        //            //// Add Upgrade button if needed (e.g. powerlines)
        //            //var list = new List<NetTool.Mode>(panel.m_Modes);
        //            //if (!list.Contains(NetTool.Mode.Upgrade))
        //            //{
        //            //    Log.Debug($"Adding upgrade button for {ModInfo.GetString(prefab)}", "[NA51]");
        //            //}
        //            //if (m_upgradeButtonTemplate != null && prefab != null && prefab.HasVariation && !list.Contains(NetTool.Mode.Upgrade))
        //            //{
        //            //    UITabstrip toolMode = panel.component.Find<UITabstrip>("ToolMode");
        //            //    if (toolMode != null)
        //            //    {
        //            //        list.Add(NetTool.Mode.Upgrade);
        //            //        panel.m_Modes = list.ToArray();

        //            //        toolMode.AddTab("Upgrade", m_upgradeButtonTemplate, false);

        //            //        Log.Debug("Upgrade button added.", "[NA17]");
        //            //    }
        //            //}

        //            //Log.Debug($"ELEVATION STEP - Button placed on elevation button\n" +
        //            //    $"      button:{ModInfo.GetString(button)}\n" +
        //            //    $"      parent:{ModInfo.GetString(m_toolOptionButton.parent)}\n" +
        //            //    $" tfrm.parent:{ModInfo.GetString(m_toolOptionButton.transform.parent)})", "[NA49]");
        //            //return;
        //        }
        //    }

        //    // No visible RoadsOptionPanel found. Put the main button in OptionsBar instead
        //    //UIPanel optionBar = UIView.Find<UIPanel>("OptionsBar");

        //    //if (optionBar == null)
        //    //{
        //    //    Log.Warning("OptionBar not found!", "[NA18]");
        //    //    return;
        //    //}
        //    //m_toolOptionButton.transform.SetParent(optionBar.transform);
        //    //Log.Debug($"MAIN OPTIONS\nButton placed on main options bar ({m_toolOptionButton.parent})", "[NA50]");
        //    IsButtonInOptionsBar = true;
        //}

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

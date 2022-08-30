﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

                            int elevation = (int)m_buildingElevationField.GetValue(m_buildingTool);
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
                    StraightSlope = !StraightSlope;
                    m_toolOptionButton.UpdateInfo();
                }
                else if (OptionsKeymapping.toggleAnarchy.IsPressed(e))
                {
                    ToggleAnarchy();
                }
                else if (OptionsKeymapping.toggleBending.IsPressed(e))
                {
                    ToggleBending();
                }
                else if (OptionsKeymapping.toggleSnapping.IsPressed(e))
                {
                    ToggleSnapping();
                }
                else if (OptionsKeymapping.toggleCollision.IsPressed(e))
                {
                    ToggleCollision();
                }
                else if (OptionsKeymapping.toggleGrid.IsPressed(e))
                {
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

        public void ToggleAnarchy() => m_toolOptionButton.m_anarchyBtn.isChecked = !m_toolOptionButton.m_anarchyBtn.isChecked;
        public void ToggleBending() => m_toolOptionButton.m_bendingBtn.isChecked = !m_toolOptionButton.m_bendingBtn.isChecked;
        public void ToggleSnapping() => m_toolOptionButton.m_snappingBtn.isChecked = !m_toolOptionButton.m_snappingBtn.isChecked;
        public void ToggleCollision() => m_toolOptionButton.m_collisionBtn.isChecked = !m_toolOptionButton.m_collisionBtn.isChecked;
        public void ToggleGrid()
        {
            if (m_toolOptionButton.m_gridBtn != null)
            {
                m_toolOptionButton.m_gridBtn.isChecked = !m_toolOptionButton.m_gridBtn.isChecked;
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

        private void LoadChirperAtlas()
        {
            string[] spriteNames = new string[]
            {
                "Chirper",
                "ChirperChristmas",
                "ChirperChristmasDisabled",
                "ChirperChristmasFocused",
                "ChirperChristmasHovered",
                "ChirperChristmasPressed",
                "ChirperConcerts",
                "ChirperConcertsDisabled",
                "ChirperConcertsFocused",
                "ChirperConcertsHovered",
                "ChirperConcertsPressed",
                "Chirpercrown",
                "ChirpercrownDisabled",
                "ChirpercrownFocused",
                "ChirpercrownHovered",
                "ChirpercrownPressed",
                "ChirperDeluxe",
                "ChirperDeluxeDisabled",
                "ChirperDeluxeFocused",
                "ChirperDeluxeHovered",
                "ChirperDeluxePressed",
                "ChirperDisabled",
                "ChirperDisastersHazmat",
                "ChirperDisastersHazmatDisabled",
                "ChirperDisastersHazmatFocused",
                "ChirperDisastersHazmatHovered",
                "ChirperDisastersHazmatPressed",
                "ChirperDisastersPilot",
                "ChirperDisastersPilotDisabled",
                "ChirperDisastersPilotFocused",
                "ChirperDisastersPilotHovered",
                "ChirperDisastersPilotPressed",
                "ChirperDisastersWorker",
                "ChirperDisastersWorkerDisabled",
                "ChirperDisastersWorkerFocused",
                "ChirperDisastersWorkerHovered",
                "ChirperDisastersWorkerPressed",
                "ChirperFocused",
                "ChirperFootball",
                "ChirperFootballDisabled",
                "ChirperFootballFocused",
                "ChirperFootballHovered",
                "ChirperFootballPressed",
                "ChirperHovered",
                "ChirperIcon",
                "ChirperJesterhat",
                "ChirperJesterhatDisabled",
                "ChirperJesterhatFocused",
                "ChirperJesterhatHovered",
                "ChirperJesterhatPressed",
                "ChirperLumberjack",
                "ChirperLumberjackDisabled",
                "ChirperLumberjackFocused",
                "ChirperLumberjackHovered",
                "ChirperLumberjackPressed",
                "ChirperParkRanger",
                "ChirperParkRangerDisabled",
                "ChirperParkRangerFocused",
                "ChirperParkRangerHovered",
                "ChirperParkRangerPressed",
                "ChirperPressed",
                "ChirperRally",
                "ChirperRallyDisabled",
                "ChirperRallyFocused",
                "ChirperRallyHovered",
                "ChirperRallyPressed",
                "ChirperRudolph",
                "ChirperRudolphDisabled",
                "ChirperRudolphFocused",
                "ChirperRudolphHovered",
                "ChirperRudolphPressed",
                "ChirperSouvenirGlasses",
                "ChirperSouvenirGlassesDisabled",
                "ChirperSouvenirGlassesFocused",
                "ChirperSouvenirGlassesHovered",
                "ChirperSouvenirGlassesPressed",
                "ChirperSurvivingMars",
                "ChirperSurvivingMarsDisabled",
                "ChirperSurvivingMarsFocused",
                "ChirperSurvivingMarsHovered",
                "ChirperSurvivingMarsPressed",
                "ChirperTrafficCone",
                "ChirperTrafficConeDisabled",
                "ChirperTrafficConeFocused",
                "ChirperTrafficConeHovered",
                "ChirperTrafficConePressed",
                "ChirperTrainConductor",
                "ChirperTrainConductorDisabled",
                "ChirperTrainConductorFocused",
                "ChirperTrainConductorHovered",
                "ChirperTrainConductorPressed",
                "ChirperWintercap",
                "ChirperWintercapDisabled",
                "ChirperWintercapFocused",
                "ChirperWintercapHovered",
                "ChirperWintercapPressed",
                "ChirperZookeeper",
                "ChirperZookeeperDisabled",
                "ChirperZookeeperFocused",
                "ChirperZookeeperHovered",
                "ChirperZookeeperPressed",
                "EmptySprite",
                "ThumbChirperBeanie",
                "ThumbChirperBeanieDisabled",
                "ThumbChirperBeanieFocused",
                "ThumbChirperBeanieHovered",
                "ThumbChirperBeaniePressed",
                "ThumbChirperFlower",
                "ThumbChirperFlowerDisabled",
                "ThumbChirperFlowerFocused",
                "ThumbChirperFlowerHovered",
                "ThumbChirperFlowerPressed",
                "ThumbChirperTech",
                "ThumbChirperTechDisabled",
                "ThumbChirperTechFocused",
                "ThumbChirperTechHovered",
                "ThumbChirperTechPressed"
            };

            chirperAtlasAnarchy = ResourceLoader.CreateTextureAtlas("ChirperAtlasAnarchy", spriteNames, "NetworkAnarchy.ChirperAtlas.");
        }
    }
}
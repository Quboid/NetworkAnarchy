﻿using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Diagnostics;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class UIToolOptionsButton : UICheckBox
    {
        private UIButton m_button;
        internal UIPanel m_toolOptionsPanel;
        private UISlider m_elevationStepSlider;
        private UILabel m_elevationStepLabel;
        internal UILabel m_maxSegmentLengthLabel;
        private UISlider m_maxSegmentLengthSlider;
        private UIDragHandle dragHandle;

        private UICheckBox m_normalModeButton;
        private UICheckBox m_groundModeButton;
        private UICheckBox m_elevatedModeButton;
        private UICheckBox m_bridgeModeButton;
        private UICheckBox m_tunnelModeButton;

        public UICheckBox m_anarchyBtn;
        public UICheckBox m_bendingBtn;
        public UICheckBox m_snappingBtn;
        public UICheckBox m_collisionBtn;
        public UICheckBox m_straightSlopeBtn;
        
        public UICheckBox m_grid;

        private UITextureAtlas m_atlas;

        private UIComponent m_parent;

        public static readonly SavedInt savedWindowX = new SavedInt("windowX", NetworkAnarchy.settingsFileName, -1000, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", NetworkAnarchy.settingsFileName, -1000, true);

        public static readonly SavedBool windowVisible = new SavedBool("windowVisible", NetworkAnarchy.settingsFileName, true, true);
        public static readonly SavedBool showElevationSlider = new SavedBool("showElevationSlider", NetworkAnarchy.settingsFileName, true, true);
        public static readonly SavedBool showMaxSegmentLengthSlider = new SavedBool("showNodeSpacer", NetworkAnarchy.settingsFileName, false, true);
        public static readonly SavedBool showLabels = new SavedBool("showLabels", NetworkAnarchy.settingsFileName, true, true);
        public static readonly SavedBool alwaysShowPanel = new SavedBool("alwaysShowPanel", NetworkAnarchy.settingsFileName, false, true);

        public static UIPanel toolOptionsPanel = null;

        public override void Start()
        {
            LoadResources();

            CreateButton();
            CreateOptionPanel();

            isChecked = windowVisible;
        }

        public new void Update() 
        {
            //if (parent != null || m_parent != null) Log.Debug($"parent:{(parent == null ? "<null>" : parent.ToString())} m_parent:{(m_parent == null ? "<null>" : m_parent.ToString())}");
            if (parent != m_parent && parent != null)
            {
                string caller = new StackTrace()?.GetFrame(1)?.GetMethod()?.Name;
                Log.Debug($"Tool button parent changed: {ModInfo.GetString(parent)} (was: {ModInfo.GetString(m_parent)})\n  Called by:{caller}", "[NA39]");
                m_parent = parent;

                UpdateButton();
            }

            if (m_toolOptionsPanel != null)
            {
                SetPanelVisible(isVisible && isChecked);
            }
        }

        public void SetPanelVisible(bool visible)
        {
            m_toolOptionsPanel.isVisible = visible;

            //if (alwaysShowPanel)
            //{
            //    m_toolOptionsPanel.isVisible = true;
            //}
            //else
            //{
            //    m_toolOptionsPanel.isVisible = visible;
            //}
        }

        public void UpdateButton()
        {
            if (NetworkAnarchy.instance == null) return;

            if (parent == null)
            {
                string caller = new StackTrace()?.GetFrame(1)?.GetMethod()?.Name;
                Log.Info($"Button parent is null (m_parent is {ModInfo.GetString(m_parent)})\n  Called by:{caller}", "[NA38]");
                isVisible = false;
                return;
            }

            //if (NetworkAnarchy.instance.IsButtonInOptionsBar)
            //{
            //    relativePosition = new Vector2(36, 0);
            //}
            //else
            //{
            //    relativePosition = Vector2.zero;
            //    parent.BringToFront();
            //}

            m_button.text = NetworkAnarchy.instance.elevationStep + "m\n";
            if (m_elevationStepSlider != null)
            {
                m_elevationStepLabel.text = NetworkAnarchy.instance.elevationStep + "m\n";
                m_elevationStepSlider.value = NetworkAnarchy.instance.elevationStep;
            }
            UpdateSlider();

            m_button.normalFgSprite = NetworkAnarchy.instance.StraightSlope ? "ToolbarIconGroup1Hovered" : null;

            switch (NetworkAnarchy.instance.mode)
            {
                case Modes.Normal:
                    m_button.text += "Nrm\n";
                    m_normalModeButton.SimulateClick();
                    break;
                case Modes.Ground:
                    m_button.text += "Gnd\n";
                    m_groundModeButton.SimulateClick();
                    break;
                case Modes.Elevated:
                    m_button.text += "Elv\n";
                    m_elevatedModeButton.SimulateClick();
                    break;
                case Modes.Bridge:
                    m_button.text += "Bdg\n";
                    m_bridgeModeButton.SimulateClick();
                    break;
                case Modes.Tunnel:
                    m_button.text += "Tnl\n";
                    m_tunnelModeButton.SimulateClick();
                    break;
            }

            m_button.text += NetworkAnarchy.instance.elevation + "m";
        }

        private void UpdateSlider()
        {
            if (!showMaxSegmentLengthSlider || m_maxSegmentLengthSlider == null) return;

            NetworkAnarchy.saved_segmentLength.value = NetworkAnarchy.instance.MaxSegmentLength;
            try
            {
                m_maxSegmentLengthLabel.text = NetworkAnarchy.instance.MaxSegmentLength + "m";
                if (NetworkAnarchy.instance.MaxSegmentLength > 128)
                {
                    m_maxSegmentLengthLabel.textColor = new Color32(255, 191, 0, 255);
                }
                else
                {
                    m_maxSegmentLengthLabel.textColor = Color.white;
                }
                m_maxSegmentLengthLabel.tooltip = (Mathf.RoundToInt(NetworkAnarchy.instance.MaxSegmentLength / 8f * 100) / 100f).ToString() + "u";
            }
            catch (StackOverflowException e)
            {
                Log.Error(e);
            }
        }

        private void UpdateMode()
        {
            if (m_normalModeButton.isChecked)
                NetworkAnarchy.instance.mode = Modes.Normal;
            if (m_groundModeButton.isChecked)
                NetworkAnarchy.instance.mode = Modes.Ground;
            if (m_elevatedModeButton.isChecked)
                NetworkAnarchy.instance.mode = Modes.Elevated;
            if (m_bridgeModeButton.isChecked)
                NetworkAnarchy.instance.mode = Modes.Bridge;
            if (m_tunnelModeButton.isChecked)
                NetworkAnarchy.instance.mode = Modes.Tunnel;
        }

        delegate void ToggleAnarchyButton();
    }
}

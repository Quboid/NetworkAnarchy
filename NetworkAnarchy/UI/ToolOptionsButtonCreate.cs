using ColossalFramework;
using ColossalFramework.UI;
using NetworkAnarchy.Localization;
using QCommonLib;
using System;
using UnityEngine;

namespace NetworkAnarchy
{
    public partial class UIToolOptionsButton : UICheckBox
    {
        private void CreateButton()
        {
            m_button = AddUIComponent<UIButton>();
            m_button.atlas = QTextures.GetAtlas("Ingame");
            m_button.name = "NA_MainButton";
            m_button.size = new Vector2(36, 36);
            m_button.textScale = 0.7f;
            m_button.playAudioEvents = true;
            m_button.relativePosition = Vector2.zero;

            m_button.tooltip = "Network Anarchy " + QVersion.Version() + "\n\n" + Str.ui_clickForToolOptions;

            m_button.textColor = Color.white;
            m_button.textScale = 0.7f;
            m_button.dropShadowOffset = new Vector2(2, -2);
            m_button.useDropShadow = true;

            m_button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            m_button.wordWrap = true;

            m_button.normalBgSprite = "OptionBase";
            m_button.hoveredBgSprite = "OptionBaseHovered";
            m_button.pressedBgSprite = "OptionBasePressed";
            m_button.disabledBgSprite = "OptionBaseDisabled";

            m_button.eventClick += (c, p) => { base.OnClick(p); };

            eventCheckChanged += (c, isChecked) =>
            {
                if (isChecked)
                {
                    if (m_toolOptionsPanel.absolutePosition.x < 0)
                    {
                        m_toolOptionsPanel.absolutePosition = new Vector2(absolutePosition.x - ((m_toolOptionsPanel.width - m_button.width) / 2), absolutePosition.y - m_toolOptionsPanel.height);
                    }

                    Vector2 resolution = GetUIView().GetScreenResolution();

                    m_toolOptionsPanel.absolutePosition = new Vector2(
                        Mathf.Clamp(m_toolOptionsPanel.absolutePosition.x, 0, resolution.x - m_toolOptionsPanel.width),
                        Mathf.Clamp(m_toolOptionsPanel.absolutePosition.y, 0, resolution.y - m_toolOptionsPanel.height));

                    m_button.normalBgSprite = "OptionBaseFocused";

                    m_toolOptionsPanel.BringToFront();
                    windowVisible.value = true;
                }
                else
                {
                    m_button.normalBgSprite = "OptionBase";
                    windowVisible.value = false;
                }
                UpdateButton();
            };
        }

        protected override void OnClick(UIMouseEventParameter p) { }

        internal void CreateOptionPanel(bool destroyExisting = false)
        {
            if (destroyExisting && m_toolOptionsPanel != null)
            {
                GameObject.Destroy(m_toolOptionsPanel);
            }

            uint yPos = 8;

            m_toolOptionsPanel = UIView.GetAView().AddUIComponent(typeof(UIPanel)) as UIPanel;
            m_toolOptionsPanel.name = "NA_ToolOptionsPanel";
            m_toolOptionsPanel.atlas = QTextures.GetAtlas("Ingame");
            m_toolOptionsPanel.backgroundSprite = "SubcategoriesPanel";
            m_toolOptionsPanel.size = new Vector2(228, yPos);
            m_toolOptionsPanel.absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);
            m_toolOptionsPanel.clipChildren = true;

            m_toolOptionsPanel.isVisible = windowVisible;

            m_toolOptionsPanel.eventPositionChanged += (c, p) =>
            {
                if (m_toolOptionsPanel.absolutePosition.x < 0)
                    m_toolOptionsPanel.absolutePosition = new Vector2(absolutePosition.x - ((m_toolOptionsPanel.width - m_button.width) / 2), absolutePosition.y - m_toolOptionsPanel.height);

                Vector2 resolution = GetUIView().GetScreenResolution();

                m_toolOptionsPanel.absolutePosition = new Vector2(
                    Mathf.Clamp(m_toolOptionsPanel.absolutePosition.x, 0, resolution.x - m_toolOptionsPanel.width),
                    Mathf.Clamp(m_toolOptionsPanel.absolutePosition.y, 0, resolution.y - m_toolOptionsPanel.height));

                savedWindowX.value = (int)m_toolOptionsPanel.absolutePosition.x;
                savedWindowY.value = (int)m_toolOptionsPanel.absolutePosition.y;
            };

            toolOptionsPanel = m_toolOptionsPanel;

            dragHandle = m_toolOptionsPanel.AddUIComponent<UIDragHandle>();
            dragHandle.size = m_toolOptionsPanel.size;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = m_toolOptionsPanel;

            // Elevation step
            if (showElevationSlider)
            {
                if (showLabels)
                {
                    UILabel label = m_toolOptionsPanel.AddUIComponent<UILabel>();
                    label.textScale = 0.9f;
                    label.text = Str.ui_elevationStep;
                    label.relativePosition = new Vector2(8, yPos);
                    label.SendToBack();

                    ExpandY(ref yPos, 20u);
                }

                m_elevationStepSlider = CreateElevationSlider(m_toolOptionsPanel, yPos);

                ExpandY(ref yPos, 42u);
            }

            // Modes
            if (showLabels)
            {
                label = m_toolOptionsPanel.AddUIComponent<UILabel>();
                label.textScale = 0.9f;
                label.text = Str.ui_modes;
                label.relativePosition = new Vector2(8, yPos);
                label.SendToBack();
                ExpandY(ref yPos, 20u);
            }

            UIPanel modePanel = m_toolOptionsPanel.AddUIComponent<UIPanel>();
            modePanel.atlas = m_toolOptionsPanel.atlas;
            modePanel.backgroundSprite = "GenericPanel";
            modePanel.color = new Color32(206, 206, 206, 255);
            modePanel.size = new Vector2(m_toolOptionsPanel.width - 16, 52);
            modePanel.relativePosition = new Vector2(8, yPos);

            modePanel.padding = new RectOffset(8, 8, 8, 8);
            modePanel.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            modePanel.autoLayoutDirection = LayoutDirection.Horizontal;

            m_normalModeButton = CreateModeCheckBox(modePanel, "NormalMode", Str.ui_modeNormal);
            m_groundModeButton = CreateModeCheckBox(modePanel, "GroundMode", Str.ui_modeGround);
            m_elevatedModeButton = CreateModeCheckBox(modePanel, "ElevatedMode", Str.ui_modeElevated);
            m_bridgeModeButton = CreateModeCheckBox(modePanel, "BridgeMode", Str.ui_modeBridge);
            m_tunnelModeButton = CreateModeCheckBox(modePanel, "TunnelMode", Str.ui_modeTunnel);
            m_normalModeButton.isChecked = true;

            modePanel.autoLayout = true;
            ExpandY(ref yPos, 60u);

            // Anarchy buttons
            UIPanel anarchyPanel = m_toolOptionsPanel.AddUIComponent<UIPanel>();
            anarchyPanel.atlas = m_toolOptionsPanel.atlas;
            anarchyPanel.backgroundSprite = "GenericPanel";

            anarchyPanel.name = "NA_AnarchysPanel";
            anarchyPanel.color = new Color32(206, 206, 206, 255);
            anarchyPanel.size = new Vector2(m_toolOptionsPanel.width - 16, 52);
            anarchyPanel.relativePosition = new Vector2(8, yPos);

            anarchyPanel.padding = new RectOffset(8, 8, 8, 8);
            anarchyPanel.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            anarchyPanel.autoLayoutDirection = LayoutDirection.Horizontal;

            m_anarchyBtn = CreateAnarchyCheckBox(anarchyPanel, "Anarchy", Str.ui_toggleAnarchy, NetworkAnarchy.saved_anarchy, NetworkAnarchy.instance.ToggleAnarchy);
            m_bendingBtn = CreateAnarchyCheckBox(anarchyPanel, "Bending", Str.ui_toggleBending, NetworkAnarchy.saved_bending, NetworkAnarchy.instance.ToggleBending);
            m_snappingBtn = CreateAnarchyCheckBox(anarchyPanel, "Snapping", Str.ui_toggleSnapping, NetworkAnarchy.saved_nodeSnapping, NetworkAnarchy.instance.ToggleSnapping);
            m_zoneOverrideBtn = CreateAnarchyCheckBox(anarchyPanel, "ZoneOverride", Str.ui_toggleZoneOverride, NetworkAnarchy.ZoneOverride, NetworkAnarchy.instance.ToggleZoneOverride);
            m_straightSlopeBtn = CreateAnarchyCheckBox(anarchyPanel, "StraightSlope", Str.ui_toggleSlope, NetworkAnarchy.saved_smoothSlope, NetworkAnarchy.instance.ToggleStraightSlope);

            anarchyPanel.autoLayout = true;
            ExpandY(ref yPos, 58u);

            // Node spacing
            if (showMaxSegmentLengthSlider)
            {
                if (showLabels)
                {
                    UILabel label = m_toolOptionsPanel.AddUIComponent<UILabel>();
                    label.textScale = 0.825f;
                    label.text = Str.ui_maxSegmentLength;
                    label.relativePosition = new Vector2(8, yPos);
                    label.SendToBack();
                    ExpandY(ref yPos, 17u);
                }

                ExpandY(ref yPos, 2u);
                m_maxSegmentLengthSlider = CreateMaxSegmentLengthSliderPanel(m_toolOptionsPanel, yPos);
                ExpandY(ref yPos, 42u);
            }
            else
            {
                m_maxSegmentLengthSlider = CreateMaxSegmentLengthSlider(m_toolOptionsPanel, 0);
                m_maxSegmentLengthSlider.isVisible = false;
            }

            // Show Grid in Editor
            if (QCommon.Scene == QCommon.SceneTypes.AssetEditor)
            {
                ExpandY(ref yPos, 2u);
                m_grid = CreateCheckBox(m_toolOptionsPanel);
                m_grid.name = "NA_Grid";
                m_grid.label.text = Str.ui_grid;
                m_grid.isChecked = NetworkAnarchy.Grid;
                m_grid.relativePosition = new Vector3(8, yPos);
                m_grid.width = m_toolOptionsPanel.width - 16;

                m_grid.eventCheckChanged += (c, state) =>
                {
                    NetworkAnarchy.Grid = state;
                };
                ExpandY(ref yPos, 24u);
            }

            ExpandY(ref yPos, 2u);
        }

        private void ExpandY(ref uint yPos, uint increase)
        {
            m_toolOptionsPanel.height += increase;
            dragHandle.height += increase;
            yPos += increase;
        }

        private UISlider CreateElevationSlider(UIPanel parent, uint yPos)
        {
            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();
            sliderPanel.atlas = parent.atlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(parent.width - 16, 36);
            sliderPanel.relativePosition = new Vector2(8, yPos);

            m_elevationStepLabel = sliderPanel.AddUIComponent<UILabel>();
            m_elevationStepLabel.atlas = parent.atlas;
            m_elevationStepLabel.backgroundSprite = "TextFieldPanel";
            m_elevationStepLabel.verticalAlignment = UIVerticalAlignment.Bottom;
            m_elevationStepLabel.textAlignment = UIHorizontalAlignment.Center;
            m_elevationStepLabel.textScale = 0.7f;
            m_elevationStepLabel.text = "3m";
            m_elevationStepLabel.autoSize = false;
            m_elevationStepLabel.color = new Color32(91, 97, 106, 255);
            m_elevationStepLabel.size = new Vector2(38, 15);
            m_elevationStepLabel.relativePosition = new Vector2(sliderPanel.width - m_elevationStepLabel.width - 8, 10);

            UISlider slider = sliderPanel.AddUIComponent<UISlider>();
            slider.name = "NA_ElevationStepSlider";
            slider.size = new Vector2(sliderPanel.width - 20 - m_elevationStepLabel.width - 8, 18);
            slider.relativePosition = new Vector2(10, 10);
            slider.tooltip = String.Format(Str.ui_elevationSliderKeyTip, OptionsKeymapping.elevationStepUp.ToLocalizedString("KEYNAME"), OptionsKeymapping.elevationStepDown.ToLocalizedString("KEYNAME"));

            UISlicedSprite bgSlider = slider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = parent.atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(slider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);

            UISlicedSprite thumb = slider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = parent.atlas;
            thumb.spriteName = "SliderBudget";
            slider.thumbObject = thumb;

            slider.stepSize = 1;
            slider.minValue = 1;
            slider.maxValue = 12;
            slider.value = 3;

            slider.eventValueChanged += (c, v) =>
            {
                if (v != NetworkAnarchy.instance.elevationStep)
                {
                    NetworkAnarchy.instance.elevationStep = (int)v;
                    UpdateButton();
                }
            };

            return slider;
        }

        private UISlider CreateMaxSegmentLengthSliderPanel(UIPanel parent, uint yPos)
        {
            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();
            sliderPanel.atlas = parent.atlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(parent.width - 16, 36);
            sliderPanel.relativePosition = new Vector2(8, yPos);

            m_maxSegmentLengthLabel = sliderPanel.AddUIComponent<UILabel>();
            m_maxSegmentLengthLabel.atlas = parent.atlas;
            m_maxSegmentLengthLabel.backgroundSprite = "TextFieldPanel";
            m_maxSegmentLengthLabel.verticalAlignment = UIVerticalAlignment.Bottom;
            m_maxSegmentLengthLabel.textAlignment = UIHorizontalAlignment.Center;
            m_maxSegmentLengthLabel.textScale = 0.65f;
            m_maxSegmentLengthLabel.autoSize = false;
            m_maxSegmentLengthLabel.color = new Color32(91, 97, 106, 255);
            m_maxSegmentLengthLabel.size = new Vector2(38, 15);
            m_maxSegmentLengthLabel.relativePosition = new Vector2(sliderPanel.width - m_maxSegmentLengthLabel.width - 8, 10);
            try
            {
                m_maxSegmentLengthLabel.text = "96m";
            }
            catch (StackOverflowException e)
            {
                ModInfo.Log.Error(e);
            }

            UISlider slider = CreateMaxSegmentLengthSlider(sliderPanel, sliderPanel.width - 20 - m_maxSegmentLengthLabel.width - 8);

            UISlicedSprite bgSlider = slider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = parent.atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(slider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);

            UISlicedSprite thumb = slider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = parent.atlas;
            thumb.spriteName = "SliderBudget";
            slider.thumbObject = thumb;

            slider.eventValueChanged += (c, v) =>
            {
                if (v != NetworkAnarchy.instance.MaxSegmentLength)
                {
                    NetworkAnarchy.instance.MaxSegmentLength = (int)v;
                    UpdateSlider();
                }
            };

            return slider;
        }

        private UISlider CreateMaxSegmentLengthSlider(UIPanel parent, float width)
        {
            UISlider slider = parent.AddUIComponent<UISlider>();
            slider.name = "NA_MaxSegmentLengthSlider";
            slider.size = new Vector2(width, 18);
            slider.relativePosition = new Vector2(10, 10);

            slider.stepSize = NetworkAnarchy.SegmentLengthInterval;
            slider.minValue = NetworkAnarchy.SegmentLengthFloor;
            slider.maxValue = NetworkAnarchy.SegmentLengthCeiling;
            slider.value = NetworkAnarchy.instance.MaxSegmentLength;
            return slider;
        }

        private UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            checkBox.width = 300f;
            checkBox.height = 20f;
            checkBox.clipChildren = true;

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.atlas = m_toolOptionsPanel.atlas;
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).atlas = m_toolOptionsPanel.atlas;
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = " ";
            checkBox.label.textScale = 0.9f;
            checkBox.label.relativePosition = new Vector3(22f, 2f);

            return checkBox;
        }

        private UICheckBox CreateModeCheckBox(UIComponent parent, string spriteName, string toolTip)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);
            checkBox.group = parent;

            UIButton button = checkBox.AddUIComponent<UIButton>();
            button.name = "NA_" + spriteName;
            button.atlas = m_atlas;
            button.relativePosition = new Vector2(0, 0);

            button.tooltip = toolTip + "\n\n" + String.Format(Str.ui_modeCycleKeyTip, OptionsKeymapping.modesCycleLeft.ToLocalizedString("KEYNAME"), OptionsKeymapping.modesCycleRight.ToLocalizedString("KEYNAME"));

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            checkBox.eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    button.normalFgSprite = spriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    button.normalFgSprite = spriteName;
                }

                UpdateMode();
            };

            return checkBox;
        }

        private UICheckBox CreateAnarchyCheckBox(UIComponent parent, string spriteName, string toolTip, bool value, ToggleAnarchyButton method)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);

            UIButton button = checkBox.AddUIComponent<UIButton>();
            button.name = "NA_" + spriteName;
            button.atlas = m_atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            checkBox.isChecked = value;
            if (value)
            {
                button.normalBgSprite = "OptionBaseFocused";
                button.normalFgSprite = spriteName + "Focused";
            }

            checkBox.eventCheckChanged += (c, s) =>
            {
                method();
            };

            return checkBox;
        }

        private void LoadResources()
        {
            string[] spriteNames = new string[]
            {
                "NormalMode",
                "NormalModeDisabled",
                "NormalModeFocused",
                "NormalModeHovered",
                "NormalModePressed",
                "BridgeMode",
                "BridgeModeDisabled",
                "BridgeModeFocused",
                "BridgeModeHovered",
                "BridgeModePressed",
                "ElevatedMode",
                "ElevatedModeDisabled",
                "ElevatedModeFocused",
                "ElevatedModeHovered",
                "ElevatedModePressed",
                "GroundMode",
                "GroundModeDisabled",
                "GroundModeFocused",
                "GroundModeHovered",
                "GroundModePressed",
                "TunnelMode",
                "TunnelModeDisabled",
                "TunnelModeFocused",
                "TunnelModeHovered",
                "TunnelModePressed",
                "Anarchy",
                "AnarchyDisabled",
                "AnarchyFocused",
                "AnarchyHovered",
                "AnarchyPressed",
                "Bending",
                "BendingDisabled",
                "BendingFocused",
                "BendingHovered",
                "BendingPressed",
                "Snapping",
                "SnappingDisabled",
                "SnappingFocused",
                "SnappingHovered",
                "SnappingPressed",
                "ZoneOverride",
                "ZoneOverrideDisabled",
                "ZoneOverrideFocused",
                "ZoneOverrideHovered",
                "ZoneOverridePressed",
                "StraightSlope",
                "StraightSlopeDisabled",
                "StraightSlopeFocused",
                "StraightSlopeHovered",
                "StraightSlopePressed"
            };

            m_atlas = QTextures.CreateTextureAtlas("NetworkAnarchy", spriteNames, "NetworkAnarchy.Icons.");

            UITextureAtlas defaultAtlas = QTextures.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[]
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture
            };

            QTextures.AddTexturesInAtlas(m_atlas, textures);
        }
    }
}

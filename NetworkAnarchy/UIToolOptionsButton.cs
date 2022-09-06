using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace NetworkAnarchy
{
    public class UIToolOptionsButton : UICheckBox
    {
        private UIButton m_button;
        internal UIPanel m_toolOptionsPanel;
        private UISlider m_elevationStepSlider;
        private UILabel m_elevationStepLabel;
        internal UILabel m_maxSegmentLengthLabel;
        private UISlider m_maxSegmentLengthSlider;

        private UICheckBox m_normalModeButton;
        private UICheckBox m_groundModeButton;
        private UICheckBox m_elevatedModeButton;
        private UICheckBox m_bridgeModeButton;
        private UICheckBox m_tunnelModeButton;

        private UICheckBox m_straightSlope;

        public UICheckBox m_anarchyBtn;
        public UICheckBox m_bendingBtn;
        public UICheckBox m_snappingBtn;
        public UICheckBox m_collisionBtn;
        public UICheckBox m_gridBtn;

        private UITextureAtlas m_atlas;

        private UIComponent m_parent;

        public static readonly SavedInt savedWindowX = new SavedInt("windowX", NetworkAnarchy.settingsFileName, -1000, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", NetworkAnarchy.settingsFileName, -1000, true);

        public static readonly SavedBool windowVisible = new SavedBool("windowVisible", NetworkAnarchy.settingsFileName, false, true);
        public static readonly SavedBool showElevationSlider = new SavedBool("showElevationSlider", NetworkAnarchy.settingsFileName, true, true);
        public static readonly SavedBool showMaxSegmentLengthSlider = new SavedBool("showNodeSpacer", NetworkAnarchy.settingsFileName, false, true);

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
            if (parent != m_parent && parent != null)
            {
                m_parent = parent;

                UpdateInfo();
                DebugUtils.Log("Tool button parent changed: " + parent.name);
            }

            if (m_toolOptionsPanel != null)
            {
                m_toolOptionsPanel.isVisible = isVisible && isChecked;
            }
        }

        public void UpdateInfo()
        {
            if (NetworkAnarchy.instance == null)
                return;

            if (parent != null)
            {
                if (parent.name == "OptionsBar")
                {
                    relativePosition = new Vector2(36, 0);
                }
                else
                {
                    relativePosition = Vector2.zero;
                    parent.BringToFront();
                }
            }
            else
            {
                isVisible = false;
                return;
            }

            if (m_elevationStepSlider != null)
            {
                m_button.text = m_elevationStepLabel.text = NetworkAnarchy.instance.elevationStep + "m\n";
                m_elevationStepSlider.value = NetworkAnarchy.instance.elevationStep;
            }
            m_straightSlope.isChecked = NetworkAnarchy.instance.StraightSlope;
            UpdateSlider();

            m_button.normalFgSprite = NetworkAnarchy.instance.StraightSlope ? "ToolbarIconGroup1Hovered" : null;

            switch (NetworkAnarchy.instance.mode)
            {
                case Mode.Normal:
                    m_button.text += "Nrm\n";
                    m_normalModeButton.SimulateClick();
                    break;
                case Mode.Ground:
                    m_button.text += "Gnd\n";
                    m_groundModeButton.SimulateClick();
                    break;
                case Mode.Elevated:
                    m_button.text += "Elv\n";
                    m_elevatedModeButton.SimulateClick();
                    break;
                case Mode.Bridge:
                    m_button.text += "Bdg\n";
                    m_bridgeModeButton.SimulateClick();
                    break;
                case Mode.Tunnel:
                    m_button.text += "Tnl\n";
                    m_tunnelModeButton.SimulateClick();
                    break;
            }

            m_button.text += NetworkAnarchy.instance.elevation + "m";
        }

        private void UpdateSlider()
        {
            if (m_maxSegmentLengthSlider == null) return;

            NetworkAnarchy.saved_segmentLength.value = NetworkAnarchy.instance.MaxSegmentLength;
            m_maxSegmentLengthLabel.text = NetworkAnarchy.instance.MaxSegmentLength + "m";
            m_maxSegmentLengthLabel.tooltip = (Mathf.RoundToInt(NetworkAnarchy.instance.MaxSegmentLength / 8f * 100) / 100f).ToString() + "u";
        }

        private void CreateButton()
        {
            m_button = AddUIComponent<UIButton>();
            m_button.atlas = ResourceLoader.GetAtlas("Ingame");
            m_button.name = "NA_MainButton";
            m_button.size = new Vector2(36, 36);
            m_button.textScale = 0.7f;
            m_button.playAudioEvents = true;
            m_button.relativePosition = Vector2.zero;

            m_button.tooltip = "Network Anarchy " + ModInfo.Version + "\n\nClick here for Tool Options";

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
                    UpdateInfo();
                    windowVisible.value = true;
                }
                else
                {
                    m_button.normalBgSprite = "OptionBase";
                    windowVisible.value = false;
                }
                UpdateInfo();
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
            m_toolOptionsPanel.atlas = ResourceLoader.GetAtlas("Ingame");
            m_toolOptionsPanel.backgroundSprite = "SubcategoriesPanel";
            m_toolOptionsPanel.size = new Vector2(228, 172);
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

                savedWindowX.value = (int) m_toolOptionsPanel.absolutePosition.x;
                savedWindowY.value = (int) m_toolOptionsPanel.absolutePosition.y;
            };

            toolOptionsPanel = m_toolOptionsPanel;

            UIDragHandle dragHandle = m_toolOptionsPanel.AddUIComponent<UIDragHandle>();
            dragHandle.size = m_toolOptionsPanel.size;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = m_toolOptionsPanel;

            // Elevation step
            if (showElevationSlider)
            {
                UILabel label = m_toolOptionsPanel.AddUIComponent<UILabel>();
                label.textScale = 0.9f;
                label.text = "Elevation Step:";
                label.relativePosition = new Vector2(8, yPos);
                label.SendToBack();

                m_elevationStepSlider = CreateElevationSlider(m_toolOptionsPanel);

                yPos += 64u;
                m_toolOptionsPanel.height += 64;
                dragHandle.height += 64;
            }

            // Modes
            label = m_toolOptionsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Modes:";
            label.relativePosition = new Vector2(8, yPos);
            label.SendToBack();

            UIPanel modePanel = m_toolOptionsPanel.AddUIComponent<UIPanel>();
            modePanel.atlas = m_toolOptionsPanel.atlas;
            modePanel.backgroundSprite = "GenericPanel";
            modePanel.color = new Color32(206, 206, 206, 255);
            modePanel.size = new Vector2(m_toolOptionsPanel.width - 16, 52);
            modePanel.relativePosition = new Vector2(8, yPos + 20);

            modePanel.padding = new RectOffset(8, 8, 8, 8);
            modePanel.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            modePanel.autoLayoutDirection = LayoutDirection.Horizontal;

            m_normalModeButton = CreateModeCheckBox(modePanel, "NormalMode", "Normal: Unmodded road placement behavior");
            m_groundModeButton = CreateModeCheckBox(modePanel, "GroundMode", "Ground: Forces the ground to follow the elevation of the road");
            m_elevatedModeButton = CreateModeCheckBox(modePanel, "ElevatedMode", "Elevated: Forces the use of elevated pieces if available");
            m_bridgeModeButton = CreateModeCheckBox(modePanel, "BridgeMode", "Bridge: Forces the use of bridge pieces if available");
            m_tunnelModeButton = CreateModeCheckBox(modePanel, "TunnelMode", "Tunnel: Forces the use of tunnel pieces if available");

            modePanel.autoLayout = true;
            yPos += 80u;

            // Straight Slope
            m_straightSlope = CreateCheckBox(m_toolOptionsPanel);
            m_straightSlope.name = "NA_StraightSlope";
            m_straightSlope.label.text = "Straight slope";
            m_straightSlope.tooltip = "Makes the road go straight from A to B instead of following the terrain\n\n" + OptionsKeymapping.toggleStraightSlope.ToLocalizedString("KEYNAME") + " to toggle straight slope";
            m_straightSlope.isChecked = NetworkAnarchy.saved_smoothSlope;
            m_straightSlope.relativePosition = new Vector3(8, yPos);

            m_straightSlope.eventCheckChanged += (c, state) =>
            {
                NetworkAnarchy.instance.StraightSlope = state;
            };

            m_normalModeButton.isChecked = true;
            yPos += 24u;

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

            m_anarchyBtn = CreateAnarchyCheckBox(anarchyPanel, "Anarchy", "Toggle anarchy", NetworkAnarchy.saved_anarchy);
            m_bendingBtn = CreateAnarchyCheckBox(anarchyPanel, "Bending", "Toggle road bending", NetworkAnarchy.saved_bending);
            m_snappingBtn = CreateAnarchyCheckBox(anarchyPanel, "Snapping", "Toggle node snapping", NetworkAnarchy.saved_nodeSnapping);
            m_collisionBtn = CreateAnarchyCheckBox(anarchyPanel, "Collision", "Toggle collision", NetworkAnarchy.Collision);

            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                m_gridBtn = CreateAnarchyCheckBox(anarchyPanel, "Grid", "Toggle editor grid", true);
            }

            UpdateAnarchyOptions();

            anarchyPanel.autoLayout = true;
            yPos += 58;

            // Node spacing
            if (showMaxSegmentLengthSlider)
            {
                UIPanel mslPanel = m_toolOptionsPanel.AddUIComponent<UIPanel>();
                mslPanel.size = new Vector2(mslPanel.parent.width - 16, 62);
                mslPanel.name = "NA_MaxSegmentLengthPanel";
                mslPanel.relativePosition = new Vector3(8, yPos);
                mslPanel.atlas = ResourceLoader.GetAtlas("Ingame");

                mslPanel.padding = new RectOffset(0, 0, 0, 0);
                mslPanel.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
                mslPanel.autoLayoutDirection = LayoutDirection.Horizontal;

                UILabel label = mslPanel.AddUIComponent<UILabel>();
                label.textScale = 0.825f;
                label.text = "Max Segment Length:";
                label.name = "NA_MaxSegmentLengthLabel";
                label.relativePosition = new Vector2(0, 1);
                label.SendToBack();

                m_maxSegmentLengthLabel = mslPanel.AddUIComponent<UILabel>();
                m_maxSegmentLengthLabel.atlas = m_toolOptionsPanel.atlas;
                m_maxSegmentLengthLabel.verticalAlignment = UIVerticalAlignment.Bottom;
                m_maxSegmentLengthLabel.textScale = 0.85f;
                m_maxSegmentLengthLabel.autoSize = false;
                m_maxSegmentLengthLabel.color = new Color32(91, 97, 106, 255);
                m_maxSegmentLengthLabel.size = new Vector2(42, 15);
                m_maxSegmentLengthLabel.relativePosition = new Vector2(label.width + 3, 1);
                UpdateSlider();

                m_maxSegmentLengthSlider = CreateMaxSegmentLengthSlider(mslPanel);

                yPos += 64u;
                m_toolOptionsPanel.height += 64;
                dragHandle.height += 64;
            }
        }

        private UISlider CreateElevationSlider(UIPanel parent)
        {

            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();
            sliderPanel.atlas = parent.atlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(parent.width - 16, 36);
            sliderPanel.relativePosition = new Vector2(8, 28);

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

            slider.tooltip = OptionsKeymapping.elevationStepUp.ToLocalizedString("KEYNAME") + " and " + OptionsKeymapping.elevationStepDown.ToLocalizedString("KEYNAME") + " to change Elevation Step";

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
                    UpdateInfo();
                }
            };

            return slider;
        }

        private UISlider CreateMaxSegmentLengthSlider(UIPanel parent)
        {
            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();

            sliderPanel.atlas = parent.atlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(parent.width, 36);
            sliderPanel.relativePosition = new Vector2(0, 22);

            UISlider slider = sliderPanel.AddUIComponent<UISlider>();
            slider.name = "NA_MaxSegmentLengthSlider";
            slider.size = new Vector2(sliderPanel.width - 16, 18);
            slider.relativePosition = new Vector2(8, 10);
            slider.atlas = parent.atlas;

            UISlicedSprite bgSlider = slider.AddUIComponent<UISlicedSprite>();
            bgSlider.atlas = parent.atlas;
            bgSlider.spriteName = "BudgetSlider";
            bgSlider.size = new Vector2(slider.width, 9);
            bgSlider.relativePosition = new Vector2(0, 4);

            UISlicedSprite thumb = slider.AddUIComponent<UISlicedSprite>();
            thumb.atlas = parent.atlas;
            thumb.spriteName = "SliderBudget";
            slider.thumbObject = thumb;

            slider.stepSize = NetworkAnarchy.SegmentLengthInterval;
            slider.minValue = NetworkAnarchy.SegmentLengthFloor;
            slider.maxValue = NetworkAnarchy.SegmentLengthCeiling;
            slider.value = NetworkAnarchy.instance.MaxSegmentLength;

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
            ((UISprite) checkBox.checkedBoxObject).atlas = m_toolOptionsPanel.atlas;
            ((UISprite) checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
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

            button.tooltip = toolTip + "\n\n" + OptionsKeymapping.modesCycleLeft.ToLocalizedString("KEYNAME") + " and " + OptionsKeymapping.modesCycleRight.ToLocalizedString("KEYNAME") + " to cycle Modes";

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

        private void UpdateMode()
        {
            if (m_normalModeButton.isChecked)
                NetworkAnarchy.instance.mode = Mode.Normal;
            if (m_groundModeButton.isChecked)
                NetworkAnarchy.instance.mode = Mode.Ground;
            if (m_elevatedModeButton.isChecked)
                NetworkAnarchy.instance.mode = Mode.Elevated;
            if (m_bridgeModeButton.isChecked)
                NetworkAnarchy.instance.mode = Mode.Bridge;
            if (m_tunnelModeButton.isChecked)
                NetworkAnarchy.instance.mode = Mode.Tunnel;
        }

        private UICheckBox CreateAnarchyCheckBox(UIComponent parent, string spriteName, string toolTip, bool value)
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

                UpdateAnarchyOptions();
            };

            return checkBox;
        }

        private void UpdateAnarchyOptions()
        {
            NetworkAnarchy.Anarchy = m_anarchyBtn.isChecked;
            NetworkAnarchy.Bending = m_bendingBtn.isChecked;
            NetworkAnarchy.NodeSnapping = m_snappingBtn.isChecked;
            NetworkAnarchy.Collision = m_collisionBtn.isChecked;

            if (m_gridBtn != null)
            {
                NetworkAnarchy.Grid = m_gridBtn.isChecked;
            }
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
                "Collision",
                "CollisionDisabled",
                "CollisionFocused",
                "CollisionHovered",
                "CollisionPressed",
                "Grid",
                "GridDisabled",
                "GridFocused",
                "GridHovered",
                "GridPressed"
            };

            m_atlas = ResourceLoader.CreateTextureAtlas("NetworkAnarchy", spriteNames, "NetworkAnarchy.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[]
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture
            };

            ResourceLoader.AddTexturesInAtlas(m_atlas, textures);
        }
    }
}

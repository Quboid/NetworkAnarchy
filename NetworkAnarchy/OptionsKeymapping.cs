using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnifiedUI.Helpers;
using System;

namespace NetworkAnarchy
{
    public class OptionsKeymapping : UICustomControl
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";

        private SavedInputKey m_EditingBinding;

        private string m_EditingBindingCategory;

        public static readonly SavedInputKey elevationUp = new SavedInputKey(Settings.buildElevationUp, Settings.gameSettingsFile, DefaultSettings.buildElevationUp, true);
        public static readonly SavedInputKey elevationDown = new SavedInputKey(Settings.buildElevationDown, Settings.gameSettingsFile, DefaultSettings.buildElevationDown, true);
        public static readonly SavedInputKey elevationReset = new SavedInputKey("elevationReset", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.Home, false, false, false), true);

        public static readonly SavedInputKey elevationStepUp = new SavedInputKey("elevationStepUp", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.UpArrow, true, false, false), true);
        public static readonly SavedInputKey elevationStepDown = new SavedInputKey("elevationStepDown", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.DownArrow, true, false, false), true);

        public static readonly SavedInputKey modesCycleRight = new SavedInputKey("modesCycleRight", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.RightArrow, true, false, false), true);
        public static readonly SavedInputKey modesCycleLeft = new SavedInputKey("modesCycleLeft", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.LeftArrow, true, false, false), true);

        public static readonly SavedInputKey toggleStraightSlope = new SavedInputKey("toggleStraightSlope", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.S, false, true, false), true);

        public static readonly SavedInputKey toggleAnarchy = new SavedInputKey("toggleAnarchy", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.A, true, false, false), true);
        public static readonly SavedInputKey toggleBending = new SavedInputKey("toggleBending", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.B, true, false, false), true);
        public static readonly SavedInputKey toggleSnapping = new SavedInputKey("toggleSnapping", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.S, true, false, false), true);
        public static readonly SavedInputKey toggleCollision = new SavedInputKey("toggleCollision", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.C, false, false, true), true);
        public static readonly SavedInputKey toggleGrid = new SavedInputKey("toggleGrid", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.G, false, false, true), true);

        private int count = 0;

        public static void RegisterUUIHotkeys()
        {
            bool IsActive() => NetworkAnarchy.instance.isActive;
            Dictionary<SavedInputKey, Func<bool>> intoolKeys = new Dictionary<SavedInputKey, Func<bool>>
            {
                // use UUI to resolve hotkey collisions
                { elevationUp, IsActive },
                { elevationDown, IsActive },
                { elevationReset, IsActive },
                { elevationStepUp, IsActive },
                { elevationStepDown, IsActive },
                { modesCycleRight, IsActive },
                { modesCycleLeft, IsActive },
                { toggleStraightSlope, IsActive },
                { toggleAnarchy, IsActive },
                { toggleBending, IsActive },
                { toggleSnapping, IsActive },
                { toggleCollision, IsActive },
                { toggleGrid, IsActive }
            };

            UUIHelpers.RegisterHotkeys(null, activeKeys: intoolKeys);
        }

        private void Awake()
        {
            AddKeymapping("Elevation Up", elevationUp);
            AddKeymapping("Elevation Down", elevationDown);
            AddKeymapping("Reset Elevation", elevationReset);
            AddKeymapping("Elevation Step Up", elevationStepUp);
            AddKeymapping("Elevation Step Down", elevationStepDown);
            AddKeymapping("Cycle Modes Right", modesCycleRight);
            AddKeymapping("Cycle Modes Left", modesCycleLeft);
            AddKeymapping("Toggle Straight Slope", toggleStraightSlope);
            AddKeymapping("Toggle Anarchy", toggleAnarchy);
            AddKeymapping("Toggle Bending", toggleBending);
            AddKeymapping("Toggle Snapping", toggleSnapping);
            AddKeymapping("Toggle Collision", toggleCollision);
            AddKeymapping("Toggle Editor Grid", toggleGrid);
        }

        private void AddKeymapping(string label, SavedInputKey savedInputKey)
        {
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel;
            if (count++ % 2 == 1) uIPanel.backgroundSprite = null;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += new KeyPressHandler(this.OnBindingKeyDown);
            uIButton.eventMouseDown += new MouseEventHandler(this.OnBindingMouseDown);

            uILabel.text = label;
            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uIButton.objectUserData = savedInputKey;
            uIButton.eventVisibilityChanged += ButtonVisibilityChanged;
        }

        private void OnEnable()
        {
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnLocaleChanged()
        {
            this.RefreshBindableInputs();
        }

        private bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private bool IsUnbindableMouseButton(UIMouseButton code)
        {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        private bool IsAlreadyBound(SavedInputKey target, InputKey inputKey, string category, out List<SavedInputKey> currentAssigned)
        {
            currentAssigned = new List<SavedInputKey>();
            if (inputKey == SavedInputKey.Empty)
            {
                return false;
            }

            if (inputKey == elevationReset.value) currentAssigned.Add(elevationReset);
            if (inputKey == elevationStepUp.value) currentAssigned.Add(elevationReset);
            if (inputKey == elevationStepDown.value) currentAssigned.Add(elevationReset);
            if (inputKey == modesCycleLeft.value) currentAssigned.Add(elevationReset);
            if (inputKey == modesCycleRight.value) currentAssigned.Add(elevationReset);

            FieldInfo[] fields = typeof(Settings).GetFields(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInfo = fields[i];
                RebindableKeyAttribute[] array2 = fieldInfo.GetCustomAttributes(typeof(RebindableKeyAttribute), false) as RebindableKeyAttribute[];
                if (array2.Length > 0 && (string.IsNullOrEmpty(category) || category == array2[0].category || string.IsNullOrEmpty(array2[0].category)))
                {
                    string text = fieldInfo.GetValue(null) as string;
                    SavedInputKey savedInputKey = new SavedInputKey(text, Settings.gameSettingsFile, this.GetDefaultEntry(text), true);
                    if (!(target == savedInputKey))
                    {
                        if (inputKey == savedInputKey.value)
                        {
                            currentAssigned.Add(savedInputKey);
                        }
                    }
                }
            }
            return currentAssigned.Count > 0;
        }

        private KeyCode ButtonToKeycode(UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
            {
                return KeyCode.Mouse0;
            }
            if (button == UIMouseButton.Right)
            {
                return KeyCode.Mouse1;
            }
            if (button == UIMouseButton.Middle)
            {
                return KeyCode.Mouse2;
            }
            if (button == UIMouseButton.Special0)
            {
                return KeyCode.Mouse3;
            }
            if (button == UIMouseButton.Special1)
            {
                return KeyCode.Mouse4;
            }
            if (button == UIMouseButton.Special2)
            {
                return KeyCode.Mouse5;
            }
            if (button == UIMouseButton.Special3)
            {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        private static void ButtonVisibilityChanged(UIComponent component, bool isVisible) {
            if (isVisible && component.objectUserData is SavedInputKey savedInputKey) {
                (component as UIButton).text = savedInputKey.ToLocalizedString("KEYNAME");
            }
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (this.m_EditingBinding != null && !this.IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey inputKey = (p.keycode == KeyCode.Escape) ? this.m_EditingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace)
                {
                    inputKey = SavedInputKey.Empty;
                }
                List<SavedInputKey> currentAssigned;
                if (!this.IsAlreadyBound(this.m_EditingBinding, inputKey, this.m_EditingBindingCategory, out currentAssigned))
                {
                    this.m_EditingBinding.value = inputKey;
                    UITextComponent uITextComponent = p.source as UITextComponent;
                    uITextComponent.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                    this.m_EditingBinding = null;
                    this.m_EditingBindingCategory = string.Empty;
                }
                else
                {
                    string arg = (currentAssigned.Count <= 1) ? Locale.Get("KEYMAPPING", currentAssigned[0].name) : Locale.Get("KEYMAPPING_MULTIPLE");
                    string message = string.Format(Locale.Get("CONFIRM_REBINDKEY", "Message"), SavedInputKey.ToLocalizedString("KEYNAME", inputKey), arg);
                    ConfirmPanel.ShowModal(Locale.Get("CONFIRM_REBINDKEY", "Title"), message, delegate(UIComponent c, int ret)
                    {
                        if (ret == 1)
                        {
                            this.m_EditingBinding.value = inputKey;
                            for (int i = 0; i < currentAssigned.Count; i++)
                            {
                                currentAssigned[i].value = SavedInputKey.Empty;
                            }
                            this.RefreshKeyMapping();
                        }
                        UITextComponent uITextComponent2 = p.source as UITextComponent;
                        uITextComponent2.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                        this.m_EditingBinding = null;
                        this.m_EditingBindingCategory = string.Empty;
                    });
                }
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (this.m_EditingBinding == null)
            {
                p.Use();
                this.m_EditingBinding = (SavedInputKey)p.source.objectUserData;
                this.m_EditingBindingCategory = p.source.stringUserData;
                UIButton uIButton = p.source as UIButton;
                uIButton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uIButton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!this.IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                InputKey inputKey = SavedInputKey.Encode(this.ButtonToKeycode(p.buttons), this.IsControlDown(), this.IsShiftDown(), this.IsAltDown());
                List<SavedInputKey> currentAssigned;
                if (!this.IsAlreadyBound(this.m_EditingBinding, inputKey, this.m_EditingBindingCategory, out currentAssigned))
                {
                    this.m_EditingBinding.value = inputKey;
                    UIButton uIButton2 = p.source as UIButton;
                    uIButton2.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                    uIButton2.buttonsMask = UIMouseButton.Left;
                    this.m_EditingBinding = null;
                    this.m_EditingBindingCategory = string.Empty;
                }
                else
                {
                    string arg = (currentAssigned.Count <= 1) ? Locale.Get("KEYMAPPING", currentAssigned[0].name) : Locale.Get("KEYMAPPING_MULTIPLE");
                    string message = string.Format(Locale.Get("CONFIRM_REBINDKEY", "Message"), SavedInputKey.ToLocalizedString("KEYNAME", inputKey), arg);
                    ConfirmPanel.ShowModal(Locale.Get("CONFIRM_REBINDKEY", "Title"), message, delegate(UIComponent c, int ret)
                    {
                        if (ret == 1)
                        {
                            this.m_EditingBinding.value = inputKey;
                            for (int i = 0; i < currentAssigned.Count; i++)
                            {
                                currentAssigned[i].value = SavedInputKey.Empty;
                            }
                            this.RefreshKeyMapping();
                        }
                        UIButton uIButton3 = p.source as UIButton;
                        uIButton3.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                        uIButton3.buttonsMask = UIMouseButton.Left;
                        this.m_EditingBinding = null;
                        this.m_EditingBindingCategory = string.Empty;
                    });
                }
            }
        }

        private void RefreshBindableInputs()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                if (uITextComponent != null)
                {
                    SavedInputKey savedInputKey = uITextComponent.objectUserData as SavedInputKey;
                    if (savedInputKey != null)
                    {
                        uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                    }
                }
                UILabel uILabel = current.Find<UILabel>("Name");
                if (uILabel != null)
                {
                    uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
                }
            }
        }

        internal InputKey GetDefaultEntry(string entryName)
        {
            FieldInfo field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return 0;
            }
            object value = field.GetValue(null);
            if (value is InputKey)
            {
                return (InputKey)value;
            }
            return 0;
        }

        private void RefreshKeyMapping()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                SavedInputKey savedInputKey = (SavedInputKey)uITextComponent.objectUserData;
                if (this.m_EditingBinding != savedInputKey)
                {
                    uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                }
            }
        }
    }
}
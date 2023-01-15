using ColossalFramework;
using NetworkAnarchy.Lang;
using QCommonLib;
using System;
using System.Collections.Generic;
using UnifiedUI.Helpers;
using UnityEngine;

namespace NetworkAnarchy
{
    public class OptionsKeymapping : QKeybinding
    {
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
        public static readonly SavedInputKey toggleCollision = new SavedInputKey("toggleCollision", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.None, false, false, false), true);
        public static readonly SavedInputKey toggleGrid = new SavedInputKey("toggleGrid", NetworkAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.G, false, false, true), true);

        protected override void Awake()
        {
            PressAnyKeyStr = Str.key_pressAnyKey;

            Keybindings = new Dictionary<string, SavedInputKey>
            {
                { Str.key_elevationUp, elevationUp },
                { Str.key_elevationDown, elevationDown },
                { Str.key_elevationReset, elevationReset },
                { Str.key_elevationStepUp, elevationStepUp },
                { Str.key_elevationStepDown, elevationStepDown },

                { Str.key_cycleModesRight, modesCycleRight },
                { Str.key_cycleModesLeft, modesCycleLeft },
                { Str.key_toggleStraightSlopes, toggleStraightSlope },

                { Str.key_toggleAnarchy, toggleAnarchy },
                { Str.key_toggleBending, toggleBending },
                { Str.key_toggleSnapping, toggleSnapping },
                { Str.ui_toggleCollision, toggleCollision },
                { Str.key_toggleGrid, toggleGrid }
            };

            AddKeymappingsList();
        }

        public static void RegisterUUIHotkeys()
        {
            bool GetIsActive() => NetworkAnarchy.instance.IsActive;

            Dictionary<SavedInputKey, Func<bool>> intoolKeys = new Dictionary<SavedInputKey, Func<bool>>();
            // use UUI to resolve hotkey collisions
            intoolKeys.Add(OptionsKeymapping.elevationUp, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.elevationDown, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.elevationReset, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.elevationStepUp, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.elevationStepDown, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.modesCycleRight, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.modesCycleLeft, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.toggleStraightSlope, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.toggleBending, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.toggleSnapping, GetIsActive);
            intoolKeys.Add(OptionsKeymapping.toggleGrid, GetIsActive);

            UUIHelpers.RegisterHotkeys(
                onToggle: () => { },
                activationKey: OptionsKeymapping.toggleAnarchy,
                activeKeys: intoolKeys);
            UUIHelpers.RegisterHotkeys(
                onToggle: () => { },
                activationKey: OptionsKeymapping.toggleCollision);
        }
    }
}
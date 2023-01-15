namespace NetworkAnarchy.Lang
{
	public class Str
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static QCommonLib.Lang.LocalizeManager LocaleManager {get;} = new QCommonLib.Lang.LocalizeManager("Str", typeof(Str).Assembly);

		/// <summary>
		/// Cycle Modes Left
		/// </summary>
		public static string key_cycleModesLeft => LocaleManager.GetString("key_cycleModesLeft", Culture);

		/// <summary>
		/// Cycle Modes Right
		/// </summary>
		public static string key_cycleModesRight => LocaleManager.GetString("key_cycleModesRight", Culture);

		/// <summary>
		/// Elevation Down
		/// </summary>
		public static string key_elevationDown => LocaleManager.GetString("key_elevationDown", Culture);

		/// <summary>
		/// Reset Elevation
		/// </summary>
		public static string key_elevationReset => LocaleManager.GetString("key_elevationReset", Culture);

		/// <summary>
		/// Elevation Step Down
		/// </summary>
		public static string key_elevationStepDown => LocaleManager.GetString("key_elevationStepDown", Culture);

		/// <summary>
		/// Elevation Step Up
		/// </summary>
		public static string key_elevationStepUp => LocaleManager.GetString("key_elevationStepUp", Culture);

		/// <summary>
		/// Elevation Up
		/// </summary>
		public static string key_elevationUp => LocaleManager.GetString("key_elevationUp", Culture);

		/// <summary>
		/// Press any key
		/// </summary>
		public static string key_pressAnyKey => LocaleManager.GetString("key_pressAnyKey", Culture);

		/// <summary>
		/// Toggle Anarchy
		/// </summary>
		public static string key_toggleAnarchy => LocaleManager.GetString("key_toggleAnarchy", Culture);

		/// <summary>
		/// Toggle Bending
		/// </summary>
		public static string key_toggleBending => LocaleManager.GetString("key_toggleBending", Culture);

		/// <summary>
		/// Toggle Editor Grid
		/// </summary>
		public static string key_toggleGrid => LocaleManager.GetString("key_toggleGrid", Culture);

		/// <summary>
		/// Toggle Snapping
		/// </summary>
		public static string key_toggleSnapping => LocaleManager.GetString("key_toggleSnapping", Culture);

		/// <summary>
		/// Toggle Straight Slopes
		/// </summary>
		public static string key_toggleStraightSlopes => LocaleManager.GetString("key_toggleStraightSlopes", Culture);

		/// <summary>
		/// Toggle Zone Override
		/// </summary>
		public static string key_toggleZoneOverride => LocaleManager.GetString("key_toggleZoneOverride", Culture);

		/// <summary>
		/// More tool options for roads and other networks
		/// </summary>
		public static string mod_Description => LocaleManager.GetString("mod_Description", Culture);

		/// <summary>
		/// Enable debug messages logging
		/// </summary>
		public static string options_enableDebugLogging => LocaleManager.GetString("options_enableDebugLogging", Culture);

		/// <summary>
		/// If checked, additional debug messages will be logged.
		/// </summary>
		public static string options_enableDebugLoggingTooltip => LocaleManager.GetString("options_enableDebugLoggingTooltip", Culture);

		/// <summary>
		/// Max turn angle
		/// </summary>
		public static string options_maxTurnAngle => LocaleManager.GetString("options_maxTurnAngle", Culture);

		/// <summary>
		/// Reduce rail catenary masts
		/// </summary>
		public static string options_reduceCatenaries => LocaleManager.GetString("options_reduceCatenaries", Culture);

		/// <summary>
		/// Reduce the number of catenary mast of rail lines from 3 to 1 per segment.
		/// </summary>
		public static string options_reduceCatenariesTooltip => LocaleManager.GetString("options_reduceCatenariesTooltip", Culture);

		/// <summary>
		/// Reset tool window position
		/// </summary>
		public static string options_resetToolWindowPosition => LocaleManager.GetString("options_resetToolWindowPosition", Culture);

		/// <summary>
		/// Show elevation step slider
		/// </summary>
		public static string options_showElevationStepSlider => LocaleManager.GetString("options_showElevationStepSlider", Culture);

		/// <summary>
		/// Show slider for changing the elevation step, from 1m to 12m.
		/// </summary>
		public static string options_showElevationStepSliderTooltip => LocaleManager.GetString("options_showElevationStepSliderTooltip", Culture);

		/// <summary>
		/// Show Labels
		/// </summary>
		public static string options_showLabels => LocaleManager.GetString("options_showLabels", Culture);

		/// <summary>
		/// Disable to save some screen space
		/// </summary>
		public static string options_showLabelsTooltip => LocaleManager.GetString("options_showLabelsTooltip", Culture);

		/// <summary>
		/// Show max segment length slider
		/// </summary>
		public static string options_showMaxSegmentLengthSlider => LocaleManager.GetString("options_showMaxSegmentLengthSlider", Culture);

		/// <summary>
		/// Show slider for changing the maximum segment length, from 4m to 256m (default is 96m).
		/// </summary>
		public static string options_showMaxSegmentLengthSliderTooltip => LocaleManager.GetString("options_showMaxSegmentLengthSliderTooltip", Culture);

		/// <summary>
		/// Change max turn angle for more realistic tram tracks turns
		/// </summary>
		public static string options_tramMaxTurnAngle => LocaleManager.GetString("options_tramMaxTurnAngle", Culture);

		/// <summary>
		/// Change all roads with tram tracks max turn angle by the value below if current value is higher.
		/// </summary>
		public static string options_tramMaxTurnAngleTooltip => LocaleManager.GetString("options_tramMaxTurnAngleTooltip", Culture);

		/// <summary>
		/// Click this button to show Network Anarchy
		/// </summary>
		public static string popup_buttonReminder => LocaleManager.GetString("popup_buttonReminder", Culture);

		/// <summary>
		/// Collision has been combined with Anarchy mode.
		/// </summary>
		public static string popup_collisionRemoved => LocaleManager.GetString("popup_collisionRemoved", Culture);

		/// <summary>
		/// Click here for Tool Options
		/// </summary>
		public static string ui_clickForToolOptions => LocaleManager.GetString("ui_clickForToolOptions", Culture);

		/// <summary>
		/// {0} and {1} to change elevation step
		/// </summary>
		public static string ui_elevationSliderKeyTip => LocaleManager.GetString("ui_elevationSliderKeyTip", Culture);

		/// <summary>
		/// Elevation Step:
		/// </summary>
		public static string ui_elevationStep => LocaleManager.GetString("ui_elevationStep", Culture);

		/// <summary>
		/// Show Editor Grid
		/// </summary>
		public static string ui_grid => LocaleManager.GetString("ui_grid", Culture);

		/// <summary>
		/// Max Segment Length:
		/// </summary>
		public static string ui_maxSegmentLength => LocaleManager.GetString("ui_maxSegmentLength", Culture);

		/// <summary>
		/// Bridge: Forces the use of bridge pieces if available
		/// </summary>
		public static string ui_modeBridge => LocaleManager.GetString("ui_modeBridge", Culture);

		/// <summary>
		/// {0} and {1} to cycle modes
		/// </summary>
		public static string ui_modeCycleKeyTip => LocaleManager.GetString("ui_modeCycleKeyTip", Culture);

		/// <summary>
		/// Elevated: Forces the use of elevated pieces if available
		/// </summary>
		public static string ui_modeElevated => LocaleManager.GetString("ui_modeElevated", Culture);

		/// <summary>
		/// Ground: Forces the ground to follow the elevation of the road
		/// </summary>
		public static string ui_modeGround => LocaleManager.GetString("ui_modeGround", Culture);

		/// <summary>
		/// Normal: Unmodded road placement behavior
		/// </summary>
		public static string ui_modeNormal => LocaleManager.GetString("ui_modeNormal", Culture);

		/// <summary>
		/// Modes:
		/// </summary>
		public static string ui_modes => LocaleManager.GetString("ui_modes", Culture);

		/// <summary>
		/// Tunnel: Forces the use of tunnel pieces if available
		/// </summary>
		public static string ui_modeTunnel => LocaleManager.GetString("ui_modeTunnel", Culture);

		/// <summary>
		/// Toggle anarchy
		/// </summary>
		public static string ui_toggleAnarchy => LocaleManager.GetString("ui_toggleAnarchy", Culture);

		/// <summary>
		/// Toggle road bending
		/// </summary>
		public static string ui_toggleBending => LocaleManager.GetString("ui_toggleBending", Culture);

		/// <summary>
		/// Toggle collision
		/// </summary>
		public static string ui_toggleCollision => LocaleManager.GetString("ui_toggleCollision", Culture);

		/// <summary>
		/// Toggle straight slope (don't follow terrain shape)
		/// </summary>
		public static string ui_toggleSlope => LocaleManager.GetString("ui_toggleSlope", Culture);

		/// <summary>
		/// Toggle node snapping
		/// </summary>
		public static string ui_toggleSnapping => LocaleManager.GetString("ui_toggleSnapping", Culture);

		/// <summary>
		/// Override zone collision
		/// </summary>
		public static string ui_toggleZoneOverride => LocaleManager.GetString("ui_toggleZoneOverride", Culture);
	}
}
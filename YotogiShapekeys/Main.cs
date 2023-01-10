using BepInEx;
using BepInEx.Configuration;
using COM3D2API;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using UnityEngine;

//using System.Threading.Tasks;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ShapekeyMaster
{
	[BepInPlugin("ShapekeyMaster", "ShapekeyMaster", "1.5")]
	[BepInDependency("deathweasel.com3d2.api")]
	public class Main : BaseUnityPlugin
	{
		internal static Main @this { get; private set; }

		internal static BepInEx.Logging.ManualLogSource logger { get; private set; }

		public static bool enablegui { get; internal set; } = false;

		private static SaveFileDialog FileSave = new SaveFileDialog();
		private static OpenFileDialog FileOpen = new OpenFileDialog();

		private static readonly string iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABwAAAAcCAYAAAByDd+UAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANRSURBVEhLzVZdK6RhGL6G8TFjFTOrEWnGiRgRocmhA2rbAzmwp+IfOPMLHDsnKaG2FA5WkhSJspPPGSTyEWYyFsn4Zp/rnvfVDLNrD2amverufZ73fue+nvvrmdsA4JOSz0psSqxKTEqSlMQCz0qCSn4p8Sk5UwJ7RUVF8+rqquclTpifn/9JDsXlIKGLLzRd3ECHyMWQflH7H2SONwwGw1fmyhzaJgQmEsaqQP4FSYkkE/xfhEtLS+jt7UVXVxdGRkZwcnKiaaLD7Xajr68PT09Pst/Z2UFPTw8WFxdlT/yRcG5uDmNjYwgEAsjNzcXm5iYGBgY07Xs8PDxgamoK+/v72N3dhcfjke+Pjo5wf3+vffUBodVqRXt7OxobG5GXl4fb21tN+x480M3NjaxnZ2cxPDwM1W5ITk6Ganp5Txi1p4CnGRoawvX1texpoLOzU9ZEWVmZtnoPhpPGzWYzDg8PkZ+fL/aKi4uRkZGhffWGMDU1FVVVVbLe2tqC3++H3W4XycrKQnl5ueiYS+bl+PhY1XkS6uvrJZSlpaUoKSmRtdFoFELdno6IkObk5MDhcOD09FS8SUlJgc/nE6LKykrxYHp6Gt3d3VJQFxcXsFgsWFtbk9/TOEl5gOXlZUlJYWGh6HREELKq+vv7cXBwgKKiIjQ1NeHu7g6jo6OiJ8nMzIyQtLW1oaOjQ/LL/IUbV/empIOHfIsIQhpOT09HS0uLeMvw0FPmhF7TO3pJfUFBgfxmfX1djIeHbmNjQ56Xl5dYWVmRtY5XQiqvrq7gdDqFTAcPQDC01LNaMzMz5R37jRWZlpYWUYksGB6MeWZUwvFaNCwYgoWgg+FkeNQtL71IsC/Zc8Tk5CTOz8/hcrlgMvF/O4S6ujqRaODfU7Pql+/cDA4OYnt7GzabTTzxer1CWltbi4aGBrlFWIF6mbN9+G1ra6t4+RHUwb9FENLAxMSEEDFc2dnZqKmpEUKCYR8fH8fe3p60CXNcXV0d0Wd/Awn5JGEEHh8fX4LBoLaLHcgV9WpjwsNzEktEJYwnSMhRLlF4JmHoik8MgiQ8U3ehN7SPHxYWFtzqwYEYDg6p8ZxNOZNqg7BdH/U54vMqsSjh2BirYmJ9MGUc8f0AAr8BwP7aKtdTkPoAAAAASUVORK5CYII=";

		internal static ConfigEntry<float> MaxDeform;
		internal static ConfigEntry<bool> Autosave;

		internal static ConfigEntry<bool> SimpleMode;
		internal static ConfigEntry<bool> HideInactiveMaids;
		internal static ConfigEntry<int> EntriesPerPage;
		internal static ConfigEntry<string> Language;

		internal static ConfigEntry<bool> HotkeyEnabled;
		internal static ConfigEntry<KeyboardShortcut> Hotkey;

		internal static TranslationResource CurrentLanguage { get; private set; }

		private void Awake()
		{
			@this = this;

			logger = Logger;

			if (Directory.Exists(BepInEx.Paths.ConfigPath + $"\\ShapekeyMaster\\") == false) 
			{
				logger.LogFatal("It seems we're lacking any translation folder for ShapekeyMaster! This is bad and we can't start without that and the translation files! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			var translationFiles = Directory.GetFiles(BepInEx.Paths.ConfigPath + $"\\ShapekeyMaster\\", "*.*")
				.Where(s => s.ToLower().EndsWith(".json"))
				.Select(file => Path.GetFileName(file))
				.ToArray();

			if (translationFiles.Count() <= 0)
			{
				logger.LogFatal("It seems we're lacking any translation files for ShapekeyMaster! This is bad and we can't start without them! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			var acceptableValues = new AcceptableValueList<string>(translationFiles);

			MaxDeform = Config.Bind("General", "1. Max Deformation", 100f, "The max limit of the sliders in UI.");
			Autosave = Config.Bind("General", "2. Autosave", true, "Will the config be saved automatically at set points.");

			SimpleMode = Config.Bind("UI", "1. Simple Mode", true, "Simple mode is a simplified view of your shapekeys holding only the most basic of settings. All you really need in most cases.");
			HideInactiveMaids = Config.Bind("UI", "2. Hide Inactive Maids", false, "In the maids view, maids that are not present or loaded are hidden from the menu options.");
			EntriesPerPage = Config.Bind("UI", "3. Entries Per Page", 10, "How many entries to display per an entry page.");
			Language = Config.Bind("UI", "4. Language", "english.json", new ConfigDescription("The language for SKM's UI.", acceptableValues));

			Language.SettingChanged += (e, s) =>
			{
				CurrentLanguage = new TranslationResource(BepInEx.Paths.ConfigPath + $"\\ShapekeyMaster\\" + Language.Value);
			};

			CurrentLanguage = new TranslationResource(BepInEx.Paths.ConfigPath + $"\\ShapekeyMaster\\" + Language.Value);

			HotkeyEnabled = Config.Bind("Hotkey", "1. Enable Hotkey", false, "Use a hotkey to open ShapekeyMaster.");
			Hotkey = Config.Bind("Hotkey", "2. Hotkey", new KeyboardShortcut(KeyCode.F4, KeyCode.LeftControl, KeyCode.LeftAlt), "Hotkey to open ShapekeyMaster with.");

			Harmony.CreateAndPatchAll(typeof(HarmonyPatchers));

			if (File.Exists(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json"))
			{
				var serDB = LoadFromJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json");
				UI.SKDatabase.AllShapekeyDictionary = serDB.AllShapekeyDictionary;
				UI.SKDatabase.BlacklistedShapekeys = serDB.BlacklistedShapekeys;
			}

			SystemShortcutAPI.AddButton("ShapekeyMaster", () =>
			{
				enablegui = !enablegui;

				if (Autosave.Value)
				{
					SaveToJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", UI.SKDatabase);
				}
			}, "Open/Close GUI", Convert.FromBase64String(iconBase64));

			Logger.LogInfo("ShapekeyMaster is online!");
		}

		private void Update()
		{
			if (HotkeyEnabled.Value && Hotkey.Value.IsDown())
			{
				enablegui = !enablegui;
			}
		}

		private void OnGUI()
		{
			if (enablegui)
			{
				UI.Initialize();
			}
		}

		private void OnDestroy()
		{
			if (Autosave.Value)
			{
				SaveToJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", UI.SKDatabase);
			}
		}

		internal static void SaveToJson(string path, ShapekeyDatabase database, bool withPrompt = false)
		{
			if (withPrompt)
			{
				FileSave.Filter = "json files (*.json)|*.json";
				FileSave.InitialDirectory = BepInEx.Paths.GameRootPath;
				FileSave.ShowDialog();

				if (!String.IsNullOrEmpty(FileSave.FileName))
				{
					path = FileSave.FileName;
				}
			}

			if (!String.IsNullOrEmpty(path))
			{
				File.WriteAllText(path, JsonConvert.SerializeObject(database, Formatting.Indented));
			}
		}

		internal static ShapekeyDatabase LoadFromJson(string path, bool withPrompt = false)
		{
			if (withPrompt)
			{
				FileOpen.Filter = "json files (*.json)|*.json";
				FileOpen.InitialDirectory = BepInEx.Paths.GameRootPath;
				FileOpen.ShowDialog();

				if (!String.IsNullOrEmpty(FileOpen.FileName))
				{
					path = FileOpen.FileName;
				}
			}

			if (!String.IsNullOrEmpty(path) && File.Exists(path))
			{
				string mconfig = (File.ReadAllText(path));

				var serializeset = new JsonSerializerSettings();
				serializeset.ObjectCreationHandling = ObjectCreationHandling.Replace;

				return JsonConvert.DeserializeObject<ShapekeyDatabase>(mconfig, serializeset);
			}

			return null;
		}

		/*
		public static void SetBlendValues(int f_nIdx, float f_fValue, TMorph morph)
		{
			float[] blendValuesBackup = morph.BlendValuesBackup;
			morph.BlendValues[f_nIdx] = f_fValue;
			blendValuesBackup[f_nIdx] = f_fValue;
		}
		*/
	}
}
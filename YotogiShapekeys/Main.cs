using BepInEx;
using BepInEx.Configuration;
using COM3D2API;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using UnityEngine;

//using System.Threading.Tasks;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ShapekeyMaster
{
	[BepInPlugin("ShapekeyMaster", "ShapekeyMaster", "1.4.4")]
	[BepInDependency("deathweasel.com3d2.api")]
	public class Main : BaseUnityPlugin
	{
		internal static Main @this;

		internal static BepInEx.Logging.ManualLogSource logger;

		public static bool enablegui { get; private set; } = false;

		private static SaveFileDialog FileSave = new SaveFileDialog();

		private static OpenFileDialog FileOpen = new OpenFileDialog();

		private static readonly string iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABwAAAAcCAYAAAByDd+UAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANRSURBVEhLzVZdK6RhGL6G8TFjFTOrEWnGiRgRocmhA2rbAzmwp+IfOPMLHDsnKaG2FA5WkhSJspPPGSTyEWYyFsn4Zp/rnvfVDLNrD2amverufZ73fue+nvvrmdsA4JOSz0psSqxKTEqSlMQCz0qCSn4p8Sk5UwJ7RUVF8+rqquclTpifn/9JDsXlIKGLLzRd3ECHyMWQflH7H2SONwwGw1fmyhzaJgQmEsaqQP4FSYkkE/xfhEtLS+jt7UVXVxdGRkZwcnKiaaLD7Xajr68PT09Pst/Z2UFPTw8WFxdlT/yRcG5uDmNjYwgEAsjNzcXm5iYGBgY07Xs8PDxgamoK+/v72N3dhcfjke+Pjo5wf3+vffUBodVqRXt7OxobG5GXl4fb21tN+x480M3NjaxnZ2cxPDwM1W5ITk6Ganp5Txi1p4CnGRoawvX1texpoLOzU9ZEWVmZtnoPhpPGzWYzDg8PkZ+fL/aKi4uRkZGhffWGMDU1FVVVVbLe2tqC3++H3W4XycrKQnl5ueiYS+bl+PhY1XkS6uvrJZSlpaUoKSmRtdFoFELdno6IkObk5MDhcOD09FS8SUlJgc/nE6LKykrxYHp6Gt3d3VJQFxcXsFgsWFtbk9/TOEl5gOXlZUlJYWGh6HREELKq+vv7cXBwgKKiIjQ1NeHu7g6jo6OiJ8nMzIyQtLW1oaOjQ/LL/IUbV/empIOHfIsIQhpOT09HS0uLeMvw0FPmhF7TO3pJfUFBgfxmfX1djIeHbmNjQ56Xl5dYWVmRtY5XQiqvrq7gdDqFTAcPQDC01LNaMzMz5R37jRWZlpYWUYksGB6MeWZUwvFaNCwYgoWgg+FkeNQtL71IsC/Zc8Tk5CTOz8/hcrlgMvF/O4S6ujqRaODfU7Pql+/cDA4OYnt7GzabTTzxer1CWltbi4aGBrlFWIF6mbN9+G1ra6t4+RHUwb9FENLAxMSEEDFc2dnZqKmpEUKCYR8fH8fe3p60CXNcXV0d0Wd/Awn5JGEEHh8fX4LBoLaLHcgV9WpjwsNzEktEJYwnSMhRLlF4JmHoik8MgiQ8U3ehN7SPHxYWFtzqwYEYDg6p8ZxNOZNqg7BdH/U54vMqsSjh2BirYmJ9MGUc8f0AAr8BwP7aKtdTkPoAAAAASUVORK5CYII=";

		private readonly List<string> DefaultBlacklistVals = new List<string>()
{
			"arml",
			"hara",
			"munel",
			"munes",
			"munetare",
			"regfat",
			"regmeet",
			"eyeclose",
			"eyeclose2",
			"eyeclose5",
			"eyeclose6",
			"eyeclose7",
			"eyeclose8",
			"fera1",
			"earelf",
			"earnone",
			"eyebig",
			"eyeclose1_normal",
			"eyeclose1_tare",
			"eyeclose1_tsuri",
			"eyeclose2_normal",
			"eyeclose2_tare",
			"eyeclose2_tsuri",
			"eyeclose3",
			"eyeclose5_normal",
			"eyeclose5_tare",
			"eyeclose5_tsuri",
			"eyeclose6_normal",
			"eyeclose6_tare",
			"eyeclose6_tsuri",
			"eyeclose7_normal",
			"eyeclose7_tare",
			"eyeclose7_tsuri",
			"eyeclose8_normal",
			"eyeclose8_tare",
			"eyeclose8_tsuri",
			"eyeeditl1_dw",
			"eyeeditl1_up",
			"eyeeditl2_dw",
			"eyeeditl2_up",
			"eyeeditl3_dw",
			"eyeeditl3_up",
			"eyeeditl4_dw",
			"eyeeditl4_up",
			"eyeeditl5_dw",
			"eyeeditl5_up",
			"eyeeditl6_dw",
			"eyeeditl6_up",
			"eyeeditl7_dw",
			"eyeeditl7_up",
			"eyeeditl8_dw",
			"eyeeditl8_up",
			"eyeeditr1_dw",
			"eyeeditr1_up",
			"eyeeditr2_dw",
			"eyeeditr2_up",
			"eyeeditr3_dw",
			"eyeeditr3_up",
			"eyeeditr4_dw",
			"eyeeditr4_up",
			"eyeeditr5_dw",
			"eyeeditr5_up",
			"eyeeditr6_dw",
			"eyeeditr6_up",
			"eyeeditr7_dw",
			"eyeeditr7_up",
			"eyeeditr8_dw",
			"eyeeditr8_up",
			"hitomih",
			"hitomis",
			"hoho",
			"hoho2",
			"hohol",
			"hohos",
			"mayueditin",
			"mayueditout",
			"mayuha",
			"mayuup",
			"mayuv",
			"mayuvhalf",
			"mayuw",
			"moutha",
			"mouthc",
			"mouthdw",
			"mouthfera",
			"mouthferar",
			"mouthhe",
			"mouthi",
			"mouths",
			"mouthup",
			"mouthuphalf",
			"namida",
			"nosefook",
			"shape",
			"shapehoho",
			"shapehohopushr",
			"shapeslim",
			"shock",
			"tangopen",
			"tangout",
			"tangup",
			"tear1",
			"tear2",
			"tear3",
			"toothoff",
			"uru-uru",
			"yodare"
		};

		internal static ConfigEntry<float> MaxDeform;
		internal static ConfigEntry<bool> SimpleMode;
		internal static ConfigEntry<bool> HideInactiveMaids;
		internal static ConfigEntry<bool> HotkeyEnabled;
		internal static ConfigEntry<int> EntriesPerPage;
		internal static ConfigEntry<KeyboardShortcut> Hotkey;
		internal static ConfigEntry<bool> Autosave;

		internal static bool BackgroundTasks = true;

		private void Awake()
		{
			@this = this;

			logger = Logger;

			MaxDeform = Config.Bind("General", "1. Max Deformation", 100f, "The max limit of the sliders in UI.");
			Autosave = Config.Bind("General", "2. Autosave", true, "Will the config be saved automatically at set points.");

			SimpleMode = Config.Bind("UI", "1. Simple Mode", true, "Simple mode is a simplified view of your shapekeys holding only the most basic of settings. All you really need in most cases.");
			HideInactiveMaids = Config.Bind("UI", "2. Hide Inactive Maids", false, "In the maids view, maids that are not present or loaded are hidden from the menu options.");
			EntriesPerPage = Config.Bind("UI", "3. Entries Per Page", 10, "How many entries to display per an entry page.");
			HotkeyEnabled = Config.Bind("Hotkey", "1. Enable Hotkey", false, "Use a hotkey to open ShapekeyMaster.");
			Hotkey = Config.Bind("Hotkey", "2. Hotkey", new KeyboardShortcut(KeyCode.F4, KeyCode.LeftControl), "Hotkey to open ShapekeyMaster with.");

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
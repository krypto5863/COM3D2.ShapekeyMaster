using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using COM3D2API;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeKeyMaster.GUI;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ShapeKeyMaster
{
	[BepInPlugin("ShapeKeyMaster", "ShapeKeyMaster", "1.6.2")]
	[BepInDependency("deathweasel.com3d2.api")]
	public class ShapeKeyMaster : BaseUnityPlugin
	{
		internal static ShapeKeyMaster instance { get; private set; }

		internal static ManualLogSource pluginLogger => instance.Logger;

		public static bool EnableGui { get; internal set; }

		private static readonly SaveFileDialog FileSave = new SaveFileDialog();
		private static readonly OpenFileDialog FileOpen = new OpenFileDialog();

		private const string IconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABwAAAAcCAYAAAByDd+UAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANRSURBVEhLzVZdK6RhGL6G8TFjFTOrEWnGiRgRocmhA2rbAzmwp+IfOPMLHDsnKaG2FA5WkhSJspPPGSTyEWYyFsn4Zp/rnvfVDLNrD2amverufZ73fue+nvvrmdsA4JOSz0psSqxKTEqSlMQCz0qCSn4p8Sk5UwJ7RUVF8+rqquclTpifn/9JDsXlIKGLLzRd3ECHyMWQflH7H2SONwwGw1fmyhzaJgQmEsaqQP4FSYkkE/xfhEtLS+jt7UVXVxdGRkZwcnKiaaLD7Xajr68PT09Pst/Z2UFPTw8WFxdlT/yRcG5uDmNjYwgEAsjNzcXm5iYGBgY07Xs8PDxgamoK+/v72N3dhcfjke+Pjo5wf3+vffUBodVqRXt7OxobG5GXl4fb21tN+x480M3NjaxnZ2cxPDwM1W5ITk6Ganp5Txi1p4CnGRoawvX1texpoLOzU9ZEWVmZtnoPhpPGzWYzDg8PkZ+fL/aKi4uRkZGhffWGMDU1FVVVVbLe2tqC3++H3W4XycrKQnl5ueiYS+bl+PhY1XkS6uvrJZSlpaUoKSmRtdFoFELdno6IkObk5MDhcOD09FS8SUlJgc/nE6LKykrxYHp6Gt3d3VJQFxcXsFgsWFtbk9/TOEl5gOXlZUlJYWGh6HREELKq+vv7cXBwgKKiIjQ1NeHu7g6jo6OiJ8nMzIyQtLW1oaOjQ/LL/IUbV/empIOHfIsIQhpOT09HS0uLeMvw0FPmhF7TO3pJfUFBgfxmfX1djIeHbmNjQ56Xl5dYWVmRtY5XQiqvrq7gdDqFTAcPQDC01LNaMzMz5R37jRWZlpYWUYksGB6MeWZUwvFaNCwYgoWgg+FkeNQtL71IsC/Zc8Tk5CTOz8/hcrlgMvF/O4S6ujqRaODfU7Pql+/cDA4OYnt7GzabTTzxer1CWltbi4aGBrlFWIF6mbN9+G1ra6t4+RHUwb9FENLAxMSEEDFc2dnZqKmpEUKCYR8fH8fe3p60CXNcXV0d0Wd/Awn5JGEEHh8fX4LBoLaLHcgV9WpjwsNzEktEJYwnSMhRLlF4JmHoik8MgiQ8U3ehN7SPHxYWFtzqwYEYDg6p8ZxNOZNqg7BdH/U54vMqsSjh2BirYmJ9MGUc8f0AAr8BwP7aKtdTkPoAAAAASUVORK5CYII=";

		internal static ConfigEntry<float> MaxDeform;
		internal static ConfigEntry<bool> AutoSave;

		internal static ConfigEntry<float> UpdateDelay;

		internal static ConfigEntry<bool> SimpleMode;
		internal static ConfigEntry<bool> HideInactiveMaids;
		internal static ConfigEntry<int> EntriesPerPage;
		internal static ConfigEntry<string> Language;

		internal static ConfigEntry<bool> HotKeyEnabled;
		internal static ConfigEntry<KeyboardShortcut> HotKey;

		internal static TranslationResource CurrentLanguage { get; private set; }

		private void Awake()
		{
			instance = this;

			if (Directory.Exists(Paths.ConfigPath + "\\ShapekeyMaster\\") == false)
			{
				pluginLogger.LogFatal("It seems we're lacking any translation folder for ShapeKeyMaster! instance is bad and we can't start without that and the translation files! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			var translationFiles = Directory.GetFiles(Paths.ConfigPath + "\\ShapekeyMaster\\", "*.*")
				.Where(s => s.ToLower().EndsWith(".json"))
				.Select(Path.GetFileName)
				.ToArray();

			if (!translationFiles.Any())
			{
				pluginLogger.LogFatal("It seems we're lacking any translation files for ShapeKeyMaster! instance is bad and we can't start without them! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			var acceptableValues = new AcceptableValueList<string>(translationFiles);

			MaxDeform = Config.Bind("General", "1. Max Deformation", 100f, "The max limit of the sliders in UI.");
			AutoSave = Config.Bind("General", "2. AutoSave", true, "Will the config be saved automatically at set points.");
			UpdateDelay = Config.Bind("General", "3. Update Delay", 0.1f, "How long keys take to update when you move the sliders in seconds. The lower the smoother.");

			SimpleMode = Config.Bind("UI", "1. Simple Mode", true, "Simple mode is a simplified view of your shape keys holding only the most basic of settings. All you really need in most cases.");
			HideInactiveMaids = Config.Bind("UI", "2. Hide Inactive Maids", false, "In the maids view, maids that are not present or loaded are hidden from the menu options.");
			EntriesPerPage = Config.Bind("UI", "3. Entries Per Page", 10, "How many entries to display per an entry page.");
			Language = Config.Bind("UI", "4. Language", "english.json", new ConfigDescription("The language for SKM's UI.", acceptableValues));

			Language.SettingChanged += (e, s) =>
			{
				CurrentLanguage = new TranslationResource(Paths.ConfigPath + "\\ShapekeyMaster\\" + Language.Value);
			};

			CurrentLanguage = new TranslationResource(Paths.ConfigPath + "\\ShapekeyMaster\\" + Language.Value);

			HotKeyEnabled = Config.Bind("HotKey", "1. Enable HotKey", false, "Use a hotKey to open ShapekeyMaster.");
			HotKey = Config.Bind("HotKey", "2. HotKey", new KeyboardShortcut(KeyCode.F4, KeyCode.LeftControl, KeyCode.LeftAlt), "HotKey to open ShapekeyMaster with.");

			Harmony.CreateAndPatchAll(typeof(HarmonyPatchers));

			if (File.Exists(Paths.ConfigPath + "\\ShapekeyMaster.json"))
			{
				var serDb = LoadFromJson(Paths.ConfigPath + "\\ShapekeyMaster.json");
				Ui.SkDatabase.AllShapekeyDictionary = serDb.AllShapekeyDictionary;
				Ui.SkDatabase.BlacklistedShapeKeys = serDb.BlacklistedShapeKeys;
			}

			SystemShortcutAPI.AddButton("ShapekeyMaster", () =>
			{
				EnableGui = !EnableGui;

				if (AutoSave.Value)
				{
					SaveToJson(Paths.ConfigPath + "\\ShapekeyMaster.json", Ui.SkDatabase);
				}
			}, "ShapekeyMaster", Convert.FromBase64String(IconBase64));

			Logger.LogInfo("ShapekeyMaster is online!");
		}

		private void Update()
		{
			if (HotKeyEnabled.Value && HotKey.Value.IsDown())
			{
				EnableGui = !EnableGui;
			}
		}

		private void OnGUI()
		{
			if (EnableGui)
			{
				Ui.Initialize();
			}
		}

		private void OnDestroy()
		{
			if (AutoSave.Value)
			{
				SaveToJson(Paths.ConfigPath + "\\ShapekeyMaster.json", Ui.SkDatabase);
			}
		}

		internal static void SaveToJson(string path, ShapeKeyDatabase database, bool withPrompt = false)
		{
			if (withPrompt)
			{
				FileSave.Filter = "json files (*.json)|*.json";
				FileSave.InitialDirectory = Paths.GameRootPath;
				FileSave.ShowDialog();

				if (!string.IsNullOrEmpty(FileSave.FileName))
				{
					path = FileSave.FileName;
				}
			}

			if (!string.IsNullOrEmpty(path))
			{
				File.WriteAllText(path, JsonConvert.SerializeObject(database, Formatting.Indented));
			}
		}

		internal static ShapeKeyDatabase LoadFromJson(string path, bool withPrompt = false)
		{
			if (withPrompt)
			{
				FileOpen.Filter = "json files (*.json)|*.json";
				FileOpen.InitialDirectory = Paths.GameRootPath;
				FileOpen.ShowDialog();

				if (!string.IsNullOrEmpty(FileOpen.FileName))
				{
					path = FileOpen.FileName;
				}
			}

			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return null;
			}

			var mConfig = File.ReadAllText(path);

			// Parse the JSON
			var jsonObject = JObject.Parse(mConfig);

			// Get the array of objects from the JSON
			var classCollection = (JObject)jsonObject["AllShapekeyDictionary"];

			// Flag to track if any conversion occurred
			var converted = false;

			// Iterate through each object in the array
			foreach (var obj in classCollection.Properties())
			{
				// Check if the property is a boolean
				if (obj.Value["Enabled"] != null && obj.Value["Enabled"].Type == JTokenType.Boolean)
				{
					// Convert the boolean value to an integer
					var oldValue = obj.Value["Enabled"].Value<bool>();
					var newValue = oldValue ? 2 : 1;

					// Update the property value
					obj.Value["Enabled"] = newValue;

					converted = true;
				}
			}

			// If any conversion occurred, save the updated JSON to a new file
			if (converted)
			{
				// Serialize the updated JSON and save it to a new file
				//string updatedJson = jsonObject.ToString();
				//File.WriteAllText(updatedJsonFilePath, updatedJson);

				pluginLogger.LogInfo("Conversion completed successfully.");
			}

			var serializeSet = new JsonSerializerSettings
			{
				ObjectCreationHandling = ObjectCreationHandling.Replace
			};

			
			return jsonObject.ToObject<ShapeKeyDatabase>();
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
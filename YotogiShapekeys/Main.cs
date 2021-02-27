using BepInEx;
using COM3D2API;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ShapekeyMaster
{
	[BepInPlugin("ShapekeyMaster", "ShapekeyMaster", "1.0.0.0")]
	public class Main : BaseUnityPlugin
	{
		public static Main @this;

		private static bool enablegui = false;

		private static readonly string iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABwAAAAcCAYAAAByDd+UAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANRSURBVEhLzVZdK6RhGL6G8TFjFTOrEWnGiRgRocmhA2rbAzmwp+IfOPMLHDsnKaG2FA5WkhSJspPPGSTyEWYyFsn4Zp/rnvfVDLNrD2amverufZ73fue+nvvrmdsA4JOSz0psSqxKTEqSlMQCz0qCSn4p8Sk5UwJ7RUVF8+rqquclTpifn/9JDsXlIKGLLzRd3ECHyMWQflH7H2SONwwGw1fmyhzaJgQmEsaqQP4FSYkkE/xfhEtLS+jt7UVXVxdGRkZwcnKiaaLD7Xajr68PT09Pst/Z2UFPTw8WFxdlT/yRcG5uDmNjYwgEAsjNzcXm5iYGBgY07Xs8PDxgamoK+/v72N3dhcfjke+Pjo5wf3+vffUBodVqRXt7OxobG5GXl4fb21tN+x480M3NjaxnZ2cxPDwM1W5ITk6Ganp5Txi1p4CnGRoawvX1texpoLOzU9ZEWVmZtnoPhpPGzWYzDg8PkZ+fL/aKi4uRkZGhffWGMDU1FVVVVbLe2tqC3++H3W4XycrKQnl5ueiYS+bl+PhY1XkS6uvrJZSlpaUoKSmRtdFoFELdno6IkObk5MDhcOD09FS8SUlJgc/nE6LKykrxYHp6Gt3d3VJQFxcXsFgsWFtbk9/TOEl5gOXlZUlJYWGh6HREELKq+vv7cXBwgKKiIjQ1NeHu7g6jo6OiJ8nMzIyQtLW1oaOjQ/LL/IUbV/empIOHfIsIQhpOT09HS0uLeMvw0FPmhF7TO3pJfUFBgfxmfX1djIeHbmNjQ56Xl5dYWVmRtY5XQiqvrq7gdDqFTAcPQDC01LNaMzMz5R37jRWZlpYWUYksGB6MeWZUwvFaNCwYgoWgg+FkeNQtL71IsC/Zc8Tk5CTOz8/hcrlgMvF/O4S6ujqRaODfU7Pql+/cDA4OYnt7GzabTTzxer1CWltbi4aGBrlFWIF6mbN9+G1ra6t4+RHUwb9FENLAxMSEEDFc2dnZqKmpEUKCYR8fH8fe3p60CXNcXV0d0Wd/Awn5JGEEHh8fX4LBoLaLHcgV9WpjwsNzEktEJYwnSMhRLlF4JmHoik8MgiQ8U3ehN7SPHxYWFtzqwYEYDg6p8ZxNOZNqg7BdH/U54vMqsSjh2BirYmJ9MGUc8f0AAr8BwP7aKtdTkPoAAAAASUVORK5CYII=";

		public static bool BackgroundTasks = true;

		private void Awake()
		{
			Harmony.CreateAndPatchAll(typeof(HarmonyPatchers));

			@this = this;

			if (File.Exists(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json"))
			{
				string mconfig = (File.ReadAllText(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json"));
				/*
				var jsonContracter = new JsonSerializerSettings();

				jsonContracter.ContractResolver = new PrivateSettersContractResolver();*/

				UI.ShapeKeys = JsonConvert.DeserializeObject<SortedDictionary<int, ShapeKeyEntry>>(
				mconfig);

				UI.availID = UI.ShapeKeys.Last().Key;
			}

			SystemShortcutAPI.AddButton("ShapekeyMaster", () =>
			{
				enablegui = !enablegui;

				if (UI.ShapeKeys.Count > 0)
				{
					File.WriteAllText(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", JsonConvert.SerializeObject(UI.ShapeKeys));
				}
			}, "Open/Close GUI", Convert.FromBase64String(iconBase64));

			//StartCoroutine(ShapekeyFetcherSetter.TaskQueuer());

			Debug.Log("ShapekeyMaster is online!");

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
			if (UI.ShapeKeys.Count > 0)
			{
				File.WriteAllText(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", JsonConvert.SerializeObject(UI.ShapeKeys));
			}
		}
	}
}

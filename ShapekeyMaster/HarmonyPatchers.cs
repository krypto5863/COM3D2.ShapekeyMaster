using HarmonyLib;
using MaidStatus;
using ShapeKeyMaster.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using static ShapeKeyMaster.SmEventsAndArgs;

namespace ShapeKeyMaster
{
	internal class HarmonyPatchers
	{
		public static event EventHandler MorphEvent;

		public static event EventHandler ExcitementChange;

		public static event EventHandler ClothingMaskChange;

		private static readonly List<string> YotogiLvls = new List<string> { "SceneYotogi", "SceneYotogiOld" };

		private static int _slowCounter;

		private static float[] _previousBlends;

		[HarmonyPatch(typeof(TMorph), MethodType.Constructor, typeof(TBodySkin))]
		[HarmonyPostfix]
		private static void NotifyOfLoad(ref TMorph __instance)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Morph Creation Event @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category}");
#endif
			ShapeKeyUpdate.ListOfActiveMorphs.Add(__instance);
			MorphEvent?.Invoke(null, new MorphEventArgs(__instance));
		}

		//Now that we're done naming event classes, we can put out events to work...
		//Do not use Finalize. Kiss orphans TMorphs.
		[HarmonyPatch(typeof(TMorph), "DeleteObj")]
		[HarmonyPostfix]
		private static void NotifyOfUnLoad(ref TMorph __instance)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Morph Deletion Event @ {__instance.Category}");
#endif
			ShapeKeyUpdate.ListOfActiveMorphs.Remove(__instance);

			MorphEvent?.Invoke(null, new MorphEventArgs(__instance, false));
		}

		[HarmonyPatch(typeof(Maid), "AllProcProp")]
		[HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
		[HarmonyPostfix]
		private static void NotifyOfMenuLoad(ref Maid __instance)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Menu Change Event @ {__instance.status.fullNameJpStyle}");
#endif
			ShapeKeyUpdate.UpdateKeys(__instance.status.fullNameJpStyle);
		}

		[HarmonyPatch(typeof(TBody), "FixMaskFlag")]
		[HarmonyPostfix]
		private static void NotifyOfClothesMask(ref TBody __instance)
		{
#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Mask Change Event @ {__instance.maid.status.fullNameJpStyle}");
#endif
			ClothingMaskChange?.Invoke(null, new ClothingMaskChangeEvent(__instance.maid.status.fullNameJpStyle));
		}

		[HarmonyPatch(typeof(TMorph), "FixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixFixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixBlendValues_Face")]
		[HarmonyPrefix]
		private static void VerifyBlendValuesBeforeSet(ref TMorph __instance)
		{
#if DEBUG
			ShapeKeyMaster.pluginLogger.LogDebug($"Cheking Morph Dic @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category}\n{System.Environment.StackTrace}");
#endif


			_previousBlends = __instance.BlendValues;

			if (!Ui.SkDatabase.MorphShapekeyDictionary().ContainsKey(__instance))
			{
				return;
			}

			var watch = new Stopwatch();

			watch.Start();

#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Iterating Entries @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category}");
#endif

			foreach (var shapeKeyEntry in Ui.SkDatabase.ShapeKeysByMorph(__instance))
			{
				if (!__instance.hash.ContainsKey(shapeKeyEntry.ShapeKey))
				{
					continue;
				}

				var index = (int)__instance.hash[shapeKeyEntry.ShapeKey];

				if (!shapeKeyEntry.Enabled)
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Disabling Key @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category} ::: {shapeKeyEntry.EntryName} :::: {shapeKeyEntry.ShapeKey}");
#endif
					__instance.BlendValues[index] = 0;
					__instance.BlendValuesBackup[index] = 0;
				}
				else if (shapeKeyEntry.ConditionalsToggle && __instance.bodyskin != null && __instance.bodyskin.body && __instance.bodyskin.body.maid && SlotChecker.CheckIfSlotDisableApplies(shapeKeyEntry, __instance.bodyskin.body.maid))
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Applying Conditional State @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category} ::: {shapeKeyEntry.EntryName} :::: {shapeKeyEntry.ShapeKey}");
#endif

					__instance.BlendValues[index] = shapeKeyEntry.DisabledDeform / 100;
					__instance.BlendValuesBackup[index] = shapeKeyEntry.DisabledDeform / 100;
				}
				else if (shapeKeyEntry.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Applying Excitement Deformation @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category} ::: {shapeKeyEntry.EntryName} :::: {shapeKeyEntry.ShapeKey}");
#endif

					var deform = shapeKeyEntry.CalculateExcitementDeformation();

					__instance.BlendValues[index] = deform / 100;
					__instance.BlendValuesBackup[index] = deform / 100;
				}
				else
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug($"Applying Standard Deform @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category} ::: {shapeKeyEntry.EntryName} :::: {shapeKeyEntry.ShapeKey}");
#endif

					__instance.BlendValues[index] = shapeKeyEntry.Deform / 100;
					__instance.BlendValuesBackup[index] = shapeKeyEntry.Deform / 100;
				}
			}

			watch.Stop();
			if (watch.ElapsedMilliseconds > 5)
			{
				if (++_slowCounter < 10)
				{
					return;
				}

				ShapeKeyMaster.pluginLogger.LogWarning(
					$"ShapekeyMaster's function finished slowly 10 times or more! instance last time it finished in {watch.ElapsedMilliseconds} ms. instance is really high for ShapekeyMaster, if you see this message then there could be an issue!");

				_slowCounter = 0;
			}
			else
			{
				_slowCounter = 0;
			}

#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"Update precheck done @ {__instance.bodyskin.body.maid.status.fullNameJpStyle} :: {__instance.Category}");
#endif
		}

		[HarmonyPatch(typeof(TMorph), "FixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixFixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixBlendValues_Face")]
		[HarmonyPostfix]
		private static void ReturnBlendFiles(ref TMorph __instance)
		{
			__instance.BlendValues = _previousBlends;
		}

		[HarmonyPatch(typeof(Status), "currentExcite", MethodType.Setter)]
		[HarmonyPostfix]
		public static void OnExciteSet(ref Status __instance, ref int __0)
		{
			ExcitementChange?.Invoke(null, new ExcitementChangeEvent(__instance.fullNameJpStyle, __0));

#if (DEBUG)
			ShapeKeyMaster.pluginLogger.LogDebug($"{__instance.fullNameJpStyle}'s excitement changed to {__instance.currentExcite}! Making changes...");
#endif
		}

		[HarmonyPatch(typeof(UltimateOrbitCamera), "Update")]
		[HarmonyPrefix]
		public static bool InputCheck()
		{
			return !UiToolbox.IsMouseOnGUI();
		}
	}
}
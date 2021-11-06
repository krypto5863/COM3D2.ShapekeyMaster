using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using static ShapekeyMaster.SMEventsAndArgs;

namespace ShapekeyMaster
{
	class HarmonyPatchers
	{
		public static event EventHandler MorphEvent;
		public static event EventHandler ExcitementChange;
		public static event EventHandler ClothingMaskChange;

		private static readonly List<string> YotogiLvls = new List<string>() { "SceneYotogi", "SceneYotogiOld" };

		private static int SlowCounter = 0;

		[HarmonyPatch(typeof(TMorph), MethodType.Constructor, new Type[] { typeof(TBodySkin) })]
		[HarmonyPostfix]
		private static void NotifyOfLoad(ref TMorph __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up a morph creation!");
#endif
			ShapekeyUpdate.ListOfActiveMorphs.Add(__instance);
			if (MorphEvent != null)
			{
				MorphEvent.Invoke(null, new MorphEventArgs(__instance));
			}
		}

		//Now that we're done naming event classes, we can put out events to work...

		[HarmonyPatch(typeof(TMorph), "DeleteObj")]
		[HarmonyPostfix]
		private static void NotifyOfUnLoad(ref TMorph __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up a morph deletion!");
#endif
			ShapekeyUpdate.ListOfActiveMorphs.Remove(__instance);

			if (MorphEvent != null)
				{
				MorphEvent.Invoke(null, new MorphEventArgs(__instance, false));
			}
		}

		[HarmonyPatch(typeof(Maid), "AllProcProp")]
		[HarmonyPatch(typeof(Maid), "AllProcPropSeq")]
		[HarmonyPostfix]
		private static void NotifyOfMenuLoad(ref Maid __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up a menu change!");
#endif
			ShapekeyUpdate.UpdateKeys(__instance.status.fullNameJpStyle);
		}

		[HarmonyPatch(typeof(TBody), "FixMaskFlag")]
		[HarmonyPostfix]
		private static void NotifyOfClothesMask(ref TBody __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up mask change!");
#endif
			if (ClothingMaskChange != null)
			{
				ClothingMaskChange.Invoke(null, new ClothingMaskChangeEvent(__instance.maid.status.fullNameJpStyle));
			}
		}

		[HarmonyPatch(typeof(TMorph), "FixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixFixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixBlendValues_Face")]
		[HarmonyPrefix]
		private static void VerifyBlendValuesBeforeSet(ref TMorph __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("Check morph dic for instance...");
#endif
			if (UI.SKDatabase.MorphShapekeyDictionary().ContainsKey(__instance))
			{
				Stopwatch watch = new Stopwatch();

				watch.Start();
#if (DEBUG)
				Main.logger.LogDebug("Starting set check...");
#endif
				foreach (ShapeKeyEntry shapeKeyEntry in UI.SKDatabase.ShapekeysByMorph(__instance))
				{
#if (DEBUG)
					Main.logger.LogDebug($"Checking hashset for key {shapeKeyEntry.ShapeKey}");
#endif

					if (!__instance.hash.ContainsKey(shapeKeyEntry.ShapeKey))
					{
						continue;
					}
#if (DEBUG)
					Main.logger.LogDebug("Getting key index...");
#endif
					var index = (int)__instance.hash[shapeKeyEntry.ShapeKey];

#if (DEBUG)
					Main.logger.LogDebug("Running actual checks...");
#endif

					if (!shapeKeyEntry.Enabled)
					{
#if (DEBUG)
						Main.logger.LogDebug("Disabling key");
#endif
						__instance.BlendValues[index] = 0;
						__instance.BlendValuesBackup[index] = 0;
					}
					else if (shapeKeyEntry.ConditionalsToggle && __instance.bodyskin != null && __instance.bodyskin.body && __instance.bodyskin.body.maid && SlotChecker.CheckIfSlotDisableApplies(shapeKeyEntry, __instance.bodyskin.body.maid))
					{
#if (DEBUG)
						Main.logger.LogDebug("Checking conditionals.");
#endif

						__instance.BlendValues[index] = shapeKeyEntry.DisabledDeform / 100;
						__instance.BlendValuesBackup[index] = shapeKeyEntry.DisabledDeform / 100;
					}
					else if (shapeKeyEntry.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
					{
#if (DEBUG)
						Main.logger.LogDebug("Calcing excitement deform");
#endif

						var deform = HelperClasses.CalculateExcitementDeformation(shapeKeyEntry);

						__instance.BlendValues[index] = deform / 100;
						__instance.BlendValuesBackup[index] = deform / 100;
					}
					else
					{
#if (DEBUG)
						Main.logger.LogDebug("Doing basic deform.");
#endif

						__instance.BlendValues[index] = shapeKeyEntry.Deform / 100;
						__instance.BlendValuesBackup[index] = shapeKeyEntry.Deform / 100;
					}
				}

				watch.Stop();
				if (watch.ElapsedMilliseconds > 5)
				{
					if (++SlowCounter >= 10)
					{
						Main.logger.LogWarning($"ShapekeyMaster's function finished slowly 10 times or more! This last time it finished in {watch.ElapsedMilliseconds} ms. This is really high for ShapekeyMaster, if you see this message then there could be an issue!");

						SlowCounter = 0;
					}
				}
				else 
				{
					SlowCounter = 0;
				}
				watch = null;
			}
#if (DEBUG)
			Main.logger.LogDebug("Update precheck done.");
#endif
		}

		//Patches the setter of the FPS value to run below code afterwards	
		[HarmonyPatch(typeof(MaidStatus.Status), "currentExcite", MethodType.Setter)]
		[HarmonyPostfix]
		public static void OnExciteSet(ref MaidStatus.Status __instance, ref int __0)
		{
			if (ExcitementChange != null)
			{
				ExcitementChange.Invoke(null, new ExcitementChangeEvent(__instance.fullNameJpStyle, __0));

#if (DEBUG)
				Main.logger.LogDebug($"{__instance.fullNameJpStyle }'s excitement changed to {__instance.currentExcite}! Making changes...");
#endif
			}
		}
	}
}

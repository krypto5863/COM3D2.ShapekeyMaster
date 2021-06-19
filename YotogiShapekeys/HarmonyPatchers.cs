using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace ShapekeyMaster
{
	class HarmonyPatchers
	{
		private static readonly List<string> YotogiLvls = new List<string>() { "SceneYotogi", "SceneYotogiOld" };

		[HarmonyPatch(typeof(TMorph), MethodType.Constructor, new Type[] {typeof(TBodySkin)})]
		[HarmonyPostfix]
		private static void NotifyOfLoad(ref TMorph __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up a morph creation!");
#endif

			ShapekeyUpdate.ListOfActiveMorphs.Add(__instance);
		}

		[HarmonyPatch(typeof(TMorph), "DeleteObj")]
		[HarmonyPostfix]
		private static void NotifyOfUnLoad(ref TMorph __instance)
		{
#if (DEBUG)
			Main.logger.LogDebug("ShapekeyMaster picked up a morph deletion!");
#endif

			ShapekeyUpdate.ListOfActiveMorphs.Remove(__instance);
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

			ShapekeyUpdate.UpdateKeys(__instance.maid.status.fullNameJpStyle);
		}

		[HarmonyPatch(typeof(TMorph), "FixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixFixBlendValues")]
		[HarmonyPatch(typeof(TMorph), "FixBlendValues_Face")]
		[HarmonyPrefix]
		private static void VerifyBlendValuesBeforeSet(ref TMorph __instance) 
		{
			Stopwatch watch = new Stopwatch();

			watch.Start();

			var MaidName = __instance.bodyskin.body.maid.status.fullNameJpStyle;

			if (__instance.hash == null) 
			{
				return;
			}

			foreach (ShapeKeyEntry shapeKeyEntry in UI.SKDatabase.ShapekeysByMaid(MaidName).Values.Concat(UI.SKDatabase.GlobalShapekeyDictionary().Values)) 
			{
				if (__instance.hash.ContainsKey(shapeKeyEntry.ShapeKey)) 
				{
					var index = (int)__instance.hash[shapeKeyEntry.ShapeKey];

					if (!shapeKeyEntry.Enabled)
					{
						__instance.BlendValues[index] = 0;
						__instance.BlendValuesBackup[index] = 0;
					}
					else if (shapeKeyEntry.ConditionalsToggle && SlotChecker.CheckIfSlotDisableApplies(shapeKeyEntry, __instance.bodyskin.body.maid))
					{
						__instance.BlendValues[index] = shapeKeyEntry.DisabledDeform / 100;
						__instance.BlendValuesBackup[index] = shapeKeyEntry.DisabledDeform / 100;
					}
					else if (shapeKeyEntry.AnimateWithExcitement && YotogiLvls.Contains(SceneManager.GetActiveScene().name))
					{
						var deform = HelperClasses.CalculateExcitementDeformation(shapeKeyEntry);

						__instance.BlendValues[index] = deform / 100;
						__instance.BlendValuesBackup[index] = deform / 100;
					}
					else
					{
						__instance.BlendValues[index] = shapeKeyEntry.Deform / 100;
						__instance.BlendValuesBackup[index] = shapeKeyEntry.Deform / 100;
					}
				}
			}

			watch.Stop();
			if (watch.ElapsedMilliseconds > 5) {
				Main.logger.LogWarning($"ShapekeyMaster's function finished in {watch.ElapsedMilliseconds} ms. This is really high for ShapekeyMaster and should be looked into...");
			}
		}

		//Patches the setter of the FPS value to run below code afterwards	
		[HarmonyPatch(typeof(MaidStatus.Status), "currentExcite", MethodType.Setter)]
		[HarmonyPostfix]
		public static void OnExciteSet(ref MaidStatus.Status __instance, ref int __0)
		{
			ShapekeyUpdate.UpdateKeys();
#if (DEBUG)
			Main.logger.LogDebug($"{__instance.fullNameJpStyle }'s excitement changed to {__instance.currentExcite}! Making changes...");
#endif
		}
	}
}

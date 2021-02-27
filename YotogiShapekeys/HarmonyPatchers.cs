using HarmonyLib;
using UnityEngine;

namespace ShapekeyMaster
{
	internal class HarmonyPatchers
	{
		[HarmonyPatch(typeof(ImportCM), "LoadSkinMesh_R")]
		[HarmonyPostfix]
		private static void NotifyOfLoad2(ref TMorph __1)
		{
#if (DEBUG)
			Debug.Log("ShapekeyMaster picked up a maid load! Registering maid...");
#endif
			ShapekeyFetcherSetter.RegisterMaid(__1.bodyskin.body.maid);

		}
		//A bit of a shotgun solution but basically it makes it so that our plugin has ultimate say so of what blend values are changed to what. Might not be the most compatible in the long run so a more friendly solution should eventually be seeked out.
		[HarmonyPatch(typeof(TMorph), "SetBlendValues")]
		[HarmonyPrefix]
		private static bool Intercede(ref int __0, ref float __1, ref TMorph __instance)
		{
			if (__1 > 9999f)
			{
#if (DEBUG)
				Debug.Log($"ShapekeyMaster set a value of { __1 - 10000 }!");
#endif
				__1 -= 10000f;
			}
			else
			{
				foreach (ShapeKeyEntry s in UI.ShapeKeys.Values)
				{
					if (s.Enabled && __instance.hash.ContainsKey(s.ShapeKey) && (int)__instance.hash[s.ShapeKey] == __0)
					{
						if (s.Maid == "")
						{
#if (DEBUG)
							Debug.Log("ShapekeyMaster noticed a value not set by it.. Discarding change...");
#endif

							return false;
						} else if (s.Maid == __instance.bodyskin.body.maid.status.fullNameJpStyle)
						{
#if (DEBUG)
							Debug.Log("ShapekeyMaster noticed a value not set by it. Discarding change...");
#endif

							return false;
						}
					}
				}
			}
			return true;
		}
		[HarmonyPatch(typeof(TMorph), "ResetBlendValues")]
		[HarmonyPostfix]
		private static void Intercede1(ref TMorph __instance)
		{
			Maid maid = __instance.bodyskin.body.maid;
			ShapekeyFetcherSetter.RegisterMaid(maid);
		}
		//Patches the setter of the FPS value to run below code afterwards	
		[HarmonyPatch(typeof(MaidStatus.Status), "currentExcite", MethodType.Setter)]
		[HarmonyPostfix]
		public static void OnExciteSet(ref MaidStatus.Status __instance)
		{
			ShapekeyFetcherSetter.MissionControlAll();

			#if (DEBUG)

			Debug.Log($"{__instance.fullNameJpStyle }'s excitement changed to {__instance.currentExcite}! Making changes...");
			#endif
		}
		//Orgasm was detected!
		/*
		[HarmonyPatch(typeof(YotogiPlayManager), "PlayNormalClimax")]
		[HarmonyPatch(typeof(YotogiOldPlayManager), "PlayNormalClimax")]
		[HarmonyPostfix]
		public static void OnOrgasm(string __0)
		{
				Main.@this.StartCoroutine(ShapekeyFetcherSetter.OrgasmAnimator());

				Debug.Log($"Somebody seems to be having an orgasm!");
		}*/
	}
}

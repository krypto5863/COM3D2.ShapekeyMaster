using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapekeyMaster
{
	internal static class HelperClasses
	{
		public static bool HasFlag(this Enum variable, Enum value)
		{
			// check if from the same type.
			if (variable.GetType() != value.GetType())
			{
				throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
			}

			Convert.ToUInt64(value);
			ulong num = Convert.ToUInt64(value);
			ulong num2 = Convert.ToUInt64(variable);

			return (num2 & num) == num;
		}
		public static bool IsMaidActive(string name)
		{

			if (name == "")
			{

				bool result =
					GameMain.Instance.CharacterMgr
					.GetStockMaidList()
					.Where(m => m != null)
					.Count() > 0;

#if (DEBUG)

				Main.logger.LogDebug("Maids active to edit = " + result);
#endif

				return result;
			}

			return
				GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.Where(m => m != null && m.isActiveAndEnabled && m.status.fullNameJpStyle == name)
				.Count() > 0;
		}
		public static Maid GetMaidByName(string name)
		{
			if (name == "")
			{
				return null;
			}

			return
				GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.FirstOrDefault(m => m != null && m.isActiveAndEnabled && m.status.fullNameJpStyle == name);
		}
		public static IEnumerable<TMorph> GetAllMorphsFromMaidList(List<Maid> list)
		{
			return
				list
				.Select(m => GetAllMorphsFromMaid(m))
				.SelectMany(tm => tm);
		}
		public static IEnumerable<TMorph> GetAllMorphsFromMaid(Maid maid)
		{
			return
				maid
				.body0
				.goSlot
				.Concat(new[] { maid.body0.Face })
				.Where(s => s != null)
				.Select(s => s.morph)
				.Where(o => o != null);
		}
		public static IEnumerable<string> GetAllShapeKeysFromMaidList(List<Maid> maids)
		{
			return
				maids
				.Select(m => GetAllShapeKeysFromMaid(m))
				.SelectMany(skl => skl)
				.Distinct();
		}
		public static IEnumerable<string> GetAllShapeKeysFromMaid(Maid maid)
		{
			if (maid == null) 
			{
				return null;
			}

			return
				GetAllMorphsFromMaid(maid)
				.Select(m => m.hash)
				.Select(h => h.Keys)
				.Select(k => k.Cast<string>())
				.SelectMany(k => k)
				.Distinct();
		}
		public static IEnumerable<string> GetAllShapeKeysFromAllMaids() 
		{
			var result = GetAllShapeKeysFromMaidList(GameMain.Instance.CharacterMgr
				.GetStockMaidList().Where(m => m.isActiveAndEnabled).ToList()).ToList();

			result.Sort();

			return result;
		}
		public static bool IsFaceKey(string Keyname) 
		{
			return GameMain.Instance.CharacterMgr.GetStockMaidList()
			.Where(maid => maid != null && maid.body0 != null && maid.body0.Face != null && maid.body0.Face.morph != null)
			.Select(m => m.body0.Face.morph)
			.Where(mr => mr.Contains(Keyname))
			.Count() > 0;
		}
		public static IEnumerable<string> GetNameOfAllMaids()
		{
			var result = GameMain.Instance.CharacterMgr.GetStockMaidList().Select(m => m.status.fullNameJpStyle).ToList();

			result.Sort();

			return result;
		}
		public static IEnumerable<string> GetAllBlendShapes(Mesh mesh)
		{

#if (DEBUG)
			Main.logger.LogDebug($"Was called, getting all blendshapes in {mesh.name}, there should be {mesh.blendShapeCount} blendshapes.");
#endif
			string blendshape;

			List<string> res = new List<string>();

			int shapecount = 0;

			for (int i = 0; mesh.blendShapeCount > i && shapecount < mesh.blendShapeCount; i++)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Found blendshape with name {mesh.GetBlendShapeName(i)}");
#endif

				if ((blendshape = mesh.GetBlendShapeName(i)) != null)
				{
					res.Add(blendshape);
					++shapecount;
				}
			}

			return res;
		}
		public static float CalculateExcitementDeformation(ShapeKeyEntry sk)
		{
			float excitement = GetHighestExcitement();
#if (DEBUG)
			Main.logger.LogDebug($"Highest excitement found was {excitement}");
#endif
			float percent;

			if (excitement >= sk.ExcitementMax)
			{
				percent = ((sk.ExcitementMax - sk.ExcitementMin) / 300);
			}
			else
			{
				percent = ((excitement - sk.ExcitementMin) / 300);
			}

			float result = (sk.DeformMax - sk.DeformMin) * percent;
#if (DEBUG)
			Main.logger.LogDebug($"Yotogi settings were found! Returning a value of : {result + sk.DeformMin}");
#endif
			return (result + sk.DeformMin);
		}
		public static float GetHighestExcitement()
		{
			var maids = GameMain.instance.CharacterMgr.GetStockMaidList()
			.Where(m => m != null && m.isActiveAndEnabled)
			.ToList();

			if (maids.Count() > 0)
			{
#if (DEBUG)
				Main.logger.LogDebug($"Getting highest excitement! We have to query results from this many maids: {maids.Count()}");
#endif

				return
					maids
					.Select(m => m.status.currentExcite)
					.Max();
			}
			return 0;
		}
	}
}

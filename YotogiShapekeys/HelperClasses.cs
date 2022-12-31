using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShapekeyMaster
{
	public class Tuple<T1, T2>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }

		internal Tuple(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}

	public static class Tuple
	{
		public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
		{
			var tuple = new Tuple<T1, T2>(first, second);
			return tuple;
		}
	}

	internal static class HelperClasses
	{
		private static readonly FieldInfo goSlotField = typeof(TBody).GetField("goSlot", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

		private static readonly Type goSlotType = goSlotField.FieldType;

		private static readonly MethodInfo goSlotMethod = goSlotType.GetMethod("GetListParents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public) ?? null;

		public static List<TBodySkin> FetchGoSlot(TBody body)
		{
			List<TBodySkin> result;

			if (goSlotField.FieldType == typeof(List<TBodySkin>))
			{
				result = (List<TBodySkin>)goSlotField.GetValue(body);
			}
			else
			{
				var goSlotVal = goSlotField.GetValue(body);

				result = (List<TBodySkin>)goSlotMethod.Invoke(goSlotVal, new object[0]);
			}

			if (result != null)
			{
				return result;
			}

			return null;
		}

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
				.FirstOrDefault(m => m != null && m.isActiveAndEnabled && m.status.fullNameJpStyle == name) != null;
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

		public static IEnumerable<Tuple<string, TMorph>> GetAllMorphsFromMaidList(List<Maid> list)
		{
			return
				list
				.SelectMany(m => GetAllMorphsFromMaid(m))
				.Select(r => new Tuple<string, TMorph>(r.First, r.Second));
		}

		public static IEnumerable<Tuple<string, TMorph>> GetAllMorphsFromMaid(Maid maid)
		{
			return
				FetchGoSlot(maid
				.body0)
				.Concat(maid.body0.goSlot)
				.Where(s => s != null && s.morph != null)
				.Select(r => new Tuple<string, TMorph>(r.Category, r.morph));
		}

		public static bool DoesCategoryContainKey(this Maid maid, TBody.SlotID category, string shapekey)
		{
			return
				maid.GetAllShapekeysInCategory(category)
				.FirstOrDefault(r => r.Equals(shapekey))
				.IsNullOrWhiteSpace() == false;
		}

		public static IEnumerable<string> GetAllShapekeysInCategory(this Maid maid, TBody.SlotID category)
		{
			return
				maid.GetAllMorphsInMaidOfCategory(category)
				.Select(m => m.hash)
				.SelectMany(r => r.Keys.Cast<string>());
		}

		public static IEnumerable<TMorph> GetAllMorphsInMaidOfCategory(this Maid maid, TBody.SlotID category)
		{
			return
				FetchGoSlot(maid
				.body0)
				.Concat(maid.body0.goSlot)
				.Where(s => s != null && s.morph != null && s.Category == category.ToString())
				.Select(m => m.morph);
		}
		public static bool ContainsShapekey(this TMorph morph, string shapekey) 
		{ 
			return morph.hash != null && morph.hash.Keys.Cast<string>().Contains(shapekey);
		}
		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromMaidList(List<Maid> maids)
		{
			return
				maids
				.Select(m => GetAllShapeKeysFromMaid(m))
				.SelectMany(r => r);
		}

		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromMaid(Maid maid)
		{
			if (maid == null)
			{
				return null;
			}

			List<Tuple<string, string>> result = new List<Tuple<string, string>>();

			foreach (var keyPair in GetAllMorphsFromMaid(maid))
			{
				var keyNameList = keyPair.Second.hash.Keys.Cast<string>();

				foreach (var stringKey in keyNameList)
				{
					result.Add(new Tuple<string, string>(keyPair.First, stringKey));
				}
			}

			result = result
			  .GroupBy(p => new { p.First, p.Second })
			  .Select(g => g.First())
			  .ToList();

			return result;
		}

		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromAllMaids()
		{
			var result = GetAllShapeKeysFromMaidList
				(GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.Where(m => m.isActiveAndEnabled)
				.ToList()
				);

			result = result
			  .GroupBy(p => new { p.First, p.Second })
			  .Select(g => g.First())
			  .ToList();

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

		public static bool Contains(this string source, string cont, StringComparison compare)
		{
			return source.IndexOf(cont, compare) >= 0;
		}
	}
}
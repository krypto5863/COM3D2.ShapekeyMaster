using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShapekeyMaster
{
	internal class SMEventsAndArgs
	{
		//Cleaner for instancing.
		public class MorphEventArgs : EventArgs
		{
			public TMorph Morph { get; private set; }
			public bool Creation { get; private set; }

			public MorphEventArgs(TMorph changedMorph, bool wasCreated = true)
			{
				Morph = changedMorph;
				Creation = wasCreated;
			}
		}

		public class ExcitementChangeEvent : EventArgs
		{
			public string Maid { get; private set; }
			public int Excitement { get; private set; }

			public ExcitementChangeEvent(string maid, int excite)
			{
				Maid = maid;
				Excitement = excite;
			}
		}

		public class ClothingMaskChangeEvent : EventArgs
		{
			public string Maid { get; private set; }

			public ClothingMaskChangeEvent(string maid)
			{
				Maid = maid;
			}
		}
	}

	internal static class Extensions
	{
		private static readonly FieldInfo goSlotField = typeof(TBody).GetField("goSlot", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

		private static readonly Type goSlotType = goSlotField.FieldType;

		private static readonly MethodInfo goSlotMethod = goSlotType.GetMethod("GetListParents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public) ?? null;

		/// <summary>
		/// A convenience method meant to provide support for 2.00 and 3.00 which vary in this field. The ultimate values are the same, they just need to be fetched differently. :P
		/// </summary>
		/// <param name="body"></param>
		/// <returns></returns>
		public static List<TBodySkin> FetchGoSlot(this TBody body)
		{
			if (goSlotField.FieldType == typeof(List<TBodySkin>))
			{
				return (List<TBodySkin>)goSlotField.GetValue(body) ?? null;
			}

			var goSlotVal = goSlotField.GetValue(body);
			return (List<TBodySkin>)goSlotMethod.Invoke(goSlotVal, new object[0]) ?? null;
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

		public static bool IsMaidActive(this Maid maid) 
		{
			return maid != null && maid.isActiveAndEnabled;
		}

		public static bool IsMaidActive(string name)
		{
			if (name == "")
			{
				bool result =
					GameMain.Instance.CharacterMgr
					.GetStockMaidList()
					.Where(m => m.IsMaidActive())
					.Count() > 0;
#if (DEBUG)

				Main.logger.LogDebug("Maids active to edit = " + result);
#endif

				return result;
			}

			return GetMaidByName(name).IsMaidActive();
		}

		public static Maid GetMaidByName(string name)
		{
			//If name string is empty, return null. Otherwise, execute link to find.
			return name.IsNullOrWhiteSpace() ? null :
				GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.FirstOrDefault(m => m != null && m.status.fullNameJpStyle.Equals(name));
		}

		public static IEnumerable<Tuple<string, TMorph>> GetAllMorphsFromMaidList(List<Maid> list)
		{
			return
				list
				.SelectMany(m => GetAllMorphsFromMaid(m))
				.Select(r => new Tuple<string, TMorph>(r.item1, r.item2));
		}

		public static IEnumerable<Tuple<string, TMorph>> GetAllMorphsFromMaid(this Maid maid)
		{
			return
				maid
				.body0
				.FetchGoSlot()
				.Where(s => s?.morph != null)
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
				maid.GetAllMorphsFromMaid()
				.Where(r => r.item1.Equals(category.ToString()))
				.Select(m => m.item2);
		}
		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromMaidList(List<Maid> maids)
		{
			return
				maids
				.Select(m => GetAllShapeKeysFromMaid(m))
				.SelectMany(r => r);
		}

		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromMaid(this Maid maid)
		{
			if (maid == null)
			{
				return null;
			}

			List<Tuple<string, string>> result = new List<Tuple<string, string>>();

			foreach (var keyPair in GetAllMorphsFromMaid(maid))
			{
				var keyNameList = keyPair.item2.hash.Keys.Cast<string>();

				foreach (var stringKey in keyNameList)
				{
					result.Add(new Tuple<string, string>(keyPair.item1, stringKey));
				}
			}

			result = result
			  .GroupBy(p => new { p.item1, p.item2 })
			  .Select(g => g.First())
			  .ToList();

			return result;
		}

		public static IEnumerable<Tuple<string, string>> GetAllShapeKeysFromAllMaids()
		{
			var result = GetAllShapeKeysFromMaidList
				(GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.Where(m => m.IsMaidActive())
				.ToList()
				);

			result = result
			  .GroupBy(p => new { p.item1, p.item2 })
			  .Select(g => g.First())
			  .ToList();

			return result;
		}

		public static bool IsFaceKey(string Keyname)
		{
			return GameMain.Instance.CharacterMgr.GetStockMaidList()
			.Where(maid => maid?.body0?.Face?.morph != null)
			.Select(m => m.body0.Face.morph)
			.Where(mr => mr.Contains(Keyname))
			.Count() > 0;
		}

		public static IEnumerable<string> GetNameOfAllMaids()
		{
			var result = GameMain.Instance.CharacterMgr
				.GetStockMaidList()
				.Select(m => m.status.fullNameJpStyle)
				.ToList();

			result.Sort();

			return result;
		}

		public static float CalculateExcitementDeformation(this ShapeKeyEntry sk)
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

		/// <summary>
		/// A faster way to check if a string contains another while using StringComparison.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="cont"></param>
		/// <param name="compare"></param>
		/// <returns></returns>
		public static bool Contains(this string source, string cont, StringComparison compare)
		{
			return source.IndexOf(cont, compare) >= 0;
		}
	}
}
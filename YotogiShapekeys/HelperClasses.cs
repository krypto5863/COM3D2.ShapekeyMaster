using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapekeyMaster
{
	internal static class HelperClasses
	{
		public static bool IsMaidActive(string name)
		{

			if (name == "")
			{

				bool result =
					ShapekeyFetcherSetter
					.GetMaidsList()
					.Where(m => m != null)
					.Count() > 0;

#if (DEBUG)

				Debug.Log("Maids active to edit = " + result);
#endif

				return result;
			}

			return
				ShapekeyFetcherSetter
				.GetMaidsList()
				.Where(m => m != null && m.isActiveAndEnabled && m.status.fullNameJpStyle == name)
				.Count() > 0;
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
				.GetListParents()
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
			return
				GetAllMorphsFromMaid(maid)
				.Select(m => m.hash)
				.Select(h => h.Keys)
				.Select(k => k.Cast<string>())
				.SelectMany(k => k)
				.Distinct();
		}
		public static IEnumerable<string> GetAllBlendShapes(Mesh mesh)
		{

#if (DEBUG)
			Debug.Log($"Was called, getting all blendshapes in {mesh.name}, there should be {mesh.blendShapeCount} blendshapes.");
#endif
			string blendshape;

			List<string> res = new List<string>();

			int shapecount = 0;

			for (int i = 0; mesh.blendShapeCount > i && shapecount < mesh.blendShapeCount; i++)
			{

#if (DEBUG)
				Debug.Log($"Found blendshape with name {mesh.GetBlendShapeName(i)}");
#endif

				if ((blendshape = mesh.GetBlendShapeName(i)) != null)
				{
					res.Add(blendshape);
					++shapecount;
				}
			}

			return res;
		}
		public static float CalculateExcitementDeformation(ShapeKeyEntry sk, List<Maid> maids)
		{
			float excitement = GetHighestExcitement(maids);
#if (DEBUG)
			Debug.Log($"Highest excitement found was {excitement}");
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
			Debug.Log($"Yotogi settings were found! Returning a value of : {result + sk.DeformMin}");
#endif

			return (result + sk.DeformMin);
		}
		public static float GetHighestExcitement(List<Maid> maids)
		{
			maids = maids
			.Where(m => m != null && m.isActiveAndEnabled)
			.ToList();

			if (maids.Count() > 0)
			{

#if (DEBUG)
				Debug.Log($"Getting highest excitement! We have to query results from this many maids: {maids.Count()}");
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

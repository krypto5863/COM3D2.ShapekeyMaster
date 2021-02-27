using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ShapekeyMaster.HelperClasses;

namespace ShapekeyMaster
{
	internal static class BlendShapeFetcherSetter
	{
		public static void RunAll()
		{

#if (DEBUG)
			Debug.Log($"Updated called. Running on all.");
#endif

			UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>()
			.ToList()
			//.Where(smr => smr.sharedMesh.blendShapeCount > 0)
			.ForEach(sm => RunSkinnedMesh(sm));
		}
		static void RunSkinnedMesh(SkinnedMeshRenderer smr)
		{
#if (DEBUG)
			Debug.Log($"Working on skinned mesh: {smr.name}");
#endif
			foreach (KeyValuePair<int, ShapeKeyEntry> shapekey in UI.ShapeKeys)
			{
				if (shapekey.Value.IsProp)
				{
#if (DEBUG)
					Debug.Log($"Working on shapekey entry: {shapekey.Value.EntryName}");
#endif
					GetAllBlendShapes(smr.sharedMesh)
					.Where(b => b.Equals(shapekey.Value.ShapeKey))
					.ToList()
					.ForEach((bs) =>
					{
#if (DEBUG)
						Debug.Log($"Working on blendvalue: {bs}");
#endif
						smr.SetBlendShapeWeight(smr.sharedMesh.GetBlendShapeIndex(bs), shapekey.Value.Deform);
					});
				}
			}
		}
	}
}

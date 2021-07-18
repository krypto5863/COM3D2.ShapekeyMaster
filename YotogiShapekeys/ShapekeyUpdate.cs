using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShapekeyMaster
{
	class ShapekeyUpdate
	{
		internal static List<TMorph> ListOfActiveMorphs = new List<TMorph>();

		private static IEnumerator UpdateAllCo;
		private static IEnumerator UpdateSingleCo;
		private static IEnumerator UpdateMorphCo;

		internal static void UpdateKeys(bool avoidwait = false) 
		{
			if (UpdateAllCo != null)
			{
				Main.@this.StopCoroutine(UpdateAllCo);
				UpdateAllCo = null;
			}
			if (UpdateSingleCo != null)
			{
				Main.@this.StopCoroutine(UpdateSingleCo);
				UpdateSingleCo = null;
			}
			if (UpdateMorphCo != null)
			{
				Main.@this.StopCoroutine(UpdateMorphCo);
				UpdateMorphCo = null;
			}

			UpdateAllCo = UpdateAll(avoidwait);
			Main.@this.StartCoroutine(UpdateAllCo);
		}
		internal static void UpdateKeys(string maid, bool avoidwait = false) 
		{
			if (!string.IsNullOrEmpty(maid))
			{
				if (UpdateSingleCo != null)
				{
					Main.@this.StopCoroutine(UpdateSingleCo);
				}

				UpdateSingleCo = UpdateMaid(maid, avoidwait);
				Main.@this.StartCoroutine(UpdateSingleCo);
			}
		}
		internal static void UpdateKeys(ShapeKeyEntry shapeKeyEntry, bool avoidwait = false)
		{
			if (shapeKeyEntry != null)
			{
				Main.@this.StartCoroutine(UpdateMorph(shapeKeyEntry, avoidwait));
			}
		}
		private static IEnumerator UpdateAll(bool avoidwait = false) 
		{
			if (!avoidwait)
			{
				yield return new WaitForSeconds(0.10f);
			}

			UI.SKDatabase.MorphShapekeyDictionary().Keys.ToList().ForEach(morph =>
			{
				morph.FixBlendValues();
				try
				{
					if (morph == morph.bodyskin.body.Face.morph)
					{
						morph.FixBlendValues_Face();
					}
				}
				catch 
				{ 
					
				}
			});
		}
		//Setters, loaders, etc,.
		private static IEnumerator UpdateMaid(string Maid, bool avoidwait = false) 
		{
			if (!avoidwait) {
				yield return new WaitForSeconds(0.10f);
			}

			try
			{

				UI.SKDatabase.MorphShapekeyDictionary().Keys.Where(m => m.bodyskin.body.maid.status.fullNameJpStyle.Equals(Maid)).ToList().ForEach(morph =>
				{
					morph.FixBlendValues();

					try
					{
						if (morph == morph.bodyskin.body.Face.morph)
						{
							morph.FixBlendValues_Face();
						}
					}
					catch
					{

					}
				});
			}
			catch 
			{ 
				//Sometimes a maid has a nullref somewhere in that recursive line of checking. Just catch the error and discard the operation if so...
			}
		}
		private static IEnumerator UpdateMorph(ShapeKeyEntry shapeKeyEntry, bool avoidwait = false)
		{
			if (!avoidwait)
			{
				yield return new WaitForSeconds(0.10f);
			}

			UI.SKDatabase.MorphShapekeyDictionary().Where(m => m.Value.Contains(shapeKeyEntry)).ToList().ForEach(keyVal =>
			{
				keyVal.Key.FixBlendValues();

				try
				{
					if (keyVal.Key == keyVal.Key.bodyskin.body.Face.morph)
					{
						keyVal.Key.FixBlendValues_Face();
					}
				}
				catch
				{

				}
			});
		}
	}
}

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

		internal static void UpdateKeys(string maid = "", bool avoidwait = false) 
		{
			if (String.IsNullOrEmpty(maid))
			{
				if (UpdateAllCo != null)
				{
					Main.@this.StopCoroutine(UpdateAllCo);
				}
				if (UpdateSingleCo != null)
				{
					Main.@this.StopCoroutine(UpdateSingleCo);
				}

				UpdateAllCo = UpdateAll(avoidwait);
				Main.@this.StartCoroutine(UpdateAllCo);
			}
			else 
			{
				if (UpdateSingleCo != null)
				{
					Main.@this.StopCoroutine(UpdateSingleCo);
				}

				UpdateSingleCo = UpdateMaid(maid, avoidwait);
				Main.@this.StartCoroutine(UpdateSingleCo);
			}
		}
		private static IEnumerator UpdateAll(bool avoidwait = false) 
		{
			if (!avoidwait)
			{
				yield return new WaitForSeconds(0.10f);
			}

			ListOfActiveMorphs.ForEach(morph =>
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
		private static IEnumerator UpdateMaid(string Maid, bool avoidwait = false) 
		{
			if (!avoidwait) {
				yield return new WaitForSeconds(0.10f);
			}

			ListOfActiveMorphs.Where(m => m.bodyskin.body.maid.status.fullNameJpStyle.Equals(Maid)).ToList().ForEach(morph =>
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
	}
}

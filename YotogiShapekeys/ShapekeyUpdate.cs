using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapekeyMaster
{
	internal class ShapekeyUpdate
	{

		/*
		internal static string ShapekeyToPreview;
		internal static float PreviewValToSet { get; private set; }
		private static DateTime? TimeToViewKey = null;
		*/

		internal static List<TMorph> ListOfActiveMorphs = new List<TMorph>();
		private static readonly Dictionary<string, IEnumerator> CoroutinesUpdatingMaid = new Dictionary<string, IEnumerator>();
		private static readonly Dictionary<ShapeKeyEntry, IEnumerator> CoroutinesUpdatingMorph = new Dictionary<ShapeKeyEntry, IEnumerator>();

		private static IEnumerator UpdateAllCo;
		//private static IEnumerator PreviewKeyCo;

		internal static void UpdateKeys(bool avoidwait = false)
		{
			if (UpdateAllCo != null)
			{
				Main.@this.StopCoroutine(UpdateAllCo);
				UpdateAllCo = null;
			}
			foreach (var kp in CoroutinesUpdatingMaid)
			{
				Main.@this.StopCoroutine(kp.Value);
			}
			CoroutinesUpdatingMaid.Clear();

			foreach (var kp in CoroutinesUpdatingMorph)
			{
				Main.@this.StopCoroutine(kp.Value);
			}
			CoroutinesUpdatingMorph.Clear();

			UpdateAllCo = UpdateAll(avoidwait);
			Main.@this.StartCoroutine(UpdateAllCo);
		}

		/*
		internal static void PreviewKey(string key)
		{
			TimeToViewKey = DateTime.Now.AddSeconds(6);
			ShapekeyToPreview = key;
			PreviewValToSet = 0;

			if (!string.IsNullOrEmpty(key))
			{
				if (PreviewKeyCo == null) 
				{
					//Main.logger.LogMessage("Preview isn't running. Starting...");

					PreviewKeyCo = PreviewKey();
					Main.@this.StartCoroutine(PreviewKeyCo);
				}
			}
		}
		*/

		internal static void UpdateKeys(string maid, bool avoidwait = false)
		{
			if (!string.IsNullOrEmpty(maid))
			{
				if (CoroutinesUpdatingMaid.TryGetValue(maid, out var coroute) && coroute != null)
				{
					Main.@this.StopCoroutine(coroute);
				}

				CoroutinesUpdatingMaid[maid] = UpdateMaid(maid, avoidwait);
				Main.@this.StartCoroutine(CoroutinesUpdatingMaid[maid]);
			}
		}

		internal static void UpdateKeys(ShapeKeyEntry shapeKeyEntry, bool avoidwait = false)
		{
			if (shapeKeyEntry != null)
			{
				if (CoroutinesUpdatingMorph.TryGetValue(shapeKeyEntry, out var coroute) && coroute != null)
				{
					Main.@this.StopCoroutine(coroute);
				}

				CoroutinesUpdatingMorph[shapeKeyEntry] = UpdateMorph(shapeKeyEntry, avoidwait);
				Main.@this.StartCoroutine(CoroutinesUpdatingMorph[shapeKeyEntry]);
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
		private static IEnumerator UpdateMaid(string maid, bool avoidwait = false)
		{
			if (!avoidwait)
			{
				yield return new WaitForSeconds(0.10f);
			}

			try
			{
				UI.SKDatabase.MorphShapekeyDictionary()
					.Keys
					.Where(m => m.bodyskin.body.maid.status.fullNameJpStyle.Equals(maid))
					.ToList()
					.ForEach(morph =>
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

		/*
		private static IEnumerator PreviewKey()
		{
			//Main.logger.LogMessage("Preview is starting!");

			int ResetCount = 0;

			while (true) 
			{
				if (ShapekeyToPreview.IsNullOrWhiteSpace() == false)
				{
					ResetCount = 0;
					var keysToKick = ListOfActiveMorphs.Where(r => r.ContainsShapekey(ShapekeyToPreview));

					foreach (var morph in keysToKick)
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
					}

					if(TimeToViewKey.HasValue && TimeToViewKey < DateTime.Now)
					{
						PreviewValToSet = 0;

						ShapekeyToPreview = null;
						TimeToViewKey = null;

						foreach (var morph in keysToKick)
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
						}
					}
					else if (PreviewValToSet >= 100)
					{
						PreviewValToSet = 0;
					}
					else 
					{
						PreviewValToSet += 33;
					}
				}
				else 
				{
					ResetCount++;
				}

				if (ResetCount > 5) 
				{
					//Main.logger.LogMessage("Preview is done!");
					PreviewKeyCo = null;
					yield break;
				}

				yield return new WaitForSecondsRealtime(0.5f);
			}
		}
		*/
	}


}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ShapekeyMaster
{
	class ShapekeyDatabase
	{
		private Dictionary<Guid, ShapeKeyEntry> allShapekeyDictionary;
		private Dictionary<string, List<ShapeKeyEntry>> maidShapekeyDictionary;
		private Dictionary<Guid, ShapeKeyEntry> globalShapekeyDictionary;
		//These below handle morph specifics. They're important because they shorthand alot of the work involved tremendously.
		private List<TMorph> ListOfActiveMorphs = ShapekeyUpdate.ListOfActiveMorphs;
		private Dictionary<TMorph, HashSet<ShapeKeyEntry>> morphShapekeyDictionary;

		public Dictionary<Guid, ShapeKeyEntry> AllShapekeyDictionary 
		{
			get 
			{
				return allShapekeyDictionary;
			}
			set 
			{
				allShapekeyDictionary = value;
				RefreshSubDictionaries();
			}
		}

		internal Dictionary<Guid, ShapeKeyEntry> GlobalShapekeyDictionary()
		{
			return globalShapekeyDictionary;
		}
		internal Dictionary<TMorph, HashSet<ShapeKeyEntry>> MorphShapekeyDictionary()
		{
			return morphShapekeyDictionary;
		}
		internal List<string> ListOfMaidsWithKeys() 
		{
			return maidShapekeyDictionary.Keys.ToList();
		}
		internal ShapekeyDatabase() 
		{
			allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>();
			morphShapekeyDictionary = new Dictionary<TMorph, HashSet<ShapeKeyEntry>>();
			HarmonyPatchers.MorphEvent += (s ,e) => Main.@this.StartCoroutine(UpdateMorphDic(e));

			RefreshSubDictionaries();
		}
		internal void RefreshSubDictionaries()
		{
			maidShapekeyDictionary = new Dictionary<string, List<ShapeKeyEntry>>();

			foreach (ShapeKeyEntry shapeKeyEntry in allShapekeyDictionary.Values)
			{
				if (String.IsNullOrEmpty(shapeKeyEntry.Maid))
				{
					continue;
				}

				if (!maidShapekeyDictionary.ContainsKey(shapeKeyEntry.Maid))
				{
					maidShapekeyDictionary[shapeKeyEntry.Maid] = new List<ShapeKeyEntry>();
				}

				maidShapekeyDictionary[shapeKeyEntry.Maid].Add(shapeKeyEntry);
			}

			globalShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>();

			globalShapekeyDictionary = allShapekeyDictionary.Where(kv => String.IsNullOrEmpty(kv.Value.Maid)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			if (ListOfActiveMorphs.Count > 0) 
			{
				Main.@this.StartCoroutine(ReloadMorphDic());
			}
		}

		internal IEnumerator ReloadMorphDic()
		{
			morphShapekeyDictionary = new Dictionary<TMorph, HashSet<ShapeKeyEntry>>();

			foreach (TMorph morph in ListOfActiveMorphs)
			{
				bool done = false;

				string maid = "";

				while (!done)
				{
					try
					{
						if (morph.bodyskin != null && morph.bodyskin.body != null && morph.bodyskin.body.maid != null && morph.bodyskin.body.maid.status != null)
						{
							maid = morph.bodyskin.body.maid.status.fullNameJpStyle;
							done = true;
						}
					}
					catch
					{
						done = false;
					}

					yield return null;
				}

				foreach (ShapeKeyEntry shapeKeyEntry in ShapekeysByMaid(maid).Values.Concat(globalShapekeyDictionary.Values))
				{
					if (morph.hash.ContainsKey(shapeKeyEntry.ShapeKey))
					{
						if (!morphShapekeyDictionary.ContainsKey(morph))
						{
							morphShapekeyDictionary[morph] = new HashSet<ShapeKeyEntry>();
						}

						morphShapekeyDictionary[morph].Add(shapeKeyEntry);
					}
				}
			}
		}

		internal IEnumerator UpdateMorphDic(EventArgs args)
		{
			if (args is SMEventsAndArgs.MorphEventArgs morphArgs)
			{
				if (morphArgs.Creation)
				{
					bool done = false;

					string maid = "";

					while (!done)
					{
						try
						{
							maid = morphArgs.Morph.bodyskin.body.maid.status.fullNameJpStyle;
							done = true;
						}
						catch
						{
							done = false;
						}

						yield return null;
					}

					foreach (ShapeKeyEntry shapeKeyEntry in globalShapekeyDictionary.Values.Concat(ShapekeysByMaid(maid).Values))
					{
						if (morphArgs.Morph.hash.ContainsKey(shapeKeyEntry.ShapeKey))
						{
							if (!morphShapekeyDictionary.ContainsKey(morphArgs.Morph))
							{
								morphShapekeyDictionary[morphArgs.Morph] = new HashSet<ShapeKeyEntry>();
							}

							morphShapekeyDictionary[morphArgs.Morph].Add(shapeKeyEntry);
						}
					}
				}
				else
				{
					if (morphShapekeyDictionary.ContainsKey(morphArgs.Morph))
					{
						morphShapekeyDictionary.Remove(morphArgs.Morph);
					}
				}
			}
		}

		internal bool DoesMaidPartialEntryName(string Maid, string Entry)
		{
			return (maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.EntryName.ToLower(), $@".*{Entry.ToLower()}.*")).Count() > 0);
		}
		internal bool DoesMaidPartialShapekey(string Maid, string shapekey)
		{
			return maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.ShapeKey.ToLower(), $@".*{shapekey.ToLower()}.*")).Count() > 0;
		}
		internal Dictionary<Guid, ShapeKeyEntry> ShapekeysByMaid(string Maid)
		{
			if (!maidShapekeyDictionary.ContainsKey(Maid)) 
			{
				return new Dictionary<Guid, ShapeKeyEntry>();
			}

			return new Dictionary<Guid, ShapeKeyEntry>(maidShapekeyDictionary[Maid].ToDictionary(sk => sk.Id, sk => sk));
		}
		internal HashSet<ShapeKeyEntry> ShapekeysByMorph(TMorph morph)
		{
			if (!morphShapekeyDictionary.ContainsKey(morph))
			{
				return new HashSet<ShapeKeyEntry>();
			}

			return morphShapekeyDictionary[morph];
		}
		internal void Add(ShapeKeyEntry newVal)
		{
			allShapekeyDictionary[newVal.Id] = newVal;
			RefreshSubDictionaries();
		}
		internal void Remove(ShapeKeyEntry newVal)
		{
			allShapekeyDictionary.Remove(newVal.Id);
			RefreshSubDictionaries();
		}
		internal void ConcatenateDictionary(Dictionary<Guid, ShapeKeyEntry> newDictionary, bool overwrite = false)
		{
			if (!overwrite) 
			{
				allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>(
						allShapekeyDictionary
						.Concat(
						newDictionary
						.Where(kv => !allShapekeyDictionary.ContainsKey(kv.Key))
						)
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			} else 
			{
				allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>(
						newDictionary
						.Concat(
						allShapekeyDictionary
						)
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			}

			RefreshSubDictionaries();
		}
		internal void OverwriteDictionary(Dictionary<Guid, ShapeKeyEntry> newDictionary)
		{
			allShapekeyDictionary = newDictionary;

			RefreshSubDictionaries();
		}
	}
}
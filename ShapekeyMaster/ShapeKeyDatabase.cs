using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ShapeKeyMaster
{
	internal class ShapeKeyDatabase
	{
		[JsonIgnore]
		private Dictionary<Guid, ShapeKeyEntry> allShapekeyDictionary;

		[JsonIgnore]
		private Dictionary<string, List<ShapeKeyEntry>> maidShapekeyDictionary;

		[JsonIgnore]
		private Dictionary<Guid, ShapeKeyEntry> globalShapekeyDictionary;

		[JsonProperty]
		public ShapeKeyBlacklist BlacklistedShapeKeys { get; internal set; } = new ShapeKeyBlacklist();

		//These below handle morph specifics. They're important because they shorthand alot of the work involved tremendously.
		[JsonIgnore]
		private List<TMorph> ListOfActiveMorphs = ShapeKeyUpdate.ListOfActiveMorphs;

		[JsonIgnore]
		private Dictionary<TMorph, HashSet<ShapeKeyEntry>> morphShapekeyDictionary;

		[JsonProperty]
		public Dictionary<Guid, ShapeKeyEntry> AllShapekeyDictionary
		{
			get => allShapekeyDictionary;
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

		internal ShapeKeyDatabase()
		{
			allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>();
			morphShapekeyDictionary = new Dictionary<TMorph, HashSet<ShapeKeyEntry>>();
			HarmonyPatchers.MorphEvent += (s, e) => ShapeKeyMaster.Instance.StartCoroutine(UpdateMorphDic(e));

			RefreshSubDictionaries();
		}

		internal void RefreshSubDictionaries()
		{
			maidShapekeyDictionary = new Dictionary<string, List<ShapeKeyEntry>>();

			foreach (var shapeKeyEntry in allShapekeyDictionary.Values)
			{
				if (string.IsNullOrEmpty(shapeKeyEntry.Maid))
				{
					continue;
				}

				if (!maidShapekeyDictionary.ContainsKey(shapeKeyEntry.Maid))
				{
					maidShapekeyDictionary[shapeKeyEntry.Maid] = new List<ShapeKeyEntry>();
				}

				maidShapekeyDictionary[shapeKeyEntry.Maid].Add(shapeKeyEntry);
			}

			//Reassign order number to all shapekeys when:
			//If any shapekey does not have an order number, OR
			//If the list contains duplicate order numbers
			foreach (var maidSkDb in maidShapekeyDictionary)
			{
				var maidSkList = maidSkDb.Value;
				if (maidSkList.Any(s => !s.OrderNum.HasValue) 
					|| maidSkList.Count != maidSkList.Select(s => s.OrderNum).Distinct().ToList().Count)
				{
					for (var i = 0; i < maidSkList.Count; i++)
					{
						maidSkList[i].OrderNum = null;
						maidSkList[i].OrderNum = i + 1;
					}
				}
			}

			globalShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>();

			globalShapekeyDictionary = allShapekeyDictionary.Where(kv => string.IsNullOrEmpty(kv.Value.Maid)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//Reassign order number to all shapekeys when:
			//If any shapekey does not have an order number, OR
			//If the list contains duplicate order numbers
			var globalSkList = globalShapekeyDictionary.Values.ToList();
			if (globalSkList.Any(s => !s.OrderNum.HasValue) 
				|| globalSkList.Count != globalSkList.Select(s => s.OrderNum).Distinct().ToList().Count)
			{
				for (var i = 0; i < globalSkList.Count; i++)
				{
					globalSkList[i].OrderNum = null;
					globalSkList[i].OrderNum = i + 1;
				}
			}

			if (ListOfActiveMorphs.Count > 0)
			{
				ShapeKeyMaster.Instance.StartCoroutine(ReloadMorphDic());
			}
		}

		internal IEnumerator ReloadMorphDic()
		{
			morphShapekeyDictionary = new Dictionary<TMorph, HashSet<ShapeKeyEntry>>();

			foreach (var morph in ListOfActiveMorphs)
			{
				var done = false;

				var maid = "";

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

				foreach (var shapeKeyEntry in ShapeKeysByMaid(maid).Values.Concat(globalShapekeyDictionary.Values))
				{
					if (!morph.hash.ContainsKey(shapeKeyEntry.ShapeKey))
					{
						continue;
					}

					if (!morphShapekeyDictionary.ContainsKey(morph))
					{
						morphShapekeyDictionary[morph] = new HashSet<ShapeKeyEntry>();
					}

					morphShapekeyDictionary[morph].Add(shapeKeyEntry);
				}
			}
		}

		internal IEnumerator UpdateMorphDic(EventArgs args)
		{
			if (!(args is SmEventsAndArgs.MorphEventArgs morphArgs))
			{
				yield break;
			}

			if (morphArgs.Creation)
			{
				var done = false;

				var maid = "";

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

				foreach (var shapeKeyEntry in globalShapekeyDictionary.Values.Concat(ShapeKeysByMaid(maid).Values))
				{
					if (!morphArgs.Morph.hash.ContainsKey(shapeKeyEntry.ShapeKey))
					{
						continue;
					}

					if (!morphShapekeyDictionary.ContainsKey(morphArgs.Morph))
					{
						morphShapekeyDictionary[morphArgs.Morph] = new HashSet<ShapeKeyEntry>();
					}

					morphShapekeyDictionary[morphArgs.Morph].Add(shapeKeyEntry);
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

		internal bool DoesMaidPartialEntryName(string maid, string entry)
		{
			return maidShapekeyDictionary[maid]
				.Any(t => t.EntryName.Contains(entry, StringComparison.OrdinalIgnoreCase));
		}

		internal bool DoesMaidPartialShapeKey(string maid, string shapeKey)
		{
			return maidShapekeyDictionary[maid]
				.Any(t => t.ShapeKey.Contains(shapeKey, StringComparison.OrdinalIgnoreCase));
		}

		internal Dictionary<Guid, ShapeKeyEntry> ShapeKeysByMaid(string maid)
		{
			return !maidShapekeyDictionary.ContainsKey(maid)
				? new Dictionary<Guid, ShapeKeyEntry>()
				: new Dictionary<Guid, ShapeKeyEntry>(maidShapekeyDictionary[maid].ToDictionary(sk => sk.Id, sk => sk));
		}

		internal HashSet<ShapeKeyEntry> ShapeKeysByMorph(TMorph morph)
		{
			return !morphShapekeyDictionary.ContainsKey(morph) ? new HashSet<ShapeKeyEntry>() : morphShapekeyDictionary[morph];
		}

		internal void Add(ShapeKeyEntry newVal)
		{
			var skList = string.IsNullOrEmpty(newVal.Maid) ? globalShapekeyDictionary.Values.ToList() :
							maidShapekeyDictionary.ContainsKey(newVal.Maid) ? maidShapekeyDictionary[newVal.Maid] : null;
			var count = skList?.Max(s => s.OrderNum);
			newVal.OrderNum = null;
			newVal.OrderNum = count.HasValue ? count + 1 : 1;

			allShapekeyDictionary[newVal.Id] = newVal;
			RefreshSubDictionaries();
		}

		internal void Remove(ShapeKeyEntry newVal)
		{
			var skList = string.IsNullOrEmpty(newVal.Maid) ? globalShapekeyDictionary.Values.ToList() : maidShapekeyDictionary[newVal.Maid];
			var skAfter = skList.Where(s => s.OrderNum > newVal.OrderNum).ToList();
			foreach (var s in skAfter) //Move all sks after removed key forward
			{
				var orderNum = s.OrderNum;
				s.OrderNum = null;
				s.OrderNum = orderNum -= 1;
			}

			allShapekeyDictionary.Remove(newVal.Id);
			//newVal.Deform = 0;
			RefreshSubDictionaries();
		}

		internal void Reorder_MoveForward(ShapeKeyEntry sk)
		{
			if (sk.OrderNum != 1) //First one in list
			{
				sk.OrderNum -= 1; //setter handles sorting
			}
		}

		internal void Reorder_MoveBack(ShapeKeyEntry sk)
		{
			var skList = string.IsNullOrEmpty(sk.Maid) ? globalShapekeyDictionary.Values.ToList() : maidShapekeyDictionary[sk.Maid];
			if (sk.OrderNum != skList.Count) //Last one in list
			{
				sk.OrderNum += 1; //setter handles sorting
			}
		}

		internal int Reorder_Insert(ShapeKeyEntry sk, int newOrderNum) 
		{
			var oldOrderNum = sk.OrderNum.Value;
			var skList = string.IsNullOrEmpty(sk.Maid) ? globalShapekeyDictionary.Values.ToList() : maidShapekeyDictionary[sk.Maid];
			if (newOrderNum < 1)
			{
				newOrderNum = 1;
			}
			if (newOrderNum > skList.Count)
			{
				newOrderNum = skList.Count;
			}
			if (oldOrderNum != newOrderNum)
			{
				if (oldOrderNum < newOrderNum) //Move Back
				{
					var skBetween = skList.Where(s => oldOrderNum < s.OrderNum.Value && s.OrderNum.Value <= newOrderNum);
					foreach (var s in skBetween)
					{
						var orderNum = s.OrderNum;
						s.OrderNum = null;
						s.OrderNum = orderNum -= 1;
					}
				}
				if (oldOrderNum > newOrderNum) //Move Forward
				{
					var skBetween = skList.Where(s => newOrderNum <= s.OrderNum.Value && s.OrderNum.Value < oldOrderNum);
					foreach (var s in skBetween)
					{
						var orderNum = s.OrderNum;
						s.OrderNum = null;
						s.OrderNum = orderNum += 1;
					}
				}
				return newOrderNum;
			}
			else
			{
				return oldOrderNum;
			}
		}

		internal void ConcatenateDictionary(Dictionary<Guid, ShapeKeyEntry> newDictionary, bool overwrite = false)
		{
			if (newDictionary is null)
			{
				return;
			}

			if (!overwrite)
			{
				allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>(
						allShapekeyDictionary
						.Concat(
						newDictionary
						.Where(kv => !allShapekeyDictionary.ContainsKey(kv.Key))
						)
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			}
			else
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
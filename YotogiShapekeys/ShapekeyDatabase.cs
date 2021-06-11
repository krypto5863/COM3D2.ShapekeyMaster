using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShapekeyMaster
{
	class ShapekeyDatabase
	{
		private SortedDictionary<Guid, ShapeKeyEntry> allShapekeyDictionary;
		private SortedDictionary<string, List<ShapeKeyEntry>> maidShapekeyDictionary;
		private SortedDictionary<Guid, ShapeKeyEntry> globalShapekeyDictionary;

		public SortedDictionary<Guid, ShapeKeyEntry> AllShapekeyDictionary 
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

		public SortedDictionary<Guid, ShapeKeyEntry> GlobalShapekeyDictionary()
		{
			return globalShapekeyDictionary;
		}

		public List<string> ListOfMaidsWithKeys() 
		{

			return maidShapekeyDictionary.Keys.ToList();

		}
		public int ShapekeysCount()
		{
			return allShapekeyDictionary.Count;
		}

		public ShapekeyDatabase() 
		{
			allShapekeyDictionary = new SortedDictionary<Guid, ShapeKeyEntry>();		
			
			RefreshSubDictionaries();
		}

		private void RefreshSubDictionaries()
		{
			maidShapekeyDictionary = new SortedDictionary<string, List<ShapeKeyEntry>>();

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

			globalShapekeyDictionary = new SortedDictionary<Guid, ShapeKeyEntry>();

			globalShapekeyDictionary = new SortedDictionary<Guid, ShapeKeyEntry>(allShapekeyDictionary.Where(kv => String.IsNullOrEmpty(kv.Value.Maid)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
		}

		public bool DoesMaidHaveKey(string Maid, ShapeKeyEntry keyEntry)
		{
			return (maidShapekeyDictionary[Maid].Contains(keyEntry));
		}
		public bool DoesMaidPartialEntryName(string Maid, string Entry)
		{
			return (maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.EntryName.ToLower(), $@".*{Entry.ToLower()}.*")).Count() > 0);
		}
		public bool DoesMaidPartialShapekey(string Maid, string shapekey)
		{
			return (maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.ShapeKey.ToLower(), $@".*{shapekey.ToLower()}.*")).Count() > 0);
		}
		public SortedDictionary<Guid, ShapeKeyEntry> ShapekeysByMaid(string Maid)
		{
			return new SortedDictionary<Guid, ShapeKeyEntry>(maidShapekeyDictionary[Maid].ToDictionary(sk => sk.Id, sk => sk));
		}

		public void Add(ShapeKeyEntry newVal)
		{
			allShapekeyDictionary[newVal.Id] = newVal;
			RefreshSubDictionaries();
		}

		public void Remove(ShapeKeyEntry newVal)
		{
			allShapekeyDictionary.Remove(newVal.Id);
			RefreshSubDictionaries();
		}

		public void Set(Guid ID, ShapeKeyEntry newVal)
		{
			allShapekeyDictionary[ID] = newVal;
			RefreshSubDictionaries();
		}
		public void SetMaid(Guid ID, string Maid)
		{
			allShapekeyDictionary[ID].SetMaid(Maid);
			RefreshSubDictionaries();
		}
		public void ConcatenateDictionary(SortedDictionary<Guid, ShapeKeyEntry> newDictionary)
		{

			allShapekeyDictionary = new SortedDictionary<Guid, ShapeKeyEntry>(
					allShapekeyDictionary
					.Concat(
					newDictionary
					.Where(kv => !allShapekeyDictionary.ContainsKey(kv.Key))
					)
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

			RefreshSubDictionaries();
		}
	}
}

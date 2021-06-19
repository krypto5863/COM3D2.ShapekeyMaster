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
		internal List<string> ListOfMaidsWithKeys() 
		{
			return maidShapekeyDictionary.Keys.ToList();
		}
		internal int ShapekeysCount()
		{
			return allShapekeyDictionary.Count;
		}

		internal ShapekeyDatabase() 
		{
			allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>();		
			
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
		}

		internal bool DoesMaidHaveKey(string Maid, ShapeKeyEntry keyEntry)
		{
			return (maidShapekeyDictionary[Maid].Contains(keyEntry));
		}
		internal bool DoesMaidPartialEntryName(string Maid, string Entry)
		{
			return (maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.EntryName.ToLower(), $@".*{Entry.ToLower()}.*")).Count() > 0);
		}
		internal bool DoesMaidPartialShapekey(string Maid, string shapekey)
		{
			return (maidShapekeyDictionary[Maid].Where(t => Regex.IsMatch(t.ShapeKey.ToLower(), $@".*{shapekey.ToLower()}.*")).Count() > 0);
		}
		internal Dictionary<Guid, ShapeKeyEntry> ShapekeysByMaid(string Maid)
		{
			if (!maidShapekeyDictionary.ContainsKey(Maid)) 
			{
				return new Dictionary<Guid, ShapeKeyEntry>();
			}

			return new Dictionary<Guid, ShapeKeyEntry>(maidShapekeyDictionary[Maid].ToDictionary(sk => sk.Id, sk => sk));
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

		internal void Set(Guid ID, ShapeKeyEntry newVal)
		{
			allShapekeyDictionary[ID] = newVal;
			RefreshSubDictionaries();
		}
		internal void ConcatenateDictionary(Dictionary<Guid, ShapeKeyEntry> newDictionary)
		{

			allShapekeyDictionary = new Dictionary<Guid, ShapeKeyEntry>(
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

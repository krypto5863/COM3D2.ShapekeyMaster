using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ShapekeyMaster
{
	internal static class UI
	{
		//public static SortedDictionary<Guid, ShapeKeyEntry> ShapeKeys = new SortedDictionary<Guid, ShapeKeyEntry>();
		//public static SortedDictionary<Guid, ShapeKeyEntry> FilteredShapeKeys = new SortedDictionary<Guid, ShapeKeyEntry>();
		//public static SortedDictionary<string, List<ShapeKeyEntry>> MaidFilteredShapeKeys = new SortedDictionary<string, List<ShapeKeyEntry>>();

		private static List<string> ShapekeysNameList = new List<string>();
		private static List<string> MaidNameList = new List<string>();

		//private static readonly List<Guid> DeleteList = new List<Guid>();

		internal static ShapekeyDatabase SKDatabase = new ShapekeyDatabase();

		private static readonly int WindowID = 777777;

		private static bool runonce = true;
		public static bool changewasmade = false;

		private static string Filter = "";
		private static int FilterMode = 0;

		private static int TabSelection = 0;

		private static List<string> MaidSelection = new List<string>();

		//Used for exporting...
		//private static List<String> MaidNamesWithKeys = new List<string>();

		private static Vector2 scrollPosition = Vector2.zero;
		private static Guid OpenSKMenu;
		private static Guid OpenMaidMenu;
		private static Guid OpenRenameMenu;
		private static string MaidGroupRenameMenu;
		private static string MaidGroupRename;
		private static bool MaidGroupCreateOpen;
		private static bool ExportMenuOpen;

		private static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 4, Screen.width / 3f, Screen.height / 1.5f);

		private static GUIStyle seperator;
		private static GUIStyle Sections;
		private static GUIStyle Sections2;
		private static GUIStyle RightAlignedButton;

		/*
		private static void RefreshFilteredDictionary()
		{
			FilteredShapeKeys = ShapeKeys;
			
			if (TabSelection == 1)
			{
				FilteredShapeKeys = new SortedDictionary<Guid, ShapeKeyEntry>(ShapeKeys.Where(kv => String.IsNullOrEmpty(kv.Value.Maid)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			}
			else if (TabSelection == 2)
			{
				MaidFilteredShapeKeys.Clear();

				foreach (ShapeKeyEntry s in ShapeKeys.Values)
				{
					if (!String.IsNullOrEmpty(s.Maid))
					{
						if (!MaidFilteredShapeKeys.ContainsKey(s.Maid))
						{
							MaidFilteredShapeKeys[s.Maid] = new List<ShapeKeyEntry>();
						}

						MaidFilteredShapeKeys[s.Maid].Add(s);
					}
				}
			}
		}*/

		/*
		private static void RefreshGroups()
		{
			GroupedShapeKeys.Clear();

			var groups = ShapeKeys.Select(sk => sk.Value.Group).Distinct();

			foreach (KeyValuePair<Guid, ShapeKeyEntry> s in ShapeKeys)
			{
				if (!GroupedShapeKeys.ContainsKey(s.Value.Group))
				{
					GroupedShapeKeys[s.Value.Group] = new List<ShapeKeyEntry>();
				}

				GroupedShapeKeys[s.Value.Group].Add(s.Value);
			}
		}
		*/
		public static void Initialize()
		{
			if (runonce)
			{
				seperator = new GUIStyle(GUI.skin.horizontalSlider);
				seperator.fixedHeight = 1f;
				seperator.normal.background = MyGUI.Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.8f));
				seperator.margin.top = 10;
				seperator.margin.bottom = 10;

				Sections = new GUIStyle(GUI.skin.box);
				Sections.normal.background = MyGUI.Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));
				Sections2 = new GUIStyle(GUI.skin.box);
				Sections2.normal.background = MyGUI.Helpers.MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

				RightAlignedButton = new GUIStyle(GUI.skin.button);

				RightAlignedButton.alignment = TextAnchor.MiddleRight;

				runonce = false;
			}
			windowRect = GUILayout.Window(WindowID, windowRect, GuiWindowControls, "ShapekeyMaster");
		}

		private static void GuiWindowControls(int windowID)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

			//ToolbarSelection = ToolbarSelection = GUILayout.Toolbar(ToolbarSelection, ToolbarStrings);

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			//if (ToolbarSelection == 0)
			//{
			if (ExportMenuOpen)
			{
				DisplayExportMenu();
			}
			else if (SKDatabase.AllShapekeyDictionary.ContainsKey(OpenSKMenu))
			{
				DisplayShapeKeySelectMenu(SKDatabase.AllShapekeyDictionary[OpenSKMenu]);
			}
			else if (SKDatabase.AllShapekeyDictionary.ContainsKey(OpenMaidMenu))
			{
				DisplayMaidSelectMenu(SKDatabase.AllShapekeyDictionary[OpenMaidMenu]);
			}
			else if (SKDatabase.AllShapekeyDictionary.ContainsKey(OpenRenameMenu))
			{
				DisplayRenameMenu(SKDatabase.AllShapekeyDictionary[OpenRenameMenu]);
			}
			else if (!String.IsNullOrEmpty(MaidGroupRenameMenu))
			{
				DisplayMaidRenameMenu(MaidGroupRenameMenu);
			} else if (MaidGroupCreateOpen) 
			{
				DisplayMaidGroupCreateMenu();
			}
			else
			{
				DisplayHeaderMenu();

				Main.SimpleMode.Value = GUILayout.Toggle(Main.SimpleMode.Value, "Simple");

				switch (TabSelection)
				{
					case 1:
						if (Main.SimpleMode.Value)
						{
							GUILayout.BeginVertical(Sections);
							GUILayout.BeginHorizontal(Sections2);
							GUILayout.FlexibleSpace();
							GUILayout.Label("Globals");
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();

							SimpleDisplayShapeKeyEntriesMenu(SKDatabase.GlobalShapekeyDictionary());
							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(Sections);
							GUILayout.BeginHorizontal(Sections2);
							GUILayout.FlexibleSpace();
							GUILayout.Label("Globals");
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();

							DisplayShapeKeyEntriesMenu(SKDatabase.GlobalShapekeyDictionary());

							GUILayout.EndVertical();
						}
						break;

					case 2:
						Main.HideInactiveMaids.Value = GUILayout.Toggle(Main.HideInactiveMaids.Value, "Hide Inactive Maids");

						GUILayout.BeginVertical(Sections);
						GUILayout.BeginHorizontal(Sections2);
						GUILayout.FlexibleSpace();
						GUILayout.Label("Maids");
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						DisplayMaidOptions();

						GUILayout.EndVertical();
						break;

					default:
						if (Main.SimpleMode.Value)
						{
							GUILayout.BeginVertical(Sections);
							GUILayout.BeginHorizontal(Sections2);
							GUILayout.FlexibleSpace();
							GUILayout.Label("All");
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();

							SimpleDisplayShapeKeyEntriesMenu(SKDatabase.AllShapekeyDictionary);

							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(Sections);
							GUILayout.BeginHorizontal(Sections2);
							GUILayout.FlexibleSpace();
							GUILayout.Label("All");
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();

							DisplayShapeKeyEntriesMenu(SKDatabase.AllShapekeyDictionary);

							GUILayout.EndVertical();
						}
						break;
				}
			}

			GUILayout.EndScrollView();

			DisplayFooter();

			ShapekeyMaster.MyGUI.Helpers.ChkMouseClick(windowRect);
		}

		private static void DisplayHeaderMenu()
		{
			GUILayout.BeginHorizontal(Sections2);

			if (GUILayout.Button("All"))
			{
				TabSelection = 0;
			}
			else if (GUILayout.Button("Globals"))
			{
				TabSelection = 1;
			}
			else if (GUILayout.Button("Maids"))
			{
				TabSelection = 2;
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(Sections2);

			GUILayout.Label("Search by ");

			GUILayout.FlexibleSpace();

			if (FilterMode == 0)
			{
				if (GUILayout.Button("Entry Name"))
				{
					FilterMode = 1;
				}
			}
			else if (FilterMode == 1)
			{
				if (GUILayout.Button("Maid Name"))
				{
					FilterMode = 2;
				}
			}
			else if (FilterMode == 2)
			{
				if (GUILayout.Button("Shapekey Name"))
				{
					FilterMode = 0;
				}
			}

			Filter = GUILayout.TextField(Filter, GUILayout.Width(160));

			GUILayout.EndHorizontal();
		}

		private static void DisplayFooter() 
		{
			GUILayout.BeginVertical(Sections2);
			if (ExportMenuOpen)
			{
				if (GUILayout.Button("Cancel"))
				{
					ExportMenuOpen = false;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				if (SKDatabase.ShapekeysCount() > 0 && GUILayout.Button("Apply"))
				{
					ShapekeyFetcherSetter.RunAll();
				}
				if (GUILayout.Button("Save"))
				{
#if (DEBUG)
					Main.logger.LogDebug("Saving data to configs now!");
#endif

					Main.SaveToJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", SKDatabase);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Export"))
				{
					ExportMenuOpen = true;
				}
				if (GUILayout.Button("Import"))
				{
					SKDatabase.ConcatenateDictionary(Main.LoadFromJson(null, true));
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}

		private static void DisplayMaidOptions()
		{
			GUILayout.BeginVertical(Sections);

			foreach (String MaidWithKey in SKDatabase.ListOfMaidsWithKeys())
			{
				if (Main.HideInactiveMaids.Value) 
				{ 
					if (!HelperClasses.IsMaidActive(MaidWithKey))
					{
						continue;
					}
				}

				if (Filter != "")
				{
					if (FilterMode == 0) 
					{
						if (!SKDatabase.DoesMaidPartialEntryName(MaidWithKey, Filter)) 
						{
							continue;
						}
					}
					else if (FilterMode == 1)
					{
						if (!Regex.IsMatch(MaidWithKey.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
					else if (FilterMode == 2)
					{
						if (!SKDatabase.DoesMaidPartialShapekey(MaidWithKey, Filter))
						{
							continue;
						}
					}
				}

				GUILayout.BeginVertical(Sections);
				GUILayout.BeginHorizontal(Sections);
				GUILayout.Label(MaidWithKey);

				if (GUILayout.Button("I/O"))
				{
					bool targettoggle = !SKDatabase.ShapekeysByMaid(MaidWithKey).Values.FirstOrDefault().Enabled;

					foreach (ShapeKeyEntry sk in SKDatabase.ShapekeysByMaid(MaidWithKey).Values)
					{
						sk.SetEnabled(targettoggle);
					}

					return;
				}

				if (GUILayout.Button("Rename"))
				{
					MaidGroupRenameMenu = MaidWithKey;
					MaidGroupRename = MaidGroupRenameMenu;

					MaidNameList = HelperClasses.GetNameOfAllMaids().ToList();

					return;
				}

				if (GUILayout.Button("Del"))
				{
					foreach (ShapeKeyEntry skp in SKDatabase.ShapekeysByMaid(MaidWithKey).Values)
					{
						SKDatabase.Remove(skp);
					}

					return;
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("☰"))
				{
					if (!MaidSelection.Contains(MaidWithKey))
					{
						MaidSelection.Add(MaidWithKey);
					}
					else
					{
						MaidSelection.Remove(MaidWithKey);
					}

					return;
				}

				GUILayout.EndHorizontal();

				if (MaidSelection.Contains(MaidWithKey))
				{
					if (Main.SimpleMode.Value)
					{
						SimpleDisplayShapeKeyEntriesMenu(SKDatabase.ShapekeysByMaid(MaidWithKey));
					}
					else
					{
						DisplayShapeKeyEntriesMenu(SKDatabase.ShapekeysByMaid(MaidWithKey));
					}

					GUILayout.BeginHorizontal();

					if (GUILayout.Button("+", GUILayout.Width(40)))  
					{
						SKDatabase.Add(new ShapeKeyEntry(Guid.NewGuid(), MaidWithKey));

						return;
					}

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}

			if (GUILayout.Button("New Maid"))
			{
				MaidNameList = HelperClasses.GetNameOfAllMaids().ToList();

				MaidGroupCreateOpen = true;

				return;
			}

			GUILayout.EndVertical();
		}

		/*static bool DisplayGroups(string Maid, bool Filter = true)
		{
			bool CallForRefresh = true;

			GUILayout.BeginVertical(Sections);
			foreach (KeyValuePair<string, List<ShapeKeyEntry>> GroupedShapekey in GroupedShapeKeys)
			{
				if (Filter && GroupedShapekey.Value.Where(tp => tp.Maid.Equals(Maid)).Count() == 0) 
				{
					continue;
				}

				GUILayout.BeginHorizontal(Sections);
				GUILayout.Label(GroupedShapekey.Key);

				if (GUILayout.Button("I/O"))
				{
					bool targettoggle = !GroupedShapekey.Value.FirstOrDefault().Enabled;

					foreach (ShapeKeyEntry sk in GroupedShapekey.Value)
					{
						sk.Enabled = targettoggle;
					}
				}

				if (GUILayout.Button("Rename"))
				{
					GroupRenameMenu = GroupedShapekey.Key;
					GroupRename = GroupedShapekey.Key;
				}

				if (GUILayout.Button("Del"))
				{
					foreach (Guid skp in ShapeKeys.Where(kvp => kvp.Value.Group.Equals(GroupedShapekey.Key) && (kvp.Value.Maid.Equals(Maid) || Filter == false)).Select(kt => kt.Key)) 
					{
						ShapeKeys.Remove(skp);

						return true;
					}

					CallForRefresh = true;
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("☰"))
				{
					if (!GroupSelection.ContainsKey(Maid))
					{
						GroupSelection[Maid] = new List<string>();
					}

					if (!GroupSelection[Maid].Contains(GroupedShapekey.Key))
					{
						GroupSelection[Maid].Add(GroupedShapekey.Key);
					}
					else
					{
						GroupSelection[Maid].Remove(GroupedShapekey.Key);
					}

					CallForRefresh = true;
				}

				GUILayout.EndHorizontal();

				if (GroupSelection.Values.SelectMany(t => t).Contains(GroupedShapekey.Key) && (GroupSelection.ContainsKey(Maid) && GroupSelection[Maid].Contains(GroupedShapekey.Key)) || Filter == false)
				{
					var TempDic = new SortedDictionary<Guid, ShapeKeyEntry>(GroupedShapekey.Value.Where(tp => tp.Maid.Equals(Maid) || Filter == false).ToDictionary(ps => ps.Id, ps => ps));

					if (SimpleMode)
					{
						SimpleDisplayShapeKeyEntriesMenu(TempDic);
					}
					else
					{
						DisplayShapeKeyEntriesMenu(TempDic);
					}

					if (GUILayout.Button("+"))
					{
#if (DEBUG)
						Main.logger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
						Guid newkey = Guid.NewGuid();

						ShapeKeys.Add(newkey, new ShapeKeyEntry(newkey, GroupedShapekey.Key));

						ShapeKeys[newkey].SetMaid(Maid);

						CallForRefresh = true;
					}
				}
			}

			GUILayout.EndVertical();

			if (GUILayout.Button("New Group"))
			{
#if (DEBUG)
				Main.logger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				int i = 0;

				for (i = 0; GroupedShapeKeys.Keys.Contains($"No Group {i}"); i++) ;

				Guid newkey = Guid.NewGuid();

				ShapeKeys.Add(newkey, new ShapeKeyEntry(newkey, $"No Group {i}"));

				ShapeKeys[newkey].SetMaid(Maid);

				CallForRefresh = true;
			}

			return CallForRefresh;
		}
		*/

		private static void SimpleDisplayShapeKeyEntriesMenu(SortedDictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{

			//GUILayout.Label("", seperator);

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values)
			{

				if (Filter != "")
				{
					if (FilterMode == 0)
					{
						if (!Regex.IsMatch(s.EntryName.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
					else if (FilterMode == 1)
					{
						if (!Regex.IsMatch(s.Maid.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
					else if (FilterMode == 2)
					{
						if (!Regex.IsMatch(s.ShapeKey.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
				}

				GUILayout.BeginVertical(Sections);

				GUILayout.BeginHorizontal();
				s.SetEnabled(GUILayout.Toggle(s.Enabled, "I/O", GUILayout.Width(40)));

				if (GUILayout.Button("+", GUILayout.Width(40)))
				{
					ShapekeysNameList = HelperClasses.GetAllShapeKeysFromAllMaids().ToList();
					
					OpenSKMenu = s.Id;
				}

				s.SetShapeKey(GUILayout.TextField(s.ShapeKey, GUILayout.Width(120)));

				s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));

				GUILayout.Label(s.Deform.ToString(), GUILayout.Width(30));

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(Sections);

				if (TabSelection == 0)
				{
					if (String.IsNullOrEmpty(s.Maid))
					{
						GUILayout.Label("Global");
					}
					else
					{
						GUILayout.Label(s.Maid);
					}
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button($"Del"))
				{
					SKDatabase.Remove(s);
					return;
				}

				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			if (TabSelection != 2 && GUILayout.Button("+", GUILayout.Width(40)))
			{
#if (DEBUG)
				Main.logger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				SKDatabase.Add(new ShapeKeyEntry(Guid.NewGuid()));

				return;
			}
		}

		private static void DisplayShapeKeyEntriesMenu(SortedDictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			if (TabSelection != 2 && GUILayout.Button("Add New Shapekey"))
			{
#if (DEBUG)
				Main.logger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				SKDatabase.Add(new ShapeKeyEntry(Guid.NewGuid()));

				return;
			}

			//GUILayout.Label("", seperator);

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values)
			{

				if (Filter != "")
				{
					if (FilterMode == 0)
					{
						if (!Regex.IsMatch(s.EntryName.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
					else if (FilterMode == 1)
					{
						if (!Regex.IsMatch(s.Maid.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
					else if (FilterMode == 2)
					{
						if (!Regex.IsMatch(s.ShapeKey.ToLower(), $@".*{Filter.ToLower()}.*"))
						{
							continue;
						}
					}
				}

				GUILayout.BeginVertical(Sections);

				if (s.Collapsed == false)
				{
					GUILayout.BeginHorizontal(Sections);
					GUILayout.Label(s.EntryName);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("☰"))
					{
						s.Collapsed = !s.Collapsed;
					}
					GUILayout.EndHorizontal();

					s.SetEnabled(GUILayout.Toggle(s.Enabled, $"Enable"));
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("ShapeKey", GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.Label("Maid (Optional)", GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						OpenSKMenu = s.Id;
						ShapekeysNameList = HelperClasses.GetAllShapeKeysFromMaidList(ShapekeyFetcherSetter.GetMaidsList()).ToList();
						ShapekeysNameList.Sort();
						return;
					}

					s.SetShapeKey(GUILayout.TextField(s.ShapeKey, GUILayout.Width(200)));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						OpenMaidMenu = s.Id;
						MaidNameList = HelperClasses.GetNameOfAllMaids().ToList();
						return;
					}
					SKDatabase.SetMaid(s.Id, GUILayout.TextField(s.Maid, GUILayout.Width(200)));

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));
					if (s.AnimateWithExcitement == false && s.SetAnimate(GUILayout.Toggle(s.Animate, $"Animate")))
					{
						GUILayout.Label($"Animation speed = (1000 ms / {s.GetAnimationPoll() * 1000} ms) x {s.GetAnimationRate()} = {1000 / (s.GetAnimationPoll() * 1000) * s.GetAnimationRate()} %/Second");
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.Deform = (GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value));
						GUILayout.Label($"Max Animation Deformation: {s.AnimationMaximum}");
						s.SetAnimationMaximum(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMaximum, s.AnimationMinimum, Main.MaxDeform.Value)));
						GUILayout.Label($"Minimum Animation Deformation: {s.AnimationMinimum}");
						s.SetAnimationMinimum(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMinimum, 0, s.AnimationMaximum)));
						GUILayout.Label($"Animation Rate (Amount to increase/decrease deformation on poll): {s.AnimationRate}");
						GUILayout.BeginHorizontal();
						//s.SetAnimationRate(GUILayout.HorizontalSlider(s.GetAnimationRate(), 0, 100).ToString());
						s.SetAnimationRate(GUILayout.TextField(s.AnimationRate));
						GUILayout.EndHorizontal();
						GUILayout.Label($"Animation Polling Rate in Seconds (Lower = Faster): {s.AnimationPoll}");
						s.SetAnimationPoll(GUILayout.TextField(s.AnimationPoll.ToString()));
					}
					else if (s.Animate == false && s.SetAnimateWithExcitement(GUILayout.Toggle(s.AnimateWithExcitement, $"Animate with yotogi excitement")))
					{

						GUILayout.Label($"Max Excitement Threshold: {s.ExcitementMax}");
						s.SetExcitementMax(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMax, s.ExcitementMin, 300.0F)));
						GUILayout.Label($"Min Excitement Threshold: {s.ExcitementMin}");
						s.SetExcitementMin(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMin, 0.0F, s.ExcitementMax)));

						GUILayout.Label($"Default Shapekey Deformation: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
						GUILayout.Label($"Max Shapekey Deformation: {s.DeformMax}");
						s.SetDeformMax(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMax, s.DeformMin, Main.MaxDeform.Value)));
						GUILayout.Label($"Min Shapekey Deformation: {s.DeformMin}");
						s.SetDeformMin(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMin, 0.0F, s.DeformMax)));
					}
					else
					{
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
					}

					GUILayout.BeginHorizontal(Sections);

					GUILayout.FlexibleSpace();

					if (GUILayout.Button($"Rename"))
					{
						OpenRenameMenu = s.Id;
						return;
					}

					if (GUILayout.Button($"Delete"))
					{
						SKDatabase.Remove(s);
						return;
					}

					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.BeginHorizontal(Sections);
					GUILayout.Label(s.EntryName);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("☰"))
					{
						s.Collapsed = !s.Collapsed;
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
		}

		//From here on down are submenus....

		private static void DisplayShapeKeySelectMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"{s.EntryName} Select ShapeKey");

			if (GUILayout.Button("None"))
			{
				OpenSKMenu = Guid.Empty;
				s.SetShapeKey("");
			}

			foreach (string str in ShapekeysNameList)
			{
				if (GUILayout.Button(str))
				{
					OpenSKMenu = Guid.Empty;
					s.SetShapeKey(str);
				}
			}
		}
		private static void DisplayMaidSelectMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"{s.EntryName} Select Maid");

			if (GUILayout.Button("None"))
			{
				OpenMaidMenu = Guid.Empty;
				SKDatabase.SetMaid(s.Id, "");
			}

			foreach (string mn in MaidNameList)
			{
				if (GUILayout.Button(mn))
				{
					OpenMaidMenu = Guid.Empty;
					SKDatabase.SetMaid(s.Id, mn);
				}
			}
		}
		private static void DisplayMaidGroupCreateMenu()
		{
			GUILayout.Label($"Select Maid To Make New Group For...");

			if (GUILayout.Button("None"))
			{
				int i = 0;

				for (i = 0; SKDatabase.ListOfMaidsWithKeys().Contains($"None {i}"); i++) ;

				var newGUID = Guid.NewGuid();

				SKDatabase.Add(new ShapeKeyEntry(newGUID, $"None {i}"));

				MaidGroupCreateOpen = false;
			}

			foreach (string mn in MaidNameList)
			{
				if (GUILayout.Button(mn))
				{
					Guid newkey = Guid.NewGuid();

					SKDatabase.Add(new ShapeKeyEntry(newkey, mn));

					MaidGroupCreateOpen = false;
				}
			}
		}
		private static void DisplayRenameMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"Now renaming {s.EntryName}");

			s.EntryName = GUILayout.TextField(s.EntryName);

			if (GUILayout.Button("Finish"))
			{
				OpenRenameMenu = Guid.Empty;
			}
		}
		/*
		private static void DisplayGroupRenameMenu(ref string s)
		{
			GUILayout.Label($"Now renaming group: {s}");

			GroupRename = GUILayout.TextField(GroupRename);

			if (GUILayout.Button("Finish"))
			{
				var t = s;

				ShapeKeys.Values.ToList().ForEach(sk =>
				{
					if (sk.Group.Equals(t)) 
					{
						sk.Group = GroupRename;
					}
				});

				RefreshFilteredDictionary();
				RefreshGroups();

				GroupRename = "";
				s = "";
			}
		}
		*/
		private static void DisplayMaidRenameMenu(string s)
		{
			GUILayout.Label($"Now renaming maid group: {s}");

			GUILayout.BeginHorizontal();

			MaidGroupRename = GUILayout.TextField(MaidGroupRename);

			if (GUILayout.Button("Apply"))
			{
				SKDatabase.AllShapekeyDictionary.Values.ToList().ForEach(sk =>
				{
					if (sk.Maid.Equals(s))
					{
						sk.Maid = MaidGroupRename;
					}
				});

				MaidGroupRename = "";
				MaidGroupRenameMenu = "";
			}

			GUILayout.EndHorizontal();

			foreach (string mn in MaidNameList)
			{
				if (GUILayout.Button(mn))
				{
					SKDatabase.AllShapekeyDictionary.Values.ToList().ForEach(sk =>
					{
						if (sk.Maid.Equals(s))
						{
							sk.Maid = mn;
						}
					});

					MaidGroupRename = "";
					MaidGroupRenameMenu = "";
				}
			}
		}
		private static void DisplayExportMenu()
		{
			if (GUILayout.Button("All"))
			{
				Main.SaveToJson(null, SKDatabase, true);
			}
			if (GUILayout.Button("Global"))
			{
				var GlobalOnlyDatabase = new ShapekeyDatabase();

				GlobalOnlyDatabase.AllShapekeyDictionary = SKDatabase.GlobalShapekeyDictionary();

				Main.SaveToJson(null, GlobalOnlyDatabase, true);
			}
			foreach (string m in SKDatabase.ListOfMaidsWithKeys())
			{
				if (GUILayout.Button(m))
				{
					var MaidOnlyDatabase = new ShapekeyDatabase();

					MaidOnlyDatabase.AllShapekeyDictionary = SKDatabase.ShapekeysByMaid(m);

					Main.SaveToJson(null, MaidOnlyDatabase, true);
				}
			}
		}
	}
}

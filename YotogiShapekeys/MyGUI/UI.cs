using System;
using System.Collections.Generic;
using System.IO;
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

		private static Dictionary<string, bool> ThingsToExport = new Dictionary<string, bool>();

		//private static readonly List<Guid> DeleteList = new List<Guid>();

		internal static ShapekeyDatabase SKDatabase = new ShapekeyDatabase();

		private static readonly int WindowID = 777777;

		private static bool runonce = true;
		public static bool changewasmade = false;

		// Used for remembering old values when entering SKMenu
		private static string oldSKMenuFilter = "";
		private static Vector2 oldSKMenuScrollPosition = Vector2.zero;
		private static Vector2 oldPreSKMenuScrollPosition = Vector2.zero;

		private static string Filter = "";
		private static int FilterMode = 0;
		private static int Page = 0;

		private static int TabSelection = 0;

		private static string MaidSelection = "";

		//Used for exporting...
		//private static List<String> MaidNamesWithKeys = new List<string>();

		private static Vector2 scrollPosition = Vector2.zero;
		private static Vector2 scrollPosition1 = Vector2.zero;
		private static Guid OpenSKMenu;
		private static Guid OpenMaidMenu;
		private static Guid OpenRenameMenu;
		private static Guid OpenSlotConditions;
		private static string MaidGroupRenameMenu;
		private static string MaidGroupRename;
		private static bool MaidGroupCreateOpen;
		private static bool ExportMenuOpen;

		private static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 4, Screen.width / 3f, Screen.height / 1.5f);

		private static GUIStyle seperator;
		private static GUIStyle MainWindow;
		private static GUIStyle Sections;
		private static GUIStyle Sections2;

		public static void Initialize()
		{
			if (runonce)
			{
				seperator = new GUIStyle(GUI.skin.horizontalSlider);
				seperator.fixedHeight = 1f;
				seperator.normal.background = MyGUI.Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.8f));
				seperator.margin.top = 10;
				seperator.margin.bottom = 10;

				MainWindow = new GUIStyle(GUI.skin.window);
				MainWindow.normal.background = MyGUI.Helpers.MakeWindowTex(new Color(0, 0, 0, 0.05f), new Color(0, 0, 0, 0.5f));
				MainWindow.normal.textColor = new Color(1, 1, 1, 0.05f);
				MainWindow.hover.background = MyGUI.Helpers.MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.3f), new Color(1, 0, 0, 0.5f));
				MainWindow.hover.textColor = new Color(1, 1, 1, 0.3f);
				MainWindow.onNormal.background = MyGUI.Helpers.MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.6f), new Color(1, 0, 0, 0.5f));

				Sections = new GUIStyle(GUI.skin.box);
				Sections.normal.background = MyGUI.Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));
				Sections2 = new GUIStyle(GUI.skin.box);
				Sections2.normal.background = MyGUI.Helpers.MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

				runonce = false;
			}
			windowRect = GUILayout.Window(WindowID, windowRect, GuiWindowControls, "ShapekeyMaster", MainWindow);
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
			else if (OpenSKMenu != Guid.Empty)
			{
				DisplayShapeKeySelectMenu(SKDatabase.AllShapekeyDictionary[OpenSKMenu]);
			}
			else if (OpenMaidMenu != Guid.Empty)
			{
				DisplayMaidSelectMenu(SKDatabase.AllShapekeyDictionary[OpenMaidMenu]);
			}
			else if (OpenRenameMenu != Guid.Empty)
			{
				DisplayRenameMenu(SKDatabase.AllShapekeyDictionary[OpenRenameMenu]);
			}
			else if (OpenSlotConditions != Guid.Empty)
			{
				DisplaySlotConditionsMenu(SKDatabase.AllShapekeyDictionary[OpenSlotConditions]);
			}
			else if (!String.IsNullOrEmpty(MaidGroupRenameMenu))
			{
				DisplayMaidRenameMenu(MaidGroupRenameMenu);
			}
			else if (MaidGroupCreateOpen)
			{
				DisplayMaidGroupCreateMenu(SKDatabase.AllShapekeyDictionary);
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
							if (GUILayout.Button("<<"))
							{
								Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
							}
							GUILayout.FlexibleSpace();
							GUILayout.Label($"Globals:{Page} ~ {Page + Main.EntriesPerPage.Value}");
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(">>"))
							{
								Page = Math.Min(SKDatabase.GlobalShapekeyDictionary().Count - Main.EntriesPerPage.Value, Page + Main.EntriesPerPage.Value);
							}
							GUILayout.EndHorizontal();

							SimpleDisplayShapeKeyEntriesMenu(SKDatabase.GlobalShapekeyDictionary());
							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(Sections);
							GUILayout.BeginHorizontal(Sections2);
							GUILayout.FlexibleSpace();
							if (GUILayout.Button("<<"))
							{
								Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
							}
							GUILayout.FlexibleSpace();
							GUILayout.Label($"Globals:{Page} ~ {Page + Main.EntriesPerPage.Value}");
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(">>"))
							{
								Page = Math.Min(SKDatabase.GlobalShapekeyDictionary().Count - Main.EntriesPerPage.Value, Page + Main.EntriesPerPage.Value);
							}
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
							if (GUILayout.Button("<<"))
							{
								Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
							}
							GUILayout.FlexibleSpace();
							GUILayout.Label($"All:{Page} ~ {Page + Main.EntriesPerPage.Value}");
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(">>"))
							{
								Page = Math.Min(SKDatabase.AllShapekeyDictionary.Count - Main.EntriesPerPage.Value, Page + Main.EntriesPerPage.Value);
							}
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
							if (GUILayout.Button("<<"))
							{
								Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
							}
							GUILayout.FlexibleSpace();
							GUILayout.Label($"All:{Page} ~ {Page + Main.EntriesPerPage.Value}");
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(">>"))
							{
								Page = Math.Min(SKDatabase.AllShapekeyDictionary.Count - Main.EntriesPerPage.Value, Page + Main.EntriesPerPage.Value);
							}
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
				Page = 0;
				TabSelection = 0;
			}
			else if (GUILayout.Button("Globals"))
			{
				Page = 0;
				TabSelection = 1;
			}
			else if (GUILayout.Button("Maids"))
			{
				Page = 0;
				TabSelection = 2;
			}

			GUILayout.EndHorizontal();

			DisplaySearchMenu();
		}
		private static void DisplaySearchMenu(bool NoModes = false)
		{
			GUILayout.BeginHorizontal(Sections2);

			GUILayout.Label("Search by ");

			GUILayout.FlexibleSpace();

			if (NoModes == false)
			{
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
				if (GUILayout.Button("Reload"))
				{
					OpenSKMenu = Guid.Empty;
					OpenMaidMenu = Guid.Empty;
					OpenRenameMenu = Guid.Empty;
					OpenSlotConditions = Guid.Empty;

					SKDatabase.OverwriteDictionary(Main.LoadFromJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", false));

					return;
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

					return;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}

		private static void DisplayMaidOptions()
		{
			GUILayout.BeginVertical(Sections);

			foreach (String MaidWithKey in SKDatabase.ListOfMaidsWithKeys().OrderBy(maid => maid))
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
						sk.Enabled = targettoggle;
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
						Page = 0;
						MaidSelection = MaidWithKey;
					}
					else
					{
						MaidSelection = "";
						Page = 0;
					}

					return;
				}

				GUILayout.EndHorizontal();

				if (MaidSelection.Contains(MaidWithKey))
				{
					GUILayout.BeginHorizontal(Sections2);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("<<"))
					{
						Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
					}
					GUILayout.FlexibleSpace();
					GUILayout.Label($"{Page} ~ {Page + Main.EntriesPerPage.Value}");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(">>"))
					{
						Page = Math.Min(SKDatabase.ShapekeysByMaid(MaidWithKey).Count - Main.EntriesPerPage.Value, Page + Main.EntriesPerPage.Value);
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

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

		private static void SimpleDisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			int i = 0;

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.OrderBy(val => val.EntryName))
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

				if (i++ < Page)
				{
					continue;
				}
				else if (i > Page + Main.EntriesPerPage.Value)
				{
					break;
				}

				GUILayout.BeginVertical(Sections);

				GUILayout.BeginHorizontal();
				s.Enabled = GUILayout.Toggle(s.Enabled, "I/O", GUILayout.Width(40));

				if (GUILayout.Button("+", GUILayout.Width(40)))
				{

					if (String.IsNullOrEmpty(s.Maid))
					{
						ShapekeysNameList = HelperClasses.GetAllShapeKeysFromAllMaids().ToList();
					}
					else
					{
						ShapekeysNameList = HelperClasses.GetAllShapeKeysFromMaid(HelperClasses.GetMaidByName(s.Maid)).ToList();

						if (ShapekeysNameList == null || ShapekeysNameList.Count == 0)
						{
							ShapekeysNameList = HelperClasses.GetAllShapeKeysFromAllMaids().ToList();
						}
					}

					OpenSKMenu = s.Id;
					Filter = oldSKMenuFilter;
					oldPreSKMenuScrollPosition = scrollPosition;
					scrollPosition = oldSKMenuScrollPosition;
					ShapekeysNameList.Sort();
				}

				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(120));

				s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));

				GUILayout.Label(s.Deform.ToString(), GUILayout.Width(30));

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(Sections);

				if (GUILayout.Button($"Open Slot Conditions Menu"))
				{
					OpenSlotConditions = s.Id;
					return;
				}

				s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, "Enable Conditionals");

				GUILayout.FlexibleSpace();

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
				Guid activeGUID = Guid.NewGuid();

				SKDatabase.Add(new ShapeKeyEntry(activeGUID));

				FocusToNewKey(activeGUID, GivenShapeKeys);

				return;
			}
		}

		private static void DisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			if (TabSelection != 2 && GUILayout.Button("Add New Shapekey"))
			{
#if (DEBUG)
				Main.logger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				var activeGUID = Guid.NewGuid();

				SKDatabase.Add(new ShapeKeyEntry(activeGUID));

				FocusToNewKey(activeGUID, GivenShapeKeys);

				return;
			}

			int i = 0;

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.OrderBy(val => val.EntryName))
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

				if (i++ < Page)
				{
					continue;
				}
				else if (i > Page + Main.EntriesPerPage.Value)
				{
					break;
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

					s.Enabled = GUILayout.Toggle(s.Enabled, $"Enable");
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
						if (String.IsNullOrEmpty(s.Maid))
						{
							ShapekeysNameList = HelperClasses.GetAllShapeKeysFromAllMaids().ToList();
						}
						else
						{
							ShapekeysNameList = HelperClasses.GetAllShapeKeysFromMaid(HelperClasses.GetMaidByName(s.Maid)).ToList();

							if (ShapekeysNameList == null || ShapekeysNameList.Count == 0)
							{
								ShapekeysNameList = HelperClasses.GetAllShapeKeysFromAllMaids().ToList();
							}
						}
						Filter = oldSKMenuFilter;
						oldPreSKMenuScrollPosition = scrollPosition;
						scrollPosition = oldSKMenuScrollPosition;
						ShapekeysNameList.Sort();
						return;
					}

					s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(200));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						OpenMaidMenu = s.Id;
						MaidNameList = HelperClasses.GetNameOfAllMaids().ToList();
						return;
					}
					GUILayout.Label(s.Maid, GUILayout.Width(200));

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));

					s.Animate = GUILayout.Toggle(s.Animate, $"Animate");

					s.AnimateWithExcitement = GUILayout.Toggle(s.AnimateWithExcitement, $"Animate with yotogi excitement");

					if (s.Animate)
					{
						GUILayout.Label($"Animation speed = (1000 ms / {s.AnimationPollFloat * 1000} ms) x {s.AnimationRate} = {1000 / (s.AnimationPollFloat * 1000) * s.AnimationRateFloat} %/Second");
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.Deform = (GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value));
						GUILayout.Label($"Max Animation Deformation: {s.AnimationMaximum}");
						s.AnimationMaximum = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMaximum, s.AnimationMinimum, Main.MaxDeform.Value));
						GUILayout.Label($"Minimum Animation Deformation: {s.AnimationMinimum}");
						s.AnimationMinimum = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMinimum, 0, s.AnimationMaximum));
						GUILayout.Label($"Animation Rate (Amount to increase/decrease deformation on poll): {s.AnimationRate}");
						GUILayout.BeginHorizontal();
						//s.SetAnimationRate(GUILayout.HorizontalSlider(s.GetAnimationRate(), 0, 100).ToString());
						s.AnimationRate = GUILayout.TextField(s.AnimationRate.ToString());
						GUILayout.EndHorizontal();
						GUILayout.Label($"Animation Polling Rate in Seconds (Lower = Faster): {s.AnimationPoll}");
						s.AnimationPoll = GUILayout.TextField(s.AnimationPoll.ToString());
					}
					else if (s.Animate == false && s.AnimateWithExcitement)
					{

						GUILayout.Label($"Max Excitement Threshold: {s.ExcitementMax}");
						s.ExcitementMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMax, s.ExcitementMin, 300.0F));

						GUILayout.Label($"Min Excitement Threshold: {s.ExcitementMin}");
						s.ExcitementMin = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMin, 0.0F, s.ExcitementMax));

						GUILayout.Label($"Default Shapekey Deformation: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
						GUILayout.Label($"Max Shapekey Deformation: {s.DeformMax}");
						s.DeformMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMax, s.DeformMin, Main.MaxDeform.Value));
						GUILayout.Label($"Min Shapekey Deformation: {s.DeformMin}");
						s.DeformMin = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMin, 0.0F, s.DeformMax));
					}
					else
					{
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
					}

					GUILayout.BeginHorizontal(Sections);

					if (GUILayout.Button($"Open Slot Conditions Menu"))
					{
						OpenSlotConditions = s.Id;
						return;
					}

					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, "Enable Conditionals");

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

			DisplaySearchMenu(true);

			GUILayout.Label($"{s.EntryName} Select ShapeKey");

			if (GUILayout.Button("None"))
			{
				OpenSKMenu = Guid.Empty;
				oldSKMenuFilter = Filter;
				oldSKMenuScrollPosition = scrollPosition;
				scrollPosition = oldPreSKMenuScrollPosition;

				s.ShapeKey = "";
				Filter = "";
			}

			GUILayout.BeginVertical(Sections);
			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Body Keys");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			foreach (string str in ShapekeysNameList.Where(key => !HelperClasses.IsFaceKey(key)))
			{
				if (Filter != "")
				{
					if (!Regex.IsMatch(str.ToLower(), $@".*{Filter.ToLower()}.*"))
					{
						continue;
					}
				}

				if (GUILayout.Button(str))
				{
					OpenSKMenu = Guid.Empty;
					s.ShapeKey = str;
					oldSKMenuFilter = Filter;
					oldSKMenuScrollPosition = scrollPosition;
					scrollPosition = oldPreSKMenuScrollPosition;
					Filter = "";
				}
			}

			GUILayout.EndVertical();

			GUILayout.BeginVertical(Sections);
			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Face Keys");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			foreach (string str in ShapekeysNameList.Where(key => HelperClasses.IsFaceKey(key)))
			{
				if (Filter != "")
				{
					if (!Regex.IsMatch(str.ToLower(), $@".*{Filter.ToLower()}.*"))
					{
						continue;
					}
				}

				if (GUILayout.Button(str))
				{
					OpenSKMenu = Guid.Empty;
					s.ShapeKey = str;
					oldSKMenuFilter = Filter;
					oldSKMenuScrollPosition = scrollPosition;
					scrollPosition = oldPreSKMenuScrollPosition;
					Filter = "";
				}
			}
			GUILayout.EndHorizontal();
		}
		private static void DisplayMaidSelectMenu(ShapeKeyEntry s)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"{s.EntryName} Select Maid");

			GUILayout.BeginHorizontal(Sections2);

			s.Maid = GUILayout.TextField(s.Maid, GUILayout.Width(200));

			if (GUILayout.Button("Finish", GUILayout.Width(80)))
			{
				OpenMaidMenu = Guid.Empty;
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button("None"))
			{
				OpenMaidMenu = Guid.Empty;
				s.Maid = "";
				Filter = "";
			}

			foreach (string mn in MaidNameList)
			{
				if (Filter != "")
				{
					if (!Regex.IsMatch(mn.ToLower(), $@".*{Filter.ToLower()}.*"))
					{
						continue;
					}
				}

				if (GUILayout.Button(mn))
				{
					OpenMaidMenu = Guid.Empty;
					s.Maid = mn;
					Filter = "";
				}
			}
		}
		private static void DisplayMaidGroupCreateMenu(Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"Select Maid To Make New Group For...");

			if (GUILayout.Button("None"))
			{
				int i = 0;

				for (i = 0; SKDatabase.ListOfMaidsWithKeys().Contains($"None {i}"); i++) ;

				var newGUID = Guid.NewGuid();

				SKDatabase.Add(new ShapeKeyEntry(newGUID, $"None {i}"));

				FocusToNewKey(newGUID, GivenShapeKeys);

				MaidGroupCreateOpen = false;

				Filter = "";
			}

			foreach (string mn in MaidNameList)
			{
				if (Filter != "")
				{
					if (!Regex.IsMatch(mn.ToLower(), $@".*{Filter.ToLower()}.*"))
					{
						continue;
					}
				}

				if (GUILayout.Button(mn))
				{
					Guid newkey = Guid.NewGuid();

					SKDatabase.Add(new ShapeKeyEntry(newkey, mn));

					FocusToNewKey(newkey, GivenShapeKeys);

					MaidGroupCreateOpen = false;

					Filter = "";
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
		private static void DisplaySlotConditionsMenu(ShapeKeyEntry s)
		{
			GUILayout.BeginVertical(Sections);

			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label("General Settings");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, "Enable Conditionals");

			if (s.DisableWhen)
			{
				s.DisableWhen = GUILayout.Toggle(s.DisableWhen, "Set Shapekey if");
			}
			else
			{
				s.DisableWhen = GUILayout.Toggle(s.DisableWhen, "Do Not Set Shapekey if");
			}

			GUILayout.Label("Disabled Key Deform");
			s.DisabledDeform = GUILayout.HorizontalSlider(s.DisabledDeform, 0, Main.MaxDeform.Value);

			if (s.WhenAll)
			{
				s.WhenAll = GUILayout.Toggle(s.WhenAll, "If All in Slots is Equipped");
			}
			else
			{
				s.WhenAll = GUILayout.Toggle(s.WhenAll, "If Any in Slots is Equipped");
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(Sections);

			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Slots");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			int i = 0;

			foreach (DisableWhenEquipped slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
			{

				if (i == 0)
				{
					GUILayout.BeginHorizontal();
				}

				var temp = GUILayout.Toggle(HelperClasses.HasFlag(s.SlotFlags, slot), slot.ToString(), GUILayout.Width(100));

				if (i++ == 4)
				{
					GUILayout.EndHorizontal();
					i = 0;
				}

				if (temp == true)
				{
					s.SlotFlags |= slot;
				}
				else
				{
					s.SlotFlags &= ~slot;
				}
			}

			if (i != 0)
			{
				GUILayout.EndHorizontal();
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(Sections);

			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Menu Files");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1);

			var TempMenuFileConditions = new Dictionary<Guid, string>(s.MenuFileConditionals);

			foreach (Guid menu in TempMenuFileConditions.Keys)
			{
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("X", GUILayout.Width(20)))
				{
					s.MenuFileConditionals.Remove(menu);
					return;
				}

				s.MenuFileConditionals[menu] = GUILayout.TextField(s.MenuFileConditionals[menu]);

				GUILayout.EndHorizontal();

			}

			GUILayout.EndScrollView();

			if (GUILayout.Button("Add New Menu File"))
			{
				s.MenuFileConditionals[Guid.NewGuid()] = "";
			}

			GUILayout.EndVertical();

			if (GUILayout.Button("Finish"))
			{
				OpenSlotConditions = Guid.Empty;
			}
		}
		private static void DisplayMaidRenameMenu(string s)
		{
			DisplaySearchMenu(true);

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
				Filter = "";
			}

			GUILayout.EndHorizontal();

			foreach (string mn in MaidNameList)
			{
				if (Filter != "")
				{
					if (!Regex.IsMatch(mn.ToLower(), $@".*{Filter.ToLower()}.*"))
					{
						continue;
					}
				}

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
					Filter = "";
				}
			}
		}
		private static void DisplayExportMenu()
		{
			Main.HideInactiveMaids.Value = GUILayout.Toggle(Main.HideInactiveMaids.Value, "Hide Inactive Maids");

			if (Main.HideInactiveMaids.Value == false && GUILayout.Button("All"))
			{
				Main.logger.LogMessage("Exporting All");

				Main.SaveToJson(null, SKDatabase, true);
			}

			GUILayout.BeginVertical(Sections);

			if (ThingsToExport.ContainsKey("global") == false)
			{
				ThingsToExport["global"] = false;
			}

			ThingsToExport["global"] = GUILayout.Toggle(ThingsToExport["global"], "Global");

			foreach (string m in SKDatabase.ListOfMaidsWithKeys())
			{
				if (Main.HideInactiveMaids.Value == false || HelperClasses.IsMaidActive(m))
				{
					if (ThingsToExport.ContainsKey(m) == false)
					{
						ThingsToExport[m] = false;
					}

					ThingsToExport[m] = GUILayout.Toggle(ThingsToExport[m], m);
				}
			}

			GUILayout.EndVertical();

			if (GUILayout.Button("Done"))
			{
				var SelectionDataBase = new ShapekeyDatabase();

				if (ThingsToExport.TryGetValue("global", out var toExport) && toExport == true)
				{
					SelectionDataBase.AllShapekeyDictionary = SKDatabase.GlobalShapekeyDictionary();
				}

				foreach (string m in SKDatabase.ListOfMaidsWithKeys())
				{
					if (ThingsToExport.TryGetValue(m, out var toExport1) && toExport1 == true)
					{
						SelectionDataBase.AllShapekeyDictionary = SelectionDataBase.AllShapekeyDictionary.Concat(SKDatabase.ShapekeysByMaid(m)).ToDictionary(r => r.Key, f => f.Value);
					}
				}

				if (SelectionDataBase.AllShapekeyDictionary.Count > 0)
				{
					Main.SaveToJson(null, SelectionDataBase, true);

					ExportMenuOpen = false;
				}

				ThingsToExport.Clear();
			}
		}

		//UI Helper funcs
		internal static void FocusToNewKey(Guid guid, Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			double pos = 0;

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.OrderBy(val => val.EntryName))
			{
				if (s.Id != guid)
				{
					++pos;
				}
				else
				{
					Page = ((int)Math.Floor(pos / 10)) * 10;
				}
			}
		}
	}
}
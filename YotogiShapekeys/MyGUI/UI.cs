using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ShapekeyMaster.MyGUI.Helpers;

namespace ShapekeyMaster
{
	internal static class UI
	{
		private static DateTime? TimeToKillNewKeyShine;
		private static Guid? NewKeyToShine;

		private static List<Tuple<string, string>> ShapekeysNameList = new List<Tuple<string, string>>();
		private static List<string> MaidNameList = new List<string>();

		private static Dictionary<string, bool> ThingsToExport = new Dictionary<string, bool>();

		internal static ShapekeyDatabase SKDatabase = new ShapekeyDatabase();

		public static readonly int WindowID = 777777;

		private static bool runonce = true;
		public static bool changewasmade = false;

		// Used for remembering old values when entering SKMenu
		private static string oldSKMenuFilter = "";

		private static Vector2 oldSKMenuScrollPosition = Vector2.zero;
		private static Vector2 oldPreSKMenuScrollPosition = Vector2.zero;

		private static string Filter = "";
		private static int FilterMode = 0;
		private static int Page = 0;
		private static bool FilterCommonKeys = true;
		private static bool HideBlacklistedKeys = true;

		private static int TabSelection = 0;

		private static string MaidSelection = "";

		private static readonly ShapekeyEntryComparer entryComparer = new ShapekeyEntryComparer();

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

		public static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 4, Screen.width / 3f, Screen.height / 1.5f);
		private static int currentHeight = 0;
		private static int currentWidth = 0;

		private static GUIStyle seperator;
		private static GUIStyle MainWindow;
		private static GUIStyle Sections;
		private static GUIStyle Sections2;

		private static GUIStyle ShineSections;
		private static GUIStyle BlacklistedButton;
		//private static GUIStyle PreviewButton;

		public static void Initialize()
		{
			//Setup some UI properties.
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

				ShineSections = new GUIStyle(GUI.skin.box);
				ShineSections.normal.background = MyGUI.Helpers.MakeTex(2, 2, new Color(0.92f, 0.74f, 0.2f, 0.3f));

				BlacklistedButton = new GUIStyle(GUI.skin.button);
				BlacklistedButton.normal.textColor = Color.red;
				BlacklistedButton.hover.textColor = Color.red;
				BlacklistedButton.active.textColor = Color.red;

				/*
				PreviewButton = new GUIStyle(GUI.skin.button);
				PreviewButton.normal.textColor = Color.yellow;
				PreviewButton.hover.textColor = Color.yellow;
				PreviewButton.active.textColor = Color.yellow;
				*/

				entryComparer.Mode = 0;

				runonce = false;
			}

			//Sometimes the UI can be improperly sized, this sets it to some measurements.
			if (currentHeight != Screen.height || currentWidth != Screen.width)
			{
				windowRect.height = Math.Max(Screen.height / 1.5f, 200);
				windowRect.width = Math.Max(Screen.width / 3f, 500);

				windowRect.y = Screen.height / 4;
				windowRect.x = Screen.width / 3;

				Main.logger.LogDebug($"Changing sizes of SKM UI to {windowRect.width} x {windowRect.height}");

				currentHeight = Screen.height;
				currentWidth = Screen.width;
			}

			windowRect = GUILayout.Window(WindowID, windowRect, GuiWindowControls, "ShapekeyMaster", MainWindow);
		}

		private static void GuiWindowControls(int windowID)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

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
				//This handles the shine of the UI.
				if (NewKeyToShine.HasValue && TimeToKillNewKeyShine < DateTime.Now)
				{
					NewKeyToShine = null;
					TimeToKillNewKeyShine = null;
				}

				DisplayHeaderMenu();

				Main.SimpleMode.Value = GUILayout.Toggle(Main.SimpleMode.Value, "Simple");

				switch (TabSelection)
				{
					case 1:
						if (Main.SimpleMode.Value)
						{
							GUILayout.BeginVertical(Sections);

							DisplayPageManager();

							SimpleDisplayShapeKeyEntriesMenu(SKDatabase.GlobalShapekeyDictionary());

							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(Sections);

							DisplayPageManager();

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

							DisplayPageManager();

							SimpleDisplayShapeKeyEntriesMenu(SKDatabase.AllShapekeyDictionary);

							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(Sections);

							DisplayPageManager();

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

			var modeLabel = entryComparer.Mode == 0 ? "Date" : entryComparer.Mode == 1 ? "Name" : "Shapekey";
			var ascendLabel = entryComparer.Ascending ? "↑" : "↓";

			GUILayout.Label("Sort By:");
			if (GUILayout.Button(modeLabel))
			{
				++entryComparer.Mode;
			}
			if (GUILayout.Button(ascendLabel))
			{
				entryComparer.Ascending = !entryComparer.Ascending;
			}

			GUILayout.FlexibleSpace();

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

			if (OpenSKMenu != Guid.Empty)
			{
				FilterCommonKeys = GUILayout.Toggle(FilterCommonKeys, "Filter Common Keys");

				HideBlacklistedKeys = GUILayout.Toggle(HideBlacklistedKeys, "Hide Blacklisted Keys");
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label("Search by ");

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

		private static void DisplayPageManager(string MaidWithKey = null)
		{
			string headerString;
			int applicantcount;

			switch (TabSelection)
			{
				case 1:
					headerString = "Globals";
					applicantcount = SKDatabase.GlobalShapekeyDictionary().Count;
					break;

				case 2:
					headerString = MaidWithKey;
					applicantcount = SKDatabase.ShapekeysByMaid(MaidWithKey).Count;
					break;

				default:
					headerString = "All";
					applicantcount = SKDatabase.AllShapekeyDictionary.Count;
					break;
			}

			GUILayout.BeginHorizontal(Sections2);
			if (GUILayout.Button("<<"))
			{
				Page = Math.Max(Page - Main.EntriesPerPage.Value, 0);
			}
			GUILayout.FlexibleSpace();
			GUILayout.Label($"{headerString}:{Page} ~ {Page + Main.EntriesPerPage.Value}");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(">>"))
			{
				if (Page + Main.EntriesPerPage.Value < applicantcount)
				{
					Page = Page + Main.EntriesPerPage.Value;
				}
			}
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

					var serDB = Main.LoadFromJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", false);

					SKDatabase.OverwriteDictionary(serDB.AllShapekeyDictionary);
					SKDatabase.BlacklistedShapekeys = serDB.BlacklistedShapekeys;

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
					SKDatabase.ConcatenateDictionary(Main.LoadFromJson(null, true).AllShapekeyDictionary);

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
						if (MaidWithKey.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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
					DisplayPageManager(MaidWithKey);

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
						var id = Guid.NewGuid();
						SKDatabase.Add(new ShapeKeyEntry(id, MaidWithKey));

						FocusToNewKey(id, SKDatabase.AllShapekeyDictionary, MaidWithKey);

						return;
					}

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}

			if (GUILayout.Button("New Maid Group"))
			{
				MaidNameList = HelperClasses.GetNameOfAllMaids().ToList();

				MaidGroupCreateOpen = true;

				return;
			}

			GUILayout.EndVertical();
		}

		private static void SimpleDisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys)
		{
			int i = 0;

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.OrderBy(val => val, entryComparer))
			{
				if (Filter != "")
				{
					if (FilterMode == 0)
					{
						if (s.EntryName.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
						{
							continue;
						}
					}
					else if (FilterMode == 1)
					{
						if (s.Maid.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
						{
							continue;
						}
					}
					else if (FilterMode == 2)
					{
						if (s.ShapeKey.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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

				var Style = NewKeyToShine.HasValue && s.Id == NewKeyToShine ? ShineSections : Sections;

				GUILayout.BeginVertical(Style);

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
					//ShapekeysNameList.Sort();
				}

				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(120));

				s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));

				GUILayout.Label(s.Deform.ToString(), GUILayout.Width(30));

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(Style);

				if (GUILayout.Button($"Open Slot Conditions Menu"))
				{
					OpenSlotConditions = s.Id;
					return;
				}

				s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, "Enable Conditionals");

				GUILayout.FlexibleSpace();

				string cateNameLabel = !String.IsNullOrEmpty(s.EntryName) ? s.EntryName : "";
				cateNameLabel += TabSelection == 0 && !String.IsNullOrEmpty(cateNameLabel) ? " | " : "";
				cateNameLabel += TabSelection == 0 ? String.IsNullOrEmpty(s.Maid) ? "Global" : s.Maid : "";

				GUILayout.Label(cateNameLabel);

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

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.OrderBy(val => val, entryComparer))
			{
				if (Filter != "")
				{
					if (FilterMode == 0)
					{
						if (s.EntryName.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
						{
							continue;
						}
					}
					else if (FilterMode == 1)
					{
						if (s.Maid.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
						{
							continue;
						}
					}
					else if (FilterMode == 2)
					{
						if (s.ShapeKey.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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

				var Style = NewKeyToShine.HasValue && s.Id == NewKeyToShine ? ShineSections : Sections;

				GUILayout.BeginVertical(Style);

				if (s.Collapsed == false)
				{
					GUILayout.BeginHorizontal(Style);
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
						//ShapekeysNameList.Sort();

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

					GUILayout.BeginHorizontal(Style);

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

			GUILayout.BeginHorizontal();

			GUILayout.Label($"{s.EntryName} Select ShapeKey");

			GUILayout.FlexibleSpace();

			GUILayout.Label($"R-Click To Blacklist!");

			GUILayout.EndHorizontal();

			if (GUILayout.Button("None"))
			{
				OpenSKMenu = Guid.Empty;
				oldSKMenuFilter = Filter;
				oldSKMenuScrollPosition = scrollPosition;
				scrollPosition = oldPreSKMenuScrollPosition;

				s.ShapeKey = "";
				Filter = "";
			}

			int columns = 0;
			int totalGroupsWorked = 0;

			var groupedKeys = ShapekeysNameList
				.GroupBy(r => r.First)
				.OrderBy(r => r.Key);

			var bodyKeys = groupedKeys
				.Where(r => r.Key.Equals("body"))
				.SelectMany(r => r)
				.Select(t => t.Second)
				.ToList();

			var headKeys = groupedKeys
				.Where(r => r.Key.Equals("head"))
				.SelectMany(r => r)
				.Select(t => t.Second)
				.ToList();

			foreach (var group in groupedKeys)
			{
				totalGroupsWorked++;

				var filteredList = group.ToList();

				if (FilterCommonKeys)
				{
					if (group.Key.Equals("body") == false)
					{
						filteredList = filteredList.Where(t => !bodyKeys.Contains(t.Second)).ToList();
					}

					if (group.Key.Equals("head") == false)
					{
						filteredList = filteredList.Where(t => !headKeys.Contains(t.Second)).ToList();
					}
				}

				if (HideBlacklistedKeys)
				{
					filteredList = filteredList.Where(t => SKDatabase.BlacklistedShapekeys.IsBlacklisted(t.Second) == false).ToList();
				}

				if (filteredList.Count > 0)
				{
					if (columns++ == 0)
					{
						//Columnar ordering
						GUILayout.BeginHorizontal();
					}

					//Category vertical
					GUILayout.BeginVertical(Sections);

					//Header for category
					GUILayout.BeginHorizontal(Sections2);
					GUILayout.FlexibleSpace();
					GUILayout.Label($"{group.Key}");
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					foreach (var str in filteredList.OrderBy(r => r.Second))
					{
						if (Filter != "")
						{
							if (str.Second.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
							{
								continue;
							}
						}

						var style = HideBlacklistedKeys == false && SKDatabase.BlacklistedShapekeys.IsBlacklisted(str.Second) ? BlacklistedButton : GUI.skin.button;
						if (GUILayout.Button(str.Second, style))
						{
							/*
							if (LastMouseButtonUp == 2)
							{
								ShapekeyUpdate.PreviewKey(str.Second);
							}
							*/
							if (LastMouseButtonUp == 1)
							{
								if (SKDatabase.BlacklistedShapekeys.IsBlacklisted(str.Second) == false)
								{
									SKDatabase.BlacklistedShapekeys.AddItem(str.Second);
								}
								else
								{
									SKDatabase.BlacklistedShapekeys.RemoveItem(str.Second);
								}
							}
							else if (LastMouseButtonUp == 0)
							{
								OpenSKMenu = Guid.Empty;
								s.ShapeKey = str.Second;
								oldSKMenuFilter = Filter;
								oldSKMenuScrollPosition = scrollPosition;
								scrollPosition = oldPreSKMenuScrollPosition;
								Filter = "";
							}
						}
					}

					GUILayout.EndVertical();
				}

				if (columns == 3 || totalGroupsWorked == groupedKeys.Count() && columns > 0)
				{
					GUILayout.EndHorizontal();
					columns = 0;
				}
			}
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
					if (mn.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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
					if (mn.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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

			s.IgnoreCategoriesWithShapekey = GUILayout.Toggle(s.IgnoreCategoriesWithShapekey, "Ignore Categories With Shapekey");

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
					if (mn.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
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
		internal static void FocusToNewKey(Guid guid, Dictionary<Guid, ShapeKeyEntry> GivenShapeKeys, string maid = null)
		{
			NewKeyToShine = guid;
			TimeToKillNewKeyShine = DateTime.Now.AddSeconds(3);

			double pos = 0;

			foreach (ShapeKeyEntry s in GivenShapeKeys.Values.Where(r => maid.IsNullOrWhiteSpace() || r.Maid.Equals(maid)).OrderBy(val => val, entryComparer))
			{
				if (s.Id != guid)
				{
					++pos;
				}
				else
				{
					Page = ((int)Math.Floor(pos / Main.EntriesPerPage.Value)) * Main.EntriesPerPage.Value;
				}
			}
		}
	}
}
using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RootMotion.FinalIK.IKSolverVR;
using static ShapekeyMaster.MyGUI.UIToolbox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
		private static Rect dragWindow = new Rect(0, 0, 10000, 20);
		private static Rect closeButton = new Rect(0, 0, 25, 15);
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
				seperator.normal.background = MyGUI.UIToolbox.MakeTex(2, 2, new Color(0, 0, 0, 0.8f));
				seperator.margin.top = 10;
				seperator.margin.bottom = 10;

				MainWindow = new GUIStyle(GUI.skin.window);
				MainWindow.normal.background = MyGUI.UIToolbox.MakeWindowTex(new Color(0, 0, 0, 0.05f), new Color(0, 0, 0, 0.5f));
				MainWindow.normal.textColor = new Color(1, 1, 1, 0.05f);
				MainWindow.hover.background = MyGUI.UIToolbox.MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.3f), new Color(1, 0, 0, 0.5f));
				MainWindow.hover.textColor = new Color(1, 1, 1, 0.3f);
				MainWindow.onNormal.background = MyGUI.UIToolbox.MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.6f), new Color(1, 0, 0, 0.5f));

				Sections = new GUIStyle(GUI.skin.box);
				Sections.normal.background = MyGUI.UIToolbox.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));
				Sections2 = new GUIStyle(GUI.skin.box);
				Sections2.normal.background = MyGUI.UIToolbox.MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

				ShineSections = new GUIStyle(GUI.skin.box);
				ShineSections.normal.background = MyGUI.UIToolbox.MakeTex(2, 2, new Color(0.92f, 0.74f, 0.2f, 0.3f));

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

			windowRect = GUILayout.Window(WindowID, windowRect, GuiWindowControls, Main.CurrentLanguage["title"], MainWindow);
		}

		private static void GuiWindowControls(int windowID)
		{
			closeButton.x = windowRect.width - (closeButton.width + 5);
			dragWindow.width = windowRect.width - (closeButton.width + 5);

			GUI.DragWindow(dragWindow);

			if (GUI.Button(closeButton, "X"))
			{
				Main.enablegui = false;
			}

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

				Main.SimpleMode.Value = GUILayout.Toggle(Main.SimpleMode.Value, Main.CurrentLanguage["simple"]);

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
						Main.HideInactiveMaids.Value = GUILayout.Toggle(Main.HideInactiveMaids.Value, Main.CurrentLanguage["hideInactiveMaids"]);

						GUILayout.BeginVertical(Sections);

						GUILayout.BeginHorizontal(Sections2);
						GUILayout.FlexibleSpace();
						GUILayout.Label(Main.CurrentLanguage["maids"]);
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
			ShapekeyMaster.MyGUI.UIToolbox.ChkMouseClick(windowRect);
		}

		private static void DisplayHeaderMenu()
		{
			GUILayout.BeginHorizontal(Sections2);

			var modeLabel = entryComparer.Mode == 0 ? Main.CurrentLanguage["date"] : entryComparer.Mode == 1 ? Main.CurrentLanguage["name"] : Main.CurrentLanguage["shapekey"];
			var ascendLabel = entryComparer.Ascending ? "↑" : "↓";

			GUILayout.Label(Main.CurrentLanguage["sortBy"] + ":");
			if (GUILayout.Button(modeLabel))
			{
				++entryComparer.Mode;
			}
			if (GUILayout.Button(ascendLabel))
			{
				entryComparer.Ascending = !entryComparer.Ascending;
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(Main.CurrentLanguage["all"]))
			{
				Page = 0;
				TabSelection = 0;
			}
			else if (GUILayout.Button(Main.CurrentLanguage["globals"]))
			{
				Page = 0;
				TabSelection = 1;
			}
			else if (GUILayout.Button(Main.CurrentLanguage["maids"]))
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
				FilterCommonKeys = GUILayout.Toggle(FilterCommonKeys, Main.CurrentLanguage["filterCommonKeys"]);

				HideBlacklistedKeys = GUILayout.Toggle(HideBlacklistedKeys, Main.CurrentLanguage["hideBlacklistedKeys"]);
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label(Main.CurrentLanguage["searchBy"] + ":");

			if (NoModes == false)
			{
				if (FilterMode == 0)
				{
					if (GUILayout.Button(Main.CurrentLanguage["name"]))
					{
						FilterMode = 1;
					}
				}
				else if (FilterMode == 1)
				{
					if (GUILayout.Button(Main.CurrentLanguage["maid"]))
					{
						FilterMode = 2;
					}
				}
				else if (FilterMode == 2)
				{
					if (GUILayout.Button(Main.CurrentLanguage["shapekey"]))
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
					headerString = Main.CurrentLanguage["globals"];
					applicantcount = SKDatabase.GlobalShapekeyDictionary().Count;
					break;

				case 2:
					headerString = MaidWithKey;
					applicantcount = SKDatabase.ShapekeysByMaid(MaidWithKey).Count;
					break;

				default:
					headerString = Main.CurrentLanguage["all"];
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
				if (GUILayout.Button(Main.CurrentLanguage["cancel"]))
				{
					ExportMenuOpen = false;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(Main.CurrentLanguage["reload"]))
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
				if (GUILayout.Button(Main.CurrentLanguage["save"]))
				{
#if (DEBUG)
					Main.logger.LogDebug("Saving data to configs now!");
#endif

					Main.SaveToJson(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", SKDatabase);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(Main.CurrentLanguage["export"]))
				{
					ExportMenuOpen = true;
				}
				if (GUILayout.Button(Main.CurrentLanguage["import"]))
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
					if (!Extensions.IsMaidActive(MaidWithKey))
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

				if (GUILayout.Button(Main.CurrentLanguage["rename"]))
				{
					MaidGroupRenameMenu = MaidWithKey;
					MaidGroupRename = MaidGroupRenameMenu;

					MaidNameList = Extensions.GetNameOfAllMaids().ToList();

					return;
				}

				if (GUILayout.Button(Main.CurrentLanguage["delete"]))
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

			if (GUILayout.Button(Main.CurrentLanguage["newMaidGroup"]))
			{
				MaidNameList = Extensions.GetNameOfAllMaids().ToList();

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
						ShapekeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
					}
					else
					{
						ShapekeysNameList = Extensions.GetMaidByName(s.Maid)?.GetAllShapeKeysFromMaid()?.ToList();

						if (ShapekeysNameList == null || ShapekeysNameList.Count == 0)
						{
							ShapekeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
						}
					}

					OpenSKMenu = s.Id;
					Filter = oldSKMenuFilter;
					oldPreSKMenuScrollPosition = scrollPosition;
					scrollPosition = oldSKMenuScrollPosition;
				}

				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(120));

				s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));

				GUILayout.Label(s.Deform.ToString(), GUILayout.Width(30));

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(Style);

				if (GUILayout.Button(Main.CurrentLanguage["openSlotCondMenu"]))
				{
					OpenSlotConditions = s.Id;
					return;
				}

				s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, Main.CurrentLanguage["enableConditionals"]);

				GUILayout.FlexibleSpace();

				string cateNameLabel = !String.IsNullOrEmpty(s.EntryName) ? s.EntryName : "";
				cateNameLabel += TabSelection == 0 && !String.IsNullOrEmpty(cateNameLabel) ? " | " : "";
				cateNameLabel += TabSelection == 0 ? String.IsNullOrEmpty(s.Maid) ? Main.CurrentLanguage["global"] : s.Maid : "";

				GUILayout.Label(cateNameLabel);

				GUILayout.FlexibleSpace();

				if (GUILayout.Button(Main.CurrentLanguage["delete"]))
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
			if (TabSelection != 2 && GUILayout.Button(Main.CurrentLanguage["addNewShapekey"]))
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

					s.Enabled = GUILayout.Toggle(s.Enabled, Main.CurrentLanguage["enable"]);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label(Main.CurrentLanguage["shapekey"], GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.Label(Main.CurrentLanguage["maidOptional"], GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						OpenSKMenu = s.Id;
						if (String.IsNullOrEmpty(s.Maid))
						{
							ShapekeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
						}
						else
						{
							ShapekeysNameList = Extensions.GetMaidByName(s.Maid)?.GetAllShapeKeysFromMaid()?.ToList();

							if (ShapekeysNameList == null || ShapekeysNameList.Count == 0)
							{
								ShapekeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
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
						MaidNameList = Extensions.GetNameOfAllMaids().ToList();
						return;
					}
					GUILayout.Label(s.Maid, GUILayout.Width(200));

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));

					s.Animate = GUILayout.Toggle(s.Animate, Main.CurrentLanguage["animate"]);

					s.AnimateWithExcitement = GUILayout.Toggle(s.AnimateWithExcitement, Main.CurrentLanguage["animateWithYotogiExcitement"]);

					if (s.Animate)
					{
						GUILayout.Label($"{Main.CurrentLanguage["animationSpeed"]} = (1000 ms / {s.AnimationPollFloat * 1000} ms) x {s.AnimationRate} = {1000 / (s.AnimationPollFloat * 1000) * s.AnimationRateFloat} %/{Main.CurrentLanguage["seconds"]}");
						GUILayout.Label($"{Main.CurrentLanguage["shapekeyDeformation"]}: {s.Deform}");
						s.Deform = (GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value));
						GUILayout.Label($"{Main.CurrentLanguage["maxAnimationDeformation"]}: {s.AnimationMaximum}");
						s.AnimationMaximum = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMaximum, s.AnimationMinimum, Main.MaxDeform.Value));
						GUILayout.Label($"{Main.CurrentLanguage["minAnimationDeformation"]}: {s.AnimationMinimum}");
						s.AnimationMinimum = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMinimum, 0, s.AnimationMaximum));
						GUILayout.Label($"{Main.CurrentLanguage["animationRate"]}: {s.AnimationRate}");
						GUILayout.BeginHorizontal();
						//s.SetAnimationRate(GUILayout.HorizontalSlider(s.GetAnimationRate(), 0, 100).ToString());
						s.AnimationRate = GUILayout.TextField(s.AnimationRate.ToString());
						GUILayout.EndHorizontal();
						GUILayout.Label($"{Main.CurrentLanguage["animationPollingRate"]}: {s.AnimationPoll}");
						s.AnimationPoll = GUILayout.TextField(s.AnimationPoll.ToString());
					}
					else if (s.Animate == false && s.AnimateWithExcitement)
					{
						GUILayout.Label($"{Main.CurrentLanguage["maxExcitementThreshold"]}: {s.ExcitementMax}");
						s.ExcitementMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMax, s.ExcitementMin, 300.0F));

						GUILayout.Label($"{Main.CurrentLanguage["minExcitementThreshold"]}: {s.ExcitementMin}");
						s.ExcitementMin = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.ExcitementMin, 0.0F, s.ExcitementMax));

						GUILayout.Label($"{Main.CurrentLanguage["defShapekeyDeformation"]}: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
						GUILayout.Label($"{Main.CurrentLanguage["maxShapekeyDeformation"]}: {s.DeformMax}");
						s.DeformMax = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMax, s.DeformMin, Main.MaxDeform.Value));
						GUILayout.Label($"{Main.CurrentLanguage["minShapekeyDeformation"]}: {s.DeformMin}");
						s.DeformMin = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMin, 0.0F, s.DeformMax));
					}
					else
					{
						GUILayout.Label($"{Main.CurrentLanguage["shapekeyDeformation"]}: {s.Deform}");
						s.Deform = (Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, Main.MaxDeform.Value)));
					}

					GUILayout.BeginHorizontal(Style);

					if (GUILayout.Button(Main.CurrentLanguage["openSlotCondMenu"]))
					{
						OpenSlotConditions = s.Id;
						return;
					}

					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, Main.CurrentLanguage["enableConditionals"]);

					GUILayout.FlexibleSpace();

					if (GUILayout.Button(Main.CurrentLanguage["rename"]))
					{
						OpenRenameMenu = s.Id;
						return;
					}

					if (GUILayout.Button(Main.CurrentLanguage["delete"]))
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

			GUILayout.Label($"{s.EntryName}: {Main.CurrentLanguage["selectShapekey"]}");

			GUILayout.FlexibleSpace();

			GUILayout.Label(Main.CurrentLanguage["rclickToBlacklist"]);

			GUILayout.EndHorizontal();

			if (GUILayout.Button(Main.CurrentLanguage["finish"]))
			{
				OpenSKMenu = Guid.Empty;
				oldSKMenuFilter = Filter;
				oldSKMenuScrollPosition = scrollPosition;
				scrollPosition = oldPreSKMenuScrollPosition;

				Filter = "";
			}

			if (GUILayout.Button(Main.CurrentLanguage["none"]))
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
				.GroupBy(r => r.item1)
				.OrderBy(r => r.Key);

			var bodyKeys = groupedKeys
				.Where(r => r.Key.Equals("body"))
				.SelectMany(r => r)
				.Select(t => t.item2)
				.ToList();

			var headKeys = groupedKeys
				.Where(r => r.Key.Equals("head"))
				.SelectMany(r => r)
				.Select(t => t.item2)
				.ToList();

			foreach (var group in groupedKeys)
			{
				totalGroupsWorked++;

				var filteredList = group.ToList();

				if (FilterCommonKeys)
				{
					if (group.Key.Equals("body") == false)
					{
						filteredList = filteredList.Where(t => !bodyKeys.Contains(t.item2)).ToList();
					}

					if (group.Key.Equals("head") == false)
					{
						filteredList = filteredList.Where(t => !headKeys.Contains(t.item2)).ToList();
					}
				}

				if (HideBlacklistedKeys)
				{
					filteredList = filteredList.Where(t => SKDatabase.BlacklistedShapekeys.IsBlacklisted(t.item2) == false).ToList();
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
					GUILayout.Label(Main.CurrentLanguage[group.Key]);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					foreach (var str in filteredList.OrderBy(r => r.item2))
					{
						if (Filter != "")
						{
							if (str.item2.Contains(Filter, StringComparison.OrdinalIgnoreCase) == false)
							{
								continue;
							}
						}

						var style = HideBlacklistedKeys == false && SKDatabase.BlacklistedShapekeys.IsBlacklisted(str.item2) ? BlacklistedButton : GUI.skin.button;
						if (GUILayout.Button(str.item2, style))
						{
							/*
							if (LastMouseButtonUp == 2)
							{
								ShapekeyUpdate.PreviewKey(str.Second);
							}
							*/
							if (LastMouseButtonUp == 1)
							{
								if (SKDatabase.BlacklistedShapekeys.IsBlacklisted(str.item2) == false)
								{
									SKDatabase.BlacklistedShapekeys.AddItem(str.item2);
								}
								else
								{
									SKDatabase.BlacklistedShapekeys.RemoveItem(str.item2);
								}
							}
							else if (LastMouseButtonUp == 0)
							{
								OpenSKMenu = Guid.Empty;
								s.ShapeKey = str.item2;
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

			GUILayout.Label($"{s.EntryName} {Main.CurrentLanguage["selectMaid"]}");

			GUILayout.BeginHorizontal(Sections2);

			s.Maid = GUILayout.TextField(s.Maid, GUILayout.Width(200));

			if (GUILayout.Button(Main.CurrentLanguage["finish"], GUILayout.Width(80)))
			{
				OpenMaidMenu = Guid.Empty;
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button(Main.CurrentLanguage["none"]))
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

			GUILayout.Label(Main.CurrentLanguage["selectNewMaidGroup"]);

			if (GUILayout.Button(Main.CurrentLanguage["none"]))
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
			GUILayout.Label($"{Main.CurrentLanguage["nowRenaming"]} {s.EntryName}");

			s.EntryName = GUILayout.TextField(s.EntryName);

			if (GUILayout.Button(Main.CurrentLanguage["finish"]))
			{
				OpenRenameMenu = Guid.Empty;
			}
		}

		private static void DisplaySlotConditionsMenu(ShapeKeyEntry s)
		{
			GUILayout.BeginVertical(Sections);

			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label(Main.CurrentLanguage["generalSettings"]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, Main.CurrentLanguage["enableConditionals"]);

			s.IgnoreCategoriesWithShapekey = GUILayout.Toggle(s.IgnoreCategoriesWithShapekey, Main.CurrentLanguage["ignoreCateWithKey"]);

			if (s.DisableWhen)
			{
				s.DisableWhen = GUILayout.Toggle(s.DisableWhen, Main.CurrentLanguage["setShapekeyIf"]);
			}
			else
			{
				s.DisableWhen = GUILayout.Toggle(s.DisableWhen, Main.CurrentLanguage["doNotSetShapekeyIf"]);
			}

			GUILayout.Label(Main.CurrentLanguage["disabledKeyDeform"]);
			s.DisabledDeform = GUILayout.HorizontalSlider(s.DisabledDeform, 0, Main.MaxDeform.Value);

			if (s.WhenAll)
			{
				s.WhenAll = GUILayout.Toggle(s.WhenAll, Main.CurrentLanguage["ifAllSlotsEquipped"]);
			}
			else
			{
				s.WhenAll = GUILayout.Toggle(s.WhenAll, Main.CurrentLanguage["ifAnySlotEquipped"]);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(Sections);

			GUILayout.BeginHorizontal(Sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label(Main.CurrentLanguage["slots"]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			int i = 0;

			foreach (DisableWhenEquipped slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
			{
				if (i == 0)
				{
					GUILayout.BeginHorizontal();
				}

				var temp = GUILayout.Toggle(Extensions.HasFlag(s.SlotFlags, slot), Main.CurrentLanguage[SlotChecker.SlotToSlotList[slot].ToString()], GUILayout.Width(100));

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
			GUILayout.Label(Main.CurrentLanguage["menuFiles"]);
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

			if (GUILayout.Button(Main.CurrentLanguage["addNewMenuFile"]))
			{
				s.MenuFileConditionals[Guid.NewGuid()] = "";
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(Main.CurrentLanguage["finish"]))
			{
				OpenSlotConditions = Guid.Empty;
			}
		}

		private static void DisplayMaidRenameMenu(string s)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"{Main.CurrentLanguage["renamingMaidGroup"]}: {s}");

			GUILayout.BeginHorizontal();

			MaidGroupRename = GUILayout.TextField(MaidGroupRename);

			if (GUILayout.Button(Main.CurrentLanguage["apply"]))
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
			Main.HideInactiveMaids.Value = GUILayout.Toggle(Main.HideInactiveMaids.Value, Main.CurrentLanguage["hideInactiveMaids"]);

			if (Main.HideInactiveMaids.Value == false && GUILayout.Button(Main.CurrentLanguage["all"]))
			{
				Main.logger.LogMessage(Main.CurrentLanguage["exportingAll"]);

				Main.SaveToJson(null, SKDatabase, true);
			}

			GUILayout.BeginVertical(Sections);

			if (ThingsToExport.ContainsKey("global") == false)
			{
				ThingsToExport["global"] = false;
			}

			ThingsToExport["global"] = GUILayout.Toggle(ThingsToExport["global"], Main.CurrentLanguage["global"]);

			foreach (string m in SKDatabase.ListOfMaidsWithKeys())
			{
				if (Main.HideInactiveMaids.Value == false || Extensions.IsMaidActive(m))
				{
					if (ThingsToExport.ContainsKey(m) == false)
					{
						ThingsToExport[m] = false;
					}

					ThingsToExport[m] = GUILayout.Toggle(ThingsToExport[m], m);
				}
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(Main.CurrentLanguage["done"]))
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
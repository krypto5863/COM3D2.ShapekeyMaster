using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ShapeKeyMaster.GUI.UiToolbox;
using Random = System.Random;

namespace ShapeKeyMaster.GUI
{
	internal static class Ui
	{
		private static DateTime? _timeToKillNewKeyShine;
		private static Guid? _newKeyToShine;

		private static List<Tuple<string, string>> _shapeKeysNameList = new List<Tuple<string, string>>();
		private static List<string> _maidNameList = new List<string>();

		private static readonly Dictionary<string, bool> ThingsToExport = new Dictionary<string, bool>();

		internal static ShapeKeyDatabase SkDatabase = new ShapeKeyDatabase();

		public static readonly int WindowId = 777777;

		private static bool _runOnce = true;

		// Used for remembering old values when entering SKMenu
		private static string _oldSkMenuFilter = "";

		private static Vector2 _oldSkMenuScrollPosition = Vector2.zero;
		private static Vector2 _oldPreSkMenuScrollPosition = Vector2.zero;

		private static string _filter = "";
		private static int _filterMode;
		private static int _page;
		private static bool _filterCommonKeys = true;
		private static bool _hideBlacklistedKeys = true;

		private static int _tabSelection;

		private static string _maidSelection = "";

		private static readonly ShapekeyEntryComparer EntryComparer = new ShapekeyEntryComparer();

		private static Vector2 _scrollPosition = Vector2.zero;
		private static Vector2 _scrollPosition1 = Vector2.zero;
		private static Guid _openSkMenu;
		private static Guid _openMaidMenu;
		private static Guid _openRenameMenu;
		private static Guid _openSlotConditions;
		private static string _maidGroupRenameMenu;
		private static string _maidGroupRename;
		private static bool _maidGroupCreateOpen;
		private static bool _exportMenuOpen;

		public static Rect WindowRect = new Rect(Screen.width / 3f, Screen.height / 4f, Screen.width / 3f, Screen.height / 1.5f);
		private static Rect _dragWindow = new Rect(0, 0, 10000, 20);
		private static Rect _closeButton = new Rect(0, 0, 25, 15);
		private static int _currentHeight;
		private static int _currentWidth;

		private static GUIStyle _mainWindow;
		private static GUIStyle _sections;
		private static GUIStyle _sections2;

		private static GUIStyle _shineSections;
		private static GUIStyle _blacklistedButton;
		//private static GUIStyle PreviewButton;

		public static void Initialize()
		{
			//Setup some UI properties.
			if (_runOnce)
			{
				_mainWindow = new GUIStyle(UnityEngine.GUI.skin.window)
				{
					normal =
					{
						background = MakeWindowTex(new Color(0, 0, 0, 0.05f), new Color(0, 0, 0, 0.5f)),
						textColor = new Color(1, 1, 1, 0.05f)
					},
					hover =
					{
						background = MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.3f), new Color(1, 0, 0, 0.5f)),
						textColor = new Color(1, 1, 1, 0.3f)
					},
					onNormal =
					{
						background = MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.6f), new Color(1, 0, 0, 0.5f))
					}
				};

				_sections = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = MakeTex(2, 2, new Color(0, 0, 0, 0.3f))
					}
				};
				_sections2 = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f))
					}
				};

				_shineSections = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = MakeTex(2, 2, new Color(0.92f, 0.74f, 0.2f, 0.3f))
					}
				};

				_blacklistedButton = new GUIStyle(UnityEngine.GUI.skin.button)
				{
					normal =
					{
						textColor = Color.red
					},
					hover =
					{
						textColor = Color.red
					},
					active =
					{
						textColor = Color.red
					},
					fontSize = ShapeKeyMaster.FontSize.Value
				};

				EntryComparer.Mode = ConvertDefaultSortMethod(ShapeKeyMaster.DefaultSortingMethod.Value);

				_runOnce = false;
			}

			//Sometimes the UI can be improperly sized, this sets it to some measurements.
			if (_currentHeight != Screen.height || _currentWidth != Screen.width)
			{
				WindowRect.height = Math.Max(Screen.height / 1.5f, ShapeKeyMaster.MinUIHeight.Value);
				WindowRect.width = Math.Max(Screen.width / 3f, ShapeKeyMaster.MinUIWidth.Value);

				float uiPosY = Screen.height / 4f;
				float uiPosX = Screen.width / 3f;
				switch (ShapeKeyMaster.DefaultUIPosition.Value)
				{
					case "TopLeft":
						uiPosY = 20f;
						uiPosX = 20f;
						break;
					case "TopRight":
						uiPosY = 20f;
						uiPosX = Screen.width - WindowRect.width - 20f;
						break;
					case "BottomLeft":
						uiPosY = Screen.height - WindowRect.height - 20f;
						uiPosX = 20f;
						break;
					case "BottomRight":
						uiPosY = Screen.height - WindowRect.height - 20f;
						uiPosX = Screen.width - WindowRect.width - 20f;
						break;
					case "Center":
						uiPosY = (Screen.height / 2f) - (WindowRect.height / 2f);
						uiPosX = (Screen.width / 2f) - (WindowRect.width / 2f);
						break;
					default:
						break;
				}
				if (uiPosY < 0 || uiPosX < 0 || uiPosY > Screen.height || uiPosX > Screen.width)
				{
					uiPosY = Screen.height / 4f;
					uiPosX = Screen.width / 3f;
				}

				WindowRect.y = uiPosY;
				WindowRect.x = uiPosX;

				ShapeKeyMaster.pluginLogger.LogDebug($"Changing sizes of SKM UI to {WindowRect.width} x {WindowRect.height}");

				_currentHeight = Screen.height;
				_currentWidth = Screen.width;
			}

			UnityEngine.GUI.skin = UIUserOverrides.CustomSkin;

			WindowRect = GUILayout.Window(WindowId, WindowRect, GuiWindowControls, ShapeKeyMaster.CurrentLanguage["title"], _mainWindow);

			UnityEngine.GUI.skin = null;
		}

		private static void GuiWindowControls(int windowId)
		{
			_closeButton.x = WindowRect.width - (_closeButton.width + 5);
			_dragWindow.width = WindowRect.width - (_closeButton.width + 5);

			UnityEngine.GUI.DragWindow(_dragWindow);

			if (UnityEngine.GUI.Button(_closeButton, "X"))
			{
				ShapeKeyMaster.EnableGui = false;
			}

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			if (_exportMenuOpen)
			{
				DisplayExportMenu();
			}
			else if (_openSkMenu != Guid.Empty)
			{
				DisplayShapeKeySelectMenu(SkDatabase.AllShapekeyDictionary[_openSkMenu]);
			}
			else if (_openMaidMenu != Guid.Empty)
			{
				DisplayMaidSelectMenu(SkDatabase.AllShapekeyDictionary[_openMaidMenu]);
			}
			else if (_openRenameMenu != Guid.Empty)
			{
				DisplayRenameMenu(SkDatabase.AllShapekeyDictionary[_openRenameMenu]);
			}
			else if (_openSlotConditions != Guid.Empty)
			{
				DisplaySlotConditionsMenu(SkDatabase.AllShapekeyDictionary[_openSlotConditions]);
			}
			else if (!string.IsNullOrEmpty(_maidGroupRenameMenu))
			{
				DisplayMaidRenameMenu(_maidGroupRenameMenu);
			}
			else if (_maidGroupCreateOpen)
			{
				DisplayMaidGroupCreateMenu(SkDatabase.AllShapekeyDictionary);
			}
			else
			{
				//instance handles the shine of the UI.
				if (_newKeyToShine.HasValue && _timeToKillNewKeyShine < DateTime.Now)
				{
					_newKeyToShine = null;
					_timeToKillNewKeyShine = null;
				}

				DisplayHeaderMenu();

				GUILayout.BeginHorizontal();

				ShapeKeyMaster.SimpleMode.Value = GUILayout.Toggle(ShapeKeyMaster.SimpleMode.Value, ShapeKeyMaster.CurrentLanguage["simple"]);
				if (ShapeKeyMaster.SimpleMode.Value)
				{
					ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value = GUILayout.Toggle(ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value, ShapeKeyMaster.CurrentLanguage["showMoreFunctions"]);
				}

				GUILayout.EndHorizontal();

				switch (_tabSelection)
				{
					case 1:
						if (ShapeKeyMaster.SimpleMode.Value)
						{
							GUILayout.BeginVertical(_sections);

							DisplayPageManager();

							SimpleDisplayShapeKeyEntriesMenu(SkDatabase.GlobalShapekeyDictionary());

							DisplayPageManager();

							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(_sections);

							DisplayPageManager();

							DisplayShapeKeyEntriesMenu(SkDatabase.GlobalShapekeyDictionary());

							DisplayPageManager();

							GUILayout.EndVertical();
						}
						break;

					case 2:
						ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"]);

						GUILayout.BeginVertical(_sections);

						GUILayout.BeginHorizontal(_sections2);
						GUILayout.FlexibleSpace();
						GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maids"]);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						DisplayMaidOptions();

						GUILayout.EndVertical();
						break;

					default:

						if (ShapeKeyMaster.SimpleMode.Value)
						{
							GUILayout.BeginVertical(_sections);

							DisplayPageManager();

							SimpleDisplayShapeKeyEntriesMenu(SkDatabase.AllShapekeyDictionary);

							DisplayPageManager();

							GUILayout.EndVertical();
						}
						else
						{
							GUILayout.BeginVertical(_sections);

							DisplayPageManager();

							DisplayShapeKeyEntriesMenu(SkDatabase.AllShapekeyDictionary);

							DisplayPageManager();

							GUILayout.EndVertical();
						}
						break;
				}
			}

			GUILayout.EndScrollView();

			DisplayFooter();
			ChkMouseClick(WindowRect);
		}

		private static void DisplayHeaderMenu()
		{
			GUILayout.BeginHorizontal(_sections2);

			var modeLabel = EntryComparer.Mode == 0 ? ShapeKeyMaster.CurrentLanguage["date"] : 
							EntryComparer.Mode == 1 ? ShapeKeyMaster.CurrentLanguage["name"] : 
							EntryComparer.Mode == 2 ? ShapeKeyMaster.CurrentLanguage["shapekey"] : 
							EntryComparer.Mode == 3 ? "GUID" : ShapeKeyMaster.CurrentLanguage["orderNumber"];
			var ascendLabel = EntryComparer.Ascending ? "↑" : "↓";

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["sortBy"] + ":");
			if (GUILayout.Button(modeLabel))
			{
				++EntryComparer.Mode;
			}
			if (GUILayout.Button(ascendLabel))
			{
				EntryComparer.Ascending = !EntryComparer.Ascending;
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["all"]))
			{
				_page = 0;
				_tabSelection = 0;
			}
			else if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["globals"]))
			{
				_page = 0;
				_tabSelection = 1;
			}
			else if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["maids"]))
			{
				_page = 0;
				_tabSelection = 2;
			}

			GUILayout.EndHorizontal();

			DisplaySearchMenu();
		}

		private static void DisplaySearchMenu(bool noModes = false)
		{
			GUILayout.BeginHorizontal(_sections2);

			if (_openSkMenu != Guid.Empty)
			{
				_filterCommonKeys = GUILayout.Toggle(_filterCommonKeys, ShapeKeyMaster.CurrentLanguage["filterCommonKeys"]);

				_hideBlacklistedKeys = GUILayout.Toggle(_hideBlacklistedKeys, ShapeKeyMaster.CurrentLanguage["hideBlacklistedKeys"]);
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["searchBy"] + ":");

			if (noModes == false)
			{
				if (_filterMode == 0)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["name"]))
					{
						_filterMode = 1;
					}
				}
				else if (_filterMode == 1)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["maid"]))
					{
						_filterMode = 2;
					}
				}
				else if (_filterMode == 2)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["shapekey"]))
					{
						_filterMode = 0;
					}
				}
			}

			_filter = GUILayout.TextField(_filter, GUILayout.Width(160));

			GUILayout.EndHorizontal();
		}

		private static void DisplayPageManager(string maidWithKey = null)
		{
			string headerString;
			int applicantCount;

			switch (_tabSelection)
			{
				case 1:
					headerString = ShapeKeyMaster.CurrentLanguage["globals"];
					applicantCount = SkDatabase.GlobalShapekeyDictionary().Count;
					break;

				case 2:
					headerString = maidWithKey;
					applicantCount = SkDatabase.ShapeKeysByMaid(maidWithKey).Count;
					break;

				default:
					headerString = ShapeKeyMaster.CurrentLanguage["all"];
					applicantCount = SkDatabase.AllShapekeyDictionary.Count;
					break;
			}

			GUILayout.BeginHorizontal(_sections2);
			if (GUILayout.Button("<<"))
			{
				_page = Math.Max(_page - ShapeKeyMaster.EntriesPerPage.Value, 0);
			}
			GUILayout.FlexibleSpace();
			BuildPageManagerLabel(headerString, applicantCount);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(">>"))
			{
				if (_page + ShapeKeyMaster.EntriesPerPage.Value < applicantCount)
				{
					_page += ShapeKeyMaster.EntriesPerPage.Value;
				}
			}
			GUILayout.EndHorizontal();
		}

		private static void DisplayFooter()
		{
			GUILayout.BeginVertical(_sections2);
			if (_exportMenuOpen)
			{
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["cancel"]))
				{
					_exportMenuOpen = false;
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["reload"]))
				{
					_openSkMenu = Guid.Empty;
					_openMaidMenu = Guid.Empty;
					_openRenameMenu = Guid.Empty;
					_openSlotConditions = Guid.Empty;

					var serDb = ShapeKeyMaster.LoadFromJson(Paths.ConfigPath + "\\ShapekeyMaster.json");

					SkDatabase.OverwriteDictionary(serDb.AllShapekeyDictionary);
					SkDatabase.BlacklistedShapeKeys = serDb.BlacklistedShapeKeys;

					return;
				}
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["save"]))
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug("Saving data to configs now!");
#endif

					ShapeKeyMaster.SaveToJson(Paths.ConfigPath + "\\ShapekeyMaster.json", SkDatabase);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["export"]))
				{
					_exportMenuOpen = true;
				}
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["import"]))
				{
					SkDatabase.ConcatenateDictionary(ShapeKeyMaster.LoadFromJson(null, true).AllShapekeyDictionary);

					return;
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}

		private static void DisplayMaidOptions()
		{
			GUILayout.BeginVertical(_sections);

			foreach (var maidWithKey in SkDatabase.ListOfMaidsWithKeys().OrderBy(maid => maid))
			{
				if (ShapeKeyMaster.HideInactiveMaids.Value)
				{
					if (!Extensions.IsMaidActive(maidWithKey))
					{
						continue;
					}
				}

				if (_filter != "")
				{
					switch (_filterMode)
					{
						case 0 when SkDatabase.DoesMaidPartialEntryName(maidWithKey, _filter) == false:
						case 1 when maidWithKey.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false:
						case 2 when SkDatabase.DoesMaidPartialShapeKey(maidWithKey, _filter) == false:
							continue;
					}
				}

				GUILayout.BeginVertical(_sections);
				GUILayout.BeginHorizontal(_sections);
				GUILayout.Label(maidWithKey);

				if (GUILayout.Button("I/O"))
				{
					ShapeKeyEntry first = null;
					foreach (var value in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						first = value;
						break;
					}

					var targetToggle = (first.Enabled - 1);

					foreach (var sk in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						sk.Enabled = targetToggle;
					}

					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["copy_to"]))
				{
					var keysToCopy = SkDatabase.ShapeKeysByMaid(maidWithKey).Values;
					var newKeys = keysToCopy.Select(r => r.Clone() as ShapeKeyEntry).ToDictionary(r => r.Id, m => m);

					ShapeKeyMaster.pluginLogger.LogInfo($"Key count {newKeys.Count}");

					var rand = new Random();
					string tempMaidGroupName;

					do
					{
						tempMaidGroupName = "Temporary Maid Group Name" + rand.Next();
					}
					while (SkDatabase.ShapeKeysByMaid(tempMaidGroupName).Count > 0);

					foreach (var key in newKeys)
					{
						key.Value.Maid = tempMaidGroupName;
					}

					SkDatabase.ConcatenateDictionary(newKeys);

					_maidGroupRenameMenu = newKeys.FirstOrDefault().Value.Maid;
					_maidGroupRename = _maidGroupRenameMenu;

					_maidNameList = Extensions.GetNameOfAllMaids().ToList();

					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["rename"]))
				{
					_maidGroupRenameMenu = maidWithKey;
					_maidGroupRename = _maidGroupRenameMenu;

					_maidNameList = Extensions.GetNameOfAllMaids().ToList();

					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"]))
				{
					foreach (var skp in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						SkDatabase.Remove(skp);
					}

					return;
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("☰"))
				{
					if (!_maidSelection.Contains(maidWithKey))
					{
						_page = 0;
						_maidSelection = maidWithKey;
					}
					else
					{
						_maidSelection = "";
						_page = 0;
					}

					return;
				}

				GUILayout.EndHorizontal();

				if (_maidSelection.Contains(maidWithKey))
				{
					DisplayPageManager(maidWithKey);

					if (ShapeKeyMaster.SimpleMode.Value)
					{
						SimpleDisplayShapeKeyEntriesMenu(SkDatabase.ShapeKeysByMaid(maidWithKey));
					}
					else
					{
						DisplayShapeKeyEntriesMenu(SkDatabase.ShapeKeysByMaid(maidWithKey));
					}

					GUILayout.BeginHorizontal();

					if (GUILayout.Button("+", GUILayout.Width(40)))
					{
						var id = Guid.NewGuid();
						SkDatabase.Add(new ShapeKeyEntry(id, maidWithKey));

						FocusToNewKey(id, SkDatabase.AllShapekeyDictionary, maidWithKey);

						return;
					}

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					DisplayPageManager(maidWithKey);
				}

				GUILayout.EndVertical();
			}

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["newMaidGroup"]))
			{
				_maidNameList = Extensions.GetNameOfAllMaids().ToList();

				_maidGroupCreateOpen = true;

				return;
			}

			GUILayout.EndVertical();
		}

		private static void SimpleDisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			var filteredKeys = givenShapeKeys.Values
				.Where(s => string.IsNullOrEmpty(_filter) ||
							(_filterMode == 0 && s.EntryName.Contains(_filter, StringComparison.OrdinalIgnoreCase)) ||
							(_filterMode == 1 && s.Maid.Contains(_filter, StringComparison.OrdinalIgnoreCase)) ||
							(_filterMode == 2 && s.ShapeKey.Contains(_filter, StringComparison.OrdinalIgnoreCase)))
				.OrderBy(s => s, EntryComparer);

			foreach (var s in filteredKeys.Skip(_page).Take(ShapeKeyMaster.EntriesPerPage.Value))
			{
				var style = (_newKeyToShine.HasValue && s.Id == _newKeyToShine) ? _shineSections : _sections;

				GUILayout.BeginVertical(style);

				GUILayout.BeginHorizontal();
				var status = s.Enabled == 0 ? "ignore" : (s.Enabled == 1 ? "zero" : "on");
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage[status]))
				{
					s.Enabled -= 1;
				}

				if (GUILayout.Button("+", GUILayout.Width(30)))
				{
					if (string.IsNullOrEmpty(s.Maid))
					{
						_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
					}
					else
					{
						_shapeKeysNameList = Extensions.GetMaidByName(s.Maid)?.GetAllShapeKeysFromMaid()?.ToList();

						if (_shapeKeysNameList == null || _shapeKeysNameList.Count == 0)
						{
							_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
						}
					}

					_openSkMenu = s.Id;
					_filter = _oldSkMenuFilter;
					_oldPreSkMenuScrollPosition = _scrollPosition;
					_scrollPosition = _oldSkMenuScrollPosition;
				}
				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));
				s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
				if (!ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"]))
					{
						SkDatabase.Remove(s);
					}
				}
				GUILayout.EndHorizontal();

				if (ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value)
				{
					GUILayout.BeginHorizontal(style);
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["openSlotCondMenu"]))
					{
						_openSlotConditions = s.Id;
					}
					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"]);
					var cateNameLabel = !string.IsNullOrEmpty(s.EntryName) ? s.EntryName : "";
					cateNameLabel += _tabSelection == 0 && !string.IsNullOrEmpty(cateNameLabel) ? " | " : "";
					cateNameLabel += _tabSelection == 0 ? string.IsNullOrEmpty(s.Maid) ? ShapeKeyMaster.CurrentLanguage["global"] : s.Maid : "";
					cateNameLabel = cateNameLabel.Length > 25 ? cateNameLabel.Substring(0, 25) + "..." : cateNameLabel;
					GUILayout.Label(cateNameLabel);
					GUILayout.FlexibleSpace();

					if (EntryComparer.Mode == 4)
					{
						BuildOrderNumControls(s);
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["copy"]))
					{
						var newEntry = s.Clone() as ShapeKeyEntry;
						newEntry.EntryName += "(Copy)";
						SkDatabase.Add(newEntry);
						FocusToNewKey(newEntry.Id, givenShapeKeys);
					}
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"]))
					{
						SkDatabase.Remove(s);
					}
					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}

			if (_tabSelection != 2 && GUILayout.Button("+", GUILayout.Width(40)))
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				var activeGuid = Guid.NewGuid();
				SkDatabase.Add(new ShapeKeyEntry(activeGuid));
				FocusToNewKey(activeGuid, givenShapeKeys);
			}
		}


		/*
		private static void SimpleDisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			var i = 0;

			foreach (var s in givenShapeKeys.Values.OrderBy(val => val, EntryComparer))
			{
				if (_filter != "")
				{
					switch (_filterMode)
					{
						case 0 when s.EntryName.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false:
						case 1 when s.Maid.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false:
						case 2 when s.ShapeKey.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false:
							continue;
					}
				}

				if (i++ < _page)
				{
					continue;
				}

				if (i > _page + ShapeKeyMaster.EntriesPerPage.Value)
				{
					break;
				}

				var style = _newKeyToShine.HasValue && s.Id == _newKeyToShine ? _shineSections : _sections;

				GUILayout.BeginVertical(style);
				GUILayout.BeginHorizontal();

				var status = s.Enabled == 0 ? "ignore" : s.Enabled == 1 ? "zero" : "on";

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage[status], GUILayout.Width(60)))
				{
					s.Enabled -= 1;
				}

				if (GUILayout.Button("+", GUILayout.Width(40)))
				{
					if (string.IsNullOrEmpty(s.Maid))
					{
						_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
					}
					else
					{
						_shapeKeysNameList = Extensions.GetMaidByName(s.Maid)?.GetAllShapeKeysFromMaid()?.ToList();

						if (_shapeKeysNameList == null || _shapeKeysNameList.Count == 0)
						{
							_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
						}
					}

					_openSkMenu = s.Id;
					_filter = _oldSkMenuFilter;
					_oldPreSkMenuScrollPosition = _scrollPosition;
					_scrollPosition = _oldSkMenuScrollPosition;
				}

				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(120));

				s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
				//s.Deform = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
				//GUILayout.Label(s.Deform.ToString(CultureInfo.InvariantCulture), GUILayout.Width(30));

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(style);

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["openSlotCondMenu"]))
				{
					_openSlotConditions = s.Id;
					return;
				}

				s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"]);

				GUILayout.FlexibleSpace();

				var cateNameLabel = !string.IsNullOrEmpty(s.EntryName) ? s.EntryName : "";
				cateNameLabel += _tabSelection == 0 && !string.IsNullOrEmpty(cateNameLabel) ? " | " : "";
				cateNameLabel += _tabSelection == 0 ? string.IsNullOrEmpty(s.Maid) ? ShapeKeyMaster.CurrentLanguage["global"] : s.Maid : "";

				cateNameLabel = cateNameLabel.Length > 25 ? cateNameLabel.Substring(0, 25) + "..." : cateNameLabel;

				GUILayout.Label(cateNameLabel);

				GUILayout.FlexibleSpace();

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["copy"]))
				{
					var newEntry = s.Clone() as ShapeKeyEntry;
					newEntry.EntryName += "(Copy)";
					SkDatabase.Add(newEntry);
					FocusToNewKey(newEntry.Id, givenShapeKeys);
					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"]))
				{
					SkDatabase.Remove(s);
					return;
				}

				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			if (_tabSelection != 2 && GUILayout.Button("+", GUILayout.Width(40)))
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				var activeGuid = Guid.NewGuid();

				SkDatabase.Add(new ShapeKeyEntry(activeGuid));

				FocusToNewKey(activeGuid, givenShapeKeys);
			}
		}
		*/

		private static void DisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			if (_tabSelection != 2 && GUILayout.Button(ShapeKeyMaster.CurrentLanguage["addNewShapekey"]))
			{
#if (DEBUG)
				ShapeKeyMaster.pluginLogger.LogDebug("I've been clicked! Oh the humanity!!");
#endif
				var activeGuid = Guid.NewGuid();

				SkDatabase.Add(new ShapeKeyEntry(activeGuid));

				FocusToNewKey(activeGuid, givenShapeKeys);

				return;
			}

			var filteredKeys = givenShapeKeys.Values
				.Where(s => string.IsNullOrEmpty(_filter) ||
							(_filterMode == 0 && s.EntryName.Contains(_filter, StringComparison.OrdinalIgnoreCase)) ||
							(_filterMode == 1 && s.Maid.Contains(_filter, StringComparison.OrdinalIgnoreCase)) ||
							(_filterMode == 2 && s.ShapeKey.Contains(_filter, StringComparison.OrdinalIgnoreCase)))
				.OrderBy(s => s, EntryComparer);

			foreach (var s in filteredKeys.Skip(_page).Take(ShapeKeyMaster.EntriesPerPage.Value))
			{
				var style = _newKeyToShine.HasValue && s.Id == _newKeyToShine ? _shineSections : _sections;

				GUILayout.BeginVertical(style);

				if (s.Collapsed == false)
				{
					GUILayout.BeginHorizontal(style);
					GUILayout.Label(s.EntryName);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("☰"))
					{
						s.Collapsed = !s.Collapsed;
					}
					GUILayout.EndHorizontal();

					var status = s.Enabled == 0 ? "ignore" : s.Enabled == 1 ? "zero" : "on";
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage[status]))
					{
						s.Enabled -= 1;
					}

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekey"], GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maidOptional"], GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						_openSkMenu = s.Id;
						if (string.IsNullOrEmpty(s.Maid))
						{
							_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
						}
						else
						{
							_shapeKeysNameList = Extensions.GetMaidByName(s.Maid)?.GetAllShapeKeysFromMaid()?.ToList();

							if (_shapeKeysNameList == null || _shapeKeysNameList.Count == 0)
							{
								_shapeKeysNameList = Extensions.GetAllShapeKeysFromAllMaids().ToList();
							}
						}

						_filter = _oldSkMenuFilter;
						_oldPreSkMenuScrollPosition = _scrollPosition;
						_scrollPosition = _oldSkMenuScrollPosition;

						return;
					}

					s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+"))
					{
						_openMaidMenu = s.Id;
						_maidNameList = Extensions.GetNameOfAllMaids().ToList();
						return;
					}
					GUILayout.Label(s.Maid, GUILayout.Width(200));

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));

					s.Animate = GUILayout.Toggle(s.Animate, ShapeKeyMaster.CurrentLanguage["animate"]);

					s.AnimateWithExcitement = GUILayout.Toggle(s.AnimateWithExcitement, ShapeKeyMaster.CurrentLanguage["animateWithYotogiExcitement"]);

					switch (s.Animate)
					{
						case true:
							GUILayout.Label(
								$"{ShapeKeyMaster.CurrentLanguage["animationSpeed"]} = (1000 ms / {s.AnimationPollFloat * 1000} ms) x {s.AnimationRate} = {1000 / (s.AnimationPollFloat * 1000) * s.AnimationRateFloat} %/{ShapeKeyMaster.CurrentLanguage["seconds"]}");

							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekeyDeformation"]);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxAnimationDeformation"]);
							s.AnimationMaximum = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.AnimationMaximum, s.AnimationMinimum, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minAnimationDeformation"]);
							s.AnimationMinimum = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.AnimationMinimum, 0, ShapeKeyMaster.MaxDeform.Value));

							GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["animationRate"]}: {s.AnimationRate}");
							GUILayout.BeginHorizontal();
							s.AnimationRate = GUILayout.TextField(s.AnimationRate);
							GUILayout.EndHorizontal();
							GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["animationPollingRate"]}: {s.AnimationPoll}");
							s.AnimationPoll = GUILayout.TextField(s.AnimationPoll);
							break;

						case false when s.AnimateWithExcitement:

							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxExcitementThreshold"]);
							s.ExcitementMax = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.ExcitementMax, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minExcitementThreshold"]);
							s.ExcitementMin = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.ExcitementMin, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["defShapekeyDeformation"]);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxShapekeyDeformation"]);
							s.DeformMax = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.DeformMax, s.DeformMin, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minShapekeyDeformation"]);
							s.DeformMin = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.DeformMin, 0, s.DeformMax));
							break;

						default:
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekeyDeformation"]);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							break;
					}

					GUILayout.BeginHorizontal(style);

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["openSlotCondMenu"]))
					{
						_openSlotConditions = s.Id;
						return;
					}

					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"]);

					GUILayout.FlexibleSpace();

					if (EntryComparer.Mode == 4)
					{
						BuildOrderNumControls(s);
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["rename"]))
					{
						_openRenameMenu = s.Id;
						return;
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"]))
					{
						SkDatabase.Remove(s);
						return;
					}

					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.BeginHorizontal(_sections);
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

			GUILayout.Label($"{s.EntryName}: {ShapeKeyMaster.CurrentLanguage["selectShapekey"]}");

			GUILayout.FlexibleSpace();

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["rclickToBlacklist"]);

			GUILayout.EndHorizontal();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"]))
			{
				_openSkMenu = Guid.Empty;
				_oldSkMenuFilter = _filter;
				_oldSkMenuScrollPosition = _scrollPosition;
				_scrollPosition = _oldPreSkMenuScrollPosition;

				_filter = "";
			}

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"]))
			{
				_openSkMenu = Guid.Empty;
				_oldSkMenuFilter = _filter;
				_oldSkMenuScrollPosition = _scrollPosition;
				_scrollPosition = _oldPreSkMenuScrollPosition;

				s.ShapeKey = "";
				_filter = "";
			}

			var columns = 0;
			var totalGroupsWorked = 0;

			var groupedKeys = _shapeKeysNameList
				.GroupBy(r => r.item1)
				.OrderBy(r => r.Key)
				.ToArray();

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

				if (_filterCommonKeys)
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

				if (_hideBlacklistedKeys)
				{
					filteredList = filteredList.Where(t => SkDatabase.BlacklistedShapeKeys.IsBlacklisted(t.item2) == false).ToList();
				}

				if (filteredList.Count > 0)
				{
					if (columns++ == 0)
					{
						//Columnar ordering
						GUILayout.BeginHorizontal();
					}

					//Category vertical
					GUILayout.BeginVertical(_sections);

					//Header for category
					GUILayout.BeginHorizontal(_sections2);
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage[group.Key]);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					foreach (var str in filteredList.OrderBy(r => r.item2))
					{
						if (_filter != "")
						{
							if (str.item2.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
							{
								continue;
							}
						}

						var style = _hideBlacklistedKeys == false && SkDatabase.BlacklistedShapeKeys.IsBlacklisted(str.item2) ? _blacklistedButton : UIUserOverrides.getButtonStyleOverride();
						if (GUILayout.Button(str.item2, style))
						{
							switch (LastMouseButtonUp)
							{
								case 1 when SkDatabase.BlacklistedShapeKeys.IsBlacklisted(str.item2) == false:
									SkDatabase.BlacklistedShapeKeys.AddItem(str.item2);
									break;

								case 1:
									SkDatabase.BlacklistedShapeKeys.RemoveItem(str.item2);
									break;

								case 0:
									_openSkMenu = Guid.Empty;
									s.ShapeKey = str.item2;
									_oldSkMenuFilter = _filter;
									_oldSkMenuScrollPosition = _scrollPosition;
									_scrollPosition = _oldPreSkMenuScrollPosition;
									_filter = "";
									break;
							}
						}
					}

					GUILayout.EndVertical();
				}

				if (columns == 3 || totalGroupsWorked == groupedKeys.Length && columns > 0)
				{
					GUILayout.EndHorizontal();
					columns = 0;
				}
			}
		}

		private static void DisplayMaidSelectMenu(ShapeKeyEntry s)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"{s.EntryName} {ShapeKeyMaster.CurrentLanguage["selectMaid"]}");

			GUILayout.BeginHorizontal(_sections2);

			s.Maid = GUILayout.TextField(s.Maid, GUILayout.Width(200));

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"], GUILayout.Width(80)))
			{
				_openMaidMenu = Guid.Empty;
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"]))
			{
				_openMaidMenu = Guid.Empty;
				s.Maid = "";
				_filter = "";
			}

			foreach (var mn in _maidNameList)
			{
				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn))
				{
					_openMaidMenu = Guid.Empty;
					s.Maid = mn;
					_filter = "";
				}
			}
		}

		private static void DisplayMaidGroupCreateMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			DisplaySearchMenu(true);

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["selectNewMaidGroup"]);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"]))
			{
				int i;

				for (i = 0; SkDatabase.ListOfMaidsWithKeys().Contains($"None {i}"); i++)
				{
				}

				var newGuid = Guid.NewGuid();

				SkDatabase.Add(new ShapeKeyEntry(newGuid, $"None {i}"));

				FocusToNewKey(newGuid, givenShapeKeys);

				_maidGroupCreateOpen = false;

				_filter = "";
			}

			foreach (var mn in _maidNameList)
			{
				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn))
				{
					var newKey = Guid.NewGuid();

					SkDatabase.Add(new ShapeKeyEntry(newKey, mn));

					FocusToNewKey(newKey, givenShapeKeys);

					_maidGroupCreateOpen = false;

					_filter = "";
				}
			}
		}

		private static void DisplayRenameMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["nowRenaming"]} {s.EntryName}");

			s.EntryName = GUILayout.TextField(s.EntryName);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"]))
			{
				_openRenameMenu = Guid.Empty;
			}
		}

		private static void DisplaySlotConditionsMenu(ShapeKeyEntry s)
		{
			GUILayout.BeginVertical(_sections);

			GUILayout.BeginHorizontal(_sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["generalSettings"]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"]);

			s.IgnoreCategoriesWithShapekey = GUILayout.Toggle(s.IgnoreCategoriesWithShapekey, ShapeKeyMaster.CurrentLanguage["ignoreCateWithKey"]);

			s.DisableWhen = GUILayout.Toggle(s.DisableWhen, s.DisableWhen ? ShapeKeyMaster.CurrentLanguage["setShapekeyIf"] : ShapeKeyMaster.CurrentLanguage["doNotSetShapekeyIf"]);

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["disabledKeyDeform"]);
			s.DisabledDeform = GUILayout.HorizontalSlider(s.DisabledDeform, 0, ShapeKeyMaster.MaxDeform.Value);

			s.WhenAll = GUILayout.Toggle(s.WhenAll, s.WhenAll ? ShapeKeyMaster.CurrentLanguage["ifAllSlotsEquipped"] : ShapeKeyMaster.CurrentLanguage["ifAnySlotEquipped"]);

			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(_sections);

			GUILayout.BeginHorizontal(_sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["slots"]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			var i = 0;

			foreach (var slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
			{
				if (i == 0)
				{
					GUILayout.BeginHorizontal();
				}

				var temp = GUILayout.Toggle(s.SlotFlags.HasFlag(slot), ShapeKeyMaster.CurrentLanguage[SlotChecker.SlotToSlotList[slot].ToString()], GUILayout.Width(150));

				if (i++ == 4)
				{
					GUILayout.EndHorizontal();
					i = 0;
				}

				if (temp)
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

			GUILayout.BeginVertical(_sections);

			GUILayout.BeginHorizontal(_sections2);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["menuFiles"]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			_scrollPosition1 = GUILayout.BeginScrollView(_scrollPosition1);

			var tempMenuFileConditions = new Dictionary<Guid, string>(s.MenuFileConditionals);

			foreach (var menu in tempMenuFileConditions.Keys)
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

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["addNewMenuFile"]))
			{
				s.MenuFileConditionals[Guid.NewGuid()] = "";
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"]))
			{
				_openSlotConditions = Guid.Empty;
			}
		}

		private static void DisplayMaidRenameMenu(string s)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["renamingMaidGroup"]}: {s}");

			GUILayout.BeginHorizontal();

			_maidGroupRename = GUILayout.TextField(_maidGroupRename);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["apply"]))
			{
				SkDatabase.AllShapekeyDictionary.Values.ToList().ForEach(sk =>
				{
					if (sk.Maid.Equals(s))
					{
						sk.Maid = _maidGroupRename;
					}
				});

				_maidGroupRename = "";
				_maidGroupRenameMenu = "";
				_filter = "";
			}

			GUILayout.EndHorizontal();

			foreach (var mn in _maidNameList)
			{
				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn))
				{
					SkDatabase.AllShapekeyDictionary.Values.ToList().ForEach(sk =>
					{
						if (sk.Maid.Equals(s))
						{
							sk.Maid = mn;
						}
					});

					_maidGroupRename = "";
					_maidGroupRenameMenu = "";
					_filter = "";
				}
			}
		}

		private static void DisplayExportMenu()
		{
			ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"]);

			if (ShapeKeyMaster.HideInactiveMaids.Value == false && GUILayout.Button(ShapeKeyMaster.CurrentLanguage["all"]))
			{
				ShapeKeyMaster.pluginLogger.LogMessage(ShapeKeyMaster.CurrentLanguage["exportingAll"]);

				ShapeKeyMaster.SaveToJson(null, SkDatabase, true);
			}

			GUILayout.BeginVertical(_sections);

			if (ThingsToExport.ContainsKey("global") == false)
			{
				ThingsToExport["global"] = false;
			}

			ThingsToExport["global"] = GUILayout.Toggle(ThingsToExport["global"], ShapeKeyMaster.CurrentLanguage["global"]);

			foreach (var m in SkDatabase.ListOfMaidsWithKeys())
			{
				if (ShapeKeyMaster.HideInactiveMaids.Value && !Extensions.IsMaidActive(m)) continue;

				if (ThingsToExport.ContainsKey(m) == false)
				{
					ThingsToExport[m] = false;
				}

				ThingsToExport[m] = GUILayout.Toggle(ThingsToExport[m], m);
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["done"]))
			{
				var selectionDataBase = new ShapeKeyDatabase();

				if (ThingsToExport.TryGetValue("global", out var toExport) && toExport)
				{
					selectionDataBase.AllShapekeyDictionary = SkDatabase.GlobalShapekeyDictionary();
				}

				foreach (var m in SkDatabase.ListOfMaidsWithKeys())
				{
					if (ThingsToExport.TryGetValue(m, out var toExport1) && toExport1)
					{
						selectionDataBase.AllShapekeyDictionary = selectionDataBase.AllShapekeyDictionary.Concat(SkDatabase.ShapeKeysByMaid(m)).ToDictionary(r => r.Key, f => f.Value);
					}
				}

				if (selectionDataBase.AllShapekeyDictionary.Count > 0)
				{
					ShapeKeyMaster.SaveToJson(null, selectionDataBase, true);

					_exportMenuOpen = false;
				}

				ThingsToExport.Clear();
			}
		}

		private static void BuildPageManagerLabel(string headerString, int applicantCount)
		{
			if (EntryComparer.Ascending)
			{
				var countStart = _page + 1;
				var countEnd = Math.Min(_page + ShapeKeyMaster.EntriesPerPage.Value, applicantCount);
				if (countStart == applicantCount)
				{
					GUILayout.Label($"{headerString} : {countStart}");
				}
				else
				{
					GUILayout.Label($"{headerString} : {countStart} ~ {countEnd}");
				}
			}
			else
			{
				var countStart = applicantCount - _page;
				var countEnd = Math.Max(countStart - ShapeKeyMaster.EntriesPerPage.Value + 1, 1);
				if (countStart == 1)
				{
					GUILayout.Label($"{headerString} : {countStart}");
				}
				else
				{
					GUILayout.Label($"{headerString} : {countStart} ~ {countEnd}");
				}
			}
		}

		private static void BuildOrderNumControls(ShapeKeyEntry s)
		{
			if (Event.current.type == EventType.keyDown
				&& (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
				&& UnityEngine.GUI.GetNameOfFocusedControl() == "OrderNumInput")
			{
				if (s.OrderNumTmp.HasValue)
				{
					s.OrderNum = s.OrderNumTmp;
					s.OrderNumTmp = null;
				}
			}

			if (s.OrderNumTmp.HasValue)
			{
				if (GUILayout.Button("X", GUILayout.Width(50)))
				{
					s.OrderNumTmp = null;
					return;
				}

				UnityEngine.GUI.SetNextControlName("OrderNumInput");
				s.OrderNumTmp = UiToolbox.IntField(s.OrderNumTmp.Value, 0, 999, 75);

				if (GUILayout.Button("✓", GUILayout.Width(50)))
				{
					s.OrderNum = s.OrderNumTmp;
					s.OrderNumTmp = null;
					return;
				}
			}
			else
			{
				if (GUILayout.Button("↑", GUILayout.Width(50)))
				{
					SkDatabase.Reorder_MoveForward(s);
					return;
				}

				if (GUILayout.Button(s.OrderNum.Value.ToString(), GUILayout.Width(75)))
				{
					s.OrderNumTmp = s.OrderNum;
					return;
				}

				if (GUILayout.Button("↓", GUILayout.Width(50)))
				{
					SkDatabase.Reorder_MoveBack(s);
					return;
				}
			}
		}

		private static int ConvertDefaultSortMethod(string sortMethodStr)
		{
			switch (sortMethodStr)
			{
				case "Date":
					return 0;
				case "Name":
					return 1;
				case "Shapekey":
					return 2;
				case "Id":
					return 3;
				case "Order Number":
					return 4;
				default: 
					return 0;
			}
		}

		//UI Helper funcs
		internal static void FocusToNewKey(Guid guid, Dictionary<Guid, ShapeKeyEntry> givenShapeKeys, string maid = null)
		{
			_newKeyToShine = guid;
			_timeToKillNewKeyShine = DateTime.Now.AddSeconds(3);

			double pos = 0;

			foreach (var s in givenShapeKeys.Values.Where(r => maid.IsNullOrWhiteSpace() || r.Maid.Equals(maid)).OrderBy(val => val, EntryComparer))
			{
				if (s.Id != guid)
				{
					++pos;
				}
				else
				{
					_page = (int)Math.Floor(pos / ShapeKeyMaster.EntriesPerPage.Value) * ShapeKeyMaster.EntriesPerPage.Value;
				}
			}
		}
	}
}
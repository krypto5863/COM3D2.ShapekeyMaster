using BepInEx;
using System;
using System.Collections.Generic;
using System.Globalization;
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

		private static Vector2 _scrollPosition_ExportMenu = Vector2.zero;
		private static Vector2 _scrollPosition_ShapeKeySelectMenu = Vector2.zero;
		private static Vector2 _scrollPosition_MaidSelectMenu = Vector2.zero;
		private static Vector2 _scrollPosition_RenameMenu = Vector2.zero;
		private static Vector2 _scrollPosition_SlotConditionsMenu = Vector2.zero;
		private static Vector2 _scrollPosition_MaidRenameMenu = Vector2.zero;
		private static Vector2 _scrollPosition_MaidGroupCreateMenu = Vector2.zero;
		private static Vector2 _scrollPosition_MaidShapeKeyList = Vector2.zero;
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

		private static Texture2D normalTexture;
		private static Texture2D hoverTexture;
		private static Texture2D onNormalTexture;
		private static Texture2D sectionsTexture;
		private static Texture2D sections2Texture;
		private static Texture2D shineSectionsTexture;

		private static GUIStyle _mainWindow;
		private static GUIStyle _sections;
		private static GUIStyle _sections2;

		private static GUIStyle _shineSections;
		private static GUIStyle _blacklistedButton;
		private static bool _collapseBodyShapekeys;
		private static bool _collapseHeadShapekeys;
		//private static GUIStyle PreviewButton;

		private static readonly GUILayoutOption[] EmptyGuiLayoutOptions = new GUILayoutOption[0];

		public static void Initialize()
		{
			//Setup some UI properties.
			if (_runOnce)
			{
				normalTexture = MakeWindowTex(new Color(0, 0, 0, 0.05f), new Color(0, 0, 0, 0.5f));
				hoverTexture = MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.3f), new Color(1, 0, 0, 0.5f));
				onNormalTexture = MakeWindowTex(new Color(0.3f, 0.3f, 0.3f, 0.6f), new Color(1, 0, 0, 0.5f));

				_mainWindow = new GUIStyle(UnityEngine.GUI.skin.window)
				{
					normal =
					{
						background = normalTexture,
						textColor = new Color(1, 1, 1, 0.05f)
					},
					hover =
					{
						background = hoverTexture,
						textColor = new Color(1, 1, 1, 0.3f)
					},
					onNormal =
					{
						background = onNormalTexture
					}
				};

				sectionsTexture = MakeTex(2, 2, new Color(0, 0, 0, 0.3f));

				_sections = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = sectionsTexture
					}
				};

				sections2Texture = MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

				_sections2 = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = sections2Texture
					}
				};

				shineSectionsTexture = MakeTex(2, 2, new Color(0.92f, 0.74f, 0.2f, 0.3f));

				_shineSections = new GUIStyle(UnityEngine.GUI.skin.box)
				{
					normal =
					{
						background = shineSectionsTexture
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
				_tabSelection = ConvertDefaultTabSelection(ShapeKeyMaster.DefaultTabSelection.Value);
				_collapseBodyShapekeys = ShapeKeyMaster.CollapseBodyShapekeysAtStart.Value;
				_collapseHeadShapekeys = ShapeKeyMaster.CollapseHeadShapekeysAtStart.Value;

				_runOnce = false;
			}

			//Sometimes the UI can be improperly sized, this sets it to some measurements.
			if (_currentHeight != Screen.height || _currentWidth != Screen.width)
			{
				WindowRect.height = Math.Max(Screen.height / 1.5f, ShapeKeyMaster.MinUIHeight.Value);
				WindowRect.width = Math.Max(Screen.width / 3f, ShapeKeyMaster.MinUIWidth.Value);

				var uiPosY = Screen.height / 4f;
				var uiPosX = Screen.width / 3f;
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
						uiPosY = Screen.height / 2f - WindowRect.height / 2f;
						uiPosX = Screen.width / 2f - WindowRect.width / 2f;
						break;
				}
				if (uiPosY < 0 || uiPosX < 0 || uiPosY > Screen.height || uiPosX > Screen.width)
				{
					uiPosY = Screen.height / 4f;
					uiPosX = Screen.width / 3f;
				}

				WindowRect.y = uiPosY;
				WindowRect.x = uiPosX;

				ShapeKeyMaster.PluginLogger.LogDebug($"Changing sizes of SKM UI to {WindowRect.width.ToString()} x {WindowRect.height.ToString()}");

				_currentHeight = Screen.height;
				_currentWidth = Screen.width;
			}

			UnityEngine.GUI.skin = UiUserOverrides.CustomSkin;

			WindowRect = GUILayout.Window(WindowId, WindowRect, GuiWindowControls, ShapeKeyMaster.CurrentLanguage["title"], _mainWindow, EmptyGuiLayoutOptions);

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

				GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

				ShapeKeyMaster.SimpleMode.Value = GUILayout.Toggle(ShapeKeyMaster.SimpleMode.Value, ShapeKeyMaster.CurrentLanguage["simple"], EmptyGuiLayoutOptions);
				if (ShapeKeyMaster.SimpleMode.Value)
				{
					ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value = GUILayout.Toggle(ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value, ShapeKeyMaster.CurrentLanguage["showMoreFunctions"], EmptyGuiLayoutOptions);
				}

				GUILayout.EndHorizontal();

				switch (_tabSelection)
				{
					case 1:

						_scrollPosition_MaidShapeKeyList = GUILayout.BeginScrollView(_scrollPosition_MaidShapeKeyList, EmptyGuiLayoutOptions);

						GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

						DisplayPageManager();

						if (ShapeKeyMaster.SimpleMode.Value)
						{
							SimpleDisplayShapeKeyEntriesMenu(SkDatabase.GlobalShapekeyDictionary());
						}
						else
						{
							DisplayShapeKeyEntriesMenu(SkDatabase.GlobalShapekeyDictionary());
						}

						DisplayPageManager();

						GUILayout.EndVertical();

						GUILayout.EndScrollView();

						break;

					case 2:
						ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"], EmptyGuiLayoutOptions);

						GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

						GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
						GUILayout.FlexibleSpace();
						GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maids"], EmptyGuiLayoutOptions);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();

						_scrollPosition_MaidShapeKeyList = GUILayout.BeginScrollView(_scrollPosition_MaidShapeKeyList, EmptyGuiLayoutOptions);

						DisplayMaidOptions();

						GUILayout.EndScrollView();

						if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["newMaidGroup"], EmptyGuiLayoutOptions))
						{
							_maidNameList = Extensions.GetNameOfAllMaids().ToList();

							_maidGroupCreateOpen = true;

							return;
						}

						GUILayout.EndVertical();
						break;

					default:

						_scrollPosition_MaidShapeKeyList = GUILayout.BeginScrollView(_scrollPosition_MaidShapeKeyList, EmptyGuiLayoutOptions);

						GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

						DisplayPageManager();

						if (ShapeKeyMaster.SimpleMode.Value)
						{
							SimpleDisplayShapeKeyEntriesMenu(SkDatabase.AllShapekeyDictionary);
						}
						else
						{
							DisplayShapeKeyEntriesMenu(SkDatabase.AllShapekeyDictionary);
						}

						DisplayPageManager();

						GUILayout.EndVertical();

						GUILayout.EndScrollView();

						break;
				}
			}

			DisplayFooter();
			ChkMouseClick(WindowRect);
		}

		private static void DisplayHeaderMenu()
		{
			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);

			var modeLabel = EntryComparer.Mode == 0 ? ShapeKeyMaster.CurrentLanguage["date"] : 
							EntryComparer.Mode == 1 ? ShapeKeyMaster.CurrentLanguage["name"] : 
							EntryComparer.Mode == 2 ? ShapeKeyMaster.CurrentLanguage["shapekey"] : 
							EntryComparer.Mode == 3 ? "GUID" : ShapeKeyMaster.CurrentLanguage["orderNumber"];
			var ascendLabel = EntryComparer.Ascending ? "↑" : "↓";

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["sortBy"] + ":", EmptyGuiLayoutOptions);
			if (GUILayout.Button(modeLabel, EmptyGuiLayoutOptions))
			{
				++EntryComparer.Mode;
			}
			if (GUILayout.Button(ascendLabel, EmptyGuiLayoutOptions))
			{
				EntryComparer.Ascending = !EntryComparer.Ascending;
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["all"], EmptyGuiLayoutOptions))
			{
				_page = 0;
				_tabSelection = 0;
			}
			else if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["globals"], EmptyGuiLayoutOptions))
			{
				_page = 0;
				_tabSelection = 1;
			}
			else if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["maids"], EmptyGuiLayoutOptions))
			{
				_page = 0;
				_tabSelection = 2;
			}

			GUILayout.EndHorizontal();

			DisplaySearchMenu();
		}

		private static void DisplaySearchMenu(bool noModes = false)
		{
			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);

			if (_openSkMenu != Guid.Empty)
			{
				_filterCommonKeys = GUILayout.Toggle(_filterCommonKeys, ShapeKeyMaster.CurrentLanguage["filterCommonKeys"], EmptyGuiLayoutOptions);

				_hideBlacklistedKeys = GUILayout.Toggle(_hideBlacklistedKeys, ShapeKeyMaster.CurrentLanguage["hideBlacklistedKeys"], EmptyGuiLayoutOptions);
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["searchBy"] + ":", EmptyGuiLayoutOptions);

			if (noModes == false)
			{
				if (_filterMode == 0)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["name"], EmptyGuiLayoutOptions))
					{
						_filterMode = 1;
					}
				}
				else if (_filterMode == 1)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["maid"], EmptyGuiLayoutOptions))
					{
						_filterMode = 2;
					}
				}
				else if (_filterMode == 2)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["shapekey"], EmptyGuiLayoutOptions))
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

			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
			//To Beginning
			if (GUILayout.Button("<<" , EmptyGuiLayoutOptions))
			{
				_page = 0;
			}
			if (GUILayout.Button(" < ", EmptyGuiLayoutOptions))
			{
				if (_page - ShapeKeyMaster.EntriesPerPage.Value >= 0)
				{
					_page -= ShapeKeyMaster.EntriesPerPage.Value;
				}
				//Past first shapekey, wrap to end
				else
				{
					if (applicantCount == 0)
					{
						_page = 0;
					}
					else if (applicantCount % ShapeKeyMaster.EntriesPerPage.Value == 0)
					{
						_page = applicantCount - ShapeKeyMaster.EntriesPerPage.Value;
					}
                    else
					{
						_page = applicantCount - applicantCount % ShapeKeyMaster.EntriesPerPage.Value;
					}
				}
			}
			GUILayout.FlexibleSpace();
			BuildPageManagerLabel(headerString, applicantCount);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(" > ", EmptyGuiLayoutOptions))
			{
				if (_page + ShapeKeyMaster.EntriesPerPage.Value < applicantCount)
				{
					_page += ShapeKeyMaster.EntriesPerPage.Value;
				}
				//Past last shapekey, wrap to beginning
				else
				{
					_page = 0;
				}
			}
			//To End
			if (GUILayout.Button(">>", EmptyGuiLayoutOptions))
			{
				if (applicantCount == 0)
				{
					_page = 0;
				}
				else if (applicantCount % ShapeKeyMaster.EntriesPerPage.Value == 0)
				{
					_page = applicantCount - ShapeKeyMaster.EntriesPerPage.Value;
				}
				else
				{
					_page = applicantCount - applicantCount % ShapeKeyMaster.EntriesPerPage.Value;
				}
			}
			GUILayout.EndHorizontal();
		}

		private static void DisplayFooter()
		{
			GUILayout.BeginVertical(_sections2, EmptyGuiLayoutOptions);
			if (_exportMenuOpen)
			{
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["cancel"], EmptyGuiLayoutOptions))
				{
					_exportMenuOpen = false;
				}
			}
			else
			{
				GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["reload"], EmptyGuiLayoutOptions))
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
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["save"], EmptyGuiLayoutOptions))
				{
#if (DEBUG)
					ShapeKeyMaster.pluginLogger.LogDebug("Saving data to configs now!");
#endif

					ShapeKeyMaster.SaveToJson(Paths.ConfigPath + "\\ShapekeyMaster.json", SkDatabase);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["export"], EmptyGuiLayoutOptions))
				{
					_exportMenuOpen = true;
				}
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["import"], EmptyGuiLayoutOptions))
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
			GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

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

				GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);
				GUILayout.BeginHorizontal(_sections, EmptyGuiLayoutOptions);
				GUILayout.Label(maidWithKey, EmptyGuiLayoutOptions);

				if (GUILayout.Button("I/O", EmptyGuiLayoutOptions))
				{
					ShapeKeyEntry first = null;
					foreach (var value in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						first = value;
						break;
					}

					var targetToggle = first.Enabled - 1;

					foreach (var sk in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						sk.Enabled = targetToggle;
					}

					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["copy_to"], EmptyGuiLayoutOptions))
				{
					var keysToCopy = SkDatabase.ShapeKeysByMaid(maidWithKey).Values;
					var newKeys = keysToCopy.Select(r => r.Clone() as ShapeKeyEntry).ToDictionary(r => r.Id, m => m);

					ShapeKeyMaster.PluginLogger.LogInfo($"Key count {newKeys.Count.ToString()}");

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

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["rename"], EmptyGuiLayoutOptions))
				{
					_maidGroupRenameMenu = maidWithKey;
					_maidGroupRename = _maidGroupRenameMenu;

					_maidNameList = Extensions.GetNameOfAllMaids().ToList();

					return;
				}

				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"], EmptyGuiLayoutOptions))
				{
					foreach (var skp in SkDatabase.ShapeKeysByMaid(maidWithKey).Values)
					{
						SkDatabase.Remove(skp);
					}

					return;
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("☰", EmptyGuiLayoutOptions))
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

					GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

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
				var style = _newKeyToShine.HasValue && s.Id == _newKeyToShine ? _shineSections : _sections;

				GUILayout.BeginVertical(style, EmptyGuiLayoutOptions);

				GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
				var status = s.Enabled == 0 ? "ignore" : s.Enabled == 1 ? "zero" : "on";
				if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage[status], EmptyGuiLayoutOptions))
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
					_oldPreSkMenuScrollPosition = _scrollPosition_MaidShapeKeyList;
					_scrollPosition_ShapeKeySelectMenu = _oldSkMenuScrollPosition;
				}
				s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));
				s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
				if (!ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value)
				{
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"], EmptyGuiLayoutOptions))
					{
						SkDatabase.Remove(s);
					}
				}
				GUILayout.EndHorizontal();

				if (ShapeKeyMaster.SimpleMode_ShowMoreFunctions.Value)
				{
					GUILayout.BeginHorizontal(style, EmptyGuiLayoutOptions);
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["openSlotCondMenu"], EmptyGuiLayoutOptions))
					{
						_openSlotConditions = s.Id;
					}
					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"], EmptyGuiLayoutOptions);
					var cateNameLabel = !string.IsNullOrEmpty(s.EntryName) ? s.EntryName : "";
					cateNameLabel += _tabSelection == 0 && !string.IsNullOrEmpty(cateNameLabel) ? " | " : "";
					cateNameLabel += _tabSelection == 0 ? string.IsNullOrEmpty(s.Maid) ? ShapeKeyMaster.CurrentLanguage["global"] : s.Maid : "";
					cateNameLabel = cateNameLabel.Length > 25 ? cateNameLabel.Substring(0, 25) + "..." : cateNameLabel;
					GUILayout.Label(cateNameLabel, EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();

					if (EntryComparer.Mode == 4)
					{
						BuildOrderNumControls(s);
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["copy"], EmptyGuiLayoutOptions))
					{
						var newEntry = s.Clone() as ShapeKeyEntry;
						newEntry.EntryName += "(Copy)";
						SkDatabase.Add(newEntry);
						FocusToNewKey(newEntry.Id, givenShapeKeys);
					}
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"], EmptyGuiLayoutOptions))
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

		private static void DisplayShapeKeyEntriesMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			if (_tabSelection != 2 && GUILayout.Button(ShapeKeyMaster.CurrentLanguage["addNewShapekey"], EmptyGuiLayoutOptions))
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

				GUILayout.BeginVertical(style, EmptyGuiLayoutOptions);

				if (s.Collapsed == false)
				{
					GUILayout.BeginHorizontal(style, EmptyGuiLayoutOptions);
					GUILayout.Label(s.EntryName, EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("☰", EmptyGuiLayoutOptions))
					{
						s.Collapsed = !s.Collapsed;
					}
					GUILayout.EndHorizontal();

					var status = s.Enabled == 0 ? "ignore" : s.Enabled == 1 ? "zero" : "on";
					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage[status], EmptyGuiLayoutOptions))
					{
						s.Enabled -= 1;
					}

					GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekey"], GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maidOptional"], GUILayout.Width(200));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+", EmptyGuiLayoutOptions))
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
						_oldPreSkMenuScrollPosition = _scrollPosition_MaidShapeKeyList;
						_scrollPosition_ShapeKeySelectMenu = _oldSkMenuScrollPosition;

						return;
					}

					s.ShapeKey = GUILayout.TextField(s.ShapeKey, GUILayout.Width(ShapeKeyMaster.MinShapekeyNameTextboxWidth.Value));

					GUILayout.FlexibleSpace();

					if (GUILayout.Button("+", EmptyGuiLayoutOptions))
					{
						_openMaidMenu = s.Id;
						_maidNameList = Extensions.GetNameOfAllMaids().ToList();
						return;
					}
					GUILayout.Label(s.Maid, GUILayout.Width(200));

					GUILayout.FlexibleSpace();

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));

					s.Animate = GUILayout.Toggle(s.Animate, ShapeKeyMaster.CurrentLanguage["animate"], EmptyGuiLayoutOptions);

					s.AnimateWithExcitement = GUILayout.Toggle(s.AnimateWithExcitement, ShapeKeyMaster.CurrentLanguage["animateWithYotogiExcitement"], EmptyGuiLayoutOptions);

					switch (s.Animate)
					{
						case true:
							GUILayout.Label(
								$"{ShapeKeyMaster.CurrentLanguage["animationSpeed"]} = (1000 ms / {(s.AnimationPollFloat * 1000).ToString(CultureInfo.CurrentCulture)} ms) x {s.AnimationRate} = {(1000 / (s.AnimationPollFloat * 1000) * s.AnimationRateFloat).ToString(CultureInfo.CurrentCulture)} %/{ShapeKeyMaster.CurrentLanguage["seconds"]}", EmptyGuiLayoutOptions);

							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekeyDeformation"], EmptyGuiLayoutOptions);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxAnimationDeformation"], EmptyGuiLayoutOptions);
							s.AnimationMaximum = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.AnimationMaximum, s.AnimationMinimum, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minAnimationDeformation"], EmptyGuiLayoutOptions);
							s.AnimationMinimum = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.AnimationMinimum, 0, ShapeKeyMaster.MaxDeform.Value));

							GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["animationRate"]}: {s.AnimationRate}", EmptyGuiLayoutOptions);
							GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
							s.AnimationRate = GUILayout.TextField(s.AnimationRate, EmptyGuiLayoutOptions);
							GUILayout.EndHorizontal();
							GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["animationPollingRate"]}: {s.AnimationPoll}", EmptyGuiLayoutOptions);
							s.AnimationPoll = GUILayout.TextField(s.AnimationPoll, EmptyGuiLayoutOptions);
							break;

						case false when s.AnimateWithExcitement:

							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxExcitementThreshold"], EmptyGuiLayoutOptions);
							s.ExcitementMax = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.ExcitementMax, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minExcitementThreshold"], EmptyGuiLayoutOptions);
							s.ExcitementMin = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.ExcitementMin, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["defShapekeyDeformation"], EmptyGuiLayoutOptions);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["maxShapekeyDeformation"], EmptyGuiLayoutOptions);
							s.DeformMax = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.DeformMax, s.DeformMin, ShapeKeyMaster.MaxDeform.Value));
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["minShapekeyDeformation"], EmptyGuiLayoutOptions);
							s.DeformMin = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.DeformMin, 0, s.DeformMax));
							break;

						default:
							GUILayout.Label(ShapeKeyMaster.CurrentLanguage["shapekeyDeformation"], EmptyGuiLayoutOptions);
							s.Deform = Mathf.RoundToInt(HorizontalSliderWithInputBox(s.Deform, 0, ShapeKeyMaster.MaxDeform.Value));
							break;
					}

					GUILayout.BeginHorizontal(style, EmptyGuiLayoutOptions);

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["openSlotCondMenu"], EmptyGuiLayoutOptions))
					{
						_openSlotConditions = s.Id;
						return;
					}

					s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"], EmptyGuiLayoutOptions);

					GUILayout.FlexibleSpace();

					if (EntryComparer.Mode == 4)
					{
						BuildOrderNumControls(s);
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["rename"], EmptyGuiLayoutOptions))
					{
						_openRenameMenu = s.Id;
						return;
					}

					if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["delete"], EmptyGuiLayoutOptions))
					{
						SkDatabase.Remove(s);
						return;
					}

					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.BeginHorizontal(_sections, EmptyGuiLayoutOptions);
					GUILayout.Label(s.EntryName, EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("☰", EmptyGuiLayoutOptions))
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

			GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

			GUILayout.Label($"{s.EntryName}: {ShapeKeyMaster.CurrentLanguage["selectShapekey"]}", EmptyGuiLayoutOptions);

			GUILayout.FlexibleSpace();

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["rclickToBlacklist"], EmptyGuiLayoutOptions);

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["collapseShapekeyList"], EmptyGuiLayoutOptions);
			_collapseBodyShapekeys = GUILayout.Toggle(_collapseBodyShapekeys, ShapeKeyMaster.CurrentLanguage["body"], EmptyGuiLayoutOptions);
			_collapseHeadShapekeys = GUILayout.Toggle(_collapseHeadShapekeys, ShapeKeyMaster.CurrentLanguage["head"], EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"], EmptyGuiLayoutOptions))
			{
				_openSkMenu = Guid.Empty;
				_oldSkMenuFilter = _filter;
				_oldSkMenuScrollPosition = _scrollPosition_ShapeKeySelectMenu;
				_scrollPosition_MaidShapeKeyList = _oldPreSkMenuScrollPosition;

				_filter = "";
			}

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"], EmptyGuiLayoutOptions))
			{
				_openSkMenu = Guid.Empty;
				_oldSkMenuFilter = _filter;
				_oldSkMenuScrollPosition = _scrollPosition_ShapeKeySelectMenu;
				_scrollPosition_MaidShapeKeyList = _oldPreSkMenuScrollPosition;

				s.ShapeKey = "";
				_filter = "";
			}

			_scrollPosition_ShapeKeySelectMenu = GUILayout.BeginScrollView(_scrollPosition_ShapeKeySelectMenu, EmptyGuiLayoutOptions);

			var columns = 0;
			var totalGroupsWorked = 0;
			
			var groupedKeys = _shapeKeysNameList
				.GroupBy(r => r.item1)
				.OrderBy(r => r.Key)
				.ToArray();

			var bodyKeys = new HashSet<string>(groupedKeys
				.Where(r => r.Key.Equals("body", StringComparison.Ordinal))
				.SelectMany(r => r)
				.Select(t => t.item2),
				StringComparer.Ordinal);

			var headKeys = new HashSet<string>(groupedKeys
				.Where(r => r.Key.Equals("head", StringComparison.Ordinal))
				.SelectMany(r => r)
				.Select(t => t.item2),
				StringComparer.Ordinal);

			foreach (var group in groupedKeys)
			{
				totalGroupsWorked++;

				var filteredList = group.AsEnumerable();
				
				if (_filterCommonKeys)
				{
					if (group.Key.Equals("body", StringComparison.Ordinal) == false)
					{
						filteredList = filteredList.Where(t => !bodyKeys.Contains(t.item2));
					}

					if (group.Key.Equals("head", StringComparison.Ordinal) == false)
					{
						filteredList = filteredList.Where(t => !headKeys.Contains(t.item2));
					}
				}

				if (_hideBlacklistedKeys)
				{
					filteredList = filteredList.Where(t => SkDatabase.BlacklistedShapeKeys.IsBlacklisted(t.item2) == false);
				}
				
				if (filteredList.Any())
				{
					if (columns++ == 0)
					{
						//Columnar ordering
						GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
					}

					//Category vertical
					GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

					//Header for category
					GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();
					GUILayout.Label(ShapeKeyMaster.CurrentLanguage[group.Key], EmptyGuiLayoutOptions);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					var collapse = (group.Key.Equals("body") && _collapseBodyShapekeys) || (group.Key.Equals("head") && _collapseHeadShapekeys);

					if (!collapse)
					{
						foreach (var str in filteredList.OrderBy(r => r.item2))
						{
							if (_filter != "")
							{
								if (str.item2.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
								{
									continue;
								}
							}

							var style = _hideBlacklistedKeys == false && SkDatabase.BlacklistedShapeKeys.IsBlacklisted(str.item2) ? _blacklistedButton : UiUserOverrides.GetButtonStyleOverride();
							if (GUILayout.Button(str.item2, style, EmptyGuiLayoutOptions))
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
										_oldSkMenuScrollPosition = _scrollPosition_ShapeKeySelectMenu;
										_scrollPosition_MaidShapeKeyList = _oldPreSkMenuScrollPosition;
										_filter = "";
										break;
								}
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
			
			GUILayout.EndScrollView();
		}

		private static void DisplayMaidSelectMenu(ShapeKeyEntry s)
		{
			DisplaySearchMenu(true);

			GUILayout.Label($"{s.EntryName} {ShapeKeyMaster.CurrentLanguage["selectMaid"]}", EmptyGuiLayoutOptions);

			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);

			s.Maid = GUILayout.TextField(s.Maid, GUILayout.Width(200));

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"], GUILayout.Width(80)))
			{
				_openMaidMenu = Guid.Empty;
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"], EmptyGuiLayoutOptions))
			{
				_openMaidMenu = Guid.Empty;
				s.Maid = "";
				_filter = "";
			}

			_scrollPosition_MaidSelectMenu = GUILayout.BeginScrollView(_scrollPosition_MaidSelectMenu, EmptyGuiLayoutOptions);

			foreach (var mn in _maidNameList)
			{
				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn, EmptyGuiLayoutOptions))
				{
					_openMaidMenu = Guid.Empty;
					s.Maid = mn;
					_filter = "";
				}
			}

			GUILayout.EndScrollView();
		}

		private static void DisplayMaidGroupCreateMenu(Dictionary<Guid, ShapeKeyEntry> givenShapeKeys)
		{
			DisplaySearchMenu(true);

			ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"], EmptyGuiLayoutOptions);

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["selectNewMaidGroup"], EmptyGuiLayoutOptions);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["cancel"], EmptyGuiLayoutOptions))
			{
				_maidGroupCreateOpen = false;
			}

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["none"], EmptyGuiLayoutOptions))
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

			_scrollPosition_MaidGroupCreateMenu = GUILayout.BeginScrollView(_scrollPosition_MaidGroupCreateMenu, EmptyGuiLayoutOptions);

			foreach (var mn in _maidNameList)
			{
				if (ShapeKeyMaster.HideInactiveMaids.Value && !Extensions.IsMaidActive(mn))
				{
					continue;
				}

				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn, EmptyGuiLayoutOptions))
				{
					var newKey = Guid.NewGuid();

					SkDatabase.Add(new ShapeKeyEntry(newKey, mn));

					FocusToNewKey(newKey, givenShapeKeys);

					_maidGroupCreateOpen = false;

					_filter = "";
				}
			}

			GUILayout.EndScrollView();
		}

		private static void DisplayRenameMenu(ShapeKeyEntry s)
		{
			_scrollPosition_RenameMenu = GUILayout.BeginScrollView(_scrollPosition_RenameMenu, EmptyGuiLayoutOptions);
			GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["nowRenaming"]} {s.EntryName}", EmptyGuiLayoutOptions);

			s.EntryName = GUILayout.TextField(s.EntryName, EmptyGuiLayoutOptions);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"], EmptyGuiLayoutOptions))
			{
				_openRenameMenu = Guid.Empty;
			}
			GUILayout.EndScrollView();
		}

		private static void DisplaySlotConditionsMenu(ShapeKeyEntry s)
		{
			_scrollPosition_SlotConditionsMenu = GUILayout.BeginScrollView(_scrollPosition_SlotConditionsMenu, EmptyGuiLayoutOptions);

			GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["generalSettings"], EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			s.ConditionalsToggle = GUILayout.Toggle(s.ConditionalsToggle, ShapeKeyMaster.CurrentLanguage["enableConditionals"], EmptyGuiLayoutOptions);

			s.IgnoreCategoriesWithShapekey = GUILayout.Toggle(s.IgnoreCategoriesWithShapekey, ShapeKeyMaster.CurrentLanguage["ignoreCateWithKey"], EmptyGuiLayoutOptions);

			s.DisableWhen = GUILayout.Toggle(s.DisableWhen, s.DisableWhen ? ShapeKeyMaster.CurrentLanguage["setShapekeyIf"] : ShapeKeyMaster.CurrentLanguage["doNotSetShapekeyIf"], EmptyGuiLayoutOptions);

			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["disabledKeyDeform"], EmptyGuiLayoutOptions);
			s.DisabledDeform = GUILayout.HorizontalSlider(s.DisabledDeform, 0, ShapeKeyMaster.MaxDeform.Value, EmptyGuiLayoutOptions);

			s.WhenAll = GUILayout.Toggle(s.WhenAll, s.WhenAll ? ShapeKeyMaster.CurrentLanguage["ifAllSlotsEquipped"] : ShapeKeyMaster.CurrentLanguage["ifAnySlotEquipped"], EmptyGuiLayoutOptions);

			GUILayout.EndHorizontal();

			GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["slots"], EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			var i = 0;

			foreach (var slot in Enum.GetValues(typeof(DisableWhenEquipped)).Cast<DisableWhenEquipped>())
			{
				if (i == 0)
				{
					GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);
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

			GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

			GUILayout.BeginHorizontal(_sections2, EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.Label(ShapeKeyMaster.CurrentLanguage["menuFiles"], EmptyGuiLayoutOptions);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			_scrollPosition1 = GUILayout.BeginScrollView(_scrollPosition1, EmptyGuiLayoutOptions);

			var tempMenuFileConditions = new Dictionary<Guid, string>(s.MenuFileConditionals);

			foreach (var menu in tempMenuFileConditions.Keys)
			{
				GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

				if (GUILayout.Button("X", GUILayout.Width(20)))
				{
					s.MenuFileConditionals.Remove(menu);
					return;
				}

				s.MenuFileConditionals[menu] = GUILayout.TextField(s.MenuFileConditionals[menu], EmptyGuiLayoutOptions);

				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["addNewMenuFile"], EmptyGuiLayoutOptions))
			{
				s.MenuFileConditionals[Guid.NewGuid()] = "";
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["finish"], EmptyGuiLayoutOptions))
			{
				_openSlotConditions = Guid.Empty;
			}
			GUILayout.EndScrollView();
		}

		private static void DisplayMaidRenameMenu(string s)
		{
			DisplaySearchMenu(true);

			ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"], EmptyGuiLayoutOptions);

			GUILayout.Label($"{ShapeKeyMaster.CurrentLanguage["renamingMaidGroup"]}: {s}", EmptyGuiLayoutOptions);

			GUILayout.BeginHorizontal(EmptyGuiLayoutOptions);

			_maidGroupRename = GUILayout.TextField(_maidGroupRename, EmptyGuiLayoutOptions);

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["apply"], EmptyGuiLayoutOptions))
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

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["cancel"], EmptyGuiLayoutOptions))
			{
				_maidGroupRename = "";
				_maidGroupRenameMenu = "";
			}

			_scrollPosition_MaidRenameMenu = GUILayout.BeginScrollView(_scrollPosition_MaidRenameMenu, EmptyGuiLayoutOptions);

			foreach (var mn in _maidNameList)
			{
				if (ShapeKeyMaster.HideInactiveMaids.Value && !Extensions.IsMaidActive(mn))
				{
					continue;
				}

				if (_filter != "")
				{
					if (mn.Contains(_filter, StringComparison.OrdinalIgnoreCase) == false)
					{
						continue;
					}
				}

				if (GUILayout.Button(mn, EmptyGuiLayoutOptions))
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

			GUILayout.EndScrollView();
		}

		private static void DisplayExportMenu()
		{
			_scrollPosition_ExportMenu = GUILayout.BeginScrollView(_scrollPosition_ExportMenu, EmptyGuiLayoutOptions);

			ShapeKeyMaster.HideInactiveMaids.Value = GUILayout.Toggle(ShapeKeyMaster.HideInactiveMaids.Value, ShapeKeyMaster.CurrentLanguage["hideInactiveMaids"], EmptyGuiLayoutOptions);

			if (ShapeKeyMaster.HideInactiveMaids.Value == false && GUILayout.Button(ShapeKeyMaster.CurrentLanguage["all"], EmptyGuiLayoutOptions))
			{
				ShapeKeyMaster.PluginLogger.LogMessage(ShapeKeyMaster.CurrentLanguage["exportingAll"]);

				ShapeKeyMaster.SaveToJson(null, SkDatabase, true);
			}

			GUILayout.BeginVertical(_sections, EmptyGuiLayoutOptions);

			if (ThingsToExport.ContainsKey("global") == false)
			{
				ThingsToExport["global"] = false;
			}

			ThingsToExport["global"] = GUILayout.Toggle(ThingsToExport["global"], ShapeKeyMaster.CurrentLanguage["global"], EmptyGuiLayoutOptions);

			foreach (var m in SkDatabase.ListOfMaidsWithKeys())
			{
				if (ShapeKeyMaster.HideInactiveMaids.Value && !Extensions.IsMaidActive(m)) continue;

				if (ThingsToExport.ContainsKey(m) == false)
				{
					ThingsToExport[m] = false;
				}

				ThingsToExport[m] = GUILayout.Toggle(ThingsToExport[m], m, EmptyGuiLayoutOptions);
			}

			GUILayout.EndVertical();

			if (GUILayout.Button(ShapeKeyMaster.CurrentLanguage["done"], EmptyGuiLayoutOptions))
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

			GUILayout.EndScrollView();
		}

		private static void BuildPageManagerLabel(string headerString, int applicantCount)
		{
			if (EntryComparer.Ascending)
			{
				var countStart = _page + 1;
				var countEnd = Math.Min(_page + ShapeKeyMaster.EntriesPerPage.Value, applicantCount);
				if (countStart == applicantCount)
				{
					GUILayout.Label($"{headerString} : {countStart}", EmptyGuiLayoutOptions);
				}
				else
				{
					GUILayout.Label($"{headerString} : {countStart} ~ {countEnd}", EmptyGuiLayoutOptions);
				}
			}
			else
			{
				var countStart = applicantCount - _page;
				var countEnd = Math.Max(countStart - ShapeKeyMaster.EntriesPerPage.Value + 1, 1);
				if (countStart == 1)
				{
					GUILayout.Label($"{headerString} : {countStart.ToString()}", EmptyGuiLayoutOptions);
				}
				else
				{
					GUILayout.Label($"{headerString} : {countStart.ToString()} ~ {countEnd.ToString()}", EmptyGuiLayoutOptions);
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
				s.OrderNumTmp = IntField(s.OrderNumTmp.Value, 0, 999);

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

		private static int ConvertDefaultTabSelection(string tabSelStr)
		{
			switch (tabSelStr)
			{
				case "All":
					return 0;
				case "Globals":
					return 1;
				case "Maids":
					return 2;
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
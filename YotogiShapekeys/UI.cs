using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ShapekeyMaster
{
	internal static class UI
	{
		public static SortedDictionary<int, ShapeKeyEntry> ShapeKeys = new SortedDictionary<int, ShapeKeyEntry>();

		private static List<string> ShapekeysNameList = new List<string>();
		private static List<string> MaidNameList = new List<string>();

		private static readonly List<int> DeleteList = new List<int>();
		public static int availID = 0;

		private static readonly int WindowID = 777777;

		private static bool runonce = true;
		public static bool changewasmade = false;

		private static string Filter = "";
		private static int FilterMode = 0;

		//private static int ToolbarSelection = 0;
		private static readonly string[] ToolbarStrings = new string[2] { "Maids", "Props" };

		private static Vector2 scrollPosition = Vector2.zero;
		private static int OpenSKMenu = 999999999;
		private static int OpenMaidMenu = 999999999;
		private static int OpenRenameMenu = 999999999;

		private static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 4, Screen.width / 4, Screen.height / 2);

		private static GUIStyle seperator;

		public static void Initialize()
		{

			if (runonce)
			{
				seperator = new GUIStyle(GUI.skin.horizontalSlider);
				seperator.fixedHeight = 1f;
				seperator.normal.background = Texture2D.whiteTexture;
				seperator.margin.top = 10;
				seperator.margin.bottom = 10;

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
			if (ShapeKeys.ContainsKey(OpenSKMenu))
			{
				DisplayShapeKeySelectMenu(ShapeKeys[OpenSKMenu]);

			}
			else if (ShapeKeys.ContainsKey(OpenMaidMenu))
			{
				DisplayMaidSelectMenu(ShapeKeys[OpenMaidMenu]);

			}
			else if (ShapeKeys.ContainsKey(OpenRenameMenu))
			{
				DisplayRenameMenu(ShapeKeys[OpenRenameMenu]);

			}
			else
			{
				DisplayHeaderMenu();

				foreach (int i in DeleteList)
				{
					ShapeKeys.Remove(i);
				}

				DeleteList.Clear();

				DisplayShapeKeyEntriesMenu(ShapeKeys);
			}
			/*}
			else if (ToolbarSelection == 1)
			{


				if (true)
				{
					DisplayHeaderMenu();

					foreach (int i in DeleteList)
					{
						ShapeKeys.Remove(i);
					}

					DeleteList.Clear();

					DisplayPropShapeKeyEntriesMenu(ShapeKeys);
				}

				if (GUI.changed || changewasmade)
				{
					changewasmade = true;
					if (MakeshiftTimer-- <= 0)
					{
						BlendShapeFetcherSetter.RunAll();
						MakeshiftTimer = 30;
						changewasmade = false;
					}
				}

			}*/

			GUILayout.EndScrollView();

			if (UI.ShapeKeys.Count > 0)
			{

				if (GUILayout.Button("Apply"))
				{
					ShapekeyFetcherSetter.RunAll();
				}
				if (GUILayout.Button("Save"))
				{
#if (DEBUG)
					Debug.Log("Saving data to configs now!");
#endif

					File.WriteAllText(BepInEx.Paths.ConfigPath + "\\ShapekeyMaster.json", JsonConvert.SerializeObject(ShapeKeys));
				}
			}

			ChkMouseClick();
		}

		private static void DisplayHeaderMenu()
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label("Search by ");

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

			Filter = GUILayout.TextField(Filter);

			GUILayout.EndHorizontal();
		}

		private static void DisplayShapeKeyEntriesMenu(SortedDictionary<int, ShapeKeyEntry> ShapeKeys)
		{
			if (GUILayout.Button("Add New Shapekey"))
			{
#if (DEBUG)
				Debug.Log("I've been clicked! Oh the humanity!!");
#endif

				while (ShapeKeys.ContainsKey(availID))
				{
					++availID;
				}

				ShapeKeys.Add(availID, new ShapeKeyEntry(availID));
			}

			GUILayout.Label("", seperator);

			foreach (ShapeKeyEntry s in ShapeKeys.Values)
			{

				if (s.IsProp == true)
				{
					continue;
				}

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

				if (s.Collapsed == false)
				{
					if (GUILayout.Button($"Collapse {s.EntryName}"))
					{
						s.Collapsed = !s.Collapsed;
					}

					s.SetEnabled(GUILayout.Toggle(s.Enabled, $"Enable"));
					GUILayout.BeginHorizontal();
					GUILayout.Label("ShapeKey");
					GUILayout.Label("Maid (Optional)");
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();

					if (GUILayout.Button("+"))
					{
						OpenSKMenu = s.Id;
						ShapekeysNameList = HelperClasses.GetAllShapeKeysFromMaidList(ShapekeyFetcherSetter.GetMaidsList()).ToList();
						ShapekeysNameList.Sort();
					}

					s.SetShapeKey(GUILayout.TextField(s.ShapeKey));
					s.SetMaid(GUILayout.TextField(s.Maid));

					if (GUILayout.Button("+"))
					{
						OpenMaidMenu = s.Id;
						MaidNameList = ShapekeyFetcherSetter.GetMaidsList().Select(m => m.status.fullNameJpStyle).ToList();
						MaidNameList.Sort();
					}

					GUILayout.EndHorizontal();

					//s.SetAnimateWithOrgasm(GUILayout.Toggle(s.GetAnimateWithOrgasm(), $"Animate during orgasm"));
					if (s.AnimateWithExcitement == false && s.SetAnimate(GUILayout.Toggle(s.Animate, $"Animate")))
					{
						GUILayout.Label($"Animation speed = (1000 ms / {s.GetAnimationPoll() * 1000} ms) x {s.GetAnimationRate()} = {1000 / (s.GetAnimationPoll() * 1000) * s.GetAnimationRate()} %/Second");
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.SetDeform(GUILayout.HorizontalSlider(s.Deform, 0, 100.0F));
						GUILayout.Label($"Max Animation Deformation: {s.AnimationMaximum}");
						s.SetAnimationMaximum(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.AnimationMaximum, s.AnimationMinimum, 100)));
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
						s.SetDeform(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, 100.0F)));
						GUILayout.Label($"Max Shapekey Deformation: {s.DeformMax}");
						s.SetDeformMax(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMax, s.DeformMin, 100.0F)));
						GUILayout.Label($"Min Shapekey Deformation: {s.DeformMin}");
						s.SetDeformMin(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.DeformMin, 0.0F, s.DeformMax)));
					}
					else
					{
						GUILayout.Label($"Shapekey Deformation: {s.Deform}");
						s.SetDeform(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, 100.0F)));
					}

					if (GUILayout.Button($"Rename {s.EntryName}"))
					{
						OpenRenameMenu = s.Id;
					}

					if (GUILayout.Button($"Delete {s.EntryName}"))
					{
						DeleteList.Add(s.Id);
					}
				}
				else
				{
					if (GUILayout.Button($"Expand {s.EntryName}"))
					{
						s.Collapsed = !s.Collapsed;
					}
				}
				GUILayout.Label("", seperator);
			}
		}
		private static void DisplayPropShapeKeyEntriesMenu(SortedDictionary<int, ShapeKeyEntry> ShapeKeys)
		{
			if (GUILayout.Button("Add New Shapekey"))
			{
#if (DEBUG)
				Debug.Log("I've been clicked! Oh the humanity!!");
#endif

				while (ShapeKeys.ContainsKey(availID))
				{
					++availID;
				}

				ShapeKeys.Add(availID, new ShapeKeyEntry(availID, true));
			}

			GUILayout.Label("", seperator);

			foreach (ShapeKeyEntry s in ShapeKeys.Values)
			{

				if (s.IsProp == false)
				{
					continue;
				}

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

				if (s.Collapsed == false)
				{
					if (GUILayout.Button($"Collapse {s.EntryName}"))
					{
						s.Collapsed = !s.Collapsed;
					}

					s.SetEnabled(GUILayout.Toggle(s.Enabled, $"Enable"));
					GUILayout.BeginHorizontal();
					GUILayout.Label("ShapeKey");
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();

					if (GUILayout.Button("+"))
					{

					}

					s.SetShapeKey(GUILayout.TextField(s.ShapeKey));

					GUILayout.EndHorizontal();

					GUILayout.Label($"Shapekey Deformation: {s.DeformMax}");
					s.SetDeform(Mathf.RoundToInt(GUILayout.HorizontalSlider(s.Deform, 0, 100.0F)));

					if (GUILayout.Button($"Rename {s.EntryName}"))
					{
						OpenRenameMenu = s.Id;
					}

					if (GUILayout.Button($"Delete {s.EntryName}"))
					{
						DeleteList.Add(s.Id);
					}
				}
				else
				{
					if (GUILayout.Button($"Expand {s.EntryName}"))
					{
						s.Collapsed = !s.Collapsed;
					}
				}
				GUILayout.Label("", seperator);
			}
		}
		private static void DisplayShapeKeySelectMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"{s.EntryName} Select ShapeKey");

			if (GUILayout.Button("None"))
			{
				OpenSKMenu = 999999999;
				s.SetShapeKey("");
			}

			foreach (string str in ShapekeysNameList)
			{
				if (GUILayout.Button(str))
				{
					OpenSKMenu = 999999999;
					s.SetShapeKey(str);
				}
			}
		}
		private static void DisplayMaidSelectMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"{s.EntryName} Select Maid");

			if (GUILayout.Button("None"))
			{
				OpenMaidMenu = 999999999;
				s.SetMaid("");
			}

			foreach (string mn in MaidNameList)
			{
				if (GUILayout.Button(mn))
				{
					OpenMaidMenu = 999999999;
					s.SetMaid(mn);
				}
			}
		}
		private static void DisplayRenameMenu(ShapeKeyEntry s)
		{
			GUILayout.Label($"Now renaming {s.EntryName}");

			s.EntryName = GUILayout.TextField(s.EntryName);

			if (GUILayout.Button("Finish"))
			{
				OpenRenameMenu = 999999999;
			}
		}
		private static void ChkMouseClick()
		{
			if (Input.GetMouseButtonUp(0) && IsMouseOnGUI())
			{
				Input.ResetInputAxes();
			}
		}
		private static bool IsMouseOnGUI()
		{
			Vector2 point = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return windowRect.Contains(point);
		}
	}
}

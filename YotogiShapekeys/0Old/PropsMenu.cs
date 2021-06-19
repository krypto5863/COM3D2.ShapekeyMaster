using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShapekeyMaster._0Old
{
	class PropsMenu
	{
		private static void DisplayPropShapeKeyEntriesMenu(SortedDictionary<Guid, ShapeKeyEntry> ShapeKeys)
		{
			if (GUILayout.Button("Add New Shapekey"))
			{
#if (DEBUG)
				Main.logger.
				("I've been clicked! Oh the humanity!!");
#endif

				Guid newkey = Guid.NewGuid();

				ShapeKeys.Add(newkey, new ShapeKeyEntry(newkey, null, true));
			}

			//GUILayout.Label("", seperator);

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
				//GUILayout.Label("", seperator);
			}
		}
	}
}

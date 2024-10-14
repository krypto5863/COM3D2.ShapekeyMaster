using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShapeKeyMaster.GUI
{
	internal static class UIUserOverrides
	{
		private static Dictionary<string, GUIStyle> cache = new Dictionary<string, GUIStyle>();
		internal static GUIStyle getButtonStyleOverride()
		{
			GUIStyle buttonStyleOverride;
			if (!cache.TryGetValue("ButtonStyleOverride", out buttonStyleOverride))
			{
				buttonStyleOverride = new GUIStyle(UnityEngine.GUI.skin.button)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				cache.Add("ButtonStyleOverride", buttonStyleOverride);
				return buttonStyleOverride;
			}
			else 
			{
				buttonStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
				return buttonStyleOverride;
			}
		}

		internal static GUIStyle getLabelStyleOverride()
		{
			GUIStyle labelStyleOverride;
			if (!cache.TryGetValue("LabelStyleOverride", out labelStyleOverride))
			{
				labelStyleOverride = new GUIStyle(UnityEngine.GUI.skin.label)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				cache.Add("LabelStyleOverride", labelStyleOverride);
				return labelStyleOverride;
			}
			else
			{
				labelStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
				return labelStyleOverride;
			}
		}

		internal static GUIStyle getTextFieldStyleOverride()
		{
			GUIStyle textFieldStyleOverride;
			if (!cache.TryGetValue("TextFieldStyleOverride", out textFieldStyleOverride))
			{
				textFieldStyleOverride = new GUIStyle(UnityEngine.GUI.skin.textField)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				cache.Add("TextFieldStyleOverride", textFieldStyleOverride);
				return textFieldStyleOverride;
			}
			else
			{
				textFieldStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
				return textFieldStyleOverride;
			}
		}

		internal static GUIStyle getToggleStyleOverride()
		{
			GUIStyle toggleStyleOverride;
			if (!cache.TryGetValue("ToggleStyleOverride", out toggleStyleOverride))
			{
				toggleStyleOverride = new GUIStyle(UnityEngine.GUI.skin.toggle)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				cache.Add("ToggleStyleOverride", toggleStyleOverride);
				return toggleStyleOverride;
			}
			else
			{
				toggleStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
				return toggleStyleOverride;
			}
		}

		internal static GUIStyle getSliderStyleOverride()
		{
			GUIStyle sliderStyleOverride;
			if (!cache.TryGetValue("SliderStyleOverride", out sliderStyleOverride))
			{
				sliderStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSlider)
				{
					fixedHeight = ShapeKeyMaster.SliderSize.Value
				};
				cache.Add("SliderStyleOverride", sliderStyleOverride);
				return sliderStyleOverride;
			}
			else
			{
				sliderStyleOverride.fixedHeight = ShapeKeyMaster.SliderSize.Value;
				return sliderStyleOverride;
			}
		}

		internal static GUIStyle getSliderThumbStyleOverride()
		{
			GUIStyle sliderThumbStyleOverride;
			if (!cache.TryGetValue("SliderThumbStyleOverride", out sliderThumbStyleOverride))
			{
				sliderThumbStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSliderThumb)
				{
					fixedHeight = ShapeKeyMaster.SliderSize.Value,
					fixedWidth = ShapeKeyMaster.SliderSize.Value
				};
				cache.Add("SliderThumbStyleOverride", sliderThumbStyleOverride);
				return sliderThumbStyleOverride;
			}
			else
			{
				sliderThumbStyleOverride.fixedHeight = ShapeKeyMaster.SliderSize.Value;
				sliderThumbStyleOverride.fixedWidth = ShapeKeyMaster.SliderSize.Value;
				return sliderThumbStyleOverride;
			}
		}
	}
}

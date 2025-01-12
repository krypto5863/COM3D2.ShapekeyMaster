using System.Collections.Generic;
using UnityEngine;

namespace ShapeKeyMaster.GUI
{
	internal static class UiUserOverrides
	{
		private static readonly Dictionary<string, GUIStyle> Cache = new Dictionary<string, GUIStyle>();

		private static readonly GUISkin _CustomSkin = Object.Instantiate(UnityEngine.GUI.skin);

		internal static GUISkin CustomSkin
		{ 
			get 
			{
				_CustomSkin.button = GetButtonStyleOverride();
				_CustomSkin.label = GetLabelStyleOverride();
				_CustomSkin.toggle = GetToggleStyleOverride();
				_CustomSkin.textField = GetTextFieldStyleOverride();
				_CustomSkin.horizontalSlider = GetSliderStyleOverride();
				_CustomSkin.horizontalSliderThumb = GetSliderThumbStyleOverride();
				return _CustomSkin;
			}
		}

		internal static GUIStyle GetButtonStyleOverride()
		{
			if (!Cache.TryGetValue("ButtonStyleOverride", out var buttonStyleOverride))
			{
				buttonStyleOverride = new GUIStyle(UnityEngine.GUI.skin.button)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				Cache.Add("ButtonStyleOverride", buttonStyleOverride);
				return buttonStyleOverride;
			}

			buttonStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
			return buttonStyleOverride;
		}

		internal static GUIStyle GetLabelStyleOverride()
		{
			if (!Cache.TryGetValue("LabelStyleOverride", out var labelStyleOverride))
			{
				labelStyleOverride = new GUIStyle(UnityEngine.GUI.skin.label)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				Cache.Add("LabelStyleOverride", labelStyleOverride);
				return labelStyleOverride;
			}

			labelStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
			return labelStyleOverride;
		}

		internal static GUIStyle GetTextFieldStyleOverride()
		{
			if (!Cache.TryGetValue("TextFieldStyleOverride", out var textFieldStyleOverride))
			{
				textFieldStyleOverride = new GUIStyle(UnityEngine.GUI.skin.textField)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				Cache.Add("TextFieldStyleOverride", textFieldStyleOverride);
				return textFieldStyleOverride;
			}

			textFieldStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
			return textFieldStyleOverride;
		}

		internal static GUIStyle GetToggleStyleOverride()
		{
			if (!Cache.TryGetValue("ToggleStyleOverride", out var toggleStyleOverride))
			{
				toggleStyleOverride = new GUIStyle(UnityEngine.GUI.skin.toggle)
				{
					fontSize = ShapeKeyMaster.FontSize.Value
				};
				Cache.Add("ToggleStyleOverride", toggleStyleOverride);
				return toggleStyleOverride;
			}

			toggleStyleOverride.fontSize = ShapeKeyMaster.FontSize.Value;
			return toggleStyleOverride;
		}

		internal static GUIStyle GetSliderStyleOverride()
		{
			if (!Cache.TryGetValue("SliderStyleOverride", out var sliderStyleOverride))
			{
				sliderStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSlider)
				{
					fixedHeight = ShapeKeyMaster.SliderSize.Value
				};
				Cache.Add("SliderStyleOverride", sliderStyleOverride);
				return sliderStyleOverride;
			}

			sliderStyleOverride.fixedHeight = ShapeKeyMaster.SliderSize.Value;
			return sliderStyleOverride;
		}

		internal static GUIStyle GetSliderThumbStyleOverride()
		{
			if (!Cache.TryGetValue("SliderThumbStyleOverride", out var sliderThumbStyleOverride))
			{
				sliderThumbStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSliderThumb)
				{
					fixedHeight = ShapeKeyMaster.SliderSize.Value,
					fixedWidth = ShapeKeyMaster.SliderSize.Value
				};
				Cache.Add("SliderThumbStyleOverride", sliderThumbStyleOverride);
				return sliderThumbStyleOverride;
			}

			sliderThumbStyleOverride.fixedHeight = ShapeKeyMaster.SliderSize.Value;
			sliderThumbStyleOverride.fixedWidth = ShapeKeyMaster.SliderSize.Value;
			return sliderThumbStyleOverride;
		}
	}
}

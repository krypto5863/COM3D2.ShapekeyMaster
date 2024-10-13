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
		internal static GUIStyle getButtonStyleOverride()
		{
			var buttonStyleOverride = new GUIStyle(UnityEngine.GUI.skin.button)
			{
				fontSize = ShapeKeyMaster.FontSize.Value
			};
			return buttonStyleOverride;
		}

		internal static GUIStyle getLabelStyleOverride()
		{
			var labelStyleOverride = new GUIStyle(UnityEngine.GUI.skin.label)
			{
				fontSize = ShapeKeyMaster.FontSize.Value
			};
			return labelStyleOverride;
		}

		internal static GUIStyle getTextFieldStyleOverride()
		{
			var textFieldStyleOverride = new GUIStyle(UnityEngine.GUI.skin.textField)
			{
				fontSize = ShapeKeyMaster.FontSize.Value
			};
			return textFieldStyleOverride;
		}

		internal static GUIStyle getToggleStyleOverride()
		{
			return new GUIStyle(UnityEngine.GUI.skin.toggle)
			{
				fontSize = ShapeKeyMaster.FontSize.Value
			};
		}

		internal static GUIStyle getSliderStyleOverride()
		{
			var sliderStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSlider)
			{
				fixedHeight = ShapeKeyMaster.SliderSize.Value
			};
			return sliderStyleOverride;
		}

		internal static GUIStyle getSliderThumbStyleOverride()
		{
			var sliderThumbStyleOverride = new GUIStyle(UnityEngine.GUI.skin.horizontalSliderThumb)
			{
				fixedHeight = ShapeKeyMaster.SliderSize.Value,
				fixedWidth = ShapeKeyMaster.SliderSize.Value
			};
			return sliderThumbStyleOverride;
		}
	}
}

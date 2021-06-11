using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShapekeyMaster.MyGUI
{
	class Helpers
	{
		public static void ChkMouseClick(Rect windowRect)
		{
			if ((Input.mouseScrollDelta.y != 0 ||Input.GetMouseButtonUp(0)) && IsMouseOnGUI(windowRect))
			{
				Input.ResetInputAxes();
			}
		}
		public static bool IsMouseOnGUI(Rect windowRect)
		{
			Vector2 point = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return windowRect.Contains(point);
		}
		public static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; ++i)
			{
				pix[i] = col;
			}
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		public static Texture2D MakeTexWithRoundedCorner(Color col)
		{
			int xy = 12;
			{
				Texture2D result = new Texture2D(xy, xy);
				for (int i = 0; i < xy; ++i)
				{
					for (int j = 0; j < xy; j++)
					{
						bool topLeft = (i == 0 && (j == 0 || j == 1)) || (j == 0 && (i == 0 || i == 1));
						bool bottomLeft = (i == 0 && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == 0 || i == 1));
						bool topRight = (i == (xy - 1) && (j == 0 || j == 1)) || (j == 0 && (i == (xy - 1) || i == (xy - 2)));
						bool bottomRight = (i == (xy - 1) && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == (xy - 1) || i == (xy - 2)));

						//Corner
						if (topLeft || topRight || bottomLeft || bottomRight)
						{
							result.SetPixels(i, j, 1, 1, new Color[] { new Color(0, 0, 0, 0) });
						}
						//Border
						else if (i == 0 || j == 0 || i == (xy - 1) || j == (xy - 1) ||
								(i == 1 && j == 1) || (i == (xy - 2) && j == 1) || (i == 1 && j == (xy - 2)) || (i == (xy - 2) && j == (xy - 2)))
						{
							result.SetPixels(i, j, 1, 1, new Color[] { Color.black });
						}
						//Normal
						else
						{
							result.SetPixels(i, j, 1, 1, new Color[] { col });
						}
					}
				}

				result.Apply();
				return result;
			}
		}
	}
}

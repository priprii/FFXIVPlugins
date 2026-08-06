using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Ktisis.Editor.Selection;

namespace Ktisis.Common.Utility;

public static class GuiHelpers
{
	public static SelectMode GetSelectMode()
	{
		SelectMode result = SelectMode.Default;
		if (ImGui.IsKeyDown((ImGuiKey)641))
		{
			result = SelectMode.Multiple;
		}
		return result;
	}

	public static float CalcContrastRatio(uint background, uint foreground)
	{
		float num = (background >> 24) & 0xFF;
		float num2 = (foreground >> 24) & 0xFF;
		float num3 = (0.00083372544f * num * (float)((background >> 16) & 0xFF) + 0.0028047059f * num * (float)((background >> 8) & 0xFF) + 0.00028313725f * num * (float)(background & 0xFF) + 0.05f) / (0.00083372544f * num2 * (float)((foreground >> 16) & 0xFF) + 0.0028047059f * num2 * (float)((foreground >> 8) & 0xFF) + 0.00028313725f * num2 * (float)(foreground & 0xFF) + 0.05f);
		if (num3 < 1f)
		{
			return 1f / num3;
		}
		return num3;
	}

	public static uint CalcBlackWhiteTextColor(uint background)
	{
		if (!(CalcContrastRatio(background, uint.MaxValue) < 2f))
		{
			return uint.MaxValue;
		}
		return 4278190080u;
	}

	public static void OpenBrowser(string url)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = url,
			UseShellExecute = true
		});
	}

	public static Vector4 VectorColorFromString(string color)
	{
		string text = color.TrimStart('#');
		byte b = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
		byte b2 = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
		byte b3 = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
		byte b4 = byte.Parse(text.Substring(6, 2), NumberStyles.HexNumber);
		return new Vector4((float)(int)b / 255f, (float)(int)b2 / 255f, (float)(int)b3 / 255f, (float)(int)b4 / 255f);
	}
}

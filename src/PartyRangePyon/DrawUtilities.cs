using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace PartyRangePyon;

public class DrawUtilities
{
	public static void TextOutlined(Vector2 startingPosition, string text, float scale, KnownColor color)
	{
		startingPosition = new Vector2(MathF.Ceiling(startingPosition.X), MathF.Ceiling(startingPosition.Y));
		int num = (int)MathF.Ceiling(1f * scale);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				if (i != 0 || j != 0)
				{
					DrawText(startingPosition + new Vector2(i, j), text, KnownColor.Black, scale);
				}
			}
		}
		DrawText(startingPosition, text, color, scale);
	}

	public static Vector2 CalculateTextSize(string text, float scale)
	{
		Plugin.GameFont.Push();
		Vector2 vector = ImGui.CalcTextSize(text) / ImGuiHelpers.GlobalScale;
		Plugin.GameFont.Pop();
		return new Vector2(vector.X, vector.Y) * scale;
	}

	private static void DrawText(Vector2 drawPosition, string text, KnownColor color, float scale)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Plugin.GameFont.Push();
		ImFontPtr font = ImGui.GetFont();
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		((ImDrawListPtr)(ref backgroundDrawList)).AddText(font, ((ImFontPtr)(ref font)).FontSize * scale, drawPosition, ImGui.GetColorU32(ColorHelpers.Vector(color)), text);
		Plugin.GameFont.Pop();
	}
}

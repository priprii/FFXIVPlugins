using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Ui;

namespace PyonPix.Utility;

public static class UiUtil
{
	public unsafe static uint GameWidth => ((RenderTargetManager)RenderTargetManager.Instance()).Resolution_Width;

	public unsafe static uint GameHeight => ((RenderTargetManager)RenderTargetManager.Instance()).Resolution_Height;

	public static Vector2 GameResolution => new Vector2(GameWidth, GameHeight);

	public static Vector4 RGBA(int r, int g, int b, float a)
	{
		return new Vector4((float)r / 255f, (float)g / 255f, (float)b / 255f, a / 255f);
	}

	public static Vector2 CalcTextSize(string text, float fontSize, bool globalScale = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f) * (fontSize / ImGui.GetFontSize()) * (globalScale ? ImGuiHelpers.GlobalScale : 1f);
	}

	public static Vector2 CalcTextSize(IFontHandle font, string text, float? scale = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (scale.HasValue)
		{
			ImGui.SetWindowFontScale(scale.Value);
		}
		Vector2 result;
		using (font.Push())
		{
			result = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
		}
		if (scale.HasValue)
		{
			ImGui.SetWindowFontScale(1f);
		}
		return result;
	}

	public static Vector2 CalcIconTextSize(FontAwesomeIcon icon, string text, float? iconScale = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float iconTextPadding = UIShared.IconTextPadding;
		float iconTextPadding2 = UIShared.IconTextPadding;
		string text2 = FontAwesomeExtensions.ToIconString(icon);
		Vector2 vector = CalcTextSize(UIShared.NormalIconFont, text2, iconScale);
		Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
		return new Vector2(iconTextPadding * 2f + vector.X + iconTextPadding2 + vector2.X, MathF.Max(vector.Y, vector2.Y) + iconTextPadding * 2f);
	}

	public static Vector2 AlignCenter(Vector2 min, Vector2 max, Vector2 size)
	{
		return new Vector2(min.X + (max.X - min.X - size.X) * 0.5f, min.Y + (max.Y - min.Y - size.Y) * 0.5f);
	}

	public static Vector2 AlignCenter(Vector2 min, Vector2 max, float size)
	{
		return AlignCenter(min, max, new Vector2(size));
	}

	public static bool IsRectHovered(Vector2 rMin, Vector2 rMax)
	{
		if (ImGui.IsWindowHovered((ImGuiHoveredFlags)3))
		{
			return ImGui.IsMouseHoveringRect(rMin, rMax);
		}
		return false;
	}

	public static bool IsRectClicked(Vector2 rMin, Vector2 rMax, ImGuiMouseButton button = (ImGuiMouseButton)0)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsRectHovered(rMin, rMax))
		{
			return ImGui.IsMouseReleased(button);
		}
		return false;
	}

	public static FontAwesomeIcon GetIconForPixType(PixType type)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		return (FontAwesomeIcon)(type switch
		{
			PixType.Video => 62060, 
			PixType.Audio => 61441, 
			PixType.Image => 61502, 
			PixType.Game => 61723, 
			PixType.Light => 61675, 
			_ => 61874, 
		});
	}

	public static Vector2 CenterWindow(Vector2 windowSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
		Vector2 size = ((ImGuiViewportPtr)(ref mainViewport)).Size;
		return new Vector2(size.X / 2f - windowSize.X / 2f, size.Y / 2f - windowSize.Y / 2f);
	}

	public static void OpenDiscord()
	{
		Process.Start(new ProcessStartInfo("https://discord.gg/3wBtUrVDJh")
		{
			UseShellExecute = true
		});
	}

	public static void OpenKofi()
	{
		Process.Start(new ProcessStartInfo("https://ko-fi.com/primu")
		{
			UseShellExecute = true
		});
	}
}

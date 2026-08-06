using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;

namespace TriggerPyon;

public static class ImGuiEx
{
	private static readonly Dictionary<uint, bool> TreeOpenStates = new Dictionary<uint, bool>();

	private static Vector3 editingColour = Vector3.One;

	public static bool DrawHonorificTitle(DiscordCounter counter, string label, int index)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, 0u, !counter.IsEditing && counter.EditingIndex != index);
		try
		{
			if (InputText(label, counter.TitleTemplates[index], delegate(string x)
			{
				counter.TitleTemplates[index] = x;
			}, 32))
			{
				result = true;
			}
			counter.IsEditing = ImGui.IsItemActive();
			if (!counter.IsEditing)
			{
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				ImGuiStylePtr style = ImGui.GetStyle();
				ImGui.SetCursorScreenPos(itemRectMin + ((ImGuiStylePtr)(ref style)).FramePadding);
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				Vector2 itemRectMin2 = ImGui.GetItemRectMin();
				style = ImGui.GetStyle();
				Vector2 vector = itemRectMin2 + ((ImGuiStylePtr)(ref style)).FramePadding;
				Vector2 itemRectMax = ImGui.GetItemRectMax();
				style = ImGui.GetStyle();
				Vector2 vector2 = itemRectMax - ((ImGuiStylePtr)(ref style)).FramePadding;
				vector.Y = MathF.Max(vector.Y, ImGui.GetWindowPos().Y);
				vector2.Y = MathF.Min(vector2.Y, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight());
				((ImDrawListPtr)(ref windowDrawList)).PushClipRect(vector, vector2);
				ReadOnlySpan<byte> readOnlySpan = counter.ToSeString(counter.TitleTemplates[index], includeQuotes: false).Encode();
				SeStringDrawParams val2 = default(SeStringDrawParams);
				((SeStringDrawParams)(ref val2)).Color = uint.MaxValue;
				((SeStringDrawParams)(ref val2)).WrapWidth = float.MaxValue;
				((SeStringDrawParams)(ref val2)).TargetDrawList = windowDrawList;
				((SeStringDrawParams)(ref val2)).Font = UiBuilder.DefaultFont;
				((SeStringDrawParams)(ref val2)).FontSize = UiBuilder.DefaultFontSizePx;
				((SeStringDrawParams)(ref val2)).ScreenOffset = ImGui.GetCursorScreenPos();
				ImGuiHelpers.SeStringWrapped(readOnlySpan, ref val2, default(ImGuiId), (ImGuiButtonFlags)1);
				((ImDrawListPtr)(ref windowDrawList)).PopClipRect();
			}
			else
			{
				counter.EditingIndex = index;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return result;
	}

	public static bool DrawHonorificTitle(Counter counter, string label = "##counterTemplate")
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, 0u, !counter.IsEditing);
		try
		{
			if (InputText(label, counter.TitleTemplate, delegate(string x)
			{
				counter.TitleTemplate = x;
			}, 32))
			{
				result = true;
			}
			counter.IsEditing = ImGui.IsItemActive();
			if (!counter.IsEditing)
			{
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				ImGuiStylePtr style = ImGui.GetStyle();
				ImGui.SetCursorScreenPos(itemRectMin + ((ImGuiStylePtr)(ref style)).FramePadding);
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				Vector2 itemRectMin2 = ImGui.GetItemRectMin();
				style = ImGui.GetStyle();
				Vector2 vector = itemRectMin2 + ((ImGuiStylePtr)(ref style)).FramePadding;
				Vector2 itemRectMax = ImGui.GetItemRectMax();
				style = ImGui.GetStyle();
				Vector2 vector2 = itemRectMax - ((ImGuiStylePtr)(ref style)).FramePadding;
				vector.Y = MathF.Max(vector.Y, ImGui.GetWindowPos().Y);
				vector2.Y = MathF.Min(vector2.Y, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight());
				((ImDrawListPtr)(ref windowDrawList)).PushClipRect(vector, vector2);
				ReadOnlySpan<byte> readOnlySpan = counter.ToSeString(includeQuotes: false).Encode();
				SeStringDrawParams val2 = default(SeStringDrawParams);
				((SeStringDrawParams)(ref val2)).Color = uint.MaxValue;
				((SeStringDrawParams)(ref val2)).WrapWidth = float.MaxValue;
				((SeStringDrawParams)(ref val2)).TargetDrawList = windowDrawList;
				((SeStringDrawParams)(ref val2)).Font = UiBuilder.DefaultFont;
				((SeStringDrawParams)(ref val2)).FontSize = UiBuilder.DefaultFontSizePx;
				((SeStringDrawParams)(ref val2)).ScreenOffset = ImGui.GetCursorScreenPos();
				ImGuiHelpers.SeStringWrapped(readOnlySpan, ref val2, default(ImGuiId), (ImGuiButtonFlags)1);
				((ImDrawListPtr)(ref windowDrawList)).PopClipRect();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return result;
	}

	public static void DrawStyledLinkText(string text, string url, uint colorId, string tooltip = "")
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		string text2 = (text.Contains("##") ? text.Substring(text.IndexOf("##")) : text.Replace(" ", string.Empty));
		text = text.Replace(text2, string.Empty);
		string obj = $"<colortype({colorId})><edgecolortype({colorId})>{text}<colortype(0)><edgecolortype(0)>";
		SeStringDrawParams val = default(SeStringDrawParams);
		ImGuiHelpers.CompileSeStringWrapped(obj, ref val, ImGuiId.op_Implicit(text2), (ImGuiButtonFlags)1);
		if (!StringExtensions.IsNullOrWhitespace(tooltip))
		{
			SetItemTooltip(tooltip, (ImGuiHoveredFlags)0);
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetMouseCursor((ImGuiMouseCursor)7);
		}
		if (ImGui.IsItemClicked((ImGuiMouseButton)0))
		{
			try
			{
				Process.Start(new ProcessStartInfo(url)
				{
					UseShellExecute = true
				});
			}
			catch
			{
			}
		}
	}

	public static void DrawStyledText(string text, uint colorId, string tooltip = "", Action? action = null)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		string text2 = (text.Contains("##") ? text.Substring(text.IndexOf("##")) : text.Replace(" ", string.Empty));
		text = text.Replace(text2, string.Empty);
		string obj = $"<colortype({colorId})><edgecolortype({colorId})>{text}<colortype(0)><edgecolortype(0)>";
		SeStringDrawParams val = default(SeStringDrawParams);
		ImGuiHelpers.CompileSeStringWrapped(obj, ref val, ImGuiId.op_Implicit(text2), (ImGuiButtonFlags)1);
		if (!StringExtensions.IsNullOrWhitespace(tooltip))
		{
			SetItemTooltip(tooltip, (ImGuiHoveredFlags)0);
		}
		if (action != null)
		{
			if (ImGui.IsItemHovered())
			{
				ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)0))
			{
				action();
			}
		}
	}

	public static string EnumToString<T>(T value, string separator = "+") where T : Enum
	{
		if (Convert.ToUInt64(value) == 0L)
		{
			return "None";
		}
		List<string> list = new List<string>();
		foreach (Enum value2 in Enum.GetValues(typeof(T)))
		{
			ulong num = Convert.ToUInt64(value2);
			if (num != 0L && (Convert.ToUInt64(value) & num) == num)
			{
				list.Add(value2.ToString());
			}
		}
		if (list.Count <= 0)
		{
			return value.ToString();
		}
		return string.Join(separator, list);
	}

	public static string EnumToSelectedCountString<T>(T value, string noneText = "None", string allText = "All") where T : Enum
	{
		if (Convert.ToUInt64(value) == 0L)
		{
			return noneText;
		}
		int num = 0;
		Array values = Enum.GetValues(typeof(T));
		foreach (Enum item in values)
		{
			ulong num2 = Convert.ToUInt64(item);
			if (num2 != 0L && (Convert.ToUInt64(value) & num2) == num2)
			{
				num++;
			}
		}
		if (num != 0)
		{
			if (num != values.Length - 1 || string.IsNullOrWhiteSpace(allText))
			{
				return $"{num} Selected";
			}
			return allText;
		}
		return noneText;
	}

	public static TriState NextTriState(TriState current)
	{
		return current switch
		{
			TriState.Ignored => TriState.Allow, 
			TriState.Allow => TriState.Disallow, 
			TriState.Disallow => TriState.Ignored, 
			_ => TriState.Ignored, 
		};
	}

	public static bool Checkbox(string label, bool value, Action<bool> setter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.Checkbox(ImU8String.op_Implicit(label), ref value))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool InputText(string label, string value, Action<string> setter, int maxLength = 256)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.InputText(ImU8String.op_Implicit(label), ref value, maxLength, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool InputTextWithHint(string label, string hint, string value, Action<string> setter, int maxLength = 256)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.InputTextWithHint(ImU8String.op_Implicit(label), ImU8String.op_Implicit(hint), ref value, maxLength, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool InputInt(string label, int value, Action<int> setter, int step = 1, int stepFast = 100)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.InputInt(ImU8String.op_Implicit(label), ref value, step, stepFast, default(ImU8String), (ImGuiInputTextFlags)0))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool DragInt(string label, object obj, string nameofProp, float spd, int min, int max)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.DragInt(ImU8String.op_Implicit(label), ref num, spd, min, max, default(ImU8String), (ImGuiSliderFlags)0);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool DragInt(string label, Func<int> getter, Action<int> setter, float speed = 1f, int min = 0, int max = 0, string format = "", ImGuiSliderFlags flags = (ImGuiSliderFlags)0)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		int obj = getter();
		if (ImGui.DragInt(ImU8String.op_Implicit(label), ref obj, speed, min, max, default(ImU8String), (ImGuiSliderFlags)0))
		{
			setter(obj);
			return true;
		}
		return false;
	}

	public static bool DragUInt(string label, uint value, Action<uint> setter, float speed = 1f, uint min = 0u, uint max = 0u)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.DragUInt(ImU8String.op_Implicit(label), ref value, speed, min, max, default(ImU8String), (ImGuiSliderFlags)0))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool DragInt(string label, int value, Action<int> setter, float speed = 1f, int min = 0, int max = 0)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (max == 0)
		{
			max = int.MaxValue;
		}
		if (ImGui.DragInt(ImU8String.op_Implicit(label), ref value, speed, min, max, default(ImU8String), (ImGuiSliderFlags)0))
		{
			setter(value);
			return true;
		}
		return false;
	}

	public static bool DragFloat(string label, float value, Action<float> setter, float speed = 1f, float min = 0f, float max = 0f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (max == 0f)
		{
			max = float.MaxValue;
		}
		if (ImGui.DragFloat(ImU8String.op_Implicit(label), ref value, speed, min, max, default(ImU8String), (ImGuiSliderFlags)0))
		{
			setter(value);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsItemHovered(flags))
		{
			ImGui.SetTooltip(ImU8String.op_Implicit(s));
		}
	}

	public static void IconTextUnformatted(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextUnformatted(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		ImGui.PopFont();
	}

	public static void IconText(FontAwesomeIcon icon, Vector4 color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextColored(ref color, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		ImGui.PopFont();
	}

	public static void IconText(FontAwesomeIcon icon, ImGuiCol color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.TextColored(ref ((ImGuiStylePtr)(ref style)).Colors[(int)color], ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		ImGui.PopFont();
	}

	public static void IconWarningTooltip(string tooltip)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
		ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
		ImGui.PopFont();
		SetItemTooltip(tooltip, (ImGuiHoveredFlags)0);
	}

	public static void IconAlertTooltip(string tooltip)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		Vector4 dalamudRed = ImGuiColors.DalamudRed;
		ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
		ImGui.PopFont();
		SetItemTooltip(tooltip, (ImGuiHoveredFlags)0);
	}

	public static void IconWarningText(string text, bool wrapped = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
		ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
		ImGui.PopFont();
		ImGui.SameLine();
		if (wrapped)
		{
			dalamudYellow = ImGuiColors.DalamudYellow;
			ImGui.TextColoredWrapped(ref dalamudYellow, ImU8String.op_Implicit(text));
		}
		else
		{
			dalamudYellow = ImGuiColors.DalamudYellow;
			ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(text));
		}
	}

	public static void IconAlertText(string text, bool wrapped = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		Vector4 dalamudRed = ImGuiColors.DalamudRed;
		ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
		ImGui.PopFont();
		ImGui.SameLine();
		if (wrapped)
		{
			dalamudRed = ImGuiColors.DalamudRed;
			ImGui.TextColoredWrapped(ref dalamudRed, ImU8String.op_Implicit(text));
		}
		else
		{
			dalamudRed = ImGuiColors.DalamudRed;
			ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit(text));
		}
	}

	public static bool IconButton(FontAwesomeIcon icon, string id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(2, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(FontAwesomeExtensions.ToIconString(icon));
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		bool result = ImGui.Button(val, default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static float GetIconButtonWidth(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, -1f);
		ImGui.PopFont();
		float x = vector.X;
		ImGuiStylePtr style = ImGui.GetStyle();
		return x + ((ImGuiStylePtr)(ref style)).FramePadding.X * 4f;
	}

	public static bool IconSelectable(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Selectable(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, (ImGuiSelectableFlags)0, default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static void IconCheckbox(bool isChecked)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		if (isChecked)
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.TextColored(ref ((ImGuiStylePtr)(ref style)).Colors[18], ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61452)));
		}
		else
		{
			Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61452)), false, -1f);
			ImGui.Dummy(new Vector2(vector.X, vector.Y));
		}
		ImGui.PopFont();
	}

	public static void IconTriState(TriState state)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		switch (state)
		{
		case TriState.Allow:
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.TextColored(ref ((ImGuiStylePtr)(ref style)).Colors[18], ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61452)));
			break;
		}
		case TriState.Disallow:
		{
			Vector4 dalamudRed = ImGuiColors.DalamudRed;
			ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61453)));
			break;
		}
		default:
		{
			Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61452)), false, -1f);
			ImGui.Dummy(new Vector2(vector.X, vector.Y));
			break;
		}
		}
		ImGui.PopFont();
	}

	public static bool TreeNode(string text, Action? contextMenu = null, Vector4 col = default(Vector4), ImGuiTreeNodeFlags flags = (ImGuiTreeNodeFlags)0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		uint iD = ImGui.GetID(ImU8String.op_Implicit(text));
		ImGui.PushID((IntPtr)(int)iD);
		TreeOpenStates.TryGetValue(iD, out var value);
		Vector4 obj;
		if (value)
		{
			obj = ((col == default(Vector4)) ? ImGuiColors.DalamudViolet : col);
		}
		else if (!(col == default(Vector4)))
		{
			obj = col;
		}
		else
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			obj = ((ImGuiStylePtr)(ref style)).Colors[0];
		}
		Vector4 vector = obj;
		ImGui.PushStyleColor((ImGuiCol)0, vector);
		bool flag = ImGui.TreeNodeEx(ImU8String.op_Implicit(text), flags, default(ImU8String));
		TreeOpenStates[iD] = flag;
		ImGui.PopStyleColor();
		if (contextMenu != null && ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##treeContext"), (ImGuiPopupFlags)1))
		{
			contextMenu();
			ImGui.EndPopup();
		}
		ImGui.PopID();
		return flag;
	}

	public static bool HonorificGlowPicker(string label, string id, Vector3? color, int? gradientColorSet, GradientAnimationStyle? gradientAnimationStyle, Action<Vector3, int?, GradientAnimationStyle?> setter)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (!color.HasValue)
		{
			color = Vector3.One;
		}
		Vector4 vector = color.Value.AsVector4();
		vector.W = 1f;
		Vector4 vector2 = Vector4.One;
		if (gradientColorSet.HasValue)
		{
			GradientStyle style = GradientSystem.GetStyle(gradientColorSet.Value, gradientAnimationStyle);
			vector2 = ((style == null) ? Vector4.One : new Vector4(GradientSystem.GetColourVec3(style, 0, 3), 1f));
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		((ImU8String)(ref val)).AppendLiteral("Button");
		ImU8String val2 = val;
		Vector4 vector3 = ((vector2 != Vector4.One) ? vector2 : vector);
		if (ImGui.ColorButton(val2, ref vector3, (ImGuiColorEditFlags)32, default(Vector2)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(label))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(label));
		}
		bool flag = false;
		if (ImGui.BeginPopup(ImU8String.op_Implicit(id), (ImGuiWindowFlags)0))
		{
			flag |= HonorificGradientPicker(vector2.AsVector3(), ref gradientColorSet, ref gradientAnimationStyle);
			if (!gradientColorSet.HasValue)
			{
				ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
				bool num = flag;
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(2, 2);
				((ImU8String)(ref val3)).AppendFormatted<string>(label);
				((ImU8String)(ref val3)).AppendLiteral("##");
				((ImU8String)(ref val3)).AppendFormatted<string>(id);
				flag = num | ImGui.ColorPicker4(val3, ref vector, (ImGuiColorEditFlags)181404032);
			}
			ImGui.EndPopup();
		}
		if (flag)
		{
			setter(vector.AsVector3(), gradientColorSet, gradientAnimationStyle);
		}
		return flag;
	}

	public static bool HonorificGradientPicker(Vector3 curColor, ref int? gradientColorSet, ref GradientAnimationStyle? gradientAnimationStyle)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		bool r = false;
		float w = ImGui.CalcItemWidth();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(19, 0);
		((ImU8String)(ref val)).AppendLiteral("##rainbowModeSelect");
		if (ImGui.BeginCombo(val, ImU8String.op_Implicit((!gradientColorSet.HasValue) ? "Default Glow" : ""), (ImGuiComboFlags)16))
		{
			if (ImGui.Selectable(ImU8String.op_Implicit("Default Glow"), !gradientColorSet.HasValue, (ImGuiSelectableFlags)1, default(Vector2)))
			{
				ImGui.CloseCurrentPopup();
				gradientColorSet = null;
				gradientAnimationStyle = null;
				r = true;
			}
			if (ImGui.BeginTabBar(ImU8String.op_Implicit("gradientAnimations"), (ImGuiTabBarFlags)0))
			{
				if (ImGui.BeginTabItem(ImU8String.op_Implicit("Wave"), (ImGuiTabItemFlags)0))
				{
					DrawTab(curColor, ref gradientColorSet, ref gradientAnimationStyle, GradientAnimationStyle.Wave);
				}
				if (ImGui.BeginTabItem(ImU8String.op_Implicit("Pulse"), (ImGuiTabItemFlags)0))
				{
					DrawTab(curColor, ref gradientColorSet, ref gradientAnimationStyle, GradientAnimationStyle.Pulse);
				}
				if (ImGui.BeginTabItem(ImU8String.op_Implicit("Static"), (ImGuiTabItemFlags)0))
				{
					DrawTab(curColor, ref gradientColorSet, ref gradientAnimationStyle, GradientAnimationStyle.Static);
				}
				ImGui.EndTabBar();
			}
			ImGui.EndCombo();
		}
		if (gradientColorSet.HasValue)
		{
			GradientStyle style = GradientSystem.GetStyle(gradientColorSet.Value, gradientAnimationStyle);
			Counter obj = new Counter
			{
				TitleColour = curColor,
				TitleGradientAnimationStyle = gradientAnimationStyle,
				TitleGradientColorSet = gradientColorSet,
				TitleTemplate = (style?.Name ?? "Invalid Style")
			};
			Vector2 itemRectMin = ImGui.GetItemRectMin();
			ImGuiStylePtr style2 = ImGui.GetStyle();
			ImGui.SetCursorScreenPos(itemRectMin + ((ImGuiStylePtr)(ref style2)).FramePadding);
			ReadOnlySpan<byte> readOnlySpan = obj.ToSeString(includeQuotes: false).Encode();
			SeStringDrawParams val2 = default(SeStringDrawParams);
			((SeStringDrawParams)(ref val2)).Color = uint.MaxValue;
			((SeStringDrawParams)(ref val2)).WrapWidth = float.MaxValue;
			((SeStringDrawParams)(ref val2)).TargetDrawList = ImGui.GetWindowDrawList();
			((SeStringDrawParams)(ref val2)).Font = UiBuilder.DefaultFont;
			((SeStringDrawParams)(ref val2)).FontSize = UiBuilder.DefaultFontSizePx;
			((SeStringDrawParams)(ref val2)).ScreenOffset = ImGui.GetCursorScreenPos();
			ImGuiHelpers.SeStringWrapped(readOnlySpan, ref val2, default(ImGuiId), (ImGuiButtonFlags)1);
		}
		return r;
		void DrawTab(Vector3 titleColour, ref int? reference, ref GradientAnimationStyle? reference2, GradientAnimationStyle animationStyleTab)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			if (ImGui.BeginChild(ImU8String.op_Implicit("gradientPicker"), new Vector2(w), false, (ImGuiWindowFlags)0))
			{
				ImU8String val3 = default(ImU8String);
				for (int i = 0; i < GradientSystem.NumColourSets; i++)
				{
					GradientStyle style3 = GradientSystem.GetStyle(i, animationStyleTab);
					if (style3 != null && style3.AnimationStyle == animationStyleTab)
					{
						((ImU8String)(ref val3))._002Ector(14, 1);
						((ImU8String)(ref val3)).AppendLiteral("##rainbowMode_");
						((ImU8String)(ref val3)).AppendFormatted<int>(i);
						if (ImGui.Selectable(val3, reference == i && reference2 == animationStyleTab, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							ImGui.CloseCurrentPopup();
							reference = style3.ColourSet;
							reference2 = style3.AnimationStyle;
							r = true;
						}
						Vector2 itemRectMin2 = ImGui.GetItemRectMin();
						ImGuiStylePtr style4 = ImGui.GetStyle();
						ImGui.SetCursorScreenPos(itemRectMin2 + ((ImGuiStylePtr)(ref style4)).FramePadding);
						ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
						ReadOnlySpan<byte> readOnlySpan2 = new Counter
						{
							TitleColour = titleColour,
							TitleTemplate = style3.Name,
							TitleGradientColorSet = i,
							TitleGradientAnimationStyle = animationStyleTab
						}.ToSeString(includeQuotes: false).Encode();
						SeStringDrawParams val4 = default(SeStringDrawParams);
						((SeStringDrawParams)(ref val4)).Color = uint.MaxValue;
						((SeStringDrawParams)(ref val4)).WrapWidth = float.MaxValue;
						((SeStringDrawParams)(ref val4)).TargetDrawList = windowDrawList;
						((SeStringDrawParams)(ref val4)).Font = UiBuilder.DefaultFont;
						((SeStringDrawParams)(ref val4)).FontSize = UiBuilder.DefaultFontSizePx;
						((SeStringDrawParams)(ref val4)).ScreenOffset = ImGui.GetCursorScreenPos();
						ImGuiHelpers.SeStringWrapped(readOnlySpan2, ref val4, default(ImGuiId), (ImGuiButtonFlags)1);
						ImGui.NewLine();
					}
				}
			}
			ImGui.EndChild();
			ImGui.EndTabItem();
		}
	}

	public static bool ColorPicker3(string label, string id, Vector3? value, Action<Vector3> setter)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!value.HasValue)
		{
			value = new Vector3(255f, 255f, 255f);
		}
		Vector4 value2 = value.Value.AsVector4();
		value2.W = 1f;
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		((ImU8String)(ref val)).AppendLiteral("Button");
		if (ImGui.ColorButton(val, ref value2, (ImGuiColorEditFlags)32, default(Vector2)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(label))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(label));
		}
		bool flag = false;
		if (ImGui.BeginPopup(ImU8String.op_Implicit(id), (ImGuiWindowFlags)0))
		{
			ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(2, 2);
			((ImU8String)(ref val2)).AppendFormatted<string>(label);
			((ImU8String)(ref val2)).AppendLiteral("##");
			((ImU8String)(ref val2)).AppendFormatted<string>(id);
			flag = ImGui.ColorPicker4(val2, ref value2, (ImGuiColorEditFlags)181404032);
			ImGui.EndPopup();
		}
		if (flag)
		{
			setter(value2.AsVector3());
		}
		return flag;
	}

	public static bool DrawColorPicker(string label, Vector3 value, Action<Vector3> setter, Vector2 checkboxSize)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		ImGui.SetNextItemWidth(checkboxSize.X * 2f);
		bool flag;
		if (value == default(Vector3))
		{
			ImGui.PushStyleColor((ImGuiCol)7, uint.MaxValue);
			ImGui.PushStyleColor((ImGuiCol)9, uint.MaxValue);
			ImGui.PushStyleColor((ImGuiCol)8, uint.MaxValue);
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			flag = ImGui.BeginCombo(ImU8String.op_Implicit(label), ImU8String.op_Implicit(" "), (ImGuiComboFlags)16);
			((ImDrawListPtr)(ref windowDrawList)).AddLine(cursorScreenPos, cursorScreenPos + new Vector2(checkboxSize.X), 4278190335u, 3f * ImGuiHelpers.GlobalScale);
			ImGui.PopStyleColor(3);
		}
		else
		{
			ImGui.PushStyleColor((ImGuiCol)7, new Vector4(value, 1f));
			ImGui.PushStyleColor((ImGuiCol)9, new Vector4(value, 1f));
			ImGui.PushStyleColor((ImGuiCol)8, new Vector4(value, 1f));
			flag = ImGui.BeginCombo(ImU8String.op_Implicit(label), ImU8String.op_Implicit("  "), (ImGuiComboFlags)16);
			ImGui.PopStyleColor(3);
		}
		if (flag)
		{
			if (ImGui.IsWindowAppearing())
			{
				editingColour = value;
			}
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(16, 0);
			((ImU8String)(ref val)).AppendLiteral("##ColorPickClear");
			ImU8String val2 = val;
			Vector4 one = Vector4.One;
			if (ImGui.ColorButton(val2, ref one, (ImGuiColorEditFlags)64, default(Vector2)))
			{
				value = default(Vector3);
				result = true;
				setter(value);
				ImGui.CloseCurrentPopup();
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(ImU8String.op_Implicit("Clear selected colour"));
				ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			}
			ImDrawListPtr windowDrawList2 = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList2)).AddLine(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 4278190335u, 3f * ImGuiHelpers.GlobalScale);
			if (value != default(Vector3))
			{
				ImGui.SameLine();
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(15, 0);
				((ImU8String)(ref val3)).AppendLiteral("##ColorPick_old");
				ImU8String val4 = val3;
				one = new Vector4(value, 1f);
				if (ImGui.ColorButton(val4, ref one, (ImGuiColorEditFlags)64, default(Vector2)))
				{
					ImGui.CloseCurrentPopup();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(ImU8String.op_Implicit("Revert to previous selection"));
					ImGui.SetMouseCursor((ImGuiMouseCursor)7);
				}
			}
			ImGui.SameLine();
			ImU8String val5 = ImU8String.op_Implicit("Confirm");
			one = new Vector4(editingColour, 1f);
			if (ImGui.ColorButton(val5, ref one, (ImGuiColorEditFlags)64, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetItemRectSize().Y)))
			{
				value = editingColour;
				result = true;
				setter(value);
				ImGui.CloseCurrentPopup();
			}
			Vector2 itemRectSize = ImGui.GetItemRectSize();
			if (ImGui.IsItemHovered())
			{
				((ImDrawListPtr)(ref windowDrawList2)).AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 858993459u);
				ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			}
			Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit("Confirm"), false, -1f);
			((ImDrawListPtr)(ref windowDrawList2)).AddText(ImGui.GetItemRectMin() + itemRectSize / 2f - vector / 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(editingColour, 1f)) ^ 0xFFFFFF, ImU8String.op_Implicit("Confirm"));
			ImU8String val6 = default(ImU8String);
			((ImU8String)(ref val6))._002Ector(11, 0);
			((ImU8String)(ref val6)).AppendLiteral("##ColorPick");
			ImGui.ColorPicker3(val6, ref editingColour, (ImGuiColorEditFlags)272);
			ImGui.EndCombo();
		}
		return result;
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Text;
using PyonPix.Interop;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Ui;
using PyonPix.Ui;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Extensions;

public static class ImGuiEx
{
	private readonly struct IconTextSegment
	{
		public readonly string? Text;

		public readonly FontAwesomeIcon? Icon;

		public bool IsIcon => Icon.HasValue;

		public IconTextSegment(string text)
		{
			Text = text;
			Icon = null;
		}

		public IconTextSegment(FontAwesomeIcon icon)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Text = null;
			Icon = icon;
		}
	}

	private static readonly Dictionary<string, bool> WasUsingColorPicker4 = new Dictionary<string, bool>();

	private static readonly Dictionary<string, bool> WasUsingColorPicker3 = new Dictionary<string, bool>();

	private static readonly Dictionary<string, MouseLockState> DragLocked = new Dictionary<string, MouseLockState>();

	private static readonly Dictionary<string, InteractionState> DragFocused = new Dictionary<string, InteractionState>();

	private static readonly Dictionary<string, bool> WasUsingDrag = new Dictionary<string, bool>();

	private static readonly ConcurrentDictionary<string, InteractionState> StyledInputFocused = new ConcurrentDictionary<string, InteractionState>();

	private static readonly Dictionary<string, bool> WasUsingInput = new Dictionary<string, bool>();

	private static readonly Dictionary<uint, bool> ExpandedStates = new Dictionary<uint, bool>();

	public static UIState ColorPicker4(string labelId, ref Vector4 value, float? size = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		UIState result = UIState.None;
		string text = string.Empty;
		string text2 = labelId;
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit(labelId), true);
		try
		{
			if (labelId.Contains("##"))
			{
				string[] array = labelId.Split("##");
				text = array[0];
				text2 = array[1];
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		float valueOrDefault = size.GetValueOrDefault();
		if (!size.HasValue)
		{
			valueOrDefault = UIShared.NormalIconSize;
			size = valueOrDefault;
		}
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(2, 1);
		((ImU8String)(ref val2)).AppendLiteral("##");
		((ImU8String)(ref val2)).AppendFormatted<string>(text2);
		if (ImGui.ColorButton(val2, ref value, (ImGuiColorEditFlags)32, new Vector2(size.Value)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(text2), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(text));
		}
		if (ImGui.BeginPopup(ImU8String.op_Implicit(text2), (ImGuiWindowFlags)0))
		{
			ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
			bool num = ImGui.ColorPicker4(ImU8String.op_Implicit(labelId), ref value, (ImGuiColorEditFlags)181404032);
			bool flag = ImGui.IsItemActive();
			bool value2;
			bool flag2 = WasUsingColorPicker4.TryGetValue(text2, out value2) && value2;
			if (num)
			{
				WasUsingColorPicker4[text2] = true;
				result = UIState.Using;
			}
			if (!flag && flag2)
			{
				WasUsingColorPicker4[text2] = false;
				result = UIState.Ended;
			}
			ImGui.EndPopup();
		}
		return result;
	}

	public static UIState ColorPicker3(string labelId, ref Vector3 value, float? size = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		UIState result = UIState.None;
		string text = string.Empty;
		string text2 = labelId;
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit(labelId), true);
		try
		{
			if (labelId.Contains("##"))
			{
				string[] array = labelId.Split("##");
				text = array[0];
				text2 = array[1];
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		float valueOrDefault = size.GetValueOrDefault();
		if (!size.HasValue)
		{
			valueOrDefault = UIShared.NormalIconSize;
			size = valueOrDefault;
		}
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(2, 1);
		((ImU8String)(ref val2)).AppendLiteral("##");
		((ImU8String)(ref val2)).AppendFormatted<string>(text2);
		ImU8String val3 = val2;
		Vector4 vector = new Vector4(value, 1f);
		if (ImGui.ColorButton(val3, ref vector, (ImGuiColorEditFlags)32, new Vector2(size.Value)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(text2), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(text));
		}
		if (ImGui.BeginPopup(ImU8String.op_Implicit(text2), (ImGuiWindowFlags)0))
		{
			ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
			bool num = ImGui.ColorPicker3(ImU8String.op_Implicit(labelId), ref value, (ImGuiColorEditFlags)181404032);
			bool flag = ImGui.IsItemActive();
			bool value2;
			bool flag2 = WasUsingColorPicker3.TryGetValue(text2, out value2) && value2;
			if (num)
			{
				WasUsingColorPicker3[text2] = true;
				result = UIState.Using;
			}
			if (!flag && flag2)
			{
				WasUsingColorPicker3[text2] = false;
				result = UIState.Ended;
			}
			ImGui.EndPopup();
		}
		return result;
	}

	public static void Separator(float width, float? spacing = null)
	{
		float num = spacing ?? UIShared.SeparatorSpacing;
		Separator(width, num, num);
	}

	public static void Separator(float width, float topSpacing, float botSpacing)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		if (topSpacing != 0f)
		{
			ImGui.Dummy(new Vector2(0f, topSpacing));
		}
		((ImDrawListPtr)(ref windowDrawList)).AddLine(cursorScreenPos + new Vector2(0f, topSpacing), cursorScreenPos + new Vector2(width, topSpacing), ImGui.GetColorU32(UIShared.Separator));
		if (botSpacing != 0f)
		{
			ImGui.Dummy(new Vector2(0f, botSpacing));
		}
	}

	public static void SpacingY(float spacing)
	{
		ImGui.Dummy(new Vector2(0f, spacing));
	}

	public static void SpacingX(float spacing, bool sameLinePrior = false, bool sameLineAfter = false)
	{
		if (sameLinePrior)
		{
			ImGui.SameLine(0f, 0f);
		}
		ImGui.Dummy(new Vector2(spacing, 0f));
		if (sameLineAfter)
		{
			ImGui.SameLine(0f, 0f);
		}
	}

	public static UIState Drag<T>(string labelId, ref T value, float speed = 1f, T min = default(T), T max = default(T), int floatPrecision = 2, ImU8String format = default(ImU8String), bool disabled = false, float width = 0f, float? height = null, float barHeightPercent = 0.1f, bool insetLabel = true, string? tooltip = null, string? tooltipSub = null) where T : unmanaged
	{
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aff: Unknown result type (might be due to invalid IL or missing references)
		width = ((width == 0f) ? ImGui.GetContentRegionAvail().X : width);
		height = ((!height.HasValue) ? new float?(UIShared.LineHeight) : ((height == 0f) ? new float?(ImGui.GetFrameHeight()) : height));
		bool flag = typeof(T) == typeof(int);
		bool flag2 = typeof(T) == typeof(float);
		bool flag3 = typeof(T) == typeof(uint);
		bool flag4 = typeof(T) == typeof(short);
		float num = 1f;
		string text = "0";
		if (flag2)
		{
			floatPrecision = Math.Clamp(floatPrecision, 0, 6);
			num = MathF.Pow(10f, -floatPrecision);
			text = ((floatPrecision == 0) ? "0" : ("0." + new string('#', floatPrecision)));
		}
		float num2;
		float num3;
		if (flag)
		{
			Unsafe.As<T, int>(ref value);
			num2 = Unsafe.As<T, int>(ref min);
			num3 = Unsafe.As<T, int>(ref max);
		}
		else if (flag2)
		{
			Unsafe.As<T, float>(ref value);
			num2 = Unsafe.As<T, float>(ref min);
			num3 = Unsafe.As<T, float>(ref max);
		}
		else if (flag3)
		{
			Unsafe.As<T, uint>(ref value);
			num2 = Unsafe.As<T, uint>(ref min);
			num3 = Unsafe.As<T, uint>(ref max);
		}
		else if (flag4)
		{
			Unsafe.As<T, short>(ref value);
			num2 = Unsafe.As<T, short>(ref min);
			num3 = Unsafe.As<T, short>(ref max);
		}
		else
		{
			num2 = (num3 = 0f);
		}
		UIState result = UIState.None;
		string text2 = labelId;
		string text3 = labelId;
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit(labelId), true);
		try
		{
			if (labelId.Contains("##"))
			{
				string[] array = labelId.Split("##");
				text2 = array[0];
				text3 = array[1];
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		DragFocused.TryGetValue(text3, out var value2);
		DragLocked.TryGetValue(text3, out var value3);
		Vector4 vector = (disabled ? UIShared.InputBgDisabled : ((value2.IsActive || value2.IsInputActive) ? UIShared.InputBgActive : (value2.IsHovered ? UIShared.InputBgHovered : UIShared.InputBgNormal)));
		Vector4 vector2 = (disabled ? UIShared.InputTextDisabled : ((value2.IsActive || value2.IsInputActive) ? UIShared.InputTextActive : (value2.IsHovered ? UIShared.InputTextHovered : UIShared.InputTextNormal)));
		ImGui.BeginGroup();
		ImGui.SetNextItemWidth(width);
		bool flag5 = false;
		using (UIShared.NormalFont.Push())
		{
			ImGui.PushStyleVar((ImGuiStyleVar)11, 0f);
			ImGui.PushStyleColor((ImGuiCol)7, Vector4.Zero);
			ImGui.PushStyleColor((ImGuiCol)8, Vector4.Zero);
			ImGui.PushStyleColor((ImGuiCol)9, Vector4.Zero);
			ImGui.PushStyleColor((ImGuiCol)0, value2.IsInputActive ? vector2 : Vector4.Zero);
			ImGui.PushStyleColor((ImGuiCol)49, UIShared.InputBgTextSelected);
			if (disabled)
			{
				ImGui.BeginDisabled();
			}
			if (flag)
			{
				ref int reference = ref Unsafe.As<T, int>(ref value);
				flag5 = ImGui.DragInt(ImU8String.op_Implicit(insetLabel ? ("##" + text3) : labelId), ref reference, speed, Unsafe.As<T, int>(ref min), Unsafe.As<T, int>(ref max), format, (ImGuiSliderFlags)0);
			}
			else if (flag2)
			{
				ref float reference2 = ref Unsafe.As<T, float>(ref value);
				flag5 = ImGui.DragFloat(ImU8String.op_Implicit(insetLabel ? ("##" + text3) : labelId), ref reference2, speed, Unsafe.As<T, float>(ref min), Unsafe.As<T, float>(ref max), format, (ImGuiSliderFlags)0);
				if (flag5)
				{
					reference2 = MathF.Round(reference2 / num) * num;
					if (num3 > num2)
					{
						reference2 = Math.Clamp(reference2, num2, num3);
					}
					if (!value3.IsLocked)
					{
						value3.StoredDelta = 0f;
					}
				}
			}
			else if (flag3)
			{
				ref uint reference3 = ref Unsafe.As<T, uint>(ref value);
				flag5 = ImGui.DragUInt(ImU8String.op_Implicit(insetLabel ? ("##" + text3) : labelId), ref reference3, speed, Unsafe.As<T, uint>(ref min), Unsafe.As<T, uint>(ref max), format, (ImGuiSliderFlags)0);
			}
			else if (flag4)
			{
				ref short reference4 = ref Unsafe.As<T, short>(ref value);
				flag5 = ImGui.DragShort(ImU8String.op_Implicit(insetLabel ? ("##" + text3) : labelId), ref reference4, speed, Unsafe.As<T, short>(ref min), Unsafe.As<T, short>(ref max), format, (ImGuiSliderFlags)0);
			}
			if (disabled)
			{
				ImGui.EndDisabled();
			}
			ImGui.PopStyleColor(5);
			ImGui.PopStyleVar();
		}
		bool flag6 = ImGui.IsItemHovered();
		bool flag7 = ImGui.IsItemActive();
		bool value4;
		bool flag8 = WasUsingDrag.TryGetValue(text3, out value4) && value4;
		bool flag9 = flag6 && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0);
		bool flag10 = ImGui.IsItemDeactivated();
		bool num4 = flag6 && ImGui.IsMouseDown((ImGuiMouseButton)0);
		bool flag11 = ImGui.IsMouseReleased((ImGuiMouseButton)0);
		bool flag12 = !ImGui.IsWindowFocused((ImGuiFocusedFlags)3);
		if (num4 && !value3.IsLocked && !value2.IsInputActive && Win32Interop.BeginDrag(out var xPos, out var yPos))
		{
			value3.X = xPos;
			value3.Y = yPos;
			value3.IsLocked = true;
			value3.StoredDelta = 0f;
		}
		if (value3.IsLocked)
		{
			int xPos2;
			int yPos2;
			if (flag11 || flag12 || ImGui.IsKeyPressed((ImGuiKey)526))
			{
				Win32Interop.EndDrag();
				value3.IsLocked = false;
			}
			else if (Win32Interop.GetDragCursorPos(out xPos2, out yPos2))
			{
				int num5 = xPos2 - value3.X;
				if (flag2)
				{
					value3.StoredDelta += (float)num5 * speed;
					if (MathF.Abs(value3.StoredDelta) >= num)
					{
						int num6 = (int)MathF.Truncate(value3.StoredDelta / num);
						float num7 = (float)num6 * num;
						ref float reference5 = ref Unsafe.As<T, float>(ref value);
						reference5 += num7;
						if (num3 > num2)
						{
							reference5 = Math.Clamp(reference5, num2, num3);
						}
						reference5 = MathF.Round(reference5 / num) * num;
						value3.StoredDelta -= (float)num6 * num;
						flag5 = true;
					}
				}
				else
				{
					value3.StoredDelta += (float)num5 * speed;
					if (MathF.Abs(value3.StoredDelta) >= 1f)
					{
						int num8 = (int)MathF.Truncate(value3.StoredDelta);
						value3.StoredDelta -= num8;
						if (flag)
						{
							ref int reference6 = ref Unsafe.As<T, int>(ref value);
							int num9 = reference6 + num8;
							num9 = ((num3 > num2) ? Math.Clamp(num9, (int)num2, (int)num3) : Math.Max(num9, (int)num2));
							reference6 = num9;
						}
						else if (flag3)
						{
							ref uint reference7 = ref Unsafe.As<T, uint>(ref value);
							uint num10 = reference7 + (uint)num8;
							num10 = ((num3 > num2) ? Math.Clamp(num10, (uint)num2, (uint)num3) : Math.Max(num10, (uint)num2));
							reference7 = num10;
						}
						else if (flag4)
						{
							ref short reference8 = ref Unsafe.As<T, short>(ref value);
							short num11 = (short)(reference8 + num8);
							num11 = ((num3 > num2) ? Math.Clamp(num11, (short)num2, (short)num3) : Math.Max(num11, (short)num2));
							reference8 = num11;
						}
						flag5 = true;
					}
				}
				Win32Interop.SetDragCursorPos(value3.X, value3.Y);
			}
		}
		else
		{
			if (tooltip != null)
			{
				Tooltip.Show(tooltip, tooltipSub);
			}
			if (flag6 && !value2.IsInputActive)
			{
				ImGui.SetMouseCursor((ImGuiMouseCursor)4);
			}
		}
		if (flag5)
		{
			WasUsingDrag[text3] = true;
			result = UIState.Using;
		}
		if (!flag7 && flag8 && !value3.IsLocked)
		{
			WasUsingDrag[text3] = false;
			result = UIState.Ended;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		float num12 = itemRectMax.Y - itemRectMin.Y;
		float num13 = ((height > 0f) ? height.Value : num12);
		ImGuiStylePtr style;
		float num14;
		if (!insetLabel && !string.IsNullOrEmpty(text2))
		{
			float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f).X;
			style = ImGui.GetStyle();
			num14 = x + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		}
		else
		{
			num14 = 0f;
		}
		float num15 = num14;
		Vector2 vector3 = new Vector2(itemRectMin.X, itemRectMin.Y);
		Vector2 vector4 = new Vector2(itemRectMax.X - num15, itemRectMin.Y + num13);
		float num16 = 6f * ImGuiHelpers.GlobalScale;
		float num17 = num13 * barHeightPercent;
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector3, vector4, ImGui.GetColorU32(vector), UIShared.InputRounding, (ImDrawFlags)48);
		Vector2 vector5 = new Vector2(vector3.X, vector4.Y - num17);
		Vector2 vector6 = vector4;
		if (Math.Abs(num3 - num2) > float.Epsilon)
		{
			float num18 = Math.Clamp(((flag ? ((float)Unsafe.As<T, int>(ref value)) : (flag3 ? ((float)Unsafe.As<T, uint>(ref value)) : (flag4 ? ((float)Unsafe.As<T, short>(ref value)) : Unsafe.As<T, float>(ref value)))) - num2) / (num3 - num2), 0.01f, 1f);
			Vector2 vector7 = new Vector2(vector5.X + (vector6.X - vector5.X) * num18, vector6.Y);
			Vector4 vector8 = (disabled ? UIShared.DragFgDisabled : ((value2.IsActive || value2.IsInputActive) ? UIShared.DragFgActive : (value2.IsHovered ? UIShared.DragFgHovered : UIShared.DragFgNormal)));
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector5, vector6, ImGui.GetColorU32(vector));
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector5, vector7, ImGui.GetColorU32(vector8));
		}
		if (!value2.IsInputActive)
		{
			using (UIShared.NormalFont.Push())
			{
				string text4;
				if (flag2)
				{
					float num19 = Unsafe.As<T, float>(ref value);
					text4 = num19.ToString(text);
				}
				else
				{
					text4 = (flag ? Unsafe.As<T, int>(ref value).ToString() : (flag3 ? Unsafe.As<T, uint>(ref value).ToString() : Unsafe.As<T, short>(ref value).ToString()));
				}
				Vector2 size = ImGui.CalcTextSize(ImU8String.op_Implicit(text4), false, -1f);
				Vector2 vector9 = UiUtil.AlignCenter(vector3, new Vector2(vector4.X, vector4.Y), size);
				if (insetLabel)
				{
					Vector2 vector10 = new Vector2(vector3.X + num16, vector9.Y);
					((ImDrawListPtr)(ref windowDrawList)).AddText(vector10, ImGui.GetColorU32(vector2), ImU8String.op_Implicit(text2));
					Vector2 vector11 = new Vector2(vector4.X - size.X - num16, vector9.Y);
					((ImDrawListPtr)(ref windowDrawList)).AddText(vector11, ImGui.GetColorU32(vector2), ImU8String.op_Implicit(text4));
				}
				else
				{
					((ImDrawListPtr)(ref windowDrawList)).AddText(vector9, ImGui.GetColorU32(vector2), ImU8String.op_Implicit(text4));
				}
			}
			if (!insetLabel && !string.IsNullOrEmpty(text2))
			{
				using (UIShared.NormalFont.Push())
				{
					ImU8String val2 = new ImU8String(0, 1);
					((ImU8String)(ref val2)).AppendFormatted<string>(text2);
					Vector2 size2 = ImGui.CalcTextSize(val2, false, -1f);
					Vector2 vector12 = UiUtil.AlignCenter(vector3, vector4, size2);
					float x2 = vector4.X;
					style = ImGui.GetStyle();
					vector12 = new Vector2(x2 + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X, vector12.Y);
					((ImDrawListPtr)(ref windowDrawList)).AddText(vector12, ImGui.GetColorU32(vector2), ImU8String.op_Implicit(text2));
				}
			}
		}
		float num20 = num13 - num12;
		if (num20 > 0f)
		{
			ImGui.Dummy(new Vector2(0f, num20));
		}
		ImGui.EndGroup();
		DragFocused[text3] = new InteractionState
		{
			IsHovered = flag6,
			IsActive = flag7,
			IsInputActive = (flag9 || (!flag10 && value2.IsInputActive)),
			IsDragging = value3.IsLocked
		};
		DragLocked[text3] = value3;
		return result;
	}

	public unsafe static UIState StyledInput(ImU8String label, ref string text, string hint = "", bool disabled = false, int maxLength = 512, float width = 0f, ImGuiInputTextFlags flags = (ImGuiInputTextFlags)16, string? tooltip = null, string? tooltipSub = null, Action? onEnter = null, FontAwesomeIcon buttonIcon = (FontAwesomeIcon)0, Action? onButtonClick = null, FontAwesomeIcon labelIcon = (FontAwesomeIcon)0, string[]? autoCompleteList = null, int autoCompleteMaxItemsDisplayed = 3, Action<string>? onAutoCompleteSelection = null)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Invalid comparison between Unknown and I4
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		width = ((width == 0f) ? ImGui.GetContentRegionAvail().X : width);
		string text2 = ((object)(*(ImU8String*)(&label))/*cast due to constrained. prefix*/).ToString();
		ImGui.PushID(label);
		StyledInputFocused.TryGetValue(text2, out var value);
		Vector4 vector = (disabled ? UIShared.InputBgDisabled : (value.IsActive ? UIShared.InputBgActive : (value.IsHovered ? UIShared.InputBgHovered : UIShared.InputBgNormal)));
		Vector4 vector2 = (disabled ? UIShared.InputTextDisabled : (value.IsActive ? UIShared.InputTextActive : (value.IsHovered ? UIShared.InputTextHovered : UIShared.InputTextNormal)));
		Vector2 vector3 = UIShared.InputPadding * ImGuiHelpers.GlobalScale;
		bool flag = (int)buttonIcon > 0;
		bool flag2 = (int)labelIcon > 0;
		Action action = onEnter;
		Action action2 = onButtonClick;
		if (flag)
		{
			if (action == null && action2 != null)
			{
				action = action2;
			}
			else if (action2 == null && action != null)
			{
				action2 = action;
			}
		}
		ImGui.PushStyleVar((ImGuiStyleVar)11, UIShared.InputRounding);
		ImGui.PushStyleVar((ImGuiStyleVar)10, vector3);
		ImGui.PushStyleColor((ImGuiCol)7, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)0, vector2);
		ImGui.PushStyleColor((ImGuiCol)1, UIShared.InputTextHint);
		ImGui.PushStyleColor((ImGuiCol)49, UIShared.InputBgTextSelected);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float frameHeight = ImGui.GetFrameHeight();
		float x = (flag2 ? frameHeight : 0f);
		float num = (flag2 ? (frameHeight * 0.8f) : 0f);
		float num2 = (flag ? frameHeight : 0f);
		float nextItemWidth = MathF.Max(1f, width - num - num2);
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, cursorScreenPos + new Vector2(width, frameHeight), ImGui.GetColorU32(vector), UIShared.InputRounding);
		if (flag2)
		{
			Vector4 vector4 = (disabled ? UIShared.IconDisabled : UIShared.IconNormal);
			using (UIShared.NormalIconFont.Push())
			{
				string text3 = FontAwesomeExtensions.ToIconString(labelIcon);
				Vector2 vector5 = ImGui.CalcTextSize(ImU8String.op_Implicit(text3), false, -1f);
				Vector2 vector6 = cursorScreenPos + new Vector2(x, frameHeight);
				Vector2 vector7 = (cursorScreenPos + vector6) * 0.5f - vector5 * 0.5f;
				((ImDrawListPtr)(ref windowDrawList)).AddText(vector7, ImGui.GetColorU32(vector4), ImU8String.op_Implicit(text3));
			}
		}
		if (disabled)
		{
			ImGui.BeginDisabled();
		}
		ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(num, 0f));
		ImGui.SetNextItemWidth(nextItemWidth);
		bool flag3 = ImGui.InputTextWithHint(label, ImU8String.op_Implicit(hint), ref text, maxLength, flags, (ImGuiInputTextCallbackDelegate)null);
		bool flag4 = ImGui.IsItemFocused();
		ImGui.IsItemHovered();
		bool flag5 = ImGui.IsItemActive();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		bool flag6 = false;
		bool flag7 = false;
		if (flag)
		{
			ImGui.SameLine(0f, 0f);
			Vector2 cursorScreenPos2 = ImGui.GetCursorScreenPos();
			Vector2 vector8 = cursorScreenPos2 + new Vector2(num2, frameHeight);
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(6, 1);
			((ImU8String)(ref val)).AppendFormatted<string>(text2);
			((ImU8String)(ref val)).AppendLiteral("button");
			bool flag8 = ImGui.InvisibleButton(val, new Vector2(num2, frameHeight), (ImGuiButtonFlags)0);
			flag6 = ImGui.IsItemHovered();
			flag7 = ImGui.IsItemActive();
			Vector4 vector9 = (disabled ? UIShared.InputBgDisabled : (flag7 ? UIShared.InputBgActive : (flag6 ? UIShared.InputBgHovered : vector)));
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos2, vector8, ImGui.GetColorU32(vector9), UIShared.InputRounding);
			Vector4 vector10 = (disabled ? UIShared.IconDisabled : (flag7 ? UIShared.IconActive : (flag6 ? UIShared.IconHovered : UIShared.IconNormal)));
			using (UIShared.NormalIconFont.Push())
			{
				string text4 = FontAwesomeExtensions.ToIconString(buttonIcon);
				Vector2 vector11 = ImGui.CalcTextSize(ImU8String.op_Implicit(text4), false, -1f);
				Vector2 vector12 = (cursorScreenPos2 + vector8) * 0.5f - vector11 * 0.5f;
				((ImDrawListPtr)(ref windowDrawList)).AddText(vector12, ImGui.GetColorU32(vector10), ImU8String.op_Implicit(text4));
			}
			if (flag8 && !disabled)
			{
				action2?.Invoke();
			}
		}
		bool flag9 = ImGui.IsMouseHoveringRect(cursorScreenPos, cursorScreenPos + new Vector2(width, frameHeight));
		if (tooltip != null && flag9)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		UIState result = UIState.None;
		bool value2;
		bool flag10 = WasUsingInput.TryGetValue(text2, out value2) && value2;
		if (flag3)
		{
			WasUsingInput[text2] = true;
			result = UIState.Using;
		}
		if (!flag5 && flag10)
		{
			WasUsingInput[text2] = false;
			result = UIState.Ended;
		}
		if (action != null && flag4 && ImGui.IsKeyPressed((ImGuiKey)525))
		{
			action();
		}
		if (disabled)
		{
			ImGui.EndDisabled();
		}
		if (value.IsActive && autoCompleteList != null && autoCompleteList.Length != 0)
		{
			float x2 = width;
			float y = MathF.Min(autoCompleteList.Length, autoCompleteMaxItemsDisplayed) * (itemRectMax.Y - itemRectMin.Y);
			Vector2 vector13 = new Vector2(itemRectMin.X, itemRectMax.Y);
			Vector2 vector14 = vector13 + new Vector2(x2, y);
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector13, vector14, ImGui.GetColorU32(UIShared.ContextMenuBg), UIShared.InputRounding);
			for (int i = 0; i < autoCompleteList.Length && i < autoCompleteMaxItemsDisplayed; i++)
			{
				string text5 = autoCompleteList[i];
				Vector2 vector15 = vector13 + new Vector2(0f, (float)i * (itemRectMax.Y - itemRectMin.Y));
				Vector2 vector16 = vector15 + new Vector2(x2, itemRectMax.Y - itemRectMin.Y);
				bool flag11 = UiUtil.IsRectHovered(vector15, vector16);
				if (flag11)
				{
					flag5 = true;
					((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(vector15, vector16, ImGui.GetColorU32(UIShared.ContextItemBgHovered), UIShared.InputRounding);
				}
				if (UiUtil.IsRectClicked(vector15, vector16, (ImGuiMouseButton)0))
				{
					onAutoCompleteSelection?.Invoke(text5);
				}
				using (UIShared.SubFont.Push())
				{
					Vector4 vector17 = (flag11 ? UIShared.ContextItemTextHovered : UIShared.ContextItemTextNormal);
					((ImDrawListPtr)(ref windowDrawList)).AddText(new Vector2(vector15.X + vector3.X, vector15.Y + (vector16.Y - vector15.Y - ImGui.GetFontSize()) * 0.5f), ImGui.GetColorU32(vector17), ImU8String.op_Implicit(text5));
				}
			}
		}
		InteractionState value3 = new InteractionState
		{
			IsHovered = (flag9 || flag6),
			IsActive = (flag5 || flag7)
		};
		StyledInputFocused[text2] = value3;
		ImGui.PopStyleColor(4);
		ImGui.PopStyleVar(2);
		ImGui.PopID();
		return result;
	}

	private static Vector2 DrawComboButton(string id, string text, string valueText, float width, float height, bool disabled, out bool isHovered, out bool isActive, out bool isClicked, out bool isOpen)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(0, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(text);
		((ImU8String)(ref val)).AppendFormatted<string>(valueText);
		isClicked = ImGui.InvisibleButton(val, new Vector2(width, height), (ImGuiButtonFlags)0);
		isHovered = ImGui.IsItemHovered();
		isActive = ImGui.IsItemActive();
		isOpen = ImGui.IsPopupOpen(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector4 vector = (disabled ? UIShared.InputBgDisabled : ((isActive | isOpen) ? UIShared.InputBgActive : (isHovered ? UIShared.InputBgHovered : UIShared.InputBgNormal)));
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(itemRectMin, itemRectMin + new Vector2(width, height), ImGui.GetColorU32(vector), UIShared.InputRounding);
		float num = 6f * ImGuiHelpers.GlobalScale;
		Vector2 vector4;
		Vector2 vector5;
		using (UIShared.NormalIconFont.Push())
		{
			FontAwesomeIcon val2 = (FontAwesomeIcon)(isOpen ? 61655 : 61658);
			Vector4 vector2 = (disabled ? UIShared.IconDisabled : ((isActive | isOpen) ? UIShared.IconActive : (isHovered ? UIShared.IconHovered : UIShared.IconNormal)));
			Vector2 vector3 = (itemRectMin + ImGui.GetItemRectMax()) * 0.5f;
			vector4 = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61655)), false, -1f);
			vector5 = new Vector2(itemRectMin.X + num, vector3.Y - vector4.Y * 0.5f);
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector5, ImGui.GetColorU32(vector2), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(val2)));
		}
		using (UIShared.NormalFont.Push())
		{
			Vector4 vector6 = (disabled ? UIShared.IconLabelDisabled : (isActive ? UIShared.IconLabelActive : (isHovered ? UIShared.IconLabelHovered : UIShared.IconLabelNormal)));
			Vector2 vector7 = vector5 + new Vector2(vector4.X + num, 0f);
			uint colorU = ImGui.GetColorU32(vector6);
			ImU8String val3 = new ImU8String(0, 2);
			((ImU8String)(ref val3)).AppendFormatted<string>(text);
			((ImU8String)(ref val3)).AppendFormatted<string>(valueText);
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector7, colorU, val3);
			return itemRectMin;
		}
	}

	private static bool DrawComboPopup<TItem>(string id, Vector2 anchorPos, float width, float itemHeight, IReadOnlyList<TItem> items, int maxItemsDisplayed, bool closeOnSelection, Func<TItem, string> labelOf, Func<TItem, bool> isSelected, Func<TItem, bool> onClick, Action? drawHeader, Func<TItem, FontAwesomeIcon?>? prefixIcon = null)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Invalid comparison between Unknown and I4
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || items.Count == 0)
		{
			return false;
		}
		int count = items.Count;
		int num = Math.Min(count, Math.Max(1, maxItemsDisplayed));
		float num2 = ((drawHeader != null) ? itemHeight : 0f);
		float num3 = num2 + itemHeight * (float)num;
		ImGui.SetNextWindowPos(anchorPos, (ImGuiCond)8);
		ImGui.SetNextWindowSize(new Vector2(width, num3), (ImGuiCond)8);
		ImGui.PushStyleVar((ImGuiStyleVar)13, Vector2.Zero);
		ImGui.PushStyleVar((ImGuiStyleVar)8, UIShared.InputRounding);
		ImGui.PushStyleColor((ImGuiCol)4, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)3, Vector4.Zero);
		bool flag = false;
		if (ImGui.BeginPopup(ImU8String.op_Implicit(id), (ImGuiWindowFlags)263))
		{
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(anchorPos, anchorPos + new Vector2(width, num3), ImGui.GetColorU32(UIShared.ContextMenuBg), UIShared.InputRounding);
			((ImDrawListPtr)(ref windowDrawList)).AddRect(anchorPos, anchorPos + new Vector2(width, num3), ImGui.GetColorU32(UIShared.ContextMenuBorder), UIShared.InputRounding);
			if (num2 > 0f)
			{
				ImU8String val = default(ImU8String);
				((ImU8String)(ref val))._002Ector(6, 1);
				((ImU8String)(ref val)).AppendFormatted<string>(id);
				((ImU8String)(ref val)).AppendLiteral("header");
				ImGui.BeginChild(val, new Vector2(width, num2), false, (ImGuiWindowFlags)24);
				drawHeader?.Invoke();
				ImGui.EndChild();
			}
			float y = num3 - num2;
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(7, 1);
			((ImU8String)(ref val2)).AppendFormatted<string>(id);
			((ImU8String)(ref val2)).AppendLiteral("content");
			ImGui.BeginChild(val2, new Vector2(width, y), false, (ImGuiWindowFlags)0);
			Vector2 windowPos = ImGui.GetWindowPos();
			Vector2 vector = windowPos + ImGui.GetWindowSize();
			((ImDrawListPtr)(ref windowDrawList)).PushClipRect(windowPos, vector, true);
			ImU8String val3 = default(ImU8String);
			for (int i = 0; i < count; i++)
			{
				TItem arg = items[i];
				((ImU8String)(ref val3))._002Ector(5, 2);
				((ImU8String)(ref val3)).AppendFormatted<string>(id);
				((ImU8String)(ref val3)).AppendFormatted<int>(i);
				((ImU8String)(ref val3)).AppendLiteral("dummy");
				ImGui.InvisibleButton(val3, new Vector2(width, itemHeight), (ImGuiButtonFlags)0);
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				Vector2 itemRectMax = ImGui.GetItemRectMax();
				bool flag2 = ImGui.IsItemHovered();
				bool flag3 = isSelected(arg);
				if (flag2 || flag3)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(itemRectMin, itemRectMax, ImGui.GetColorU32(flag2 ? UIShared.ContextItemBgHovered : UIShared.ContextItemBgActive), UIShared.InputRounding);
				}
				if (ImGui.IsItemClicked((ImGuiMouseButton)0))
				{
					bool flag4 = onClick(arg);
					flag = flag || flag4;
					if (closeOnSelection)
					{
						ImGui.CloseCurrentPopup();
					}
				}
				float num4 = itemRectMin.X + UIShared.ComboItemPadding;
				if (prefixIcon != null)
				{
					FontAwesomeIcon? val4 = prefixIcon(arg);
					if (val4.HasValue)
					{
						using (UIShared.SubIconFont.Push())
						{
							string text = FontAwesomeExtensions.ToIconString(val4.Value);
							Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
							if ((int)val4.GetValueOrDefault() != 61640)
							{
								Vector4 vector3 = (flag3 ? UIShared.ContextItemTextActive : (flag2 ? UIShared.ContextItemTextHovered : UIShared.ContextItemTextNormal));
								((ImDrawListPtr)(ref windowDrawList)).AddText(new Vector2(itemRectMin.X + UIShared.ComboItemPadding, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - vector2.Y) * 0.5f), ImGui.GetColorU32(vector3), ImU8String.op_Implicit(text));
							}
							num4 += vector2.X + UIShared.ComboItemPadding;
						}
					}
				}
				using (UIShared.SubFont.Push())
				{
					Vector4 vector4 = (flag2 ? UIShared.ContextItemTextHovered : (flag3 ? UIShared.ContextItemTextActive : UIShared.ContextItemTextNormal));
					((ImDrawListPtr)(ref windowDrawList)).AddText(new Vector2(num4, itemRectMin.Y + (itemRectMax.Y - itemRectMin.Y - ImGui.GetFontSize()) * 0.5f), ImGui.GetColorU32(vector4), ImU8String.op_Implicit(labelOf(arg)));
				}
			}
			((ImDrawListPtr)(ref windowDrawList)).PopClipRect();
			ImGui.EndChild();
			ImGui.EndPopup();
		}
		ImGui.PopStyleColor(2);
		ImGui.PopStyleVar(2);
		return flag;
	}

	private static string GetComboValueText(string valueText, ComboButtonDisplayType displayType)
	{
		switch (displayType)
		{
		case ComboButtonDisplayType.Label:
			return string.Empty;
		case ComboButtonDisplayType.SelectionCount:
		{
			int value = valueText.Split(',').Length;
			return $"{value} Selected";
		}
		default:
			return valueText;
		}
	}

	public static bool EnumCombo<T>(string id, string text, ref T value, ComboButtonDisplayType displayType = ComboButtonDisplayType.Label, bool disabled = false, string? tooltip = null, string? tooltipSub = null, int maxItemsDisplayed = 6, float width = 0f, float? height = null, Action? drawHeader = null, T? ignoredValue = null) where T : struct, Enum
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		width = ((width == 0f) ? ImGui.GetContentRegionAvail().X : width);
		height = ((!height.HasValue) ? new float?(UIShared.LineHeight) : ((height == 0f) ? new float?(ImGui.GetFrameHeight()) : height));
		ImGui.PushID(ImU8String.op_Implicit(id));
		string comboValueText = GetComboValueText(value.ToString(), displayType);
		bool isHovered;
		bool isActive;
		bool isClicked;
		bool isOpen;
		Vector2 vector = DrawComboButton(id, text, comboValueText, width, height.Value, disabled, out isHovered, out isActive, out isClicked, out isOpen);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		if (isClicked && !disabled)
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!isOpen)
		{
			ImGui.PopID();
			return false;
		}
		T[] array = ((!ignoredValue.HasValue) ? Enum.GetValues<T>() : (from x in Enum.GetValues<T>()
			where !EqualityComparer<T>.Default.Equals(x, ignoredValue.Value)
			select x).ToArray());
		if (array.Length == 0)
		{
			ImGui.PopID();
			return false;
		}
		T localValue = value;
		bool num = DrawComboPopup(id, new Vector2(vector.X, vector.Y + height.Value), width, height.Value, array, maxItemsDisplayed, closeOnSelection: true, (T item) => item.ToString(), (T item) => EqualityComparer<T>.Default.Equals(localValue, item), delegate(T item)
		{
			if (!EqualityComparer<T>.Default.Equals(localValue, item))
			{
				localValue = item;
				return true;
			}
			return false;
		}, drawHeader);
		if (num)
		{
			value = localValue;
		}
		ImGui.PopID();
		return num;
	}

	public static bool EnumFlagsCombo<T>(string id, string text, ref T value, ComboButtonDisplayType displayType = ComboButtonDisplayType.Label, bool disabled = false, string? tooltip = null, string? tooltipSub = null, int maxItemsDisplayed = 6, float width = 0f, float? height = null, Action? drawHeader = null) where T : struct, Enum
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		width = ((width == 0f) ? ImGui.GetContentRegionAvail().X : width);
		height = ((!height.HasValue) ? new float?(UIShared.LineHeight) : ((height == 0f) ? new float?(ImGui.GetFrameHeight()) : height));
		ImGui.PushID(ImU8String.op_Implicit(id));
		string comboValueText = GetComboValueText(value.ToString(), displayType);
		bool isHovered;
		bool isActive;
		bool isClicked;
		bool isOpen;
		Vector2 vector = DrawComboButton(id, text, comboValueText, width, height.Value, disabled, out isHovered, out isActive, out isClicked, out isOpen);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		if (isClicked && !disabled)
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!isOpen)
		{
			ImGui.PopID();
			return false;
		}
		T[] values = Enum.GetValues<T>();
		if (values.Length == 0)
		{
			ImGui.PopID();
			return false;
		}
		T localValue = value;
		ulong curBits = Convert.ToUInt64(localValue);
		(T, ulong)[] items = (from v in values
			select (Value: v, Bits: Convert.ToUInt64(v)) into x
			where x.Bits == 0L || (x.Bits & (x.Bits - 1)) == 0
			select x).ToArray();
		Func<(T, ulong), string> labelOf = ((T Value, ulong Bits) item) => item.Value.ToString();
		Func<(T, ulong), bool> isSelected = ((T Value, ulong Bits) item) => (item.Bits == 0L) ? (curBits == 0) : ((curBits & item.Bits) == item.Bits);
		Func<(T, ulong), FontAwesomeIcon?> prefixIcon = ((T Value, ulong Bits) item) => (FontAwesomeIcon)(((item.Bits == 0L) ? (curBits == 0) : ((curBits & item.Bits) == item.Bits)) ? 61770 : 61640);
		bool num = DrawComboPopup<(T, ulong)>(id, new Vector2(vector.X, vector.Y + height.Value), width, height.Value, items, maxItemsDisplayed, closeOnSelection: false, labelOf, isSelected, delegate((T Value, ulong Bits) item)
		{
			ulong num2 = ((item.Bits == 0L) ? 0 : (curBits ^ item.Bits));
			T val = (T)Enum.ToObject(typeof(T), num2);
			if (!EqualityComparer<T>.Default.Equals(localValue, val))
			{
				localValue = val;
				curBits = num2;
				return true;
			}
			return false;
		}, drawHeader, prefixIcon);
		if (num)
		{
			value = localValue;
		}
		ImGui.PopID();
		return num;
	}

	public static bool ListCombo<TKey>(string id, string text, string hintText, ref TKey selectedId, IEnumerable<(TKey id, string label)> items, ComboButtonDisplayType displayType = ComboButtonDisplayType.Items, bool disabled = false, string? tooltip = null, string? tooltipSub = null, int maxItemsDisplayed = 6, float width = 0f, float? height = null, Action? drawHeader = null) where TKey : struct, IEquatable<TKey>
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		width = ((width == 0f) ? ImGui.GetContentRegionAvail().X : width);
		height = ((!height.HasValue) ? new float?(UIShared.LineHeight) : ((height == 0f) ? new float?(ImGui.GetFrameHeight()) : height));
		ImGui.PushID(ImU8String.op_Implicit(id));
		List<(TKey, string)> list = items.ToList();
		string valueText = hintText;
		TKey localValue = selectedId;
		if (displayType == ComboButtonDisplayType.Items)
		{
			string item = list.FirstOrDefault<(TKey, string)>(((TKey id, string label) x) => x.id.Equals(localValue)).Item2;
			if (item != null)
			{
				valueText = GetComboValueText(item, displayType);
			}
		}
		else
		{
			valueText = GetComboValueText(valueText, displayType);
		}
		bool isHovered;
		bool isActive;
		bool isClicked;
		bool isOpen;
		Vector2 vector = DrawComboButton(id, text, valueText, width, height.Value, disabled, out isHovered, out isActive, out isClicked, out isOpen);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		if (isClicked && !disabled)
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!isOpen)
		{
			ImGui.PopID();
			return false;
		}
		if (list.Count == 0)
		{
			ImGui.PopID();
			return false;
		}
		bool num = DrawComboPopup<(TKey, string)>(id, new Vector2(vector.X, vector.Y + height.Value), width, height.Value, list, maxItemsDisplayed, closeOnSelection: true, ((TKey id, string label) tuple) => tuple.label, ((TKey id, string label) tuple) => EqualityComparer<TKey>.Default.Equals(localValue, tuple.id), delegate((TKey id, string label) tuple)
		{
			if (!EqualityComparer<TKey>.Default.Equals(localValue, tuple.id))
			{
				(localValue, _) = tuple;
				return true;
			}
			return false;
		}, drawHeader);
		if (num)
		{
			selectedId = localValue;
		}
		ImGui.PopID();
		return num;
	}

	private static float GetEffectiveWrapWidth(bool multiline, float? width, float? wrapWidth = float.MaxValue, float xPadding = 0f)
	{
		if (!multiline)
		{
			return wrapWidth ?? float.MaxValue;
		}
		if (wrapWidth.HasValue && wrapWidth.Value != float.MaxValue)
		{
			return wrapWidth.Value - xPadding * 2f;
		}
		if (width.HasValue)
		{
			return width.Value - xPadding * 2f;
		}
		return ImGui.GetContentRegionAvail().X - xPadding * 2f;
	}

	public static void StyledText(ImU8String text, float? fontSize = null, float opacity = 0.8f, float bgOpacity = 0f, float bgRounding = 4f, float glowStrength = 0.2f, AnimationType animationType = AnimationType.Static, Vector3? colorA = null, Vector3? colorB = null, Vector3? glowA = null, Vector3? glowB = null, Vector3? bgColor = null, float? xPadding = null, float? yPadding = null, float? width = null, float? wrapWidth = float.MaxValue, bool multiline = false, string? tooltip = null, string? tooltipSub = null, Action? action = null, ImDrawListPtr? targetDrawList = null, Vector2? screenOffset = null, Vector2? clipMin = null, Vector2? clipMax = null)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		float valueOrDefault = xPadding.GetValueOrDefault();
		if (!xPadding.HasValue)
		{
			valueOrDefault = ((bgOpacity > 0f) ? UIShared.TextBgPadding.X : 0f);
			xPadding = valueOrDefault;
		}
		valueOrDefault = yPadding.GetValueOrDefault();
		if (!yPadding.HasValue)
		{
			valueOrDefault = ((bgOpacity > 0f) ? UIShared.TextBgPadding.Y : 0f);
			yPadding = valueOrDefault;
		}
		Vector2 vector = screenOffset ?? ImGui.GetCursorScreenPos();
		float effectiveWrapWidth = GetEffectiveWrapWidth(multiline, width, wrapWidth, xPadding.Value);
		Vector2 vector2 = ImGui.CalcTextSize(text, true, effectiveWrapWidth);
		Vector2 vector3 = new Vector2(vector2.X + xPadding.Value * 2f, vector2.Y + yPadding.Value * 2f);
		Vector2 vector4 = vector + vector3;
		if (!targetDrawList.HasValue)
		{
			if (action != null)
			{
				ImU8String val = default(ImU8String);
				((ImU8String)(ref val))._002Ector(2, 1);
				((ImU8String)(ref val)).AppendLiteral("##");
				((ImU8String)(ref val)).AppendFormatted<Guid>(Guid.NewGuid());
				ImGui.InvisibleButton(val, vector3, (ImGuiButtonFlags)0);
			}
			else
			{
				ImGui.Dummy(vector3);
			}
		}
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub, vector, vector4);
		}
		if (action != null && UiUtil.IsRectHovered(vector, vector4))
		{
			ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			if (UiUtil.IsRectClicked(vector, vector4, (ImGuiMouseButton)0))
			{
				action();
			}
		}
		BuildStyledText(vector, vector3, text, fontSize, opacity, bgOpacity, bgRounding, glowStrength, animationType, colorA, colorB, glowA, glowB, bgColor, xPadding.Value, yPadding.Value, effectiveWrapWidth, targetDrawList, screenOffset, clipMin, clipMax);
	}

	public static void NoticeText(string text)
	{
		Highlighted(text, UIShared.AccentActive);
	}

	public static void WarningText(string text)
	{
		Highlighted(text, UIShared.Warn);
	}

	public static void Highlighted(string text, Vector4 col)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (UIShared.SubFont.Push())
		{
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = col.AsVector3();
			float? wrapWidth = ImGui.GetContentRegionAvail().X;
			StyledText(text2, null, 0.8f, 0.2f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, wrapWidth);
		}
	}

	public static bool BeginContainer(string text, bool defaultExpanded = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		bool num = Container(ImU8String.op_Implicit(text), ExpandedStates, defaultExpanded, ImGui.GetContentRegionAvail().X - UIShared.WindowPadding.X, AnimationType.Static, UIShared.AccentActive.AsVector3());
		if (num)
		{
			ImGui.Indent();
		}
		SpacingY(4f);
		return num;
	}

	public static void EndContainer()
	{
		ImGui.Unindent();
		Separator(ImGui.GetContentRegionAvail().X, 0f, UIShared.SeparatorSpacing);
	}

	public static bool Container(ImU8String text, Dictionary<uint, bool> expandedStates, bool defaultExpanded = false, float? width = null, AnimationType animationType = AnimationType.Static, Vector3? colorA = null, Vector3? colorB = null, Vector3? glowA = null, Vector3? glowB = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		uint iD = ImGui.GetID(text);
		ImGui.PushID((UIntPtr)iD);
		if (!expandedStates.TryGetValue(iD, out var value))
		{
			value = (expandedStates[iD] = defaultExpanded);
		}
		Vector2 vector = new Vector2(4f, 4f);
		float num = 4f;
		float num2 = 3f;
		Vector4 iconTextBgNormal = UIShared.IconTextBgNormal;
		Vector4 iconTextBgHovered = UIShared.IconTextBgHovered;
		Vector4 iconTextBgClicked = UIShared.IconTextBgClicked;
		Vector4 iconTextNormal = UIShared.IconTextNormal;
		Vector4 iconTextHovered = UIShared.IconTextHovered;
		Vector4 iconTextActive = UIShared.IconTextActive;
		string text2 = FontAwesomeExtensions.ToIconString((FontAwesomeIcon)(value ? 61655 : 61658));
		Vector2 vector2 = UiUtil.CalcTextSize(UIShared.NormalIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61655));
		Vector2 size = ImGui.CalcTextSize(text, false, -1f);
		Vector2 vector3 = new Vector2(vector.X + vector2.X + num, MathF.Max(vector2.Y, size.Y) + vector.Y * 2f);
		Vector2 vector4 = new Vector2(vector.X + size.X, MathF.Max(vector2.Y, size.Y) + vector.Y * 2f);
		Vector2 vector5 = new Vector2(width ?? (vector3.X + vector4.X), vector3.Y);
		if (ImGui.InvisibleButton(ImU8String.op_Implicit("##btn"), vector5, (ImGuiButtonFlags)0))
		{
			value = (expandedStates[iD] = !value);
		}
		bool flag3 = ImGui.IsItemHovered();
		bool num3 = ImGui.IsItemActive();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector4 vector6 = (num3 ? iconTextBgClicked : (flag3 ? iconTextBgHovered : iconTextBgNormal));
		Vector4 vector7 = (num3 ? iconTextActive : (flag3 ? iconTextHovered : iconTextNormal));
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(itemRectMin, itemRectMax, ImGui.GetColorU32(vector6), num2);
		MathF.Max(vector2.Y, size.Y);
		using (UIShared.NormalIconFont.Push())
		{
			Vector2 vector8 = new Vector2(itemRectMin.X + vector.X, itemRectMin.Y + (vector5.Y - vector2.Y) * 0.5f);
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector8, ImGui.GetColorU32(vector7), ImU8String.op_Implicit(text2));
		}
		BuildStyledText(new Vector2(itemRectMin.X + vector3.X, itemRectMin.Y + (vector5.Y - size.Y) * 0.5f), size, text, null, 0.8f, 0f, 4f, 0.2f, animationType, colorA, colorB, glowA, glowB, null, 0f, 0f, float.MaxValue);
		ImGui.PopID();
		return value;
	}

	private unsafe static bool BuildStyledText(Vector2 drawPos, Vector2 size, ImU8String text, float? fontSize = null, float opacity = 0.8f, float bgOpacity = 0f, float bgRounding = 4f, float glowStrength = 0.2f, AnimationType animationType = AnimationType.Static, Vector3? colorA = null, Vector3? colorB = null, Vector3? glowA = null, Vector3? glowB = null, Vector3? bgColor = null, float xPadding = 0f, float yPadding = 0f, float? wrapWidth = float.MaxValue, ImDrawListPtr? targetDrawList = null, Vector2? screenOffset = null, Vector2? clipMin = null, Vector2? clipMax = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Expected O, but got Unknown
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_070e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImDrawListPtr value = (ImDrawListPtr)(((_003F?)targetDrawList) ?? ImGui.GetWindowDrawList());
		Vector2 vector;
		Vector2 vector2;
		if (!clipMin.HasValue || !clipMax.HasValue)
		{
			vector = Vector2.Max(drawPos, ImGui.GetWindowPos());
			vector2 = Vector2.Min(drawPos + size, ImGui.GetWindowPos() + ImGui.GetWindowSize());
		}
		else
		{
			vector = clipMin.Value;
			vector2 = clipMax.Value;
		}
		if (vector.X >= vector2.X || vector.Y >= vector2.Y)
		{
			return false;
		}
		((ImDrawListPtr)(ref value)).PushClipRect(vector, vector2, true);
		ImGui.SetCursorScreenPos(vector);
		Vector2 vector3 = drawPos + new Vector2(xPadding, yPadding);
		try
		{
			if (((ImU8String)(ref text)).IsEmpty)
			{
				return false;
			}
			Vector3 valueOrDefault = colorA.GetValueOrDefault();
			if (!colorA.HasValue)
			{
				valueOrDefault = Vector3.One;
				colorA = valueOrDefault;
			}
			Vector3? vector4 = colorB;
			if (!vector4.HasValue)
			{
				colorB = colorA;
			}
			vector4 = glowA;
			if (!vector4.HasValue)
			{
				glowA = colorA;
			}
			vector4 = glowB;
			if (!vector4.HasValue)
			{
				glowB = glowA;
			}
			bool flag = colorA == colorB;
			bool flag2 = glowA == glowB;
			if (bgOpacity > 0f)
			{
				Vector4 vector5 = ((!bgColor.HasValue) ? new Vector4(colorA.Value, opacity * bgOpacity) : new Vector4(bgColor.Value, bgOpacity));
				((ImDrawListPtr)(ref value)).AddRectFilled(vector, vector2, ImGui.GetColorU32(vector5), bgRounding * ImGuiHelpers.GlobalScale);
			}
			string text2 = ((object)(*(ImU8String*)(&text))/*cast due to constrained. prefix*/).ToString();
			SeStringBuilder val = new SeStringBuilder();
			bool flag3 = (uint)(animationType - 5) <= 1u;
			if (!flag3 && flag && flag2)
			{
				val.PushColorRgba(new Vector4(colorA.Value, 1f));
				val.PushEdgeColorRgba(new Vector4(glowA.Value, 1f));
				val.Append(text2);
				val.PopEdgeColor();
				val.PopColor();
			}
			else
			{
				float num = (float)ImGui.GetTime();
				int num2 = 0;
				int num3 = text2.Count((char c2) => c2 != '\r' && c2 != '\n');
				string text3 = text2;
				for (int num4 = 0; num4 < text3.Length; num4++)
				{
					char c = text3[num4];
					if (c == '\r' || c == '\n')
					{
						val.Append(c.ToString());
						continue;
					}
					float num5 = ((num3 > 1) ? ((float)num2 / (float)(num3 - 1)) : 0f);
					float num6;
					float value2;
					switch (animationType)
					{
					case AnimationType.Wave:
					{
						float num12 = num5 + num * 0.5f;
						float num13 = num12 - MathF.Floor(num12);
						num6 = ((num13 <= 0.5f) ? (num13 * 2f) : (1f - (num13 - 0.5f) * 2f));
						value2 = num6;
						break;
					}
					case AnimationType.Chase:
					{
						float num8 = num * 0.6f % 1f;
						float num9 = MathF.Abs(num5 - num8);
						float num10 = 0.15f;
						float num11 = 1f - num9 / num10;
						if (num11 < 0f)
						{
							num11 = 0f;
						}
						else if (num11 > 1f)
						{
							num11 = 1f;
						}
						value2 = (num6 = num11 * num11 * (3f - 2f * num11));
						break;
					}
					case AnimationType.Pulse:
						num6 = 0.5f * (1f + MathF.Sin((float)Math.PI * 2f * num * 0.2f));
						value2 = num6;
						break;
					case AnimationType.EasePulse:
					{
						float num7 = 0.5f * (1f + MathF.Sin((float)Math.PI * 2f * num * 0.6f));
						value2 = (num6 = num7 * num7 * (3f - 2f * num7));
						break;
					}
					case AnimationType.RainbowWave:
					{
						Vector3 vector7 = HsvToRgbVec((num5 + num * 0.15f) % 1f, 0.9f, 0.95f);
						val.PushColorRgba(new Vector4(vector7, 1f));
						val.PushEdgeColorRgba(new Vector4(vector7 * 0.25f, 1f));
						val.Append(c.ToString());
						val.PopEdgeColor();
						val.PopColor();
						num2++;
						continue;
					}
					case AnimationType.RainbowPulse:
					{
						Vector3 vector6 = HsvToRgbVec(num * 0.15f % 1f, 0.9f, 0.95f);
						val.PushColorRgba(new Vector4(vector6, 1f));
						val.PushEdgeColorRgba(new Vector4(vector6 * 0.25f, 1f));
						val.Append(c.ToString());
						val.PopEdgeColor();
						val.PopColor();
						num2++;
						continue;
					}
					default:
						num6 = num5;
						value2 = num5;
						break;
					}
					float amount = Math.Clamp(num6, 0f, 1f);
					float amount2 = Math.Clamp(value2, 0f, 1f);
					Vector3 value3 = (flag ? colorA.Value : Vector3.Lerp(colorA.Value, colorB.Value, amount));
					Vector3 value4 = (flag2 ? glowA.Value : Vector3.Lerp(glowA.Value, glowB.Value, amount2));
					val.PushColorRgba(new Vector4(value3, 1f));
					val.PushEdgeColorRgba(new Vector4(value4, 1f));
					val.Append(c.ToString());
					val.PopEdgeColor();
					val.PopColor();
					num2++;
				}
			}
			byte[] array = SeString.Parse(val.GetViewAsSpan()).Encode();
			SeStringDrawParams val2 = default(SeStringDrawParams);
			((SeStringDrawParams)(ref val2)).Color = ImGui.GetColorU32(new Vector4(colorA.Value, 1f));
			((SeStringDrawParams)(ref val2)).Opacity = opacity;
			((SeStringDrawParams)(ref val2)).Edge = glowStrength > 0f;
			((SeStringDrawParams)(ref val2)).EdgeColor = ImGui.GetColorU32(new Vector4(glowA.Value, 1f));
			((SeStringDrawParams)(ref val2)).EdgeStrength = glowStrength;
			((SeStringDrawParams)(ref val2)).Font = ImGui.GetFont();
			((SeStringDrawParams)(ref val2)).FontSize = ((!fontSize.HasValue) ? new float?(ImGui.GetFontSize()) : (fontSize * ImGuiHelpers.GlobalScale));
			((SeStringDrawParams)(ref val2)).WrapWidth = wrapWidth;
			((SeStringDrawParams)(ref val2)).TargetDrawList = value;
			((SeStringDrawParams)(ref val2)).ScreenOffset = screenOffset ?? vector3;
			SeStringDrawParams val3 = val2;
			ImGuiHelpers.SeStringWrapped((ReadOnlySpan<byte>)array, ref val3, default(ImGuiId), (ImGuiButtonFlags)1);
		}
		finally
		{
			((ImDrawListPtr)(ref value)).PopClipRect();
			ImGui.SetCursorScreenPos(cursorScreenPos);
		}
		return true;
	}

	private static Vector3 HsvToRgbVec(float h, float s, float v)
	{
		var (x, y, z) = HsvToRgb(h, s, v);
		return new Vector3(x, y, z);
	}

	private static (float, float, float) HsvToRgb(float h, float s, float v)
	{
		if (s <= 0f)
		{
			return (v, v, v);
		}
		h = (h % 1f + 1f) % 1f;
		float num = h * 6f;
		int num2 = (int)MathF.Floor(num);
		float num3 = num - (float)num2;
		float num4 = v * (1f - s);
		float num5 = v * (1f - s * num3);
		float num6 = v * (1f - s * (1f - num3));
		return num2 switch
		{
			0 => (v, num6, num4), 
			1 => (num5, v, num4), 
			2 => (num4, v, num6), 
			3 => (num4, num5, v), 
			4 => (num6, num4, v), 
			_ => (v, num4, num5), 
		};
	}

	public static void IconText(string text, Vector4? colorA = null, float opacity = 1f, float wrapWidth = 0f, bool multiline = true)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (wrapWidth <= 0f)
		{
			wrapWidth = ImGui.GetContentRegionAvail().X;
		}
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float num = cursorScreenPos.X;
		float num2 = cursorScreenPos.Y;
		ImGui.GetFontSize();
		float textLineHeight = ImGui.GetTextLineHeight();
		IFontHandle normalIconFont = UIShared.NormalIconFont;
		_ = UIShared.NormalIconSize;
		foreach (IconTextSegment item in ParseIconText(text))
		{
			if (item.IsIcon)
			{
				string text2 = FontAwesomeExtensions.ToIconString(item.Icon.Value);
				using (normalIconFont.Push())
				{
					float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f).X;
					if (num > cursorScreenPos.X && num + x > cursorScreenPos.X + wrapWidth)
					{
						num = cursorScreenPos.X;
						num2 += textLineHeight;
					}
					ImGui.SetCursorScreenPos(new Vector2(num, num2));
					ImU8String text3 = ImU8String.op_Implicit(text2);
					Vector3? colorA2 = colorA?.AsVector3() ?? UIShared.AccentActive.AsVector3();
					float opacity2 = opacity;
					StyledText(text3, null, opacity2, 0f, 4f, 0.2f, AnimationType.Static, colorA2, null, null, null, null, null, null, null, float.MaxValue);
					num += x;
				}
				continue;
			}
			string[] array = Regex.Split(item.Text, "(\\s+)");
			foreach (string text4 in array)
			{
				if (string.IsNullOrEmpty(text4))
				{
					continue;
				}
				using (UIShared.NormalFont.Push())
				{
					float x2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text4), false, -1f).X;
					if (!string.IsNullOrWhiteSpace(text4) && num > cursorScreenPos.X && num + x2 > cursorScreenPos.X + wrapWidth)
					{
						num = cursorScreenPos.X;
						num2 += textLineHeight;
					}
					ImGui.SetCursorScreenPos(new Vector2(num, num2));
					ImU8String text5 = ImU8String.op_Implicit(text4);
					Vector3? colorA2 = colorA?.AsVector3() ?? UIShared.Normal.AsVector3();
					float opacity2 = opacity;
					StyledText(text5, null, opacity2, 0f, 4f, 0.2f, AnimationType.Static, colorA2, null, null, null, null, null, null, null, float.MaxValue);
					num += x2;
				}
			}
		}
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, num2 + textLineHeight));
	}

	private static List<IconTextSegment> ParseIconText(string text)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		List<IconTextSegment> list = new List<IconTextSegment>();
		MatchCollection matchCollection = new Regex("\\[icon:(?<icon>[A-Za-z0-9_]+)\\]").Matches(text);
		int num = 0;
		foreach (Match item in matchCollection)
		{
			if (item.Index > num)
			{
				int num2 = num;
				list.Add(new IconTextSegment(text.Substring(num2, item.Index - num2)));
			}
			if (Enum.TryParse<FontAwesomeIcon>(item.Groups["icon"].Value, out FontAwesomeIcon result))
			{
				list.Add(new IconTextSegment(result));
			}
			else
			{
				list.Add(new IconTextSegment(item.Value));
			}
			num = item.Index + item.Length;
		}
		if (num < text.Length)
		{
			list.Add(new IconTextSegment(text.Substring(num)));
		}
		return list;
	}

	public static void IconLabel(FontAwesomeIcon icon, string id, string? tooltip = null, string? tooltipSub = null, float? size = null, float iconScale = 1f, Vector4? color = null, bool hover = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(id));
		if (!size.HasValue)
		{
			size = UIShared.LineHeight;
		}
		else if (size == 0f)
		{
			ImGui.PushStyleVar((ImGuiStyleVar)10, Vector2.Zero);
			size = ImGui.GetFrameHeight();
			ImGui.PopStyleVar();
		}
		ImGui.InvisibleButton(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), new Vector2(size.Value, size.Value), (ImGuiButtonFlags)0);
		bool flag = hover && ImGui.IsItemHovered();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		ImGui.SetWindowFontScale(iconScale);
		using (UIShared.NormalIconFont.Push())
		{
			Vector4 vector = color ?? (flag ? UIShared.IconHovered : UIShared.IconNormal);
			Vector2 vector2 = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * 0.5f;
			Vector2 vector3 = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, -1f);
			Vector2 vector4 = vector2 - vector3 * 0.5f;
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector4, ImGui.GetColorU32(vector), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		}
		ImGui.SetWindowFontScale(1f);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		ImGui.PopID();
	}

	public static bool IconButton(FontAwesomeIcon icon, string id, bool disabled = false, string? tooltip = null, string? tooltipSub = null, float? size = null, float iconScale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(id));
		if (!size.HasValue)
		{
			size = UIShared.NormalIconSize * iconScale;
		}
		else if (size == 0f)
		{
			ImGui.PushStyleVar((ImGuiStyleVar)10, Vector2.Zero);
			size = ImGui.GetFrameHeight();
			ImGui.PopStyleVar();
		}
		bool flag = ImGui.InvisibleButton(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), new Vector2(size.Value, size.Value), (ImGuiButtonFlags)0);
		bool flag2 = ImGui.IsItemHovered();
		bool flag3 = ImGui.IsItemActive();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		ImGui.SetWindowFontScale(iconScale);
		using (UIShared.NormalIconFont.Push())
		{
			Vector4 vector = (disabled ? UIShared.IconDisabled : (flag3 ? UIShared.IconActive : (flag2 ? UIShared.IconHovered : UIShared.IconNormal)));
			Vector2 vector2 = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * 0.5f;
			Vector2 vector3 = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, -1f);
			Vector2 vector4 = vector2 - vector3 * 0.5f;
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector4, ImGui.GetColorU32(vector), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		}
		ImGui.SetWindowFontScale(1f);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		ImGui.PopID();
		if (flag)
		{
			return !disabled;
		}
		return false;
	}

	public static bool IconToggleButton(FontAwesomeIcon icon, string label, ref bool value, bool disabled = false, string? tooltip = null, string? tooltipSub = null, float? size = null, float iconScale = 1f, FontAwesomeIcon? toggledIcon = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(label));
		if (label.StartsWith("##"))
		{
			label = string.Empty;
		}
		if (label.Contains("##"))
		{
			label = label.Split("##")[0];
		}
		icon = ((value && toggledIcon.HasValue) ? toggledIcon.Value : icon);
		float num = 2f * ImGuiHelpers.GlobalScale;
		if (!size.HasValue)
		{
			size = UIShared.NormalIconSize * iconScale;
		}
		else if (size == 0f)
		{
			ImGui.PushStyleVar((ImGuiStyleVar)10, Vector2.Zero);
			size = ImGui.GetFrameHeight();
			ImGui.PopStyleVar();
		}
		bool flag = ImGui.InvisibleButton(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), new Vector2(size.Value, size.Value), (ImGuiButtonFlags)0);
		bool flag2 = ImGui.IsItemHovered();
		bool flag3 = ImGui.IsItemActive();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		ImGui.SetWindowFontScale(iconScale);
		Vector2 vector4;
		using (UIShared.NormalIconFont.Push())
		{
			Vector4 vector = (disabled ? UIShared.IconDisabled : (flag3 ? UIShared.IconActive : (flag2 ? UIShared.IconHovered : (value ? UIShared.IconToggled : UIShared.IconNormal))));
			Vector2 vector2 = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * 0.5f;
			Vector2 vector3 = ImGui.CalcTextSize(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), false, -1f);
			vector4 = vector2 - vector3 * 0.5f;
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector4, ImGui.GetColorU32(vector), ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)));
		}
		ImGui.SetWindowFontScale(1f);
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		if (!string.IsNullOrWhiteSpace(label))
		{
			IFontHandle normalFont = UIShared.NormalFont;
			try
			{
				Vector4 obj = (disabled ? UIShared.IconLabelDisabled : (flag3 ? UIShared.IconLabelActive : (flag2 ? UIShared.IconLabelHovered : (value ? UIShared.IconLabelToggled : UIShared.IconLabelNormal))));
				ImGui.SameLine(0f, num);
				ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, vector4.Y));
				ImGui.TextColored(ImGui.GetColorU32(obj), ImU8String.op_Implicit(label));
			}
			finally
			{
				((IDisposable)normalFont)?.Dispose();
			}
		}
		ImGui.PopID();
		int num2;
		if (flag)
		{
			num2 = ((!disabled) ? 1 : 0);
			if (num2 != 0)
			{
				value = !value;
			}
		}
		else
		{
			num2 = 0;
		}
		return (byte)num2 != 0;
	}

	public static bool IconToggleButton(FontAwesomeIcon icon, string label, bool value, bool disabled = false, string? tooltip = null, string? tooltipSub = null, float? size = null, float iconScale = 1f, FontAwesomeIcon? toggledIcon = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return IconToggleButton(icon, label, ref value, disabled, tooltip, tooltipSub, size, iconScale, toggledIcon);
	}

	public static bool Checkbox(string label, ref bool value, bool disabled = false, string? tooltip = null, string? tooltipSub = null, float? size = null)
	{
		return IconToggleButton((FontAwesomeIcon)61640, label, ref value, disabled, tooltip, tooltipSub, size, 1f, (FontAwesomeIcon)61770);
	}

	public static bool IconTextButton(FontAwesomeIcon icon, string text, string id, bool disabled = false, string? tooltip = null, string? tooltipSub = null, float? width = null, float? height = null, float iconScale = 0.8f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(id));
		float iconTextPadding = UIShared.IconTextPadding;
		float iconTextPadding2 = UIShared.IconTextPadding;
		Vector4 iconTextBgNormal = UIShared.IconTextBgNormal;
		Vector4 iconTextBgHovered = UIShared.IconTextBgHovered;
		Vector4 iconTextBgClicked = UIShared.IconTextBgClicked;
		Vector4 iconTextBgActive = UIShared.IconTextBgActive;
		Vector4 iconTextNormal = UIShared.IconTextNormal;
		Vector4 iconTextHovered = UIShared.IconTextHovered;
		Vector4 iconTextActive = UIShared.IconTextActive;
		Vector4 iconTextDisabled = UIShared.IconTextDisabled;
		string text2 = FontAwesomeExtensions.ToIconString(icon);
		Vector2 vector = UiUtil.CalcTextSize(UIShared.NormalIconFont, text2, iconScale);
		Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
		Vector2 vector3 = UiUtil.CalcIconTextSize(icon, text, iconScale);
		if (width.HasValue)
		{
			vector3.X = width.Value;
		}
		if (height.HasValue)
		{
			vector3.Y = height.Value;
		}
		bool flag = ImGui.InvisibleButton(ImU8String.op_Implicit("##btn"), vector3, (ImGuiButtonFlags)0);
		bool flag2 = ImGui.IsItemHovered();
		bool flag3 = ImGui.IsItemActive();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector4 vector4 = (disabled ? iconTextBgActive : (flag3 ? iconTextBgClicked : (flag2 ? iconTextBgHovered : iconTextBgNormal)));
		Vector4 vector5 = (disabled ? iconTextDisabled : (flag3 ? iconTextActive : (flag2 ? iconTextHovered : iconTextNormal)));
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(itemRectMin, itemRectMax, ImGui.GetColorU32(vector4), UIShared.IconTextRounding);
		float num = MathF.Max(vector.Y, vector2.Y);
		ImGui.SetWindowFontScale(iconScale);
		using (UIShared.NormalIconFont.Push())
		{
			Vector2 vector6 = new Vector2(itemRectMin.X + iconTextPadding, itemRectMin.Y + (vector3.Y - vector.Y) * 0.5f);
			((ImDrawListPtr)(ref windowDrawList)).AddText(vector6, ImGui.GetColorU32(vector5), ImU8String.op_Implicit(text2));
		}
		ImGui.SetWindowFontScale(1f);
		Vector2 vector7 = new Vector2(itemRectMin.X + iconTextPadding + vector.X + iconTextPadding2, itemRectMin.Y + (vector3.Y - vector2.Y) * 0.5f);
		((ImDrawListPtr)(ref windowDrawList)).AddText(vector7, ImGui.GetColorU32(vector5), ImU8String.op_Implicit(text));
		if (tooltip != null)
		{
			Tooltip.Show(tooltip, tooltipSub);
		}
		ImGui.PopID();
		if (flag)
		{
			return !disabled;
		}
		return false;
	}

	public static bool AxisXDrag(string id, ref float value, float width, float speed = 0.001f)
	{
		return AxisDrag(id, ref value, new Vector4(0.8f, 0.2f, 0.2f, 0.7f), width, speed);
	}

	public static bool AxisYDrag(string id, ref float value, float width, float speed = 0.001f)
	{
		return AxisDrag(id, ref value, new Vector4(0.2f, 0.8f, 0.2f, 0.7f), width, speed);
	}

	public static bool AxisZDrag(string id, ref float value, float width, float speed = 0.001f)
	{
		return AxisDrag(id, ref value, new Vector4(0.2f, 0.4f, 1f, 0.7f), width, speed);
	}

	public static bool AxisDrag(string id, ref float value, Vector4 borderColor, float width, float speed = 0.001f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushID(ImU8String.op_Implicit(id));
		ImGui.PushStyleVar((ImGuiStyleVar)12, 1f);
		ImGui.PushStyleColor((ImGuiCol)5, borderColor);
		ImGui.SetNextItemWidth(width);
		bool result = ImGui.DragFloat(ImU8String.op_Implicit(id), ref value, speed, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0);
		ImGui.PopStyleColor(1);
		ImGui.PopStyleVar();
		ImGui.PopID();
		return result;
	}
}

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace TargetPyon.Extensions;

public static class ImGuiEx
{
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

	public static bool IconButton(FontAwesomeIcon icon)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Button(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString(icon)), default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static bool Combo(string label, object obj, string nameofProp, string[] items, int itemsCount)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.Combo(ImU8String.op_Implicit(label), ref num, (ReadOnlySpan<string>)items, itemsCount);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool ColorPicker4(string label, string id, object obj, string nameofProp)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo? property = obj.GetType().GetProperty(nameofProp);
		Vector4 vector = (Vector4)(property?.GetValue(obj) ?? ((object)default(Vector4)));
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		((ImU8String)(ref val)).AppendLiteral("Button");
		if (ImGui.ColorButton(val, ref vector, (ImGuiColorEditFlags)32, default(Vector2)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(id), (ImGuiPopupFlags)0);
		}
		if (!string.IsNullOrWhiteSpace(label))
		{
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(label));
		}
		bool result = false;
		if (ImGui.BeginPopup(ImU8String.op_Implicit(id), (ImGuiWindowFlags)0))
		{
			ImGui.SetColorEditOptions((ImGuiColorEditFlags)177209344);
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(2, 2);
			((ImU8String)(ref val2)).AppendFormatted<string>(label);
			((ImU8String)(ref val2)).AppendLiteral("##");
			((ImU8String)(ref val2)).AppendFormatted<string>(id);
			result = ImGui.ColorPicker4(val2, ref vector, (ImGuiColorEditFlags)181404032);
			ImGui.EndPopup();
		}
		property?.SetValue(obj, vector);
		return result;
	}

	public static bool SliderInt(string label, object obj, string nameofProp, int min, int max)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.SliderInt(ImU8String.op_Implicit(label), ref num, min, max, default(ImU8String), (ImGuiSliderFlags)0);
		property?.SetValue(obj, num);
		return result;
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

	public static bool DragFloat(string label, object obj, string nameofProp, float spd, float min, float max)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		float num = (float)(property?.GetValue(obj) ?? ((object)0f));
		bool result = ImGui.DragFloat(ImU8String.op_Implicit(label), ref num, spd, min, max, default(ImU8String), (ImGuiSliderFlags)0);
		property?.SetValue(obj, num);
		return result;
	}

	public static void InputText(string label, ref int value, int length = 255)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		string s = value.ToString();
		ImGui.InputText(ImU8String.op_Implicit(label), ref s, length, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		int.TryParse(s, out value);
	}

	public static void InputText(string label, ref double value, int length = 255)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		string s = value.ToString();
		ImGui.InputText(ImU8String.op_Implicit(label), ref s, length, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		double.TryParse(s, out value);
	}

	public static bool InputText(string label, object obj, string nameofProp, int length = 255)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		string value = (string)(property?.GetValue(obj) ?? "");
		bool result = ImGui.InputText(ImU8String.op_Implicit(label), ref value, length, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		property?.SetValue(obj, value);
		return result;
	}

	public static bool InputInt(string label, object obj, string nameofProp)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.InputInt(ImU8String.op_Implicit(label), ref num, 1, 1, default(ImU8String), (ImGuiInputTextFlags)0);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool InputFloat(string label, object obj, string nameofProp)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		float num = (float)(property?.GetValue(obj) ?? ((object)0f));
		bool result = ImGui.InputFloat(ImU8String.op_Implicit(label), ref num, 0f, 0f, default(ImU8String), (ImGuiInputTextFlags)0);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool InputDouble(string label, object obj, string nameofProp)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		double num = (double)(property?.GetValue(obj) ?? ((object)0.0));
		bool result = ImGui.InputDouble(ImU8String.op_Implicit(label), ref num, 0.0, 0.0, default(ImU8String), (ImGuiInputTextFlags)0);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool Checkbox(string label, object obj, string nameofProp)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		bool flag = (bool)(property?.GetValue(obj) ?? ((object)false));
		bool result = ImGui.Checkbox(ImU8String.op_Implicit(label), ref flag);
		property?.SetValue(obj, flag);
		return result;
	}
}

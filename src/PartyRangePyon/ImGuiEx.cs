using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using ImGuiNET;

namespace PartyRangePyon;

public static class ImGuiEx
{
	public static bool Combo(string label, object obj, string nameofProp, string[] items, int itemsCount)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.Combo(label, ref num, items, itemsCount);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool ColorEdit4(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		Vector4 vector = (Vector4)(property?.GetValue(obj) ?? ((object)default(Vector4)));
		bool result = ImGui.ColorEdit4(label, ref vector);
		property?.SetValue(obj, vector);
		return result;
	}

	public static bool DragInt(string label, object obj, string nameofProp, float spd, int min, int max)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.DragInt(label, ref num, spd, min, max);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool DragFloat(string label, object obj, string nameofProp, float spd, float min, float max)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		float num = (float)(property?.GetValue(obj) ?? ((object)0f));
		bool result = ImGui.DragFloat(label, ref num, spd, min, max);
		property?.SetValue(obj, num);
		return result;
	}

	public static void InputText(string label, ref int value, uint length = 255u)
	{
		string s = value.ToString();
		ImGui.InputText(label, ref s, length);
		int.TryParse(s, out value);
	}

	public static void InputText(string label, ref double value, uint length = 255u)
	{
		string s = value.ToString();
		ImGui.InputText(label, ref s, length);
		double.TryParse(s, out value);
	}

	public static bool InputText(string label, object obj, string nameofProp, uint length = 255u)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		string value = (string)(property?.GetValue(obj) ?? "");
		bool result = ImGui.InputText(label, ref value, length);
		property?.SetValue(obj, value);
		return result;
	}

	public static bool InputInt(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		int num = (int)(property?.GetValue(obj) ?? ((object)0));
		bool result = ImGui.InputInt(label, ref num);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool InputFloat(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		float num = (float)(property?.GetValue(obj) ?? ((object)0f));
		bool result = ImGui.InputFloat(label, ref num);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool InputDouble(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		double num = (double)(property?.GetValue(obj) ?? ((object)0.0));
		bool result = ImGui.InputDouble(label, ref num);
		property?.SetValue(obj, num);
		return result;
	}

	public static bool Checkbox(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		bool flag = (bool)(property?.GetValue(obj) ?? ((object)false));
		bool result = ImGui.Checkbox(label, ref flag);
		property?.SetValue(obj, flag);
		return result;
	}

	public static bool IconButton(FontAwesomeIcon icon, string id = "ImguiExButton", Vector2 size = default(Vector2))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Button($"{FontAwesomeExtensions.ToIconString(icon)}##{FontAwesomeExtensions.ToIconString(icon)}-{id}", size);
		ImGui.PopFont();
		return result;
	}

	public static bool SmallIconButton(string icon, string id = "ImguiExButton")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.SmallButton($"{icon}##{icon}-{id}");
		ImGui.PopFont();
		return result;
	}

	public static bool IconButton(string icon, string id = "ImguiExButton")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(UiBuilder.IconFont);
		bool result = ImGui.Button($"{icon}##{icon}-{id}");
		ImGui.PopFont();
		return result;
	}
}

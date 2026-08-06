using System.Reflection;
using ImGuiNET;

namespace PvPyon;

public static class ImGuiEx
{
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

	public static bool InputText(string label, object obj, string nameofProp, uint length = 255u)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		string value = (string)(property?.GetValue(obj) ?? ((object)0));
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

	public static bool Checkbox(string label, object obj, string nameofProp)
	{
		PropertyInfo property = obj.GetType().GetProperty(nameofProp);
		bool flag = (bool)(property?.GetValue(obj) ?? ((object)false));
		bool result = ImGui.Checkbox(label, ref flag);
		property?.SetValue(obj, flag);
		return result;
	}
}

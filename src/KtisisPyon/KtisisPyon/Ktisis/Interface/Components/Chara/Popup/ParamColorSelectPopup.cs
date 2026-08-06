using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Editor.Characters.Types;

namespace Ktisis.Interface.Components.Chara.Popup;

public class ParamColorSelectPopup
{
	private bool _isOpening;

	private bool _isOpen;

	private CustomizeIndex Index;

	private bool IsAlpha;

	private Vector4[] Colors = Array.Empty<Vector4>();

	private string PopupId => $"##ColorSelect_{GetHashCode():X}";

	public void Open(CustomizeIndex index, uint[] colors)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_isOpening = true;
		Index = index;
		IsAlpha = colors.Length == 128;
		Colors = colors.Take(IsAlpha ? 96 : colors.Length).Select((Func<uint, Vector4>)ImGui.ColorConvertU32ToFloat4).ToArray();
	}

	public void Draw(ICustomizeEditor editor)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (_isOpening)
		{
			_isOpening = false;
			ImGui.OpenPopup(ImU8String.op_Implicit(PopupId), (ImGuiPopupFlags)0);
		}
		if (!ImGui.IsPopupOpen(ImU8String.op_Implicit(PopupId), (ImGuiPopupFlags)0))
		{
			return;
		}
		PopupDisposable val = ImRaii.Popup(ImU8String.op_Implicit(PopupId), (ImGuiWindowFlags)64);
		try
		{
			if (!val.Success)
			{
				if (_isOpen)
				{
					OnClose();
				}
				return;
			}
			_isOpen = true;
			byte customization = editor.GetCustomization(Index);
			if (IsAlpha)
			{
				DrawAlphaToggle(editor, customization);
				ImGui.Spacing();
			}
			DrawColorInput(editor, customization);
			ImGui.Spacing();
			DrawColorTable(editor, customization);
		}
		finally
		{
			((PopupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawColorInput(ICustomizeEditor editor, byte current)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ImGui.SetNextItemWidth(ImGui.GetFrameHeight() * 8f);
		int num = current & (IsAlpha ? (-129) : 255);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##Input_");
		((ImU8String)(ref val)).AppendFormatted<CustomizeIndex>(Index);
		if (ImGui.InputInt(val, ref num, 0, 0, default(ImU8String), (ImGuiInputTextFlags)0))
		{
			SetColor(editor, current, (byte)num);
		}
	}

	private void DrawColorTable(ICustomizeEditor editor, byte current)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)13, Vector2.Zero, true);
		try
		{
			StyleDisposable val2 = ImRaii.PushStyle((ImGuiStyleVar)11, 0f, true);
			try
			{
				for (int i = 0; i < Colors.Length; i++)
				{
					if (i % 8 != 0)
					{
						ImGui.SameLine();
					}
					Vector4 vector = Colors[i];
					ImU8String val3 = new ImU8String(2, 2);
					((ImU8String)(ref val3)).AppendFormatted<int>(i);
					((ImU8String)(ref val3)).AppendLiteral("##");
					((ImU8String)(ref val3)).AppendFormatted<CustomizeIndex>(Index);
					if (ImGui.ColorButton(val3, ref vector, (ImGuiColorEditFlags)7536640, default(Vector2)))
					{
						SetColor(editor, current, (byte)i);
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawAlphaToggle(ICustomizeEditor editor, byte current)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (current & 0x80) != 0;
		if (ImGui.Checkbox(ImU8String.op_Implicit("Transparency"), ref flag))
		{
			editor.SetCustomization(Index, (byte)(current ^ 0x80));
		}
	}

	private void SetColor(ICustomizeEditor editor, byte current, byte value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (IsAlpha)
		{
			value |= (byte)(current & 0x80);
		}
		if ((int)Index == 9)
		{
			editor.SetEyeColor(value);
		}
		else
		{
			editor.SetCustomization(Index, value);
		}
	}

	private void OnClose()
	{
		Colors = Array.Empty<Vector4>();
	}
}

using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonCam.Config;
using PyonCam.Services;

namespace PyonCam.UI.Windows;

public class KeybindsWindow : Window
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	private readonly IWindowContext _windows;

	private KeyValuePair<KeybindInput, VirtualKey>? SelectedBinding;

	private InputService InputService => _services.Get<InputService>();

	public KeybindsWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonCam FreeCam Keybinds")
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		_config = config;
		_services = services;
		_windows = windows;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(300f, 300f);
		((WindowSizeConstraints)(ref value)).MaximumSize = new Vector2(600f, 480f);
		((Window)this).SizeConstraints = value;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(300f, 300f) * ImGuiHelpers.GlobalScale;
	}

	public override void Draw()
	{
		if (!((Window)this).IsOpen)
		{
			SelectedBinding = null;
			InputService.IsConfiguringKeybind = false;
		}
		else
		{
			DrawKeybindList();
		}
	}

	private void DrawLabel(string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		float num = ImGui.GetColumnWidth() - x;
		ImGuiStylePtr style = ImGui.GetStyle();
		float num2 = num - ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		if (num2 > 0f)
		{
			ImGui.SetCursorPosX(num2);
		}
		ImGui.TextUnformatted(ImU8String.op_Implicit(text));
	}

	private void DrawKeybindList()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		ImGui.NewLine();
		ImGui.Columns(2, ImU8String.op_Implicit("keybindColumns"), true);
		foreach (KeyValuePair<KeybindInput, VirtualKey> keybind in _config.Keybinds)
		{
			DrawLabel($"{keybind.Key}");
			ImGui.NextColumn();
			if (ImGui.Button(ImU8String.op_Implicit((SelectedBinding?.Key == keybind.Key) ? "<Assign>" : (((int)keybind.Value != 0) ? $"{keybind.Value}" : "Unset")), new Vector2(100f, 0f) * ImGuiHelpers.GlobalScale))
			{
				SelectedBinding = ((SelectedBinding?.Key == keybind.Key) ? ((KeyValuePair<KeybindInput, VirtualKey>?)null) : new KeyValuePair<KeybindInput, VirtualKey>?(keybind));
				InputService.IsConfiguringKeybind = SelectedBinding.HasValue;
			}
			if (SelectedBinding?.Key == keybind.Key && !InputService.IsConfiguringKeybind)
			{
				if ((int)InputService.ConfiguredKey != 0)
				{
					foreach (KeyValuePair<KeybindInput, VirtualKey> keybind2 in _config.Keybinds)
					{
						if (keybind2.Value == InputService.ConfiguredKey)
						{
							_config.Keybinds[keybind2.Key] = (VirtualKey)0;
						}
					}
				}
				_config.Keybinds[keybind.Key] = InputService.ConfiguredKey;
				_config.Save();
				SelectedBinding = null;
			}
			ImGui.NextColumn();
		}
		DrawLabel("Adjust_Speed");
		ImGui.NextColumn();
		ImGui.BeginDisabled();
		ImGui.Button(ImU8String.op_Implicit("SCROLLWHEEL"), new Vector2(100f, 0f) * ImGuiHelpers.GlobalScale);
		ImGui.EndDisabled();
		ImGui.NextColumn();
		DrawLabel("Pan_Camera");
		ImGui.NextColumn();
		ImGui.BeginDisabled();
		ImGui.Button(ImU8String.op_Implicit("LCLICK/RCLICK"), new Vector2(100f, 0f) * ImGuiHelpers.GlobalScale);
		ImGui.EndDisabled();
		ImGui.NextColumn();
		DrawLabel("Exit_FreeCam");
		ImGui.NextColumn();
		ImGui.BeginDisabled();
		ImGui.Button(ImU8String.op_Implicit("ESCAPE"), new Vector2(100f, 0f) * ImGuiHelpers.GlobalScale);
		ImGui.EndDisabled();
		ImGui.NextColumn();
		ImGui.Columns(1, default(ImU8String), true);
	}
}

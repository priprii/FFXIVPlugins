using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Ktisis.Actions.Binds;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Actions;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;

namespace Ktisis.Editor.Actions.Input;

public class InputManager : IInputManager, IDisposable
{
	private class KeybindRegister
	{
		public readonly ActionKeybind Keybind;

		public readonly KeyInvokeHandler Handler;

		public readonly KeybindTrigger Trigger;

		public bool Enabled => Keybind.Enabled;

		public KeybindRegister(ActionKeybind keybind, KeyInvokeHandler handler, KeybindTrigger trigger)
		{
			Keybind = keybind;
			Handler = handler;
			Trigger = trigger;
		}
	}

	private readonly IEditorContext _context;

	private readonly HookScope _scope;

	private readonly IKeyState _keyState;

	private readonly List<KeybindRegister> Keybinds = new List<KeybindRegister>();

	private Configuration Config => _context.Config;

	private InputModule? Module { get; set; }

	public InputManager(IEditorContext context, HookScope scope, IKeyState keyState)
	{
		_context = context;
		_scope = scope;
		_keyState = keyState;
	}

	public void Initialize()
	{
		Module = _scope.Create<InputModule>(new object[1] { _context });
		Module.Initialize();
		Module.OnKeyEvent += OnKeyEvent;
		Module.EnableAll();
	}

	public void Register(ActionKeybind keybind, KeyInvokeHandler handler, KeybindTrigger trigger)
	{
		KeybindRegister item = new KeybindRegister(keybind, handler, trigger);
		Keybinds.Add(item);
	}

	private bool OnKeyEvent(VirtualKey key, VirtualKeyState state)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!_context.IsGPosing || !Config.Keybinds.Enabled || IsChatInputActive())
		{
			return false;
		}
		return GetActiveHotkey(key, state switch
		{
			VirtualKeyState.Down => KeybindTrigger.OnDown, 
			VirtualKeyState.Held => KeybindTrigger.OnHeld, 
			VirtualKeyState.Released => KeybindTrigger.OnRelease, 
			_ => throw new Exception($"Invalid key state encountered ({state})"), 
		})?.Handler() ?? false;
	}

	private KeybindRegister? GetActiveHotkey(VirtualKey key, KeybindTrigger trigger)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		KeybindRegister keybindRegister = null;
		int num = 0;
		foreach (KeybindRegister keybind in Keybinds)
		{
			KeyCombo combo = keybind.Keybind.Combo;
			if (keybind.Trigger.HasFlag(trigger) && combo.Key == key && combo.Modifiers.All((VirtualKey mod) => _keyState[mod]))
			{
				int num2 = combo.Modifiers.Length;
				if (keybindRegister == null || num2 >= num)
				{
					keybindRegister = keybind;
					num = num2;
				}
			}
		}
		return keybindRegister;
	}

	public unsafe static bool IsChatInputActive()
	{
		UIModule* ptr = UIModule.Instance();
		if (ptr == null)
		{
			return false;
		}
		RaptureAtkModule* raptureAtkModule = ((UIModule)ptr).GetRaptureAtkModule();
		if (raptureAtkModule != null)
		{
			return ((AtkModule)(&((RaptureAtkModule)raptureAtkModule).AtkModule)).IsTextInputActive();
		}
		return false;
	}

	public void Dispose()
	{
		try
		{
			Module?.Dispose();
			Keybinds.Clear();
			if (Module != null)
			{
				Module.OnKeyEvent -= OnKeyEvent;
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to dispose input manager:\n{value}");
		}
		GC.SuppressFinalize(this);
	}
}

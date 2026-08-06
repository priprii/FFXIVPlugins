using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PyonCam.Config;

namespace PyonCam.Services;

public class InputService : IDisposable
{
	private unsafe delegate void OnInputUpdateDelegate(InputDeviceManager* mgr, nint a2, void* controller, MouseDeviceData* mouseData, KeyboardDeviceData* keyData);

	private readonly Configuration _config;

	private readonly IServiceContext _services;

	public bool IsConfiguringKeybind;

	public VirtualKey ConfiguredKey;

	private CameraService CameraService => _services.Get<CameraService>();

	private Hook<OnInputUpdateDelegate>? HookInputUpdate { get; set; }

	private unsafe MouseDeviceData* MouseData { get; set; }

	public unsafe KeyboardDeviceData* KeyData { get; set; }

	public InputService(Configuration config, IServiceContext services)
	{
		_config = config;
		_services = services;
	}

	public unsafe void Initialize()
	{
		HookInputUpdate = _services.GameInteropProvider.HookFromSignature<OnInputUpdateDelegate>("E8 ?? ?? ?? ?? 83 7B 58 00", (OnInputUpdateDelegate)OnInputUpdateDetour, (HookBackend)0);
		HookInputUpdate?.Enable();
	}

	private unsafe void OnInputUpdateDetour(InputDeviceManager* mgr, nint a2, void* controller, MouseDeviceData* mouseData, KeyboardDeviceData* keyData)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected I4, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		HookInputUpdate?.Original(mgr, a2, controller, mouseData, keyData);
		try
		{
			UIModule* ptr = UIModule.Instance();
			if (ptr != null)
			{
				RaptureAtkModule* raptureAtkModule = ((UIModule)ptr).GetRaptureAtkModule();
				if (raptureAtkModule != null && ((AtkModule)(&((RaptureAtkModule)raptureAtkModule).AtkModule)).IsTextInputActive())
				{
					MouseData = null;
					KeyData = null;
					return;
				}
			}
			MouseData = mouseData;
			KeyData = keyData;
			if (IsConfiguringKeybind)
			{
				foreach (VirtualKey value2 in Enum.GetValues(typeof(VirtualKey)))
				{
					ushort num = (ushort)(int)value2;
					if (num <= 31)
					{
						if (num <= 15)
						{
							if (num < 8 || num >= 13)
							{
								goto IL_00e7;
							}
						}
						else if (num <= 26)
						{
							if (num >= 21)
							{
								goto IL_00e7;
							}
						}
						else if (num >= 28)
						{
							goto IL_00e7;
						}
					}
					else if (num <= 183)
					{
						if (num >= 160)
						{
							goto IL_00e7;
						}
					}
					else if (num > 228)
					{
						goto IL_00e7;
					}
					bool flag = false;
					goto IL_00ef;
					IL_00e7:
					flag = true;
					goto IL_00ef;
					IL_00ef:
					if (!flag && IsKeyDown(value2))
					{
						ConfiguredKey = (VirtualKey)(((int)value2 != 27) ? ((int)value2) : 0);
						IsConfiguringKeybind = false;
					}
				}
				return;
			}
			if (CameraService.FreeCam.Enabled)
			{
				if (CameraService.GetOrbitTarget() != 0L)
				{
					CameraService.FreeCam.Toggle();
				}
				CameraService.FreeCam.UpdateControl();
			}
			else if (CameraService.GetOrbitTarget() != 0L && IsKeyDown((VirtualKey)27))
			{
				CameraService.RevertOrbitTarget();
			}
		}
		catch (Exception value)
		{
			_services.Log.Error($"InputService Failed: {value}", Array.Empty<object>());
		}
	}

	public unsafe Vector2 MouseDelta()
	{
		if (MouseData != null)
		{
			return MouseData->GetDelta();
		}
		return Vector2.Zero;
	}

	public unsafe int ScrollDelta()
	{
		if (MouseData != null)
		{
			return MouseData->ScrollDelta;
		}
		return 0;
	}

	public unsafe bool IsMouseButtonHeld(MouseButton button, bool consume = false)
	{
		if (MouseData != null)
		{
			return MouseData->IsButtonHeld(button, consume);
		}
		return false;
	}

	public unsafe bool IsKeyDown(VirtualKey key, bool consume = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (KeyData != null)
		{
			return KeyData->IsKeyDown(key, consume);
		}
		return false;
	}

	public bool IsKeyDown(KeybindInput keybind, bool consume = true)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<KeybindInput, VirtualKey> keybinds = _config.Keybinds;
		if (keybinds.Keys.Contains(keybind) && (int)keybinds[keybind] != 0)
		{
			return IsKeyDown(keybinds[keybind], consume);
		}
		return false;
	}

	public void Dispose()
	{
		HookInputUpdate?.Dispose();
	}
}

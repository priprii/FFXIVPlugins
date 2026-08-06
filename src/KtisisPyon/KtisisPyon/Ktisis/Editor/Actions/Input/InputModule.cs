using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;

namespace Ktisis.Editor.Actions.Input;

public class InputModule : HookModule
{
	private enum WinMsg : uint
	{
		WM_KEYDOWN = 256u,
		WM_KEYUP = 257u,
		WM_MOUSEMOVE = 512u
	}

	private delegate nint InputNotificationDelegate(nint a1, WinMsg a2, nint a3, uint a4);

	private unsafe delegate nint ProcessMouseStateDelegate(TargetSystem* targets, nint a2, nint a3);

	private IEditorContext _context;

	[Signature("48 89 5C 24 ?? 55 56 57 41 56 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 40 4D 8B F9", DetourName = "InputNotificationDetour")]
	private Hook<InputNotificationDelegate> InputNotificationHook;

	[Signature("E8 ?? ?? ?? ?? 4C 8B BC 24 ?? ?? ?? ?? 4C 8B B4 24 ?? ?? ?? ?? 48 8B B4 24 ?? ?? ?? ?? 48 8B 9C 24 ?? ?? ?? ??", DetourName = "ProcessMouseStateDetour")]
	private Hook<ProcessMouseStateDelegate> ProcessMouseStateHook;

	public event KeyEventHandler? OnKeyEvent;

	public InputModule(IHookMediator hook, IEditorContext context)
		: base(hook)
	{
		_context = context;
	}

	private bool InvokeKeyEvent(VirtualKey key, VirtualKeyState state)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (this.OnKeyEvent == null)
		{
			return false;
		}
		return this.OnKeyEvent.GetInvocationList().Cast<KeyEventHandler>().Aggregate(seed: false, (bool result, KeyEventHandler handler) => result | handler(key, state));
	}

	private nint InputNotificationDetour(nint hWnd, WinMsg uMsg, nint wParam, uint lParam)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		VirtualKey key = (VirtualKey)(ushort)wParam;
		switch (uMsg)
		{
		case WinMsg.WM_KEYDOWN:
			if (InvokeKeyEvent(key, (lParam >> 30 != 0) ? VirtualKeyState.Held : VirtualKeyState.Down))
			{
				return 0;
			}
			break;
		case WinMsg.WM_KEYUP:
			if (InvokeKeyEvent(key, VirtualKeyState.Released))
			{
				return 0;
			}
			break;
		}
		return InputNotificationHook.Original(hWnd, uMsg, wParam, lParam);
	}

	private unsafe nint ProcessMouseStateDetour(TargetSystem* targets, nint a2, nint a3)
	{
		GameObject* gPoseTarget = ((TargetSystem)targets).GPoseTarget;
		nint num = ProcessMouseStateHook.Original(targets, a2, a3);
		int num2;
		int num3;
		if (((TargetSystem)targets).GPoseTarget != gPoseTarget)
		{
			if (_context.Config.Keybinds.BlockTargetLeftClick)
			{
				num2 = ((num == 0) ? 1 : 0);
				if (num2 != 0)
				{
					goto IL_0065;
				}
			}
			else
			{
				num2 = 0;
			}
			if (!_context.Config.Keybinds.BlockTargetRightClick)
			{
				goto IL_0065;
			}
			num3 = ((num == 16) ? 1 : 0);
			goto IL_0066;
		}
		goto IL_0072;
		IL_0065:
		num3 = 0;
		goto IL_0066;
		IL_0066:
		bool flag = (byte)num3 != 0;
		if (((uint)num2 | (flag ? 1u : 0u)) != 0)
		{
			((TargetSystem)targets).GPoseTarget = gPoseTarget;
		}
		goto IL_0072;
		IL_0072:
		return num;
	}
}

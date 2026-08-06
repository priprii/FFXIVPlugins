using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Hypostasis.Dalamud;

namespace Hypostasis.Game;

public class GameFunction<T> : IGameFunction where T : Delegate
{
	private nint? address;

	private T del;

	public string Signature { get; protected set; }

	public nint Address => address ?? SetupAddress(required: false);

	public T Invoke
	{
		get
		{
			if (Address == IntPtr.Zero)
			{
				return null;
			}
			return del ?? SetupDelegate();
		}
	}

	public Hook<T> Hook { get; private set; }

	public T Original
	{
		get
		{
			Hook<T> hook = Hook;
			return ((hook != null) ? hook.Original : null) ?? Invoke;
		}
	}

	public bool IsValid => Invoke != null;

	public bool IsHooked => Hook != null;

	public GameFunction()
	{
	}

	public GameFunction(string sig, bool required = false)
	{
		Signature = sig;
		if (required)
		{
			SetupAddress(required: true);
		}
	}

	protected nint SetupAddress(bool required)
	{
		try
		{
			address = ScanAddress();
		}
		catch (Exception exception)
		{
			address = IntPtr.Zero;
			DalamudApi.LogWarning("Failed to find signature " + Signature, exception);
			if (required)
			{
				throw;
			}
		}
		return address.Value;
	}

	protected virtual nint ScanAddress()
	{
		return DalamudApi.SigScanner.DalamudSigScanner.ScanText(Signature);
	}

	private T SetupDelegate()
	{
		nint? num = address;
		if (!num.HasValue || num.GetValueOrDefault() <= 0)
		{
			return null;
		}
		return del = Marshal.GetDelegateForFunctionPointer<T>(address.Value);
	}

	public void CreateHook(T detour, bool enable = true, bool dispose = true)
	{
		if (Address != IntPtr.Zero)
		{
			if (IsHooked)
			{
				throw new ApplicationException("Attempted to hook function more than once");
			}
			Hook = DalamudApi.GameInteropProvider.HookFromAddress<T>((IntPtr)Address, detour, (HookBackend)0);
			DalamudApi.SigScanner.AddHook<T>(Hook, enable, dispose);
		}
	}
}

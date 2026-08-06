using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc;

namespace TargetPyon;

public static class IPC
{
	public static ICallGateSubscriber<object> PyonCamInitializedSubscriber;

	public static ICallGateSubscriber<object> PyonCamDisposedSubscriber;

	public static ICallGateSubscriber<int> PyonCamGetIPCVersionSubscriber;

	public static ICallGateSubscriber<string> PyonCamGetVersionSubscriber;

	public static ICallGateSubscriber<ulong, bool> PyonCamSetCamTargetObjectSubscriber;

	public static ICallGateSubscriber<ulong> PyonCamGetCamTargetObjectSubscriber;

	public static ICallGateSubscriber<object> PyonCamResetCamTargetObjectSubscriber;

	public static ICallGateSubscriber<List<nint>> MareGetHandledAddresses;

	public static bool PyonCamEnabled { get; private set; }

	public static int PyonCamIPCVersion
	{
		get
		{
			try
			{
				return PyonCamGetIPCVersionSubscriber.InvokeFunc();
			}
			catch
			{
				return 0;
			}
		}
	}

	public static string PyonCamVersion
	{
		get
		{
			try
			{
				return PyonCamGetVersionSubscriber.InvokeFunc();
			}
			catch
			{
				return "0.0.0.0";
			}
		}
	}

	public static void Initialize()
	{
		PyonCamInitializedSubscriber = Plugin.PluginInterface.GetIpcSubscriber<object>("PyonCam.Initialized");
		PyonCamDisposedSubscriber = Plugin.PluginInterface.GetIpcSubscriber<object>("PyonCam.Disposed");
		PyonCamInitializedSubscriber.Subscribe((Action)EnablePyonCamIPC);
		PyonCamDisposedSubscriber.Subscribe((Action)DisablePyonCamIPC);
		MareGetHandledAddresses = Plugin.PluginInterface.GetIpcSubscriber<List<nint>>("LightlessSync.GetHandledAddresses");
		EnablePyonCamIPC();
	}

	public static bool SetCamTarget(ulong objectID)
	{
		if (!PyonCamEnabled)
		{
			return false;
		}
		try
		{
			return PyonCamSetCamTargetObjectSubscriber.InvokeFunc(objectID);
		}
		catch
		{
			PyonCamEnabled = false;
			return false;
		}
	}

	public static ulong GetCamTarget()
	{
		if (!PyonCamEnabled)
		{
			return 0uL;
		}
		try
		{
			return PyonCamGetCamTargetObjectSubscriber.InvokeFunc();
		}
		catch
		{
			PyonCamEnabled = false;
			return 0uL;
		}
	}

	public static void ResetCamTarget()
	{
		if (!PyonCamEnabled)
		{
			return;
		}
		try
		{
			PyonCamResetCamTargetObjectSubscriber.InvokeFunc();
		}
		catch
		{
			PyonCamEnabled = false;
		}
	}

	public static void EnablePyonCamIPC()
	{
		if (!PyonCamEnabled)
		{
			PyonCamGetIPCVersionSubscriber = Plugin.PluginInterface.GetIpcSubscriber<int>("PyonCam.GetIPCVersion");
			if (PyonCamIPCVersion != 2)
			{
				PyonCamEnabled = false;
				return;
			}
			PyonCamGetVersionSubscriber = Plugin.PluginInterface.GetIpcSubscriber<string>("PyonCam.GetVersion");
			PyonCamSetCamTargetObjectSubscriber = Plugin.PluginInterface.GetIpcSubscriber<ulong, bool>("PyonCam.SetCamTargetObject");
			PyonCamGetCamTargetObjectSubscriber = Plugin.PluginInterface.GetIpcSubscriber<ulong>("PyonCam.GetCamTargetObject");
			PyonCamResetCamTargetObjectSubscriber = Plugin.PluginInterface.GetIpcSubscriber<object>("PyonCam.ResetCamTargetObject");
			PyonCamEnabled = true;
		}
	}

	public static void DisablePyonCamIPC()
	{
		if (PyonCamEnabled)
		{
			PyonCamGetVersionSubscriber = null;
			PyonCamSetCamTargetObjectSubscriber = null;
			PyonCamGetCamTargetObjectSubscriber = null;
			PyonCamResetCamTargetObjectSubscriber = null;
			PyonCamEnabled = false;
		}
	}

	public static List<nint>? MareGetNearbyPlayerAddresses()
	{
		try
		{
			return MareGetHandledAddresses.InvokeFunc();
		}
		catch
		{
			return null;
		}
	}

	public static void Dispose()
	{
		ResetCamTarget();
		PyonCamInitializedSubscriber?.Unsubscribe((Action)EnablePyonCamIPC);
		PyonCamDisposedSubscriber?.Unsubscribe((Action)DisablePyonCamIPC);
		PyonCamInitializedSubscriber = null;
		PyonCamDisposedSubscriber = null;
		PyonCamGetVersionSubscriber = null;
		PyonCamSetCamTargetObjectSubscriber = null;
		PyonCamGetCamTargetObjectSubscriber = null;
		PyonCamResetCamTargetObjectSubscriber = null;
		PyonCamEnabled = false;
		MareGetHandledAddresses = null;
	}
}

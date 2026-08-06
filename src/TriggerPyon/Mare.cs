using System.Collections.Generic;
using Dalamud.Plugin.Ipc;

namespace TriggerPyon;

public static class Mare
{
	public static ICallGateSubscriber<List<nint>> LightlessGetHandledAddresses;

	public static ICallGateSubscriber<List<nint>> SnowcloakGetHandledAddresses;

	public static ICallGateSubscriber<List<nint>> MareGetHandledAddresses;

	public static ICallGateSubscriber<List<nint>> PlayerSyncGetHandledAddresses;

	public static void Initialize()
	{
		LightlessGetHandledAddresses = Plugin.PluginInterface.GetIpcSubscriber<List<nint>>("LightlessSync.GetHandledAddresses");
		SnowcloakGetHandledAddresses = Plugin.PluginInterface.GetIpcSubscriber<List<nint>>("Snowcloak.GetHandledAddresses");
		MareGetHandledAddresses = Plugin.PluginInterface.GetIpcSubscriber<List<nint>>("MareSynchronos.GetHandledAddresses");
		PlayerSyncGetHandledAddresses = Plugin.PluginInterface.GetIpcSubscriber<List<nint>>("PlayerSync.GetHandledAddresses");
	}

	public static HashSet<nint>? MareGetNearbyPlayerAddresses()
	{
		try
		{
			HashSet<nint> result = new HashSet<nint>();
			if (LightlessGetHandledAddresses != null && ((ICallGateSubscriber)LightlessGetHandledAddresses).HasFunction)
			{
				LightlessGetHandledAddresses.InvokeFunc()?.ForEach(delegate(nint x)
				{
					result.Add(x);
				});
			}
			if (SnowcloakGetHandledAddresses != null && ((ICallGateSubscriber)SnowcloakGetHandledAddresses).HasFunction)
			{
				SnowcloakGetHandledAddresses.InvokeFunc()?.ForEach(delegate(nint x)
				{
					result.Add(x);
				});
			}
			if (MareGetHandledAddresses != null && ((ICallGateSubscriber)MareGetHandledAddresses).HasFunction)
			{
				MareGetHandledAddresses.InvokeFunc()?.ForEach(delegate(nint x)
				{
					result.Add(x);
				});
			}
			if (PlayerSyncGetHandledAddresses != null && ((ICallGateSubscriber)PlayerSyncGetHandledAddresses).HasFunction)
			{
				PlayerSyncGetHandledAddresses.InvokeFunc()?.ForEach(delegate(nint x)
				{
					result.Add(x);
				});
			}
			return result;
		}
		catch
		{
			return null;
		}
	}

	public static void Dispose()
	{
		LightlessGetHandledAddresses = null;
		SnowcloakGetHandledAddresses = null;
		MareGetHandledAddresses = null;
		PlayerSyncGetHandledAddresses = null;
	}
}

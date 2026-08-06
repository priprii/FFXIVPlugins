using System;
using System.Reflection;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Ktisis.Core;
using Ktisis.Interop.Ipc;
using Ktisis.Localization;
using Ktisis.Services.Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Ktisis;

public sealed class Ktisis : IDalamudPlugin, IDisposable
{
	private readonly ServiceProvider _services;

	public static LoggingService Log { get; private set; }

	public static INotificationManager Notification { get; private set; }

	public static LocaleManager Locale { get; private set; }

	public Ktisis(IPluginLog logger, INotificationManager notification, IDalamudPluginInterface dpi)
	{
		Log = new LoggingService(logger);
		Notification = notification;
		Locale = new LocaleManager(dpi);
		_services = new ServiceComposer().AddFromAttributes().AddDalamudServices(dpi).AddSingleton(Log)
			.AddSingleton<INotificationManager>(notification)
			.AddSingleton(Locale)
			.BuildProvider();
		_services.GetRequiredService<PluginContext>().Initialize();
		_services.GetRequiredService<IpcProvider>().RegisterIpc();
	}

	public static string GetVersion()
	{
		return Assembly.GetCallingAssembly().GetName().Version.ToString(3);
	}

	public static void WarningNotification(string content)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		Notification.AddNotification(new Notification
		{
			Content = content,
			Title = "[Warning] KtisisPyon",
			Type = (NotificationType)2
		});
	}

	public void Dispose()
	{
		try
		{
			_services.Dispose();
		}
		catch (Exception value)
		{
			Log.Error($"Error occurred during disposal:\n{value}");
		}
	}
}

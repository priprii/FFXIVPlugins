using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Ktisis.Services.Plugin;

[Singleton]
public class LoggingService
{
	private readonly LoggingLevelSwitch levelSwitch;

	public IPluginLog DalamudLog { get; private set; }

	public ILogger Logger { get; }

	public Queue<string> Logs { get; } = new Queue<string>();

	public LoggingService(IPluginLog logger)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		DalamudLog = logger;
		levelSwitch = new LoggingLevelSwitch(GetDefaultLevel());
		LoggerConfiguration val = new LoggerConfiguration().Enrich.WithProperty("Dalamud.PluginName", (object)"KtisisPyon", false).MinimumLevel.ControlledBy(levelSwitch).WriteTo.Logger(Log.Logger, false, (LogEventLevel)0, (LoggingLevelSwitch)null);
		Logger = (ILogger)(object)val.CreateLogger();
	}

	public void Fatal(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)5, null, messageTemplate, values);
	}

	public void Fatal(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)5, exception, messageTemplate, values);
	}

	public void Error(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)4, null, messageTemplate, values);
	}

	public void Error(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)4, exception, messageTemplate, values);
	}

	public void Warning(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)3, null, messageTemplate, values);
	}

	public void Warning(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)3, exception, messageTemplate, values);
	}

	public void Information(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)2, null, messageTemplate, values);
	}

	public void Information(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)2, exception, messageTemplate, values);
	}

	public void Info(string messageTemplate, params object[] values)
	{
		Information(messageTemplate, values);
	}

	public void Info(Exception? exception, string messageTemplate, params object[] values)
	{
		Information(exception, messageTemplate, values);
	}

	public void Debug(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)1, null, messageTemplate, values);
	}

	public void Debug(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)1, exception, messageTemplate, values);
	}

	public void Verbose(string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)0, null, messageTemplate, values);
	}

	public void Verbose(Exception? exception, string messageTemplate, params object[] values)
	{
		Write((LogEventLevel)0, exception, messageTemplate, values);
	}

	public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object[] values)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (Logs.Count >= 50)
		{
			for (int num = Logs.Count; num >= 50; num--)
			{
				Logs.Dequeue();
			}
		}
		Logs.Enqueue($"{level} | {DateTime.Now}   : {messageTemplate}\n");
		DalamudLog.Write(level, exception, messageTemplate ?? "", values);
	}

	private LogEventLevel GetDefaultLevel()
	{
		return (LogEventLevel)0;
	}
}

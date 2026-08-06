using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;

namespace Hypostasis.Dalamud;

public sealed class PluginCommandManager : IDisposable
{
	private readonly HashSet<string> pluginCommands = new HashSet<string>();

	public PluginCommandManager(object o)
	{
		MethodInfo[] allMethods = o.GetType().GetAllMethods();
		foreach (MethodInfo method in allMethods)
		{
			AddPluginCommandMethod(o, method);
		}
	}

	private void AddPluginCommandMethod(object o, MethodInfo method)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		PluginCommandAttribute customAttribute = method.GetCustomAttribute<PluginCommandAttribute>();
		if (customAttribute == null)
		{
			return;
		}
		CommandInfo val = new CommandInfo((HandlerDelegate)Delegate.CreateDelegate(typeof(HandlerDelegate), o, method))
		{
			HelpMessage = customAttribute.HelpMessage,
			ShowInHelp = customAttribute.ShowInHelp
		};
		string[] commands = customAttribute.Commands;
		foreach (string text in commands)
		{
			if (DalamudApi.CommandManager.AddHandler(text, val))
			{
				pluginCommands.Add(text);
			}
		}
	}

	public void Dispose()
	{
		foreach (string pluginCommand in pluginCommands)
		{
			DalamudApi.CommandManager.RemoveHandler(pluginCommand);
		}
	}
}

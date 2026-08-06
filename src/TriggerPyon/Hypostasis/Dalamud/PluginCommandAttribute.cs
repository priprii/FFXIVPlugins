using System;

namespace Hypostasis.Dalamud;

[AttributeUsage(AttributeTargets.Method)]
public class PluginCommandAttribute(params string[] commands) : Attribute
{
	public string[] Commands { get; init; } = commands;

	public string HelpMessage { get; init; } = string.Empty;

	public bool ShowInHelp { get; init; } = true;
}

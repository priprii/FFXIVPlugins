using System;

namespace Hypostasis.Dalamud;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class HypostasisMemberInjectionAttribute : HypostasisInjectionAttribute
{
	public string DetourName { get; init; }

	public int Offset { get; init; }

	public bool Required { get; init; }

	public bool EnableHook { get; init; } = true;

	public bool DisposeHook { get; init; } = true;
}

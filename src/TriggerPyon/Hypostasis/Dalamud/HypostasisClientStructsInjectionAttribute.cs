using System;

namespace Hypostasis.Dalamud;

public class HypostasisClientStructsInjectionAttribute : HypostasisMemberInjectionAttribute
{
	public Type ClientStructsType { get; init; }

	public string MemberName { get; init; } = "Instance";

	protected HypostasisClientStructsInjectionAttribute()
	{
	}

	public HypostasisClientStructsInjectionAttribute(Type type)
	{
		ClientStructsType = type;
	}
}
public sealed class HypostasisClientStructsInjectionAttribute<T> : HypostasisClientStructsInjectionAttribute
{
	public HypostasisClientStructsInjectionAttribute()
	{
		base.ClientStructsType = typeof(T);
	}
}

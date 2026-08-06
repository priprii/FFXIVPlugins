using System;

namespace Hypostasis.Dalamud;

[AttributeUsage(AttributeTargets.Struct)]
public class GameStructureAttribute(string ctor) : Attribute
{
	public string CtorSignature { get; init; } = ctor;
}

using System;
using System.Diagnostics;

namespace Hypostasis.Dalamud;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
[Conditional("DEBUG")]
public class HypostasisDebuggableAttribute : Attribute
{
}

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ktisis.Data.Mcdf;

public record McdfHeader
{
	public byte Version { get; set; }

	public required string FilePath { get; set; }

	public required McdfData Data { get; set; }

	[CompilerGenerated]
	[SetsRequiredMembers]
	protected McdfHeader(McdfHeader original)
	{
		Version = original.Version;
		FilePath = original.FilePath;
		Data = original.Data;
	}
}

using System;
using System.Collections.Generic;

namespace Ktisis.Data.Mcdf;

public record McdfData
{
	public record FileData
	{
		public string[] GamePaths { get; set; } = Array.Empty<string>();

		public int Length { get; set; }

		public string Hash { get; set; } = string.Empty;
	}

	public record FileSwap
	{
		public string[] GamePaths { get; set; } = Array.Empty<string>();

		public string FileSwapPath { get; set; } = string.Empty;
	}

	public string Description { get; set; } = string.Empty;

	public string GlamourerData { get; set; } = string.Empty;

	public string CustomizePlusData { get; set; } = string.Empty;

	public string ManipulationData { get; set; } = string.Empty;

	public List<FileData> Files { get; set; } = new List<FileData>();

	public List<FileSwap> FileSwaps { get; set; } = new List<FileSwap>();
}

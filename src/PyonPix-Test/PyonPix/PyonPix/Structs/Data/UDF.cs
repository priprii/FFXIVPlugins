using System;

namespace PyonPix.Structs.Data;

public class UDF
{
	public string FolderName { get; set; } = string.Empty;

	public string FolderPath { get; set; } = string.Empty;

	public string PixId { get; set; } = string.Empty;

	public string? PixName { get; set; }

	public bool PersistentCache { get; set; }

	public long SizeBytes { get; set; } = -1L;

	public DateTime? LastWriteUtc { get; set; }

	public bool IsRemoving { get; set; }

	public bool PixExists { get; set; }
}

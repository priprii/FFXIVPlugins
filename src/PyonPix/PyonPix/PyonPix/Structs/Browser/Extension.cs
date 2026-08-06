using System;

namespace PyonPix.Structs.Browser;

public class Extension
{
	public string CrxId { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public string Developer { get; set; } = string.Empty;

	public string Version { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;

	public bool IsDownloaded { get; set; }

	public bool IsUpdateAvailable { get; set; }

	public bool IsInstalled { get; set; }

	public bool IsEnabled { get; set; }
}

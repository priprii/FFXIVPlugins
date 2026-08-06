using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionManifest
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("author")]
	public string? Developer { get; set; }

	[JsonPropertyName("version_name")]
	public string? Version { get; set; }
}

using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionSearchRoot
{
	[JsonPropertyName("extensionList")]
	public ExtensionSearchResult[]? Results { get; set; }
}

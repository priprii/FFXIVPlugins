using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionAutoSuggestItem
{
	[JsonPropertyName("crxId")]
	public string? CrxId { get; set; }

	[JsonPropertyName("title")]
	public string? Title { get; set; }
}

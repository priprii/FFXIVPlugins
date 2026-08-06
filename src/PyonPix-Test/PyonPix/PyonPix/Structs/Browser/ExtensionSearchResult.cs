using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionSearchResult
{
	[JsonPropertyName("crxId")]
	public string CrxId { get; set; } = "";
}

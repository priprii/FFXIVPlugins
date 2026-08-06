using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionAutoCompleteResult
{
	[JsonPropertyName("autoCompleteList")]
	public string[]? AutoCompleteList { get; set; }

	[JsonPropertyName("autoSuggestExtensionsList")]
	public ExtensionAutoSuggestItem[]? AutoSuggestExtensionsList { get; set; }
}

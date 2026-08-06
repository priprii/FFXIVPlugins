using System.Text.Json.Serialization;

namespace PyonPix.Structs.Browser;

public class ExtensionProductDetails
{
	[JsonPropertyName("crxId")]
	public string? CrxId { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("shortDescription")]
	public string? ShortDescription { get; set; }

	[JsonPropertyName("developer")]
	public string? DeveloperName { get; set; }

	[JsonPropertyName("version")]
	public string? Version { get; set; }

	[JsonPropertyName("lastUpdateDate")]
	public double? LastUpdateDate { get; set; }

	[JsonPropertyName("activeInstallCount")]
	public uint? InstallCount { get; set; }

	[JsonPropertyName("averageRating")]
	public float? Rating { get; set; }

	[JsonPropertyName("ratingCount")]
	public uint? RatingCount { get; set; }
}

namespace Ktisis.Data.Config.Props;

public record PropEntry
{
	public string Item = string.Empty;

	public int Model;

	public int Submodel;

	public int Variant;

	public string Description = string.Empty;
}

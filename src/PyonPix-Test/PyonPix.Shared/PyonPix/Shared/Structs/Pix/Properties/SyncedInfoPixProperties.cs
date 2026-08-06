namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedInfoPixProperties : ISynced<InfoPixProperties>
{
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public PixType Type { get; set; }

	public void ApplyTo(InfoPixProperties target)
	{
		target.Name = Name;
		target.Description = Description;
		target.Type = Type;
	}
}

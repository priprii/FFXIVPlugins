namespace PyonPix.Shared.Structs.Pix.Properties;

public class InfoPixProperties : ILocal<SyncedInfoPixProperties>
{
	public string Name = string.Empty;

	public string Description = string.Empty;

	public PixType Type;

	public SyncedInfoPixProperties ToSynced()
	{
		return new SyncedInfoPixProperties
		{
			Name = Name,
			Description = Description,
			Type = Type
		};
	}
}

using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Sync.Dto;

namespace PyonPix.Shared.Structs.Pix;

public interface IPix
{
	int Version { get; set; }

	string Id { get; set; }

	InfoPixProperties Info { get; }

	BrowserPixProperties Browser { get; }

	TerritoryPixProperties Territory { get; }

	RendererPixProperties Renderer { get; }

	LightPixProperties Light { get; }

	AudioPixProperties Audio { get; }

	SyncPixProperties Sync { get; }

	string GetDisplayName();

	SyncedPixMetaDto GetSyncedMetaData();
}

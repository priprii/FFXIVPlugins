using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Sync.Dto;

public class SyncedPixUpdateRendererProperties(string pixId, SyncedRendererPixProperties renderer) : BaseSyncedPixUpdate(pixId)
{
	public override PixUpdateType UpdateType => PixUpdateType.RendererProperties;

	public SyncedRendererPixProperties Renderer { get; set; } = renderer;
}

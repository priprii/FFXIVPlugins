using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Shared.Structs.Pix;

public class Pix
{
	public int Version { get; set; }

	public InfoPixProperties Info { get; set; } = new InfoPixProperties();

	public BrowserPixProperties Browser { get; set; } = new BrowserPixProperties();

	public TerritoryPixProperties Territory { get; set; } = new TerritoryPixProperties();

	public RendererPixProperties Renderer { get; set; } = new RendererPixProperties();

	public LightPixProperties Light { get; set; } = new LightPixProperties();

	public AudioPixProperties Audio { get; set; } = new AudioPixProperties();
}

using PyonPix.Config.Global.Properties;

namespace PyonPix.Config.Global;

public class GlobalProperties
{
	public GeneralGlobalProperties General { get; set; } = new GeneralGlobalProperties();

	public BrowserGlobalProperties Browser { get; set; } = new BrowserGlobalProperties();

	public RendererGlobalProperties Renderer { get; set; } = new RendererGlobalProperties();

	public LightGlobalProperties Light { get; set; } = new LightGlobalProperties();

	public AudioGlobalProperties Audio { get; set; } = new AudioGlobalProperties();
}

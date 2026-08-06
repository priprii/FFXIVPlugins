using SharpDX.Direct3D11;

namespace PyonPix.Structs.Renderer;

public class DSVItem(DepthStencilView dsv)
{
	public DepthStencilView DSV { get; set; } = dsv;

	public Texture2D? Texture { get; set; }

	public Texture2DDescription Desc { get; set; }
}

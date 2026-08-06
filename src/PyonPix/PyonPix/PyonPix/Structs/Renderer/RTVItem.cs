using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace PyonPix.Structs.Renderer;

public class RTVItem(RenderTargetView rtv)
{
	public int Index { get; set; }

	public RenderTargetView RTV { get; set; } = rtv;

	public int Width { get; set; }

	public int Height { get; set; }

	public Format Format { get; set; }

	public bool IsBound { get; set; }

	public ulong Calls { get; set; }

	public ulong PairedCalls { get; set; }

	public ulong LastPresent { get; set; }
}

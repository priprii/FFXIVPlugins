using SharpDX.Direct3D11;

namespace PyonPix.Config.Global.Properties;

public class RendererGlobalProperties
{
	public FormatType Format;

	public ResourceBindingType ResourceBindingType;

	public DepthMode DepthMode;

	public RenderMode RenderMode;

	public bool UseShaderTarget;

	public bool IsBlendEnabled = true;

	public bool AlphaToCoverageEnable;

	public bool IndependentBlendEnable;

	public BlendOption SourceBlend = BlendOption.SourceAlpha;

	public BlendOption DestinationBlend = BlendOption.InverseSourceAlpha;

	public BlendOperation BlendOperation = BlendOperation.Add;

	public BlendOption SourceAlphaBlend = BlendOption.One;

	public BlendOption DestinationAlphaBlend = BlendOption.Zero;

	public BlendOperation AlphaBlendOperation = BlendOperation.Add;

	public ColorWriteMaskFlags RenderTargetWriteMask = ColorWriteMaskFlags.All;
}

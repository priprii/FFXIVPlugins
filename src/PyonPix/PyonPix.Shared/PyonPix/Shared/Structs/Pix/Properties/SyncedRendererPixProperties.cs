using PyonPix.Shared.Structs.Renderer;
using PyonPix.Shared.Utility;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class SyncedRendererPixProperties : ISynced<RendererPixProperties>
{
	public MathUtil.SyncedVector3 Position { get; set; }

	public MathUtil.SyncedQuaternion Rotation { get; set; }

	public MathUtil.SyncedVector3 Scale { get; set; }

	public MathUtil.SyncedVector4 ScreenTint { get; set; }

	public MathUtil.SyncedVector4 EdgeColour { get; set; }

	public MathUtil.SyncedVector4 BackColour { get; set; }

	public float BorderWidthH { get; set; }

	public float BorderWidthV { get; set; }

	public MathUtil.SyncedVector4 BorderColour { get; set; }

	public BorderMode BorderMode { get; set; }

	public float BorderFeather { get; set; }

	public float EdgeFeather { get; set; }

	public bool Depth { get; set; }

	public float DepthOffset { get; set; }

	public DepthComparison DepthComparison { get; set; }

	public CullMode CullMode { get; set; }

	public void ApplyTo(RendererPixProperties target)
	{
		target.Position = Position.ToLocal();
		target.Rotation = Rotation.ToLocal();
		target.Scale = Scale.ToLocal();
		target.ScreenTint = ScreenTint.ToLocal();
		target.EdgeColour = EdgeColour.ToLocal();
		target.BackColour = BackColour.ToLocal();
		target.BorderWidthH = BorderWidthH;
		target.BorderWidthV = BorderWidthV;
		target.BorderColour = BorderColour.ToLocal();
		target.BorderMode = BorderMode;
		target.BorderFeather = BorderFeather;
		target.EdgeFeather = EdgeFeather;
		target.Depth = Depth;
		target.DepthOffset = DepthOffset;
		target.DepthComparison = DepthComparison;
		target.CullMode = CullMode;
	}
}

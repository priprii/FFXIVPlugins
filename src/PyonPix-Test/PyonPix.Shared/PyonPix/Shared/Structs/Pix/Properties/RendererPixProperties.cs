using System.Numerics;
using PyonPix.Shared.Structs.Renderer;
using PyonPix.Shared.Utility;

namespace PyonPix.Shared.Structs.Pix.Properties;

public class RendererPixProperties : ILocal<SyncedRendererPixProperties>
{
	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Scale;

	public Vector4 ScreenTint = Vector4.One;

	public Vector4 EdgeColour = new Vector4(0.01f, 0.01f, 0.01f, 1f);

	public Vector4 BackColour = new Vector4(0.01f, 0.01f, 0.01f, 1f);

	public float BorderWidthH;

	public float BorderWidthV;

	public Vector4 BorderColour = new Vector4(0.01f, 0.01f, 0.01f, 1f);

	public BorderMode BorderMode;

	public float BorderFeather = 2f;

	public float EdgeFeather;

	public bool Depth = true;

	public float DepthOffset = 0.1f;

	public DepthComparison DepthComparison;

	public CullMode CullMode = CullMode.Back;

	public SyncedRendererPixProperties ToSynced()
	{
		return new SyncedRendererPixProperties
		{
			Position = Position.ToSynced(),
			Rotation = Rotation.ToSynced(),
			Scale = Scale.ToSynced(),
			ScreenTint = ScreenTint.ToSynced(),
			EdgeColour = EdgeColour.ToSynced(),
			BackColour = BackColour.ToSynced(),
			BorderWidthH = BorderWidthH,
			BorderWidthV = BorderWidthV,
			BorderColour = BorderColour.ToSynced(),
			BorderMode = BorderMode,
			BorderFeather = BorderFeather,
			EdgeFeather = EdgeFeather,
			Depth = Depth,
			DepthOffset = DepthOffset,
			DepthComparison = DepthComparison,
			CullMode = CullMode
		};
	}
}

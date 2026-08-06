using System;
using System.Numerics;
using System.Text.Json.Serialization;
using PyonPix.Shared.Structs.Renderer;

namespace PyonPix.Shared.Structs.Pix.Properties;

[Serializable]
public class RendererPixVariantOverrides
{
	public Vector3? Position;

	public Quaternion? Rotation;

	public Vector3? Scale;

	public Vector4? ScreenTint;

	public Vector4? EdgeColour;

	public Vector4? BackColour;

	public float? BorderWidthH;

	public float? BorderWidthV;

	public Vector4? BorderColour;

	public BorderMode? BorderMode;

	public float? BorderFeather;

	public float? EdgeFeather;

	public bool? Depth;

	public float? DepthOffset;

	public DepthComparison? DepthComparison;

	public CullMode? CullMode;

	[JsonIgnore]
	public bool HasAny
	{
		get
		{
			if (!Position.HasValue && !Rotation.HasValue && !Scale.HasValue && !ScreenTint.HasValue && !EdgeColour.HasValue && !BackColour.HasValue && !BorderWidthH.HasValue && !BorderWidthV.HasValue && !BorderColour.HasValue && !BorderMode.HasValue && !BorderFeather.HasValue && !EdgeFeather.HasValue && !Depth.HasValue && !DepthOffset.HasValue && !DepthComparison.HasValue)
			{
				return CullMode.HasValue;
			}
			return true;
		}
	}

	public void ApplyTo(RendererPixProperties target)
	{
		if (Position.HasValue)
		{
			target.Position = Position.Value;
		}
		if (Rotation.HasValue)
		{
			target.Rotation = Rotation.Value;
		}
		if (Scale.HasValue)
		{
			target.Scale = Scale.Value;
		}
		if (ScreenTint.HasValue)
		{
			target.ScreenTint = ScreenTint.Value;
		}
		if (EdgeColour.HasValue)
		{
			target.EdgeColour = EdgeColour.Value;
		}
		if (BackColour.HasValue)
		{
			target.BackColour = BackColour.Value;
		}
		if (BorderWidthH.HasValue)
		{
			target.BorderWidthH = BorderWidthH.Value;
		}
		if (BorderWidthV.HasValue)
		{
			target.BorderWidthV = BorderWidthV.Value;
		}
		if (BorderColour.HasValue)
		{
			target.BorderColour = BorderColour.Value;
		}
		if (BorderMode.HasValue)
		{
			target.BorderMode = BorderMode.Value;
		}
		if (BorderFeather.HasValue)
		{
			target.BorderFeather = BorderFeather.Value;
		}
		if (EdgeFeather.HasValue)
		{
			target.EdgeFeather = EdgeFeather.Value;
		}
		if (Depth.HasValue)
		{
			target.Depth = Depth.Value;
		}
		if (DepthOffset.HasValue)
		{
			target.DepthOffset = DepthOffset.Value;
		}
		if (DepthComparison.HasValue)
		{
			target.DepthComparison = DepthComparison.Value;
		}
		if (CullMode.HasValue)
		{
			target.CullMode = CullMode.Value;
		}
	}
}

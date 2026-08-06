using System;

namespace PyonCam.Config.Cam;

public class CameraConfigPreset
{
	public Guid ID;

	public string Name;

	public bool UseStartOnLogin;

	public float MinZoom = 1.5f;

	public float MaxZoom = 20f;

	public float ZoomDelta = 0.75f;

	public float MinFoV = 0.69f;

	public float MaxFoV = 0.78f;

	public float FoVDelta = (float)Math.PI / 36f;

	public float MinVRotation = -1.48353f;

	public float MaxVRotation = 0.785398f;

	public float HeightOffset;

	public float SideOffset;

	public float Tilt;

	public float LookAtHeightOffset;

	public bool EnablePoV;

	public bool PoVRotation;

	public float PoVFoV = 1.4f;

	public float PoVHeightOffset;

	public float PoVForwardOffset = -0.02f;

	public float PoVSideOffset;

	public float PoVMinVRotation = -1f;

	public float PoVMaxVRotation = 1f;

	public CameraConfigPreset(string name = "New Preset")
	{
		ID = Guid.NewGuid();
		Name = name;
	}
}

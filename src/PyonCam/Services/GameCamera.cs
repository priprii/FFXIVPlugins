using System.Runtime.InteropServices;

namespace PyonCam.Services;

[StructLayout(LayoutKind.Explicit)]
public struct GameCamera
{
	[FieldOffset(0)]
	public unsafe nint* vtbl;

	[FieldOffset(96)]
	public float x;

	[FieldOffset(100)]
	public float y;

	[FieldOffset(104)]
	public float z;

	[FieldOffset(144)]
	public float lookAtX;

	[FieldOffset(148)]
	public float lookAtY;

	[FieldOffset(152)]
	public float lookAtZ;

	[FieldOffset(292)]
	public float currentZoom;

	[FieldOffset(296)]
	public float minZoom;

	[FieldOffset(300)]
	public float maxZoom;

	[FieldOffset(304)]
	public float currentFoV;

	[FieldOffset(308)]
	public float minFoV;

	[FieldOffset(312)]
	public float maxFoV;

	[FieldOffset(316)]
	public float addedFoV;

	[FieldOffset(320)]
	public float currentHRotation;

	[FieldOffset(324)]
	public float currentVRotation;

	[FieldOffset(328)]
	public float hRotationDelta;

	[FieldOffset(344)]
	public float minVRotation;

	[FieldOffset(348)]
	public float maxVRotation;

	[FieldOffset(368)]
	public float tilt;

	[FieldOffset(384)]
	public int mode;

	[FieldOffset(388)]
	public int controlType;

	[FieldOffset(396)]
	public float interpolatedZoom;

	[FieldOffset(416)]
	public float transition;

	[FieldOffset(448)]
	public float viewX;

	[FieldOffset(452)]
	public float viewY;

	[FieldOffset(456)]
	public float viewZ;

	[FieldOffset(500)]
	public byte isFlipped;

	[FieldOffset(556)]
	public float interpolatedY;

	[FieldOffset(564)]
	public float lookAtHeightOffset;

	[FieldOffset(568)]
	public byte resetLookatHeightOffset;

	[FieldOffset(576)]
	public float interpolatedLookAtHeightOffset;

	[FieldOffset(704)]
	public byte lockPosition;

	[FieldOffset(724)]
	public float lookAtY2;
}

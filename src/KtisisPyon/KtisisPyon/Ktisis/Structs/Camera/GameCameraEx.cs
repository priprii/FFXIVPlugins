using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Ktisis.Common.Utility;

namespace Ktisis.Structs.Camera;

[StructLayout(LayoutKind.Explicit)]
public struct GameCameraEx
{
	[FieldOffset(0)]
	public Camera GameCamera;

	[FieldOffset(96)]
	public Vector3 Position;

	[FieldOffset(292)]
	public float Distance;

	[FieldOffset(296)]
	public float DistanceMin;

	[FieldOffset(300)]
	public float DistanceMax;

	[FieldOffset(316)]
	public float Zoom;

	[FieldOffset(320)]
	public Vector2 Angle;

	[FieldOffset(348)]
	public float YMin;

	[FieldOffset(344)]
	public float YMax;

	[FieldOffset(352)]
	public Vector2 Pan;

	[FieldOffset(368)]
	public float Rotation;

	[FieldOffset(536)]
	public Vector2 DistanceCollide;

	public unsafe RenderCameraEx* RenderEx => (RenderCameraEx*)GameCamera.CameraBase.SceneCamera.RenderCamera;

	public Quaternion CalcPointDirection()
	{
		return (new Vector3(0f - (Angle.Y + Pan.Y), (Angle.X + 3.14159f) % 6.28319f - Pan.X, 0f) * MathHelpers.Rad2Deg).EulerAnglesToQuaternion();
	}

	public Vector3 CalcRotation()
	{
		return new Vector3(Angle.X - Pan.X, 0f - Angle.Y - Pan.Y, Rotation);
	}

	public unsafe static GameCameraEx* GetActive()
	{
		return (GameCameraEx*)((CameraManager)CameraManager.Instance()).GetActiveCamera();
	}
}

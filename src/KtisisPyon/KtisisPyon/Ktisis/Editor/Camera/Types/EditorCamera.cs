using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Structs.Camera;

namespace Ktisis.Editor.Camera.Types;

public class EditorCamera
{
	protected readonly ICameraManager Manager;

	public string Name = string.Empty;

	public CameraFlags Flags;

	public ushort? OrbitTarget;

	public Vector3? FixedPosition;

	public Vector3 RelativeOffset = Vector3.Zero;

	public float OrthographicZoom = 10f;

	public TrackingMode Tracking = TrackingMode.None;

	public bool IsTracking;

	public virtual nint Address { get; set; } = IntPtr.Zero;

	public bool IsValid
	{
		get
		{
			if (Manager.IsValid)
			{
				return Address != IntPtr.Zero;
			}
			return false;
		}
	}

	public bool IsDefault => Flags.HasFlag(CameraFlags.DefaultCamera);

	public bool IsNoCollide => Flags.HasFlag(CameraFlags.NoCollide);

	public bool IsOrthographic => Flags.HasFlag(CameraFlags.Orthographic);

	public bool IsDelimited => Flags.HasFlag(CameraFlags.Delimit);

	public List<BoneNode> Target { get; set; }

	public unsafe Camera* GameCamera => (Camera*)Address;

	public unsafe GameCameraEx* Camera => (GameCameraEx*)Address;

	public EditorCamera(ICameraManager manager)
	{
		Manager = manager;
		Target = new List<BoneNode>();
	}

	public void SetActive()
	{
		SetOrthographic(IsOrthographic);
	}

	public unsafe virtual Vector3? GetPosition()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Camera* gameCamera = GameCamera;
		if (gameCamera == null)
		{
			return null;
		}
		return FixedPosition ?? Vector3.op_Implicit(((Object)(&((Camera)(&((CameraBase)(&((Camera)gameCamera).CameraBase)).SceneCamera)).Object)).Position);
	}

	public unsafe virtual void WritePosition()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Camera* gameCamera = GameCamera;
		if (gameCamera != null)
		{
			Camera* num = &((CameraBase)(&((Camera)gameCamera).CameraBase)).SceneCamera;
			Vector3 position = ((Object)(&((Camera)num).Object)).Position;
			Vector3 val = Vector3.op_Implicit(GetPosition().Value + RelativeOffset);
			Unsafe.Write(&((Object)(&((Camera)num).Object)).Position, val);
			Vector3* lookAtVector = &((Camera)num).LookAtVector;
			Unsafe.Write(lookAtVector, *lookAtVector + (val - position));
			RenderCameraEx* renderEx = Camera->RenderEx;
			if (renderEx != null && IsOrthographic)
			{
				renderEx->OrthographicZoom = OrthographicZoom;
			}
		}
	}

	public unsafe void SetDelimited(bool delimit)
	{
		if (delimit)
		{
			Flags |= CameraFlags.Delimit;
		}
		else
		{
			Flags &= ~CameraFlags.Delimit;
		}
		GameCameraEx* camera = Camera;
		if (camera != null)
		{
			float max = (camera->DistanceMax = (delimit ? 350f : 20f));
			camera->DistanceMin = (delimit ? 0f : 1.5f);
			camera->Distance = Math.Clamp(camera->Distance, 0f, max);
			camera->YMin = (delimit ? 1.5f : 1.25f);
			camera->YMax = (delimit ? (-1.5f) : (-1.4f));
		}
	}

	public unsafe void SetOrthographic(bool enabled)
	{
		if (enabled)
		{
			Flags |= CameraFlags.Orthographic;
		}
		else
		{
			Flags &= ~CameraFlags.Orthographic;
		}
		RenderCameraEx* renderEx = Camera->RenderEx;
		if (renderEx != null)
		{
			renderEx->OrthographicEnabled = enabled;
			renderEx->OrthographicZoom = (enabled ? OrthographicZoom : 10f);
		}
	}

	public unsafe void ResetState()
	{
		if (Camera != null)
		{
			SetDelimited(delimit: false);
			SetOrthographic(enabled: false);
		}
	}
}

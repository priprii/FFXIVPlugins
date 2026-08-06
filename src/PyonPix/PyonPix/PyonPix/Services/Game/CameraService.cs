using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace PyonPix.Services.Game;

public static class CameraService
{
	public unsafe static Camera* GetGameCamera()
	{
		CameraManager* ptr = CameraManager.Instance();
		if (ptr == null)
		{
			return null;
		}
		return ((CameraManager)ptr).GetActiveCamera();
	}

	public unsafe static Camera* GetSceneCamera()
	{
		Camera* gameCamera = GetGameCamera();
		if (gameCamera == null)
		{
			return null;
		}
		return &((CameraBase)(&((Camera)gameCamera).CameraBase)).SceneCamera;
	}

	public unsafe static Camera* GetRenderCamera()
	{
		Camera* sceneCamera = GetSceneCamera();
		if (sceneCamera == null)
		{
			return null;
		}
		return ((Camera)sceneCamera).RenderCamera;
	}

	public unsafe static Matrix4x4 GetProjectionMatrixForGizmo()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Camera* renderCamera = GetRenderCamera();
		if (renderCamera == null)
		{
			return Matrix4x4.Identity;
		}
		Matrix4x4 val = ((Camera)renderCamera).ProjectionMatrix * 1f;
		float farPlane = ((Camera)renderCamera).FarPlane;
		float nearPlane = ((Camera)renderCamera).NearPlane;
		float num = farPlane / (farPlane - nearPlane);
		val.M43 = 0f - num * nearPlane;
		val.M33 = 0f - (farPlane + nearPlane) / (farPlane - nearPlane);
		return Matrix4x4.op_Implicit(val);
	}

	public unsafe static Matrix4x4 GetProjectionMatrix()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Camera* renderCamera = GetRenderCamera();
		if (renderCamera != null)
		{
			return Matrix4x4.op_Implicit(((Camera)renderCamera).ProjectionMatrix);
		}
		return Matrix4x4.Identity;
	}

	public unsafe static Matrix4x4 GetViewMatrix()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Camera* sceneCamera = GetSceneCamera();
		if (sceneCamera == null)
		{
			return Matrix4x4.Identity;
		}
		Matrix4x4 viewMatrix = ((Camera)sceneCamera).ViewMatrix;
		viewMatrix.M44 = 1f;
		return Matrix4x4.op_Implicit(viewMatrix);
	}
}

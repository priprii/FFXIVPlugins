using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace Ktisis.Services.Game;

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

	public unsafe static Matrix4x4? GetProjectionMatrix()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Camera* renderCamera = GetRenderCamera();
		if (renderCamera == null)
		{
			return null;
		}
		Matrix4x4 projectionMatrix = ((Camera)renderCamera).ProjectionMatrix;
		float farPlane = ((Camera)renderCamera).FarPlane;
		float nearPlane = ((Camera)renderCamera).NearPlane;
		float num = farPlane / (farPlane - nearPlane);
		projectionMatrix.M33 = 0f - (farPlane + nearPlane) / (farPlane - nearPlane);
		projectionMatrix.M43 = 0f - num * nearPlane;
		return Matrix4x4.op_Implicit(projectionMatrix);
	}

	public unsafe static Matrix4x4? GetViewMatrix()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Camera* sceneCamera = GetSceneCamera();
		if (sceneCamera == null)
		{
			return null;
		}
		Matrix4x4 viewMatrix = ((Camera)sceneCamera).ViewMatrix;
		viewMatrix.M44 = 1f;
		return Matrix4x4.op_Implicit(viewMatrix);
	}

	public unsafe static bool WorldToScreen(Camera* camera, Vector3 worldPos, out Vector2 screenPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 val = ((Camera)camera).ViewMatrix;
		if (((Camera)((Camera)camera).RenderCamera).IsOrtho)
		{
			Matrix4x4 val2 = val;
			val2.M44 = 1f;
			val = val2;
		}
		Vector3 screenPos2;
		bool result = WorldToScreenDepth(Matrix4x4.op_Implicit(val * ((Camera)((Camera)camera).RenderCamera).ProjectionMatrix), worldPos, out screenPos2);
		screenPos = new Vector2(screenPos2.X, screenPos2.Y);
		return result;
	}

	private static bool WorldToScreenDepth(Matrix4x4 m, Vector3 v, out Vector3 screenPos)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		float num = m.M11 * v.X + m.M21 * v.Y + m.M31 * v.Z + m.M41;
		float num2 = m.M12 * v.X + m.M22 * v.Y + m.M32 * v.Z + m.M42;
		float num3 = m.M14 * v.X + m.M24 * v.Y + m.M34 * v.Z + m.M44;
		ImGuiViewportPtr mainViewport = ImGuiHelpers.MainViewport;
		float num4 = ((ImGuiViewportPtr)(ref mainViewport)).Size.X / 2f;
		float num5 = ((ImGuiViewportPtr)(ref mainViewport)).Size.Y / 2f;
		screenPos = new Vector3(num4 + num4 * num / num3 + ((ImGuiViewportPtr)(ref mainViewport)).Pos.X, num5 - num5 * num2 / num3 + ((ImGuiViewportPtr)(ref mainViewport)).Pos.Y, num3);
		return num3 > 0.001f;
	}
}

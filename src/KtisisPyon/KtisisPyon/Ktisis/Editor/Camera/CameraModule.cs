using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Interop;
using Ktisis.Editor.Actions.Input;
using Ktisis.Editor.Camera.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Structs.Camera;
using Ktisis.Structs.Input;

namespace Ktisis.Editor.Camera;

public class CameraModule : HookModule
{
	private unsafe delegate Matrix4x4* LoadMatrixDelegate(RenderCameraEx* camera, Matrix4x4* matrix);

	private delegate nint CameraControlDelegate(nint a1);

	private delegate nint CameraPreUpdateDelegate(nint a1);

	private unsafe delegate nint CalcViewMatrixDelegate(Camera* camera);

	private unsafe delegate void UpdateInputDelegate(InputDeviceManager* mgr, nint a2, void* controller, MouseDeviceData* mouseData, KeyboardDeviceData* keyData);

	private unsafe delegate nint CameraCollideDelegate(Camera* a1, Vector3* a2, Vector3* a3, float a4, nint a5, float a6);

	private unsafe delegate Camera* ActiveCameraDelegate(nint a1);

	private delegate char CameraEventDelegate(nint a1, nint a2, int a3);

	private delegate void CameraUiDelegate(nint a1);

	private unsafe delegate float* CameraCalculateLookPositionDelegate(Camera* pointer, float* lookAtVector, float* cameraPosition, char cameraMode);

	private delegate nint CameraTargetDelegate(nint a1);

	private unsafe class CameraRedirect(int index) : IDisposable
	{
		public unsafe Camera* Value = null;

		public unsafe void Dispose()
		{
			if (Value != null)
			{
				Camera** ptr = (Camera**)CameraManager.Instance();
				ptr[index] = Value;
				Value = null;
			}
		}
	}

	private readonly CameraManager Manager;

	private readonly ISigScanner _sigScanner;

	private readonly IGameInteropProvider _interop;

	private readonly IObjectTable _objectTable;

	[Signature("E8 ?? ?? ?? ?? 48 8B 17 48 8D 4D E0")]
	private LoadMatrixDelegate _loadMatrix;

	[Signature("E8 ?? ?? ?? ?? 48 83 3D ?? ?? ?? ?? ?? 74 0C", DetourName = "CameraControlDetour")]
	private Hook<CameraControlDelegate> CameraControlHook;

	[Signature("8B 41 ?? 85 C0 74 ?? 83 F8 ?? 75 ?? 48 8B 41", DetourName = "CameraPreUpdateDetour")]
	private Hook<CameraPreUpdateDelegate> CameraPreUpdateHook;

	[Signature("48 89 5C 24 ?? 57 48 81 EC ?? ?? ?? ?? F6 81 ?? ?? ?? ?? ?? 48 8B D9 48 89 B4 24 ?? ?? ?? ??", DetourName = "CalcViewMatrixDetour")]
	private Hook<CalcViewMatrixDelegate> CalcViewMatrixHook;

	[Signature("E8 ?? ?? ?? ?? 83 7B 58 00", DetourName = "UpdateInputDetour")]
	private Hook<UpdateInputDelegate> UpdateInputHook;

	[Signature("48 8B C4 48 89 58 ?? 48 89 70 ?? 48 89 78 ?? 55 41 56 41 57 48 8D 68 ?? 48 81 EC ?? ?? ?? ?? F3 0F 58 1D", DetourName = "CameraCollideDetour")]
	private Hook<CameraCollideDelegate> CameraCollideHook;

	[Signature("E8 ?? ?? ?? ?? 45 32 FF 40 32 F6", DetourName = "ActiveCameraDetour")]
	private Hook<ActiveCameraDelegate> ActiveCameraHook;

	[Signature("E8 ?? ?? ?? ?? 0F B6 F8 EB 34", DetourName = "CameraEventDetour")]
	private Hook<CameraEventDelegate> CameraEventHook;

	[Signature("E8 ?? ?? ?? ?? 80 BB ?? ?? ?? ?? ?? 74 0D 8B 53 28", DetourName = "CameraUiDetour")]
	private Hook<CameraUiDelegate> CameraUiHook;

	[Signature("4C 8B DC 49 89 5B ?? 49 89 73 ?? 55 57 41 56 49 8D 6B ?? 48 81 EC ?? ?? ?? ?? 45 0F 29 4B", DetourName = "CameraCalculateLookPositionDetour")]
	private Hook<CameraCalculateLookPositionDelegate> CameraCalculateLookPositionHook;

	private Hook<CameraTargetDelegate>? CameraTargetHook;

	public CameraModule(IHookMediator hook, CameraManager manager, ISigScanner sigScanner, IGameInteropProvider interop, IObjectTable objectTable)
		: base(hook)
	{
		Manager = manager;
		_sigScanner = sigScanner;
		_interop = interop;
		_objectTable = objectTable;
	}

	public override bool Initialize()
	{
		InitVfHook();
		return base.Initialize();
	}

	private unsafe void InitVfHook()
	{
		nint num = default(nint);
		if (!_sigScanner.TryGetStaticAddressFromSig("48 8D 05 ?? ?? ?? ?? C7 83 ?? ?? ?? ?? ?? ?? ?? ?? 48 89 03 0F 57 C0 33 C0 48 C7 83 ?? ?? ?? ?? ?? ?? ?? ??", ref num, 0))
		{
			Ktisis.Log.Warning("Failed to find signature for CameraTarget hook!");
			return;
		}
		nint* ptr = (nint*)num;
		CameraTargetHook = _interop.HookFromAddress<CameraTargetDelegate>((IntPtr)ptr[18], (CameraTargetDelegate)CameraTargetDetour, (HookBackend)0);
	}

	public void Setup()
	{
		if (base.IsInit)
		{
			CameraControlHook.Enable();
			CameraCollideHook.Enable();
			CameraTargetHook?.Enable();
			CameraCalculateLookPositionHook.Enable();
		}
	}

	public void ChangeCamera(EditorCamera camera)
	{
		if (base.IsInit)
		{
			bool flag = !camera.IsDefault;
			Ktisis.Log.Verbose($"Updating redirect hooks: {flag}");
			if (flag)
			{
				ActiveCameraHook.Enable();
				CameraEventHook.Enable();
				CameraUiHook.Enable();
				CameraPreUpdateHook.Enable();
			}
			else
			{
				ActiveCameraHook.Disable();
				CameraEventHook.Disable();
				CameraUiHook.Disable();
				CameraPreUpdateHook.Disable();
			}
			if (camera is WorkCamera)
			{
				CalcViewMatrixHook.Enable();
				UpdateInputHook.Enable();
			}
			else
			{
				CalcViewMatrixHook.Disable();
				UpdateInputHook.Disable();
			}
			SetSceneCamera(camera);
			camera.SetActive();
		}
	}

	public unsafe IGameObject? ResolveOrbitTarget(EditorCamera camera)
	{
		if (camera.OrbitTarget.HasValue)
		{
			IGameObject val = _objectTable[(int)camera.OrbitTarget.Value];
			if (val != null)
			{
				return val;
			}
		}
		nint gPoseTarget = (nint)((TargetSystem)TargetSystem.Instance()).GPoseTarget;
		return _objectTable.CreateObjectReference((IntPtr)gPoseTarget);
	}

	private nint CameraControlDetour(nint a1)
	{
		nint result;
		using (Redirect())
		{
			result = CameraControlHook.Original(a1);
		}
		try
		{
			EditorCamera current = Manager.Current;
			if (current != null && current.IsValid)
			{
				current.WritePosition();
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle camera control:\n{value}");
			DisableAll();
		}
		return result;
	}

	private nint CameraPreUpdateDetour(nint a1)
	{
		using (Redirect())
		{
			return CameraPreUpdateHook.Original(a1);
		}
	}

	private unsafe nint CalcViewMatrixDetour(Camera* camera)
	{
		nint result = CalcViewMatrixHook.Original(camera);
		try
		{
			if (Manager.Current is WorkCamera workCamera)
			{
				workCamera.Update();
				Matrix4x4* ptr = (Matrix4x4*)(&((Camera)camera).ViewMatrix);
				*ptr = workCamera.CalculateViewMatrix();
				_loadMatrix(workCamera.Camera->RenderEx, ptr);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle work camera:\n{value}");
		}
		return result;
	}

	private unsafe void UpdateInputDetour(InputDeviceManager* mgr, nint a2, void* controller, MouseDeviceData* mouseData, KeyboardDeviceData* keyData)
	{
		UpdateInputHook.Original(mgr, a2, controller, mouseData, keyData);
		try
		{
			if (Manager.Current is WorkCamera workCamera && !InputManager.IsChatInputActive())
			{
				workCamera.UpdateControl(mouseData, keyData);
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle work camera input:\n{value}");
		}
	}

	private unsafe nint CameraCollideDetour(Camera* a1, Vector3* a2, Vector3* a3, float a4, nint a5, float a6)
	{
		EditorCamera current = Manager.Current;
		if (current != null && current.IsNoCollide && current.Camera != null)
		{
			float num = a4 + 0.001f;
			current.Camera->DistanceCollide.X = num;
			current.Camera->DistanceCollide.Y = num;
			return 0;
		}
		return CameraCollideHook.Original(a1, a2, a3, a4, a5, a6);
	}

	private unsafe Camera* ActiveCameraDetour(nint a1)
	{
		EditorCamera current = Manager.Current;
		if (current != null && current.IsValid)
		{
			return current.GameCamera;
		}
		return ActiveCameraHook.Original(a1);
	}

	private char CameraEventDetour(nint a1, nint a2, int a3)
	{
		using (Redirect(a3 == 5))
		{
			return CameraEventHook.Original(a1, a2, a3);
		}
	}

	private void CameraUiDetour(nint a1)
	{
		using (Redirect())
		{
			CameraUiHook.Original(a1);
		}
	}

	private unsafe float* CameraCalculateLookPositionDetour(Camera* pointer, float* targetPosition, float* cameraPosition, char mode)
	{
		EditorCamera? current = Manager.Current;
		if (current != null && current.Target.Count > 0 && Manager.Current.IsTracking)
		{
			Vector3 vector = CalculateAveragePosition(Manager.Current.Target);
			ActorEntity actorEntity = (ActorEntity)Manager.Current.Target.First().Root;
			switch (Manager.Current.Tracking)
			{
			case TrackingMode.Follow:
			{
				EditorCamera? current3 = Manager.Current;
				if (current3 != null)
				{
					current3.RelativeOffset = vector - actorEntity.Actor.Position;
				}
				EditorCamera? current4 = Manager.Current;
				if (current4 != null)
				{
					current4.RelativeOffset.Y = vector.Y - actorEntity.Actor.Position.Y - ((Vector3)(&((GameObject)actorEntity.CsGameObject).CameraOffset)).Y;
				}
				break;
			}
			case TrackingMode.Pan:
			{
				*targetPosition = vector.X;
				targetPosition[1] = vector.Y;
				targetPosition[2] = vector.Z;
				EditorCamera? current7 = Manager.Current;
				if (current7 != null)
				{
					current7.RelativeOffset = Vector3.Zero;
				}
				break;
			}
			case TrackingMode.FollowAndPan:
			{
				Vector3 position = actorEntity.Actor.Position;
				Vector3 value = vector;
				Vector3 position2 = actorEntity.Actor.Position;
				position2.Y = actorEntity.Actor.Position.Y + ((Vector3)(&((GameObject)actorEntity.CsGameObject).CameraOffset)).Y;
				Vector3 vector2 = Vector3.Lerp(position, value, Vector3.Hypot(Vector3.Normalize(position2), Vector3.Normalize(vector)).ToScalar() / float.RootN(2f, 2));
				EditorCamera? current5 = Manager.Current;
				if (current5 != null)
				{
					current5.RelativeOffset = vector2 - actorEntity.Actor.Position;
				}
				EditorCamera? current6 = Manager.Current;
				if (current6 != null)
				{
					current6.RelativeOffset.Y = 0f;
				}
				Vector3 vector3 = vector2 - actorEntity.Actor.Position;
				*targetPosition = vector.X - vector3.X;
				targetPosition[1] = vector.Y;
				targetPosition[2] = vector.Z - vector3.Z;
				break;
			}
			case TrackingMode.None:
			{
				EditorCamera? current2 = Manager.Current;
				if (current2 != null)
				{
					current2.RelativeOffset = Vector3.Zero;
				}
				break;
			}
			}
		}
		return CameraCalculateLookPositionHook.Original(pointer, targetPosition, cameraPosition, mode);
	}

	private Vector3 CalculateAveragePosition(List<BoneNode> points)
	{
		Vector3 vector = default(Vector3);
		foreach (BoneNode item in points.Where((BoneNode p) => p.GetTransform() != null))
		{
			vector += item.CalcTransformWorld().Position;
		}
		return vector / points.Count((BoneNode p) => p.GetTransform() != null);
	}

	private nint CameraTargetDetour(nint a1)
	{
		EditorCamera current = Manager.Current;
		if (current != null)
		{
			ushort? orbitTarget = current.OrbitTarget;
			if (orbitTarget.HasValue)
			{
				ushort valueOrDefault = orbitTarget.GetValueOrDefault();
				nint objectAddress = _objectTable.GetObjectAddress((int)valueOrDefault);
				if (objectAddress != IntPtr.Zero)
				{
					return objectAddress;
				}
			}
		}
		return CameraTargetHook.Original(a1);
	}

	private unsafe CameraRedirect Redirect(bool condition = true)
	{
		CameraManager* intPtr = CameraManager.Instance();
		int activeCameraIndex = ((CameraManager)intPtr).ActiveCameraIndex;
		Camera** ptr = (Camera**)intPtr;
		CameraRedirect cameraRedirect = new CameraRedirect(activeCameraIndex);
		if (!Manager.IsValid || !condition)
		{
			return cameraRedirect;
		}
		EditorCamera current = Manager.Current;
		if (current == null || current.IsDefault || current.GameCamera == null)
		{
			return cameraRedirect;
		}
		cameraRedirect.Value = ptr[activeCameraIndex];
		ptr[activeCameraIndex] = current.GameCamera;
		return cameraRedirect;
	}

	private unsafe static void SetSceneCamera(EditorCamera camera)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		CameraManager* ptr = CameraManager.Instance();
		((CameraManager)ptr).Cameras[((CameraManager)ptr).CameraIndex] = Pointer<Camera>.op_Implicit(&((CameraBase)(&((Camera)camera.GameCamera).CameraBase)).SceneCamera);
	}

	private unsafe static void ResetSceneCamera()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		CameraManager* ptr = CameraManager.Instance();
		Camera* activeCamera = ((CameraManager)CameraManager.Instance()).GetActiveCamera();
		((CameraManager)ptr).Cameras[((CameraManager)ptr).CameraIndex] = Pointer<Camera>.op_Implicit(&((CameraBase)(&((Camera)activeCamera).CameraBase)).SceneCamera);
	}

	public override void Dispose()
	{
		base.Dispose();
		ResetSceneCamera();
		GC.SuppressFinalize(this);
	}
}

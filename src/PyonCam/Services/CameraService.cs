using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Math;
using PyonCam.Config;
using PyonCam.Config.Cam;

namespace PyonCam.Services;

public class CameraService : IDisposable
{
	private unsafe delegate void CameraLookAtDelegate(GameCamera* camera, Vector3* lookAtPosition, Vector3* cameraPosition, Vector3* a4);

	private unsafe delegate void CameraPositionDelegate(GameCamera* camera, GameObject* target, Vector3* position, bool swapPerson);

	private delegate bool CameraChangeViewDelegate();

	private delegate float CameraZoomDeltaDelegate();

	private unsafe delegate GameObject* CameraTargetDelegate(GameCamera* camera);

	private unsafe delegate byte CameraAutoRotateModeDelegate(GameCamera* camera, Framework* framework);

	private unsafe delegate float CameraMaxMaintainDistanceDelegate(GameCamera* camera);

	private unsafe delegate bool CameraLookAtHeightOffsetDelegate(GameCamera* camera, GameObject* o, bool zero);

	private unsafe delegate bool CameraDisplayObjectDelegate(GameCamera* camera, GameObject* o, Vector3* cameraPosition, Vector3* cameraLookAt);

	private unsafe delegate bool WorldBonePositionDelegate(GameObject* o, uint bone, Vector3* outPosition);

	private readonly Configuration _config;

	private readonly IServiceContext _services;

	private bool IsSpectating;

	public bool SpectatingEnabled;

	public FreeCam FreeCam;

	public PoV PoV;

	public bool NoClipEnabled;

	public bool NoClipValid;

	private nint ClipAddr = IntPtr.Zero;

	private byte[] ClipOld;

	private byte[] ClipNew = new byte[5] { 48, 192, 144, 144, 144 };

	private unsafe float* FoVDeltaPtr;

	private Hook<CameraLookAtDelegate>? CameraLookAtHook;

	private Hook<CameraPositionDelegate>? CameraPositionHook;

	private Hook<CameraChangeViewDelegate>? CameraChangeViewHook;

	private Hook<CameraZoomDeltaDelegate>? CameraZoomDeltaHook;

	private Hook<CameraTargetDelegate>? CameraTargetHook;

	private Hook<CameraAutoRotateModeDelegate>? CameraAutoRotateModeHook;

	private CameraAutoRotateModeDelegate? CameraAutoRotateMode;

	private Hook<CameraMaxMaintainDistanceDelegate>? CameraMaxMaintainDistanceHook;

	private CameraMaxMaintainDistanceDelegate? CameraMaxMaintainDistance;

	private Hook<CameraLookAtHeightOffsetDelegate>? CameraLookAtHeightOffsetHook;

	private CameraLookAtHeightOffsetDelegate? CameraLookAtHeightOffset;

	private Hook<CameraDisplayObjectDelegate>? CameraDisplayObjectHook;

	private CameraDisplayObjectDelegate? CameraDisplayObject;

	private WorldBonePositionDelegate? WorldBonePosition;

	private PresetService PresetService => _services.Get<PresetService>();

	private ulong OrbitTargetId { get; set; }

	private unsafe CameraManager* CameraManager => (CameraManager*)CameraManager.Instance();

	public unsafe GameCamera* Camera
	{
		get
		{
			if (CameraManager != null)
			{
				return CameraManager->worldCamera;
			}
			return null;
		}
	}

	public unsafe GameCamera* MenuCamera
	{
		get
		{
			if (CameraManager != null)
			{
				return CameraManager->menuCamera;
			}
			return null;
		}
	}

	public unsafe float FoVDelta
	{
		get
		{
			if (FoVDeltaPtr == null)
			{
				return 0f;
			}
			return *FoVDeltaPtr;
		}
		set
		{
			if (FoVDeltaPtr != null)
			{
				*FoVDeltaPtr = value;
			}
		}
	}

	public CameraService(Configuration config, IServiceContext services)
	{
		_config = config;
		_services = services;
		FreeCam = new FreeCam(_config, _services);
		PoV = new PoV(_config, _services);
	}

	public unsafe void Initialize()
	{
		try
		{
			nint num = default(nint);
			if (!_services.SigScanner.TryGetStaticAddressFromSig("48 8D 05 ?? ?? ?? ?? C7 83 ?? ?? ?? ?? ?? ?? ?? ?? 48 89 03 0F 57 C0 33 C0 48 C7 83 ?? ?? ?? ?? ?? ?? ?? ??", ref num, 0))
			{
				_services.Log.Error("CameraService Failed: GameCamera Mismatch", Array.Empty<object>());
				return;
			}
			if (!_services.SigScanner.TryScanModule("E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? F3 0F 10 44 24 ?? 41 B7 01", ref ClipAddr))
			{
				_services.Log.Error("CameraService Failed: NoClip Mismatch", Array.Empty<object>());
				return;
			}
			nint foVDeltaPtr = default(nint);
			if (!_services.SigScanner.TryGetStaticAddressFromSig("F3 0F 59 35 ?? ?? ?? ?? F3 0F 10 45 ??", ref foVDeltaPtr, 0))
			{
				_services.Log.Error("CameraService Failed: FoVDelta Mismatch", Array.Empty<object>());
				return;
			}
			nint* ptr = (nint*)num;
			CameraLookAtHook = _services.GameInteropProvider.HookFromAddress<CameraLookAtDelegate>((IntPtr)ptr[15], (CameraLookAtDelegate)CameraLookAtDetour, (HookBackend)0);
			CameraLookAtHook?.Enable();
			CameraPositionHook = _services.GameInteropProvider.HookFromAddress<CameraPositionDelegate>((IntPtr)ptr[16], (CameraPositionDelegate)CameraPositionDetour, (HookBackend)0);
			CameraPositionHook?.Enable();
			CameraTargetHook = _services.GameInteropProvider.HookFromAddress<CameraTargetDelegate>((IntPtr)ptr[18], (CameraTargetDelegate)CameraTargetDetour, (HookBackend)0);
			CameraTargetHook?.Enable();
			CameraChangeViewHook = _services.GameInteropProvider.HookFromAddress<CameraChangeViewDelegate>((IntPtr)ptr[23], (CameraChangeViewDelegate)CameraChangeViewDetour, (HookBackend)0);
			CameraChangeViewHook?.Enable();
			CameraZoomDeltaHook = _services.GameInteropProvider.HookFromAddress<CameraZoomDeltaDelegate>((IntPtr)ptr[29], (CameraZoomDeltaDelegate)CameraZoomDeltaDetour, (HookBackend)0);
			CameraZoomDeltaHook?.Enable();
			if (ClipAddr != IntPtr.Zero && SafeMemory.ReadBytes((IntPtr)ClipAddr, ClipNew.Length, ref ClipOld))
			{
				NoClipValid = true;
				if (_config.EnableCameraNoClippy)
				{
					EnableNoClip();
				}
			}
			FoVDeltaPtr = (float*)foVDeltaPtr;
			nint num2 = default(nint);
			if (_services.SigScanner.TryScanText("E8 ?? ?? ?? ?? 48 8B CB 85 C0 0F 84 ?? ?? ?? ?? 83 E8 01", ref num2))
			{
				CameraAutoRotateMode = Marshal.GetDelegateForFunctionPointer<CameraAutoRotateModeDelegate>(num2);
				CameraAutoRotateModeHook = _services.GameInteropProvider.HookFromAddress<CameraAutoRotateModeDelegate>((IntPtr)num2, (CameraAutoRotateModeDelegate)CameraAutoRotateModeDetour, (HookBackend)0);
				CameraAutoRotateModeHook?.Enable();
			}
			if (_services.SigScanner.TryScanText("E8 ?? ?? ?? ?? F3 0F 5D 44 24 58", ref num2))
			{
				CameraMaxMaintainDistance = Marshal.GetDelegateForFunctionPointer<CameraMaxMaintainDistanceDelegate>(num2);
				CameraMaxMaintainDistanceHook = _services.GameInteropProvider.HookFromAddress<CameraMaxMaintainDistanceDelegate>((IntPtr)num2, (CameraMaxMaintainDistanceDelegate)CameraMaxMaintainDistanceDetour, (HookBackend)0);
				CameraMaxMaintainDistanceHook?.Enable();
			}
			if (_services.SigScanner.TryScanText("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 48 8B 02 48 8B F1 48 8B CA", ref num2))
			{
				CameraLookAtHeightOffset = Marshal.GetDelegateForFunctionPointer<CameraLookAtHeightOffsetDelegate>(num2);
				CameraLookAtHeightOffsetHook = _services.GameInteropProvider.HookFromAddress<CameraLookAtHeightOffsetDelegate>((IntPtr)num2, (CameraLookAtHeightOffsetDelegate)CameraLookAtHeightOffsetDetour, (HookBackend)0);
				CameraLookAtHeightOffsetHook?.Enable();
			}
			if (_services.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 75 18 48 8D 0D ?? ?? ?? ?? B3 01", ref num2))
			{
				CameraDisplayObject = Marshal.GetDelegateForFunctionPointer<CameraDisplayObjectDelegate>(num2);
				CameraDisplayObjectHook = _services.GameInteropProvider.HookFromAddress<CameraDisplayObjectDelegate>((IntPtr)num2, (CameraDisplayObjectDelegate)CameraDisplayObjectDetour, (HookBackend)0);
				CameraDisplayObjectHook?.Enable();
			}
			if (_services.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 74 61 F3 0F 10 44 24", ref num2))
			{
				WorldBonePosition = Marshal.GetDelegateForFunctionPointer<WorldBonePositionDelegate>(num2);
			}
		}
		catch (Exception value)
		{
			_services.Log.Error($"CameraService Failed: {value}", Array.Empty<object>());
		}
	}

	public void EnableNoClip()
	{
		if (!NoClipEnabled && NoClipValid)
		{
			SafeMemory.WriteBytes((IntPtr)ClipAddr, ClipNew);
			NoClipEnabled = true;
		}
	}

	public void DisableNoClip()
	{
		if (NoClipEnabled && NoClipValid)
		{
			SafeMemory.WriteBytes((IntPtr)ClipAddr, ClipOld);
			NoClipEnabled = false;
		}
	}

	public void ToggleNoClip()
	{
		if (NoClipEnabled)
		{
			DisableNoClip();
		}
		else
		{
			EnableNoClip();
		}
	}

	private unsafe void CameraLookAtDetour(GameCamera* camera, Vector3* lookAtPosition, Vector3* cameraPosition, Vector3* a4)
	{
		if (!FreeCam.Enabled)
		{
			CameraLookAtHook.Original(camera, lookAtPosition, cameraPosition, a4);
		}
	}

	private unsafe void CameraPositionDetour(GameCamera* camera, GameObject* target, Vector3* position, bool swapPerson)
	{
		if (FreeCam.Enabled)
		{
			*position = FreeCam.Position;
			return;
		}
		if (PoV.Enabled)
		{
			CameraConfigPreset currentPreset = PresetService.CurrentPreset;
			if (!PoV.Update(camera, target, (Vector3*)position, swapPerson, currentPreset))
			{
				CameraPositionHook.Original(camera, target, position, swapPerson);
			}
			return;
		}
		CameraPositionHook.Original(camera, target, position, swapPerson);
		if (Camera != null)
		{
			CameraConfigPreset currentPreset2 = PresetService.CurrentPreset;
			position->Y += currentPreset2.HeightOffset;
			if (currentPreset2.SideOffset != 0f && camera->mode == 1)
			{
				float x = Camera->currentHRotation - (float)Math.PI / 2f;
				position->X += (0f - currentPreset2.SideOffset) * MathF.Sin(x);
				position->Z += (0f - currentPreset2.SideOffset) * MathF.Cos(x);
			}
		}
	}

	private bool CameraChangeViewDetour()
	{
		return !FreeCam.Enabled;
	}

	private float CameraZoomDeltaDetour()
	{
		return PresetService.CurrentPreset.ZoomDelta;
	}

	public void SetOrbitTarget(ulong targetId)
	{
		OrbitTargetId = targetId;
	}

	public ulong GetOrbitTarget()
	{
		return OrbitTargetId;
	}

	public void RevertOrbitTarget()
	{
		OrbitTargetId = 0uL;
	}

	private unsafe GameObject* CameraTargetDetour(GameCamera* camera)
	{
		if (OrbitTargetId != 0L)
		{
			IGameObject val = _services.Objects.SearchById(OrbitTargetId);
			if (val != null && val.Address != (IntPtr)IntPtr.Zero)
			{
				IsSpectating = true;
				return (GameObject*)val.Address;
			}
			OrbitTargetId = 0uL;
		}
		if (SpectatingEnabled)
		{
			IGameObject focusTarget = _services.TargetManager.FocusTarget;
			if (focusTarget != null)
			{
				IsSpectating = true;
				return (GameObject*)focusTarget.Address;
			}
			IGameObject softTarget = _services.TargetManager.SoftTarget;
			if (softTarget != null)
			{
				IsSpectating = true;
				return (GameObject*)softTarget.Address;
			}
		}
		if (_config.DeathCamMode == DeathCamSetting.Spectate && _services.Condition[(ConditionFlag)2])
		{
			IGameObject target = _services.TargetManager.Target;
			if (target != null)
			{
				IsSpectating = true;
				return (GameObject*)target.Address;
			}
		}
		IsSpectating = false;
		return CameraTargetHook.Original(camera);
	}

	public unsafe byte CameraAutoRotateModeDetour(GameCamera* camera, Framework* framework)
	{
		return (byte)((FreeCam.Enabled || IsSpectating) ? 4 : CameraAutoRotateModeHook.Original(camera, framework));
	}

	public unsafe float CameraMaxMaintainDistanceDetour(GameCamera* camera)
	{
		float num = CameraMaxMaintainDistanceHook.Original(camera);
		if (!(num < 10f))
		{
			return camera->maxZoom;
		}
		return num;
	}

	public unsafe bool CameraLookAtHeightOffsetDetour(GameCamera* camera, GameObject* o, bool zero)
	{
		bool num = CameraLookAtHeightOffsetHook.Original(camera, o, zero);
		if (num && !zero)
		{
			IPlayerCharacter localPlayer = _services.Objects.LocalPlayer;
			if ((GameObject*?)o == (GameObject*?)((localPlayer != null) ? new nint?(((IGameObject)localPlayer).Address) : ((nint?)null)) && PresetService.CurrentPreset != PresetService.DefaultPreset)
			{
				camera->lookAtHeightOffset = PresetService.CurrentPreset.LookAtHeightOffset;
			}
		}
		return num;
	}

	public unsafe bool CameraDisplayObjectDetour(GameCamera* camera, GameObject* gameObject, Vector3* cameraPosition, Vector3* cameraLookAt)
	{
		if (_config.DisableCullingInGpose && _services.ClientState.IsGPosing)
		{
			PoV.Disable(camera);
			return true;
		}
		if (_config.DisableCullingInFreeCam && FreeCam.Enabled)
		{
			PoV.Disable(camera);
			return true;
		}
		CameraConfigPreset currentPreset = PresetService.CurrentPreset;
		if (currentPreset != null && currentPreset.EnablePoV && _services.Objects.LocalPlayer != null)
		{
			PoV.Toggle(camera);
			if (camera->mode == 0)
			{
				return true;
			}
		}
		else if (PoV.Enabled)
		{
			PoV.Disable(camera);
		}
		IPlayerCharacter localPlayer = _services.Objects.LocalPlayer;
		if ((GameObject*?)gameObject != (GameObject*?)((localPlayer != null) ? new nint?(((IGameObject)localPlayer).Address) : ((nint?)null)) || camera != Camera || camera->mode != 0 || (camera->transition != 0f && camera->controlType <= 2))
		{
			return CameraDisplayObjectHook.Original(camera, gameObject, cameraPosition, cameraLookAt);
		}
		return false;
	}

	public unsafe float? GetDefaultLookAtHeightOffset()
	{
		GameObject* value = ((ObjectArrays)(&((GameObjectManager)GameObjectManager.Instance()).Objects)).IndexSorted[0].Value;
		if (Camera == null || value == null)
		{
			return 0f;
		}
		float lookAtHeightOffset = Camera->lookAtHeightOffset;
		if (!CameraLookAtHeightOffsetHook.Original(Camera, value, zero: false))
		{
			return null;
		}
		float lookAtHeightOffset2 = Camera->lookAtHeightOffset;
		Camera->lookAtHeightOffset = lookAtHeightOffset;
		return lookAtHeightOffset2;
	}

	public unsafe Vector3 GetWorldBonePosition(GameObject* o, uint bone)
	{
		Vector3 zero = Vector3.Zero;
		WorldBonePosition?.Invoke(o, bone, &zero);
		return zero;
	}

	public unsafe void ApplyPreset(CameraConfigPreset? preset)
	{
		if (preset != null && Camera != null)
		{
			Camera->currentZoom = Math.Min(Math.Max(Camera->currentZoom, preset.MinZoom), preset.MaxZoom);
			Camera->minZoom = preset.MinZoom;
			Camera->maxZoom = preset.MaxZoom;
			Camera->currentFoV = Math.Min(Math.Max(Camera->currentFoV, preset.MinFoV), preset.MaxFoV);
			Camera->minFoV = preset.MinFoV;
			Camera->maxFoV = preset.MaxFoV;
			FoVDelta = preset.FoVDelta;
			Camera->minVRotation = preset.MinVRotation;
			Camera->maxVRotation = preset.MaxVRotation;
			Camera->tilt = preset.Tilt;
			Camera->lookAtHeightOffset = preset.LookAtHeightOffset;
		}
	}

	public unsafe void Update()
	{
		if (!_services.ClientState.IsLoggedIn || !_services.PlayerState.IsLoaded || FreeCam.Enabled || _services.Condition[(ConditionFlag)45])
		{
			return;
		}
		Character* address = (Character*)((IGameObject)_services.Objects.LocalPlayer).Address;
		try
		{
			if (_config.SelectedPresetID != Guid.Empty)
			{
				CameraConfigPreset cameraConfigPreset = _config.Presets.FirstOrDefault((CameraConfigPreset x) => x.ID == _config.SelectedPresetID);
				if (cameraConfigPreset != null && PresetService.CurrentPreset != cameraConfigPreset)
				{
					PresetService.CurrentPreset = cameraConfigPreset;
					return;
				}
			}
			if (RequiresReapplying(PresetService.CurrentPreset))
			{
				ApplyPreset(PresetService.CurrentPreset);
			}
		}
		catch (Exception value)
		{
			_services.Log.Warning($"{value}", Array.Empty<object>());
		}
	}

	private unsafe bool RequiresReapplying(CameraConfigPreset preset)
	{
		if (preset == null)
		{
			return false;
		}
		if (Camera == null)
		{
			return false;
		}
		if (Camera->minZoom != preset.MinZoom)
		{
			return true;
		}
		if (Camera->maxZoom != preset.MaxZoom)
		{
			return true;
		}
		if (Camera->lookAtHeightOffset != preset.LookAtHeightOffset)
		{
			return true;
		}
		return false;
	}

	public unsafe void Dispose()
	{
		ApplyPreset(PresetService.DefaultPreset);
		if (FreeCam.Enabled)
		{
			FreeCam.Toggle();
		}
		if (PoV.Enabled)
		{
			PoV.Toggle(Camera);
		}
		DisableNoClip();
		CameraLookAtHook?.Disable();
		CameraLookAtHook?.Dispose();
		CameraPositionHook?.Disable();
		CameraPositionHook?.Dispose();
		CameraTargetHook?.Disable();
		CameraTargetHook?.Dispose();
		CameraChangeViewHook?.Disable();
		CameraChangeViewHook?.Dispose();
		CameraZoomDeltaHook?.Disable();
		CameraZoomDeltaHook?.Dispose();
		CameraAutoRotateModeHook?.Disable();
		CameraAutoRotateModeHook?.Dispose();
		CameraMaxMaintainDistanceHook?.Disable();
		CameraMaxMaintainDistanceHook?.Dispose();
		CameraLookAtHeightOffsetHook?.Disable();
		CameraLookAtHeightOffsetHook?.Dispose();
		CameraDisplayObjectHook?.Disable();
		CameraDisplayObjectHook?.Dispose();
	}
}

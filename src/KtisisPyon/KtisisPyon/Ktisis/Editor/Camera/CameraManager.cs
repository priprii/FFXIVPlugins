using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;

namespace Ktisis.Editor.Camera;

public class CameraManager : ICameraManager, IDisposable
{
	private readonly IEditorContext _context;

	private readonly HookScope _scope;

	private readonly List<EditorCamera> CameraList = new List<EditorCamera>();

	public bool IsValid => _context.IsValid;

	private CameraModule? Module { get; set; }

	private EditorCamera? Active { get; set; }

	private EditorCamera? Default { get; set; }

	private WorkCamera? WorkCamera { get; set; }

	public bool IsWorkCameraActive { get; private set; }

	public EditorCamera? Current
	{
		get
		{
			if (IsWorkCameraActive)
			{
				WorkCamera workCamera = WorkCamera;
				if (workCamera != null && workCamera.IsValid)
				{
					return workCamera;
				}
			}
			EditorCamera active = Active;
			if (active != null && active.IsValid)
			{
				return active;
			}
			EditorCamera editorCamera = Default;
			if (editorCamera != null && editorCamera.IsValid)
			{
				return editorCamera;
			}
			return null;
		}
	}

	public CameraManager(IEditorContext context, HookScope scope)
	{
		_context = context;
		_scope = scope;
	}

	public void Initialize()
	{
		Ktisis.Log.Verbose("Initializing camera manager...");
		try
		{
			SetupCameras();
			Module = _scope.Create<CameraModule>(new object[1] { this });
			if (Module.Initialize())
			{
				Module.Setup();
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize camera manager:\n{value}");
		}
	}

	private unsafe void SetupCameras()
	{
		Camera* activeCamera = ((CameraManager)CameraManager.Instance()).GetActiveCamera();
		if (activeCamera != null)
		{
			EditorCamera obj = new EditorCamera(this)
			{
				Name = Ktisis.Locale.Translate("cameras.main"),
				Address = (nint)activeCamera,
				Flags = CameraFlags.DefaultCamera
			};
			EditorCamera active = obj;
			Default = obj;
			Active = active;
			CameraList.Add(Default);
		}
	}

	private void SetupWorkCamera()
	{
		if (WorkCamera == null)
		{
			WorkCamera obj = new WorkCamera(this, _context)
			{
				Name = Ktisis.Locale.Translate("cameras.work")
			};
			WorkCamera workCamera = obj;
			WorkCamera = obj;
		}
		if (!CopyOntoCamera(WorkCamera))
		{
			throw new Exception("Failed to setup work camera.");
		}
	}

	public IEnumerable<EditorCamera> GetCameras()
	{
		return CameraList;
	}

	public void SetCurrent(EditorCamera camera)
	{
		if (!camera.IsValid)
		{
			throw new Exception("Attempting to set invalid camera as current.");
		}
		if (Active != camera)
		{
			Active = camera;
			Module?.ChangeCamera(camera);
			if (camera != WorkCamera)
			{
				IsWorkCameraActive = false;
			}
		}
	}

	public void SetNext()
	{
		if (Current != null && CameraList.Contains(Current))
		{
			int index = (CameraList.IndexOf(Current) + 1) % CameraList.Count;
			SetCurrent(CameraList[index]);
		}
	}

	public void SetPrevious()
	{
		if (Current != null && CameraList.Contains(Current))
		{
			int num = CameraList.IndexOf(Current);
			int num2 = ((num > 0) ? num : CameraList.Count) - 1;
			if (num2 < CameraList.Count)
			{
				SetCurrent(CameraList[num2]);
			}
		}
	}

	public void SetWorkCameraMode(bool enabled)
	{
		if (IsWorkCameraActive == enabled)
		{
			return;
		}
		if (enabled)
		{
			SetupWorkCamera();
			Module?.ChangeCamera(WorkCamera);
			IsWorkCameraActive = true;
			return;
		}
		IsWorkCameraActive = false;
		EditorCamera active = Active;
		if (active != null && active.IsValid)
		{
			Module?.ChangeCamera(active);
		}
	}

	public void ToggleWorkCameraMode()
	{
		SetWorkCameraMode(!IsWorkCameraActive);
	}

	public KtisisCamera Create(CameraFlags flags = CameraFlags.None, bool setActive = true)
	{
		KtisisCamera ktisisCamera = new KtisisCamera(this)
		{
			Name = GetNextAvailableName(),
			Flags = flags
		};
		if (ktisisCamera.Address == IntPtr.Zero)
		{
			throw new Exception("Failed to allocate camera.");
		}
		if (!CopyOntoCamera(ktisisCamera))
		{
			throw new Exception("Failed to setup new camera.");
		}
		CameraList.Add(ktisisCamera);
		if (setActive)
		{
			SetCurrent(ktisisCamera);
		}
		return ktisisCamera;
	}

	public bool DeleteCurrent()
	{
		EditorCamera current = Current;
		if (current == null || !current.IsValid || current.IsDefault)
		{
			return false;
		}
		try
		{
			SetPrevious();
			CameraList.Remove(current);
			if (current is KtisisCamera ktisisCamera)
			{
				ktisisCamera.Dispose();
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"CameraManager.DeleteCurrent: {value}");
			return false;
		}
		return true;
	}

	private unsafe bool CopyOntoCamera(EditorCamera camera)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		EditorCamera current = Current;
		if (current == null || !current.IsValid || current == camera)
		{
			return false;
		}
		camera.OrbitTarget = current.OrbitTarget;
		camera.FixedPosition = current.FixedPosition;
		camera.RelativeOffset = current.RelativeOffset;
		Unsafe.Write(camera.GameCamera, *current.GameCamera);
		if (camera is WorkCamera workCamera)
		{
			workCamera.SetInitialPosition(current.GetPosition().Value, current.Camera->CalcRotation());
			workCamera.Camera->Zoom = 0f;
		}
		else
		{
			camera.Flags = current.Flags & ~CameraFlags.DefaultCamera;
		}
		camera.OrthographicZoom = current.OrthographicZoom;
		return true;
	}

	private string GetNextAvailableName()
	{
		for (int i = CameraList.Count + 1; i <= 100; i++)
		{
			string name = $"{Ktisis.Locale.Translate("cameras.camera")} #{i}";
			if (!CameraList.Any((EditorCamera camera) => camera.Name == name))
			{
				return name;
			}
		}
		return Ktisis.Locale.Translate("cameras.new");
	}

	public IGameObject? ResolveOrbitTarget(EditorCamera camera)
	{
		return Module?.ResolveOrbitTarget(camera);
	}

	private void ResetState()
	{
		Default?.ResetState();
		Active = null;
		WorkCamera?.Dispose();
		WorkCamera = null;
		CameraList.ForEach(delegate(EditorCamera cam)
		{
			if (cam is KtisisCamera ktisisCamera)
			{
				ktisisCamera.Dispose();
			}
		});
		CameraList.Clear();
	}

	public void Dispose()
	{
		try
		{
			Module?.Dispose();
			ResetState();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to dispose camera manager!\n{value}");
		}
		GC.SuppressFinalize(this);
	}
}

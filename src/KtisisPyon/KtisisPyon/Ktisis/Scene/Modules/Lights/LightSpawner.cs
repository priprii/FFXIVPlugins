using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Common.Utility;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Structs.Common;
using Ktisis.Structs.Lights;

namespace Ktisis.Scene.Modules.Lights;

public class LightSpawner : HookModule
{
	private unsafe delegate SceneLight* SceneLightCtorDelegate(SceneLight* self);

	private unsafe delegate bool SceneLightInitializeDelegate(SceneLight* self);

	private unsafe delegate nint SceneLightSetupDelegate(SceneLight* self);

	private unsafe delegate void CleanupRenderDelegate(SceneLight* light);

	private unsafe delegate void DestructorDelegate(SceneLight* light, bool a2);

	private readonly IFramework _framework;

	private readonly IEditorContext _context;

	private readonly HashSet<nint> _created = new HashSet<nint>();

	[Signature("E8 ?? ?? ?? ?? 48 89 84 FB ?? ?? ?? ?? 48 85 C0 0F 84 ?? ?? ?? ?? 48 8B C8")]
	private SceneLightCtorDelegate _sceneLightCtor;

	[Signature("E8 ?? ?? ?? ?? 48 8B 94 FB ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ??")]
	private SceneLightInitializeDelegate _sceneLightInit;

	[Signature("F6 41 38 01")]
	private SceneLightSetupDelegate _sceneLightSpawn;

	public LightSpawner(IHookMediator hook, IFramework framework, IEditorContext context)
		: base(hook)
	{
		_framework = framework;
		_context = context;
	}

	public void TryInitialize()
	{
		try
		{
			Initialize();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize light spawner:\n{value}");
		}
	}

	public unsafe SceneLight* Create()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* ptr = ((IMemorySpace)IMemorySpace.GetDefaultSpace()).Malloc<SceneLight>(8uL);
		_sceneLightCtor(ptr);
		_sceneLightInit(ptr);
		_sceneLightSpawn(ptr);
		EditorCamera current = _context.Cameras.Current;
		if (current is WorkCamera workCamera)
		{
			Unsafe.Write(&((Transform)(&ptr->Transform)).Position, Vector3.op_Implicit(workCamera.Position));
			Unsafe.Write(&((Transform)(&ptr->Transform)).Rotation, Quaternion.op_Implicit(workCamera.CalculateLookDirection().EulerAnglesToQuaternion()));
		}
		else
		{
			Unsafe.Write(&((Transform)(&ptr->Transform)).Position, Vector3.op_Implicit(current.Camera->Position));
			Unsafe.Write(&((Transform)(&ptr->Transform)).Rotation, Quaternion.op_Implicit(current.Camera->CalcPointDirection()));
		}
		((long*)ptr)[7] |= 2L;
		RenderLight* renderLight = ptr->RenderLight;
		if (renderLight != null)
		{
			renderLight->Flags = LightFlags.Reflection;
			renderLight->LightType = LightType.PointLight;
			renderLight->Transform = &ptr->Transform;
			renderLight->Color = new ColorHDR();
			renderLight->ShadowNear = 0.1f;
			renderLight->ShadowFar = 15f;
			renderLight->FalloffType = FalloffType.Quadratic;
			renderLight->AreaAngle = Vector2.Zero;
			renderLight->Falloff = 1.1f;
			renderLight->LightAngle = 45f;
			renderLight->FalloffAngle = 0.5f;
			renderLight->Range = 100f;
			renderLight->CharaShadowRange = 100f;
		}
		_created.Add((nint)ptr);
		return ptr;
	}

	public unsafe void Destroy(SceneLight* light)
	{
		_created.Remove((nint)light);
		_framework.RunOnFrameworkThread((Action)delegate
		{
			InvokeDtor(light);
		});
	}

	private unsafe void DestroyAll()
	{
		if (_framework.IsFrameworkUnloading)
		{
			return;
		}
		_framework.RunOnFrameworkThread((Action)delegate
		{
			foreach (nint item in _created)
			{
				InvokeDtor((SceneLight*)item);
			}
			_created.Clear();
		});
	}

	private unsafe void InvokeDtor(SceneLight* light)
	{
		GetVirtualFunc<CleanupRenderDelegate>(light, 1)(light);
		GetVirtualFunc<DestructorDelegate>(light, 0)(light, a2: false);
	}

	private unsafe static T GetVirtualFunc<T>(SceneLight* light, int index)
	{
		return Marshal.GetDelegateForFunctionPointer<T>(light->_vf[index]);
	}

	public override void Dispose()
	{
		base.Dispose();
		Ktisis.Log.Verbose("Disposing light spawn manager...");
		DestroyAll();
		GC.SuppressFinalize(this);
	}
}

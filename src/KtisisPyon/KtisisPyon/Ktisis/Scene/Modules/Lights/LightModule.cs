using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using InteropGenerator.Runtime;
using Ktisis.Common.Utility;
using Ktisis.Data.Config.Gobos;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Types;
using Ktisis.Structs.GPose;
using Ktisis.Structs.Lights;
using Ktisis.Structs.Objects;

namespace Ktisis.Scene.Modules.Lights;

public class LightModule : SceneModule
{
	private unsafe delegate void SceneLightUpdateCullingDelegate(SceneLight* self);

	private unsafe delegate void SceneLightUpdateMaterialsDelegate(SceneLight* self);

	private unsafe delegate bool SceneLightTextureDelegate(SceneLight* self, ResourceCategory* category, CStringPointer path);

	private unsafe delegate ResourceCategory* GetResourceCategoryForPathDelegate(ResourceCategory* category, CStringPointer path);

	private unsafe delegate bool ToggleLightDelegate(GPoseState* state, uint index);

	private readonly GroupPoseModule _gpose;

	private readonly IFramework _framework;

	private readonly LightSpawner _spawner;

	private readonly IEditorContext _ctx;

	[Signature("48 89 5C 24 ?? 57 48 83 EC 40 48 8B B9 ?? ?? ?? ??")]
	private SceneLightUpdateCullingDelegate _sceneLightUpdateCulling;

	[Signature("40 53 48 83 EC 20 0F B6 81 ?? ?? ?? ?? 48 8B D9 A8 04 75 45 0C 04 B2 05")]
	private SceneLightUpdateMaterialsDelegate _sceneLightUpdateMaterials;

	[Signature("40 53 48 83 EC ?? 48 8B D9 C7 44 24 ?? ?? ?? ?? ?? 33 C9")]
	private SceneLightTextureDelegate _sceneLightTexture;

	[Signature("40 53 48 83 EC ?? ?? ?? ?? ?? 4C 8B CA 0F BE 42")]
	private GetResourceCategoryForPathDelegate _resourceCat;

	[Signature("48 83 EC 28 4C 8B C1 83 FA 03", DetourName = "ToggleLightDetour")]
	private Hook<ToggleLightDelegate>? ToggleLightHook;

	public LightModule(IHookMediator hook, ISceneManager scene, GroupPoseModule gpose, IFramework framework)
		: base(hook, scene)
	{
		_ctx = scene.Context;
		_gpose = gpose;
		_framework = framework;
		_spawner = hook.Create<LightSpawner>(new object[1] { _ctx });
	}

	public override void Setup()
	{
		EnableAll();
		BuildLightEntities();
		_spawner.TryInitialize();
	}

	public unsafe void RefreshLightEntities()
	{
		if (!CheckValid())
		{
			return;
		}
		GPoseState* gPoseState = _gpose.GetGPoseState();
		if (gPoseState == null)
		{
			return;
		}
		Span<Pointer<SceneLight>> lights = gPoseState->GetLights();
		for (int i = 0; i < lights.Length; i++)
		{
			SceneLight* found = gPoseState->GetLight((uint)i);
			if (found != null && Scene.Children.FirstOrDefault((SceneEntity entity) => entity is LightEntity lightEntity && lightEntity.Address == (nint)found) == null)
			{
				Ktisis.Log.Debug($"backfilling gpose LightEntity for index {i}");
				AddLight(lights[i].Value, (uint)i);
			}
		}
	}

	public unsafe void AddFromOverworld(WorldObject worldLight)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		SceneLight* light = (SceneLight*)worldLight.Address;
		if (light == null || Scene.Children.FirstOrDefault((SceneEntity entity) => entity is LightEntity lightEntity2 && lightEntity2.Address == (nint)light) != null)
		{
			return;
		}
		Ktisis.Log.Debug($"adding gpose LightEntity for overworld light {worldLight.Address:X}");
		SceneLight* ptr = _spawner.Create();
		if (ptr == null)
		{
			return;
		}
		RenderLight* renderLight = ptr->RenderLight;
		if (renderLight == null)
		{
			return;
		}
		RenderLight* renderLight2 = light->RenderLight;
		if (renderLight2 != null)
		{
			Unsafe.Write(&ptr->Transform, light->Transform);
			renderLight->Flags = renderLight2->Flags;
			renderLight->LightType = renderLight2->LightType;
			renderLight->Color.RGB = renderLight2->Color.RGB;
			renderLight->Color.Intensity = renderLight2->Color.Intensity;
			renderLight->ShadowNear = renderLight2->ShadowNear;
			renderLight->ShadowFar = renderLight2->ShadowFar;
			renderLight->FalloffType = renderLight2->FalloffType;
			renderLight->Falloff = renderLight2->Falloff;
			renderLight->FalloffAngle = renderLight2->FalloffAngle;
			renderLight->AreaAngle = renderLight2->AreaAngle;
			renderLight->LightAngle = renderLight2->LightAngle;
			renderLight->Range = renderLight2->Range;
			renderLight->CharaShadowRange = renderLight2->CharaShadowRange;
			LightEntity lightEntity = Scene.Factory.BuildLight().SetName("World Light").SetAddress(ptr)
				.SetWorldLight(worldLight)
				.Add();
			if (light->Texture != null)
			{
				string text = ((object)(*(StdString*)(&((TextureResourceHandle)light->Texture).FileName))/*cast due to constrained. prefix*/).ToString();
				GoboEntry gobo = new GoboEntry
				{
					Name = text,
					Path = text
				};
				lightEntity.SetGobo(gobo);
			}
			lightEntity.Visible = true;
		}
	}

	private unsafe void BuildLightEntities()
	{
		GPoseState* gPoseState = _gpose.GetGPoseState();
		if (gPoseState == null)
		{
			return;
		}
		Span<Pointer<SceneLight>> lights = gPoseState->GetLights();
		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i].Value != null)
			{
				AddLight(lights[i].Value, (uint)i);
			}
		}
	}

	private unsafe void AddLight(SceneLight* light, uint index)
	{
		Scene.Factory.BuildLight().SetName($"Camera Light {index + 1}").SetAddress(light)
			.Add();
	}

	private unsafe void RemoveLight(SceneLight* light)
	{
		Scene.Children.FirstOrDefault((SceneEntity entity) => entity is LightEntity lightEntity && lightEntity.Address == (nint)light)?.Remove();
	}

	public unsafe void UpdateLightObject(LightEntity entity)
	{
		if (base.IsInit && entity.IsValid)
		{
			SceneLight* ptr = entity.GetObject();
			if (ptr != null)
			{
				_sceneLightUpdateCulling(ptr);
				_sceneLightUpdateMaterials(ptr);
			}
			entity.Flags &= ~LightEntityFlags.Update;
		}
	}

	public unsafe void UpdateSceneLightTexture(SceneLight* self, string? path)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (path != null)
		{
			path += "\0";
			Ktisis.Log.Debug($"updating texture for light {(nint)self:X} to {path}");
			byte* ptr = stackalloc byte[(int)(uint)path.Length];
			for (int i = 0; i < path.Length; i++)
			{
				ptr[i] = (byte)path[i];
			}
			ResourceCategory* category = (ResourceCategory*)stackalloc byte[4];
			_resourceCat(category, CStringPointer.op_Implicit(ptr));
			_sceneLightTexture(self, category, CStringPointer.op_Implicit(ptr));
		}
	}

	private unsafe bool ToggleLightDetour(GPoseState* state, uint index)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		try
		{
			bool num = CheckValid();
			SceneLight* ptr = (num ? state->GetLight(index) : null);
			flag = ToggleLightHook.Original(state, index);
			if (num && flag)
			{
				SceneLight* light = state->GetLight(index);
				if (light != null && light != ptr)
				{
					if (_ctx.Cameras.Current is WorkCamera workCamera)
					{
						Unsafe.Write(&((Transform)(&light->Transform)).Position, Vector3.op_Implicit(workCamera.Position));
						Unsafe.Write(&((Transform)(&light->Transform)).Rotation, Quaternion.op_Implicit(workCamera.CalculateLookDirection().EulerAnglesToQuaternion()));
					}
					AddLight(light, index);
				}
				else if (light == null && ptr != null)
				{
					RemoveLight(ptr);
				}
			}
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to handle light toggle:\n{value}");
		}
		return flag;
	}

	public async Task<LightEntity> Spawn()
	{
		return await _framework.RunOnFrameworkThread<LightEntity>((Func<LightEntity>)(() => CreateLight() ?? throw new Exception("Failed to create light entity.")));
	}

	private unsafe LightEntity? CreateLight()
	{
		SceneLight* ptr = _spawner.Create();
		if (ptr == null)
		{
			return null;
		}
		return Scene.Factory.BuildLight().SetName("Light").SetAddress(ptr)
			.Add();
	}

	public unsafe void Delete(LightEntity light)
	{
		SceneLight* address = (SceneLight*)light.Address;
		light.Address = IntPtr.Zero;
		light.Remove();
		if (address != null)
		{
			_spawner.Destroy(address);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		_spawner.Dispose();
		GC.SuppressFinalize(this);
	}
}

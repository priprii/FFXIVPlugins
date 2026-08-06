using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Common.Extensions;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Lights;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Modules.Lights;
using Ktisis.Scene.Types;
using Ktisis.Services.Data;
using Ktisis.Services.Game;

namespace Ktisis.Scene;

public class SceneManager : SceneModuleContainer, ISceneManager, IComposite, IDisposable
{
	private readonly IFramework _framework;

	private readonly IObjectTable _objectTable;

	private readonly SceneRoot Root;

	private Vector3 SceneOrigin;

	private bool IsDisposing;

	public bool IsValid
	{
		get
		{
			if (Context.IsValid)
			{
				return !IsDisposing;
			}
			return false;
		}
	}

	public IEditorContext Context { get; }

	public IEntityFactory Factory { get; }

	public OverlayService Overlay { get; }

	public WorldService World { get; }

	public SceneDataService Data { get; }

	public double UpdateTime { get; private set; }

	public SceneEntity? Parent
	{
		get
		{
			return Root.Parent;
		}
		set
		{
			Root.Parent = value;
		}
	}

	public IEnumerable<SceneEntity> Children => Root.Children;

	public SceneManager(IEditorContext context, HookScope scope, IFramework framework, IEntityFactory factory, IObjectTable objectTable, SceneDataService sceneDataService, OverlayService overlay, WorldService world)
		: base(scope)
	{
		Context = context;
		Factory = factory;
		Root = new SceneRoot(this);
		_framework = framework;
		_objectTable = objectTable;
		Data = sceneDataService;
		Overlay = overlay;
		World = world;
	}

	public void Initialize()
	{
		Ktisis.Log.Info("Initializing scene...");
		SetupModules();
		SetSceneOrigin();
	}

	public unsafe void SetSceneOrigin()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		DrawObject* drawObject = ((IGameObject)(object)_objectTable.LocalPlayer).GetDrawObject();
		SceneOrigin = Vector3.op_Implicit(((DrawObject)drawObject).Position);
	}

	private void SetupModules()
	{
		GroupPoseModule groupPoseModule = AddModule<GroupPoseModule>(Array.Empty<object>());
		AddModule<ActorModule>(new object[1] { groupPoseModule });
		AddModule<LightModule>(new object[1] { groupPoseModule });
		AddModule<EnvModule>(Array.Empty<object>());
		InitializeModules();
		SetupSavedState();
		Overlay.Initialize(Context);
	}

	private void SetupSavedState()
	{
		foreach (ReferenceImage.SetupData referenceImage in Context.Config.Editor.ReferenceImages)
		{
			Factory.BuildRefImage().FromData(referenceImage).Add();
		}
	}

	public void Update()
	{
		if (IsValid)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			UpdateModules();
			Root.Update();
			stopwatch.Stop();
			UpdateTime = stopwatch.Elapsed.TotalMilliseconds;
		}
	}

	public void Refresh()
	{
		foreach (IConfigurable item in (from entity in Root.Recurse()
			where entity is IConfigurable
			select entity).Cast<IConfigurable>())
		{
			item.Refresh();
		}
	}

	public bool Add(SceneEntity entity)
	{
		return Root.Add(entity);
	}

	public bool Remove(SceneEntity entity)
	{
		return Root.Remove(entity);
	}

	public IEnumerable<SceneEntity> Recurse()
	{
		return Root.Recurse();
	}

	public ActorEntity? GetEntityForActor(IGameObject actor)
	{
		return GetEntityForIndex(actor.ObjectIndex);
	}

	public ActorEntity? GetEntityForIndex(uint objectIndex)
	{
		return (from entity in Children.ToList()
			where entity is ActorEntity actorEntity && actorEntity.IsValid
			select entity).Cast<ActorEntity>().FirstOrDefault((ActorEntity entity) => entity.Actor.ObjectIndex == objectIndex);
	}

	public ActorEntity GetFirstActor()
	{
		return (from ActorEntity entity in Children.Where((SceneEntity entity) => entity is ActorEntity actorEntity && actorEntity.IsValid)
			orderby entity.Actor.ObjectIndex
			select entity).First();
	}

	public Vector3 GetSceneOrigin()
	{
		return SceneOrigin;
	}

	public Vector3 GetActorRelativePosition(Vector3 position)
	{
		return position - SceneOrigin;
	}

	public Task ApplyLightFile(LightEntity light, LightFile file)
	{
		EntityLightConverter converter = new EntityLightConverter(light);
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			converter.Apply(file);
		});
	}

	public Task<LightFile> SaveLightFile(LightEntity light)
	{
		EntityLightConverter converter = new EntityLightConverter(light);
		return _framework.RunOnFrameworkThread<LightFile>((Func<LightFile>)(() => converter.Save()));
	}

	public void Dispose()
	{
		IsDisposing = true;
		try
		{
			Root.Clear();
			DisposeModules();
			Overlay.Disable();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to dispose scene!\n{value}");
		}
		GC.SuppressFinalize(this);
	}
}

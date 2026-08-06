using System;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Modules;
using Ktisis.Services.Data;
using Ktisis.Services.Game;

namespace Ktisis.Scene.Types;

public interface ISceneManager : IComposite, IDisposable
{
	bool IsValid { get; }

	IEditorContext Context { get; }

	IEntityFactory Factory { get; }

	OverlayService Overlay { get; }

	WorldService World { get; }

	SceneDataService Data { get; }

	double UpdateTime { get; }

	T GetModule<T>() where T : SceneModule;

	bool TryGetModule<T>(out T? module) where T : SceneModule;

	void Initialize();

	void Update();

	void Refresh();

	ActorEntity? GetEntityForActor(IGameObject actor);

	ActorEntity? GetEntityForIndex(uint objectIndex);

	ActorEntity GetFirstActor();

	Task ApplyLightFile(LightEntity light, LightFile file);

	Task<LightFile> SaveLightFile(LightEntity light);

	Vector3 GetSceneOrigin();

	Vector3 GetActorRelativePosition(Vector3 Position);
}

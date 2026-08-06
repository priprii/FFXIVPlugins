using System;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Data.Files;
using Ktisis.Data.Mcdf;
using Ktisis.Editor.Characters.State;
using Ktisis.GameData.Excel.Types;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Editor.Characters.Types;

public interface ICharacterManager : IDisposable
{
	bool IsValid { get; }

	McdfManager Mcdf { get; }

	event DisableDrawHandler? OnDisableDraw;

	void Initialize();

	ICustomizeEditor GetCustomizeEditor(ActorEntity actor);

	IEquipmentEditor GetEquipmentEditor(ActorEntity actor);

	bool TryGetStateForActor(IGameObject actor, out ActorEntity entity, out AppearanceState state);

	void ApplyStateToGameObject(ActorEntity entity);

	Task ApplyCharaFile(ActorEntity actor, CharaFile file, SaveModes modes = SaveModes.All, bool gameState = false);

	Task<CharaFile> SaveCharaFile(ActorEntity actor);

	Task ApplyNpc(ActorEntity actor, INpcBase npc, SaveModes modes = SaveModes.All, bool gameState = false);
}

using System.Threading.Tasks;
using Dalamud.Utility;
using Ktisis.Data.Files;
using Ktisis.Data.Mcdf;
using Ktisis.Editor.Characters;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Creators;

public sealed class ActorCreator : EntityCreator<ActorEntity, IActorCreator>, IActorCreator, IEntityCreator<ActorEntity, IActorCreator>, IEntityBuilderBase<ActorEntity, IActorCreator>
{
	private CharaFile? Appearance;

	private string? McdfFile;

	private McdfManager McdfManager { get; init; }

	protected override IActorCreator Builder => this;

	public ActorCreator(ISceneManager scene, McdfManager mcdfManager)
		: base(scene)
	{
		McdfManager = mcdfManager;
	}

	public IActorCreator WithAppearance(CharaFile file)
	{
		Appearance = file;
		return this;
	}

	public IActorCreator WithMcdf(string mcdfPath)
	{
		McdfFile = mcdfPath;
		return this;
	}

	public async Task<ActorEntity?> Spawn()
	{
		ActorEntity entity = await Scene.GetModule<ActorModule>().Spawn();
		if (entity == null)
		{
			return null;
		}
		entity.Name = (StringExtensions.IsNullOrEmpty(base.Name) ? $"Actor #{entity.Actor.ObjectIndex}" : base.Name);
		if (Appearance != null)
		{
			await Scene.Context.Characters.ApplyCharaFile(entity, Appearance, SaveModes.All, gameState: true);
		}
		if (McdfFile != null)
		{
			McdfManager.LoadAndApplyTo(McdfFile, entity.Actor);
		}
		return entity;
	}
}

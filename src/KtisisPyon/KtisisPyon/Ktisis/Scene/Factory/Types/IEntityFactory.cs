using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Factory.Creators;
using Ktisis.Structs.Lights;

namespace Ktisis.Scene.Factory.Types;

public interface IEntityFactory
{
	IActorBuilder BuildActor(IGameObject actor);

	ILightBuilder BuildLight();

	IObjectBuilder BuildObject();

	IPoseBuilder BuildPose();

	IOverlayBuilder BuildOverlay(OverlayTypes type);

	IActorCreator CreateActor();

	ILightCreator CreateLight();

	ILightCreator CreateLight(LightType type);

	IRefImageBuilder BuildRefImage();
}

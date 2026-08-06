using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;

namespace Ktisis.Scene.Factory.Builders;

public interface IObjectBuilder : IEntityBuilder<WorldEntity, IObjectBuilder>, IEntityBuilderBase<WorldEntity, IObjectBuilder>
{
	IObjectBuilder SetAddress(nint address);

	unsafe IObjectBuilder SetAddress(Object* pointer);
}

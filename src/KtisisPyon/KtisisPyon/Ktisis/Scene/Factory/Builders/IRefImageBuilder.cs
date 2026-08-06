using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Factory.Types;

namespace Ktisis.Scene.Factory.Builders;

public interface IRefImageBuilder : IEntityBuilder<ReferenceImage, IRefImageBuilder>, IEntityBuilderBase<ReferenceImage, IRefImageBuilder>
{
	IRefImageBuilder FromData(ReferenceImage.SetupData data);

	IRefImageBuilder SetPath(string path);
}

using System.IO;
using Dalamud.Utility;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Builders;

public sealed class RefImageBuilder(ISceneManager scene) : EntityBuilder<ReferenceImage, IRefImageBuilder>(scene), IRefImageBuilder, IEntityBuilder<ReferenceImage, IRefImageBuilder>, IEntityBuilderBase<ReferenceImage, IRefImageBuilder>
{
	private ReferenceImage.SetupData Data = new ReferenceImage.SetupData();

	protected override IRefImageBuilder Builder => this;

	public IRefImageBuilder FromData(ReferenceImage.SetupData data)
	{
		Data = data;
		return this;
	}

	public IRefImageBuilder SetPath(string path)
	{
		Data.FilePath = path;
		return this;
	}

	protected override ReferenceImage Build()
	{
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			base.Name = Path.GetFileName(Data.FilePath);
		}
		return new ReferenceImage(Scene, Data)
		{
			Name = base.Name
		};
	}
}

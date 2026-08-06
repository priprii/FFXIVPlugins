using System.Linq;
using Dalamud.Utility;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Factory.Builders;

public sealed class OverlayBuilder : EntityBuilder<OverlayEntity, IOverlayBuilder>, IOverlayBuilder, IEntityBuilder<OverlayEntity, IOverlayBuilder>, IEntityBuilderBase<OverlayEntity, IOverlayBuilder>
{
	private OverlayTypes _type;

	protected override IOverlayBuilder Builder => this;

	public OverlayBuilder(ISceneManager scene, OverlayTypes type)
		: base(scene)
	{
		_type = type;
	}

	protected override OverlayEntity Build()
	{
		return _type switch
		{
			OverlayTypes.Talk => BuildTalk(), 
			OverlayTypes.Balloon => BuildBalloon(), 
			OverlayTypes.Status => BuildStatus(), 
			_ => BuildTalk(), 
		};
	}

	private TalkOverlay BuildTalk()
	{
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			base.Name = $"Dialog {Scene.Children.OfType<TalkOverlay>().Count() + 1}";
		}
		return new TalkOverlay(Scene)
		{
			Name = base.Name
		};
	}

	private BalloonOverlay BuildBalloon()
	{
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			base.Name = $"Balloon {Scene.Children.OfType<BalloonOverlay>().Count() + 1}";
		}
		return new BalloonOverlay(Scene)
		{
			Name = base.Name
		};
	}

	private StatusOverlay BuildStatus()
	{
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			base.Name = $"Status {Scene.Children.OfType<StatusOverlay>().Count() + 1}";
		}
		return new StatusOverlay(Scene)
		{
			Name = base.Name
		};
	}
}

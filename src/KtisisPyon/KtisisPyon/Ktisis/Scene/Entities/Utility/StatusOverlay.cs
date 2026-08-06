using System.Numerics;
using Ktisis.Interface.KTK;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Utility;

public class StatusOverlay : OverlayEntity
{
	protected override StatusNode Node { get; }

	public string StatusText
	{
		get
		{
			return Node.Text;
		}
		set
		{
			Node.Text = value;
		}
	}

	public StatusType StatusType
	{
		get
		{
			return Node.Type;
		}
		set
		{
			Node.Type = value;
		}
	}

	public string IconPath
	{
		get
		{
			return Node.IconPath;
		}
		set
		{
			Node.IconPath = value;
		}
	}

	public StatusOverlay(ISceneManager scene)
		: base(scene)
	{
		base.Type = EntityType.StatusOverlay;
		Node = new StatusNode(StatusType.Buff, "New Status", "ui/icon/213000/213001_hr1.tex")
		{
			Size = new Vector2(247f, 32f),
			Position = new Vector2(450f, 450f),
			EnableMoving = false,
			IsVisible = true
		};
		Scene.Overlay.AddNode(Node);
	}
}

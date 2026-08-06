using System.Numerics;
using Ktisis.Interface.KTK;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Utility;

public class BalloonOverlay : OverlayEntity
{
	public readonly uint[] FontSizes = new uint[8] { 8u, 9u, 10u, 11u, 12u, 14u, 16u, 18u };

	protected override BalloonNode Node { get; }

	public string Dialog
	{
		get
		{
			return Node.Dialog;
		}
		set
		{
			Node.Dialog = value;
		}
	}

	public BalloonBackground Background
	{
		get
		{
			return Node.BgChoice;
		}
		set
		{
			Node.BgChoice = value;
		}
	}

	public BalloonColor Color
	{
		get
		{
			return Node.ColorChoice;
		}
		set
		{
			Node.ColorChoice = value;
		}
	}

	public bool Arrow
	{
		get
		{
			return Node.ArrowVisible;
		}
		set
		{
			Node.ArrowVisible = value;
		}
	}

	public float ArrowX
	{
		get
		{
			return Node.ArrowX;
		}
		set
		{
			Node.ArrowX = value;
		}
	}

	public uint FontSize
	{
		get
		{
			return Node.FontSize;
		}
		set
		{
			Node.FontSize = value;
		}
	}

	public BalloonOverlay(ISceneManager scene)
		: base(scene)
	{
		base.Type = EntityType.BalloonOverlay;
		Node = new BalloonNode(BalloonBackground.Say, BalloonColor.Default, "New dialog...", arrowVisible: true, 130f, 12u)
		{
			Size = new Vector2(200f, 90f),
			Position = new Vector2(500f, 500f),
			EnableMoving = false,
			IsVisible = true
		};
		Scene.Overlay.AddNode(Node);
	}
}

using System.Numerics;
using Ktisis.Interface.KTK;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Utility;

public class TalkOverlay : OverlayEntity
{
	public readonly uint[] FontSizes = new uint[11]
	{
		8u, 9u, 10u, 11u, 12u, 14u, 16u, 18u, 20u, 22u,
		24u
	};

	protected override TalkNode Node { get; }

	public string Speaker
	{
		get
		{
			return Node.Speaker;
		}
		set
		{
			Node.Speaker = value;
		}
	}

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

	public TalkBackground Background
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

	public TalkCursor Cursor
	{
		get
		{
			return Node.CursorChoice;
		}
		set
		{
			Node.CursorChoice = value;
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

	public TalkOverlay(ISceneManager scene)
		: base(scene)
	{
		base.Type = EntityType.TalkOverlay;
		Node = new TalkNode(TalkBackground.Basic, TalkCursor.Pin, "Speaker", "New dialog...", 14u)
		{
			Size = new Vector2(680f, 180f),
			Position = new Vector2(600f, 600f),
			EnableMoving = false,
			IsVisible = true
		};
		Scene.Overlay.AddNode(Node);
	}
}

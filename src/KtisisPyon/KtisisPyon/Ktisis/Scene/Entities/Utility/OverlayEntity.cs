using System.Numerics;
using KamiToolKit.Overlay.UiOverlay;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Utility;

public abstract class OverlayEntity : SceneEntity, IVisibility, IDeletable
{
	private bool _visible = true;

	protected abstract OverlayNode Node { get; }

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
			Node.IsVisible = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return Node.Position;
		}
		set
		{
			Node.Position = value;
		}
	}

	public float Scale
	{
		get
		{
			return Node.ScaleX;
		}
		set
		{
			Node.ScaleX = value;
			Node.ScaleY = value;
		}
	}

	public Vector2 Size
	{
		get
		{
			return Node.Size;
		}
		private set
		{
			Node.Size = value;
		}
	}

	public bool Draggable
	{
		get
		{
			return Node.EnableMoving;
		}
		set
		{
			Node.EnableMoving = value;
		}
	}

	public float Alpha
	{
		get
		{
			return Node.Alpha;
		}
		set
		{
			Node.Alpha = value;
		}
	}

	public OverlayEntity(ISceneManager scene)
		: base(scene)
	{
	}

	public bool Delete()
	{
		Scene.Overlay.RemoveNode(Node);
		Node.Dispose();
		Remove();
		return true;
	}
}

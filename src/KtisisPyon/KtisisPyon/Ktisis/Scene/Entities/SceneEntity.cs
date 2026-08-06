using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ktisis.Editor.Selection;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities;

public abstract class SceneEntity : IComposite
{
	protected readonly ISceneManager Scene;

	private readonly List<SceneEntity> _children = new List<SceneEntity>();

	public virtual string Name { get; set; } = string.Empty;

	public EntityType Type { get; protected init; }

	public virtual bool IsValid
	{
		get
		{
			if (Scene.IsValid)
			{
				return Parent != null;
			}
			return false;
		}
	}

	public SceneEntity Root
	{
		get
		{
			SceneEntity parent = Parent;
			if ((parent != null && parent.Type != EntityType.Invalid) || 1 == 0)
			{
				return Parent.Root;
			}
			return this;
		}
	}

	private ISelectManager Selection => Scene.Context.Selection;

	public bool IsSelected => Selection.IsSelected(this);

	public virtual SceneEntity? Parent { get; set; }

	public virtual IEnumerable<SceneEntity> Children => _children;

	protected SceneEntity(ISceneManager scene)
	{
		Scene = scene;
	}

	public virtual void Update()
	{
		if (!IsValid)
		{
			return;
		}
		foreach (SceneEntity child in Children)
		{
			child.Update();
		}
	}

	public void Select(SelectMode mode = SelectMode.Default)
	{
		Selection.Select(this, mode);
	}

	public void Unselect()
	{
		Selection.Unselect(this);
	}

	protected List<SceneEntity> GetChildren()
	{
		return _children;
	}

	public virtual bool Add(SceneEntity entity)
	{
		if (_children.Contains(entity))
		{
			return false;
		}
		_children.Add(entity);
		entity.Parent?.Remove(entity);
		entity.Parent = this;
		return true;
	}

	public virtual bool Remove(SceneEntity entity)
	{
		bool result = _children.Remove(entity);
		entity.Parent = null;
		return result;
	}

	public virtual void Remove()
	{
		Parent?.Remove(this);
		Clear();
	}

	public virtual void Clear()
	{
		foreach (SceneEntity item in Children.ToList())
		{
			item.Remove();
		}
	}

	public IEnumerable<SceneEntity> Recurse()
	{
		foreach (SceneEntity child in Children)
		{
			yield return child;
			foreach (SceneEntity item in child.Recurse())
			{
				yield return item;
			}
		}
	}

	public bool IsChildOf(SceneEntity entity)
	{
		SceneEntity parent = Parent;
		int num = 0;
		while (parent != null && num++ < 1000)
		{
			if (parent == entity)
			{
				return true;
			}
			parent = parent.Parent;
		}
		return false;
	}

	protected void ToggleView(ImmutableHashSet<string> names, bool newState)
	{
		if (this is BoneNode boneNode && names.Contains(boneNode.Info.Name))
		{
			boneNode.Visible = newState;
			((IVisibility)boneNode).Toggle();
		}
		foreach (BoneNode item in Recurse().OfType<BoneNode>())
		{
			if (names.Contains(item.Info.Name))
			{
				item.Visible = newState;
			}
		}
	}

	protected ImmutableHashSet<string> GetEnabledBones()
	{
		HashSet<string> hashSet = new HashSet<string>(128);
		foreach (BoneNode item in Recurse().OfType<BoneNode>())
		{
			if (item.Visible)
			{
				hashSet.Add(item.Info.Name);
			}
		}
		return hashSet.ToImmutableHashSet();
	}
}

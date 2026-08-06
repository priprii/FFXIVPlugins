using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Types;

public abstract class EntityEditWindow<T> : KtisisWindow where T : SceneEntity
{
	protected readonly IEditorContext Context;

	private T? _target;

	protected T Target
	{
		get
		{
			return _target;
		}
		private set
		{
			_target = value;
		}
	}

	protected EntityEditWindow(string name, IEditorContext ctx, ImGuiWindowFlags flags = (ImGuiWindowFlags)0, string windowId = "")
		: base(name, flags, windowId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Context = ctx;
	}

	public override void PreDraw()
	{
		if (Context.IsValid)
		{
			T target = _target;
			if (target != null && target.IsValid)
			{
				return;
			}
		}
		Ktisis.Log.Verbose("State for " + ((object)this).GetType().Name + " is stale, closing...");
		Close();
	}

	public virtual void SetTarget(T target)
	{
		if (!target.IsValid)
		{
			throw new Exception("Attempted to set invalid target.");
		}
		Target = target;
	}

	protected void UpdateTarget()
	{
		T val = (T)Context.Selection.GetSelected().FirstOrDefault((SceneEntity entity) => entity is T);
		if (val != null && Target != val)
		{
			SetTarget(val);
		}
	}
}

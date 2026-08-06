using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ktisis.Actions.Types;
using Ktisis.Common.Utility;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Transforms;

public class TransformHandler : ITransformHandler
{
	private class TransformMemento : ITransformMemento, IMemento
	{
		private readonly TransformHandler _handler;

		private readonly ITransformTarget Target;

		private TransformSetup Setup = new TransformSetup();

		private Transform? Initial;

		private Transform? Final;

		private bool IsDispatch;

		public TransformMemento(TransformHandler handler, ITransformTarget target)
		{
			_handler = handler;
			Target = target;
		}

		public ITransformMemento Save()
		{
			Initial = Target.GetTransform();
			Setup = Target.Setup._003CClone_003E_0024();
			return this;
		}

		public void SetTransform(Transform transform)
		{
			Target.SetTransform(transform);
		}

		public void SetMatrix(Matrix4x4 matrix)
		{
			Target.SetMatrix(matrix);
		}

		public void Restore()
		{
			if (Initial != null)
			{
				ApplyState(Initial);
			}
		}

		public void Apply()
		{
			if (Final != null)
			{
				ApplyState(Final);
			}
		}

		private void ApplyState(Transform transform)
		{
			Target.Setup = Setup._003CClone_003E_0024();
			Target.SetTransform(transform);
		}

		public void Dispatch()
		{
			if (!IsDispatch)
			{
				IsDispatch = true;
				Final = Target.GetTransform();
				_handler._action.History.Add(this);
			}
		}
	}

	private readonly IEditorContext _context;

	private readonly IActionManager _action;

	private readonly ISelectManager _select;

	public ITransformTarget? Target { get; private set; }

	public TransformHandler(IEditorContext context, IActionManager action, ISelectManager select)
	{
		_context = context;
		_action = action;
		_select = select;
		select.Changed += OnSelectionChanged;
	}

	private void OnSelectionChanged(ISelectManager sender)
	{
		List<SceneEntity> list = (from entity in TransformResolver.GetCorrelatingBones(from entity in _select.GetSelected()
				where entity?.IsValid ?? false
				select entity, yieldDefault: true)
			where entity is ITransform
			select entity).ToList();
		if (list.Count == 0)
		{
			Target = null;
			return;
		}
		SceneEntity sceneEntity = list.FirstOrDefault();
		if (sceneEntity is SkeletonNode)
		{
			sceneEntity = TransformResolver.GetPoseTarget(list);
		}
		Target = new TransformTarget(sceneEntity, list);
	}

	public ITransformMemento Begin(ITransformTarget target)
	{
		return Begin(target, delegate(TransformSetup s)
		{
			s.Configure(_context.Config.Gizmo);
		});
	}

	public ITransformMemento Begin(ITransformTarget target, Action<TransformSetup> configure)
	{
		configure(target.Setup);
		return new TransformMemento(this, target).Save();
	}
}

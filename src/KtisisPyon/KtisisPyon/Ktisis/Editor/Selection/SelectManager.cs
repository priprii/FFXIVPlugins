using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Context.Types;
using Ktisis.Events;
using Ktisis.Scene;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Services.Game;

namespace Ktisis.Editor.Selection;

public class SelectManager : ISelectManager
{
	private readonly IEditorContext _context;

	private readonly GPoseService _gpose;

	private readonly Event<Action<ISelectManager>> _changed = new Event<Action<ISelectManager>>();

	private readonly HashSet<ActorEntity> PreviousActors = new HashSet<ActorEntity>();

	private readonly List<SceneEntity> Selected = new List<SceneEntity>();

	private IGameObject? Targeted;

	public int Count => Selected.Count;

	public event SelectChangedHandler Changed
	{
		add
		{
			_changed.Add(value.Invoke);
		}
		remove
		{
			_changed.Remove(value.Invoke);
		}
	}

	public SelectManager(IEditorContext context, GPoseService gpose)
	{
		_context = context;
		_gpose = gpose;
	}

	public void Update()
	{
		if (Selected.RemoveAll((SceneEntity item) => !item.IsValid) > 0)
		{
			InvokeChanged();
		}
		if (_context.Config.Editor.SelectOnTarget && Targeted != null && _gpose.GPoseTarget != null && !((IEquatable<IGameObject>)Targeted).Equals(_gpose.GPoseTarget))
		{
			ActorEntity entityForIndex = _context.Scene.GetEntityForIndex(_gpose.GPoseTarget.ObjectIndex);
			if (entityForIndex != null)
			{
				Select(entityForIndex, SelectMode.Force);
			}
		}
		if (_context.Config.Overlay.PresetsOnActiveActor)
		{
			ActiveState activeStateType = _context.Config.Overlay.ActiveStateType;
			bool flag = ((activeStateType == ActiveState.Target || activeStateType == ActiveState.Both) ? true : false);
			if (flag && Targeted != null && _gpose.GPoseTarget != null && !((IEquatable<IGameObject>)Targeted).Equals(_gpose.GPoseTarget))
			{
				ActorEntity entityForIndex2 = _context.Scene.GetEntityForIndex(Targeted.ObjectIndex);
				ActorEntity entityForIndex3 = _context.Scene.GetEntityForIndex(_gpose.GPoseTarget.ObjectIndex);
				if (entityForIndex2 != null && entityForIndex3 != null)
				{
					foreach (var item in from p in entityForIndex2.GetPresets()
						where p.isEnabled == PresetState.Enabled
						select p)
					{
						entityForIndex3.TogglePreset(item.Item1, true);
						entityForIndex2.TogglePreset(item.Item1, false);
					}
				}
			}
			activeStateType = _context.Config.Overlay.ActiveStateType;
			if ((uint)(activeStateType - 1) <= 1u)
			{
				List<ActorEntity> list = _context.Scene.Children.OfType<ActorEntity>().Where(IsActorSelected).ToList();
				if (PreviousActors.Count > 0)
				{
					List<(string, PresetState)> list2 = (from p in PreviousActors.First().GetPresets()
						where p.isEnabled == PresetState.Enabled
						select p).ToList();
					foreach (ActorEntity item2 in list.Except(PreviousActors))
					{
						foreach (var item3 in list2)
						{
							item2.TogglePreset(item3.Item1, true);
						}
					}
					foreach (ActorEntity item4 in PreviousActors.Except(list))
					{
						foreach (var item5 in list2)
						{
							item4.TogglePreset(item5.Item1, false);
						}
					}
				}
				PreviousActors.Clear();
				PreviousActors.UnionWith(list);
			}
		}
		Targeted = _gpose.GPoseTarget;
	}

	public IEnumerable<SceneEntity> GetSelected()
	{
		return Selected.AsReadOnly();
	}

	public SceneEntity? GetFirstSelected()
	{
		return Selected.FirstOrDefault();
	}

	public bool IsSelected(SceneEntity entity)
	{
		return Selected.Contains(entity);
	}

	public bool IsActorSelected(ActorEntity actor)
	{
		foreach (SceneEntity item in GetSelected())
		{
			SceneEntity sceneEntity = ((item is BoneNode boneNode) ? boneNode.Pose.Parent : ((item is BoneNodeGroup boneNodeGroup) ? boneNodeGroup.Pose.Parent : ((!(item is EntityPose entityPose)) ? item : entityPose.Parent)));
			if (sceneEntity is ActorEntity actorEntity && actorEntity == actor)
			{
				return true;
			}
		}
		return false;
	}

	public void Select(SceneEntity entity)
	{
		Selected.Remove(entity);
		Selected.Add(entity);
		InvokeChanged();
	}

	public void Select(SceneEntity entity, SelectMode mode)
	{
		if (mode == SelectMode.Force)
		{
			if (!IsSelected(entity) || Count != 1)
			{
				Selected.Clear();
				Selected.Add(entity);
				InvokeChanged();
			}
			return;
		}
		bool num = IsSelected(entity);
		bool flag = Count > 1;
		bool flag2 = mode == SelectMode.Multiple;
		if (!flag2)
		{
			Selected.Clear();
		}
		if (!num || (!flag2 && flag))
		{
			Selected.Add(entity);
		}
		else
		{
			Selected.Remove(entity);
		}
		InvokeChanged();
	}

	public void Unselect(SceneEntity entity)
	{
		if (Selected.Remove(entity))
		{
			InvokeChanged();
		}
	}

	public void Clear()
	{
		Selected.Clear();
		InvokeChanged();
	}

	private void InvokeChanged()
	{
		try
		{
			_changed.Invoke(this);
		}
		catch (Exception ex)
		{
			Ktisis.Log.Error(ex.ToString());
		}
	}
}

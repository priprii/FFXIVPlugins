using System.Linq;
using GLib.Popups.Context;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Selection;
using Ktisis.Interface.Editor.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Types;

namespace Ktisis.Interface.Editor.Context;

public class SceneEntityMenuBuilder
{
	private readonly IEditorContext _ctx;

	private readonly SceneEntity _entity;

	private IEditorInterface Ui => _ctx.Interface;

	public SceneEntityMenuBuilder(IEditorContext ctx, SceneEntity entity)
	{
		_ctx = ctx;
		_entity = entity;
	}

	public ContextMenu Create()
	{
		return new ContextMenuBuilder().Group(BuildEntityBaseTop).Group(BuildEntityType).Group(BuildEntityBaseBottom)
			.Build($"EntityContextMenu_{GetHashCode():X}");
	}

	private void BuildEntityBaseTop(ContextMenuBuilder menu)
	{
		if (!_entity.IsSelected)
		{
			menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.select"), delegate
			{
				_entity.Select(SelectMode.Multiple);
			});
		}
		else
		{
			menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.deselect"), _entity.Unselect);
		}
		if (_entity.Children.Any())
		{
			menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.hierarchy"), delegate
			{
				foreach (SceneEntity item in _entity.Children.Where((SceneEntity sceneEntity) => !sceneEntity.IsSelected))
				{
					item.Select(SelectMode.Multiple);
				}
				if (!_entity.IsSelected)
				{
					_entity.Select(SelectMode.Multiple);
				}
			});
		}
		if (_entity is BoneNodeGroup boneNodeGroup && boneNodeGroup != null)
		{
			BoneCategory cat = boneNodeGroup.Category;
			if (cat != null && cat != null)
			{
				menu.Action((cat.HideOnPoseEntity ? "Show" : "Hide") + " group when 'Pose' overlay toggled", delegate
				{
					cat.HideOnPoseEntity = !cat.HideOnPoseEntity;
					foreach (BoneNodeGroup item2 in _entity.Recurse().OfType<BoneNodeGroup>())
					{
						BoneCategory category2 = item2.Category;
						if (category2 != null)
						{
							category2.HideOnPoseEntity = cat.HideOnPoseEntity;
						}
					}
				});
			}
		}
		SceneEntity entity = _entity;
		BoneNode boneNode = entity as BoneNode;
		if (boneNode == null || !(boneNode.Parent is BoneNodeGroup boneNodeGroup2))
		{
			return;
		}
		BoneCategory category = boneNodeGroup2.Category;
		if (category == null)
		{
			return;
		}
		CategoryBone catBone = category.Bones.FirstOrDefault((CategoryBone x) => x.Name == boneNode.Info.Name);
		if (catBone != null)
		{
			menu.Action((catBone.HideOnPoseEntity ? "Show" : "Hide") + " bone when 'Pose' overlay toggled", delegate
			{
				catBone.HideOnPoseEntity = !catBone.HideOnPoseEntity;
			});
		}
	}

	private void BuildEntityBaseBottom(ContextMenuBuilder menu)
	{
		SceneEntity entity = _entity;
		IAttachable attach = entity as IAttachable;
		if (attach != null && attach.IsAttached())
		{
			menu.Separator().Action(Ktisis.Locale.Translate("workspace.entity_menu.base.detach"), delegate
			{
				_ctx.Posing.Attachments.Detach(attach);
			});
		}
		menu.Separator().Action(Ktisis.Locale.Translate("workspace.entity_menu.base.rename"), delegate
		{
			Ui.OpenRenameEntity(_entity);
		});
		entity = _entity;
		IDeletable deletable = entity as IDeletable;
		if (deletable != null)
		{
			menu.Separator();
			entity = _entity;
			ActorEntity actor = entity as ActorEntity;
			if (actor != null)
			{
				menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.duplicate"), delegate
				{
					DuplicateActor(actor);
				});
			}
			entity = _entity;
			LightEntity light = entity as LightEntity;
			if (light != null)
			{
				menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.duplicate"), delegate
				{
					DuplicateLight(light);
				});
			}
			entity = _entity;
			OverlayEntity overlay = entity as OverlayEntity;
			if (overlay != null)
			{
				menu.Action(Ktisis.Locale.Translate("workspace.entity_menu.base.duplicate"), delegate
				{
					DuplicateOverlay(overlay);
				});
			}
			if (_entity is LightEntity { WorldLight: not null })
			{
				menu.Action("Untrack", delegate
				{
					deletable.Delete();
				});
			}
			else
			{
				menu.Action("Delete", delegate
				{
					deletable.Delete();
				});
			}
		}
		entity = _entity;
		ObjectEntity obj = entity as ObjectEntity;
		if (obj != null)
		{
			menu.Separator();
			menu.Action("Reset", delegate
			{
				obj.Reset();
			});
			menu.Action("Untrack", delegate
			{
				obj.Remove();
			});
		}
	}

	private void BuildEntityType(ContextMenuBuilder menu)
	{
		SceneEntity entity = _entity;
		if (!(entity is ActorEntity actor))
		{
			if (!(entity is EntityPose pose))
			{
				if (entity is LightEntity light)
				{
					BuildLightMenu(menu, light);
				}
			}
			else
			{
				BuildPoseMenu(menu, pose);
			}
		}
		else
		{
			BuildActorMenu(menu, actor);
		}
	}

	private void OpenEditor()
	{
		Ui.OpenEditorFor(_entity);
	}

	private unsafe void BuildActorMenu(ContextMenuBuilder menu, ActorEntity actor)
	{
		menu.Separator().Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.target"), actor.Actor.SetGPoseTarget).Separator()
			.Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.edit"), OpenEditor)
			.Group(delegate(ContextMenuBuilder sub)
			{
				BuildActorIpcMenu(sub, actor);
			})
			.Separator()
			.SubMenu(Ktisis.Locale.Translate("workspace.entity_menu.actor.import"), delegate(ContextMenuBuilder sub)
			{
				ContextMenuBuilder contextMenuBuilder = sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.chara"), delegate
				{
					Ui.OpenCharaImport(actor);
				}).Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.npc"), delegate
				{
					Ui.OpenCharaImport(actor, openNpc: true);
				}).Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.pose"), delegate
				{
					Ui.OpenPoseImport(actor);
				});
				if (_ctx.Plugin.Ipc.IsAnyMcdfActive && actor.GetHuman() != null)
				{
					contextMenuBuilder.Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.mcdf"), delegate
					{
						Ui.OpenMcdfFile(delegate(string path)
						{
							ImportMcdf(actor, path);
						});
					});
				}
			})
			.SubMenu(Ktisis.Locale.Translate("workspace.entity_menu.actor.export"), delegate(ContextMenuBuilder sub)
			{
				sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.chara"), delegate
				{
					Ui.OpenCharaExport(actor);
				}).Action(Ktisis.Locale.Translate("workspace.entity_menu.actor.pose"), delegate
				{
					ExportPose(actor.Pose);
				});
			});
	}

	private unsafe void BuildActorIpcMenu(ContextMenuBuilder menu, ActorEntity actor)
	{
		menu.SubMenu(Ktisis.Locale.Translate("workspace.entity_menu.ipc.submenu"), delegate(ContextMenuBuilder sub)
		{
			bool flag = actor.GetHuman() != null;
			if (_ctx.Plugin.Ipc.IsPenumbraActive)
			{
				sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.ipc.penumbra.collection"), delegate
				{
					Ui.OpenAssignCollection(actor);
				});
				if (flag)
				{
					sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.ipc.penumbra.invisible_skin"), delegate
					{
						_ctx.Characters.Mcdf.SetInvisibleSkin(actor);
					});
				}
			}
			if (_ctx.Plugin.Ipc.IsGlamourerActive)
			{
				sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.ipc.glamourer.design"), delegate
				{
					Ui.OpenApplyDesign(actor);
				});
			}
			if (_ctx.Plugin.Ipc.IsCustomizeActive)
			{
				sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.ipc.customize.profile"), delegate
				{
					Ui.OpenAssignCProfile(actor);
				});
			}
			if (_ctx.Plugin.Ipc.IsAnyMcdfActive && flag)
			{
				sub.Action(Ktisis.Locale.Translate("workspace.entity_menu.ipc.revert"), delegate
				{
					_ctx.Characters.Mcdf.Revert(actor.Actor);
					actor.AssignedProfile = null;
				});
			}
		});
	}

	private void ImportMcdf(ActorEntity actor, string path)
	{
		_ctx.Characters.Mcdf.LoadAndApplyTo(path, actor.Actor);
	}

	private async void DuplicateActor(ActorEntity actor)
	{
		CharaFile file = await _ctx.Characters.SaveCharaFile(actor);
		ActorEntity actorEntity = await _ctx.Scene.Factory.CreateActor().WithAppearance(file).Spawn();
		if (_ctx.Plugin.Ipc.IsGlamourerActive)
		{
			_ctx.Plugin.Ipc.GetGlamourerIpc().CopyState(actor.Actor.ObjectIndex, actorEntity.Actor.ObjectIndex);
		}
	}

	private void BuildPoseMenu(ContextMenuBuilder menu, EntityPose pose)
	{
		menu.Separator().Action(Ktisis.Locale.Translate("workspace.entity_menu.pose.import"), delegate
		{
			ImportPose(pose);
		}).Action(Ktisis.Locale.Translate("workspace.entity_menu.pose.export"), delegate
		{
			ExportPose(pose);
		})
			.Separator()
			.Action(Ktisis.Locale.Translate("workspace.entity_menu.pose.reference"), delegate
			{
				_ctx.Posing.ApplyReferencePose(pose);
			});
	}

	private void ImportPose(EntityPose pose)
	{
		if (pose.Parent is ActorEntity actor)
		{
			Ui.OpenPoseImport(actor);
		}
	}

	private async void ExportPose(EntityPose? pose)
	{
		if (pose != null)
		{
			await Ui.OpenPoseExport(pose);
		}
	}

	private void BuildLightMenu(ContextMenuBuilder menu, LightEntity light)
	{
		menu.Separator().Action(Ktisis.Locale.Translate("workspace.entity_menu.light.edit"), OpenEditor).Separator()
			.Action(Ktisis.Locale.Translate("workspace.entity_menu.light.import"), delegate
			{
				Ui.OpenLightFile(delegate(string path, LightFile file)
				{
					ImportLight(light, file);
				});
			})
			.Action(Ktisis.Locale.Translate("workspace.entity_menu.light.export"), delegate
			{
				Ui.OpenLightExport(light);
			});
	}

	private async void ImportLight(LightEntity light, LightFile file)
	{
		await _ctx.Scene.ApplyLightFile(light, file);
	}

	private async void DuplicateLight(LightEntity light)
	{
		ImportLight(file: await _ctx.Scene.SaveLightFile(light), light: await _ctx.Scene.Factory.CreateLight().Spawn());
	}

	private void DuplicateOverlay(OverlayEntity overlay)
	{
		EntityType type = overlay.Type;
		OverlayTypes overlayTypes = default(OverlayTypes);
		switch (type)
		{
		case EntityType.TalkOverlay:
			overlayTypes = OverlayTypes.Talk;
			break;
		case EntityType.BalloonOverlay:
			overlayTypes = OverlayTypes.Balloon;
			break;
		case EntityType.StatusOverlay:
			overlayTypes = OverlayTypes.Status;
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(type);
			break;
		}
		OverlayTypes type2 = overlayTypes;
		OverlayEntity overlayEntity = _ctx.Scene.Factory.BuildOverlay(type2).Add();
		overlayEntity.Alpha = overlay.Alpha * 255f;
		overlayEntity.Position = overlay.Position;
		overlayEntity.Scale = overlay.Scale;
		overlayEntity.Visible = overlay.Visible;
		if (overlayEntity is TalkOverlay talkOverlay && overlay is TalkOverlay talkOverlay2)
		{
			talkOverlay.Speaker = talkOverlay2.Speaker;
			talkOverlay.Background = talkOverlay2.Background;
			talkOverlay.Cursor = talkOverlay2.Cursor;
			talkOverlay.Dialog = talkOverlay2.Dialog;
		}
		else if (overlayEntity is BalloonOverlay balloonOverlay && overlay is BalloonOverlay balloonOverlay2)
		{
			balloonOverlay.Background = balloonOverlay2.Background;
			balloonOverlay.Dialog = balloonOverlay2.Dialog;
			balloonOverlay.Arrow = balloonOverlay2.Arrow;
			balloonOverlay.ArrowX = balloonOverlay2.ArrowX;
		}
		else if (overlayEntity is StatusOverlay statusOverlay && overlay is StatusOverlay statusOverlay2)
		{
			statusOverlay.IconPath = statusOverlay2.IconPath;
			statusOverlay.StatusText = statusOverlay2.StatusText;
			statusOverlay.StatusType = statusOverlay2.StatusType;
		}
	}
}

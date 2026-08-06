using System.IO;
using GLib.Popups.Context;
using Ktisis.Common.Extensions;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Factory.Types;
using Ktisis.Structs.Lights;

namespace Ktisis.Interface.Editor.Context;

public class SceneCreateMenuBuilder
{
	private readonly IEditorContext _ctx;

	private IEntityFactory Factory => _ctx.Scene.Factory;

	public SceneCreateMenuBuilder(IEditorContext ctx)
	{
		_ctx = ctx;
	}

	public ContextMenu Create()
	{
		return new ContextMenuBuilder().Group(BuildActorGroup).Separator().Group(BuildLightGroup)
			.Separator()
			.Group(BuildUtilityGroup)
			.Build($"##SceneCreateMenu_{GetHashCode():X}");
	}

	public ContextMenu CreateActor()
	{
		return new ContextMenuBuilder().Group(BuildActorGroup).Build($"##SceneCreateActorMenu_{GetHashCode():X}");
	}

	public ContextMenu CreateLight()
	{
		return new ContextMenuBuilder().Group(BuildLightMenu).Build($"##SceneCreateLightMenu_{GetHashCode():X}");
	}

	public ContextMenu CreateOverlay()
	{
		return new ContextMenuBuilder().Group(BuildOverlayGroup).Build($"##SceneCreateOverlayMenu_{GetHashCode():X}");
	}

	private void BuildActorGroup(ContextMenuBuilder sub)
	{
		sub.Action(Ktisis.Locale.Translate("workspace.create_menu.actor.create"), delegate
		{
			Factory.CreateActor().Spawn();
		}).Action(Ktisis.Locale.Translate("workspace.create_menu.actor.file"), ImportCharaFromFile).Action(Ktisis.Locale.Translate("workspace.create_menu.actor.mcdf"), ImportCharaFromMcdf)
			.Action(Ktisis.Locale.Translate("workspace.create_menu.actor.overworld"), _ctx.Interface.OpenOverworldActorList)
			.Separator()
			.Action("Refresh scene entities", delegate
			{
				_ctx.Interface.RefreshSceneEntities();
			});
	}

	private void BuildLightGroup(ContextMenuBuilder sub)
	{
		sub.SubMenu(Ktisis.Locale.Translate("workspace.create_menu.light.create"), BuildLightMenu);
	}

	private void BuildLightMenu(ContextMenuBuilder sub)
	{
		sub.Action(Ktisis.Locale.Translate("workspace.create_menu.light.point"), delegate
		{
			SpawnLight(LightType.PointLight);
		}).Action(Ktisis.Locale.Translate("workspace.create_menu.light.spot"), delegate
		{
			SpawnLight(LightType.SpotLight);
		}).Action(Ktisis.Locale.Translate("workspace.create_menu.light.area"), delegate
		{
			SpawnLight(LightType.AreaLight);
		})
			.Action(Ktisis.Locale.Translate("workspace.create_menu.light.directional"), delegate
			{
				SpawnLight(LightType.Directional);
			})
			.Action(Ktisis.Locale.Translate("workspace.create_menu.light.file"), delegate
			{
				ImportLightFromFile();
			});
		void SpawnLight(LightType type)
		{
			Factory.CreateLight(type).Spawn();
		}
	}

	private async void ImportLightFromFile()
	{
		_ctx.Interface.OpenLightFile(async delegate(string path, LightFile file)
		{
			Path.GetFileNameWithoutExtension(path).Truncate(32);
			LightEntity light = await Factory.CreateLight().Spawn();
			await _ctx.Scene.ApplyLightFile(light, file);
		});
	}

	private void BuildUtilityGroup(ContextMenuBuilder sub)
	{
		sub.SubMenu("Add new overlay...", BuildOverlayGroup);
		sub.Action(Ktisis.Locale.Translate("workspace.create_menu.reference"), OpenReferenceImage);
	}

	private void BuildOverlayGroup(ContextMenuBuilder sub)
	{
		sub.Action("Dialog", delegate
		{
			Factory.BuildOverlay(OverlayTypes.Talk).Add();
		}).Action("Balloon", delegate
		{
			Factory.BuildOverlay(OverlayTypes.Balloon).Add();
		}).Action("Status", delegate
		{
			Factory.BuildOverlay(OverlayTypes.Status).Add();
		})
			.Separator()
			.Action(Ktisis.Locale.Translate("workspace.create_menu.reference"), OpenReferenceImage);
	}

	private void ImportCharaFromFile()
	{
		_ctx.Interface.OpenCharaFile(delegate(string path, CharaFile file)
		{
			string name = Path.GetFileNameWithoutExtension(path).Truncate(32);
			Factory.CreateActor().WithAppearance(file).SetName(name)
				.Spawn();
		});
	}

	private void ImportCharaFromMcdf()
	{
		_ctx.Interface.OpenMcdfFile(delegate(string path)
		{
			string name = Path.GetFileNameWithoutExtension(path).Truncate(32);
			Factory.CreateActor().WithMcdf(path).SetName(name)
				.Spawn();
		});
	}

	private void OpenReferenceImage()
	{
		_ctx.Interface.OpenReferenceImages(delegate(string path)
		{
			Factory.BuildRefImage().SetPath(path).Add()
				.Save();
		});
	}
}

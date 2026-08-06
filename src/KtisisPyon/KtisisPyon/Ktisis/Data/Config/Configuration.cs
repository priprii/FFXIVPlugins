using System;
using Dalamud.Configuration;
using Ktisis.Data.Config.Actions;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Entity;
using Ktisis.Data.Config.Sections;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Data.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public const int CurrentVersion = 12;

	public CategoryConfig Categories = new CategoryConfig();

	public EditorConfig Editor = new EditorConfig();

	public FileConfig File = new FileConfig();

	public GizmoConfig Gizmo = new GizmoConfig();

	public InputConfig Keybinds = new InputConfig();

	public LocaleConfig Locale = new LocaleConfig();

	public OverlayConfig Overlay = new OverlayConfig();

	public AutoSaveConfig AutoSave = new AutoSaveConfig();

	public PresetConfig Presets = new PresetConfig();

	public PoseViewConfig PoseView = new PoseViewConfig();

	public OffsetConfig Offsets = new OffsetConfig();

	public PyonConfig Pyon = new PyonConfig();

	public int Version { get; set; } = 12;

	public EntityDisplay GetEntityDisplay(SceneEntity entity)
	{
		EntityDisplay displayForType = Editor.GetDisplayForType(entity.Type);
		if (entity is BoneNodeGroup boneNodeGroup)
		{
			BoneCategory category = boneNodeGroup.Category;
			if (category != null)
			{
				return displayForType with
				{
					Color = category.GroupColor
				};
			}
		}
		else if (entity is BoneNode && entity.Parent is BoneNodeGroup boneNodeGroup2)
		{
			BoneCategory category2 = boneNodeGroup2.Category;
			if (category2 != null)
			{
				return displayForType with
				{
					Color = (category2.LinkedColors ? category2.GroupColor : category2.BoneColor)
				};
			}
		}
		return displayForType;
	}
}

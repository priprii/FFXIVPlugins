using System.Collections.Generic;
using Dalamud.Interface;
using Ktisis.Scene.Types;

namespace Ktisis.Data.Config.Entity;

public record EntityDisplay
{
	public uint Color;

	public FontAwesomeIcon Icon;

	public DisplayMode Mode;

	private const uint BoneBlue = 4294942568u;

	private const uint ModelMint = 4290445234u;

	private const uint LightLemon = 4285066751u;

	private const uint OverlayOrange = 4278228223u;

	public EntityDisplay(uint color = uint.MaxValue, FontAwesomeIcon icon = (FontAwesomeIcon)0, DisplayMode mode = DisplayMode.Icon)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Color = color;
		Icon = icon;
		Mode = mode;
	}

	public static Dictionary<EntityType, EntityDisplay> GetDefaults()
	{
		return new Dictionary<EntityType, EntityDisplay>
		{
			{
				EntityType.Invalid,
				new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)0)
			},
			{
				EntityType.Actor,
				new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)61870)
			},
			{
				EntityType.Armature,
				new EntityDisplay(4294942568u, (FontAwesomeIcon)58594)
			},
			{
				EntityType.BoneGroup,
				new EntityDisplay(4294942568u, (FontAwesomeIcon)0, DisplayMode.None)
			},
			{
				EntityType.BoneNode,
				new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)0, DisplayMode.Dot)
			},
			{
				EntityType.Model,
				new EntityDisplay(4290445234u, (FontAwesomeIcon)58598)
			},
			{
				EntityType.ModelSlot,
				new EntityDisplay(4290445234u, (FontAwesomeIcon)0)
			},
			{
				EntityType.Weapon,
				new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)61648)
			},
			{
				EntityType.Light,
				new EntityDisplay(4285066751u, (FontAwesomeIcon)61675)
			},
			{
				EntityType.RefImage,
				new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)61502)
			},
			{
				EntityType.TalkOverlay,
				new EntityDisplay(4278228223u, (FontAwesomeIcon)61557)
			},
			{
				EntityType.BalloonOverlay,
				new EntityDisplay(4278228223u, (FontAwesomeIcon)61574)
			},
			{
				EntityType.StatusOverlay,
				new EntityDisplay(4278228223u, (FontAwesomeIcon)61568)
			}
		};
	}
}

using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace TargetPyon;

[Serializable]
public class Config : IPluginConfiguration
{
	[NonSerialized]
	private IDalamudPluginInterface? PluginInterface;

	public int Version { get; set; } = 1;

	public bool Enabled { get; set; } = true;

	public bool OverlayVisible { get; set; } = true;

	public bool IncludeSoftTarget { get; set; }

	public bool ClickThrough { get; set; }

	public bool ClickThroughBypassCtrl { get; set; }

	public bool ClickThroughBypassShift { get; set; }

	public bool ClickThroughBypassAlt { get; set; } = true;

	public bool LockPosition { get; set; }

	public bool CustomizationMode { get; set; }

	public ClickButton TargetClickButton { get; set; }

	public ClickButton RemoveClickButton { get; set; } = ClickButton.Middle;

	public ClickButton PlateClickButton { get; set; } = ClickButton.None;

	public ClickButton InspectClickButton { get; set; } = ClickButton.None;

	public ClickButton CamOrbitClickButton { get; set; } = ClickButton.None;

	public ClickButton ContextClickButton { get; set; } = ClickButton.Right;

	public bool ChatAlert { get; set; }

	public int SoundID { get; set; } = 16;

	public bool UseCustomAudioAlert { get; set; }

	public bool UseGameSFXVolume { get; set; } = true;

	public int AudioVolume { get; set; } = 100;

	public bool NoDutyAllyAlert { get; set; } = true;

	public bool PvEAllyAlert { get; set; } = true;

	public bool PvPAllyAlert { get; set; }

	public bool PvPEnemyAlert { get; set; } = true;

	public bool OnlyShowNearbyPlayers { get; set; }

	public int ShowTarget { get; set; } = 1;

	public int ShowTargeters { get; set; } = 1;

	public int MaxPlayers { get; set; } = 10;

	public int UpdateMs { get; set; } = 250;

	public int DisplayTime { get; set; }

	public int OverlayWidth { get; set; } = 180;

	public int OverlayHeight { get; set; } = 180;

	public float OverlayBGOpacity { get; set; } = 0.5f;

	public int Font { get; set; } = 1;

	public int FontSize { get; set; } = 16;

	public float FontScale { get; set; } = 1f;

	public int FontOutline { get; set; } = 1;

	public float MarkerSize { get; set; } = 3f;

	public bool OnlyShowMarkerOnHover { get; set; } = true;

	public Vector4 MarkerColour { get; set; } = new Vector4(1f, 0f, 0f, 0.8f);

	public Vector4 TargetColour { get; set; } = new Vector4(1f, 1f, 1f, 1f);

	public Vector4 OutlineColour { get; set; } = new Vector4(0.4862745f, 8f / 51f, 0.6039216f, 1f);

	public string CurrentTargetFormat { get; set; } = "[%h%:%m%]%dir% %fn% %sn%";

	public Vector4 NoTargetColour { get; set; } = new Vector4(0.7058824f, 0.7058824f, 0.7058824f, 1f);

	public Vector4 NoTargetOutlineColour { get; set; } = new Vector4(0f, 0f, 0f, 1f);

	public string PreviousTargetFormat { get; set; } = "[%h%:%m%]%dir% %fn% %sn%";

	public Vector4 PlayersTargetColour { get; set; } = new Vector4(31f / 51f, 31f / 51f, 31f / 51f, 1f);

	public Vector4 PlayersTargetOutlineColour { get; set; } = new Vector4(0f, 0f, 0f, 1f);

	public string PlayersTargetFormat { get; set; } = "%d% %fn% %sn%";

	public int PlayersTargetIndent { get; set; } = 30;

	public string CustomDirLeft { get; set; } = "";

	public string CustomDirRight { get; set; } = "";

	public string CustomDirBoth { get; set; } = "";

	public int DirectionIconLeftOffset { get; set; }

	public int DirectionIconRightOffset { get; set; }

	public int DirectionIconSizeOffset { get; set; }

	public int DirectionIconMinDistance { get; set; }

	public bool PlayerVisibilityFilter { get; set; }

	public bool ObjectVisibilityFilter { get; set; }

	public int ListPlayersMax { get; set; } = 200;

	public bool ListPlayersOrderByDistance { get; set; }

	public int ListObjectsMax { get; set; } = 500;

	public bool ListObjectsOrderByDistance { get; set; }

	public ObjectTypeFilter ListObjectsTypeFilter { get; set; } = ObjectTypeFilter.Aetheryte | ObjectTypeFilter.BattleNpc | ObjectTypeFilter.Companion | ObjectTypeFilter.EventNpc | ObjectTypeFilter.EventObj | ObjectTypeFilter.GatheringPoint | ObjectTypeFilter.Housing | ObjectTypeFilter.Ornament | ObjectTypeFilter.Light;

	public void Initialize(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}

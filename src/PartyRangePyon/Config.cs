using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface;
using Dalamud.Plugin;

namespace PartyRangePyon;

[Serializable]
public class Config : IPluginConfiguration
{
	[NonSerialized]
	private IDalamudPluginInterface? PluginInterface;

	public int Version { get; set; }

	public bool Enabled { get; set; } = true;

	public float CloseRangeMax { get; set; } = 15f;

	public float MidRangeMax { get; set; } = 20f;

	public string TextFormat { get; set; } = "00";

	public int Font { get; set; } = 5;

	public int FontSize { get; set; } = 13;

	public float FontScale { get; set; } = 1f;

	public int TextOutline { get; set; } = 2;

	public float TextPosX { get; set; } = -41.5f;

	public float TextPosY { get; set; } = -6.7f;

	public Vector4 OutlineColour { get; set; } = ColorHelpers.Vector(KnownColor.Black);

	public Vector4 CloseRangeColour { get; set; } = ColorHelpers.Vector(KnownColor.White);

	public Vector4 MidRangeColour { get; set; } = ColorHelpers.Vector(KnownColor.Orange);

	public Vector4 FarRangeColour { get; set; } = ColorHelpers.Vector(KnownColor.Red);

	public int UpdateMs { get; set; } = 10;

	public void Initialize(IDalamudPluginInterface pluginInterface)
	{
		PluginInterface = pluginInterface;
	}

	public void Save()
	{
		PluginInterface.SavePluginConfig((IPluginConfiguration)(object)this);
	}
}

using System;
using System.Text.Json.Serialization;
using PyonPix.Shared.Structs.Pix.Properties;

namespace PyonPix.Config.Pix;

[Serializable]
public class PixVariant
{
	public bool Active;

	public bool IsSynced;

	public DateTime LastSeenUtc = DateTime.UtcNow;

	public bool PersistentCache;

	public bool SyncCookies = true;

	public bool ScreenInteraction = true;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public BrowserPixVariantOverrides? Browser;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public RendererPixVariantOverrides? Renderer;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public LightPixVariantOverrides? Light;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public AudioPixVariantOverrides? Audio;

	public BrowserPixVariantOverrides EnsureBrowser()
	{
		return Browser ?? (Browser = new BrowserPixVariantOverrides());
	}

	public RendererPixVariantOverrides EnsureRenderer()
	{
		return Renderer ?? (Renderer = new RendererPixVariantOverrides());
	}

	public LightPixVariantOverrides EnsureLight()
	{
		return Light ?? (Light = new LightPixVariantOverrides());
	}

	public AudioPixVariantOverrides EnsureAudio()
	{
		return Audio ?? (Audio = new AudioPixVariantOverrides());
	}

	public void PruneEmpty()
	{
		BrowserPixVariantOverrides? browser = Browser;
		if (browser == null || !browser.HasAny)
		{
			Browser = null;
		}
		RendererPixVariantOverrides? renderer = Renderer;
		if (renderer == null || !renderer.HasAny)
		{
			Renderer = null;
		}
		LightPixVariantOverrides? light = Light;
		if (light == null || !light.HasAny)
		{
			Light = null;
		}
		AudioPixVariantOverrides? audio = Audio;
		if (audio == null || !audio.HasAny)
		{
			Audio = null;
		}
	}
}

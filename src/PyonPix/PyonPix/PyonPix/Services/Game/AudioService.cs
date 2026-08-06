using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.Config;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Interop;
using PyonPix.Services.Core;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Structs.Audio;
using PyonPix.Structs.Renderer;
using PyonPix.Ui;

namespace PyonPix.Services.Game;

public class AudioService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private long SpatialAudioTick;

	private const uint SpatialAudioTickRate = 100u;

	public float MasterVolume = 1f;

	public bool MasterVolumeMuted;

	private PixService? PixService => Services.Get<PixService>();

	private StateService? StateService => Services.Get<StateService>();

	private BrowserService? BrowserService => Services.Get<BrowserService>();

	private RendererService? RendererService => Services.Get<RendererService>();

	public override async Task Initialize()
	{
		bool masterVolumeMuted = default(bool);
		if (Services.GameConfig.System.TryGetBool("IsSndMaster", ref masterVolumeMuted))
		{
			MasterVolumeMuted = masterVolumeMuted;
		}
		uint num = default(uint);
		if (Services.GameConfig.System.TryGetUInt("SoundMaster", ref num))
		{
			MasterVolume = (float)num / 100f;
		}
		Services.GameConfig.SystemChanged += GameConfig_SystemChanged;
	}

	private void GameConfig_SystemChanged(object? sender, ConfigChangeEvent e)
	{
		bool masterVolumeMuted = default(bool);
		if (e.Name == "IsSndMaster" && Services.GameConfig.System.TryGetBool(e.Name, ref masterVolumeMuted))
		{
			MasterVolumeMuted = masterVolumeMuted;
		}
		uint num = default(uint);
		if (e.Name == "SoundMaster" && Services.GameConfig.System.TryGetUInt(e.Name, ref num))
		{
			MasterVolume = (float)num / 100f;
		}
	}

	public override void Update()
	{
		if (RendererService != null && RendererService.Renderers.Count != 0)
		{
			CalculateSpatialAudio(RendererService.Renderers.Values);
		}
	}

	public void CalculateSpatialAudio(Dictionary<string, Renderer>.ValueCollection renderers)
	{
		if (PixService == null || PixService.SpawnedPixs.Count == 0 || BrowserService == null || StateService == null)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount - SpatialAudioTick < 100)
		{
			return;
		}
		SpatialAudioTick = tickCount;
		AudioGlobalProperties audio = Config.Global.Audio;
		bool flag = audio.ListenerType == AudioListenerType.Camera || Services.Objects.LocalPlayer == null;
		Vector3 vector;
		Vector3 vector2;
		if (flag)
		{
			Matrix4x4.Invert(CameraService.GetViewMatrix(), out var result);
			vector = result.Translation;
			vector2 = Vector3.Normalize(new Vector3(result.M11, result.M12, result.M13));
		}
		else
		{
			vector = StateService.LocalPlayerPosition;
			vector2 = Vector3.Normalize(Vector3.Transform(Vector3.UnitX, StateService.LocalPlayerRotation));
		}
		float num = (((audio.MuteInBackground && !Win32Interop.IsGameFocused) || (audio.UseGameMuteState && MasterVolumeMuted)) ? 0f : (audio.UseGameMasterVolume ? MasterVolume : audio.MasterVolume));
		foreach (Renderer renderer in renderers)
		{
			if (renderer.ScreenTransform.HasValue && PixService.SpawnedPixs.TryGetValue(renderer.PixId, out IPix value))
			{
				AudioPixProperties audio2 = value.Audio;
				if (audio2.SpatialEnabled)
				{
					Vector3 value2 = renderer.ScreenTransform.Value.Translation - vector;
					float num2 = value2.Length();
					float num3 = MathF.Max(0.01f, audio2.FalloffMaxDistance);
					float num4 = MathF.Min(num2 / num3, 1f);
					float num5 = 1f - num4 * num4 * (3f - 2f * num4);
					float num6 = audio2.Volume * num5 * num;
					value2.Y = 0f;
					vector2.Y = 0f;
					value2 = Vector3.Normalize(value2);
					vector2 = Vector3.Normalize(vector2);
					float y = (flag ? Vector3.Dot(value2, vector2) : (0f - Vector3.Dot(value2, vector2)));
					y = MathF.Max(-1f, MathF.Min(1f, y));
					float left = num6 * MathF.Sqrt(0.5f * (1f - y));
					float right = num6 * MathF.Sqrt(0.5f * (1f + y));
					BrowserService.UpdateSpatialAudio(renderer.PixId, left, right);
				}
			}
		}
	}

	public override Task Dispose()
	{
		Services.GameConfig.SystemChanged -= GameConfig_SystemChanged;
		return Task.CompletedTask;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace TargetPyon;

public class AudioManager : IDisposable
{
	private List<IWavePlayer> Instances;

	private bool Disposed;

	private readonly string AudioFile = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory.FullName, "Alert.mp3");

	public bool AudioFileExists => File.Exists(AudioFile);

	public AudioManager()
	{
		Instances = new List<IWavePlayer>();
	}

	public void SetAudioFile(string path)
	{
		if (AudioFileExists)
		{
			File.Delete(AudioFile);
		}
		File.Copy(path, AudioFile);
	}

	public void Play()
	{
		if (!AudioFileExists)
		{
			return;
		}
		new Thread((ThreadStart)delegate
		{
			try
			{
				Guid dSDEVID_DefaultPlayback = DirectSoundOut.DSDEVID_DefaultPlayback;
				using MediaFoundationReader sourceStream = new MediaFoundationReader(AudioFile);
				using WaveChannel32 waveProvider = new WaveChannel32(sourceStream)
				{
					Volume = GetVolume(),
					PadWithZeroes = false
				};
				using DirectSoundOut directSoundOut = new DirectSoundOut(dSDEVID_DefaultPlayback);
				directSoundOut.Init(waveProvider);
				lock (Instances)
				{
					Instances.Add(directSoundOut);
				}
				directSoundOut.Play();
				while (directSoundOut.PlaybackState == PlaybackState.Playing)
				{
					Thread.Sleep(100);
				}
			}
			catch (Exception ex)
			{
				Plugin.PluginLog.Error(ex, "AudioPlayback Exception", Array.Empty<object>());
			}
			finally
			{
				lock (Instances)
				{
					Instances.RemoveAll((IWavePlayer instance) => instance.PlaybackState != PlaybackState.Playing);
				}
			}
		}).Start();
	}

	public float GetVolume()
	{
		return (float)Math.Min(Plugin.Config.AudioVolume, 100) * (Plugin.Config.UseGameSFXVolume ? GetEffectiveSfxVolume() : 1f) / 100f;
	}

	private float GetEffectiveSfxVolume()
	{
		if (GameConfig.System.GetBool("IsSndSe") || GameConfig.System.GetBool("IsSndMaster"))
		{
			return 0f;
		}
		return (float)GameConfig.System.GetUInt("SoundSe") / 100f * ((float)GameConfig.System.GetUInt("SoundMaster") / 100f);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (Disposed)
		{
			return;
		}
		if (disposing)
		{
			foreach (IWavePlayer instance in Instances)
			{
				instance.Dispose();
			}
			Instances.Clear();
		}
		Disposed = true;
	}

	~AudioManager()
	{
		Dispose(disposing: false);
	}
}

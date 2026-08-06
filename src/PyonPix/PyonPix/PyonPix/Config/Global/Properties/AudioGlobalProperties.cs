using PyonPix.Structs.Audio;

namespace PyonPix.Config.Global.Properties;

public class AudioGlobalProperties
{
	public float MasterVolume = 1f;

	public AudioListenerType ListenerType;

	public bool UseGameMasterVolume = true;

	public bool UseGameMuteState = true;

	public bool MuteInBackground = true;
}

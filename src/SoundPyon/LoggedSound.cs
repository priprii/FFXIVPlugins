using System;

namespace SoundPyon;

public class LoggedSound
{
	public string Path { get; set; } = "";

	public int Count { get; set; }

	public DateTime LastPlayed { get; set; } = DateTime.MinValue;
}

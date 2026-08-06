using System;

namespace TriggerPyon;

public record RGB(byte R, byte G, byte B)
{
	public string ToHexColorCode()
	{
		return $"#{R:X2}{G:X2}{B:X2}";
	}

	public static RGB? FromHexColourCode(string hexColourCode)
	{
		if (string.IsNullOrWhiteSpace(hexColourCode))
		{
			return null;
		}
		string text = hexColourCode.Trim();
		if (text.StartsWith('#'))
		{
			text = text.Substring(1);
		}
		if (text.Length == 3)
		{
			string value = new string(text[0], 2);
			string value2 = new string(text[1], 2);
			return new RGB(B: Convert.ToByte(new string(text[2], 2), 16), R: Convert.ToByte(value, 16), G: Convert.ToByte(value2, 16));
		}
		if (text.Length == 6)
		{
			byte r = Convert.ToByte(text.Substring(0, 2), 16);
			byte g = Convert.ToByte(text.Substring(2, 2), 16);
			byte b = Convert.ToByte(text.Substring(4, 2), 16);
			return new RGB(r, g, b);
		}
		return null;
	}

	public uint ToUInt(byte alpha = byte.MaxValue)
	{
		return (uint)((alpha << 24) | (B << 16) | (G << 8) | R);
	}
}

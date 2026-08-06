using System;
using Lumina.Text;

namespace TriggerPyon;

public class GradientStyle
{
	public GradientAnimationStyle AnimationStyle;

	public string Name;

	public byte[,] Colours;

	public int? ColourSet;

	public GradientStyle(string name, string b64, GradientAnimationStyle animStyle)
	{
		Name = name;
		AnimationStyle = animStyle;
		Colours = Decode(b64);
		ColourSet = null;
	}

	public GradientStyle(string name, byte[,] colours, GradientAnimationStyle animStyle)
	{
		Name = name;
		AnimationStyle = animStyle;
		Colours = colours;
		ColourSet = null;
	}

	private static byte[,] Decode(string b64)
	{
		byte[] array = Convert.FromBase64String(b64);
		byte[,] array2 = new byte[array.Length / 3, 3];
		for (int i = 0; i < array.Length; i += 3)
		{
			array2[i / 3, 0] = array[i];
			array2[i / 3, 1] = array[i + 1];
			array2[i / 3, 2] = array[i + 2];
		}
		return array2;
	}

	public void Apply(SeStringBuilder builder, string title, bool animate)
	{
		if (!animate)
		{
			ApplyStatic(builder, title);
			return;
		}
		switch (AnimationStyle)
		{
		case GradientAnimationStyle.Wave:
			ApplyWave(builder, title);
			break;
		case GradientAnimationStyle.Pulse:
			ApplyPulse(builder, title);
			break;
		default:
			ApplyStatic(builder, title);
			break;
		}
	}

	private void ApplyPulse(SeStringBuilder builder, string title)
	{
		RGB colourRGB = GradientSystem.GetColourRGB(this, 0, 5);
		builder.PushEdgeColorRgba(colourRGB.R, colourRGB.G, colourRGB.B, byte.MaxValue);
		builder.Append(title);
		builder.PopEdgeColor();
	}

	private void ApplyWave(SeStringBuilder builder, string title)
	{
		if (title.Length > 25)
		{
			for (int i = 0; i < title.Length; i += 2)
			{
				RGB colourRGB = GradientSystem.GetColourRGB(this, i, 5);
				builder.PushEdgeColorRgba(colourRGB.R, colourRGB.G, colourRGB.B, byte.MaxValue);
				builder.Append(title.Substring(i, Math.Min(2, title.Length - i)));
				builder.PopEdgeColor();
			}
			return;
		}
		int num = 0;
		foreach (char c in title)
		{
			RGB colourRGB2 = GradientSystem.GetColourRGB(this, num++, 5);
			builder.PushEdgeColorRgba(colourRGB2.R, colourRGB2.G, colourRGB2.B, byte.MaxValue);
			builder.AppendChar((int)c);
			builder.PopEdgeColor();
		}
	}

	private void ApplyStatic(SeStringBuilder builder, string title)
	{
		int length = Colours.GetLength(0);
		if (title.Length > 25)
		{
			for (int i = 0; i < title.Length; i += 2)
			{
				int chrIndex = (int)MathF.Round((float)i / (float)title.Length * (float)length);
				RGB colourRGB = GradientSystem.GetColourRGB(this, chrIndex, 5, animate: false);
				builder.PushEdgeColorRgba(colourRGB.R, colourRGB.G, colourRGB.B, byte.MaxValue);
				builder.Append(title.Substring(i, Math.Min(2, title.Length - i)));
				builder.PopEdgeColor();
			}
			return;
		}
		int num = 0;
		foreach (char c in title)
		{
			RGB colourRGB2 = GradientSystem.GetColourRGB(this, (int)MathF.Round((float)num++ / (float)title.Length * (float)length), 5, animate: false);
			builder.PushEdgeColorRgba(colourRGB2.R, colourRGB2.G, colourRGB2.B, byte.MaxValue);
			builder.AppendChar((int)c);
			builder.PopEdgeColor();
		}
	}
}

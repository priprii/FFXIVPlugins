using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TriggerPyon;

public static class GradientSystem
{
	private static readonly List<GradientStyle> ColourLists;

	private static readonly Dictionary<(int, GradientAnimationStyle?), GradientStyle> GradientStyles;

	private static readonly List<(string Name, string Base64)> ColourSets;

	public static readonly RGB White;

	private static readonly Stopwatch TimeSinceStart;

	public static int NumColourLists => ColourLists.Count;

	public static int NumColourSets => ColourSets.Count;

	static GradientSystem()
	{
		ColourLists = new List<GradientStyle>();
		GradientStyles = new Dictionary<(int, GradientAnimationStyle?), GradientStyle>();
		int num = 15;
		List<(string, string)> list = new List<(string, string)>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<(string, string)> span = CollectionsMarshal.AsSpan(list);
		span[0] = ("Pride Rainbow", "5AMD6RsC7TMC8ksB92MB/HsA/5EA/6IA/7IA/8MA/9QA/+UA5+MEutAKjr0RYaoYNZYeCIMlAHlFAG9rAGaRAF23AFTdAkv9FkXnKj/RPjm8UjOmZi2QcymCcymCcymCcymCcymCcymCZi2QUjOmPjm8Kj/RFkXnAkv9AFTdAF23AGaRAG9rAHlFCIMlNZYeYaoYjr0RutAK5+ME/+UA/9QA/8MA/7IA/6IA/5EA/HsA92MB8ksB7TMC6RsC5AMD");
		span[1] = ("Transgender", "W876b8nygsXplsDhqbvYvbfQ0LLI5K2/9aq59rXC+MDL+cvU+tbd/OHm/ezv/vf4//z9/fH0/Obr+9zi+tHZ+MbQ97vH9rC+7qu72q/Ex7TMs7nUn77djMLleMftZcz2Zcz2eMftjMLln77ds7nUx7TM2q/E7qu79rC+97vH+MbQ+tHZ+9zi/Obr/fH0//z9/vf4/ezv/OHm+tbd+cvU+MDL9rXC9aq55K2/0LLIvbfQqbvYlsDhgsXpb8nyW876");
		span[2] = ("Lesbian", "1S0A2lQT33ol46E46MdL7e5d8Opg9Nhe98Zc+rVZ/aNX/6Rm/7eG/8qm/93H//Hn/fj79Nnp67vY4p3G2n+10WKkzGCgxl2cwVuZvFmVtleRskqJrzqBrCp4qBpvpQpmpQpmqBpvrCp4rzqBskqJtleRvFmVwVuZxl2czGCg0WKk2oC1457H67zY9Nrp/fj7//Hn/93H/8qm/7eG/6Rm/aNX+rVZ98dc9Nhe8Opg7exd6MZK46A43nkl2lMT1S0A");
		span[3] = ("Bisexual", "1gJwzgx1xxZ6vyB/uCmDsDOIqT2NoUeSm0+Wm0+Wm0+Wm0+WlU6XgUuZbUibWUWeRkKgMj+iHjylCjmnCjmnHjylMj+iRkKgWUWebUibgUuZlU6Xm0+Wm0+Wm0+Wm0+WoUeSqT2NsDOIuCmDvyB/xxZ6zgx11gJw");
		span[4] = ("Black & White", "////9/f37+/v5+fn39/f19fXzs7OxsbGvr6+tra2rq6upqamnp6elpaWjo6OhoaGfX19dXV1bW1tZWVlXV1dVVVVTU1NRUVFPT09NTU1LS0tJCQkHBwcFBQUDAwMBAQEBAQEDAwMFBQUHBwcJCQkLS0tNTU1PT09RUVFTU1NVVVVXV1dZWVlbW1tdXV1fX19hoaGjo6OlpaWnp6epqamrq6utra2vr6+xsbGzs7O19fX39/f5+fn7+/v9/f3////");
		span[5] = ("Black & Red", "/wAA9QAA6wAA4QAA1wAAzAAAwgAAuAAArgAApAAAmgAAkAAAhgAAewAAcQAAZwAAXQAAUwAASQAAPwAANQAAKwAAIAAAFgAADAAAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgAADAAAFgAAIAAAKgAANQAAPwAASQAAUwAAXQAAZwAAcQAAewAAhgAAkAAAmgAApAAArgAAuAAAwgAAzAAA1wAA4QAA6wAA9QAA/wAA");
		span[6] = ("Black & Blue", "AAD/AAD1AADrAADhAADXAADMAADCAAC4AACuAACkAACaAACQAACGAAB7AABxAABnAABdAABTAABJAAA/AAA1AAArAAAgAAAWAAAMAAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAMAAAWAAAgAAAqAAA1AAA/AABJAABTAABdAABnAABxAAB7AACGAACQAACaAACkAACuAAC4AADCAADMAADXAADhAADrAAD1AAD/");
		span[7] = ("Black & Yellow", "//8A9fUA6+sA4eEA19cAzMwAwsIAuLgArq4ApKQAmpoAkJAAhoYAe3sAcXEAZ2cAXV0AU1MASUkAPz8ANTUAKysAICAAFhYADAwAAgIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgIADAwAFhYAICAAKioANTUAPz8ASUkAU1MAXV0AZ2cAcXEAe3sAhoYAkJAAmpoApKQArq4AuLgAwsIAzMwA19cA4eEA6+sA9fUA//8A");
		span[8] = ("Black & Green", "AP8AAPUAAOsAAOEAANcAAMwAAMIAALgAAK4AAKQAAJoAAJAAAIYAAHsAAHEAAGcAAF0AAFMAAEkAAD8AADUAACsAACAAABYAAAwAAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAAwAABYAACAAACoAADUAAD8AAEkAAFMAAF0AAGcAAHEAAHsAAIYAAJAAAJoAAKQAAK4AALgAAMIAAMwAANcAAOEAAOsAAPUAAP8A");
		span[9] = ("Black & Pink", "/wD/9QD16wDr4QDh1wDXzADMwgDCuAC4rgCupACkmgCakACQhgCGewB7cQBxZwBnXQBdUwBTSQBJPwA/NQA1KwArIAAgFgAWDAAMAgACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgACDAAMFgAWIAAgKgAqNQA1PwA/SQBJUwBTXQBdZwBncQBxewB7hgCGkACQmgCapACkrgCuuAC4wgDCzADM1wDX4QDh6wDr9QD1/wD/");
		span[10] = ("Black & Cyan", "AP//APX1AOvrAOHhANfXAMzMAMLCALi4AK6uAKSkAJqaAJCQAIaGAHt7AHFxAGdnAF1dAFNTAElJAD8/ADU1ACsrACAgABYWAAwMAAICAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAICAAwMABYWACAgACoqADU1AD8/AElJAFNTAF1dAGdnAHFxAHt7AIaGAJCQAJqaAKSkAK6uALi4AMLCAMzMANfXAOHhAOvrAPX1AP//");
		span[11] = ("Cherry Blossom", "7s7/7Mj36sPv573n5bje47LW4azO3qbG3KC+2pq32pW13pG84YzD5YjK6ITR7H/Y8Hvg83bm93Lt+m30/Wn5/Wf3+Wbt9GXj72Ta6mPQ5mLH4mK+3mG02mCq1l+h0l6Yzl2Oyl2GymCEzmaI0WyM1HOP13mT2n6X34Sb4oqf5ZCi6Jam65up76Gt8qex9a60+bS4/Lq8/r/A/cHF+8LK+sPQ+cXV98bb9sfg9Mjm88rs8svy8Mz378387s7/7s7/");
		span[12] = ("Golden", "/5IA/5QE/5YI/5kL/5sP/50T/58X/6Eb/6Mf/6Yj/6gn/6or/6wv/68z/7E2/7M6/7Y+/7hC/7pG/71J/79N/8FR/8NV/8VZ/8dd/8ph/8xl/85p/9Jz/9mJ/+Cl/+a1/+uu/+2c/++L/+2D/+p+/+Z5/+N0/+Bw/9xr/9lm/9Vh/9Jc/89X/8tS/8hN/8VI/8FE/74//7s6/7c1/7Qx/7As/60n/6oi/6Yd/6MY/58T/5wO/5kK/5UF/5IA/5IA");
		span[13] = ("Pastel Rainbow", "/7y8/8K8/8i8/868/9S8/9q8/+G8/+e8/+28//O8//m8/v68+f+88/+87f+86P+84f+82/+81f+8z/+8yf+8w/+8vf+8vP/BvP/HvP/NvP/TvP/avP/gvP/mvP/svP/yvP/4vP//vPn/vPP/vOz/vOX/vN//vNj/vNL/vMz/vMX/vL//v7z/xrz/zLz/0rz/2rz/4Lz/5rz/7bz/87z/+rz//7z+/7z4/7zx/7zr/7zk/7ze/7zX/7zR/7zK/7y8");
		span[14] = ("Dark Rainbow", "MgAAMgUAMgkAMg4AMhIAMhcAMhsAMiAAMiUAMioAMi4AMTIALTIAKDIAJDIAHzIAGjIAFTIAETIADDIABzIAAzIAADICADIGADILADIQADIUADIZADIeADIiADInADIrADIwAC8yACsyACYyACEyABwyABgyABMyAA0yAAkyAAQyAQEyBQAyCgAyDwAyEwAyGQAyHgAyIgAyJwAyLAAyMQAyMgAvMgAqMgAlMgAgMgAbMgAWMgASMgANMgAAMgAA");
		ColourSets = list;
		White = new RGB(byte.MaxValue, byte.MaxValue, byte.MaxValue);
		TimeSinceStart = Stopwatch.StartNew();
		for (int i = 0; i < ColourSets.Count; i++)
		{
			(string, string) tuple = ColourSets[i];
			ColourLists.Add(new GradientStyle(tuple.Item1 + " (Wave)", tuple.Item2, GradientAnimationStyle.Wave)
			{
				ColourSet = i
			});
			ColourLists.Add(new GradientStyle(tuple.Item1 + " (Pulse)", tuple.Item2, GradientAnimationStyle.Pulse)
			{
				ColourSet = i
			});
		}
	}

	public static string GetName(int i)
	{
		if (i <= ColourLists.Count)
		{
			return ColourLists[i - 1].Name;
		}
		return "Invalid";
	}

	public static RGB GetColourRGB(int rainbowMode, int chrIndex, int throttle, bool animate = true)
	{
		if (rainbowMode == 0 || rainbowMode > ColourLists.Count)
		{
			return White;
		}
		return GetColourRGB(ColourLists[rainbowMode - 1], chrIndex, throttle, animate);
	}

	public static RGB GetColourRGB(GradientStyle style, int chrIndex, int throttle, bool animate = true)
	{
		if (throttle < 1)
		{
			throttle = 1;
		}
		byte[,] colours = style.Colours;
		long num = (animate ? (TimeSinceStart.ElapsedMilliseconds / 15) : 0);
		int num2 = ((style.AnimationStyle != GradientAnimationStyle.Pulse) ? 1 : 0);
		long num3 = (num / throttle + chrIndex * num2) % colours.GetLength(0);
		return new RGB(colours[(int)checked((nint)num3), 0], colours[(int)checked((nint)num3), 1], colours[(int)checked((nint)num3), 2]);
	}

	public static Vector3 GetColourVec3(int rainbowMode, int offset, int throttle)
	{
		RGB colourRGB = GetColourRGB(rainbowMode, offset, throttle);
		return new Vector3((float)(int)colourRGB.R / 255f, (float)(int)colourRGB.G / 255f, (float)(int)colourRGB.B / 255f);
	}

	public static Vector3 GetColourVec3(GradientStyle style, int offset, int throttle)
	{
		RGB colourRGB = GetColourRGB(style, offset, throttle);
		return new Vector3((float)(int)colourRGB.R / 255f, (float)(int)colourRGB.G / 255f, (float)(int)colourRGB.B / 255f);
	}

	public static void DrawGradientBuilder()
	{
		GradientBuilder.Draw();
	}

	public static GradientStyle? GetStyle(int i)
	{
		if (i <= 0 || i > ColourLists.Count)
		{
			return null;
		}
		return ColourLists[i - 1];
	}

	public static GradientStyle? GetStyle(int gradientColourSet, GradientAnimationStyle? gradientAnimationStyle)
	{
		if (gradientColourSet < 0 || gradientColourSet >= ColourSets.Count)
		{
			return null;
		}
		if (!GradientStyles.TryGetValue((gradientColourSet, gradientAnimationStyle), out GradientStyle value))
		{
			(string, string) tuple = ColourSets[gradientColourSet];
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted(tuple.Item1);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted((!gradientAnimationStyle.HasValue) ? "Static" : ((object)gradientAnimationStyle));
			defaultInterpolatedStringHandler.AppendLiteral(")");
			value = new GradientStyle(defaultInterpolatedStringHandler.ToStringAndClear(), tuple.Item2, gradientAnimationStyle ?? GradientAnimationStyle.Static)
			{
				ColourSet = gradientColourSet
			};
			GradientStyles[(gradientColourSet, gradientAnimationStyle)] = value;
		}
		return value;
	}
}

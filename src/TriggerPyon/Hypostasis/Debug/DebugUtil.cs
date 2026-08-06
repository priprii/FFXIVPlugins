using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hypostasis.Dalamud;

namespace Hypostasis.Debug;

public static class DebugUtil
{
	public sealed class Profiler : IDisposable
	{
		private static readonly Dictionary<string, Profiler> profilers = new Dictionary<string, Profiler>();

		private readonly string id;

		private readonly Stopwatch stopwatch = new Stopwatch();

		private readonly Stopwatch durationStopwatch = new Stopwatch();

		private long duration;

		private long count;

		private long totalTicks;

		private long highestTicks;

		private long lowestTicks;

		private Profiler(string i)
		{
			id = i;
		}

		public static Profiler Begin(string id = "", float duration = 0f)
		{
			if (!profilers.TryGetValue(id, out Profiler value))
			{
				value = (profilers[id] = new Profiler(id));
			}
			long num = (long)(duration * (float)Stopwatch.Frequency);
			if (num != value.duration)
			{
				value.duration = num;
				value.durationStopwatch.Restart();
			}
			value.stopwatch.Restart();
			return value;
		}

		public void Dispose()
		{
			stopwatch.Stop();
			long elapsedTicks = stopwatch.ElapsedTicks;
			count++;
			totalTicks += elapsedTicks;
			if (duration > 0)
			{
				if (highestTicks < elapsedTicks)
				{
					highestTicks = elapsedTicks;
				}
				if (count == 1 || lowestTicks > elapsedTicks)
				{
					lowestTicks = elapsedTicks;
				}
			}
			if (durationStopwatch.ElapsedTicks >= duration)
			{
				float num = (float)Stopwatch.Frequency / 1000f;
				string value = ((!string.IsNullOrEmpty(id)) ? (id + ", ") : string.Empty);
				if (duration > 0)
				{
					DalamudApi.LogWarning($"{value}A: {(float)totalTicks / num / (float)count:F4} / S: {(float)lowestTicks / num:F4} / L: {(float)highestTicks / num:F4} ({count} calls)");
					highestTicks = 0L;
					lowestTicks = 0L;
				}
				else
				{
					DalamudApi.LogWarning($"{value}{(float)totalTicks / num:F4} ms ({totalTicks})");
				}
				count = 0L;
				totalTicks = 0L;
				durationStopwatch.Restart();
			}
		}
	}

	public static T LogDebug<T>(this T o, string format = null)
	{
		DalamudApi.LogDebug(GetString(o, format));
		return o;
	}

	public unsafe static T* LogDebug<T>(T* o) where T : unmanaged
	{
		DalamudApi.LogDebug($"{(nint)o:X}");
		return o;
	}

	public static T LogInfo<T>(this T o, string format = null)
	{
		DalamudApi.LogInfo(GetString(o, format));
		return o;
	}

	public unsafe static T* LogInfo<T>(T* o) where T : unmanaged
	{
		DalamudApi.LogInfo($"{(nint)o:X}");
		return o;
	}

	public static T LogError<T>(this T o, string format = null)
	{
		DalamudApi.LogError(GetString(o, format));
		return o;
	}

	public unsafe static T* LogError<T>(T* o) where T : unmanaged
	{
		DalamudApi.LogError($"{(nint)o:X}");
		return o;
	}

	private static string GetString(object o, string format)
	{
		if (!string.IsNullOrEmpty(format))
		{
			return (string)(o.GetType().GetMethod("ToString", new Type[1] { typeof(string) })?.Invoke(o, new object[1] { format }) ?? o.ToString());
		}
		return o.ToString();
	}
}

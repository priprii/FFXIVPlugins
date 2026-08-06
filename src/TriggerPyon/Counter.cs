using System;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Text;

namespace TriggerPyon;

public class Counter : CounterBase
{
	public override CounterType ObjType => CounterType.Counter;

	public int Amount { get; set; }

	public bool DisplayTitle { get; set; }

	public string TitleTemplate { get; set; } = string.Empty;

	public bool TitlePrefix { get; set; } = true;

	public int TitleDuration { get; set; }

	public Vector3 TitleColour { get; set; } = new Vector3(1f, 1f, 1f);

	public Vector3? TitleGlow { get; set; } = new Vector3(1f, 1f, 1f);

	public int? TitleGradientColorSet { get; set; }

	public GradientAnimationStyle? TitleGradientAnimationStyle { get; set; }

	public int TitleMinFreq { get; set; } = 1;

	public int TitleMaxFreq { get; set; } = 1;

	public int TitleFreqThreshold { get; set; } = 1;

	public bool DisplayToast { get; set; }

	public string ToastTemplate { get; set; } = string.Empty;

	public int ToastMinFreq { get; set; } = 5;

	public int ToastMaxFreq { get; set; } = 25;

	public int ToastFreqThreshold { get; set; } = 250;

	public ToastDisplayType ToastDisplayType { get; set; }

	public ToastDisplayPosition ToastDisplayPosition { get; set; } = ToastDisplayPosition.Bottom;

	public ToastDisplaySpeed ToastDisplaySpeed { get; set; }

	public bool DisplayEcho { get; set; }

	public string EchoTemplate { get; set; } = string.Empty;

	public int EchoMinFreq { get; set; } = 5;

	public int EchoMaxFreq { get; set; } = 25;

	public int EchoFreqThreshold { get; set; } = 250;

	public SeString ToSeString(bool includeQuotes = true, bool includeColor = true, bool animate = true)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (string.IsNullOrEmpty(TitleTemplate))
		{
			return SeString.Empty;
		}
		SeStringBuilder builder = new SeStringBuilder();
		if (includeQuotes)
		{
			builder.Append("《");
		}
		if (includeColor)
		{
			builder.PushColorRgba(new Vector4(TitleColour, 1f));
		}
		AppendTitle();
		if (includeColor)
		{
			builder.PopColor();
		}
		if (includeQuotes)
		{
			builder.Append("》");
		}
		return SeString.Parse(builder.GetViewAsSpan());
		void AppendTitle()
		{
			if (!includeColor)
			{
				builder.Append(TitleTemplate);
			}
			else
			{
				if (TitleGradientColorSet.HasValue)
				{
					GradientStyle style = GradientSystem.GetStyle(TitleGradientColorSet.Value, TitleGradientAnimationStyle);
					if (style != null)
					{
						style.Apply(builder, TitleTemplate, animate);
						return;
					}
				}
				if (TitleGlow.HasValue)
				{
					builder.PushEdgeColorRgba(new Vector4(TitleGlow.Value, 1f));
					builder.Append(TitleTemplate);
					builder.PopEdgeColor();
				}
				else
				{
					builder.Append(TitleTemplate);
				}
			}
		}
	}

	public int GetDuration()
	{
		if (TitleDuration <= 0)
		{
			return Plugin.Config.CounterDuration;
		}
		return TitleDuration;
	}

	public bool CanDisplayTitle()
	{
		if (DisplayTitle)
		{
			return Amount % GetTitleFreq(Amount) == 0;
		}
		return false;
	}

	public bool CanDisplayToast()
	{
		if (DisplayToast)
		{
			return Amount % GetToastFreq(Amount) == 0;
		}
		return false;
	}

	public bool CanDisplayEcho()
	{
		if (DisplayEcho)
		{
			return Amount % GetEchoFreq(Amount) == 0;
		}
		return false;
	}

	public string GetTitleFreqText()
	{
		string text = $"{TitleMinFreq} Min, {TitleMaxFreq} Max, {TitleFreqThreshold} Threshold:\n";
		int num = -1;
		int num2 = 0;
		for (int i = 1; i <= TitleFreqThreshold; i++)
		{
			int titleFreq = GetTitleFreq(i);
			if (titleFreq != num)
			{
				if (num2++ > 15)
				{
					text += "etc..\n";
					break;
				}
				text += $"Count >= {i}, display title per {((titleFreq == 0) ? "trigger" : $"{titleFreq} triggers")}.\n";
				num = titleFreq;
			}
		}
		int titleFreq2 = GetTitleFreq(Amount);
		return text + $"\nCurrent Count = {Amount}, display title per {((titleFreq2 == 0) ? "trigger" : $"{titleFreq2} triggers")}.";
	}

	public string GetToastFreqText()
	{
		string text = $"{ToastMinFreq} Min, {ToastMaxFreq} Max, {ToastFreqThreshold} Threshold:\n";
		int num = -1;
		int num2 = 0;
		for (int i = 1; i <= ToastFreqThreshold; i++)
		{
			int toastFreq = GetToastFreq(i);
			if (toastFreq != num)
			{
				if (num2++ > 15)
				{
					text += "etc..\n";
					break;
				}
				text += $"Count >= {i}, display toast per {((toastFreq == 0) ? "trigger" : $"{toastFreq} triggers")}.\n";
				num = toastFreq;
			}
		}
		int toastFreq2 = GetToastFreq(Amount);
		return text + $"\nCurrent Count = {Amount}, display toast per {((toastFreq2 == 0) ? "trigger" : $"{toastFreq2} triggers")}.";
	}

	public string GetEchoFreqText()
	{
		string text = $"{EchoMinFreq} Min, {EchoMaxFreq} Max, {EchoFreqThreshold} Threshold:\n";
		int num = -1;
		int num2 = 0;
		for (int i = 1; i <= EchoFreqThreshold; i++)
		{
			int echoFreq = GetEchoFreq(i);
			if (echoFreq != num)
			{
				if (num2++ > 15)
				{
					text += "etc..\n";
					break;
				}
				text += $"Count >= {i}, output echo message per {((echoFreq == 0) ? "trigger" : $"{echoFreq} triggers")}.\n";
				num = echoFreq;
			}
		}
		int echoFreq2 = GetEchoFreq(Amount);
		return text + $"\nCurrent Count = {Amount}, output echo message per {((echoFreq2 == 0) ? "trigger" : $"{echoFreq2} triggers")}.";
	}

	public int GetTitleFreq(int count)
	{
		return GetCurrentFrequency(TitleMinFreq, TitleMaxFreq, TitleFreqThreshold, count);
	}

	public int GetToastFreq(int count)
	{
		return GetCurrentFrequency(ToastMinFreq, ToastMaxFreq, ToastFreqThreshold, count);
	}

	public int GetEchoFreq(int count)
	{
		return GetCurrentFrequency(EchoMinFreq, EchoMaxFreq, EchoFreqThreshold, count);
	}

	private int GetCurrentFrequency(int minFreq, int maxFreq, int freqThreshold, int count)
	{
		float t = Math.Clamp((float)count / (float)freqThreshold, 0f, 1f);
		float x = Lerp(minFreq, maxFreq, t);
		return Math.Max(1, (int)(Math.Round((double)(int)MathF.Round(x) / 5.0) * 5.0));
	}

	private float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}
}

using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using PyonPix.Utility;

namespace PyonPix.Ui.Components;

public static class Tooltip
{
	private struct TooltipRequest
	{
		public string Content;

		public string? Subtext;

		public Vector2? RectMin;

		public Vector2? RectMax;

		public Vector2? AnchorPosition;

		public Vector2? FixedSize;

		public float FadeSeconds;

		public float MaxWidth;

		public double ObservedAt;
	}

	private enum TooltipState
	{
		Idle,
		WaitingDelay,
		FadingIn,
		Visible,
		FadingOut
	}

	private static TooltipRequest? PendingRequest = null;

	private static TooltipRequest? ActiveRequest = null;

	private static TooltipState State = TooltipState.Idle;

	private static double StateStartTime = 0.0;

	private static int LastFrameDrawn = -1;

	public static void Show(string content, string? subtext = null, Vector2? rectMin = null, Vector2? rectMax = null, Vector2? anchorPosition = null, Vector2? fixedSize = null, float fadeSeconds = 0.15f, float maxWidth = 512f)
	{
		bool num = ((rectMin.HasValue && rectMax.HasValue) ? UiUtil.IsRectHovered(rectMin.Value, rectMax.Value) : ImGui.IsItemHovered((ImGuiHoveredFlags)128));
		bool hasValue = anchorPosition.HasValue;
		if (num || hasValue)
		{
			double time = ImGui.GetTime();
			TooltipRequest b = new TooltipRequest
			{
				Content = (content ?? string.Empty),
				Subtext = (string.IsNullOrEmpty(subtext) ? null : subtext),
				RectMin = rectMin,
				RectMax = rectMax,
				AnchorPosition = anchorPosition,
				FixedSize = fixedSize,
				FadeSeconds = Math.Max(0f, fadeSeconds),
				MaxWidth = maxWidth,
				ObservedAt = time
			};
			if (PendingRequest.HasValue && !TooltipRequestsDiffer(PendingRequest.Value, in b))
			{
				b.ObservedAt = PendingRequest.Value.ObservedAt;
			}
			PendingRequest = b;
		}
	}

	public static void Draw()
	{
		int frameCount = ImGui.GetFrameCount();
		if (LastFrameDrawn == frameCount)
		{
			return;
		}
		LastFrameDrawn = frameCount;
		double time = ImGui.GetTime();
		TooltipRequest? pendingRequest = PendingRequest;
		PendingRequest = null;
		if (pendingRequest.HasValue)
		{
			TooltipRequest b = pendingRequest.Value;
			if (!ActiveRequest.HasValue)
			{
				ActiveRequest = b;
				State = ((b.FadeSeconds > 0f) ? TooltipState.FadingIn : TooltipState.Visible);
				StateStartTime = time;
			}
			else if (TooltipRequestsDiffer(ActiveRequest.Value, in b))
			{
				ActiveRequest = b;
				State = ((b.FadeSeconds > 0f) ? TooltipState.FadingIn : TooltipState.Visible);
				StateStartTime = time;
			}
			else if (State == TooltipState.FadingOut)
			{
				State = ((ActiveRequest.Value.FadeSeconds > 0f) ? TooltipState.FadingIn : TooltipState.Visible);
				StateStartTime = time;
			}
		}
		else
		{
			if (!ActiveRequest.HasValue)
			{
				return;
			}
			if (!(ActiveRequest.Value.FadeSeconds > 0f))
			{
				ActiveRequest = null;
				State = TooltipState.Idle;
				StateStartTime = 0.0;
				return;
			}
			if (State != TooltipState.FadingOut)
			{
				State = TooltipState.FadingOut;
				StateStartTime = time;
			}
		}
		if (State == TooltipState.WaitingDelay)
		{
			if (!pendingRequest.HasValue)
			{
				if (ActiveRequest.HasValue)
				{
					if (ActiveRequest.Value.FadeSeconds > 0f)
					{
						State = TooltipState.FadingOut;
						StateStartTime = time;
					}
					else
					{
						ActiveRequest = null;
						State = TooltipState.Idle;
					}
				}
				return;
			}
			TooltipRequest value = pendingRequest.Value;
			ActiveRequest = value;
			State = ((value.FadeSeconds > 0f) ? TooltipState.FadingIn : TooltipState.Visible);
			StateStartTime = time;
		}
		float num = ComputeAlphaForState(time);
		if (num > 0f && ActiveRequest.HasValue)
		{
			DrawTooltip(ActiveRequest.Value, num);
		}
		if (State == TooltipState.FadingOut && ActiveRequest.HasValue)
		{
			float fadeSeconds = ActiveRequest.Value.FadeSeconds;
			if (fadeSeconds > 0f && time - StateStartTime >= (double)fadeSeconds)
			{
				ActiveRequest = null;
				State = TooltipState.Idle;
			}
		}
		if (State == TooltipState.FadingIn && ActiveRequest.HasValue)
		{
			float fadeSeconds2 = ActiveRequest.Value.FadeSeconds;
			if (fadeSeconds2 > 0f && time - StateStartTime >= (double)fadeSeconds2)
			{
				State = TooltipState.Visible;
			}
		}
	}

	private static float ComputeAlphaForState(double now)
	{
		if (!ActiveRequest.HasValue)
		{
			return 0f;
		}
		float fadeSeconds = ActiveRequest.Value.FadeSeconds;
		if (fadeSeconds <= 0f)
		{
			if (State != TooltipState.Visible && State != TooltipState.FadingIn)
			{
				return 0f;
			}
			return 1f;
		}
		switch (State)
		{
		case TooltipState.FadingIn:
			return Math.Clamp((float)((now - StateStartTime) / (double)fadeSeconds), 0f, 1f);
		case TooltipState.Visible:
			return 1f;
		case TooltipState.FadingOut:
		{
			float num = (float)((now - StateStartTime) / (double)fadeSeconds);
			return Math.Clamp(1f - num, 0f, 1f);
		}
		default:
			return 0f;
		}
	}

	private static bool TooltipRequestsDiffer(in TooltipRequest a, in TooltipRequest b)
	{
		if (!string.Equals(a.Content, b.Content, StringComparison.Ordinal))
		{
			return true;
		}
		if (!string.Equals(a.Subtext ?? string.Empty, b.Subtext ?? string.Empty, StringComparison.Ordinal))
		{
			return true;
		}
		if (a.AnchorPosition.HasValue != b.AnchorPosition.HasValue)
		{
			return true;
		}
		if (a.AnchorPosition.HasValue && b.AnchorPosition.HasValue && a.AnchorPosition.Value != b.AnchorPosition.Value)
		{
			return true;
		}
		if (a.FixedSize.HasValue != b.FixedSize.HasValue)
		{
			return true;
		}
		if (a.FixedSize.HasValue && b.FixedSize.HasValue && a.FixedSize.Value != b.FixedSize.Value)
		{
			return true;
		}
		if (Math.Abs(a.MaxWidth - b.MaxWidth) > float.Epsilon)
		{
			return true;
		}
		if (Math.Abs(a.FadeSeconds - b.FadeSeconds) > float.Epsilon)
		{
			return true;
		}
		return false;
	}

	private static void DrawTooltip(in TooltipRequest req, float alpha)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		float globalScale = ImGuiHelpers.GlobalScale;
		Vector2 tooltipPadding = UIShared.TooltipPadding;
		float tooltipRounding = UIShared.TooltipRounding;
		float tooltipBorderThickness = UIShared.TooltipBorderThickness;
		IFontHandle normalFont = UIShared.NormalFont;
		IFontHandle subFont = UIShared.SubFont;
		ImGuiIOPtr iO = ImGui.GetIO();
		Vector2 displaySize = ((ImGuiIOPtr)(ref iO)).DisplaySize;
		float num = MathF.Min(req.MaxWidth * globalScale, displaySize.X * 0.85f);
		float num2 = MathF.Max(1f, num - tooltipPadding.X * 2f);
		Vector2 vector = Vector2.Zero;
		Vector2 vector2 = Vector2.Zero;
		Vector2 vector3;
		Vector2 vector4;
		using (normalFont.Push())
		{
			vector3 = ImGui.CalcTextSize(ImU8String.op_Implicit(req.Content), false, 100000f);
			vector4 = ImGui.CalcTextSize(ImU8String.op_Implicit(req.Content), false, num2);
		}
		if (!string.IsNullOrEmpty(req.Subtext))
		{
			using (subFont.Push())
			{
				vector = ImGui.CalcTextSize(ImU8String.op_Implicit(req.Subtext), false, 100000f);
				vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(req.Subtext), false, num2);
			}
		}
		bool flag = vector3.X > num2;
		bool flag2 = vector.X > num2;
		float x = (flag ? vector4.X : vector3.X);
		float y = (string.IsNullOrEmpty(req.Subtext) ? 0f : (flag2 ? vector2.X : vector.X));
		float x2 = MathF.Max(x, y) + tooltipPadding.X * 2f;
		if (req.FixedSize.HasValue)
		{
			x2 = req.FixedSize.Value.X;
		}
		x2 = MathF.Min(x2, num);
		float num3 = (flag ? vector4.Y : vector3.Y) + tooltipPadding.Y * 2f;
		if (!string.IsNullOrEmpty(req.Subtext))
		{
			num3 += tooltipPadding.Y * 2f + (flag2 ? vector2.Y : vector.Y);
		}
		if (req.FixedSize.HasValue)
		{
			num3 = req.FixedSize.Value.Y;
		}
		float num4 = 0f;
		num4 = ((!flag) ? 0f : MathF.Max(1f, x2 - tooltipPadding.X * 2f));
		Vector2 vector5 = req.AnchorPosition ?? (ImGui.GetMousePos() + new Vector2(12f * globalScale, 18f * globalScale));
		if (vector5.X + x2 > displaySize.X)
		{
			vector5.X = MathF.Max(4f * globalScale, displaySize.X - x2 - 4f * globalScale);
		}
		if (vector5.Y + num3 > displaySize.Y)
		{
			vector5.Y = MathF.Max(4f * globalScale, displaySize.Y - num3 - 4f * globalScale);
		}
		ImDrawListPtr foregroundDrawList = ImGui.GetForegroundDrawList();
		Vector2 vector6 = vector5;
		Vector2 vector7 = vector5 + new Vector2(x2, num3);
		Vector4 tooltipBg = UIShared.TooltipBg;
		Vector4 tooltipBorder = UIShared.TooltipBorder;
		Vector4 tooltipText = UIShared.TooltipText;
		Vector4 tooltipSubText = UIShared.TooltipSubText;
		Vector4 tooltipSeparator = UIShared.TooltipSeparator;
		tooltipBg.W *= alpha;
		tooltipBorder.W *= alpha;
		tooltipText.W *= alpha;
		tooltipSubText.W *= alpha;
		tooltipSeparator.W *= alpha;
		((ImDrawListPtr)(ref foregroundDrawList)).AddRectFilled(vector6, vector7, ImGui.GetColorU32(tooltipBg), tooltipRounding);
		((ImDrawListPtr)(ref foregroundDrawList)).AddRect(vector6, vector7, ImGui.GetColorU32(tooltipBorder), tooltipRounding, (ImDrawFlags)0, tooltipBorderThickness);
		float num5 = vector6.Y + tooltipPadding.Y;
		float x3 = vector6.X + tooltipPadding.X;
		using (normalFont.Push())
		{
			if (num4 <= 0f)
			{
				((ImDrawListPtr)(ref foregroundDrawList)).AddText(new Vector2(x3, num5), ImGui.GetColorU32(tooltipText), ImU8String.op_Implicit(req.Content));
			}
			else
			{
				((ImDrawListPtr)(ref foregroundDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(x3, num5), ImGui.GetColorU32(tooltipText), ImU8String.op_Implicit(req.Content), num4);
			}
			num5 += (flag ? vector4.Y : vector3.Y);
		}
		if (string.IsNullOrEmpty(req.Subtext))
		{
			return;
		}
		num5 += tooltipPadding.Y;
		((ImDrawListPtr)(ref foregroundDrawList)).AddLine(new Vector2(x3, num5), new Vector2(vector7.X - tooltipPadding.X, num5), ImGui.GetColorU32(tooltipSeparator), MathF.Max(1f, 1f * globalScale));
		num5 += tooltipPadding.Y;
		using (subFont.Push())
		{
			if (flag2)
			{
				((ImDrawListPtr)(ref foregroundDrawList)).AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(x3, num5), ImGui.GetColorU32(tooltipSubText), ImU8String.op_Implicit(req.Subtext), MathF.Max(1f, x2 - tooltipPadding.X * 2f));
			}
			else
			{
				((ImDrawListPtr)(ref foregroundDrawList)).AddText(new Vector2(x3, num5), ImGui.GetColorU32(tooltipSubText), ImU8String.op_Implicit(req.Subtext));
			}
		}
	}
}

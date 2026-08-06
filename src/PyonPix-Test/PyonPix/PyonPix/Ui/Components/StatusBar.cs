using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Utility;

namespace PyonPix.Ui.Components;

public class StatusBar
{
	private string Text = string.Empty;

	private int DurationMs;

	private StatusType StatusType;

	private string? TooltipText;

	private string? TooltipSubText;

	private Action? ExpirationAction;

	private DateTime? VisibleTimestamp;

	private const float VerticalPadding = 4f;

	private const float HorizontalPadding = 6f;

	private const float FontSize = 13f;

	private const float BorderThickness = 1f;

	public bool IsOverlay;

	[CompilerGenerated]
	private bool _003CIsVisible_003Ek__BackingField;

	public float Height => 21f * ImGuiHelpers.GlobalScale;

	public bool IsVisible
	{
		get
		{
			if (!_003CIsVisible_003Ek__BackingField)
			{
				return false;
			}
			if (DurationMs > 0 && VisibleTimestamp.HasValue && (DateTime.UtcNow - VisibleTimestamp.Value).TotalMilliseconds >= (double)DurationMs)
			{
				_003CIsVisible_003Ek__BackingField = false;
				ExpirationAction?.Invoke();
				return false;
			}
			return true;
		}
		[CompilerGenerated]
		private set
		{
			_003CIsVisible_003Ek__BackingField = value;
		}
	}

	public void Show(string text, int durationMs = 0, bool overlay = false, StatusType statusType = StatusType.Info, string? tooltipText = null, string? tooltipSubtext = null, Action? expirationAction = null)
	{
		Text = text;
		DurationMs = durationMs;
		IsOverlay = overlay;
		VisibleTimestamp = DateTime.UtcNow;
		IsVisible = true;
		StatusType = statusType;
		TooltipText = tooltipText;
		TooltipSubText = tooltipSubtext;
		ExpirationAction = expirationAction;
	}

	public void Hide()
	{
		IsVisible = false;
	}

	public void Draw(Vector2 boundsMin, Vector2 boundsMax)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsVisible)
		{
			return;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		float height = Height;
		Vector2 vector = new Vector2(boundsMin.X, boundsMax.Y - height);
		Vector2 vector2 = boundsMax;
		ImDrawListPtr value = (IsOverlay ? ImGui.GetForegroundDrawList() : ImGui.GetWindowDrawList());
		((ImDrawListPtr)(ref value)).AddRectFilled(vector, vector2, ImGui.GetColorU32(UIShared.TooltipBg), UIShared.WindowRounding, (ImDrawFlags)192);
		float num = 1f * globalScale;
		((ImDrawListPtr)(ref value)).AddLine(new Vector2(vector.X, vector.Y + num * 0.5f), new Vector2(vector2.X, vector.Y + num * 0.5f), ImGui.GetColorU32(UIShared.TooltipBorder), num);
		float num2 = 6f * globalScale;
		float num3 = 4f * globalScale;
		Vector2 vector3 = new Vector2(vector.X + num2, vector.Y + num3);
		Vector2 value2 = new Vector2(vector2.X - num2, vector2.Y - num3);
		float value3 = value2.X - vector3.X;
		using (UIShared.NormalFont.Push())
		{
			Vector4 value4 = StatusType switch
			{
				StatusType.Warn => UIShared.Warn, 
				StatusType.Error => UIShared.Error, 
				_ => UIShared.TooltipText, 
			};
			if (IsOverlay)
			{
				ImU8String text = ImU8String.op_Implicit(Text);
				float? fontSize = 13f;
				Vector3? colorA = value4.AsVector3();
				float? wrapWidth = value3;
				ImDrawListPtr? targetDrawList = value;
				Vector2? screenOffset = vector3;
				Vector2? clipMin = vector3;
				Vector2? clipMax = value2;
				ImGuiEx.StyledText(text, fontSize, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, wrapWidth, multiline: false, null, null, null, targetDrawList, screenOffset, clipMin, clipMax);
			}
			else
			{
				ImGui.SetCursorScreenPos(vector3);
				ImU8String text2 = ImU8String.op_Implicit(Text);
				float? fontSize2 = 13f;
				Vector3? colorA2 = value4.AsVector3();
				float? wrapWidth = value3;
				Vector2? clipMax = vector3;
				Vector2? clipMin = value2;
				ImGuiEx.StyledText(text2, fontSize2, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA2, null, null, null, null, null, null, null, wrapWidth, multiline: false, null, null, null, null, null, clipMax, clipMin);
			}
		}
		if (TooltipText != null && UiUtil.IsRectHovered(vector, vector2))
		{
			Tooltip.Show(TooltipText, TooltipSubText, vector, vector2);
		}
	}
}

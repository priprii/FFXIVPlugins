using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using GLib.Popups;
using GLib.Popups.Decorators;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Editor.Animation.Game;
using Ktisis.Editor.Animation.Types;
using Ktisis.Interop.Ipc;
using Ktisis.Localization;
using Ktisis.Structs.Actors;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.Components.Chara;

[Transient]
public class AnimationEditorTab
{
	private enum AnimType
	{
		Action,
		Emote,
		Expression,
		RawTimeline,
		All
	}

	private class AnimationFilter : IFilterProvider<GameAnimation>
	{
		private AnimType Type = AnimType.Emote;

		public bool SlotFilterActive;

		public TimelineSlot Slot;

		public bool DrawOptions()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			AnimType[] values = Enum.GetValues<AnimType>();
			List<int> excludes = GetExcludes();
			int num = 0;
			ImU8String val = default(ImU8String);
			for (int i = 0; i < values.Length; i++)
			{
				if (excludes.Contains(i))
				{
					if (Type == values[i])
					{
						Type = BestDefaultTypeForExcludes(excludes);
					}
					continue;
				}
				if (num % 3 != 0)
				{
					ImGui.SameLine();
				}
				AnimType animType = values[i];
				((ImU8String)(ref val))._002Ector(0, 1);
				((ImU8String)(ref val)).AppendFormatted<AnimType>(animType);
				if (ImGui.RadioButton(val, Type == animType))
				{
					Type = animType;
					result = true;
				}
				num++;
			}
			ImGui.Spacing();
			return result;
		}

		public bool Filter(GameAnimation item)
		{
			if ((!SlotFilterActive || Slot == item.Slot) && Type == AnimType.All)
			{
				return true;
			}
			bool flag = !SlotFilterActive || Slot == item.Slot;
			if (flag)
			{
				bool flag2 = ((item is ActionAnimation) ? (Type == AnimType.Action) : ((item is EmoteAnimation emoteAnimation) ? (Type == (AnimType)((!emoteAnimation.IsExpression) ? 1 : 2)) : (item is TimelineAnimation && Type == AnimType.RawTimeline)));
				flag = flag2;
			}
			return flag;
		}

		private List<int> GetExcludes()
		{
			if (!SlotFilterActive)
			{
				return new List<int>();
			}
			if (Slot == TimelineSlot.FullBody || Slot == TimelineSlot.UpperBody)
			{
				return new List<int> { 2 };
			}
			if (Slot == TimelineSlot.Expression)
			{
				return new List<int> { 0, 1 };
			}
			if (Slot == TimelineSlot.Additive)
			{
				return new List<int> { 0, 2 };
			}
			if (Slot == TimelineSlot.Lips)
			{
				return new List<int> { 0, 1, 2, 4 };
			}
			return new List<int>();
		}

		private AnimType BestDefaultTypeForExcludes(List<int> excludes)
		{
			if (!excludes.Contains(1))
			{
				return AnimType.Emote;
			}
			if (!excludes.Contains(0))
			{
				return AnimType.Action;
			}
			if (!excludes.Contains(2))
			{
				return AnimType.Expression;
			}
			return AnimType.RawTimeline;
		}
	}

	private static readonly PoseModeEnum[] Modes = new PoseModeEnum[4]
	{
		PoseModeEnum.Idle,
		PoseModeEnum.SitGround,
		PoseModeEnum.SitChair,
		PoseModeEnum.Sleeping
	};

	private static readonly List<TimelineSlot> ScrubSlots;

	private readonly ConfigManager _cfg;

	private readonly ITextureProvider _tex;

	private readonly LocaleManager _locale;

	private readonly IpcManager _ipc;

	private readonly GameAnimationData _animData;

	private bool _openAnimList;

	private readonly AnimationFilter _animFilter = new AnimationFilter();

	private readonly PopupList<GameAnimation> _animList;

	private bool _isSetup;

	private uint TimelineId;

	private GameAnimation? PoseExpression;

	public IAnimationEditor Editor { private get; set; }

	private Configuration Config => _cfg.File;

	private ref bool PlayEmoteStart => ref Config.Editor.PlayEmoteStart;

	private ref bool ForceLoop => ref Config.Editor.ForceLoop;

	public AnimationEditorTab(ConfigManager cfg, IDataManager data, LocaleManager locale, ITextureProvider tex, IpcManager ipc)
	{
		_cfg = cfg;
		_locale = locale;
		_tex = tex;
		_ipc = ipc;
		_animData = new GameAnimationData(data);
		_animList = new PopupList<GameAnimation>("##AnimEmoteList", DrawAnimationSelect).WithSearch(AnimSearchPredicate).WithFilter(_animFilter);
	}

	public void Setup()
	{
		if (_isSetup)
		{
			return;
		}
		_isSetup = true;
		_animData.Build().ContinueWith(delegate(Task task)
		{
			if (task.Exception != null)
			{
				Ktisis.Log.Error($"Failed to fetch animations:\n{task.Exception}");
			}
		});
	}

	public void ClearPoseExpression()
	{
		PoseExpression = null;
	}

	public void Draw()
	{
		DrawAnimation();
	}

	private static float CalcItemHeight()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float textLineHeight = ImGui.GetTextLineHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		return (textLineHeight + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f;
	}

	private void DrawAnimation()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Spacing();
		if (Editor.Posing)
		{
			DrawPoseExpression();
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
		}
		else
		{
			PoseExpression = null;
		}
		Vector2 vector = ImGui.GetContentRegionAvail();
		if (Config.Editor.UseToolbar)
		{
			vector = new Vector2(500f, 420f) * ImGuiHelpers.GlobalScale;
		}
		ImU8String val = ImU8String.op_Implicit("##animFrame");
		Vector2 vector2 = vector;
		vector2.X = vector.X * 0.35f;
		ChildDisposable val2 = ImRaii.Child(val, vector2);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.animationSelect")));
			DrawEmote();
			ImGui.Spacing();
			ImGui.Text(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.idleSelect")));
			DrawPose();
		}
		finally
		{
			((ChildDisposable)(ref val2)).Dispose();
		}
		ImGui.SameLine(0f, 0f);
		ImU8String val3 = ImU8String.op_Implicit("##tlFrame");
		vector2 = vector;
		vector2.X = vector.X * 0.65f;
		ChildDisposable val4 = ImRaii.Child(val3, vector2);
		try
		{
			DrawTimelines();
		}
		finally
		{
			((ChildDisposable)(ref val4)).Dispose();
		}
		if (_openAnimList)
		{
			_openAnimList = false;
			_animList.Open();
		}
		if (_animList.Draw(_animData.GetAll(), _animData.Count, out GameAnimation selected, CalcItemHeight()))
		{
			if (!_animFilter.SlotFilterActive)
			{
				TimelineId = selected.TimelineId;
			}
			if (selected != null && selected.Slot == TimelineSlot.Expression)
			{
				PoseExpression = selected;
			}
			Editor.PlayAnimation(selected, PlayEmoteStart);
		}
	}

	private void DrawEmote()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		if (Buttons.IconButton((FontAwesomeIcon)61442))
		{
			OpenAnimationPopup();
		}
		ImGui.SameLine(0f, x);
		int timelineId = (int)TimelineId;
		if (ImGui.InputInt(ImU8String.op_Implicit("##emote"), ref timelineId, 0, 0, default(ImU8String), (ImGuiInputTextFlags)0))
		{
			TimelineId = (uint)timelineId;
		}
		if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.play")), default(Vector2)))
		{
			PlayTimeline((uint)timelineId);
		}
		ImGui.SameLine(0f, x);
		if (ImGui.Button(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.reset")), default(Vector2)))
		{
			ResetTimeline();
		}
		ImGui.SameLine(0f, x);
		ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.loop")), ref ForceLoop);
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.playStart")), ref PlayEmoteStart);
	}

	private void DrawPose()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!Editor.TryGetModeAndPose(out var mode, out var pose))
		{
			return;
		}
		float x = ImGui.GetContentRegionAvail().X;
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetNextItemWidth(x - ((ImGuiStylePtr)(ref style)).ItemSpacing.X * 2f);
		if (ImGui.BeginCombo(ImU8String.op_Implicit("##Mode"), ImU8String.op_Implicit(mode.ToString()), (ImGuiComboFlags)0))
		{
			PoseModeEnum[] modes = Modes;
			for (int i = 0; i < modes.Length; i++)
			{
				PoseModeEnum poseModeEnum = modes[i];
				if (ImGui.Selectable(ImU8String.op_Implicit(poseModeEnum.ToString()), poseModeEnum == mode, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					Editor.SetPose(poseModeEnum, 0);
				}
			}
			ImGui.EndCombo();
		}
		float x2 = ImGui.GetContentRegionAvail().X;
		style = ImGui.GetStyle();
		ImGui.SetNextItemWidth(x2 - ((ImGuiStylePtr)(ref style)).ItemSpacing.X * 2f);
		if (ImGui.InputInt(ImU8String.op_Implicit("##Pose"), ref pose, 1, 0, default(ImU8String), (ImGuiInputTextFlags)0))
		{
			int poseCount = Editor.GetPoseCount(mode);
			pose = ((pose < 0) ? (poseCount - 1) : (pose % poseCount));
			Editor.SetPose(mode, (byte)pose);
		}
		ImGui.Spacing();
		bool isWeaponDrawn = Editor.IsWeaponDrawn;
		if (ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.weapon")), ref isWeaponDrawn))
		{
			Editor.ToggleWeapon();
		}
	}

	private void DrawPoseExpression()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 0);
		((ImU8String)(ref val)).AppendLiteral("pose_exp");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
			CalcItemHeight();
			ImGui.Text(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.poseExpression.title")));
			ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)0, 4278245631u, true);
			try
			{
				ImGui.Text(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.poseExpression.warning")));
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			if (_ipc.IsBrioActive)
			{
				val3 = ImRaii.PushColor((ImGuiCol)0, 4283453124u, true);
				try
				{
					ImGui.TextWrapped(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.poseExpression.brioWarning")));
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			ImGui.TextWrapped(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.poseExpression.header")));
			if (Buttons.IconButton((FontAwesomeIcon)61442))
			{
				OpenAnimationPopup(TimelineSlot.Expression);
			}
			ImGui.SameLine(0f, x);
			string text = Ktisis.Locale.Translate("chara_edit.animation.poseExpression.apply");
			float x2 = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
			style = ImGui.GetStyle();
			float x3 = x2 + ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
			DisabledDisposable val4 = ImRaii.Disabled(PoseExpression == null);
			try
			{
				if (ImGui.Button(ImU8String.op_Implicit(text), new Vector2(x3, Buttons.CalcSize())))
				{
					Editor.DoPoseExpression(PoseExpression.TimelineId);
				}
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			ImGui.SameLine(0f, x);
			Vector2 vector = new Vector2(Buttons.CalcSize(), Buttons.CalcSize());
			GameAnimation poseExpression = PoseExpression;
			if (poseExpression != null)
			{
				if (poseExpression.Icon != 0)
				{
					ITextureProvider tex = _tex;
					GameIconLookup val5 = GameIconLookup.op_Implicit(poseExpression.Icon);
					ISharedImmediateTexture val6 = default(ISharedImmediateTexture);
					if (tex.TryGetFromGameIcon(ref val5, ref val6))
					{
						ImGui.Image(val6.GetWrapOrEmpty().Handle, vector);
						goto IL_021a;
					}
				}
				ImGui.Dummy(vector);
				goto IL_021a;
			}
			ImGui.Dummy(vector);
			ImGui.SameLine(0f, x);
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.animation.poseExpression.null")));
			return;
			IL_021a:
			ImGui.SameLine(0f, x);
			ImGui.Text(ImU8String.op_Implicit(poseExpression.Name));
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private unsafe void DrawTimelines()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		bool speedControlEnabled = Editor.SpeedControlEnabled;
		if (ImGui.Checkbox(ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.enableSpeed")), ref speedControlEnabled))
		{
			if (!speedControlEnabled)
			{
				Editor.ResetTimelineSpeeds();
			}
			Editor.SpeedControlEnabled = speedControlEnabled;
		}
		ImGui.Spacing();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		AnimationTimeline timeline = Editor.GetTimeline();
		TimelineSlot[] values = Enum.GetValues<TimelineSlot>();
		ImU8String val = default(ImU8String);
		foreach (TimelineSlot timelineSlot in values)
		{
			((ImU8String)(ref val))._002Ector(9, 1);
			((ImU8String)(ref val)).AppendLiteral("timeline_");
			((ImU8String)(ref val)).AppendFormatted<TimelineSlot>(timelineSlot);
			IdDisposable val2 = ImRaii.PushId(val, true);
			try
			{
				int num = (int)timelineSlot;
				if (Buttons.IconButton((FontAwesomeIcon)61761, new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight())))
				{
					OpenAnimationPopup(timelineSlot);
				}
				ImGui.SameLine(0f, x);
				ushort num2 = timeline.TimelineIds[num];
				ActionTimeline? timelineById = _animData.GetTimelineById(num2);
				ImGui.SetNextItemWidth(40f);
				int num3 = num2;
				ImU8String val3 = new ImU8String(4, 1);
				((ImU8String)(ref val3)).AppendLiteral("##id");
				((ImU8String)(ref val3)).AppendFormatted<int>(num);
				ImU8String val4 = val3;
				ImU8String val5 = default(ImU8String);
				ImGui.InputInt(val4, ref num3, 0, 0, val5, (ImGuiInputTextFlags)16384);
				ImGui.SameLine(0f, x);
				float nextItemWidth = ImGui.CalcItemWidth() - ImGui.GetFrameHeight() - 40f;
				ImGui.SetNextItemWidth(nextItemWidth);
				object obj;
				if (!timelineById.HasValue)
				{
					obj = null;
				}
				else
				{
					ActionTimeline valueOrDefault = timelineById.GetValueOrDefault();
					ReadOnlySeString key = ((ActionTimeline)(ref valueOrDefault)).Key;
					obj = ((ReadOnlySeString)(ref key)).ExtractText();
				}
				if (obj == null)
				{
					obj = string.Empty;
				}
				string text = (string)obj;
				DisabledDisposable val6 = ImRaii.Disabled(StringExtensions.IsNullOrEmpty(text));
				try
				{
					val5 = new ImU8String(3, 1);
					((ImU8String)(ref val5)).AppendLiteral("##s");
					((ImU8String)(ref val5)).AppendFormatted<int>(num);
					ImGui.InputText(val5, ref text, 256, (ImGuiInputTextFlags)16384, (ImGuiInputTextCallbackDelegate)null);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
				ImGui.SameLine(0f, 0f);
				ImU8String val7 = ImU8String.op_Implicit("{0}");
				ImU8String val8 = new ImU8String(0, 1);
				((ImU8String)(ref val8)).AppendFormatted<TimelineSlot>(timelineSlot);
				ImGui.LabelText(val7, val8);
				float speed = timeline.TimelineSpeeds[num];
				DisabledDisposable val9 = ImRaii.Disabled(!speedControlEnabled);
				try
				{
					ImGui.SetNextItemWidth(ImGui.GetFrameHeight() + x + 40f);
					ImU8String val10 = new ImU8String(9, 1);
					((ImU8String)(ref val10)).AppendLiteral("##speed_l");
					((ImU8String)(ref val10)).AppendFormatted<int>(num);
					ImU8String val11 = val10;
					ImU8String val12 = default(ImU8String);
					bool num4 = ImGui.InputFloat(val11, ref speed, 0f, 0f, val12, (ImGuiInputTextFlags)0);
					ImGui.SameLine(0f, x);
					ImGui.SetNextItemWidth(nextItemWidth);
					val12 = new ImU8String(9, 1);
					((ImU8String)(ref val12)).AppendLiteral("##speed_r");
					((ImU8String)(ref val12)).AppendFormatted<int>(num);
					if (num4 | ImGui.SliderFloat(val12, ref speed, 0f, 2f, ImU8String.op_Implicit(""), (ImGuiSliderFlags)0))
					{
						Editor.SetTimelineSpeed((uint)num, speed);
					}
				}
				finally
				{
					((IDisposable)val9)?.Dispose();
				}
				ImGui.SameLine(0f, 0f);
				DisabledDisposable val13 = ImRaii.Disabled(!speedControlEnabled);
				try
				{
					ImGui.LabelText(ImU8String.op_Implicit("{0}"), ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.speedSlider")));
				}
				finally
				{
					((IDisposable)val13)?.Dispose();
				}
				ImU8String val18;
				if (ScrubSlots.Contains(timelineSlot) && !StringExtensions.IsNullOrEmpty(text))
				{
					hkaDefaultAnimationControl* hkaControl = Editor.GetHkaControl(num);
					float valueOrDefault2 = Editor.GetHkaDuration(hkaControl).GetValueOrDefault();
					float valueOrDefault3 = Editor.GetHkaLocalTime(hkaControl).GetValueOrDefault();
					ImGui.SetNextItemWidth(ImGui.GetFrameHeight() + x + 40f);
					ImU8String val14 = new ImU8String(9, 1);
					((ImU8String)(ref val14)).AppendLiteral("##scrub_l");
					((ImU8String)(ref val14)).AppendFormatted<int>(num);
					ImU8String val15 = val14;
					ImU8String val16 = default(ImU8String);
					bool num5 = ImGui.InputFloat(val15, ref valueOrDefault3, 0f, 0f, val16, (ImGuiInputTextFlags)32);
					ImGui.SameLine(0f, x);
					ImGui.SetNextItemWidth(nextItemWidth);
					val16 = new ImU8String(9, 1);
					((ImU8String)(ref val16)).AppendLiteral("##scrub_r");
					((ImU8String)(ref val16)).AppendFormatted<int>(num);
					ImU8String val17 = val16;
					val18 = default(ImU8String);
					if (num5 | ImGui.SliderFloat(val17, ref valueOrDefault3, 0f, valueOrDefault2, val18, (ImGuiSliderFlags)128))
					{
						Editor.SetHkaLocalTime(hkaControl, Math.Clamp(valueOrDefault3, 0f, valueOrDefault2));
					}
				}
				else if (ScrubSlots.Contains(timelineSlot))
				{
					DisabledDisposable val19 = ImRaii.Disabled();
					try
					{
						float num6 = 0f;
						ImGui.SetNextItemWidth(ImGui.GetFrameHeight() + x + 40f);
						val18 = new ImU8String(9, 1);
						((ImU8String)(ref val18)).AppendLiteral("##scrub_l");
						((ImU8String)(ref val18)).AppendFormatted<int>(num);
						ImU8String val20 = val18;
						ImU8String val21 = default(ImU8String);
						ImGui.InputFloat(val20, ref num6, 0f, 0f, val21, (ImGuiInputTextFlags)0);
						ImGui.SameLine(0f, x);
						ImGui.SetNextItemWidth(nextItemWidth);
						val21 = new ImU8String(9, 1);
						((ImU8String)(ref val21)).AppendLiteral("##scrub_r");
						((ImU8String)(ref val21)).AppendFormatted<int>(num);
						ImGui.SliderFloat(val21, ref num6, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
					}
					finally
					{
						((IDisposable)val19)?.Dispose();
					}
				}
				ImGui.SameLine(0f, 0f);
				DisabledDisposable val22 = ImRaii.Disabled(ScrubSlots.Contains(timelineSlot) && StringExtensions.IsNullOrEmpty(text));
				try
				{
					ImGui.LabelText(ImU8String.op_Implicit("{0}"), ImU8String.op_Implicit(_locale.Translate("chara_edit.animation.controls.scrub")));
				}
				finally
				{
					((IDisposable)val22)?.Dispose();
				}
				ImGui.Spacing();
				if (timelineSlot != TimelineSlot.Lips)
				{
					ImGui.Separator();
					ImGui.Spacing();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	private bool DrawAnimationSelect(GameAnimation anim, bool isFocus)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		float num = CalcItemHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		float cursorPosX = ImGui.GetCursorPosX();
		bool result = ImGui.Button(ImU8String.op_Implicit(string.Empty), new Vector2(ImGui.GetContentRegionAvail().X, num));
		ImGui.SameLine(cursorPosX, num + x);
		ImGui.Text(ImU8String.op_Implicit(anim.Name));
		ImGui.SameLine(cursorPosX, num + x);
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetTextLineHeight());
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)0).SetAlpha(175), true);
		try
		{
			ImU8String val2 = new ImU8String(1, 2);
			((ImU8String)(ref val2)).AppendFormatted<TimelineSlot>(anim.Slot);
			((ImU8String)(ref val2)).AppendLiteral(" ");
			((ImU8String)(ref val2)).AppendFormatted<AnimType>(TypeForAnim(anim));
			ImGui.Text(val2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(cursorPosX);
		Vector2 vector = new Vector2(num, num);
		if (anim.Icon != 0)
		{
			ITextureProvider tex = _tex;
			GameIconLookup val3 = GameIconLookup.op_Implicit(anim.Icon);
			ISharedImmediateTexture val4 = default(ISharedImmediateTexture);
			if (tex.TryGetFromGameIcon(ref val3, ref val4))
			{
				ImGui.Image(val4.GetWrapOrEmpty().Handle, vector);
				goto IL_0120;
			}
		}
		ImGui.Dummy(vector);
		goto IL_0120;
		IL_0120:
		return result;
	}

	private void OpenAnimationPopup(TimelineSlot? slot = null)
	{
		bool hasValue = slot.HasValue;
		_animFilter.SlotFilterActive = hasValue;
		if (hasValue)
		{
			_animFilter.Slot = slot.Value;
		}
		_openAnimList = true;
	}

	private static bool AnimSearchPredicate(GameAnimation anim, string query)
	{
		return anim.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase);
	}

	private static AnimType TypeForAnim(GameAnimation anim)
	{
		if (!(anim is ActionAnimation))
		{
			if (!(anim is EmoteAnimation emoteAnimation))
			{
				if (anim is TimelineAnimation)
				{
					return AnimType.RawTimeline;
				}
				return AnimType.All;
			}
			return (!emoteAnimation.IsExpression) ? AnimType.Emote : AnimType.Expression;
		}
		return AnimType.Action;
	}

	private void PlayTimeline(uint id)
	{
		Editor.PlayTimeline(id);
		if (ForceLoop)
		{
			Editor.SetForceTimeline((ushort)id);
		}
	}

	private void ResetTimeline()
	{
		Editor.PlayTimeline(3u);
		Editor.SetForceTimeline(0);
	}

	static AnimationEditorTab()
	{
		int num = 2;
		List<TimelineSlot> list = new List<TimelineSlot>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<TimelineSlot> span = CollectionsMarshal.AsSpan(list);
		span[0] = TimelineSlot.FullBody;
		span[1] = TimelineSlot.UpperBody;
		ScrubSlots = list;
	}
}

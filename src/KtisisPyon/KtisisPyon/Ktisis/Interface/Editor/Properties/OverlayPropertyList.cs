using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GLib.Popups;
using GLib.Widgets;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Interface.KTK;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.Editor.Properties;

public class OverlayPropertyList : ObjectPropertyList
{
	private readonly IDataManager _data;

	private readonly ITextureProvider _texture;

	private readonly IEditorContext _ctx;

	private readonly LocaleManager _locale;

	private readonly List<StatusRow> _statuses;

	private readonly PopupList<StatusRow> _statusPopup;

	public OverlayPropertyList(IDataManager data, ITextureProvider texture, IEditorContext ctx, LocaleManager locale)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		_data = data;
		_texture = texture;
		_ctx = ctx;
		_locale = locale;
		_statuses = new List<StatusRow>();
		Enumerator<Status> enumerator = _data.GetExcelSheet<Status>((ClientLanguage?)null, (string)null).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Status status = enumerator.Current;
				ReadOnlySeString name = ((Status)(ref status)).Name;
				if (!((ReadOnlySeString)(ref name)).IsEmpty && ((Status)(ref status)).Icon != 0 && _statuses.All((StatusRow statusRow) => statusRow.Icon != ((Status)(ref status)).Icon))
				{
					try
					{
						List<StatusRow> statuses = _statuses;
						StatusRow obj = new StatusRow
						{
							Icon = ((Status)(ref status)).Icon
						};
						name = ((Status)(ref status)).Name;
						obj.Name = ((ReadOnlySeString)(ref name)).ExtractText();
						ITextureProvider texture2 = _texture;
						GameIconLookup val = GameIconLookup.op_Implicit(((Status)(ref status)).Icon);
						obj.Path = texture2.GetIconPath(ref val);
						statuses.Add(obj);
					}
					catch (FileNotFoundException ex)
					{
						Ktisis.Log.Verbose(ex.ToString());
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		_statusPopup = new PopupList<StatusRow>("##StatusPopup", DrawStatusRow).WithSearch(StatusSearchPredicate);
	}

	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity)
	{
		OverlayEntity overlay = entity as OverlayEntity;
		if (overlay != null)
		{
			builder.AddHeader(Ktisis.Locale.Translate("object_edit.overlay.header"), delegate
			{
				DrawOverlayTab(overlay);
			});
		}
	}

	private void DrawOverlayTab(OverlayEntity overlay)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.pos")));
		ImGui.Spacing();
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, ImGui.GetColorU32((ImGuiCol)23), overlay.Draggable);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)62041, Ktisis.Locale.Translate("object_edit.overlay.pos_drag")))
			{
				overlay.Draggable = !overlay.Draggable;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGui.SameLine(0f, x);
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)58558, Ktisis.Locale.Translate("object_edit.overlay.pos_snap")))
		{
			overlay.Position = GetCenter(overlay);
		}
		ImGui.SameLine(0f, x);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		Vector2 position = overlay.Position;
		if (ImGui.DragFloat2(ImU8String.op_Implicit("##OverlayPosition"), ref position, 1f, 0f, 0f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			overlay.Position = position;
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.scale")));
		ImGui.Spacing();
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61608, Ktisis.Locale.Translate("object_edit.overlay.scale_reset")))
		{
			overlay.Scale = 1f;
		}
		ImGui.SameLine(0f, x);
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		float scale = overlay.Scale;
		if (ImGui.DragFloat(ImU8String.op_Implicit("##OverlayScale"), ref scale, 0.01f, 0f, 5f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			overlay.Scale = scale;
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.alpha")));
		ImGui.Spacing();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		float alpha = overlay.Alpha / 255f;
		if (ImGui.SliderFloat(ImU8String.op_Implicit("##OverlayAlpha"), ref alpha, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0))
		{
			overlay.Alpha = alpha;
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (!(overlay is TalkOverlay talk))
		{
			if (!(overlay is BalloonOverlay balloon))
			{
				if (overlay is StatusOverlay status)
				{
					DrawStatus(status);
				}
			}
			else
			{
				DrawBalloon(balloon);
			}
		}
		else
		{
			DrawTalk(talk);
		}
	}

	private void DrawTalk(TalkOverlay talk)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		string speaker = talk.Speaker;
		if (ImGui.InputText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.talk.speaker")), ref speaker, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			talk.Speaker = speaker;
		}
		ImGui.Spacing();
		string dialog = talk.Dialog;
		if (ImGui.InputTextMultiline(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.talk.content")), ref dialog, 1000, default(Vector2), (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			talk.Dialog = dialog;
		}
		ImGui.Spacing();
		uint fontSize = talk.FontSize;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.talk.fontsize")), ImU8String.op_Implicit(fontSize.ToString()), (ImGuiComboFlags)0))
		{
			uint[] fontSizes = talk.FontSizes;
			for (int i = 0; i < fontSizes.Length; i++)
			{
				uint num = fontSizes[i];
				if (ImGui.Selectable(ImU8String.op_Implicit(num.ToString()), fontSize == num, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					talk.FontSize = num;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		TalkBackground background = talk.Background;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.talk.bg")), ImU8String.op_Implicit(_locale.Translate($"background.{background}")), (ImGuiComboFlags)0))
		{
			TalkBackground[] values = Enum.GetValues<TalkBackground>();
			foreach (TalkBackground talkBackground in values)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"background.{talkBackground}")), background == talkBackground, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					talk.Background = talkBackground;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		TalkCursor cursor = talk.Cursor;
		if (!ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.talk.cursor")), ImU8String.op_Implicit(_locale.Translate($"cursor.{cursor}")), (ImGuiComboFlags)0))
		{
			return;
		}
		TalkCursor[] values2 = Enum.GetValues<TalkCursor>();
		foreach (TalkCursor talkCursor in values2)
		{
			if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"cursor.{talkCursor}")), cursor == talkCursor, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				talk.Cursor = talkCursor;
			}
		}
		ImGui.EndCombo();
	}

	private void DrawBalloon(BalloonOverlay balloon)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		string dialog = balloon.Dialog;
		if (ImGui.InputText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.content")), ref dialog, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			balloon.Dialog = dialog;
		}
		ImGui.Spacing();
		uint fontSize = balloon.FontSize;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.fontsize")), ImU8String.op_Implicit(fontSize.ToString()), (ImGuiComboFlags)0))
		{
			uint[] fontSizes = balloon.FontSizes;
			for (int i = 0; i < fontSizes.Length; i++)
			{
				uint num = fontSizes[i];
				if (ImGui.Selectable(ImU8String.op_Implicit(num.ToString()), fontSize == num, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					balloon.FontSize = num;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		BalloonBackground background = balloon.Background;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.bg")), ImU8String.op_Implicit(_locale.Translate($"background.{background}")), (ImGuiComboFlags)0))
		{
			BalloonBackground[] values = Enum.GetValues<BalloonBackground>();
			foreach (BalloonBackground balloonBackground in values)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"background.{balloonBackground}")), background == balloonBackground, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					balloon.Background = balloonBackground;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		BalloonColor color = balloon.Color;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.gradient")), ImU8String.op_Implicit(_locale.Translate($"gradient.{color}")), (ImGuiComboFlags)0))
		{
			BalloonColor[] values2 = Enum.GetValues<BalloonColor>();
			foreach (BalloonColor balloonColor in values2)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"gradient.{balloonColor}")), color == balloonColor, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					balloon.Color = balloonColor;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		bool arrow = balloon.Arrow;
		if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.arrow_show")), ref arrow))
		{
			balloon.Arrow = arrow;
		}
		ImGui.Spacing();
		DisabledDisposable val = ImRaii.Disabled(!balloon.Arrow);
		try
		{
			float arrowX = balloon.ArrowX;
			if (ImGui.SliderFloat(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.balloon.arrow_pos")), ref arrowX, 32f, 130f, default(ImU8String), (ImGuiSliderFlags)0))
			{
				balloon.ArrowX = arrowX;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawStatus(StatusOverlay status)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		string statusText = status.StatusText;
		if (ImGui.InputText(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.status.content")), ref statusText, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			status.StatusText = statusText;
		}
		ImGui.Spacing();
		StatusType statusType = status.StatusType;
		if (ImGui.BeginCombo(ImU8String.op_Implicit(Ktisis.Locale.Translate("object_edit.overlay.status.type")), ImU8String.op_Implicit(_locale.Translate($"status.{statusType}")), (ImGuiComboFlags)0))
		{
			StatusType[] values = Enum.GetValues<StatusType>();
			foreach (StatusType statusType2 in values)
			{
				if (ImGui.Selectable(ImU8String.op_Implicit(_locale.Translate($"status.{statusType2}")), statusType == statusType2, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					status.StatusType = statusType2;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.Spacing();
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)61502, Ktisis.Locale.Translate("object_edit.overlay.status.tex_hint")))
		{
			_statusPopup.Open();
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		StatusRow statusRow = _statuses.FirstOrDefault((StatusRow stat) => stat.Path == status.IconPath);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(1, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("object_edit.overlay.status.tex"));
		((ImU8String)(ref val)).AppendLiteral(" ");
		((ImU8String)(ref val)).AppendFormatted<string>(statusRow?.Name);
		ImGui.Text(val);
		if (statusRow != null)
		{
			style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ITextureProvider texture = _texture;
			GameIconLookup val2 = GameIconLookup.op_Implicit(statusRow.Icon);
			ImGui.Image(texture.GetFromGameIcon(ref val2).GetWrapOrEmpty().Handle, new Vector2(24f, 32f));
		}
		DrawStatusPopup(status);
	}

	private void DrawStatusPopup(StatusOverlay status)
	{
		if (_statusPopup.IsOpen && _statusPopup.Draw(_statuses, _statuses.Count, out StatusRow selected, 32f))
		{
			status.IconPath = selected.Path;
		}
	}

	private bool DrawStatusRow(StatusRow status, bool isFocus)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		float cursorPosX = ImGui.GetCursorPosX();
		bool result = ImGui.Button(ImU8String.op_Implicit(string.Empty), new Vector2(ImGui.GetContentRegionAvail().X, 32f));
		ImGui.SameLine(cursorPosX, 24f + x);
		ImGui.Text(ImU8String.op_Implicit(status.Name));
		ImGui.SameLine(cursorPosX);
		ITextureProvider texture = _texture;
		GameIconLookup val = GameIconLookup.op_Implicit(status.Icon);
		ImGui.Image(texture.GetFromGameIcon(ref val).GetWrapOrEmpty().Handle, new Vector2(24f, 32f));
		return result;
	}

	private static bool StatusSearchPredicate(StatusRow status, string query)
	{
		return status.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private static Vector2 GetCenter(OverlayEntity entity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
		Vector2 vector = ((ImGuiViewportPtr)(ref mainViewport)).Size / 2f;
		Vector2 vector2 = entity.Size * entity.Scale / 2f;
		return new Vector2(vector.X - vector2.X, vector.Y - vector2.Y);
	}
}

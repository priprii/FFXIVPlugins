using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Common.Utility;
using Ktisis.Localization;

namespace Ktisis.Legacy.Interface;

public class V2MigratorWindow
{
	private readonly LegacyMigrator _migrator;

	private readonly LocaleManager Locale;

	public V2MigratorWindow(LegacyMigrator migrator, LegacyConfig.Configuration legacyCfg, LocaleManager locale)
	{
		_migrator = migrator;
		Locale = locale;
	}

	public void DrawIntro()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.mainWindow.v2.main_Desc")));
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.mainWindow.v2.migration_desc")));
		ImGui.Spacing();
		ImGui.AlignTextToFramePadding();
		ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.mainWindow.v2.wiki")));
		ImGui.SameLine();
		if (Buttons.IconButton((FontAwesomeIcon)61582))
		{
			GuiHelpers.OpenBrowser("https://docs.ktisis.tools/migration/");
		}
	}

	public void DrawEditor()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("Editor"), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.editor.header")));
			ImGui.Separator();
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Categories.ShowNsfwBones, newDefault: false, Locale.Translate("migrator.v2.editor.nsfwTooltip"), Locale.Translate("migrator.v2.editor.nsfw"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.IncognitoPlayerNames, newDefault: false, Locale.Translate("migrator.v2.editor.incognitoTooltip"), Locale.Translate("migrator.v2.editor.incognito"), Locale.Translate("migrator.v2.editor.incognitoSub"));
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.UseToolbar, newDefault: true, string.Empty, Locale.Translate("migrator.v2.editor.toolbar"), Locale.Translate("migrator.v2.editor.toolbarSub"));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawInput()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("Input"), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.input.header")));
			ImGui.Separator();
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.input.detail")));
			ImGui.Spacing();
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Keybinds.Enabled, newDefault: true, string.Empty, Locale.Translate("migrator.v2.input.keybinds"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Keybinds.BlockTargetLeftClick, newDefault: false, string.Empty, Locale.Translate("migrator.v2.input.leftClick"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Keybinds.BlockTargetRightClick, newDefault: false, string.Empty, Locale.Translate("migrator.v2.input.rightClick"), string.Empty);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawOverlay()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("Overlay"), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.overlay.header")));
			ImGui.Separator();
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.overlay.detail")));
			ImGui.Spacing();
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.DrawLines, newDefault: true, string.Empty, Locale.Translate("migrator.v2.overlay.drawLines"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.DrawLinesGizmo, newDefault: true, string.Empty, Locale.Translate("migrator.v2.overlay.drawLinesGizmo"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.DrawDotsGizmo, newDefault: true, string.Empty, Locale.Translate("migrator.v2.overlay.drawDotsGizmo"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Gizmo.AllowAxisFlip, newDefault: true, Locale.Translate("migrator.v2.overlay.allowAxisFlipTip"), Locale.Translate("migrator.v2.overlay.allowAxisFlip"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.LineThickness, 2f, string.Empty, Locale.Translate("migrator.v2.overlay.lineThickness"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.LineOpacity, 0.95f, string.Empty, Locale.Translate("migrator.v2.overlay.lineOpacity"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.LineOpacityUsing, 0.15f, string.Empty, Locale.Translate("migrator.v2.overlay.lineOpacityUsing"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.DotRadius, 7f, string.Empty, Locale.Translate("migrator.v2.overlay.dotRadius"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.SelectOnTarget, newDefault: false, string.Empty, Locale.Translate("migrator.v2.overlay.selectTarget"), Locale.Translate("migrator.v2.overlay.selectTargetTip"));
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Overlay.DimOverlayForInactiveActors, newDefault: false, string.Empty, Locale.Translate("migrator.v2.overlay.dimInactive"), Locale.Translate("migrator.v2.overlay.dimInactiveTip"));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawAutoSave()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("Auto save"), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.autosave.header")));
			ImGui.Separator();
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.autosave.detail")));
			ImGui.Spacing();
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.AutoSave.Enabled, newDefault: true, string.Empty, Locale.Translate("migrator.v2.autosave.enabled"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.AutoSave.Interval, 60, Locale.Translate("migrator.v2.autosave.intervalTip"), Locale.Translate("migrator.v2.autosave.interval"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.AutoSave.Count, 5, string.Empty, Locale.Translate("migrator.v2.autosave.numToSave"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.AutoSave.ClearOnExit, newDefault: false, string.Empty, Locale.Translate("migrator.v2.autosave.clearOnExit"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.AutoSave.OnDisconnect, newDefault: true, string.Empty, Locale.Translate("migrator.v2.autosave.autoOnDC"), string.Empty);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void DrawCamera()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("Work Camera"), true);
		try
		{
			ImGui.Text(ImU8String.op_Implicit(Locale.Translate("migrator.v2.camera.header")));
			ImGui.Separator();
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.WorkcamSens, 0.215f, string.Empty, Locale.Translate("migrator.v2.camera.sens"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.WorkcamMoveSpeed, 0.1f, string.Empty, Locale.Translate("migrator.v2.camera.movSpeed"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.WorkcamFastMulti, 2.5f, string.Empty, Locale.Translate("migrator.v2.camera.fastMulti"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.WorkcamSlowMulti, 0.25f, string.Empty, Locale.Translate("migrator.v2.camera.slowMulti"), string.Empty);
			DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.WorkcamVertMulti, 1f, string.Empty, Locale.Translate("migrator.v2.camera.vertMulti"), string.Empty);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

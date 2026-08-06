using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GLib.Widgets;
using Ktisis.Interface.Types;
using Ktisis.Localization;

namespace Ktisis.Legacy.Interface;

public class MigratorWindow : KtisisWindow
{
	private readonly IDalamudPluginInterface _dpi;

	private readonly LegacyMigrator _migrator;

	private readonly V2MigratorWindow? _v2Window;

	private readonly Stopwatch _timer = new Stopwatch();

	private bool _elapsed;

	private int _page;

	private const int WaitTime = 15;

	private bool CanBegin
	{
		get
		{
			if (!(_timer.Elapsed.TotalSeconds >= 15.0))
			{
				return _elapsed;
			}
			return true;
		}
	}

	public MigratorWindow(IDalamudPluginInterface dpi, LegacyMigrator migrator)
		: base("migrator.title", (ImGuiWindowFlags)320, "###KtisisMigrator")
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(550f, 50f);
		((Window)this).SizeConstraints = value;
		_dpi = dpi;
		_migrator = migrator;
		if (_dpi.ConfigFile.Exists)
		{
			_v2Window = new V2MigratorWindow(_migrator, _migrator._legacyCfg, Ktisis.Locale);
		}
		((Window)this).ShowCloseButton = false;
		((Window)this).RespectCloseHotkey = false;
	}

	public override void OnOpen()
	{
		_timer.Reset();
		_timer.Start();
		_elapsed = false;
	}

	private void DrawIntroPage()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(0, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("migrator.mainWindow.main_Desc"));
		ImGui.Text(val);
		Vector2 vector = new Vector2(ImGui.GetContentRegionMax().X * 0.3f, ImGui.GetContentRegionMax().X * 0.3f * 0.33f);
		if (_migrator.v2ConfigExists)
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v2.from")), vector))
			{
				_migrator.MigrateConfig();
				_page++;
				_migrator.v2ConfigExists = true;
			}
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v2.from_desc")));
		}
		if (_migrator.v3ConfigExists)
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v3.from")), vector))
			{
				_migrator.v2ConfigExists = false;
				_page++;
			}
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v3.from_desc")));
		}
		string text = (CanBegin ? Ktisis.Locale.Translate("migrator.mainWindow.skip") : $"{Ktisis.Locale.Translate("migrator.mainWindow.skip")} ({Math.Ceiling(15m - (decimal)_timer.Elapsed.Seconds)}s)");
		DisabledDisposable val2 = ImRaii.Disabled(!CanBegin && (!ImGui.IsKeyDown((ImGuiKey)641) || !ImGui.IsKeyDown((ImGuiKey)642)));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(text), vector))
			{
				if (!_migrator.v2ConfigExists)
				{
					_migrator.V3Skip();
				}
				_migrator.Begin();
				Close();
			}
			ImGui.SameLine();
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.skip_desc")));
			val2.Pop(1);
			ComboDisposable val3 = ImRaii.Combo(ImU8String.op_Implicit(Ktisis.Locale.Translate("config.language.selector")), ImU8String.op_Implicit(Ktisis.Locale.Data?.MetaData.DisplayName));
			try
			{
				if (!val3.Success)
				{
					return;
				}
				foreach (LocaleMetaData availableLocale in Ktisis.Locale.AvailableLocales)
				{
					if (ImGui.Selectable(ImU8String.op_Implicit(availableLocale.DisplayName), availableLocale.TechnicalName == Ktisis.Locale.Data?.MetaData.TechnicalName, (ImGuiSelectableFlags)0, default(Vector2)) && availableLocale.TechnicalName != Ktisis.Locale.Data?.MetaData.TechnicalName)
					{
						_migrator._tempConfig.Locale.LocaleId = availableLocale.TechnicalName;
						Ktisis.Locale.LoadLocale(availableLocale.TechnicalName);
					}
				}
			}
			finally
			{
				((ComboDisposable)(ref val3)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawV3()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v3.main_Desc")));
		ImGui.Spacing();
		if (_dpi.IsTesting)
		{
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v3.testing")));
			ImGui.AlignTextToFramePadding();
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.mainWindow.v3.installer")));
			ImGui.SameLine();
			if (Buttons.IconButton((FontAwesomeIcon)61582))
			{
				_dpi.OpenPluginInstallerTo((PluginInstallerOpenKind)0, "Ktisis");
			}
		}
		DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.ToggleOpenWindows, newDefault: true, string.Empty, Ktisis.Locale.Translate("migrator.v3.openWindowToggle"), Ktisis.Locale.Translate("migrator.v3.openWindowToggleSub"));
		DialogHelpers.BuildDialog(ref _migrator._tempConfig.Editor.UseToolbar, newDefault: false, string.Empty, Ktisis.Locale.Translate("migrator.v3.toolbar"), Ktisis.Locale.Translate("migrator.v3.toolbarSub"));
		DialogHelpers.BuildDialog(ref _migrator._tempConfig.Keybinds.Enabled, newDefault: true, string.Empty, Ktisis.Locale.Translate("migrator.v3.keybinds"), string.Empty);
	}

	public override void Draw()
	{
		if (!_elapsed && CanBegin)
		{
			_timer.Stop();
			_elapsed = true;
		}
		switch (_page)
		{
		case 0:
			DrawIntroPage();
			break;
		case 1:
			if (_migrator.v2ConfigExists)
			{
				_v2Window?.DrawIntro();
			}
			else
			{
				DrawV3();
			}
			ImGui.Spacing();
			DrawBottomBar();
			break;
		case 2:
			_v2Window?.DrawEditor();
			ImGui.Spacing();
			DrawBottomBar();
			break;
		case 3:
			_v2Window?.DrawOverlay();
			ImGui.Spacing();
			DrawBottomBar();
			break;
		case 4:
			_v2Window?.DrawAutoSave();
			ImGui.Spacing();
			DrawBottomBar();
			break;
		case 5:
			_v2Window?.DrawCamera();
			ImGui.Spacing();
			DrawBottomBar();
			break;
		case 6:
			_v2Window?.DrawInput();
			ImGui.Spacing();
			DrawBottomBar();
			break;
		}
	}

	private void DrawBottomBar()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		_ = string.Empty;
		ImGuiStylePtr style;
		if ((_migrator.v2ConfigExists && _page < 6) || (!_migrator.v2ConfigExists && _page == 0))
		{
			ImGui.SameLine();
			float num = ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.next")), false, -1f).X;
			style = ImGui.GetStyle();
			ImGui.SetCursorPosX(num - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f - 0.1f);
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.next")), default(Vector2)))
			{
				_page++;
			}
		}
		else if ((_migrator.v2ConfigExists && _page == 6) || (!_migrator.v2ConfigExists && _page == 1))
		{
			ImGui.SameLine();
			float num2 = ImGui.GetContentRegionMax().X - ImGui.CalcTextSize(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.finish")), false, -1f).X;
			style = ImGui.GetStyle();
			ImGui.SetCursorPosX(num2 - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f - 0.1f);
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("migrator.finish")), default(Vector2)))
			{
				_migrator.Begin();
				Close();
			}
		}
	}
}

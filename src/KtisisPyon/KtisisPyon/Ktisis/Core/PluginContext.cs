using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Ktisis.Actions;
using Ktisis.Core.Attributes;
using Ktisis.Core.Types;
using Ktisis.Data.Config;
using Ktisis.Editor.Context;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface;
using Ktisis.Interop;
using Ktisis.Interop.Ipc;
using Ktisis.Legacy;
using Ktisis.Services.Plugin;

namespace Ktisis.Core;

[Singleton]
public class PluginContext : IPluginContext
{
	private readonly CommandService _cmd;

	private readonly DllResolver _dll;

	private readonly ContextManager _context;

	private readonly LegacyMigrator _legacy;

	private readonly IDalamudPluginInterface _dpi;

	private readonly IFramework _framework;

	public ActionService Actions { get; }

	public ConfigManager Config { get; }

	public GuiManager Gui { get; }

	public IpcManager Ipc { get; }

	public IEditorContext? Editor => _context.Current;

	public PluginContext(ActionService actions, ConfigManager cfg, CommandService cmd, DllResolver dll, ContextManager context, GuiManager gui, IpcManager ipc, LegacyMigrator legacy, IDalamudPluginInterface dpi, IFramework framework)
	{
		_cmd = cmd;
		_dll = dll;
		_context = context;
		_legacy = legacy;
		_dpi = dpi;
		_framework = framework;
		Actions = actions;
		Config = cfg;
		Gui = gui;
		Ipc = ipc;
	}

	public void Initialize()
	{
		if (Config.GetConfigFileExists())
		{
			Config.Load();
			if (Config.File.Version < 12)
			{
				SetupLegacy();
			}
			else
			{
				Setup();
			}
		}
		else if (_dpi.GetPluginConfig() != null)
		{
			SetupLegacy();
		}
		else
		{
			Setup();
		}
		Gui.Initialize();
		if (GameMain.IsInGPose())
		{
			_framework.RunOnFrameworkThread((Action)delegate
			{
				_context.SetupEditor();
				Ktisis.Log.Verbose("Setup onload");
			});
			_context.Current?.Interface.ToggleWorkspaceWindow();
		}
	}

	private void Setup()
	{
		if (!Config._isLoaded)
		{
			Config.Load();
		}
		Gui.AddSettings();
		_dll.Create();
		Actions.RegisterActions(this);
		_context.Initialize(this);
		_cmd.RegisterHandlers();
		Gui.Locale.Initialize(Config);
	}

	private void SetupLegacy()
	{
		_legacy.Setup();
		_cmd.RegisterLegacy();
		_legacy.OnConfirmed += Setup;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Editor.Context;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface;
using Ktisis.Interface.Windows;

namespace Ktisis.Services.Plugin;

[Singleton]
public class CommandService : IDisposable
{
	private class CommandFactory
	{
		private readonly CommandService _cmd;

		private readonly string Name;

		private readonly List<string> Alias = new List<string>();

		private readonly HandlerDelegate Handler;

		private bool ShowInHelp;

		private string HelpMessage = string.Empty;

		public CommandFactory(CommandService cmd, string name, HandlerDelegate handler)
		{
			_cmd = cmd;
			Name = name;
			Handler = handler;
		}

		public CommandFactory SetMessage(string message)
		{
			ShowInHelp = true;
			HelpMessage = message;
			return this;
		}

		public CommandFactory AddAlias(string alias)
		{
			Alias.Add(alias);
			return this;
		}

		public CommandFactory AddAliases(params string[] aliases)
		{
			Alias.AddRange(aliases);
			return this;
		}

		public void Create()
		{
			_cmd.Add(Name, BuildCommandInfo());
			Alias.ForEach(CreateAlias);
		}

		private void CreateAlias(string alias)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			_cmd.Add(alias, new CommandInfo(Handler)
			{
				ShowInHelp = false
			});
		}

		private CommandInfo BuildCommandInfo()
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected O, but got Unknown
			string text = HelpMessage;
			if (HelpMessage != string.Empty && Alias.Count > 0)
			{
				string value = new string(' ', Name.Length * 2);
				text += $"\n{value} (Aliases: {string.Join(", ", Alias)})";
			}
			return new CommandInfo(Handler)
			{
				ShowInHelp = ShowInHelp,
				HelpMessage = text
			};
		}
	}

	private readonly ICommandManager _cmd;

	private readonly IChatGui _chat;

	private readonly ContextManager _ctx;

	private readonly GuiManager _gui;

	private readonly IClientState _client;

	private readonly ContextBuilder _builder;

	private readonly HashSet<string> _register = new HashSet<string>();

	public CommandService(ICommandManager cmd, IChatGui chat, ContextManager ctx, GuiManager gui, IClientState client, ContextBuilder builder)
	{
		_cmd = cmd;
		_chat = chat;
		_ctx = ctx;
		_gui = gui;
		_client = client;
		_builder = builder;
	}

	public void RegisterHandlers()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Remove("/ktisispyon");
		BuildCommand("/ktisispyon", new HandlerDelegate(OnMainCommand)).SetMessage("Toggle the main KtisisPyon window.").Create();
	}

	public void RegisterLegacy()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		BuildCommand("/ktisispyon", new HandlerDelegate(OnMainCommandLegacy)).SetMessage("Toggle the main KtisisPyon window.").Create();
	}

	private void Add(string name, CommandInfo info)
	{
		if (_register.Add(name))
		{
			_cmd.AddHandler(name, info);
		}
	}

	private void Remove(string name)
	{
		_register.Remove(name);
		if (_cmd.Commands.ContainsKey("/ktisispyon"))
		{
			_cmd.RemoveHandler(name);
		}
	}

	private CommandFactory BuildCommand(string name, HandlerDelegate handler)
	{
		return new CommandFactory(this, name, handler);
	}

	private void OnMainCommand(string command, string arguments)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		Ktisis.Log.Info("Main command used");
		IEditorContext current = _ctx.Current;
		if (!_client.IsGPosing)
		{
			_chat.PrintError("Cannot open KtisisPyon workspace outside of GPose.", (string)null, (ushort?)null);
			return;
		}
		if (_ctx.Current == null)
		{
			_ctx.SetupEditor();
		}
		if (arguments.Contains("debug"))
		{
			Ktisis.Log.Info("Debug argument provided");
			current?.Interface.ToggleDebugWindow();
		}
		else if (arguments.Contains("dump"))
		{
			Ktisis.Log.Info("Dumping log to clipboard");
			string text = string.Empty;
			foreach (string item in from e in Ktisis.Log.Logs.ToArray()
				where !e.Split('|').StartsWith(" Verbose")
				select e)
			{
				text += item;
			}
			ImGui.SetClipboardText(ImU8String.op_Implicit(text));
			Notification val = new Notification
			{
				Content = "Debug info copied to clipboard",
				Title = "[Info] KtisisPyon",
				Type = (NotificationType)4
			};
			Ktisis.Notification.AddNotification(val);
		}
		else
		{
			current?.Plugin.Gui.Get<TrayIcon>()?.Close();
			current?.Interface.ToggleWorkspaceWindow();
		}
	}

	private void OnMainCommandLegacy(string command, string arguments)
	{
		Ktisis.Log.Info("Main command used");
		_chat.PrintError("Enter GPose to complete legacy config setup.", (string)null, (ushort?)null);
	}

	public void Dispose()
	{
		foreach (string item in _register)
		{
			_cmd.RemoveHandler(item);
		}
	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Utility;
using Discord;
using Discord.WebSocket;

namespace TriggerPyon;

public class DiscordManager
{
	private Plugin plugin;

	private DisGen DisGen = new DisGen();

	private SocketGuild? Guild;

	private const ulong UsersChannel = 1414770654607310878uL;

	public const ulong KeyChannel = 1414746345507651626uL;

	public string VerificationKey = string.Empty;

	private ulong _DiscordUserId;

	private DiscordSocketClient Client { get; init; } = new DiscordSocketClient(new DiscordSocketConfig
	{
		GatewayIntents = (GatewayIntents.Guilds | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent),
		LogLevel = LogSeverity.Warning,
		ConnectionTimeout = 40000
	});

	public bool IsConnected => Client.ConnectionState == ConnectionState.Connected;

	public bool IsConnecting => Client.ConnectionState == ConnectionState.Connecting;

	public bool IsDisconnecting => Client.ConnectionState == ConnectionState.Disconnecting;

	public bool IsDisconnected => Client.ConnectionState == ConnectionState.Disconnected;

	public bool AnyTriggerEnabled => Plugin.Config.Triggers.Any((Trigger x) => x.Type == TriggerType.Discord && x.Enabled);

	private ulong DiscordUserId
	{
		get
		{
			if (_DiscordUserId == 0L && !StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey) && !ulong.TryParse(DisGen.DecId(Plugin.Config.Discord.UserKey), out _DiscordUserId))
			{
				Plugin.Config.Discord.UserKey = string.Empty;
				Plugin.Config.Save();
			}
			return _DiscordUserId;
		}
		set
		{
			_DiscordUserId = value;
			if (_DiscordUserId == 0L)
			{
				if (!StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey))
				{
					Plugin.Config.Discord.UserKey = string.Empty;
					Plugin.Config.Save();
				}
			}
			else
			{
				Plugin.Config.Discord.UserKey = DisGen.EncId($"{_DiscordUserId}");
				Plugin.Config.Save();
			}
		}
	}

	public event Action<IActivity?, Trigger?>? OnActivity;

	public void UnlinkDiscordUser()
	{
		DiscordUserId = 0uL;
	}

	public DiscordManager(Plugin plugin)
	{
		this.plugin = plugin;
		Client.Log += Client_Log;
		Client.Ready += Client_Ready;
		Client.PresenceUpdated += Client_PresenceUpdated;
		Client.MessageReceived += Client_MessageReceived;
	}

	public void Dispose()
	{
		Client.MessageReceived -= Client_MessageReceived;
		Client.PresenceUpdated -= Client_PresenceUpdated;
		Client.Ready -= Client_Ready;
		Client.Log -= Client_Log;
	}

	private Task Client_Log(LogMessage arg)
	{
		if (string.IsNullOrWhiteSpace(arg.Message))
		{
			return Task.CompletedTask;
		}
		Plugin.Log.Info("[Discord] " + arg.Message, Array.Empty<object>());
		return Task.CompletedTask;
	}

	private Task Client_Ready()
	{
		if (Client.Guilds.Count == 0)
		{
			Disconnect();
			return Task.CompletedTask;
		}
		Guild = Client.Guilds.First();
		if (DiscordUserId != 0L && Guild?.GetUser(DiscordUserId) == null)
		{
			DiscordUserId = 0uL;
		}
		return Task.CompletedTask;
	}

	private Task Client_MessageReceived(SocketMessage message)
	{
		if (!Plugin.Config.Enabled || Guild == null || message.Author.IsBot || message.Author.IsWebhook || message.Channel.ChannelType != ChannelType.Text)
		{
			return Task.CompletedTask;
		}
		if (!StringExtensions.IsNullOrWhitespace(VerificationKey) && DiscordUserId == 0L)
		{
			if (Plugin.Config.Discord.UsePyonServer)
			{
				if (!message.Channel.Name.StartsWith('@') && message.Channel.Id == 1414746345507651626L && message.Content.Contains(VerificationKey, StringComparison.OrdinalIgnoreCase))
				{
					DiscordUserId = message.Author.Id;
					Guild?.GetTextChannel(1414770654607310878uL)?.SendMessageAsync($"<@{message.Author.Id}> ({PlayerManager.LocalPlayer?.Name}@{PlayerManager.LocalPlayer?.HomeWorld})");
					message.DeleteAsync();
					VerificationKey = string.Empty;
				}
			}
			else if (message.Channel.Name.StartsWith('@') && message.Content.Contains(VerificationKey, StringComparison.OrdinalIgnoreCase))
			{
				DiscordUserId = message.Author.Id;
				VerificationKey = string.Empty;
			}
		}
		return Task.CompletedTask;
	}

	private Task Client_PresenceUpdated(SocketUser socketUser, SocketPresence oldPresence, SocketPresence newPresence)
	{
		if (!Plugin.Config.Enabled || PlayerManager.LocalPlayer == null || DiscordUserId == 0L || socketUser.Id != DiscordUserId)
		{
			return Task.CompletedTask;
		}
		try
		{
			IActivity activity = null;
			Trigger lastDiscordTrigger = plugin.TriggerManager.CounterManager.LastDiscordTrigger;
			int num = ((lastDiscordTrigger != null) ? Plugin.Config.Triggers.IndexOf(lastDiscordTrigger) : 0);
			foreach (Trigger trigger in Plugin.Config.Triggers)
			{
				if (!trigger.Enabled || trigger.Type != TriggerType.Discord || !(trigger.Counter is DiscordCounter discordCounter))
				{
					continue;
				}
				switch (discordCounter.ActivityType)
				{
				case DiscordActivityType.Listening:
					activity = newPresence.Activities.FirstOrDefault((IActivity x) => x.GetType().IsAssignableTo(typeof(SpotifyGame)));
					break;
				case DiscordActivityType.Playing:
					activity = newPresence.Activities.FirstOrDefault((IActivity x) => x.GetType().IsAssignableTo(typeof(Discord.Game)) && !string.Equals(x.Name, "Custom Status", StringComparison.OrdinalIgnoreCase) && !x.Name.Contains("Final Fantasy XIV", StringComparison.OrdinalIgnoreCase));
					break;
				case DiscordActivityType.Custom:
					activity = newPresence.Activities.FirstOrDefault((IActivity x) => x.GetType().IsAssignableTo(typeof(CustomStatusGame)) && string.Equals(x.Name, "Custom Status", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace((x as CustomStatusGame).State));
					break;
				}
				if (activity != null)
				{
					int num2 = Plugin.Config.Triggers.IndexOf(trigger);
					if (lastDiscordTrigger == null || num2 <= num)
					{
						this.OnActivity?.Invoke(activity, trigger);
						break;
					}
				}
			}
			if (activity == null && lastDiscordTrigger != null)
			{
				this.OnActivity?.Invoke(null, null);
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Error(ex, "PresenceUpdate Exception", Array.Empty<object>());
		}
		return Task.CompletedTask;
	}

	public void ConnectIfAnyTriggerEnabled()
	{
		if (!IsConnecting && !IsConnected && Plugin.Config.Enabled && AnyTriggerEnabled)
		{
			ConnectAsync();
		}
	}

	public void DisconnectIfAllTriggersDisabled()
	{
		if (!IsDisconnecting && !IsDisconnected && (!Plugin.Config.Enabled || !AnyTriggerEnabled))
		{
			Disconnect();
		}
	}

	public Task Connect()
	{
		if (IsConnecting || IsConnected)
		{
			return Task.CompletedTask;
		}
		if (!Plugin.Config.Discord.UsePyonServer && StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.BotToken))
		{
			return Task.CompletedTask;
		}
		VerificationKey = string.Empty;
		string token = (Plugin.Config.Discord.UsePyonServer ? DisGen.Dec() : Plugin.Config.Discord.BotToken);
		Task result = Client.LoginAsync(TokenType.Bot, token);
		Client.StartAsync();
		return result;
	}

	public async Task ConnectAsync()
	{
		if (IsConnecting || IsConnected || (!Plugin.Config.Discord.UsePyonServer && StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.BotToken)))
		{
			return;
		}
		try
		{
			VerificationKey = string.Empty;
			string token = (Plugin.Config.Discord.UsePyonServer ? DisGen.Dec() : Plugin.Config.Discord.BotToken);
			await Client.LoginAsync(TokenType.Bot, token).ConfigureAwait(continueOnCapturedContext: false);
			await Client.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			Plugin.Log.Error(ex, "Discord Connection Failed", Array.Empty<object>());
		}
	}

	public Task Disconnect()
	{
		this.OnActivity?.Invoke(null, null);
		if (IsDisconnecting || IsDisconnected)
		{
			return Task.CompletedTask;
		}
		VerificationKey = string.Empty;
		return Client.LogoutAsync().ContinueWith(delegate
		{
			Client.StopAsync();
			Guild = null;
		});
	}

	public void GenerateVerificationKey()
	{
		VerificationKey = DisGen.GenKey();
	}
}

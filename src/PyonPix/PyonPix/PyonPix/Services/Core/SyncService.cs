using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Events;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Structs.Browser.WebMessages;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Shared.Sync;
using PyonPix.Shared.Sync.Dto;
using PyonPix.Shared.Sync.Dto.Auth;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Sync.Dto.Session;
using PyonPix.Shared.Sync.Dto.Subbed;
using PyonPix.Shared.Sync.Dto.Syncable;
using PyonPix.Ui;
using PyonPix.Utility;

namespace PyonPix.Services.Core;

public class SyncService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private sealed record PendingSyncedPixCreate(string LocalPixId, SyncedPixCreateDto Request);

	private ClientWebSocket? Socket;

	private CancellationTokenSource? ConnectionCts;

	private Task? ReceiveLoopTask;

	private readonly SemaphoreSlim ConnectionLock = new SemaphoreSlim(1, 1);

	private readonly Random RNG = new Random();

	private const int MaxConnectionAttempts = 1000;

	private readonly object SyncablePixsLock = new object();

	public List<SyncablePixQueryItemDto> SyncablePixs = new List<SyncablePixQueryItemDto>();

	private volatile bool IsDisconnectRequested;

	private int ReconnectPending;

	private readonly ConcurrentDictionary<string, PendingSyncedPixCreate> _pendingSyncedPixCreates = new ConcurrentDictionary<string, PendingSyncedPixCreate>();

	private StateService? StateService => Services.Get<StateService>();

	private PixService? PixService => Services.Get<PixService>();

	public ConnectionState State { get; private set; }

	public string? StatusMessage { get; private set; }

	public ServerSession Server { get; private set; } = new ServerSession();

	public ClientSession Client { get; private set; } = new ClientSession();

	private bool IsSocketReady
	{
		get
		{
			if (Socket != null && ConnectionCts != null)
			{
				return Socket.State == WebSocketState.Open;
			}
			return false;
		}
	}

	public bool IsConnectedAuth
	{
		get
		{
			if (IsSocketReady && State == ConnectionState.Connected)
			{
				return Client.IsAuthenticated;
			}
			return false;
		}
	}

	public event Action<ConnectionState, string?, StatusType>? StateChanged;

	public event Action? AuthKeyReceived;

	public event Action<bool>? StyleUpdateResponse;

	public event Action? SyncablePixsUpdated;

	public event Action<string>? SubscriptionFailed;

	public event Action<LocalPix, SyncedPix>? SyncedPixCreated;

	public event Action<string, LocalPix?>? SyncedPixDeleted;

	public event Action<PremiumStatus>? PremiumStatusChanged;

	public event Action<PixMemberChangeRankSuccessDto>? PixMemberChangeRankSuccess;

	public event Action<PixMemberChangeRankFailedDto>? PixMemberChangeRankFailed;

	public event Action<PixMemberRemoveSuccessDto>? PixMemberRemoveSuccess;

	public event Action<PixMemberRemoveFailedDto>? PixMemberRemoveFailed;

	public event Action<string>? SyncedPixUnsubscribed;

	public event Action<SyncedPixMembersResponseDto>? SyncedPixMembersUpdated;

	public override Task Initialize()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		PixService? pixService = PixService;
		if (pixService != null)
		{
			pixService.PixUpdated += OnPixUpdated;
		}
		StateService? stateService = StateService;
		if (stateService != null)
		{
			stateService.TerritoryChanged += delegate(bool _, bool _, TerritoryData? territory)
			{
				SendTerritoryUpdateAsync(territory);
			};
		}
		StateService? stateService2 = StateService;
		if (stateService2 != null)
		{
			stateService2.TerritoryLoaded += delegate(TerritoryData? territory)
			{
				SendTerritoryUpdateAsync(territory);
				QuerySyncablePixs();
			};
		}
		StateService? stateService3 = StateService;
		if (stateService3 != null)
		{
			stateService3.InitialLoad += delegate
			{
				if (Config.Sync.AutoConnect)
				{
					Connect();
				}
			};
		}
		Services.ClientState.Login += delegate
		{
			if (Config.Sync.AutoConnect)
			{
				Connect();
			}
		};
		Services.ClientState.Logout += (LogoutDelegate)delegate
		{
			Disconnect();
		};
		return Task.CompletedTask;
	}

	public void QuerySyncablePixs()
	{
		QuerySyncablePixsAsync();
	}

	public void SubscribePix(string pixId, string? secretKey)
	{
		SubscribePixAsync(pixId, secretKey);
	}

	public void UnsubscribePix(string pixId)
	{
		UnsubscribePixAsync(pixId);
	}

	public void DeleteSyncedPix(string pixId)
	{
		DeleteSyncedPixAsync(pixId);
	}

	public void ChangePixMemberRank(string pixId, long characterId, PixRank newRank)
	{
		ChangePixMemberRankAsync(pixId, characterId, newRank);
	}

	public void RemovePixMember(string pixId, long characterId)
	{
		RemovePixMemberAsync(pixId, characterId);
	}

	private async Task QuerySyncablePixsAsync()
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.SyncedPixQueryRequest, new { });
		}
	}

	private async Task SubscribePixAsync(string pixId, string? secretKey)
	{
		if (!string.IsNullOrWhiteSpace(pixId) && IsConnectedAuth)
		{
			await SendAsync(MessageType.SyncedPixSubscribe, new SyncedPixSubscribeDto
			{
				PixId = pixId.Trim().ToUpperInvariant(),
				SecretKey = (string.IsNullOrWhiteSpace(secretKey) ? null : secretKey.Trim())
			});
		}
	}

	private async Task UnsubscribePixAsync(string pixId)
	{
		if (!string.IsNullOrWhiteSpace(pixId) && IsConnectedAuth)
		{
			await SendAsync(MessageType.SyncedPixUnsubscribe, new SyncedPixUnsubscribeDto
			{
				PixId = pixId
			});
		}
	}

	private async Task DeleteSyncedPixAsync(string pixId)
	{
		if (!string.IsNullOrWhiteSpace(pixId) && IsConnectedAuth)
		{
			await SendAsync(MessageType.SyncedPixDelete, new SyncedPixDeleteDto
			{
				PixId = pixId
			});
		}
	}

	private async Task SendTerritoryUpdateAsync(TerritoryData? territory)
	{
		if (!(territory == null) && IsConnectedAuth)
		{
			await SendAsync(MessageType.ClientTerritoryUpdate, territory.ToDto());
		}
	}

	private void OnPixUpdated(PixUpdate u)
	{
		if (u.Pix == null || !IsConnectedAuth || !u.EditFinished || u.Origin != PixUpdateOrigin.Local || PixService == null || !PixService.CanSyncEdit(u.Pix) || !(u.Pix is SyncedPix syncedPix))
		{
			return;
		}
		BaseSyncedPixUpdate baseSyncedPixUpdate = null;
		switch (u.Type)
		{
		case PixUpdateType.InfoProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateInfoProperties(syncedPix.Id, syncedPix.Info.ToSynced());
			break;
		case PixUpdateType.Uri:
			if (!Config.Global.Browser.SyncFileScheme && BrowserUtil.IsFileScheme(syncedPix.Browser.Uri))
			{
				return;
			}
			baseSyncedPixUpdate = new SyncedPixUpdateUri(syncedPix.Id, syncedPix.Browser.Uri);
			break;
		case PixUpdateType.BrowserProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateBrowserProperties(syncedPix.Id, syncedPix.Browser.ToSynced());
			break;
		case PixUpdateType.MediaState:
			baseSyncedPixUpdate = new SyncedPixUpdateMediaState(syncedPix.Id, syncedPix.Media);
			break;
		case PixUpdateType.RendererTransform:
		case PixUpdateType.RendererProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateRendererProperties(syncedPix.Id, syncedPix.Renderer.ToSynced());
			break;
		case PixUpdateType.LightTransform:
		case PixUpdateType.LightProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateLightProperties(syncedPix.Id, syncedPix.Light.ToSynced());
			break;
		case PixUpdateType.AudioProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateAudioProperties(syncedPix.Id, syncedPix.Audio.ToSynced());
			break;
		case PixUpdateType.SyncProperties:
			baseSyncedPixUpdate = new SyncedPixUpdateSyncProperties(syncedPix.Id, syncedPix.Sync.ToSynced());
			break;
		default:
			baseSyncedPixUpdate = new SyncedPixUpdate(syncedPix.Id, syncedPix.Info.ToSynced(), syncedPix.Browser.ToSynced(), syncedPix.Renderer.ToSynced(), syncedPix.Light.ToSynced(), syncedPix.Audio.ToSynced(), syncedPix.Sync.ToSynced());
			break;
		}
		if (baseSyncedPixUpdate != null)
		{
			SendAsync(MessageType.SyncedPixUpdate, baseSyncedPixUpdate);
		}
	}

	public override void Update()
	{
		if (Client.AuthExpiration.HasValue && Client.AuthExpirationTime.Value.TotalSeconds <= 0.0)
		{
			Client.AuthExpiration = null;
			Services.Log.Warning("[SyncService] AuthKey Expired, Disconnecting..", Array.Empty<object>());
			Disconnect("AuthKey Expired");
		}
	}

	public void Connect()
	{
		ConnectAsync();
	}

	private async Task ConnectAsync()
	{
		IsDisconnectRequested = false;
		if (StateService == null)
		{
			return;
		}
		await ConnectionLock.WaitAsync();
		ConnectionState state = State;
		if ((uint)(state - 1) <= 1u)
		{
			ConnectionLock.Release();
			return;
		}
		ConnectionCts?.Cancel();
		ConnectionCts?.Dispose();
		ConnectionCts = new CancellationTokenSource();
		Services.Log.Verbose("[SyncService] Connecting..", Array.Empty<object>());
		SetState(ConnectionState.Connecting, "Connecting..", StatusType.None);
		ConnectionLock.Release();
		for (int attempt = 1; attempt <= 1000; attempt++)
		{
			try
			{
				ClientWebSocket socket = new ClientWebSocket();
				await Task.Delay(1000, ConnectionCts.Token);
				await socket.ConnectAsync(Api.Socket, ConnectionCts.Token);
				await ConnectionLock.WaitAsync();
				Socket?.Dispose();
				Socket = socket;
				ConnectionLock.Release();
				ReceiveLoopTask = Task.Run(() => ReceiveLoopAsync(ConnectionCts.Token));
				await SendAsync(MessageType.AuthRequest, new AuthRequestDto(Plugin.Version, StateService.LocalPlayerContentId, Config.Sync.SecretKey, StateService.CurrentTerritory?.ToDto() ?? new TerritoryDto(0, 0, 0, 0, 0)));
				Services.Log.Verbose($"[SyncService] Connected ({attempt})", Array.Empty<object>());
				SetState(ConnectionState.Connected, "Connected", StatusType.Hide);
				return;
			}
			catch (OperationCanceledException value)
			{
				Services.Log.Warning($"[SyncService] Connection Aborted ({attempt}) {value}", Array.Empty<object>());
				SetState(ConnectionState.Disconnected, "Connection Aborted", StatusType.Warn);
				return;
			}
			catch (Exception value2) when (attempt < 1000)
			{
				Services.Log.Warning($"[SyncService] Connection Failed ({attempt}): {value2}", Array.Empty<object>());
				StatusMessage = "Reconnecting..";
				try
				{
					await Task.Delay(Math.Min(60000, 2000 * attempt) + RNG.Next(0, 500), ConnectionCts.Token);
				}
				catch (OperationCanceledException value3)
				{
					Services.Log.Warning($"[SyncService] Reconnect Aborted ({attempt}): {value3}", Array.Empty<object>());
					SetState(ConnectionState.Disconnected, "Reconnect Aborted", StatusType.Warn);
					return;
				}
			}
			catch (Exception value4)
			{
				Services.Log.Error($"[SyncService] Connection Failed ({attempt}): {value4}", Array.Empty<object>());
				SetState(ConnectionState.Disconnected, "Connection Failed", StatusType.Error);
				StatusMessage = "Connection Failed";
				return;
			}
		}
		Services.Log.Warning("[SyncService] Connection Attempts Exceeded", Array.Empty<object>());
		SetState(ConnectionState.Disconnected, "Connection Attempts Exceeded", StatusType.Error);
	}

	public void AbortConnection()
	{
		ConnectionCts?.Cancel();
	}

	public void Disconnect(string reason = "Client Disconnected", StatusType status = StatusType.None)
	{
		DisconnectAsync(reason, status);
	}

	private async Task DisconnectAsync(string reason = "Client Disconnected", StatusType status = StatusType.None)
	{
		IsDisconnectRequested = true;
		ConnectionCts?.Cancel();
		await ConnectionLock.WaitAsync();
		try
		{
			ClientWebSocket socket = Socket;
			bool flag;
			if (socket != null)
			{
				WebSocketState state = socket.State;
				if (state == WebSocketState.Open || state == WebSocketState.CloseReceived)
				{
					flag = true;
					goto IL_00b8;
				}
			}
			flag = false;
			goto IL_00b8;
			IL_00b8:
			if (flag)
			{
				try
				{
					await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
				}
				catch
				{
				}
			}
			SetState(ConnectionState.Disconnected, reason, status);
		}
		finally
		{
			ConnectionLock.Release();
		}
	}

	private async Task ReceiveLoopAsync(CancellationToken token)
	{
		if (Socket == null)
		{
			return;
		}
		byte[] buffer = new byte[4096];
		using MemoryStream ms = new MemoryStream();
		_ = 4;
		try
		{
			while (Socket.State == WebSocketState.Open && !token.IsCancellationRequested)
			{
				ms.SetLength(0L);
				try
				{
					WebSocketReceiveResult webSocketReceiveResult;
					do
					{
						webSocketReceiveResult = await Socket.ReceiveAsync(buffer, token);
						if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
						{
							string text = webSocketReceiveResult.CloseStatusDescription ?? "Unknown";
							Services.Log.Warning("[SyncService] Server Closed Connection: " + text, Array.Empty<object>());
							SetState(ConnectionState.Disconnected, text, StatusType.Error);
							return;
						}
						ms.Write(buffer, 0, webSocketReceiveResult.Count);
					}
					while (!webSocketReceiveResult.EndOfMessage && !token.IsCancellationRequested);
				}
				catch (OperationCanceledException value)
				{
					if (!IsDisconnectRequested)
					{
						Services.Log.Warning($"[SyncService] Disconnected (Aborted): {value}", Array.Empty<object>());
					}
					break;
				}
				catch (WebSocketException value2)
				{
					Services.Log.Warning($"[SyncService] WebSocketException ({value2})", Array.Empty<object>());
					break;
				}
				catch (IOException value3)
				{
					Services.Log.Warning($"[SyncService] IOException ({value3})", Array.Empty<object>());
					break;
				}
				if (!SyncData.TryGetMessage(Encoding.UTF8.GetString(ms.ToArray()), out SocketMessage message))
				{
					continue;
				}
				switch (message.Type)
				{
				case MessageType.Ping:
				{
					if (SyncData.TryGetObject<ServerSessionDto>(message.Data, out var dto10))
					{
						Server.UserCount = dto10.UserCount;
						Server.PixCount = dto10.PixCount;
						await SendAsync(MessageType.Pong, null);
					}
					break;
				}
				case MessageType.AuthCreateSuccess:
				{
					if (StateService == null)
					{
						break;
					}
					SetState(ConnectionState.Connected, "Connected", StatusType.Hide);
					if (SyncData.TryGetObject<AuthCreateSuccessDto>(message.Data, out var dto13))
					{
						Client.IsSecretKeyInvalid = false;
						Client.IsAuthenticated = true;
						Client.AuthKey = null;
						dto13.Style.ApplyTo(Client.Style);
						Config.Sync.GetCurrentCharacterProperties(Config, StateService).Alias = Client.Style.Alias;
						if (Client.Premium.IsSupporter != dto13.Premium.IsSupporter || Client.Premium.IsSubscriber != dto13.Premium.IsSubscriber)
						{
							Client.Premium = dto13.Premium;
							this.PremiumStatusChanged?.Invoke(Client.Premium);
						}
						Config.Sync.SecretKey = dto13.SecretKey;
						Config.Save();
						lock (SyncablePixsLock)
						{
							SyncablePixs = dto13.SyncablePixs;
						}
						this.SyncablePixsUpdated?.Invoke();
					}
					break;
				}
				case MessageType.AuthLoginSuccess:
				{
					if (PixService == null || StateService == null)
					{
						break;
					}
					SetState(ConnectionState.Connected, "Connected", StatusType.Hide);
					if (SyncData.TryGetObject<AuthLoginSuccessDto>(message.Data, out var dto17))
					{
						Server.UserCount = dto17.ServerSession.UserCount;
						Server.PixCount = dto17.ServerSession.PixCount;
						Client.IsSecretKeyInvalid = false;
						Client.IsAuthenticated = true;
						Client.AuthKey = null;
						dto17.Style.ApplyTo(Client.Style);
						CharacterProperties currentCharacterProperties2 = Config.Sync.GetCurrentCharacterProperties(Config, StateService);
						if (currentCharacterProperties2.Alias != Client.Style.Alias)
						{
							currentCharacterProperties2.Alias = Client.Style.Alias;
							Config.Save();
						}
						if (Client.Premium.IsSupporter != dto17.Premium.IsSupporter || Client.Premium.IsSubscriber != dto17.Premium.IsSubscriber)
						{
							Client.Premium = dto17.Premium;
							this.PremiumStatusChanged?.Invoke(Client.Premium);
						}
						PixService.AddOrUpdateSyncedPixs(dto17.SubbedPixs);
						lock (SyncablePixsLock)
						{
							SyncablePixs = dto17.SyncablePixs;
						}
						this.SyncablePixsUpdated?.Invoke();
					}
					break;
				}
				case MessageType.AuthRequired:
				{
					if (SyncData.TryGetObject<AuthRequiredDto>(message.Data, out var dto8))
					{
						Client.IsSecretKeyInvalid = false;
						Client.IsAuthenticated = false;
						Client.AuthKey = dto8.SecretKey;
						Client.AuthExpiration = dto8.ExpirationTimestamp;
						SetState(ConnectionState.Connected, "Connected, Authentication Required.");
						this.AuthKeyReceived?.Invoke();
					}
					break;
				}
				case MessageType.AuthFailed:
				{
					if (SyncData.TryGetObject<AuthFailedDto>(message.Data, out var dto20))
					{
						string text2 = "Server Disconnected";
						if (dto20.Reason == AuthFailedReason.InvalidAuth)
						{
							text2 = "Invalid AuthKey";
							Client.IsSecretKeyInvalid = true;
							StatusMessage = text2;
						}
						else if (dto20.Reason == AuthFailedReason.Forbidden)
						{
							text2 = "Auth Forbidden";
						}
						Services.Log.Warning("[SyncService] Disconnected (AuthFailed): " + text2, Array.Empty<object>());
						Disconnect(text2, StatusType.Error);
						return;
					}
					break;
				}
				case MessageType.StyleUpdateSuccess:
				{
					if (PixService != null && StateService != null && SyncData.TryGetObject<SyncedCharacterProperties>(message.Data, out var dto14))
					{
						dto14.ApplyTo(Client.Style);
						CharacterProperties currentCharacterProperties = Config.Sync.GetCurrentCharacterProperties(Config, StateService);
						if (currentCharacterProperties.Alias != Client.Style.Alias)
						{
							currentCharacterProperties.Alias = Client.Style.Alias;
							Config.Save();
						}
						PixService.ApplyPixStyleUpdate(new SubbedPixStyleUpdateDto(StateService.LocalPlayerContentId, dto14.Alias, dto14.AliasStyle, dto14.PixStyle));
						this.StyleUpdateResponse?.Invoke(obj: true);
					}
					break;
				}
				case MessageType.StyleUpdateFailed:
					this.StyleUpdateResponse?.Invoke(obj: false);
					break;
				case MessageType.SubbedPixStyleUpdated:
				{
					if (PixService != null && SyncData.TryGetObject<SubbedPixStyleUpdateDto>(message.Data, out var dto2))
					{
						PixService.ApplyPixStyleUpdate(dto2);
					}
					break;
				}
				case MessageType.SyncedPixCreateSuccess:
				{
					if (PixService == null || !SyncData.TryGetObject<SyncedPixCreateSuccessDto>(message.Data, out var dto3))
					{
						break;
					}
					if (_pendingSyncedPixCreates.TryRemove(dto3.RequestId, out PendingSyncedPixCreate value4))
					{
						IPix pix = PixService.GetPix(value4.LocalPixId);
						if (pix is LocalPix localPix)
						{
							SyncedPix syncedPix = await PixService.CreateSyncedPixAsync(localPix, value4.Request, dto3);
							if (syncedPix != null)
							{
								this.SyncedPixCreated?.Invoke(localPix, syncedPix);
							}
						}
					}
					QuerySyncablePixs();
					break;
				}
				case MessageType.SyncedPixCreateFailed:
				{
					if (SyncData.TryGetObject<SyncedPixCreateFailedDto>(message.Data, out var dto21))
					{
						_pendingSyncedPixCreates.TryRemove(dto21.RequestId, out PendingSyncedPixCreate _);
						Services.Log.Warning("[SyncService] SyncedPixCreateFailed: " + dto21.Reason, Array.Empty<object>());
					}
					break;
				}
				case MessageType.SubbedPixQueryResponse:
				{
					if (PixService != null && SyncData.TryGetObject<SubbedPixQueryListDto>(message.Data, out var dto19))
					{
						PixService.AddOrUpdateSyncedPixs(dto19.SubbedPixs);
						QuerySyncablePixs();
					}
					break;
				}
				case MessageType.SyncablePixQueryResponse:
				{
					if (SyncData.TryGetObject<SyncablePixQueryListDto>(message.Data, out var dto11))
					{
						lock (SyncablePixsLock)
						{
							SyncablePixs = dto11.SyncablePixs;
						}
						this.SyncablePixsUpdated?.Invoke();
					}
					break;
				}
				case MessageType.SyncedPixSubscribeSuccess:
				{
					if (PixService != null && SyncData.TryGetObject<SyncedPixSubscribeSuccessDto>(message.Data, out var dto6))
					{
						PixService.AddOrUpdateSyncedPix(dto6.Pix);
					}
					break;
				}
				case MessageType.SyncedPixSubscribeFailed:
				{
					if (SyncData.TryGetObject<SyncedPixSubscribeFailedDto>(message.Data, out var dto23))
					{
						this.SubscriptionFailed?.Invoke(dto23.Reason);
					}
					break;
				}
				case MessageType.SyncedPixUnsubscribeSuccess:
				{
					if (PixService != null && SyncData.TryGetObject<SyncedPixUnsubscribeSuccessDto>(message.Data, out var dto18) && !string.IsNullOrWhiteSpace(dto18.PixId))
					{
						PixService.RemoveSyncedSubscription(dto18.PixId);
						this.SyncedPixUnsubscribed?.Invoke(dto18.PixId);
					}
					break;
				}
				case MessageType.SyncedPixUnsubscribeFailed:
				{
					if (SyncData.TryGetObject<SyncedPixUnsubscribeFailedDto>(message.Data, out var dto15))
					{
						this.SubscriptionFailed?.Invoke(dto15.Reason);
					}
					break;
				}
				case MessageType.SyncedPixDeleteSuccess:
				{
					if (PixService != null && SyncData.TryGetObject<SyncedPixDeleteSuccessDto>(message.Data, out var success))
					{
						LocalPix arg = await PixService.RemoveSyncedPixAsync(success.PixId);
						this.SyncedPixDeleted?.Invoke(success.PixId, arg);
					}
					break;
				}
				case MessageType.SyncedPixDeleteFailed:
				{
					if (SyncData.TryGetObject<SyncedPixDeleteFailedDto>(message.Data, out var dto5))
					{
						this.SubscriptionFailed?.Invoke(dto5.Reason);
					}
					break;
				}
				case MessageType.SyncedPixDeleted:
				{
					if (PixService != null && SyncData.TryGetObject<SyncedPixDeletedDto>(message.Data, out var dto22))
					{
						PixService.RemoveSyncedSubscription(dto22.PixId);
						this.SyncedPixUnsubscribed?.Invoke(dto22.PixId);
					}
					break;
				}
				case MessageType.SyncedPixUpdate:
				{
					if (PixService != null && SyncData.TryGetSyncedPixUpdate(message.Data, out BaseSyncedPixUpdate update2))
					{
						PixService.ApplyPixPropertyUpdate(update2);
					}
					break;
				}
				case MessageType.SyncMediaStateResponse:
				{
					if (PixService != null && SyncData.TryGetSyncedPixUpdate(message.Data, out BaseSyncedPixUpdate update))
					{
						PixService.ApplyPixPropertyUpdate(update);
					}
					break;
				}
				case MessageType.SyncedPixMembersUpdate:
				{
					if (SyncData.TryGetObject<SyncedPixMembersResponseDto>(message.Data, out var dto16))
					{
						this.SyncedPixMembersUpdated?.Invoke(dto16);
					}
					break;
				}
				case MessageType.PremiumStatusChanged:
				{
					if (SyncData.TryGetObject<PremiumStatus>(message.Data, out var dto12))
					{
						Client.Premium = dto12;
						this.PremiumStatusChanged?.Invoke(dto12);
						await SendStyleUpdateAsync();
					}
					break;
				}
				case MessageType.PixMemberChangeRankSuccess:
				{
					if (SyncData.TryGetObject<PixMemberChangeRankSuccessDto>(message.Data, out var dto9))
					{
						this.PixMemberChangeRankSuccess?.Invoke(dto9);
					}
					break;
				}
				case MessageType.PixMemberChangeRankFailed:
				{
					if (SyncData.TryGetObject<PixMemberChangeRankFailedDto>(message.Data, out var dto7))
					{
						this.PixMemberChangeRankFailed?.Invoke(dto7);
					}
					break;
				}
				case MessageType.PixMemberRemoveSuccess:
				{
					if (SyncData.TryGetObject<PixMemberRemoveSuccessDto>(message.Data, out var dto4))
					{
						this.PixMemberRemoveSuccess?.Invoke(dto4);
					}
					break;
				}
				case MessageType.PixMemberRemoveFailed:
				{
					if (SyncData.TryGetObject<PixMemberRemoveFailedDto>(message.Data, out var dto))
					{
						this.PixMemberRemoveFailed?.Invoke(dto);
					}
					break;
				}
				}
			}
		}
		catch (OperationCanceledException value6)
		{
			Services.Log.Warning($"[SyncService] Server Disconnected [Socket Closed] (Aborted): {value6}", Array.Empty<object>());
		}
		catch (Exception value7)
		{
			Services.Log.Warning($"[SyncService] Socket Failed: {value7}", Array.Empty<object>());
			SetState(ConnectionState.Disconnected, "Error: Check /xllog for details", StatusType.Error);
		}
		finally
		{
			if (!IsDisconnectRequested && Interlocked.Exchange(ref ReconnectPending, 1) == 0)
			{
				Services.Log.Warning("[SyncService] Server Disconnected [Socket Closed], Reconnecting..", Array.Empty<object>());
				SetState(ConnectionState.Disconnected, "Server Disconnected, Reconnecting..", StatusType.Warn);
				StatusMessage = "Reconnecting..";
				Task.Run(async delegate
				{
					try
					{
						Connect();
					}
					finally
					{
						Interlocked.Exchange(ref ReconnectPending, 0);
					}
				});
			}
		}
	}

	public void CreateSyncedPix(IPix pix, SyncedPixMetaDto meta)
	{
		CreateSyncedPixAsync(pix, meta);
	}

	private async Task CreateSyncedPixAsync(IPix pix, SyncedPixMetaDto meta)
	{
		if (IsConnectedAuth && PixService != null)
		{
			string text = Guid.NewGuid().ToString("N");
			SyncedPixCreateDto syncedPixCreateDto = new SyncedPixCreateDto
			{
				RequestId = text,
				LocalPixId = pix.Id,
				Pix = PixService.BuildPixDto(pix),
				Meta = meta
			};
			_pendingSyncedPixCreates[text] = new PendingSyncedPixCreate(pix.Id, syncedPixCreateDto);
			await SendAsync(MessageType.SyncedPixCreate, syncedPixCreateDto);
		}
	}

	private async Task SendAsync(MessageType type, object? data)
	{
		if (IsSocketReady)
		{
			byte[] array = SyncData.CreateMessageBuffer(type, data);
			await Socket.SendAsync(array, WebSocketMessageType.Text, endOfMessage: true, ConnectionCts.Token);
		}
	}

	private void SetState(ConnectionState connectionState, string? statusMessage, StatusType statusType = StatusType.Info)
	{
		StatusMessage = null;
		State = connectionState;
		if (Client.IsAuthenticated)
		{
			Client.AuthExpiration = null;
		}
		this.StateChanged?.Invoke(connectionState, statusMessage, statusType);
		if (State != ConnectionState.Disconnected)
		{
			return;
		}
		Client.IsAuthenticated = false;
		try
		{
			ConnectionCts?.Cancel();
		}
		catch
		{
		}
		ConnectionCts?.Dispose();
		ConnectionCts = null;
		if (Socket != null)
		{
			try
			{
				Socket.Dispose();
			}
			catch
			{
			}
			Socket = null;
		}
		lock (SyncablePixsLock)
		{
			SyncablePixs = new List<SyncablePixQueryItemDto>();
		}
		this.SyncablePixsUpdated?.Invoke();
	}

	public Task SyncMediaState(string pixId, MediaState? media)
	{
		if (!IsConnectedAuth)
		{
			return Task.CompletedTask;
		}
		return SendAsync(MessageType.SyncMediaState, new SyncedPixUpdateMediaState(pixId, media));
	}

	public async Task RequestPixMembersAsync(string pixId)
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.SyncedPixMembersRequest, new SyncedPixMembersRequestDto
			{
				PixId = pixId
			});
		}
	}

	private async Task ChangePixMemberRankAsync(string pixId, long characterId, PixRank newRank)
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.PixMemberChangeRank, new PixMemberChangeRankDto
			{
				PixId = pixId,
				CharacterId = characterId,
				NewRank = newRank
			});
		}
	}

	private async Task RemovePixMemberAsync(string pixId, long characterId)
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.PixMemberRemove, new PixMemberRemoveDto
			{
				PixId = pixId,
				CharacterId = characterId
			});
		}
	}

	public void ReportPix(string pixId)
	{
		ReportPixAsync(pixId);
	}

	private async Task ReportPixAsync(string pixId)
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.ReportPix, pixId);
		}
	}

	public void ReportUser(long characterId)
	{
		ReportUserAsync(characterId);
	}

	private async Task ReportUserAsync(long characterId)
	{
		if (IsConnectedAuth)
		{
			await SendAsync(MessageType.ReportUser, characterId);
		}
	}

	public void SendStyleUpdate()
	{
		SendStyleUpdateAsync();
	}

	private async Task SendStyleUpdateAsync()
	{
		if (!IsConnectedAuth || StateService == null)
		{
			return;
		}
		SyncedCharacterProperties syncedCharacterProperties = Config.Sync.GetCurrentCharacterProperties(Config, StateService).ToSynced();
		if (!Client.Premium.IsSubscriber)
		{
			StyleDto? aliasStyle = syncedCharacterProperties.AliasStyle;
			if (aliasStyle != null)
			{
				aliasStyle.AnimationType = AnimationType.Static;
			}
			StyleDto? pixStyle = syncedCharacterProperties.PixStyle;
			if (pixStyle != null)
			{
				pixStyle.AnimationType = AnimationType.Static;
			}
		}
		await SendAsync(MessageType.StyleUpdate, syncedCharacterProperties);
	}

	public override async Task Dispose()
	{
		IsDisconnectRequested = true;
		await DisconnectAsync();
	}
}

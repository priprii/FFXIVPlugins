using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.FlatBuffers;
using PyonPix.Ipc;
using PyonPix.Shared.Structs.Renderer;

namespace PyonPix.Shared.Ipc;

public sealed class MemoryMappedIpc : IDisposable
{
	private readonly IpcChannel _inbound;

	private readonly IpcChannel _outbound;

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	private Task? _pollTask;

	private Task? _dispatchTask;

	private bool _started;

	private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();

	private readonly SemaphoreSlim _queueSignal = new SemaphoreSlim(0);

	private readonly SemaphoreSlim _sendGate = new SemaphoreSlim(1, 1);

	private readonly Dictionary<MessagePayload, Action<IpcMessage>> PayloadHandlers;

	public event Action<Command>? OnCommand;

	public event Action<Log>? OnLog;

	public event Action<InitializeBrowser>? OnInitializeBrowser;

	public event Action<HostInitializeState>? OnHostInitializeState;

	public event Action<TabInitializeState>? OnTabInitializeState;

	public event Action<CreateTab>? OnCreateTab;

	public event Action<DestroyTab>? OnDestroyTab;

	public event Action<UpdateFrame>? OnUpdateFrame;

	public event Action<CursorChanged>? OnCursorChanged;

	public event Action<NavigationStarting>? OnNavigationStarting;

	public event Action<HistoryChanged>? OnHistoryChanged;

	public event Action<TitleChanged>? OnTitleChanged;

	public event Action<NavigationCompleted>? OnNavigationCompleted;

	public event Action<NavigationCanceled>? OnNavigationCanceled;

	public event Action<FavIconChanged>? OnFavIconChanged;

	public event Action<WebMessageReceived>? OnWebMessageReceived;

	public event Action<UpdateMediaState>? OnUpdateMediaState;

	public event Action<ToggleAutoTheatreMode>? OnToggleAutoTheatreMode;

	public event Action<ToggleTheatreMode>? OnToggleTheatreMode;

	public event Action<ExtensionOperation>? OnExtensionOperation;

	public event Action<Navigate>? OnNavigate;

	public event Action<Reload>? OnReload;

	public event Action<StopNavigation>? OnStopNavigation;

	public event Action<Resize>? OnResize;

	public event Action<Reposition>? OnReposition;

	public event Action<SetFocusedTab>? OnSetFocusedTab;

	public event Action<SendMouseEvent>? OnSendMouseEvent;

	public event Action<UpdateSpatialAudio>? OnUpdateSpatialAudio;

	public event Action<OpenDevTools>? OnOpenDevTools;

	public event Action<InstallExtension>? OnInstallExtension;

	public event Action<UninstallExtension>? OnUninstallExtension;

	public event Action<EnableExtension>? OnEnableExtension;

	public event Action<DisableExtension>? OnDisableExtension;

	public MemoryMappedIpc(string baseName, bool isPlugin)
	{
		string text = baseName + "_ToRenderer";
		string text2 = baseName + "_ToPlugin";
		_outbound = new IpcChannel(isPlugin ? text : text2);
		_inbound = new IpcChannel(isPlugin ? text2 : text);
		PayloadHandlers = new Dictionary<MessagePayload, Action<IpcMessage>>
		{
			{
				MessagePayload.Command,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Command>(), out var value))
					{
						this.OnCommand?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.Log,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Log>(), out var value))
					{
						this.OnLog?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.InitializeBrowser,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<InitializeBrowser>(), out var value))
					{
						this.OnInitializeBrowser?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.HostInitializeState,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<HostInitializeState>(), out var value))
					{
						this.OnHostInitializeState?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.TabInitializeState,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<TabInitializeState>(), out var value))
					{
						this.OnTabInitializeState?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.CreateTab,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<CreateTab>(), out var value))
					{
						this.OnCreateTab?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.DestroyTab,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<DestroyTab>(), out var value))
					{
						this.OnDestroyTab?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.UpdateFrame,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<UpdateFrame>(), out var value))
					{
						this.OnUpdateFrame?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.CursorChanged,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<CursorChanged>(), out var value))
					{
						this.OnCursorChanged?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.NavigationStarting,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<NavigationStarting>(), out var value))
					{
						this.OnNavigationStarting?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.HistoryChanged,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<HistoryChanged>(), out var value))
					{
						this.OnHistoryChanged?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.TitleChanged,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<TitleChanged>(), out var value))
					{
						this.OnTitleChanged?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.NavigationCompleted,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<NavigationCompleted>(), out var value))
					{
						this.OnNavigationCompleted?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.NavigationCanceled,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<NavigationCanceled>(), out var value))
					{
						this.OnNavigationCanceled?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.FavIconChanged,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<FavIconChanged>(), out var value))
					{
						this.OnFavIconChanged?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.WebMessageReceived,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<WebMessageReceived>(), out var value))
					{
						this.OnWebMessageReceived?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.UpdateMediaState,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<UpdateMediaState>(), out var value))
					{
						this.OnUpdateMediaState?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.ToggleAutoTheatreMode,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<ToggleAutoTheatreMode>(), out var value))
					{
						this.OnToggleAutoTheatreMode?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.ToggleTheatreMode,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<ToggleTheatreMode>(), out var value))
					{
						this.OnToggleTheatreMode?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.ExtensionOperation,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<ExtensionOperation>(), out var value))
					{
						this.OnExtensionOperation?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.Navigate,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Navigate>(), out var value))
					{
						this.OnNavigate?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.Reload,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Reload>(), out var value))
					{
						this.OnReload?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.StopNavigation,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<StopNavigation>(), out var value))
					{
						this.OnStopNavigation?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.Resize,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Resize>(), out var value))
					{
						this.OnResize?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.Reposition,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<Reposition>(), out var value))
					{
						this.OnReposition?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.SetFocusedTab,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<SetFocusedTab>(), out var value))
					{
						this.OnSetFocusedTab?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.SendMouseEvent,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<SendMouseEvent>(), out var value))
					{
						this.OnSendMouseEvent?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.UpdateSpatialAudio,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<UpdateSpatialAudio>(), out var value))
					{
						this.OnUpdateSpatialAudio?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.OpenDevTools,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<OpenDevTools>(), out var value))
					{
						this.OnOpenDevTools?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.InstallExtension,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<InstallExtension>(), out var value))
					{
						this.OnInstallExtension?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.UninstallExtension,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<UninstallExtension>(), out var value))
					{
						this.OnUninstallExtension?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.EnableExtension,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<EnableExtension>(), out var value))
					{
						this.OnEnableExtension?.Invoke(value);
					}
				}
			},
			{
				MessagePayload.DisableExtension,
				delegate(IpcMessage msg)
				{
					if (TryGetPayload(msg.Payload<DisableExtension>(), out var value))
					{
						this.OnDisableExtension?.Invoke(value);
					}
				}
			}
		};
	}

	public void Start()
	{
		if (!_started)
		{
			_started = true;
			_pollTask = Task.Run((Func<Task?>)PollLoop);
			_dispatchTask = Task.Run((Func<Task?>)DispatchLoop);
		}
	}

	private async Task PollLoop()
	{
		_ = 1;
		try
		{
			while (!_cts.IsCancellationRequested)
			{
				bool flag = false;
				byte[] data;
				while (_inbound.TryRead(out data))
				{
					flag = true;
					_queue.Enqueue(data);
					_queueSignal.Release();
				}
				if (!flag)
				{
					await Task.Delay(1, _cts.Token).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await Task.Yield();
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private async Task DispatchLoop()
	{
		try
		{
			while (!_cts.IsCancellationRequested)
			{
				await _queueSignal.WaitAsync(_cts.Token).ConfigureAwait(continueOnCapturedContext: false);
				byte[] result;
				while (_queue.TryDequeue(out result))
				{
					Dispatch(result);
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private void Dispatch(byte[] data)
	{
		IpcMessage rootAsIpcMessage = IpcMessage.GetRootAsIpcMessage(new ByteBuffer(data));
		if (PayloadHandlers.TryGetValue(rootAsIpcMessage.PayloadType, out Action<IpcMessage> value))
		{
			value(rootAsIpcMessage);
		}
	}

	private static bool TryGetPayload<T>(T? payload, out T value) where T : struct
	{
		if (payload.HasValue)
		{
			value = payload.Value;
			return true;
		}
		value = default(T);
		return false;
	}

	public void SendCommand(CommandType type)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Command, Command.CreateCommand(flatBufferBuilder, type).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendLog(LogType type, string message)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Log, Log.CreateLog(flatBufferBuilder, type, flatBufferBuilder.CreateString(message)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendInitializeBrowser(string pluginPath, uint gamePid, LUID adapterLuid)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.InitializeBrowser, InitializeBrowser.CreateInitializeBrowser(flatBufferBuilder, flatBufferBuilder.CreateString(pluginPath), gamePid, adapterLuid.LowPart, adapterLuid.HighPart).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendHostInitializeState(StateType type, string? message = null)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.HostInitializeState, HostInitializeState.CreateHostInitializeState(flatBufferBuilder, type, string.IsNullOrEmpty(message) ? default(StringOffset) : flatBufferBuilder.CreateString(message)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendTabInitializeState(StateType type, string pixId, string? message = null)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.TabInitializeState, TabInitializeState.CreateTabInitializeState(flatBufferBuilder, type, flatBufferBuilder.CreateString(pixId), string.IsNullOrEmpty(message) ? default(StringOffset) : flatBufferBuilder.CreateString(message)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendCreateTab(string pixId, bool gpuAcceleration, int x, int y, uint w, uint h, bool syncCookies, string[] extensions)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		StringOffset[] array = new StringOffset[extensions.Length];
		for (int i = 0; i < extensions.Length; i++)
		{
			array[i] = flatBufferBuilder.CreateString(extensions[i]);
		}
		VectorOffset extensionsOffset = CreateTab.CreateExtensionsVector(flatBufferBuilder, array);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.CreateTab, CreateTab.CreateCreateTab(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), gpuAcceleration, x, y, w, h, syncCookies, extensionsOffset).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendDestroyTab(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.DestroyTab, DestroyTab.CreateDestroyTab(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendUpdateFrame(string pixId, long sharedTexture, uint w, uint h)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.UpdateFrame, UpdateFrame.CreateUpdateFrame(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), sharedTexture, w, h).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendCursorChanged(uint cursorId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.CursorChanged, CursorChanged.CreateCursorChanged(flatBufferBuilder, cursorId).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendNavigationStarting(string pixId, string uri, bool userInitiated)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.NavigationStarting, NavigationStarting.CreateNavigationStarting(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), flatBufferBuilder.CreateString(uri), userInitiated).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendHistoryChanged(string pixId, string uri)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.HistoryChanged, HistoryChanged.CreateHistoryChanged(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), flatBufferBuilder.CreateString(uri)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendTitleChanged(string pixId, string title)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.TitleChanged, TitleChanged.CreateTitleChanged(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), flatBufferBuilder.CreateString(title)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendNavigationCompleted(string pixId, uint statusCode)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.NavigationCompleted, NavigationCompleted.CreateNavigationCompleted(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), statusCode).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendNavigationCanceled(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.NavigationCanceled, NavigationCanceled.CreateNavigationCanceled(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendFavIconChanged(string pixId, byte[] data)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.FavIconChanged, FavIconChanged.CreateFavIconChanged(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), FavIconChanged.CreateDataVector(flatBufferBuilder, data)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendWebMessageReceived(string pixId, string json)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.WebMessageReceived, WebMessageReceived.CreateWebMessageReceived(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), flatBufferBuilder.CreateString(json)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendUpdateMediaState(string pixId, MediaStateAction action, bool isPlaying, long seekTime, long duration, long timeStamp)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.UpdateMediaState, UpdateMediaState.CreateUpdateMediaState(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), action, isPlaying, seekTime, duration, timeStamp).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendToggleAutoTheatreMode(bool state)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.ToggleAutoTheatreMode, ToggleAutoTheatreMode.CreateToggleAutoTheatreMode(flatBufferBuilder, state).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendToggleTheatreMode(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.ToggleTheatreMode, ToggleTheatreMode.CreateToggleTheatreMode(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendExtensionOperation(ExtensionOp extensionOp, string extensionId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.ExtensionOperation, ExtensionOperation.CreateExtensionOperation(flatBufferBuilder, extensionOp, flatBufferBuilder.CreateString(extensionId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendNavigate(string pixId, string uri)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Navigate, Navigate.CreateNavigate(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), flatBufferBuilder.CreateString(uri)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendReload(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Reload, Reload.CreateReload(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendStopNavigation(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.StopNavigation, StopNavigation.CreateStopNavigation(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendResize(string pixId, int x, int y, uint w, uint h)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Resize, Resize.CreateResize(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), x, y, w, h).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendReposition(string pixId, int x, int y)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.Reposition, Reposition.CreateReposition(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), x, y).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendSetFocusedTab(string pixId, bool byUserInput)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.SetFocusedTab, SetFocusedTab.CreateSetFocusedTab(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), byUserInput).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendSendMouseEvent(string pixId, uint msg, long wParam, long lParam)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.SendMouseEvent, SendMouseEvent.CreateSendMouseEvent(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), msg, wParam, lParam).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendUpdateSpatialAudio(string pixId, float left, float right)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.UpdateSpatialAudio, UpdateSpatialAudio.CreateUpdateSpatialAudio(flatBufferBuilder, flatBufferBuilder.CreateString(pixId), left, right).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendOpenDevTools(string pixId)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.OpenDevTools, OpenDevTools.CreateOpenDevTools(flatBufferBuilder, flatBufferBuilder.CreateString(pixId)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendInstallExtension(string extensionId, string extensionName)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.InstallExtension, InstallExtension.CreateInstallExtension(flatBufferBuilder, flatBufferBuilder.CreateString(extensionId), flatBufferBuilder.CreateString(extensionName)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendUninstallExtension(string extensionId, string extensionName)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.UninstallExtension, UninstallExtension.CreateUninstallExtension(flatBufferBuilder, flatBufferBuilder.CreateString(extensionId), flatBufferBuilder.CreateString(extensionName)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendEnableExtension(string extensionId, string extensionName)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.EnableExtension, EnableExtension.CreateEnableExtension(flatBufferBuilder, flatBufferBuilder.CreateString(extensionId), flatBufferBuilder.CreateString(extensionName)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendDisableExtension(string extensionId, string extensionName)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(128);
		IpcMessage.FinishIpcMessageBuffer(flatBufferBuilder, IpcMessage.CreateIpcMessage(flatBufferBuilder, MessagePayload.DisableExtension, DisableExtension.CreateDisableExtension(flatBufferBuilder, flatBufferBuilder.CreateString(extensionId), flatBufferBuilder.CreateString(extensionName)).Value));
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void Send(Action<FlatBufferBuilder> build)
	{
		FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(256);
		build(flatBufferBuilder);
		SendRaw(flatBufferBuilder.SizedByteArray());
	}

	public void SendRaw(byte[] data)
	{
		_sendGate.Wait();
		try
		{
			_outbound.Write(data);
		}
		finally
		{
			_sendGate.Release();
		}
	}

	public void Dispose()
	{
		_cts.Cancel();
		try
		{
			_queueSignal.Release();
		}
		catch
		{
		}
		try
		{
			_pollTask?.Wait(500);
		}
		catch
		{
		}
		try
		{
			_dispatchTask?.Wait(500);
		}
		catch
		{
		}
		_cts.Dispose();
		_queueSignal.Dispose();
		_sendGate.Dispose();
		_outbound.Dispose();
		_inbound.Dispose();
	}
}

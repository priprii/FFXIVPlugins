using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct IpcMessage : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public MessagePayload PayloadType
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return MessagePayload.NONE;
			}
			return (MessagePayload)__p.bb.Get(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static IpcMessage GetRootAsIpcMessage(ByteBuffer _bb)
	{
		return GetRootAsIpcMessage(_bb, default(IpcMessage));
	}

	public static IpcMessage GetRootAsIpcMessage(ByteBuffer _bb, IpcMessage obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public static bool VerifyIpcMessage(ByteBuffer _bb)
	{
		return new Verifier(_bb).VerifyBuffer("", sizePrefixed: false, IpcMessageVerify.Verify);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public IpcMessage __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public TTable? Payload<TTable>() where TTable : struct, IFlatbufferObject
	{
		int num = __p.__offset(6);
		if (num == 0)
		{
			return null;
		}
		return __p.__union<TTable>(num + __p.bb_pos);
	}

	public Command PayloadAsCommand()
	{
		return Payload<Command>().Value;
	}

	public Log PayloadAsLog()
	{
		return Payload<Log>().Value;
	}

	public InitializeBrowser PayloadAsInitializeBrowser()
	{
		return Payload<InitializeBrowser>().Value;
	}

	public HostInitializeState PayloadAsHostInitializeState()
	{
		return Payload<HostInitializeState>().Value;
	}

	public TabInitializeState PayloadAsTabInitializeState()
	{
		return Payload<TabInitializeState>().Value;
	}

	public CreateTab PayloadAsCreateTab()
	{
		return Payload<CreateTab>().Value;
	}

	public DestroyTab PayloadAsDestroyTab()
	{
		return Payload<DestroyTab>().Value;
	}

	public UpdateFrame PayloadAsUpdateFrame()
	{
		return Payload<UpdateFrame>().Value;
	}

	public CursorChanged PayloadAsCursorChanged()
	{
		return Payload<CursorChanged>().Value;
	}

	public NavigationStarting PayloadAsNavigationStarting()
	{
		return Payload<NavigationStarting>().Value;
	}

	public HistoryChanged PayloadAsHistoryChanged()
	{
		return Payload<HistoryChanged>().Value;
	}

	public TitleChanged PayloadAsTitleChanged()
	{
		return Payload<TitleChanged>().Value;
	}

	public NavigationCompleted PayloadAsNavigationCompleted()
	{
		return Payload<NavigationCompleted>().Value;
	}

	public NavigationCanceled PayloadAsNavigationCanceled()
	{
		return Payload<NavigationCanceled>().Value;
	}

	public FavIconChanged PayloadAsFavIconChanged()
	{
		return Payload<FavIconChanged>().Value;
	}

	public WebMessageReceived PayloadAsWebMessageReceived()
	{
		return Payload<WebMessageReceived>().Value;
	}

	public UpdateMediaState PayloadAsUpdateMediaState()
	{
		return Payload<UpdateMediaState>().Value;
	}

	public ToggleTheatreMode PayloadAsToggleTheatreMode()
	{
		return Payload<ToggleTheatreMode>().Value;
	}

	public ExtensionOperation PayloadAsExtensionOperation()
	{
		return Payload<ExtensionOperation>().Value;
	}

	public Navigate PayloadAsNavigate()
	{
		return Payload<Navigate>().Value;
	}

	public Reload PayloadAsReload()
	{
		return Payload<Reload>().Value;
	}

	public StopNavigation PayloadAsStopNavigation()
	{
		return Payload<StopNavigation>().Value;
	}

	public Resize PayloadAsResize()
	{
		return Payload<Resize>().Value;
	}

	public Reposition PayloadAsReposition()
	{
		return Payload<Reposition>().Value;
	}

	public SetFocusedTab PayloadAsSetFocusedTab()
	{
		return Payload<SetFocusedTab>().Value;
	}

	public SendMouseEvent PayloadAsSendMouseEvent()
	{
		return Payload<SendMouseEvent>().Value;
	}

	public UpdateSpatialAudio PayloadAsUpdateSpatialAudio()
	{
		return Payload<UpdateSpatialAudio>().Value;
	}

	public OpenDevTools PayloadAsOpenDevTools()
	{
		return Payload<OpenDevTools>().Value;
	}

	public InstallExtension PayloadAsInstallExtension()
	{
		return Payload<InstallExtension>().Value;
	}

	public UninstallExtension PayloadAsUninstallExtension()
	{
		return Payload<UninstallExtension>().Value;
	}

	public EnableExtension PayloadAsEnableExtension()
	{
		return Payload<EnableExtension>().Value;
	}

	public DisableExtension PayloadAsDisableExtension()
	{
		return Payload<DisableExtension>().Value;
	}

	public static Offset<IpcMessage> CreateIpcMessage(FlatBufferBuilder builder, MessagePayload payload_type = MessagePayload.NONE, int payloadOffset = 0)
	{
		builder.StartTable(2);
		AddPayload(builder, payloadOffset);
		AddPayloadType(builder, payload_type);
		return EndIpcMessage(builder);
	}

	public static void StartIpcMessage(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPayloadType(FlatBufferBuilder builder, MessagePayload payloadType)
	{
		builder.AddByte(0, (byte)payloadType, 0);
	}

	public static void AddPayload(FlatBufferBuilder builder, int payloadOffset)
	{
		builder.AddOffset(1, payloadOffset, 0);
	}

	public static Offset<IpcMessage> EndIpcMessage(FlatBufferBuilder builder)
	{
		return new Offset<IpcMessage>(builder.EndTable());
	}

	public static void FinishIpcMessageBuffer(FlatBufferBuilder builder, Offset<IpcMessage> offset)
	{
		builder.Finish(offset.Value);
	}

	public static void FinishSizePrefixedIpcMessageBuffer(FlatBufferBuilder builder, Offset<IpcMessage> offset)
	{
		builder.FinishSizePrefixed(offset.Value);
	}
}

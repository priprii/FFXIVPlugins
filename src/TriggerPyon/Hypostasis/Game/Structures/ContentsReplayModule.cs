using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;
using Hypostasis.Dalamud;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit, Size = 1840)]
[GameStructure("48 89 5C 24 ?? 57 48 83 EC 20 33 FF 48 8B D9 89 39 48 89 79 08")]
public struct ContentsReplayModule : IHypostasisStructure
{
	[StructLayout(LayoutKind.Explicit, Size = 112)]
	public struct InitZonePacket
	{
		[FieldOffset(0)]
		public ushort u0x0;

		[FieldOffset(2)]
		public ushort territoryType;

		[FieldOffset(4)]
		public ushort u0x4;

		[FieldOffset(6)]
		public ushort contentFinderCondition;
	}

	[StructLayout(LayoutKind.Explicit, Size = 192)]
	public struct UnknownPacket
	{
	}

	public unsafe delegate void BeginRecordingDelegate(ContentsReplayModule* contentsReplayModule, Bool saveRecording);

	public unsafe delegate void EndRecordingDelegate(ContentsReplayModule* contentsReplayModule);

	public unsafe delegate void OnZoneInPacketDelegate(ContentsReplayModule* contentsReplayModule, uint objectID, nint packet);

	public unsafe delegate Bool OnLoginDelegate(ContentsReplayModule* contentsReplayModule);

	public unsafe delegate void InitializeRecordingDelegate(ContentsReplayModule* contentsReplayModule);

	public unsafe delegate Bool RequestPlaybackDelegate(ContentsReplayModule* contentsReplayModule, byte slot);

	public unsafe delegate void ReceiveActorControlPacketDelegate(ContentsReplayModule* contentsReplayModule, uint objectID, nint packet);

	public unsafe delegate void BeginPlaybackDelegate(ContentsReplayModule* contentsReplayModule, Bool allowed);

	public unsafe delegate FFXIVReplay.DataSegment* GetReplayDataSegmentDelegate(ContentsReplayModule* contentsReplayModule);

	public unsafe delegate Bool SetChapterDelegate(ContentsReplayModule* contentsReplayModule, byte chapter);

	public unsafe delegate void OnSetChapterDelegate(ContentsReplayModule* contentsReplayModule, byte chapter);

	public unsafe delegate Bool WritePacketDelegate(ContentsReplayModule* contentsReplayModule, uint objectID, ushort opcode, byte* data, ulong length);

	public unsafe delegate Bool ReplayPacketDelegate(ContentsReplayModule* contentsReplayModule, FFXIVReplay.DataSegment* segment, byte* data);

	[FieldOffset(0)]
	public int gameBuildNumber;

	[FieldOffset(8)]
	public nint fileStream;

	[FieldOffset(16)]
	public nint fileStreamNextWrite;

	[FieldOffset(24)]
	public nint fileStreamEnd;

	[FieldOffset(32)]
	public long u0x20;

	[FieldOffset(40)]
	public long u0x28;

	[FieldOffset(48)]
	public long dataOffset;

	[FieldOffset(56)]
	public long overallDataOffset;

	[FieldOffset(64)]
	public long lastDataOffset;

	[FieldOffset(72)]
	public FFXIVReplay.Header replayHeader;

	[FieldOffset(176)]
	public FFXIVReplay.ChapterArray chapters;

	[FieldOffset(952)]
	public Utf8String contentTitle;

	[FieldOffset(1056)]
	public long nextDataSection;

	[FieldOffset(1064)]
	public long numberBytesRead;

	[FieldOffset(1072)]
	public int currentFileSection;

	[FieldOffset(1076)]
	public int dataLoadType;

	[FieldOffset(1080)]
	public long dataLoadOffset;

	[FieldOffset(1088)]
	public long dataLoadLength;

	[FieldOffset(1096)]
	public long dataLoadFileOffset;

	[FieldOffset(1104)]
	public long localCID;

	[FieldOffset(1112)]
	public byte currentReplaySlot;

	[FieldOffset(1120)]
	public Utf8String characterRecordingName;

	[FieldOffset(1224)]
	public Utf8String replayTitle;

	[FieldOffset(1328)]
	public Utf8String u0x530;

	[FieldOffset(1432)]
	public float recordingTime;

	[FieldOffset(1440)]
	public long recordingLength;

	[FieldOffset(1448)]
	public int u0x5A8;

	[FieldOffset(1452)]
	public byte u0x5AC;

	[FieldOffset(1453)]
	public byte nextReplaySaveSlot;

	[FieldOffset(1456)]
	public unsafe FFXIVReplay.Header* savedReplayHeaders;

	[FieldOffset(1464)]
	public nint u0x5B8;

	[FieldOffset(1472)]
	public nint u0x5C0;

	[FieldOffset(1480)]
	public byte u0x5C8;

	[FieldOffset(1484)]
	public uint localPlayerObjectID;

	[FieldOffset(1488)]
	public InitZonePacket initZonePacket;

	[FieldOffset(1600)]
	public long u0x640;

	[FieldOffset(1608)]
	public UnknownPacket u0x648;

	[FieldOffset(1800)]
	public int u0x708;

	[FieldOffset(1804)]
	public float seek;

	[FieldOffset(1808)]
	public float seekDelta;

	[FieldOffset(1812)]
	public float speed;

	[FieldOffset(1816)]
	public float u0x718;

	[FieldOffset(1820)]
	public byte selectedChapter;

	[FieldOffset(1824)]
	public uint startingMS;

	[FieldOffset(1828)]
	public int u0x724;

	[FieldOffset(1832)]
	public short u0x728;

	[FieldOffset(1834)]
	public byte status;

	[FieldOffset(1835)]
	public byte playbackControls;

	[FieldOffset(1836)]
	public byte u0x72C;

	public static readonly GameFunction<BeginRecordingDelegate> beginRecording = new GameFunction<BeginRecordingDelegate>("E8 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 83 C4 20 41 5C");

	public static readonly GameFunction<EndRecordingDelegate> endRecording = new GameFunction<EndRecordingDelegate>("E8 ?? ?? ?? ?? 32 C0 EB A3");

	public static readonly GameFunction<OnZoneInPacketDelegate> onZoneInPacket = new GameFunction<OnZoneInPacketDelegate>("E8 ?? ?? ?? ?? 45 33 C0 48 8D 56 10 8B CF E8 ?? ?? ?? ?? 48 8D 4E 6C");

	public static readonly GameFunction<OnLoginDelegate> onLogin = new GameFunction<OnLoginDelegate>("40 53 48 83 EC 20 F6 81 ?? ?? ?? ?? ?? 48 8B D9 0F 85 ?? ?? ?? ?? F6 81 ?? ?? ?? ?? ??");

	public static readonly GameFunction<InitializeRecordingDelegate> initializeRecording = new GameFunction<InitializeRecordingDelegate>("40 55 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 27 F6 81");

	public static readonly GameFunction<RequestPlaybackDelegate> requestPlayback = new GameFunction<RequestPlaybackDelegate>("48 89 5C 24 08 57 48 83 EC 30 F6 81 ?? ?? ?? ?? 04");

	public static readonly GameFunction<ReceiveActorControlPacketDelegate> receiveActorControlPacket = new GameFunction<ReceiveActorControlPacketDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 30 33 FF 48 8B D9");

	public static readonly GameFunction<BeginPlaybackDelegate> beginPlayback = new GameFunction<BeginPlaybackDelegate>("40 53 48 83 EC 30 0F B6 81 ?? ?? ?? ?? 48 8B D9 A8 01 0F 84 ?? ?? ?? ?? 24 FE");

	public static readonly GameFunction<InitializeRecordingDelegate> playbackUpdate = new GameFunction<InitializeRecordingDelegate>("E8 ?? ?? ?? ?? F6 83 ?? ?? ?? ?? 04 74 38 F6 83 ?? ?? ?? ?? 01");

	public static readonly GameFunction<SetChapterDelegate> setChapter = new GameFunction<SetChapterDelegate>("E8 ?? ?? ?? ?? 84 C0 E9 ?? ?? ?? ?? 48 8D 4F 10");

	public static readonly GameFunction<OnSetChapterDelegate> onSetChapter = new GameFunction<OnSetChapterDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 30 48 8B F1 0F B6 EA");

	public static readonly GameFunction<WritePacketDelegate> writePacket = new GameFunction<WritePacketDelegate>("E8 ?? ?? ?? ?? 84 C0 74 60 33 C0");

	public static readonly GameFunction<ReplayPacketDelegate> replayPacket = new GameFunction<ReplayPacketDelegate>("E8 ?? ?? ?? ?? 80 BB ?? ?? ?? ?? ?? 77 9A");

	public bool InPlayback => (playbackControls & 4) != 0;

	public bool IsPaused => (playbackControls & 8) != 0;

	public bool IsSavingPackets => (status & 4) != 0;

	public bool IsRecording => (status & 0x74) == 116;

	public bool IsLoadingChapter => selectedChapter < 64;

	public unsafe void BeginRecording(bool saveRecording = true)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			beginRecording.Invoke(contentsReplayModule, saveRecording);
		}
	}

	public unsafe void EndRecording()
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			endRecording.Invoke(contentsReplayModule);
		}
	}

	public unsafe bool OnLogin()
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			return onLogin.Invoke(contentsReplayModule);
		}
	}

	public unsafe void InitializeRecording()
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			initializeRecording.Invoke(contentsReplayModule);
		}
	}

	public unsafe bool RequestPlayback(byte slot = 0)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			return requestPlayback.Invoke(contentsReplayModule, slot);
		}
	}

	public unsafe void BeginPlayback(bool allowed = true)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			beginPlayback.Invoke(contentsReplayModule, allowed);
		}
	}

	public unsafe void PlaybackUpdate()
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			playbackUpdate.Invoke(contentsReplayModule);
		}
	}

	public unsafe bool SetChapter(byte chapter)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			return setChapter.Invoke(contentsReplayModule, chapter);
		}
	}

	public unsafe void OnSetChapter(byte chapter)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			onSetChapter.Invoke(contentsReplayModule, chapter);
		}
	}

	public unsafe bool WritePacket(uint objectID, ushort opcode, byte* data, ulong length)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			return writePacket.Invoke(contentsReplayModule, objectID, opcode, data, length);
		}
	}

	public unsafe bool WritePacket(uint objectID, ushort opcode, byte[] data)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			fixed (byte* data2 = data)
			{
				return writePacket.Invoke(contentsReplayModule, objectID, opcode, data2, (ulong)data.Length);
			}
		}
	}

	public unsafe bool ReplayPacket(FFXIVReplay.DataSegment* segment)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			return replayPacket.Invoke(contentsReplayModule, segment, segment->Data);
		}
	}

	public unsafe bool ReplayPacket(FFXIVReplay.DataSegment segment, byte[] data)
	{
		fixed (ContentsReplayModule* contentsReplayModule = &this)
		{
			fixed (byte* data2 = data)
			{
				return replayPacket.Invoke(contentsReplayModule, &segment, data2);
			}
		}
	}

	public bool Validate()
	{
		return true;
	}
}

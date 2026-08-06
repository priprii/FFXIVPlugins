using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Hypostasis.Dalamud;
using Lumina.Excel.Sheets;

namespace Hypostasis.Game.Structures;

public struct FFXIVReplay
{
	[StructLayout(LayoutKind.Explicit, Size = 104)]
	public struct Header
	{
		private static readonly byte[] validBytes = "FFXIVREPLAY"u8.ToArray();

		[FieldOffset(0)]
		public unsafe fixed byte FFXIVREPLAY[12];

		[FieldOffset(12)]
		public short replayFormatVersion;

		[FieldOffset(14)]
		public short operatingSystemType;

		[FieldOffset(16)]
		public int gameBuildNumber;

		[FieldOffset(20)]
		public uint timestamp;

		[FieldOffset(24)]
		public uint totalMS;

		[FieldOffset(28)]
		public uint displayedMS;

		[FieldOffset(32)]
		public ushort contentID;

		[FieldOffset(40)]
		public byte info;

		[FieldOffset(48)]
		public ulong localCID;

		[FieldOffset(56)]
		public unsafe fixed byte jobs[8];

		[FieldOffset(64)]
		public byte playerIndex;

		[FieldOffset(68)]
		public int u0x44;

		[FieldOffset(72)]
		public int replayLength;

		[FieldOffset(76)]
		public short u0x4C;

		[FieldOffset(78)]
		public unsafe fixed ushort npcNames[7];

		[FieldOffset(92)]
		public int u0x5C;

		[FieldOffset(96)]
		public long u0x60;

		public unsafe bool IsValid
		{
			get
			{
				for (int i = 0; i < validBytes.Length; i++)
				{
					if (validBytes[i] != FFXIVREPLAY[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		public unsafe bool IsPlayable
		{
			get
			{
				if (gameBuildNumber == Common.ContentsReplayModule->gameBuildNumber)
				{
					return IsCurrentFormatVersion;
				}
				return false;
			}
		}

		public bool IsCurrentFormatVersion => replayFormatVersion == 5;

		public bool IsLocked
		{
			get
			{
				if (IsValid && IsPlayable)
				{
					return (info & 2) != 0;
				}
				return false;
			}
		}

		public ContentFinderCondition ContentFinderCondition => DalamudApi.DataManager.GetExcelSheet<ContentFinderCondition>((ClientLanguage?)null, (string)null).GetRow((uint)contentID);

		public unsafe ClassJob LocalPlayerClassJob => DalamudApi.DataManager.GetExcelSheet<ClassJob>((ClientLanguage?)null, (string)null).GetRow((uint)jobs[playerIndex]);

		public IEnumerable<ClassJob> ClassJobs => from id in Enumerable.Range(0, 8).Select(GetJobSafe).TakeWhile((byte id) => id != 0)
			select DalamudApi.DataManager.GetExcelSheet<ClassJob>((ClientLanguage?)null, (string)null).GetRow((uint)id);

		private unsafe byte GetJobSafe(int i)
		{
			return jobs[i];
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 772)]
	public struct ChapterArray
	{
		[StructLayout(LayoutKind.Sequential, Size = 12)]
		public struct Chapter
		{
			public int type;

			public uint offset;

			public uint ms;
		}

		[FieldOffset(0)]
		public int length;

		public unsafe Chapter* this[int i]
		{
			get
			{
				if ((i < 0 || i > 63) ? true : false)
				{
					return null;
				}
				fixed (ChapterArray* ptr = &this)
				{
					void* ptr2 = ptr;
					return (Chapter*)((byte*)ptr2 + 4) + i;
				}
			}
		}
	}

	public struct DataSegment
	{
		public ushort opcode;

		public ushort dataLength;

		public uint ms;

		public uint objectID;

		public unsafe uint Length => (uint)(sizeof(DataSegment) + dataLength);

		public unsafe byte* Data
		{
			get
			{
				fixed (DataSegment* ptr = &this)
				{
					void* ptr2 = ptr;
					return (byte*)ptr2 + sizeof(DataSegment);
				}
			}
		}
	}

	public const short CurrentReplayFormatVersion = 5;

	public Header header;

	public ChapterArray chapters;

	public unsafe byte* Data
	{
		get
		{
			fixed (FFXIVReplay* ptr = &this)
			{
				void* ptr2 = ptr;
				return (byte*)ptr2 + sizeof(Header) + sizeof(ChapterArray);
			}
		}
	}

	public unsafe DataSegment* GetDataSegment(uint offset)
	{
		if (offset >= header.replayLength)
		{
			return null;
		}
		return (DataSegment*)(Data + offset);
	}
}

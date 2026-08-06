using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct UpdateMediaState : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public string PixId
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return null;
			}
			return __p.__string(num + __p.bb_pos);
		}
	}

	public MediaStateAction Action
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return MediaStateAction.Play;
			}
			return (MediaStateAction)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public bool IsPlaying
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return false;
			}
			return __p.bb.Get(num + __p.bb_pos) != 0;
		}
	}

	public long SeekTime
	{
		get
		{
			int num = __p.__offset(10);
			if (num == 0)
			{
				return 0L;
			}
			return __p.bb.GetLong(num + __p.bb_pos);
		}
	}

	public long Duration
	{
		get
		{
			int num = __p.__offset(12);
			if (num == 0)
			{
				return 0L;
			}
			return __p.bb.GetLong(num + __p.bb_pos);
		}
	}

	public long Timestamp
	{
		get
		{
			int num = __p.__offset(14);
			if (num == 0)
			{
				return 0L;
			}
			return __p.bb.GetLong(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static UpdateMediaState GetRootAsUpdateMediaState(ByteBuffer _bb)
	{
		return GetRootAsUpdateMediaState(_bb, default(UpdateMediaState));
	}

	public static UpdateMediaState GetRootAsUpdateMediaState(ByteBuffer _bb, UpdateMediaState obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public UpdateMediaState __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetPixIdBytes()
	{
		return __p.__vector_as_arraysegment(4);
	}

	public byte[] GetPixIdArray()
	{
		return __p.__vector_as_array<byte>(4);
	}

	public static Offset<UpdateMediaState> CreateUpdateMediaState(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), MediaStateAction action = MediaStateAction.Play, bool isPlaying = false, long seekTime = 0L, long duration = 0L, long timestamp = 0L)
	{
		builder.StartTable(6);
		AddTimestamp(builder, timestamp);
		AddDuration(builder, duration);
		AddSeekTime(builder, seekTime);
		AddPixId(builder, pixIdOffset);
		AddIsPlaying(builder, isPlaying);
		AddAction(builder, action);
		return EndUpdateMediaState(builder);
	}

	public static void StartUpdateMediaState(FlatBufferBuilder builder)
	{
		builder.StartTable(6);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddAction(FlatBufferBuilder builder, MediaStateAction action)
	{
		builder.AddSbyte(1, (sbyte)action, 0);
	}

	public static void AddIsPlaying(FlatBufferBuilder builder, bool isPlaying)
	{
		builder.AddBool(2, isPlaying, d: false);
	}

	public static void AddSeekTime(FlatBufferBuilder builder, long seekTime)
	{
		builder.AddLong(3, seekTime, 0L);
	}

	public static void AddDuration(FlatBufferBuilder builder, long duration)
	{
		builder.AddLong(4, duration, 0L);
	}

	public static void AddTimestamp(FlatBufferBuilder builder, long timestamp)
	{
		builder.AddLong(5, timestamp, 0L);
	}

	public static Offset<UpdateMediaState> EndUpdateMediaState(FlatBufferBuilder builder)
	{
		return new Offset<UpdateMediaState>(builder.EndTable());
	}
}

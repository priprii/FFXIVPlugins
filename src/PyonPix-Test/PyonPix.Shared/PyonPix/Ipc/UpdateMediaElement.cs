using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct UpdateMediaElement : IFlatbufferObject
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

	public MediaElementType Host
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return MediaElementType.Generic;
			}
			return (MediaElementType)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public string Source
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return null;
			}
			return __p.__string(num + __p.bb_pos);
		}
	}

	public MediaElementAction Action
	{
		get
		{
			int num = __p.__offset(10);
			if (num == 0)
			{
				return MediaElementAction.Play;
			}
			return (MediaElementAction)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public bool IsPlaying
	{
		get
		{
			int num = __p.__offset(12);
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
			int num = __p.__offset(14);
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
			int num = __p.__offset(16);
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
			int num = __p.__offset(18);
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

	public static UpdateMediaElement GetRootAsUpdateMediaElement(ByteBuffer _bb)
	{
		return GetRootAsUpdateMediaElement(_bb, default(UpdateMediaElement));
	}

	public static UpdateMediaElement GetRootAsUpdateMediaElement(ByteBuffer _bb, UpdateMediaElement obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public UpdateMediaElement __assign(int _i, ByteBuffer _bb)
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

	public ArraySegment<byte>? GetSourceBytes()
	{
		return __p.__vector_as_arraysegment(8);
	}

	public byte[] GetSourceArray()
	{
		return __p.__vector_as_array<byte>(8);
	}

	public static Offset<UpdateMediaElement> CreateUpdateMediaElement(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), MediaElementType type = MediaElementType.Generic, StringOffset sourceOffset = default(StringOffset), MediaElementAction action = MediaElementAction.Play, bool isPlaying = false, long seekTime = 0L, long duration = 0L, long timestamp = 0L)
	{
		builder.StartTable(8);
		AddTimestamp(builder, timestamp);
		AddDuration(builder, duration);
		AddSeekTime(builder, seekTime);
		AddSource(builder, sourceOffset);
		AddPixId(builder, pixIdOffset);
		AddIsPlaying(builder, isPlaying);
		AddAction(builder, action);
		AddType(builder, type);
		return EndUpdateMediaElement(builder);
	}

	public static void StartUpdateMediaElement(FlatBufferBuilder builder)
	{
		builder.StartTable(8);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddType(FlatBufferBuilder builder, MediaElementType type)
	{
		builder.AddSbyte(1, (sbyte)type, 0);
	}

	public static void AddSource(FlatBufferBuilder builder, StringOffset sourceOffset)
	{
		builder.AddOffset(2, sourceOffset.Value, 0);
	}

	public static void AddAction(FlatBufferBuilder builder, MediaElementAction action)
	{
		builder.AddSbyte(3, (sbyte)action, 0);
	}

	public static void AddIsPlaying(FlatBufferBuilder builder, bool isPlaying)
	{
		builder.AddBool(4, isPlaying, d: false);
	}

	public static void AddSeekTime(FlatBufferBuilder builder, long seekTime)
	{
		builder.AddLong(5, seekTime, 0L);
	}

	public static void AddDuration(FlatBufferBuilder builder, long duration)
	{
		builder.AddLong(6, duration, 0L);
	}

	public static void AddTimestamp(FlatBufferBuilder builder, long timestamp)
	{
		builder.AddLong(7, timestamp, 0L);
	}

	public static Offset<UpdateMediaElement> EndUpdateMediaElement(FlatBufferBuilder builder)
	{
		return new Offset<UpdateMediaElement>(builder.EndTable());
	}
}

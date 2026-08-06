using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct SendMouseEvent : IFlatbufferObject
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

	public uint Msg
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return 0u;
			}
			return __p.bb.GetUint(num + __p.bb_pos);
		}
	}

	public long WParam
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return 0L;
			}
			return __p.bb.GetLong(num + __p.bb_pos);
		}
	}

	public long LParam
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static SendMouseEvent GetRootAsSendMouseEvent(ByteBuffer _bb)
	{
		return GetRootAsSendMouseEvent(_bb, default(SendMouseEvent));
	}

	public static SendMouseEvent GetRootAsSendMouseEvent(ByteBuffer _bb, SendMouseEvent obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public SendMouseEvent __assign(int _i, ByteBuffer _bb)
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

	public static Offset<SendMouseEvent> CreateSendMouseEvent(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), uint msg = 0u, long wParam = 0L, long lParam = 0L)
	{
		builder.StartTable(4);
		AddLParam(builder, lParam);
		AddWParam(builder, wParam);
		AddMsg(builder, msg);
		AddPixId(builder, pixIdOffset);
		return EndSendMouseEvent(builder);
	}

	public static void StartSendMouseEvent(FlatBufferBuilder builder)
	{
		builder.StartTable(4);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddMsg(FlatBufferBuilder builder, uint msg)
	{
		builder.AddUint(1, msg, 0u);
	}

	public static void AddWParam(FlatBufferBuilder builder, long wParam)
	{
		builder.AddLong(2, wParam, 0L);
	}

	public static void AddLParam(FlatBufferBuilder builder, long lParam)
	{
		builder.AddLong(3, lParam, 0L);
	}

	public static Offset<SendMouseEvent> EndSendMouseEvent(FlatBufferBuilder builder)
	{
		return new Offset<SendMouseEvent>(builder.EndTable());
	}
}

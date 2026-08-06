using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct TabInitializeState : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public StateType Type
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return StateType.Success;
			}
			return (StateType)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public string PixId
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return null;
			}
			return __p.__string(num + __p.bb_pos);
		}
	}

	public string Message
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static TabInitializeState GetRootAsTabInitializeState(ByteBuffer _bb)
	{
		return GetRootAsTabInitializeState(_bb, default(TabInitializeState));
	}

	public static TabInitializeState GetRootAsTabInitializeState(ByteBuffer _bb, TabInitializeState obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public TabInitializeState __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetPixIdBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetPixIdArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public ArraySegment<byte>? GetMessageBytes()
	{
		return __p.__vector_as_arraysegment(8);
	}

	public byte[] GetMessageArray()
	{
		return __p.__vector_as_array<byte>(8);
	}

	public static Offset<TabInitializeState> CreateTabInitializeState(FlatBufferBuilder builder, StateType type = StateType.Success, StringOffset pixIdOffset = default(StringOffset), StringOffset messageOffset = default(StringOffset))
	{
		builder.StartTable(3);
		AddMessage(builder, messageOffset);
		AddPixId(builder, pixIdOffset);
		AddType(builder, type);
		return EndTabInitializeState(builder);
	}

	public static void StartTabInitializeState(FlatBufferBuilder builder)
	{
		builder.StartTable(3);
	}

	public static void AddType(FlatBufferBuilder builder, StateType type)
	{
		builder.AddSbyte(0, (sbyte)type, 0);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(1, pixIdOffset.Value, 0);
	}

	public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset)
	{
		builder.AddOffset(2, messageOffset.Value, 0);
	}

	public static Offset<TabInitializeState> EndTabInitializeState(FlatBufferBuilder builder)
	{
		return new Offset<TabInitializeState>(builder.EndTable());
	}
}

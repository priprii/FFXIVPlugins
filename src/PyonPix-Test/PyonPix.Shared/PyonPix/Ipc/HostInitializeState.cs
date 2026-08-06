using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct HostInitializeState : IFlatbufferObject
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

	public string Message
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static HostInitializeState GetRootAsHostInitializeState(ByteBuffer _bb)
	{
		return GetRootAsHostInitializeState(_bb, default(HostInitializeState));
	}

	public static HostInitializeState GetRootAsHostInitializeState(ByteBuffer _bb, HostInitializeState obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public HostInitializeState __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetMessageBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetMessageArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<HostInitializeState> CreateHostInitializeState(FlatBufferBuilder builder, StateType type = StateType.Success, StringOffset messageOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddMessage(builder, messageOffset);
		AddType(builder, type);
		return EndHostInitializeState(builder);
	}

	public static void StartHostInitializeState(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddType(FlatBufferBuilder builder, StateType type)
	{
		builder.AddSbyte(0, (sbyte)type, 0);
	}

	public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset)
	{
		builder.AddOffset(1, messageOffset.Value, 0);
	}

	public static Offset<HostInitializeState> EndHostInitializeState(FlatBufferBuilder builder)
	{
		return new Offset<HostInitializeState>(builder.EndTable());
	}
}

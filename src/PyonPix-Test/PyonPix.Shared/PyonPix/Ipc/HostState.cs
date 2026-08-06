using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct HostState : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public HostStateType Type
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return HostStateType.Success;
			}
			return (HostStateType)__p.bb.GetSbyte(num + __p.bb_pos);
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

	public static HostState GetRootAsHostState(ByteBuffer _bb)
	{
		return GetRootAsHostState(_bb, default(HostState));
	}

	public static HostState GetRootAsHostState(ByteBuffer _bb, HostState obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public HostState __assign(int _i, ByteBuffer _bb)
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

	public static Offset<HostState> CreateHostState(FlatBufferBuilder builder, HostStateType type = HostStateType.Success, StringOffset messageOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddMessage(builder, messageOffset);
		AddType(builder, type);
		return EndHostState(builder);
	}

	public static void StartHostState(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddType(FlatBufferBuilder builder, HostStateType type)
	{
		builder.AddSbyte(0, (sbyte)type, 0);
	}

	public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset)
	{
		builder.AddOffset(1, messageOffset.Value, 0);
	}

	public static Offset<HostState> EndHostState(FlatBufferBuilder builder)
	{
		return new Offset<HostState>(builder.EndTable());
	}
}

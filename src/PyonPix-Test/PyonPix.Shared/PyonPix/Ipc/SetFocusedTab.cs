using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct SetFocusedTab : IFlatbufferObject
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

	public bool ByUserInput
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return false;
			}
			return __p.bb.Get(num + __p.bb_pos) != 0;
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static SetFocusedTab GetRootAsSetFocusedTab(ByteBuffer _bb)
	{
		return GetRootAsSetFocusedTab(_bb, default(SetFocusedTab));
	}

	public static SetFocusedTab GetRootAsSetFocusedTab(ByteBuffer _bb, SetFocusedTab obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public SetFocusedTab __assign(int _i, ByteBuffer _bb)
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

	public static Offset<SetFocusedTab> CreateSetFocusedTab(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), bool byUserInput = false)
	{
		builder.StartTable(2);
		AddPixId(builder, pixIdOffset);
		AddByUserInput(builder, byUserInput);
		return EndSetFocusedTab(builder);
	}

	public static void StartSetFocusedTab(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddByUserInput(FlatBufferBuilder builder, bool byUserInput)
	{
		builder.AddBool(1, byUserInput, d: false);
	}

	public static Offset<SetFocusedTab> EndSetFocusedTab(FlatBufferBuilder builder)
	{
		return new Offset<SetFocusedTab>(builder.EndTable());
	}
}

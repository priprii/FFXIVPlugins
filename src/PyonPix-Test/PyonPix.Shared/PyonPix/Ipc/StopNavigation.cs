using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct StopNavigation : IFlatbufferObject
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static StopNavigation GetRootAsStopNavigation(ByteBuffer _bb)
	{
		return GetRootAsStopNavigation(_bb, default(StopNavigation));
	}

	public static StopNavigation GetRootAsStopNavigation(ByteBuffer _bb, StopNavigation obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public StopNavigation __assign(int _i, ByteBuffer _bb)
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

	public static Offset<StopNavigation> CreateStopNavigation(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset))
	{
		builder.StartTable(1);
		AddPixId(builder, pixIdOffset);
		return EndStopNavigation(builder);
	}

	public static void StartStopNavigation(FlatBufferBuilder builder)
	{
		builder.StartTable(1);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static Offset<StopNavigation> EndStopNavigation(FlatBufferBuilder builder)
	{
		return new Offset<StopNavigation>(builder.EndTable());
	}
}

using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct Navigate : IFlatbufferObject
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

	public string Uri
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

	public static Navigate GetRootAsNavigate(ByteBuffer _bb)
	{
		return GetRootAsNavigate(_bb, default(Navigate));
	}

	public static Navigate GetRootAsNavigate(ByteBuffer _bb, Navigate obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public Navigate __assign(int _i, ByteBuffer _bb)
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

	public ArraySegment<byte>? GetUriBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetUriArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<Navigate> CreateNavigate(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), StringOffset uriOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddUri(builder, uriOffset);
		AddPixId(builder, pixIdOffset);
		return EndNavigate(builder);
	}

	public static void StartNavigate(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddUri(FlatBufferBuilder builder, StringOffset uriOffset)
	{
		builder.AddOffset(1, uriOffset.Value, 0);
	}

	public static Offset<Navigate> EndNavigate(FlatBufferBuilder builder)
	{
		return new Offset<Navigate>(builder.EndTable());
	}
}

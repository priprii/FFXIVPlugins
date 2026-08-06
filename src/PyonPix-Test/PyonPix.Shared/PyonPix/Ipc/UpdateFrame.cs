using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct UpdateFrame : IFlatbufferObject
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

	public long SharedTexture
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return 0L;
			}
			return __p.bb.GetLong(num + __p.bb_pos);
		}
	}

	public uint W
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return 0u;
			}
			return __p.bb.GetUint(num + __p.bb_pos);
		}
	}

	public uint H
	{
		get
		{
			int num = __p.__offset(10);
			if (num == 0)
			{
				return 0u;
			}
			return __p.bb.GetUint(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static UpdateFrame GetRootAsUpdateFrame(ByteBuffer _bb)
	{
		return GetRootAsUpdateFrame(_bb, default(UpdateFrame));
	}

	public static UpdateFrame GetRootAsUpdateFrame(ByteBuffer _bb, UpdateFrame obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public UpdateFrame __assign(int _i, ByteBuffer _bb)
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

	public static Offset<UpdateFrame> CreateUpdateFrame(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), long sharedTexture = 0L, uint w = 0u, uint h = 0u)
	{
		builder.StartTable(4);
		AddSharedTexture(builder, sharedTexture);
		AddH(builder, h);
		AddW(builder, w);
		AddPixId(builder, pixIdOffset);
		return EndUpdateFrame(builder);
	}

	public static void StartUpdateFrame(FlatBufferBuilder builder)
	{
		builder.StartTable(4);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddSharedTexture(FlatBufferBuilder builder, long sharedTexture)
	{
		builder.AddLong(1, sharedTexture, 0L);
	}

	public static void AddW(FlatBufferBuilder builder, uint w)
	{
		builder.AddUint(2, w, 0u);
	}

	public static void AddH(FlatBufferBuilder builder, uint h)
	{
		builder.AddUint(3, h, 0u);
	}

	public static Offset<UpdateFrame> EndUpdateFrame(FlatBufferBuilder builder)
	{
		return new Offset<UpdateFrame>(builder.EndTable());
	}
}

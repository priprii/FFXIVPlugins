using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct Resize : IFlatbufferObject
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

	public int X
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.GetInt(num + __p.bb_pos);
		}
	}

	public int Y
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.GetInt(num + __p.bb_pos);
		}
	}

	public uint W
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

	public uint H
	{
		get
		{
			int num = __p.__offset(12);
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

	public static Resize GetRootAsResize(ByteBuffer _bb)
	{
		return GetRootAsResize(_bb, default(Resize));
	}

	public static Resize GetRootAsResize(ByteBuffer _bb, Resize obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public Resize __assign(int _i, ByteBuffer _bb)
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

	public static Offset<Resize> CreateResize(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), int x = 0, int y = 0, uint w = 0u, uint h = 0u)
	{
		builder.StartTable(5);
		AddH(builder, h);
		AddW(builder, w);
		AddY(builder, y);
		AddX(builder, x);
		AddPixId(builder, pixIdOffset);
		return EndResize(builder);
	}

	public static void StartResize(FlatBufferBuilder builder)
	{
		builder.StartTable(5);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddX(FlatBufferBuilder builder, int x)
	{
		builder.AddInt(1, x, 0);
	}

	public static void AddY(FlatBufferBuilder builder, int y)
	{
		builder.AddInt(2, y, 0);
	}

	public static void AddW(FlatBufferBuilder builder, uint w)
	{
		builder.AddUint(3, w, 0u);
	}

	public static void AddH(FlatBufferBuilder builder, uint h)
	{
		builder.AddUint(4, h, 0u);
	}

	public static Offset<Resize> EndResize(FlatBufferBuilder builder)
	{
		return new Offset<Resize>(builder.EndTable());
	}
}

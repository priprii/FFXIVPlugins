using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct NavigationCompleted : IFlatbufferObject
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

	public uint StatusCode
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static NavigationCompleted GetRootAsNavigationCompleted(ByteBuffer _bb)
	{
		return GetRootAsNavigationCompleted(_bb, default(NavigationCompleted));
	}

	public static NavigationCompleted GetRootAsNavigationCompleted(ByteBuffer _bb, NavigationCompleted obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public NavigationCompleted __assign(int _i, ByteBuffer _bb)
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

	public static Offset<NavigationCompleted> CreateNavigationCompleted(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), uint statusCode = 0u)
	{
		builder.StartTable(2);
		AddStatusCode(builder, statusCode);
		AddPixId(builder, pixIdOffset);
		return EndNavigationCompleted(builder);
	}

	public static void StartNavigationCompleted(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddStatusCode(FlatBufferBuilder builder, uint statusCode)
	{
		builder.AddUint(1, statusCode, 0u);
	}

	public static Offset<NavigationCompleted> EndNavigationCompleted(FlatBufferBuilder builder)
	{
		return new Offset<NavigationCompleted>(builder.EndTable());
	}
}

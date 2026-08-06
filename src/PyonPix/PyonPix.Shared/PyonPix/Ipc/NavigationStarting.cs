using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct NavigationStarting : IFlatbufferObject
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

	public bool UserInitiated
	{
		get
		{
			int num = __p.__offset(8);
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

	public static NavigationStarting GetRootAsNavigationStarting(ByteBuffer _bb)
	{
		return GetRootAsNavigationStarting(_bb, default(NavigationStarting));
	}

	public static NavigationStarting GetRootAsNavigationStarting(ByteBuffer _bb, NavigationStarting obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public NavigationStarting __assign(int _i, ByteBuffer _bb)
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

	public static Offset<NavigationStarting> CreateNavigationStarting(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), StringOffset uriOffset = default(StringOffset), bool userInitiated = false)
	{
		builder.StartTable(3);
		AddUri(builder, uriOffset);
		AddPixId(builder, pixIdOffset);
		AddUserInitiated(builder, userInitiated);
		return EndNavigationStarting(builder);
	}

	public static void StartNavigationStarting(FlatBufferBuilder builder)
	{
		builder.StartTable(3);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddUri(FlatBufferBuilder builder, StringOffset uriOffset)
	{
		builder.AddOffset(1, uriOffset.Value, 0);
	}

	public static void AddUserInitiated(FlatBufferBuilder builder, bool userInitiated)
	{
		builder.AddBool(2, userInitiated, d: false);
	}

	public static Offset<NavigationStarting> EndNavigationStarting(FlatBufferBuilder builder)
	{
		return new Offset<NavigationStarting>(builder.EndTable());
	}
}

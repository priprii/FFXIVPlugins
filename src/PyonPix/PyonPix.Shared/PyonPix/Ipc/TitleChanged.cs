using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct TitleChanged : IFlatbufferObject
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

	public string Title
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

	public static TitleChanged GetRootAsTitleChanged(ByteBuffer _bb)
	{
		return GetRootAsTitleChanged(_bb, default(TitleChanged));
	}

	public static TitleChanged GetRootAsTitleChanged(ByteBuffer _bb, TitleChanged obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public TitleChanged __assign(int _i, ByteBuffer _bb)
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

	public ArraySegment<byte>? GetTitleBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetTitleArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<TitleChanged> CreateTitleChanged(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), StringOffset titleOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddTitle(builder, titleOffset);
		AddPixId(builder, pixIdOffset);
		return EndTitleChanged(builder);
	}

	public static void StartTitleChanged(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddTitle(FlatBufferBuilder builder, StringOffset titleOffset)
	{
		builder.AddOffset(1, titleOffset.Value, 0);
	}

	public static Offset<TitleChanged> EndTitleChanged(FlatBufferBuilder builder)
	{
		return new Offset<TitleChanged>(builder.EndTable());
	}
}

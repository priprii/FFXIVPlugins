using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct FavIconChanged : IFlatbufferObject
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

	public int DataLength
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return 0;
			}
			return __p.__vector_len(num);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static FavIconChanged GetRootAsFavIconChanged(ByteBuffer _bb)
	{
		return GetRootAsFavIconChanged(_bb, default(FavIconChanged));
	}

	public static FavIconChanged GetRootAsFavIconChanged(ByteBuffer _bb, FavIconChanged obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public FavIconChanged __assign(int _i, ByteBuffer _bb)
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

	public byte Data(int j)
	{
		int num = __p.__offset(6);
		if (num == 0)
		{
			return 0;
		}
		return __p.bb.Get(__p.__vector(num) + j);
	}

	public ArraySegment<byte>? GetDataBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetDataArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<FavIconChanged> CreateFavIconChanged(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), VectorOffset dataOffset = default(VectorOffset))
	{
		builder.StartTable(2);
		AddData(builder, dataOffset);
		AddPixId(builder, pixIdOffset);
		return EndFavIconChanged(builder);
	}

	public static void StartFavIconChanged(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddData(FlatBufferBuilder builder, VectorOffset dataOffset)
	{
		builder.AddOffset(1, dataOffset.Value, 0);
	}

	public static VectorOffset CreateDataVector(FlatBufferBuilder builder, byte[] data)
	{
		builder.StartVector(1, data.Length, 1);
		for (int num = data.Length - 1; num >= 0; num--)
		{
			builder.AddByte(data[num]);
		}
		return builder.EndVector();
	}

	public static VectorOffset CreateDataVectorBlock(FlatBufferBuilder builder, byte[] data)
	{
		builder.StartVector(1, data.Length, 1);
		builder.Add(data);
		return builder.EndVector();
	}

	public static VectorOffset CreateDataVectorBlock(FlatBufferBuilder builder, ArraySegment<byte> data)
	{
		builder.StartVector(1, data.Count, 1);
		builder.Add(data);
		return builder.EndVector();
	}

	public static VectorOffset CreateDataVectorBlock(FlatBufferBuilder builder, nint dataPtr, int sizeInBytes)
	{
		builder.StartVector(1, sizeInBytes, 1);
		builder.Add<byte>(dataPtr, sizeInBytes);
		return builder.EndVector();
	}

	public static void StartDataVector(FlatBufferBuilder builder, int numElems)
	{
		builder.StartVector(1, numElems, 1);
	}

	public static Offset<FavIconChanged> EndFavIconChanged(FlatBufferBuilder builder)
	{
		return new Offset<FavIconChanged>(builder.EndTable());
	}
}

using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct UpdateSpatialAudio : IFlatbufferObject
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

	public float Left
	{
		get
		{
			int num = __p.__offset(6);
			if (num == 0)
			{
				return 0f;
			}
			return __p.bb.GetFloat(num + __p.bb_pos);
		}
	}

	public float Right
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return 0f;
			}
			return __p.bb.GetFloat(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static UpdateSpatialAudio GetRootAsUpdateSpatialAudio(ByteBuffer _bb)
	{
		return GetRootAsUpdateSpatialAudio(_bb, default(UpdateSpatialAudio));
	}

	public static UpdateSpatialAudio GetRootAsUpdateSpatialAudio(ByteBuffer _bb, UpdateSpatialAudio obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public UpdateSpatialAudio __assign(int _i, ByteBuffer _bb)
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

	public static Offset<UpdateSpatialAudio> CreateUpdateSpatialAudio(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), float left = 0f, float right = 0f)
	{
		builder.StartTable(3);
		AddRight(builder, right);
		AddLeft(builder, left);
		AddPixId(builder, pixIdOffset);
		return EndUpdateSpatialAudio(builder);
	}

	public static void StartUpdateSpatialAudio(FlatBufferBuilder builder)
	{
		builder.StartTable(3);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddLeft(FlatBufferBuilder builder, float left)
	{
		builder.AddFloat(1, left, 0.0);
	}

	public static void AddRight(FlatBufferBuilder builder, float right)
	{
		builder.AddFloat(2, right, 0.0);
	}

	public static Offset<UpdateSpatialAudio> EndUpdateSpatialAudio(FlatBufferBuilder builder)
	{
		return new Offset<UpdateSpatialAudio>(builder.EndTable());
	}
}

using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct CreateTab : IFlatbufferObject
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

	public bool GpuAcceleration
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

	public int X
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

	public int Y
	{
		get
		{
			int num = __p.__offset(10);
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
			int num = __p.__offset(12);
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
			int num = __p.__offset(14);
			if (num == 0)
			{
				return 0u;
			}
			return __p.bb.GetUint(num + __p.bb_pos);
		}
	}

	public bool SyncCookies
	{
		get
		{
			int num = __p.__offset(16);
			if (num == 0)
			{
				return false;
			}
			return __p.bb.Get(num + __p.bb_pos) != 0;
		}
	}

	public int ExtensionsLength
	{
		get
		{
			int num = __p.__offset(18);
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

	public static CreateTab GetRootAsCreateTab(ByteBuffer _bb)
	{
		return GetRootAsCreateTab(_bb, default(CreateTab));
	}

	public static CreateTab GetRootAsCreateTab(ByteBuffer _bb, CreateTab obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public CreateTab __assign(int _i, ByteBuffer _bb)
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

	public string Extensions(int j)
	{
		int num = __p.__offset(18);
		if (num == 0)
		{
			return null;
		}
		return __p.__string(__p.__vector(num) + j * 4);
	}

	public static Offset<CreateTab> CreateCreateTab(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), bool gpuAcceleration = false, int x = 0, int y = 0, uint w = 0u, uint h = 0u, bool syncCookies = false, VectorOffset extensionsOffset = default(VectorOffset))
	{
		builder.StartTable(8);
		AddExtensions(builder, extensionsOffset);
		AddH(builder, h);
		AddW(builder, w);
		AddY(builder, y);
		AddX(builder, x);
		AddPixId(builder, pixIdOffset);
		AddSyncCookies(builder, syncCookies);
		AddGpuAcceleration(builder, gpuAcceleration);
		return EndCreateTab(builder);
	}

	public static void StartCreateTab(FlatBufferBuilder builder)
	{
		builder.StartTable(8);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddGpuAcceleration(FlatBufferBuilder builder, bool gpuAcceleration)
	{
		builder.AddBool(1, gpuAcceleration, d: false);
	}

	public static void AddX(FlatBufferBuilder builder, int x)
	{
		builder.AddInt(2, x, 0);
	}

	public static void AddY(FlatBufferBuilder builder, int y)
	{
		builder.AddInt(3, y, 0);
	}

	public static void AddW(FlatBufferBuilder builder, uint w)
	{
		builder.AddUint(4, w, 0u);
	}

	public static void AddH(FlatBufferBuilder builder, uint h)
	{
		builder.AddUint(5, h, 0u);
	}

	public static void AddSyncCookies(FlatBufferBuilder builder, bool syncCookies)
	{
		builder.AddBool(6, syncCookies, d: false);
	}

	public static void AddExtensions(FlatBufferBuilder builder, VectorOffset extensionsOffset)
	{
		builder.AddOffset(7, extensionsOffset.Value, 0);
	}

	public static VectorOffset CreateExtensionsVector(FlatBufferBuilder builder, StringOffset[] data)
	{
		builder.StartVector(4, data.Length, 4);
		for (int num = data.Length - 1; num >= 0; num--)
		{
			builder.AddOffset(data[num].Value);
		}
		return builder.EndVector();
	}

	public static VectorOffset CreateExtensionsVectorBlock(FlatBufferBuilder builder, StringOffset[] data)
	{
		builder.StartVector(4, data.Length, 4);
		builder.Add(data);
		return builder.EndVector();
	}

	public static VectorOffset CreateExtensionsVectorBlock(FlatBufferBuilder builder, ArraySegment<StringOffset> data)
	{
		builder.StartVector(4, data.Count, 4);
		builder.Add(data);
		return builder.EndVector();
	}

	public static VectorOffset CreateExtensionsVectorBlock(FlatBufferBuilder builder, nint dataPtr, int sizeInBytes)
	{
		builder.StartVector(1, sizeInBytes, 1);
		builder.Add<StringOffset>(dataPtr, sizeInBytes);
		return builder.EndVector();
	}

	public static void StartExtensionsVector(FlatBufferBuilder builder, int numElems)
	{
		builder.StartVector(4, numElems, 4);
	}

	public static Offset<CreateTab> EndCreateTab(FlatBufferBuilder builder)
	{
		return new Offset<CreateTab>(builder.EndTable());
	}
}

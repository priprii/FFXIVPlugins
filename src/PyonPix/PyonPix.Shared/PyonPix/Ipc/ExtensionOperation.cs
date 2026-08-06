using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct ExtensionOperation : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public ExtensionOp ExtensionOp
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return ExtensionOp.Install;
			}
			return (ExtensionOp)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public string ExtensionId
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

	public static ExtensionOperation GetRootAsExtensionOperation(ByteBuffer _bb)
	{
		return GetRootAsExtensionOperation(_bb, default(ExtensionOperation));
	}

	public static ExtensionOperation GetRootAsExtensionOperation(ByteBuffer _bb, ExtensionOperation obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public ExtensionOperation __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetExtensionIdBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetExtensionIdArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<ExtensionOperation> CreateExtensionOperation(FlatBufferBuilder builder, ExtensionOp extensionOp = ExtensionOp.Install, StringOffset extensionIdOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddExtensionId(builder, extensionIdOffset);
		AddExtensionOp(builder, extensionOp);
		return EndExtensionOperation(builder);
	}

	public static void StartExtensionOperation(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddExtensionOp(FlatBufferBuilder builder, ExtensionOp extensionOp)
	{
		builder.AddSbyte(0, (sbyte)extensionOp, 0);
	}

	public static void AddExtensionId(FlatBufferBuilder builder, StringOffset extensionIdOffset)
	{
		builder.AddOffset(1, extensionIdOffset.Value, 0);
	}

	public static Offset<ExtensionOperation> EndExtensionOperation(FlatBufferBuilder builder)
	{
		return new Offset<ExtensionOperation>(builder.EndTable());
	}
}

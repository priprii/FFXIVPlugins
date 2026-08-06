using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct UninstallExtension : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public string ExtensionId
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

	public string ExtensionName
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

	public static UninstallExtension GetRootAsUninstallExtension(ByteBuffer _bb)
	{
		return GetRootAsUninstallExtension(_bb, default(UninstallExtension));
	}

	public static UninstallExtension GetRootAsUninstallExtension(ByteBuffer _bb, UninstallExtension obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public UninstallExtension __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetExtensionIdBytes()
	{
		return __p.__vector_as_arraysegment(4);
	}

	public byte[] GetExtensionIdArray()
	{
		return __p.__vector_as_array<byte>(4);
	}

	public ArraySegment<byte>? GetExtensionNameBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetExtensionNameArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<UninstallExtension> CreateUninstallExtension(FlatBufferBuilder builder, StringOffset extensionIdOffset = default(StringOffset), StringOffset extensionNameOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddExtensionName(builder, extensionNameOffset);
		AddExtensionId(builder, extensionIdOffset);
		return EndUninstallExtension(builder);
	}

	public static void StartUninstallExtension(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddExtensionId(FlatBufferBuilder builder, StringOffset extensionIdOffset)
	{
		builder.AddOffset(0, extensionIdOffset.Value, 0);
	}

	public static void AddExtensionName(FlatBufferBuilder builder, StringOffset extensionNameOffset)
	{
		builder.AddOffset(1, extensionNameOffset.Value, 0);
	}

	public static Offset<UninstallExtension> EndUninstallExtension(FlatBufferBuilder builder)
	{
		return new Offset<UninstallExtension>(builder.EndTable());
	}
}

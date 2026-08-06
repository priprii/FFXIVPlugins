using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct WebMessageReceived : IFlatbufferObject
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

	public string Json
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

	public static WebMessageReceived GetRootAsWebMessageReceived(ByteBuffer _bb)
	{
		return GetRootAsWebMessageReceived(_bb, default(WebMessageReceived));
	}

	public static WebMessageReceived GetRootAsWebMessageReceived(ByteBuffer _bb, WebMessageReceived obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public WebMessageReceived __assign(int _i, ByteBuffer _bb)
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

	public ArraySegment<byte>? GetJsonBytes()
	{
		return __p.__vector_as_arraysegment(6);
	}

	public byte[] GetJsonArray()
	{
		return __p.__vector_as_array<byte>(6);
	}

	public static Offset<WebMessageReceived> CreateWebMessageReceived(FlatBufferBuilder builder, StringOffset pixIdOffset = default(StringOffset), StringOffset jsonOffset = default(StringOffset))
	{
		builder.StartTable(2);
		AddJson(builder, jsonOffset);
		AddPixId(builder, pixIdOffset);
		return EndWebMessageReceived(builder);
	}

	public static void StartWebMessageReceived(FlatBufferBuilder builder)
	{
		builder.StartTable(2);
	}

	public static void AddPixId(FlatBufferBuilder builder, StringOffset pixIdOffset)
	{
		builder.AddOffset(0, pixIdOffset.Value, 0);
	}

	public static void AddJson(FlatBufferBuilder builder, StringOffset jsonOffset)
	{
		builder.AddOffset(1, jsonOffset.Value, 0);
	}

	public static Offset<WebMessageReceived> EndWebMessageReceived(FlatBufferBuilder builder)
	{
		return new Offset<WebMessageReceived>(builder.EndTable());
	}
}

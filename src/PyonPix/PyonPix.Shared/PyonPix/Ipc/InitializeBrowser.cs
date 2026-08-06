using System;
using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct InitializeBrowser : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public string PluginPath
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

	public uint GamePid
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

	public uint LuidLowPart
	{
		get
		{
			int num = __p.__offset(8);
			if (num == 0)
			{
				return 0u;
			}
			return __p.bb.GetUint(num + __p.bb_pos);
		}
	}

	public int LuidHighPart
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

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static InitializeBrowser GetRootAsInitializeBrowser(ByteBuffer _bb)
	{
		return GetRootAsInitializeBrowser(_bb, default(InitializeBrowser));
	}

	public static InitializeBrowser GetRootAsInitializeBrowser(ByteBuffer _bb, InitializeBrowser obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public InitializeBrowser __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public ArraySegment<byte>? GetPluginPathBytes()
	{
		return __p.__vector_as_arraysegment(4);
	}

	public byte[] GetPluginPathArray()
	{
		return __p.__vector_as_array<byte>(4);
	}

	public static Offset<InitializeBrowser> CreateInitializeBrowser(FlatBufferBuilder builder, StringOffset pluginPathOffset = default(StringOffset), uint gamePid = 0u, uint LuidLowPart = 0u, int LuidHighPart = 0)
	{
		builder.StartTable(4);
		AddLuidHighPart(builder, LuidHighPart);
		AddLuidLowPart(builder, LuidLowPart);
		AddGamePid(builder, gamePid);
		AddPluginPath(builder, pluginPathOffset);
		return EndInitializeBrowser(builder);
	}

	public static void StartInitializeBrowser(FlatBufferBuilder builder)
	{
		builder.StartTable(4);
	}

	public static void AddPluginPath(FlatBufferBuilder builder, StringOffset pluginPathOffset)
	{
		builder.AddOffset(0, pluginPathOffset.Value, 0);
	}

	public static void AddGamePid(FlatBufferBuilder builder, uint gamePid)
	{
		builder.AddUint(1, gamePid, 0u);
	}

	public static void AddLuidLowPart(FlatBufferBuilder builder, uint luidLowPart)
	{
		builder.AddUint(2, luidLowPart, 0u);
	}

	public static void AddLuidHighPart(FlatBufferBuilder builder, int luidHighPart)
	{
		builder.AddInt(3, luidHighPart, 0);
	}

	public static Offset<InitializeBrowser> EndInitializeBrowser(FlatBufferBuilder builder)
	{
		return new Offset<InitializeBrowser>(builder.EndTable());
	}
}

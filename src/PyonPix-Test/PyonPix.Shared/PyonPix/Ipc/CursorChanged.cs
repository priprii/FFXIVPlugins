using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct CursorChanged : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public uint CursorId
	{
		get
		{
			int num = __p.__offset(4);
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

	public static CursorChanged GetRootAsCursorChanged(ByteBuffer _bb)
	{
		return GetRootAsCursorChanged(_bb, default(CursorChanged));
	}

	public static CursorChanged GetRootAsCursorChanged(ByteBuffer _bb, CursorChanged obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public CursorChanged __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public static Offset<CursorChanged> CreateCursorChanged(FlatBufferBuilder builder, uint cursorId = 0u)
	{
		builder.StartTable(1);
		AddCursorId(builder, cursorId);
		return EndCursorChanged(builder);
	}

	public static void StartCursorChanged(FlatBufferBuilder builder)
	{
		builder.StartTable(1);
	}

	public static void AddCursorId(FlatBufferBuilder builder, uint cursorId)
	{
		builder.AddUint(0, cursorId, 0u);
	}

	public static Offset<CursorChanged> EndCursorChanged(FlatBufferBuilder builder)
	{
		return new Offset<CursorChanged>(builder.EndTable());
	}
}

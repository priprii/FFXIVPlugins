using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct ToggleAutoTheatreMode : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public bool State
	{
		get
		{
			int num = __p.__offset(4);
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

	public static ToggleAutoTheatreMode GetRootAsToggleAutoTheatreMode(ByteBuffer _bb)
	{
		return GetRootAsToggleAutoTheatreMode(_bb, default(ToggleAutoTheatreMode));
	}

	public static ToggleAutoTheatreMode GetRootAsToggleAutoTheatreMode(ByteBuffer _bb, ToggleAutoTheatreMode obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public ToggleAutoTheatreMode __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public static Offset<ToggleAutoTheatreMode> CreateToggleAutoTheatreMode(FlatBufferBuilder builder, bool state = false)
	{
		builder.StartTable(1);
		AddState(builder, state);
		return EndToggleAutoTheatreMode(builder);
	}

	public static void StartToggleAutoTheatreMode(FlatBufferBuilder builder)
	{
		builder.StartTable(1);
	}

	public static void AddState(FlatBufferBuilder builder, bool state)
	{
		builder.AddBool(0, state, d: false);
	}

	public static Offset<ToggleAutoTheatreMode> EndToggleAutoTheatreMode(FlatBufferBuilder builder)
	{
		return new Offset<ToggleAutoTheatreMode>(builder.EndTable());
	}
}

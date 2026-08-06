using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct Command : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public CommandType Type
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return CommandType.MediatorInitializeRequest;
			}
			return (CommandType)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static Command GetRootAsCommand(ByteBuffer _bb)
	{
		return GetRootAsCommand(_bb, default(Command));
	}

	public static Command GetRootAsCommand(ByteBuffer _bb, Command obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public Command __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public static Offset<Command> CreateCommand(FlatBufferBuilder builder, CommandType type = CommandType.MediatorInitializeRequest)
	{
		builder.StartTable(1);
		AddType(builder, type);
		return EndCommand(builder);
	}

	public static void StartCommand(FlatBufferBuilder builder)
	{
		builder.StartTable(1);
	}

	public static void AddType(FlatBufferBuilder builder, CommandType type)
	{
		builder.AddSbyte(0, (sbyte)type, 0);
	}

	public static Offset<Command> EndCommand(FlatBufferBuilder builder)
	{
		return new Offset<Command>(builder.EndTable());
	}
}

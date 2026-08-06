using Google.FlatBuffers;

namespace PyonPix.Ipc;

public struct Message : IFlatbufferObject
{
	private Table __p;

	public ByteBuffer ByteBuffer => __p.bb;

	public MessageType Type
	{
		get
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return MessageType.None;
			}
			return (MessageType)__p.bb.GetSbyte(num + __p.bb_pos);
		}
	}

	public static void ValidateVersion()
	{
		FlatBufferConstants.FLATBUFFERS_25_2_10();
	}

	public static Message GetRootAsMessage(ByteBuffer _bb)
	{
		return GetRootAsMessage(_bb, default(Message));
	}

	public static Message GetRootAsMessage(ByteBuffer _bb, Message obj)
	{
		return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
	}

	public void __init(int _i, ByteBuffer _bb)
	{
		__p = new Table(_i, _bb);
	}

	public Message __assign(int _i, ByteBuffer _bb)
	{
		__init(_i, _bb);
		return this;
	}

	public static Offset<Message> CreateMessage(FlatBufferBuilder builder, MessageType type = MessageType.None)
	{
		builder.StartTable(1);
		AddType(builder, type);
		return EndMessage(builder);
	}

	public static void StartMessage(FlatBufferBuilder builder)
	{
		builder.StartTable(1);
	}

	public static void AddType(FlatBufferBuilder builder, MessageType type)
	{
		builder.AddSbyte(0, (sbyte)type, 0);
	}

	public static Offset<Message> EndMessage(FlatBufferBuilder builder)
	{
		return new Offset<Message>(builder.EndTable());
	}
}

using System;
using System.IO;
using System.Text.Json;

namespace Ktisis.Data.Json;

public ref struct BlockBufferJsonReader(Stream stream, Span<byte> blockBuffer, JsonReaderOptions options)
{
	private enum State
	{
		INIT,
		READING,
		FINAL_READ,
		CLOSED
	}

	public Utf8JsonReader Reader = default(Utf8JsonReader);

	private readonly Stream stream = stream;

	private readonly Span<byte> blockBuffer = blockBuffer;

	private Span<byte> readSlice = default(Span<byte>);

	private JsonReaderState jsonState = new JsonReaderState(options);

	private State state = State.INIT;

	public bool Read()
	{
		switch (state)
		{
		case State.CLOSED:
			return false;
		case State.READING:
		case State.FINAL_READ:
			if (Reader.Read())
			{
				return true;
			}
			if (state == State.FINAL_READ)
			{
				state = State.CLOSED;
				return false;
			}
			goto case State.INIT;
		case State.INIT:
			acquireReader();
			goto case State.READING;
		default:
			throw new Exception("This point is unreachable");
		}
	}

	private void acquireReader()
	{
		int num = 0;
		if (state != State.INIT)
		{
			if (Reader.BytesConsumed == 0L)
			{
				throw new Exception("JSON value appears to exceed the bounds of the block buffer. Increase the buffer size or decrease your JSON value size.");
			}
			jsonState = Reader.CurrentState;
			Span<byte> span = readSlice.Slice((int)Reader.BytesConsumed);
			span.CopyTo(blockBuffer);
			num = span.Length;
		}
		Stream obj = stream;
		Span<byte> span2 = blockBuffer;
		int num2 = obj.Read(span2.Slice(num));
		span2 = blockBuffer;
		readSlice = span2.Slice(0, num + num2);
		state = ((readSlice.Length != 0) ? State.READING : State.FINAL_READ);
		Reader = new Utf8JsonReader(readSlice, readSlice.Length == 0, jsonState);
	}
}

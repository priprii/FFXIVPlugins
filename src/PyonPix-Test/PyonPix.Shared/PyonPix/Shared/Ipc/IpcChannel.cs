using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace PyonPix.Shared.Ipc;

public sealed class IpcChannel : IDisposable
{
	private const int Magic = 827871305;

	private const int HeaderSize = 32;

	private const int OffsetMagic = 0;

	private const int OffsetSlotCount = 4;

	private const int OffsetSlotPayloadSize = 8;

	private const int OffsetWriteSeq = 12;

	private const int OffsetReadSeq = 20;

	private const int SlotSeqOffset = 0;

	private const int SlotLengthOffset = 8;

	private const int SlotHeaderSize = 12;

	private readonly MemoryMappedFile _mmf;

	private readonly MemoryMappedViewAccessor _view;

	private readonly object _writeLock = new object();

	public int SlotCount { get; }

	public int SlotPayloadSize { get; }

	private int SlotSize => 12 + SlotPayloadSize;

	public int Capacity => 32 + SlotCount * SlotSize;

	public IpcChannel(string name, int slotCount = 64, int slotPayloadSize = 65536)
	{
		if (slotCount <= 0)
		{
			throw new ArgumentOutOfRangeException("slotCount");
		}
		if (slotPayloadSize <= 0)
		{
			throw new ArgumentOutOfRangeException("slotPayloadSize");
		}
		SlotCount = slotCount;
		SlotPayloadSize = slotPayloadSize;
		_mmf = MemoryMappedFile.CreateOrOpen(name, Capacity, MemoryMappedFileAccess.ReadWrite);
		_view = _mmf.CreateViewAccessor(0L, Capacity, MemoryMappedFileAccess.ReadWrite);
		InitializeHeader();
	}

	private void InitializeHeader()
	{
		int num = _view.ReadInt32(0L);
		int num2 = _view.ReadInt32(4L);
		int num3 = _view.ReadInt32(8L);
		if (num != 827871305 || num2 != SlotCount || num3 != SlotPayloadSize)
		{
			_view.Write(0L, 827871305);
			_view.Write(4L, SlotCount);
			_view.Write(8L, SlotPayloadSize);
			_view.Write(12L, 0L);
			_view.Write(20L, 0L);
			for (int i = 0; i < SlotCount; i++)
			{
				int slotOffset = GetSlotOffset(i);
				_view.Write(slotOffset, 0L);
				_view.Write(slotOffset + 8, 0);
			}
			_view.Flush();
		}
	}

	private int GetSlotOffset(long seq)
	{
		int num = (int)(seq % SlotCount);
		return 32 + num * SlotSize;
	}

	private long ReadWriteSeq()
	{
		return _view.ReadInt64(12L);
	}

	private long ReadReadSeq()
	{
		return _view.ReadInt64(20L);
	}

	private void WriteWriteSeq(long value)
	{
		_view.Write(12L, value);
	}

	private void WriteReadSeq(long value)
	{
		_view.Write(20L, value);
	}

	public void Write(ReadOnlySpan<byte> data)
	{
		if (data.Length > SlotPayloadSize)
		{
			throw new InvalidOperationException($"IPC payload too large: {data.Length} > {SlotPayloadSize}.");
		}
		lock (_writeLock)
		{
			long num = ReadWriteSeq();
			long num2 = ReadReadSeq();
			SpinWait spinWait = default(SpinWait);
			while (num - num2 >= SlotCount)
			{
				spinWait.SpinOnce();
				num2 = ReadReadSeq();
			}
			long num3 = num + 1;
			int slotOffset = GetSlotOffset(num3);
			if (data.Length > 0)
			{
				byte[] array = data.ToArray();
				_view.WriteArray(slotOffset + 12, array, 0, array.Length);
			}
			_view.Write(slotOffset + 8, data.Length);
			_view.Write(slotOffset, num3);
			WriteWriteSeq(num3);
			_view.Flush();
		}
	}

	public bool TryRead(out byte[] data)
	{
		data = Array.Empty<byte>();
		long num = ReadWriteSeq();
		long num2 = ReadReadSeq();
		if (num2 >= num)
		{
			return false;
		}
		long num3 = num2 + 1;
		int slotOffset = GetSlotOffset(num3);
		if (_view.ReadInt64(slotOffset) != num3)
		{
			return false;
		}
		int num4 = _view.ReadInt32(slotOffset + 8);
		if (num4 < 0 || num4 > SlotPayloadSize)
		{
			throw new InvalidDataException($"Corrupt IPC payload length: {num4}");
		}
		data = new byte[num4];
		if (num4 > 0)
		{
			_view.ReadArray(slotOffset + 12, data, 0, num4);
		}
		WriteReadSeq(num3);
		_view.Flush();
		return true;
	}

	public void Dispose()
	{
		_view.Dispose();
		_mmf.Dispose();
	}
}

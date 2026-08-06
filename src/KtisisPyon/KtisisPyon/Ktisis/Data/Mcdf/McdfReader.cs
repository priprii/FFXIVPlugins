using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using K4os.Compression.LZ4.Legacy;

namespace Ktisis.Data.Mcdf;

public sealed class McdfReader : IDisposable
{
	private readonly FileStream _stream;

	private readonly LZ4Stream _lz4;

	private readonly McdfHeader _header;

	private const uint MareMagic = 1178878797u;

	private McdfReader(FileStream stream, LZ4Stream lz4, McdfHeader header)
	{
		_stream = stream;
		_lz4 = lz4;
		_header = header;
	}

	public static McdfReader FromPath(string path)
	{
		FileStream fileStream = File.OpenRead(path);
		LZ4Stream lz = new LZ4Stream(fileStream, LZ4StreamMode.Decompress, LZ4StreamFlags.HighCompression);
		McdfHeader mcdfHeader = ReadHeader(path, lz);
		if (mcdfHeader == null)
		{
			throw new Exception("'" + Path.GetFileName(path) + "' is not a valid MCDF file.");
		}
		return new McdfReader(fileStream, lz, mcdfHeader);
	}

	private static McdfHeader? ReadHeader(string path, LZ4Stream lz4)
	{
		BinaryReader binaryReader = new BinaryReader(lz4);
		if (binaryReader.ReadUInt32() != 1178878797)
		{
			return null;
		}
		byte b = binaryReader.ReadByte();
		if (b != 1)
		{
			return null;
		}
		int count = binaryReader.ReadInt32();
		byte[] bytes = binaryReader.ReadBytes(count);
		string json = Encoding.UTF8.GetString(bytes);
		return new McdfHeader
		{
			Version = b,
			FilePath = path,
			Data = JsonSerializer.Deserialize<McdfData>(json)
		};
	}

	public McdfData GetData()
	{
		return _header.Data;
	}

	public Dictionary<string, string> Extract(string dir)
	{
		using BinaryReader binaryReader = new BinaryReader(_lz4);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (McdfData.FileData file in _header.Data.Files)
		{
			string text = Path.Combine(dir, "ktisis_" + file.Hash + ".tmp");
			using FileStream output = File.OpenWrite(text);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			byte[] buffer = binaryReader.ReadBytes(file.Length);
			binaryWriter.Write(buffer);
			binaryWriter.Flush();
			string[] gamePaths = file.GamePaths;
			foreach (string text2 in gamePaths)
			{
				dictionary[text2] = text;
				Ktisis.Log.Debug(text2 + " => " + Path.GetFileName(text));
			}
		}
		return dictionary;
	}

	public void Dispose()
	{
		_lz4.Dispose();
		_stream.Dispose();
	}
}

using System;
using System.IO;
using Dalamud.Plugin.Services;
using Ktisis.Structs.Characters;

namespace Ktisis.GameData.Chara;

public class CharaCmpReader(BinaryReader br)
{
	private const string HumanCmpPath = "chara/xls/charamake/human.cmp";

	private const int BlockLength = 256;

	private const int DataLength = 192;

	private const int AlphaLength = 128;

	private const int CommonBlockCount = 5;

	private const int CommonBlockSize = 10;

	private const int TribeBlockSkipCount = 3;

	private const int TribeBlockCount = 2;

	private const int GenderBlockSize = 5120;

	private const int TribeBlockSize = 10240;

	private const int CommonSeekTo = 5120;

	private const int TribesSeekTo = 13312;

	private const uint ExtendedDataLength = 208u;

	public static CharaCmpReader Open(IDataManager data)
	{
		return new CharaCmpReader(new BinaryReader(new MemoryStream((data.GetFile("chara/xls/charamake/human.cmp") ?? throw new Exception("Failed to open human.cmp")).Data)));
	}

	public CommonColors ReadCommon()
	{
		SeekTo(5120u);
		uint[] eyeColors = ReadArray(192u);
		SeekNextBlock();
		uint[] highlightColors = ReadArray(208u);
		SeekNextBlock();
		SeekNextBlock();
		SeekNextBlock();
		SeekNextBlock();
		SeekNextBlock();
		uint[] lipColors = ReadArray(128u);
		SeekNextBlock();
		uint[] faceFeatureColors = ReadArray(208u);
		SeekNextBlock();
		uint[] facepaintColors = ReadArray(128u);
		SeekNextBlock();
		CommonColors result = new CommonColors();
		result.EyeColors = eyeColors;
		result.HighlightColors = highlightColors;
		result.LipColors = lipColors;
		result.FaceFeatureColors = faceFeatureColors;
		result.FacepaintColors = facepaintColors;
		return result;
	}

	public TribeColors ReadTribeData(Tribe tribe, Gender gender)
	{
		uint num = Math.Max(0u, (uint)((int)(tribe - 1) * 2) + (uint)gender);
		SeekTo((4608 + num * 1280 + 768) * 4);
		uint[] skinColors = ReadArray(192u);
		SeekTo((4608 + num * 1280 + 1024) * 4);
		bool flag = tribe - 13 <= Tribe.Midlander;
		bool flag2 = !flag;
		uint[] hairColors = ReadArray(flag2 ? 208u : 192u);
		return new TribeColors
		{
			SkinColors = skinColors,
			HairColors = hairColors
		};
	}

	private void SeekTo(uint offset)
	{
		br.BaseStream.Seek(offset, SeekOrigin.Begin);
	}

	private uint[] ReadArray(uint length)
	{
		uint[] array = new uint[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = br.ReadUInt32();
		}
		return array;
	}

	private void SeekNextBlock()
	{
		long num = br.BaseStream.Position % 1024;
		br.BaseStream.Seek(1024 - num, SeekOrigin.Current);
	}
}

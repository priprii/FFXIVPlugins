using System.Runtime.InteropServices;

namespace Ktisis.Structs.Characters;

[StructLayout(LayoutKind.Explicit, Size = 26)]
public struct CustomizeContainer
{
	public const int Size = 26;

	[FieldOffset(0)]
	public unsafe fixed byte Bytes[26];

	[FieldOffset(23)]
	public byte BustSize;

	[FieldOffset(14)]
	public byte Eyebrows;

	[FieldOffset(9)]
	public byte EyeColor;

	[FieldOffset(15)]
	public byte EyeColor2;

	[FieldOffset(16)]
	public byte EyeShape;

	[FieldOffset(12)]
	public byte FaceFeatures;

	[FieldOffset(13)]
	public byte FaceFeaturesColor;

	[FieldOffset(24)]
	public FacialFeature Facepaint;

	[FieldOffset(25)]
	public byte FacepaintColor;

	[FieldOffset(5)]
	public byte FaceType;

	[FieldOffset(1)]
	public Gender Gender;

	[FieldOffset(10)]
	public byte HairColor;

	[FieldOffset(11)]
	public byte HairColor2;

	[FieldOffset(6)]
	public byte HairStyle;

	[FieldOffset(7)]
	public byte HasHighlights;

	[FieldOffset(3)]
	public byte Height;

	[FieldOffset(18)]
	public byte JawShape;

	[FieldOffset(20)]
	public byte LipColor;

	[FieldOffset(19)]
	public byte LipStyle;

	[FieldOffset(2)]
	public byte ModelType;

	[FieldOffset(17)]
	public byte NoseShape;

	[FieldOffset(0)]
	public Race Race;

	[FieldOffset(21)]
	public byte RaceFeatureSize;

	[FieldOffset(22)]
	public byte RaceFeatureType;

	[FieldOffset(8)]
	public byte SkinColor;

	[FieldOffset(4)]
	public Tribe Tribe;

	public byte this[uint index]
	{
		get
		{
			return Get(index);
		}
		set
		{
			Set(index, value);
		}
	}

	private unsafe byte Get(uint index)
	{
		return Bytes[index];
	}

	private unsafe void Set(uint index, byte value)
	{
		Bytes[index] = value;
	}
}

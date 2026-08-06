using System.Drawing;
using System.Numerics;
using PyonPix.Shared.Extensions;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Structs;

public class CharacterProperties : ILocal<SyncedCharacterProperties>
{
	public string Alias = string.Empty;

	public Vector3 AliasColourA = Vector3.One;

	public Vector3 AliasColourB = Vector3.One;

	public Vector3 AliasGlowA = Vector3.One;

	public Vector3 AliasGlowB = Vector3.One;

	public AnimationType AliasAnimationType;

	public Vector3 PixColourA = Vector3.One;

	public Vector3 PixColourB = Vector3.One;

	public Vector3 PixGlowA = Vector3.One;

	public Vector3 PixGlowB = Vector3.One;

	public AnimationType PixAnimationType;

	public SyncedCharacterProperties ToSynced()
	{
		return new SyncedCharacterProperties
		{
			Alias = Alias,
			AliasStyle = new StyleDto
			{
				ColourA = ColorTranslator.ToHtml(AliasColourA.ToColor()),
				ColourB = ColorTranslator.ToHtml(AliasColourB.ToColor()),
				GlowA = ColorTranslator.ToHtml(AliasGlowA.ToColor()),
				GlowB = ColorTranslator.ToHtml(AliasGlowB.ToColor()),
				AnimationType = AliasAnimationType
			},
			PixStyle = new StyleDto
			{
				ColourA = ColorTranslator.ToHtml(PixColourA.ToColor()),
				ColourB = ColorTranslator.ToHtml(PixColourB.ToColor()),
				GlowA = ColorTranslator.ToHtml(PixGlowA.ToColor()),
				GlowB = ColorTranslator.ToHtml(PixGlowB.ToColor()),
				AnimationType = PixAnimationType
			}
		};
	}

	public bool Equals(CharacterProperties? other)
	{
		if (other == null)
		{
			return false;
		}
		SyncedCharacterProperties syncedCharacterProperties = ToSynced();
		SyncedCharacterProperties syncedCharacterProperties2 = other.ToSynced();
		if (syncedCharacterProperties.Alias != other.Alias)
		{
			return false;
		}
		if (syncedCharacterProperties.AliasStyle?.ColourA != syncedCharacterProperties2.AliasStyle?.ColourA)
		{
			return false;
		}
		if (syncedCharacterProperties.AliasStyle?.ColourB != syncedCharacterProperties2.AliasStyle?.ColourB)
		{
			return false;
		}
		if (syncedCharacterProperties.AliasStyle?.GlowA != syncedCharacterProperties2.AliasStyle?.GlowA)
		{
			return false;
		}
		if (syncedCharacterProperties.AliasStyle?.GlowB != syncedCharacterProperties2.AliasStyle?.GlowB)
		{
			return false;
		}
		if (syncedCharacterProperties.AliasStyle?.AnimationType != syncedCharacterProperties2.AliasStyle?.AnimationType)
		{
			return false;
		}
		if (syncedCharacterProperties.PixStyle?.ColourA != syncedCharacterProperties2.PixStyle?.ColourA)
		{
			return false;
		}
		if (syncedCharacterProperties.PixStyle?.ColourB != syncedCharacterProperties2.PixStyle?.ColourB)
		{
			return false;
		}
		if (syncedCharacterProperties.PixStyle?.GlowA != syncedCharacterProperties2.PixStyle?.GlowA)
		{
			return false;
		}
		if (syncedCharacterProperties.PixStyle?.GlowB != syncedCharacterProperties2.PixStyle?.GlowB)
		{
			return false;
		}
		if (syncedCharacterProperties.PixStyle?.AnimationType != syncedCharacterProperties2.PixStyle?.AnimationType)
		{
			return false;
		}
		return true;
	}
}

using System.Numerics;
using PyonPix.Shared.Extensions;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto.Client;

namespace PyonPix.Shared.Structs;

public class SyncedCharacterProperties : ISynced<CharacterProperties>
{
	public string Alias { get; set; } = string.Empty;

	public StyleDto? AliasStyle { get; set; }

	public StyleDto? PixStyle { get; set; }

	public void ApplyTo(CharacterProperties target)
	{
		target.Alias = Alias;
		target.AliasColourA = AliasStyle?.ColourA?.ToVector3() ?? Vector3.One;
		target.AliasColourB = AliasStyle?.ColourB?.ToVector3() ?? target.AliasColourA;
		target.AliasGlowA = AliasStyle?.GlowA?.ToVector3() ?? target.AliasColourA;
		target.AliasGlowB = AliasStyle?.GlowB?.ToVector3() ?? target.AliasColourA;
		target.AliasAnimationType = AliasStyle?.AnimationType ?? AnimationType.Static;
		target.PixColourA = PixStyle?.ColourA?.ToVector3() ?? Vector3.One;
		target.PixColourB = PixStyle?.ColourB?.ToVector3() ?? target.PixColourA;
		target.PixGlowA = PixStyle?.GlowA?.ToVector3() ?? target.PixColourA;
		target.PixGlowB = PixStyle?.GlowB?.ToVector3() ?? target.PixColourA;
		target.PixAnimationType = PixStyle?.AnimationType ?? AnimationType.Static;
	}
}

namespace Ktisis.Services.Data;

public interface INameResolver
{
	string? GetWeaponName(ushort id, ushort secondId, ushort variant);
}

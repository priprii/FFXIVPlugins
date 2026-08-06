namespace PyonPix.Structs.PlayerState;

public class ResidentialTerritory(uint id, string name, string subName, ResidentialType residentialType)
{
	public uint Id = id;

	public string Name = name;

	public string SubName = subName;

	public ResidentialType ResidentialType = residentialType;
}

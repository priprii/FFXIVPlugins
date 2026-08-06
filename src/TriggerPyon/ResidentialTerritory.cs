namespace TriggerPyon;

public class ResidentialTerritory
{
	public uint Id;

	public string Name;

	public ResidentialType ResidentialType;

	public ResidentialTerritory(uint id, string name, ResidentialType residentialType)
	{
		Id = id;
		Name = name;
		ResidentialType = residentialType;
	}
}

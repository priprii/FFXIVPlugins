namespace Ktisis.Localization;

public class LocaleMetaData
{
	public string TechnicalName { get; }

	public string DisplayName { get; }

	public string SelfName { get; }

	public string?[] Maintainers { get; }

	internal LocaleMetaData(string technicalName, string displayName, string selfName, string?[] maintainers)
	{
		TechnicalName = technicalName;
		DisplayName = displayName;
		SelfName = selfName;
		Maintainers = maintainers;
	}
}

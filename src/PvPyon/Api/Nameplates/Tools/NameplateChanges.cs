using System.Collections.Generic;
using PvPyon.Api.Tools.Strings;

namespace PvPyon.Api.Nameplates.Tools;

public class NameplateChanges
{
	private readonly Dictionary<NameplateElements, StringChangesProps> changes = new Dictionary<NameplateElements, StringChangesProps>();

	public NameplateChanges()
	{
		changes.Add(NameplateElements.Title, new StringChangesProps());
		changes.Add(NameplateElements.Name, new StringChangesProps());
		changes.Add(NameplateElements.FreeCompany, new StringChangesProps());
	}

	public StringChangesProps GetProps(NameplateElements element)
	{
		return changes[element];
	}

	public StringChanges GetChanges(NameplateElements element)
	{
		return GetProps(element).StringChanges;
	}

	public StringChange GetChange(NameplateElements element, StringPosition position)
	{
		return GetChanges(element).GetChange(position);
	}
}

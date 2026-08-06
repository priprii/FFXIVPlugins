using System.Collections.Generic;
using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Ktisis.Core.Attributes;
using Ktisis.GameData.Excel;

namespace Ktisis.Services.Data;

[Singleton]
public class NamingService : INameResolver
{
	private readonly IDataManager _data;

	public NamingService(IDataManager data)
	{
		_data = data;
	}

	public string? GetWeaponName(ushort id, ushort secondId, ushort variant)
	{
		if (id == 0)
		{
			return null;
		}
		ItemSheet itemSheet = GetWeapons().FirstOrDefault(delegate(ItemSheet wep)
		{
			if (wep.Model.Matches(id, secondId, variant))
			{
				return true;
			}
			return wep.SubModel.Id != 0 && wep.SubModel.Matches(id, secondId, variant);
		});
		if (StringExtensions.IsNullOrEmpty(itemSheet.Name))
		{
			return null;
		}
		return itemSheet.Name;
	}

	private IEnumerable<ItemSheet> GetWeapons()
	{
		return ((IEnumerable<ItemSheet>)_data.GetExcelSheet<ItemSheet>((ClientLanguage?)null, (string)null)).Where((ItemSheet item) => item.IsWeapon());
	}
}

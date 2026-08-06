using System.Collections.Generic;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

namespace PvPyon;

public static class WorldHelper
{
	private static Dictionary<uint, string>? s_WorldNames;

	public static Dictionary<uint, string> WorldNames
	{
		get
		{
			if (s_WorldNames == null)
			{
				s_WorldNames = new Dictionary<uint, string>();
				ExcelSheet<World> excelSheet = PluginServices.DataManager.GetExcelSheet<World>();
				if (excelSheet != null)
				{
					foreach (World item in excelSheet)
					{
						s_WorldNames[((ExcelRow)item).RowId] = SeString.op_Implicit(item.Name);
					}
				}
			}
			return s_WorldNames;
		}
	}

	public static string? GetWorldName(uint? worldId)
	{
		if (worldId.HasValue && WorldNames.TryGetValue(worldId.Value, out string value))
		{
			return value;
		}
		return null;
	}
}

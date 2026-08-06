using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

namespace PvPyon;

public static class RoleHelper
{
	private static Dictionary<string, Role>? s_RolesByJobAbbreviation = null;

	private static Dictionary<string, DpsRole>? s_DpsRolesByJobAbbreviation = null;

	private static Dictionary<string, RangedDpsRole>? s_RangedDpsRolesByJobAbbreviation = null;

	private static Dictionary<string, LandHandRole>? s_LandHandRolesByJobAbbreviation = null;

	public static Dictionary<byte, Role> RolesByRoleId { get; } = new Dictionary<byte, Role>
	{
		{
			0,
			Role.LandHand
		},
		{
			1,
			Role.Tank
		},
		{
			2,
			Role.Dps
		},
		{
			3,
			Role.Dps
		},
		{
			4,
			Role.Healer
		}
	};

	public static Dictionary<byte, DpsRole> DpsRolesByRoleId { get; } = new Dictionary<byte, DpsRole>
	{
		{
			2,
			DpsRole.Melee
		},
		{
			3,
			DpsRole.Ranged
		}
	};

	public static Dictionary<byte, RangedDpsRole> RangedDpsRolesByPrimaryStat { get; } = new Dictionary<byte, RangedDpsRole>
	{
		{
			4,
			RangedDpsRole.Magical
		},
		{
			2,
			RangedDpsRole.Physical
		}
	};

	public static Dictionary<string, Role> RolesByJobAbbreviation
	{
		get
		{
			if (s_RolesByJobAbbreviation == null)
			{
				s_RolesByJobAbbreviation = new Dictionary<string, Role>();
				ExcelSheet<ClassJob> excelSheet = PluginServices.DataManager.GetExcelSheet<ClassJob>();
				if (excelSheet != null)
				{
					foreach (ClassJob item in ((IEnumerable<ClassJob>)excelSheet).Where((ClassJob classJob) => !string.IsNullOrEmpty(classJob.Abbreviation.RawString)))
					{
						if (RolesByRoleId.TryGetValue(item.Role, out var value))
						{
							s_RolesByJobAbbreviation[SeString.op_Implicit(item.Abbreviation)] = value;
						}
					}
				}
			}
			return s_RolesByJobAbbreviation;
		}
	}

	public static Dictionary<string, DpsRole> DpsRolesByJobAbbreviation
	{
		get
		{
			if (s_DpsRolesByJobAbbreviation == null)
			{
				s_DpsRolesByJobAbbreviation = new Dictionary<string, DpsRole>();
				ExcelSheet<ClassJob> excelSheet = PluginServices.DataManager.GetExcelSheet<ClassJob>();
				if (excelSheet != null)
				{
					foreach (ClassJob item in ((IEnumerable<ClassJob>)excelSheet).Where((ClassJob classJob) => !string.IsNullOrEmpty(classJob.Abbreviation.RawString)))
					{
						if (DpsRolesByRoleId.TryGetValue(item.Role, out var value))
						{
							s_DpsRolesByJobAbbreviation[SeString.op_Implicit(item.Abbreviation)] = value;
						}
					}
				}
			}
			return s_DpsRolesByJobAbbreviation;
		}
	}

	public static Dictionary<string, RangedDpsRole> RangedDpsRolesByJobAbbreviation
	{
		get
		{
			if (s_RangedDpsRolesByJobAbbreviation == null)
			{
				s_RangedDpsRolesByJobAbbreviation = new Dictionary<string, RangedDpsRole>();
				ExcelSheet<ClassJob> excelSheet = PluginServices.DataManager.GetExcelSheet<ClassJob>();
				if (excelSheet != null)
				{
					foreach (ClassJob item in ((IEnumerable<ClassJob>)excelSheet).Where((ClassJob classJob) => !string.IsNullOrEmpty(classJob.Abbreviation.RawString)))
					{
						if (DpsRolesByJobAbbreviation.TryGetValue(SeString.op_Implicit(item.Abbreviation), out var value) && value == DpsRole.Ranged && RangedDpsRolesByPrimaryStat.TryGetValue(item.PrimaryStat, out var value2))
						{
							s_RangedDpsRolesByJobAbbreviation[SeString.op_Implicit(item.Abbreviation)] = value2;
						}
					}
				}
			}
			return s_RangedDpsRolesByJobAbbreviation;
		}
	}

	public static Dictionary<string, LandHandRole> LandHandRolesByJobAbbreviation
	{
		get
		{
			if (s_LandHandRolesByJobAbbreviation == null)
			{
				s_LandHandRolesByJobAbbreviation = new Dictionary<string, LandHandRole>();
				ExcelSheet<ClassJob> excelSheet = PluginServices.DataManager.GetExcelSheet<ClassJob>();
				ExcelSheet<GatheringSubCategory> excelSheet2 = PluginServices.DataManager.GetExcelSheet<GatheringSubCategory>();
				if (excelSheet != null && excelSheet2 != null)
				{
					IEnumerable<SeString> source = (from gatheringSubCategory in (IEnumerable<GatheringSubCategory>)excelSheet2
						select gatheringSubCategory.ClassJob.Value into classJob
						where classJob != null
						select classJob.Abbreviation).Distinct();
					foreach (ClassJob item in ((IEnumerable<ClassJob>)excelSheet).Where((ClassJob classJob) => !string.IsNullOrEmpty(classJob.Abbreviation.RawString)))
					{
						if (RolesByRoleId.TryGetValue(item.Role, out var value) && value == Role.LandHand)
						{
							if (source.Contains(item.Abbreviation))
							{
								s_LandHandRolesByJobAbbreviation[SeString.op_Implicit(item.Abbreviation)] = LandHandRole.Land;
							}
							else
							{
								s_LandHandRolesByJobAbbreviation[SeString.op_Implicit(item.Abbreviation)] = LandHandRole.Hand;
							}
						}
					}
				}
			}
			return s_LandHandRolesByJobAbbreviation;
		}
	}
}

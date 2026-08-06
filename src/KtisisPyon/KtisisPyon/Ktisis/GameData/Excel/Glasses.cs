using Ktisis.Common.Extensions;
using Lumina.Excel;

namespace Ktisis.GameData.Excel;

[Sheet("Glasses", 799720129u)]
public struct Glasses(ExcelPage page, uint offset, uint row) : IExcelRow<Glasses>
{
	public ExcelPage ExcelPage => page;

	public uint RowOffset { get; } = offset;

	public uint RowId { get; } = row;

	public string Name { get; set; } = string.Empty;

	public uint Icon { get; set; } = 0u;

	static Glasses IExcelRow<Glasses>.Create(ExcelPage page, uint offset, uint row)
	{
		Glasses result = new Glasses(page, offset, row);
		result.Name = page.ReadColumn<string>(13, offset);
		result.Icon = (uint)page.ReadColumn<int>(2, offset);
		return result;
	}
}

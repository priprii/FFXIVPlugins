using Dalamud.Interface.Textures;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Services.Environment;

public class WeatherInfo
{
	public readonly string Name;

	public readonly uint RowId;

	public readonly ISharedImmediateTexture? Icon;

	public WeatherInfo(Weather row, ISharedImmediateTexture? icon)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySeString name = ((Weather)(ref row)).Name;
		string text = ((ReadOnlySeString)(ref name)).ExtractText();
		if (StringExtensions.IsNullOrEmpty(text))
		{
			text = $"Weather #{((Weather)(ref row)).RowId}";
		}
		Name = text;
		RowId = ((Weather)(ref row)).RowId;
		Icon = icon;
	}
}

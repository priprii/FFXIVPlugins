using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Core.Attributes;
using Ktisis.Structs.Env;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Ktisis.Services.Environment;

[Singleton]
public class WeatherService
{
	private readonly IDataManager _data;

	private readonly IFramework _framework;

	private readonly ITextureProvider _texture;

	public WeatherService(IDataManager data, IFramework framework, ITextureProvider texture)
	{
		_data = data;
		_framework = framework;
		_texture = texture;
	}

	public async Task<IEnumerable<WeatherInfo>> GetWeatherTypes(CancellationToken token)
	{
		await Task.Yield();
		List<WeatherInfo> results = new List<WeatherInfo>();
		Task<byte[]> task = _framework.RunOnFrameworkThread<byte[]>((Func<byte[]>)GetEnvWeatherIds);
		ExcelSheet<Weather> weatherSheet = _data.GetExcelSheet<Weather>((ClientLanguage?)null, (string)null);
		byte[] array = await task;
		foreach (byte b in array)
		{
			if (token.IsCancellationRequested)
			{
				break;
			}
			if (weatherSheet.HasRow((uint)b))
			{
				Weather row = weatherSheet.GetRow((uint)b);
				ITextureProvider texture = _texture;
				GameIconLookup val = GameIconLookup.op_Implicit((uint)((Weather)(ref row)).Icon);
				ISharedImmediateTexture fromGameIcon = texture.GetFromGameIcon(ref val);
				WeatherInfo item = new WeatherInfo(row, fromGameIcon);
				results.Add(item);
			}
		}
		token.ThrowIfCancellationRequested();
		return results;
	}

	public unsafe byte[] GetEnvWeatherIds()
	{
		EnvManagerEx* ptr = EnvManagerEx.Instance();
		EnvScene* ptr2 = ((ptr != null) ? ((EnvManager)(&ptr->_base)).EnvScene : null);
		if (ptr2 == null)
		{
			return Array.Empty<byte>();
		}
		return ((EnvScene)ptr2).WeatherIds.TrimEnd((byte)0).ToArray();
	}
}

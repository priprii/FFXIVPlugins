using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Services.Data;

[Singleton]
public class FormatService
{
	private readonly IClientState _client;

	private readonly IObjectTable _objectTable;

	private readonly IDataManager _data;

	private readonly List<string> ReplacerKeys;

	public FormatService(IClientState client, IObjectTable objectTable, IDataManager data)
	{
		int num = 10;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		span[0] = "%Date%";
		span[1] = "%Year%";
		span[2] = "%Month%";
		span[3] = "%Day%";
		span[4] = "%Time%";
		span[5] = "%Time12%";
		span[6] = "%PlayerName%";
		span[7] = "%CurrentWorld%";
		span[8] = "%HomeWorld%";
		span[9] = "%Zone%";
		ReplacerKeys = list;
		base._002Ector();
		_client = client;
		_objectTable = objectTable;
		_data = data;
	}

	public string Replace(string path)
	{
		foreach (string item in ReplacerKeys.Where(path.Contains))
		{
			string keyReplacement = GetKeyReplacement(item);
			do
			{
				path = path.Replace(item, keyReplacement, ignoreCase: true, null);
			}
			while (path.Contains(item));
		}
		return path;
	}

	public Dictionary<string, string> GetReplacements()
	{
		return ReplacerKeys.ToDictionary((string key) => key, GetKeyReplacement);
	}

	private string GetKeyReplacement(string key)
	{
		return key switch
		{
			"%Date%" => DateTime.Now.ToString("yyyy-MM-dd"), 
			"%Year%" => DateTime.Now.ToString("yyyy"), 
			"%Month%" => DateTime.Now.ToString("MM"), 
			"%Day%" => DateTime.Now.ToString("dd"), 
			"%Time%" => DateTime.Now.ToString("HH-mm-ss"), 
			"%Time12%" => DateTime.Now.ToString("hh-mm-ss"), 
			"%PlayerName%" => GetPlayerName(), 
			"%CurrentWorld%" => GetCurrentWorld(), 
			"%HomeWorld%" => GetHomeWorld(), 
			"%Zone%" => GetZone(), 
			_ => string.Empty, 
		};
	}

	private string GetPlayerName()
	{
		IPlayerCharacter localPlayer = _objectTable.LocalPlayer;
		return StripInvalidChars(((localPlayer != null) ? ((object)((IGameObject)localPlayer).Name).ToString() : null) ?? "Unknown");
	}

	private string GetCurrentWorld()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		IPlayerCharacter localPlayer = _objectTable.LocalPlayer;
		object obj;
		if (localPlayer == null)
		{
			obj = null;
		}
		else
		{
			World value = localPlayer.CurrentWorld.Value;
			obj = ((object)((World)(ref value)).Name/*cast due to constrained. prefix*/).ToString();
		}
		if (obj == null)
		{
			obj = "Unknown";
		}
		return StripInvalidChars((string)obj);
	}

	private string GetHomeWorld()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		IPlayerCharacter localPlayer = _objectTable.LocalPlayer;
		object obj;
		if (localPlayer == null)
		{
			obj = null;
		}
		else
		{
			World value = localPlayer.HomeWorld.Value;
			obj = ((object)((World)(ref value)).Name/*cast due to constrained. prefix*/).ToString();
		}
		if (obj == null)
		{
			obj = "Unknown";
		}
		return StripInvalidChars((string)obj);
	}

	private string GetZone()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		uint territoryType = _client.TerritoryType;
		ExcelSheet<TerritoryType> excelSheet = _data.GetExcelSheet<TerritoryType>((ClientLanguage?)null, (string)null);
		if (excelSheet.HasRow(territoryType))
		{
			TerritoryType row = excelSheet.GetRow(territoryType);
			RowRef<PlaceName> placeName = ((TerritoryType)(ref row)).PlaceName;
			if (placeName.IsValid)
			{
				PlaceName value = placeName.Value;
				ReadOnlySeString name = ((PlaceName)(ref value)).Name;
				return StripInvalidChars(((ReadOnlySeString)(ref name)).ExtractText());
			}
		}
		return "Unknown";
	}

	public string StripInvalidChars(string str)
	{
		return Path.GetInvalidFileNameChars().Aggregate(str, (string current, char c) => current.Replace(c, '_'));
	}
}

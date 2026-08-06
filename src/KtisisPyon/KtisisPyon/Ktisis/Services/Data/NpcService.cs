using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Ktisis.Common.Extensions;
using Ktisis.Core.Attributes;
using Ktisis.GameData.Excel;
using Ktisis.GameData.Excel.Types;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;

namespace Ktisis.Services.Data;

[Singleton]
public class NpcService
{
	private readonly IDataManager _data;

	public NpcService(IDataManager data)
	{
		_data = data;
	}

	public async Task<IEnumerable<INpcBase>> GetNpcList()
	{
		await Task.Yield();
		Stopwatch timer = new Stopwatch();
		timer.Start();
		Task<IEnumerable<INpcBase>> battleTask = GetBattleNpcs();
		Task<IEnumerable<INpcBase>> residentTask = GetResidentNpcs();
		InlineArray2<Task<IEnumerable<INpcBase>>> buffer = default(InlineArray2<Task<IEnumerable<INpcBase>>>);
		buffer[0] = battleTask;
		buffer[1] = residentTask;
		await Task.WhenAll<IEnumerable<INpcBase>>(buffer);
		IEnumerable<INpcBase> result = battleTask.Result.Concat(residentTask.Result).DistinctBy((INpcBase npc) => (Name: npc.Name, npc.GetModelId(), npc.GetCustomize(), npc.GetEquipment()));
		timer.Stop();
		Ktisis.Log.Debug($"NPC list retrieved in {timer.Elapsed.TotalMilliseconds:00.00}ms");
		return result;
	}

	private async Task<IEnumerable<INpcBase>> GetBattleNpcs()
	{
		await Task.Yield();
		List<string> failedLines;
		List<Exception> exceptions;
		List<BNpcLink> nameIndex = CsvLoader.LoadResource<BNpcLink>("LuminaSupplemental.Excel.Generated.BNpcLink.csv", includesHeaders: false, out failedLines, out exceptions, _data.GameData, _data.GameData.Options.DefaultExcelLanguage);
		return ((IEnumerable<BattleNpc>)_data.GetExcelSheet<BattleNpc>((ClientLanguage?)null, (string)null)).Skip(1).Select(delegate(BattleNpc row)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			string text = null;
			if (nameIndex.Any((BNpcLink link) => link.BNpcBase.RowId == row.RowId))
			{
				RowRef<BNpcName> bNpcName = nameIndex.First((BNpcLink link) => link.BNpcBase.RowId == row.RowId).BNpcName;
				BNpcName value = bNpcName.Value;
				ReadOnlySeString singular = ((BNpcName)(ref value)).Singular;
				string name = ((ReadOnlySeString)(ref singular)).ExtractText();
				value = bNpcName.Value;
				text = name.FormatName(((BNpcName)(ref value)).Article);
			}
			row.Name = text ?? $"B:{row.RowId:D7}";
			return row;
		}).Cast<INpcBase>();
	}

	private async Task<IEnumerable<INpcBase>> GetResidentNpcs()
	{
		await Task.Yield();
		return ((IEnumerable<ResidentNpc>)_data.GetExcelSheet<ResidentNpc>((ClientLanguage?)null, (string)null)).Where((ResidentNpc npc) => npc.Map != 0).Cast<INpcBase>();
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Ktisis.GameData.Chara;
using Ktisis.Services.Data;
using Ktisis.Structs.Characters;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Editor.Characters.Make;

public class MakeTypeData
{
	private readonly Dictionary<(Tribe, Gender), MakeTypeRace> MakeTypes = new Dictionary<(Tribe, Gender), MakeTypeRace>();

	private CommonColors Colors = new CommonColors();

	public MakeTypeRace? GetData(Tribe tribe, Gender gender)
	{
		lock (MakeTypes)
		{
			return MakeTypes.GetValueOrDefault((tribe, gender));
		}
	}

	public async Task Build(IDataManager data, CustomizeService discover)
	{
		Stopwatch stop = new Stopwatch();
		stop.Start();
		await BuildMakeType(data);
		Ktisis.Log.Debug($"Built MakeType data in {stop.Elapsed.TotalMilliseconds:00.00}ms");
		InlineArray2<Task> buffer = default(InlineArray2<Task>);
		buffer[0] = PopulateDiscoveryData(discover);
		buffer[1] = BuildColors(data);
		await Task.WhenAll(buffer);
		stop.Stop();
		Ktisis.Log.Debug($"Total {stop.Elapsed.TotalMilliseconds:00.00}ms");
	}

	private async Task BuildMakeType(IDataManager data)
	{
		await Task.Yield();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Enumerator<CharaMakeType> enumerator = data.GetExcelSheet<CharaMakeType>((ClientLanguage?)null, (string)null).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				CharaMakeType current = enumerator.Current;
				BuildRowCustomize(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Ktisis.Log.Debug($"Built customize data in {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
		stopwatch.Restart();
		PopulateCustomizeIcons(data);
		stopwatch.Stop();
		Ktisis.Log.Debug($"Populated customize icons in {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
	}

	public uint[] GetColors(CustomizeIndex index)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		switch (index - 9)
		{
		default:
			if ((int)index != 20)
			{
				if ((int)index != 25)
				{
					break;
				}
				return Colors.FacepaintColors;
			}
			return Colors.LipColors;
		case 0:
		case 6:
			return Colors.EyeColors;
		case 2:
			return Colors.HighlightColors;
		case 4:
			return Colors.FaceFeatureColors;
		case 1:
		case 3:
		case 5:
			break;
		}
		throw new Exception($"Invalid index {index} for color lookup.");
	}

	public uint[] GetColors(CustomizeIndex index, Tribe tribe, Gender gender)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if ((int)index != 8)
		{
			if ((int)index == 10)
			{
				return GetData(tribe, gender)?.Colors.HairColors ?? Array.Empty<uint>();
			}
			return GetColors(index);
		}
		return GetData(tribe, gender)?.Colors.SkinColors ?? Array.Empty<uint>();
	}

	private void BuildRowCustomize(CharaMakeType row)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Invalid comparison between Unknown and I4
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		Tribe tribe = (Tribe)((CharaMakeType)(ref row)).Tribe.RowId;
		Gender gender = (Gender)((CharaMakeType)(ref row)).Gender;
		MakeTypeRace makeTypeRace = new MakeTypeRace(tribe, gender);
		lock (MakeTypes)
		{
			MakeTypes[(tribe, gender)] = makeTypeRace;
		}
		foreach (CharaMakeStructStruct item in ((IEnumerable<CharaMakeStructStruct>)(object)((CharaMakeType)(ref row)).CharaMakeStruct).Where((CharaMakeStructStruct make) => ((CharaMakeStructStruct)(ref make)).Customize != 0))
		{
			CharaMakeStructStruct current = item;
			CustomizeIndex val = (CustomizeIndex)((CharaMakeStructStruct)(ref current)).Customize;
			if ((int)val != 12 || !makeTypeRace.Customize.ContainsKey(val))
			{
				bool isCustomize = ((CharaMakeStructStruct)(ref current)).SubMenuType == 1 && ((CharaMakeStructStruct)(ref current)).SubMenuNum > 10;
				IEnumerable<MakeTypeParam> source = BuildParamData(val, current, isCustomize);
				Dictionary<CustomizeIndex, MakeTypeFeature> customize = makeTypeRace.Customize;
				MakeTypeFeature makeTypeFeature = new MakeTypeFeature();
				string name;
				if (!((CharaMakeStructStruct)(ref current)).Menu.IsValid)
				{
					name = string.Empty;
				}
				else
				{
					Lobby value = ((CharaMakeStructStruct)(ref current)).Menu.Value;
					ReadOnlySeString text = ((Lobby)(ref value)).Text;
					name = ((ReadOnlySeString)(ref text)).ExtractText();
				}
				makeTypeFeature.Name = name;
				makeTypeFeature.Index = val;
				makeTypeFeature.Params = source.ToArray();
				makeTypeFeature.IsCustomize = isCustomize;
				makeTypeFeature.IsIcon = ((CharaMakeStructStruct)(ref current)).SubMenuType == 1;
				customize[val] = makeTypeFeature;
			}
		}
		BuildRowFaceFeatures(row, makeTypeRace);
	}

	private static IEnumerable<MakeTypeParam> BuildParamData(CustomizeIndex index, CharaMakeStructStruct feature, bool isCustomize)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (((CharaMakeStructStruct)(ref feature)).SubMenuType <= 1)
		{
			int num = ((isCustomize && (int)index == 24) ? 1 : 0);
			int len = (isCustomize ? (num + 1) : ((CharaMakeStructStruct)(ref feature)).SubMenuNum);
			for (int i = num; i < len; i++)
			{
				byte value = ((CharaMakeStructStruct)(ref feature)).SubMenuGraphic[i];
				uint graphic = ((CharaMakeStructStruct)(ref feature)).SubMenuParam[i];
				yield return new MakeTypeParam
				{
					Value = value,
					Graphic = graphic
				};
			}
		}
	}

	private static void BuildRowFaceFeatures(CharaMakeType row, MakeTypeRace data)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		MakeTypeFeature feature = data.GetFeature((CustomizeIndex)5);
		if (feature == null)
		{
			return;
		}
		Collection<FacialFeatureOptionStruct> facialFeatureOption = ((CharaMakeType)(ref row)).FacialFeatureOption;
		for (byte b = 0; b < feature.Params.Length; b++)
		{
			byte value = feature.Params[b].Value;
			if (facialFeatureOption.Count == 8)
			{
				uint[] array = new uint[7];
				FacialFeatureOptionStruct val = facialFeatureOption[(int)b];
				array[0] = (uint)((FacialFeatureOptionStruct)(ref val)).Option1;
				val = facialFeatureOption[(int)b];
				array[1] = (uint)((FacialFeatureOptionStruct)(ref val)).Option2;
				val = facialFeatureOption[(int)b];
				array[2] = (uint)((FacialFeatureOptionStruct)(ref val)).Option3;
				val = facialFeatureOption[(int)b];
				array[3] = (uint)((FacialFeatureOptionStruct)(ref val)).Option4;
				val = facialFeatureOption[(int)b];
				array[4] = (uint)((FacialFeatureOptionStruct)(ref val)).Option5;
				val = facialFeatureOption[(int)b];
				array[5] = (uint)((FacialFeatureOptionStruct)(ref val)).Option6;
				val = facialFeatureOption[(int)b];
				array[6] = (uint)((FacialFeatureOptionStruct)(ref val)).Option7;
				data.FaceFeatureIcons[value] = array;
			}
		}
	}

	private void PopulateCustomizeIcons(IDataManager data)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Invalid comparison between Unknown and I4
		ExcelSheet<CharaMakeCustomize> excelSheet = data.GetExcelSheet<CharaMakeCustomize>((ClientLanguage?)null, (string)null);
		IEnumerable<MakeTypeFeature> enumerable;
		lock (MakeTypes)
		{
			enumerable = MakeTypes.SelectMany<KeyValuePair<(Tribe, Gender), MakeTypeRace>, MakeTypeFeature>((KeyValuePair<(Tribe, Gender), MakeTypeRace> make) => make.Value.Customize.Values).Where(delegate(MakeTypeFeature feat)
			{
				if (feat != null && feat.IsCustomize)
				{
					MakeTypeParam[] array = feat.Params;
					if (array != null)
					{
						return array.Length > 0;
					}
				}
				return false;
			}).ToList();
		}
		foreach (MakeTypeFeature item in enumerable)
		{
			uint start = item.Params[0].Graphic - 2;
			uint count = (((int)item.Index == 6) ? 99u : 49u);
			item.Params = BuildParamFromCustomize(excelSheet, start, count).ToArray();
		}
	}

	private static IEnumerable<MakeTypeParam> BuildParamFromCustomize(ExcelSheet<CharaMakeCustomize> custom, uint start, uint count)
	{
		for (uint i = start; i < start + count; i++)
		{
			if (custom.HasRow(i))
			{
				CharaMakeCustomize row = custom.GetRow(i);
				if (((CharaMakeCustomize)(ref row)).FeatureID != 0 || ((CharaMakeCustomize)(ref row)).Icon != 0)
				{
					yield return new MakeTypeParam
					{
						Value = ((CharaMakeCustomize)(ref row)).FeatureID,
						Graphic = ((CharaMakeCustomize)(ref row)).Icon
					};
				}
			}
		}
	}

	private async Task BuildColors(IDataManager dataMgr)
	{
		await Task.Yield();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		CharaCmpReader charaCmpReader = CharaCmpReader.Open(dataMgr);
		Colors = charaCmpReader.ReadCommon();
		IEnumerable<MakeTypeRace> enumerable;
		lock (MakeTypes)
		{
			enumerable = MakeTypes.Values.ToList();
		}
		foreach (MakeTypeRace item in enumerable)
		{
			item.Colors = charaCmpReader.ReadTribeData(item.Tribe, item.Gender);
		}
		stopwatch.Stop();
		Ktisis.Log.Debug($"Built color data in {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
	}

	private async Task PopulateDiscoveryData(CustomizeService discover)
	{
		await Task.Yield();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		IEnumerable<MakeTypeRace> enumerable;
		lock (MakeTypes)
		{
			enumerable = MakeTypes.Values.ToList();
		}
		foreach (MakeTypeRace item in enumerable)
		{
			ushort dataId = discover.CalcDataIdFor(item.Tribe, item.Gender);
			MakeTypeFeature feature = item.GetFeature((CustomizeIndex)5);
			if (feature != null)
			{
				IEnumerable<byte> enumerable2 = discover.GetFaceTypes(dataId).Except(feature.Params.Select((MakeTypeParam param) => param.Value));
				bool flag;
				switch (item.Tribe)
				{
				case Tribe.Duskwight:
				case Tribe.Dunesfolk:
				case Tribe.MoonKeeper:
				case Tribe.Hellsguard:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					enumerable2 = enumerable2.Except(feature.Params.Select((MakeTypeParam param) => (byte)(param.Value + 100)));
				}
				ConcatFeatIds(feature, enumerable2);
			}
			MakeTypeFeature feature2 = item.GetFeature((CustomizeIndex)6);
			if (feature2 != null)
			{
				IEnumerable<byte> ids = discover.GetHairTypes(dataId).Except(feature2.Params.Select((MakeTypeParam param) => param.Value));
				ConcatFeatIds(feature2, ids);
			}
		}
		stopwatch.Stop();
		Ktisis.Log.Debug($"Populated discovery data in {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
	}

	private static void ConcatFeatIds(MakeTypeFeature feat, IEnumerable<byte> ids)
	{
		feat.Params = feat.Params.Concat(ids.Select((byte id) => new MakeTypeParam
		{
			Value = id,
			Graphic = 0u
		})).ToArray();
	}
}

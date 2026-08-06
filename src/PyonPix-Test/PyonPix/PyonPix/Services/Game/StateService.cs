using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using PyonPix.Config;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Structs.PlayerState;
using PyonPix.Ui;

namespace PyonPix.Services.Game;

public class StateService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	public Dictionary<Region, List<WorldInfo>> Worlds = new Dictionary<Region, List<WorldInfo>>();

	public List<ResidentialTerritory> ResidentialTerritories = new List<ResidentialTerritory>();

	public List<NonResidentialTerritory> NonResidentialTerritories = new List<NonResidentialTerritory>();

	public readonly List<(uint Id, string Name, bool IsResidential)> UITerritoryList = new List<(uint, string, bool)>();

	public TerritoryData? CurrentTerritory;

	private TerritoryData? PreviousTerritory;

	private bool IsLoadingTerritory;

	private bool IsInitialLoad = true;

	public Vector3 LocalPlayerPosition { get; private set; }

	public Quaternion LocalPlayerRotation { get; private set; }

	public long LocalPlayerContentId { get; private set; }

	public bool LocalPlayerExists { get; private set; }

	private unsafe short CurrentWard => (short)(((HousingManager)HousingManager.Instance()).GetCurrentWard() + 1);

	private unsafe short CurrentPlot => (short)(((HousingManager)HousingManager.Instance()).GetCurrentPlot() + 1);

	private unsafe short CurrentRoom => ((HousingManager)HousingManager.Instance()).GetCurrentRoom();

	private unsafe Floor CurrentFloor
	{
		get
		{
			if (!IsInPlotInside)
			{
				return Floor.None;
			}
			return ((IndoorTerritory)((HousingManager)HousingManager.Instance()).IndoorTerritory).CurrentFloor switch
			{
				1u => Floor.Top, 
				0u => Floor.Ground, 
				10u => Floor.Basement, 
				_ => Floor.None, 
			};
		}
	}

	public bool IsInWard => CurrentWard > 0;

	public bool IsInPlot => CurrentPlot > 0;

	public bool IsInRoom => CurrentRoom > 0;

	public unsafe bool IsInside => ((HousingManager)HousingManager.Instance()).IsInside();

	public unsafe bool IsOutside => ((HousingManager)HousingManager.Instance()).IsOutside();

	public unsafe bool IsInWorkshop => ((HousingManager)HousingManager.Instance()).IsInWorkshop();

	public bool IsInWardArea
	{
		get
		{
			if (IsInWard && !IsInPlot)
			{
				return IsOutside;
			}
			return false;
		}
	}

	public bool IsInPlotOutside
	{
		get
		{
			if (IsInWard && IsInPlot)
			{
				return IsOutside;
			}
			return false;
		}
	}

	public bool IsInPlotInside
	{
		get
		{
			if (IsInWard && IsInPlot && !IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	public bool IsInFCRoom
	{
		get
		{
			if (IsInWard && IsInPlot && IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	public bool IsInAptRoom
	{
		get
		{
			if (IsInWard && !IsInPlot && IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	public bool IsInAptLobby
	{
		get
		{
			if (IsInWard && !IsInPlot && !IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	public bool IsInNonResidentialArea
	{
		get
		{
			if (!IsInWard && !IsInPlot && !IsInRoom && !IsInside && !IsOutside)
			{
				return !IsInWorkshop;
			}
			return false;
		}
	}

	public ExcelSheet<World> WorldSheet { get; private set; }

	public ExcelSheet<TerritoryType> TerritorySheet { get; private set; }

	public event Action<bool, bool, TerritoryData?>? TerritoryChanged;

	public event Action<TerritoryData?>? TerritoryLoaded;

	public event Action<TerritoryData?>? InitialLoad;

	public override Task Initialize()
	{
		WorldSheet = Services.DataManager.GetExcelSheet<World>((ClientLanguage?)null, (string)null);
		TerritorySheet = Services.DataManager.GetExcelSheet<TerritoryType>((ClientLanguage?)null, (string)null);
		InitializeWorlds();
		InitializeTerritories();
		BuildUITerritoryList(residentialOnly: true);
		Services.ClientState.TerritoryChanged += delegate
		{
			EnsureDespawn();
		};
		return Task.CompletedTask;
	}

	private unsafe void InitializeWorlds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<World> enumerator = WorldSheet.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				World current = enumerator.Current;
				WorldDCGroupType? valueNullable = ((World)(ref current)).DataCenter.ValueNullable;
				uint? num;
				if (!valueNullable.HasValue)
				{
					num = null;
				}
				else
				{
					WorldDCGroupType valueOrDefault = valueNullable.GetValueOrDefault();
					num = ((WorldDCGroupType)(ref valueOrDefault)).Region.RowId;
				}
				uint? num2 = num;
				if (!((World)(ref current)).IsPublic || num2 < 1 || num2 > 4)
				{
					continue;
				}
				ReadOnlySeString name = ((World)(ref current)).Name;
				if (((ReadOnlySeString)(ref name)).IsEmpty)
				{
					continue;
				}
				name = ((World)(ref current)).Name;
				if (!((object)(*(ReadOnlySeString*)(&name))/*cast due to constrained. prefix*/).ToString().Contains('-'))
				{
					ushort id = (ushort)((World)(ref current)).RowId;
					Region value = (Region)num2.Value;
					if (!Worlds.ContainsKey(value))
					{
						Worlds.Add(value, new List<WorldInfo>());
					}
					List<WorldInfo> list = Worlds[value];
					name = ((World)(ref current)).Name;
					list.Add(new WorldInfo(id, ((object)(*(ReadOnlySeString*)(&name))/*cast due to constrained. prefix*/).ToString()));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (KeyValuePair<Region, List<WorldInfo>> world in Worlds)
		{
			world.Value.Sort((WorldInfo a, WorldInfo b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
		}
	}

	private void InitializeTerritories()
	{
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		int num = 40;
		List<ResidentialTerritory> list = new List<ResidentialTerritory>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ResidentialTerritory> span = CollectionsMarshal.AsSpan(list);
		span[0] = new ResidentialTerritory(136u, "Mist", "", ResidentialType.Ward);
		span[1] = new ResidentialTerritory(282u, "Mist", "Private Cottage", ResidentialType.House);
		span[2] = new ResidentialTerritory(283u, "Mist", "Private House", ResidentialType.House);
		span[3] = new ResidentialTerritory(284u, "Mist", "Private Mansion", ResidentialType.House);
		span[4] = new ResidentialTerritory(384u, "Mist", "Private Chambers", ResidentialType.Chambers);
		span[5] = new ResidentialTerritory(423u, "Mist", "Company Workshop", ResidentialType.Workshop);
		span[6] = new ResidentialTerritory(573u, "Mist", "Apartment Lobby", ResidentialType.ApartmentLobby);
		span[7] = new ResidentialTerritory(608u, "Mist", "Apartment", ResidentialType.Apartment);
		span[8] = new ResidentialTerritory(340u, "Lavender Beds", "", ResidentialType.Ward);
		span[9] = new ResidentialTerritory(342u, "Lavender Beds", "Private Cottage", ResidentialType.House);
		span[10] = new ResidentialTerritory(343u, "Lavender Beds", "Private House", ResidentialType.House);
		span[11] = new ResidentialTerritory(344u, "Lavender Beds", "Private Mansion", ResidentialType.House);
		span[12] = new ResidentialTerritory(385u, "Lavender Beds", "Private Chambers", ResidentialType.Chambers);
		span[13] = new ResidentialTerritory(425u, "Lavender Beds", "Company Workshop", ResidentialType.Workshop);
		span[14] = new ResidentialTerritory(574u, "Lavender Beds", "Apartment Lobby", ResidentialType.ApartmentLobby);
		span[15] = new ResidentialTerritory(609u, "Lavender Beds", "Apartment", ResidentialType.Apartment);
		span[16] = new ResidentialTerritory(341u, "Goblet", "", ResidentialType.Ward);
		span[17] = new ResidentialTerritory(345u, "Goblet", "Private Cottage", ResidentialType.House);
		span[18] = new ResidentialTerritory(346u, "Goblet", "Private House", ResidentialType.House);
		span[19] = new ResidentialTerritory(347u, "Goblet", "Private Mansion", ResidentialType.House);
		span[20] = new ResidentialTerritory(386u, "Goblet", "Private Chambers", ResidentialType.Chambers);
		span[21] = new ResidentialTerritory(424u, "Goblet", "Company Workshop", ResidentialType.Workshop);
		span[22] = new ResidentialTerritory(575u, "Goblet", "Apartment Lobby", ResidentialType.ApartmentLobby);
		span[23] = new ResidentialTerritory(610u, "Goblet", "Apartment", ResidentialType.Apartment);
		span[24] = new ResidentialTerritory(641u, "Shirogane", "", ResidentialType.Ward);
		span[25] = new ResidentialTerritory(649u, "Shirogane", "Private Cottage", ResidentialType.House);
		span[26] = new ResidentialTerritory(650u, "Shirogane", "Private House", ResidentialType.House);
		span[27] = new ResidentialTerritory(651u, "Shirogane", "Private Mansion", ResidentialType.House);
		span[28] = new ResidentialTerritory(652u, "Shirogane", "Private Chambers", ResidentialType.Chambers);
		span[29] = new ResidentialTerritory(653u, "Shirogane", "Company Workshop", ResidentialType.Workshop);
		span[30] = new ResidentialTerritory(654u, "Shirogane", "Apartment Lobby", ResidentialType.ApartmentLobby);
		span[31] = new ResidentialTerritory(655u, "Shirogane", "Apartment", ResidentialType.Apartment);
		span[32] = new ResidentialTerritory(979u, "Empyreum", "", ResidentialType.Ward);
		span[33] = new ResidentialTerritory(980u, "Empyreum", "Private Cottage", ResidentialType.House);
		span[34] = new ResidentialTerritory(981u, "Empyreum", "Private House", ResidentialType.House);
		span[35] = new ResidentialTerritory(982u, "Empyreum", "Private Mansion", ResidentialType.House);
		span[36] = new ResidentialTerritory(983u, "Empyreum", "Private Chambers", ResidentialType.Chambers);
		span[37] = new ResidentialTerritory(984u, "Empyreum", "Company Workshop", ResidentialType.Workshop);
		span[38] = new ResidentialTerritory(985u, "Empyreum", "Apartment Lobby", ResidentialType.ApartmentLobby);
		span[39] = new ResidentialTerritory(999u, "Empyreum", "Apartment", ResidentialType.Apartment);
		ResidentialTerritories = list;
		Enumerator<TerritoryType> enumerator = TerritorySheet.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				TerritoryType t = enumerator.Current;
				PlaceName? valueNullable = ((TerritoryType)(ref t)).PlaceName.ValueNullable;
				object obj;
				if (!valueNullable.HasValue)
				{
					obj = null;
				}
				else
				{
					PlaceName valueOrDefault = valueNullable.GetValueOrDefault();
					ReadOnlySeString name = ((PlaceName)(ref valueOrDefault)).Name;
					obj = ((ReadOnlySeString)(ref name)).ExtractText();
				}
				string text = (string)obj;
				if (!string.IsNullOrWhiteSpace(text) && ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == ((TerritoryType)(ref t)).RowId) == null)
				{
					NonResidentialTerritories.Add(new NonResidentialTerritory((ushort)((TerritoryType)(ref t)).RowId, text));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		NonResidentialTerritories.Sort((NonResidentialTerritory a, NonResidentialTerritory b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
	}

	public void BuildUITerritoryList(bool residentialOnly)
	{
		UITerritoryList.Clear();
		foreach (ResidentialTerritory residentialTerritory in ResidentialTerritories)
		{
			UITerritoryList.Add((residentialTerritory.Id, string.IsNullOrEmpty(residentialTerritory.SubName) ? residentialTerritory.Name : (residentialTerritory.Name + " - " + residentialTerritory.SubName), true));
		}
		if (residentialOnly)
		{
			return;
		}
		foreach (NonResidentialTerritory item in NonResidentialTerritories.OrderBy<NonResidentialTerritory, string>((NonResidentialTerritory x) => x.Name, StringComparer.OrdinalIgnoreCase))
		{
			UITerritoryList.Add((item.Id, item.Name, false));
		}
	}

	public unsafe void Update()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!Services.ClientState.IsLoggedIn || !Services.PlayerState.IsLoaded || Services.Condition[(ConditionFlag)45])
		{
			EnsureDespawn();
			return;
		}
		IPlayerCharacter localPlayer = Services.Objects.LocalPlayer;
		if (localPlayer == null)
		{
			EnsureDespawn();
			return;
		}
		Character* address = (Character*)((IGameObject)localPlayer).Address;
		if (address == null || ((Character)address).DrawObject == null)
		{
			return;
		}
		long contentId = (long)Services.PlayerState.ContentId;
		if (contentId != 0L)
		{
			if (CurrentTerritory == null)
			{
				CurrentTerritory = new TerritoryData();
			}
			LocalPlayerContentId = contentId;
			LocalPlayerExists = true;
			LocalPlayerPosition = ((IGameObject)localPlayer).Position;
			LocalPlayerRotation = Quaternion.op_Implicit(((DrawObject)((Character)address).DrawObject).Rotation);
			CurrentTerritory.Ward = CurrentWard;
			CurrentTerritory.Plot = CurrentPlot;
			CurrentTerritory.Room = CurrentRoom;
			CurrentTerritory.Floor = CurrentFloor;
			if (CurrentTerritory.WorldId != ((Character)address).CurrentWorld)
			{
				CurrentTerritory.WorldId = ((Character)address).CurrentWorld;
				CurrentTerritory.WorldName = GetWorldName(CurrentTerritory.WorldId);
			}
			if (CurrentTerritory.TerritoryId != Services.ClientState.TerritoryType && CurrentTerritory.RawTerritoryId != Services.ClientState.TerritoryType)
			{
				CurrentTerritory.RawTerritoryId = Services.ClientState.TerritoryType;
				CurrentTerritory.TerritoryId = GetCurrentTerritoryId();
				CurrentTerritory.TerritoryName = GetTerritoryName(CurrentTerritory.TerritoryId);
				CurrentTerritory.TerritorySubName = GetTerritorySubName(CurrentTerritory.TerritoryId, CurrentTerritory.Plot > 0);
			}
			if (!CurrentTerritory.Matches(PreviousTerritory, persistent: false))
			{
				IsLoadingTerritory = !((Character)address).GetIsTargetable();
				PreviousTerritory = new TerritoryData(CurrentTerritory);
				this.TerritoryChanged?.Invoke(arg1: false, IsLoadingTerritory, CurrentTerritory);
			}
			if (IsLoadingTerritory && ((Character)address).GetIsTargetable())
			{
				IsLoadingTerritory = false;
				this.TerritoryLoaded?.Invoke(CurrentTerritory);
			}
			if (IsInitialLoad)
			{
				IsInitialLoad = false;
				this.InitialLoad?.Invoke(CurrentTerritory);
			}
		}
	}

	public Region GetRegionFromWorld(uint worldId)
	{
		return Worlds.FirstOrDefault<KeyValuePair<Region, List<WorldInfo>>>((KeyValuePair<Region, List<WorldInfo>> w) => w.Value.Any((WorldInfo x) => x.Id == worldId)).Key;
	}

	public string GetWorldName(uint worldId)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		World? rowOrDefault = Services.DataManager.GetExcelSheet<World>((ClientLanguage?)null, (string)null).GetRowOrDefault(worldId);
		if (!rowOrDefault.HasValue)
		{
			return "";
		}
		World value = rowOrDefault.Value;
		return ((object)((World)(ref value)).Name/*cast due to constrained. prefix*/).ToString();
	}

	private unsafe uint GetCurrentTerritoryId()
	{
		HousingManager* ptr = HousingManager.Instance();
		if (ptr != null && ((HousingManager)ptr).IsInside())
		{
			return HousingManager.GetOriginalHouseTerritoryTypeId();
		}
		return Services.ClientState.TerritoryType;
	}

	public string GetTerritoryName(uint territoryId)
	{
		ResidentialTerritory residentialTerritory = ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == territoryId);
		if (residentialTerritory != null)
		{
			return residentialTerritory.Name;
		}
		return NonResidentialTerritories.FirstOrDefault((NonResidentialTerritory x) => x.Id == territoryId)?.Name ?? $"Unknown ({territoryId})";
	}

	public string GetTerritorySubName(uint territoryId, bool isPlot)
	{
		ResidentialTerritory residentialTerritory = ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == territoryId);
		if (residentialTerritory == null)
		{
			return string.Empty;
		}
		bool flag = string.IsNullOrEmpty(residentialTerritory.SubName);
		if (!(flag && isPlot))
		{
			if (!flag)
			{
				return residentialTerritory.SubName;
			}
			return string.Empty;
		}
		return "Garden";
	}

	private void EnsureDespawn()
	{
		LocalPlayerExists = false;
		if (!(PreviousTerritory == null))
		{
			CurrentTerritory = null;
			PreviousTerritory = null;
			this.TerritoryChanged?.Invoke(arg1: true, arg2: false, null);
		}
	}

	public TerritoryData GetTerritoryData(TerritoryPixProperties t, bool persistent)
	{
		return new TerritoryData
		{
			WorldId = t.WorldId,
			TerritoryId = t.TerritoryId,
			Ward = t.Ward,
			Plot = t.Plot,
			Room = (short)((!persistent) ? t.Room : 0),
			Floor = ((!persistent) ? t.Floor : Floor.None),
			WorldName = GetWorldName(t.WorldId),
			TerritoryName = GetTerritoryName(t.TerritoryId),
			TerritorySubName = GetTerritorySubName(t.TerritoryId, t.Plot > 0)
		};
	}

	public string GetResidenceFormatted(TerritoryData t)
	{
		string text = string.Empty;
		if (t.Ward > 0)
		{
			text += $"W{t.Ward}";
		}
		if (t.Plot > 0)
		{
			text += $" P{t.Plot}";
		}
		if (t.Room > 0)
		{
			text += $" R{t.Room}";
		}
		if (t.Floor != Floor.None)
		{
			text += $" F{(uint)(t.Floor - 1)}";
		}
		return text;
	}

	public override Task Dispose()
	{
		EnsureDespawn();
		return Task.CompletedTask;
	}
}

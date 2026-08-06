using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Structs.Pix.Properties;
using PyonPix.Shared.Structs.Territory;
using PyonPix.Shared.Sync.Dto;
using PyonPix.Shared.Sync.Dto.Subbed;
using PyonPix.Ui;
using PyonPix.Utility;

namespace PyonPix.Services.Core;

public class PixService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private readonly List<string> TerritoryActivationOrder = new List<string>();

	private const string PixClipboardPrefix = "PX1:";

	public readonly Dictionary<string, IPix> SpawnedPixs = new Dictionary<string, IPix>();

	public readonly ConcurrentDictionary<string, SyncedPix> SyncedPixs = new ConcurrentDictionary<string, SyncedPix>();

	public Dictionary<long, Dictionary<string, PixVariant>> PixVariants = new Dictionary<long, Dictionary<string, PixVariant>>();

	private static readonly TimeSpan SyncedVariantRetention = TimeSpan.FromDays(7);

	private const string Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		IncludeFields = true
	};

	private StateService? StateService => Services.Get<StateService>();

	private DataService? DataService => Services.Get<DataService>();

	public int PixSpawnLimit => Config.Global.General.PixSpawnLimit;

	public List<LocalPix> LocalPixs => Config.LocalPixs;

	public event Action<IPix, bool>? PixSpawned;

	public event Action<PixUpdate>? PixUpdated;

	public event Action<IPix, bool>? PixDespawned;

	public event Action? AllPixDespawned;

	public Dictionary<string, PixVariant> GetPixVariantsForCurrentCharacter()
	{
		if (StateService == null)
		{
			return new Dictionary<string, PixVariant>();
		}
		long localPlayerContentId = StateService.LocalPlayerContentId;
		if (!Config.PixVariants.TryGetValue(localPlayerContentId, out Dictionary<string, PixVariant> value))
		{
			value = new Dictionary<string, PixVariant>();
			Config.PixVariants[localPlayerContentId] = value;
		}
		return value;
	}

	public override Task Initialize()
	{
		StateService? stateService = StateService;
		if (stateService != null)
		{
			stateService.TerritoryChanged += delegate(bool isUnload, bool isTerritoryLoading, TerritoryData? territory)
			{
				if (isUnload)
				{
					DespawnAll();
				}
				else
				{
					ReevaluateCurrentTerritory(isUserAction: false, isTerritoryLoading);
				}
			};
		}
		StateService? stateService2 = StateService;
		if (stateService2 != null)
		{
			stateService2.TerritoryLoaded += delegate
			{
				ReevaluateCurrentTerritory(isUserAction: false, isTerritoryLoading: false);
			};
		}
		JsonSerializer.Serialize(new Pix(), JsonOptions);
		return Task.CompletedTask;
	}

	public bool IsActive(IPix? pix)
	{
		return GetVariant(pix)?.Active ?? false;
	}

	public bool IsSpawned(IPix? pix)
	{
		if (pix != null)
		{
			return SpawnedPixs.ContainsKey(ResolveRuntimePix(pix).Id);
		}
		return false;
	}

	public bool IsSubscribed(string? pixId)
	{
		if (!string.IsNullOrWhiteSpace(pixId))
		{
			return SyncedPixs.ContainsKey(pixId);
		}
		return false;
	}

	public void Enable(IPix pix)
	{
		pix = ResolveRuntimePix(pix);
		PixVariant? variant = GetVariant(pix, create: true);
		variant.Active = true;
		variant.LastSeenUtc = DateTime.UtcNow;
		TerritoryData territoryData = StateService?.CurrentTerritory;
		if (territoryData != null && pix.Territory.Matches(territoryData, (pix as BasePix)?.Territory.Persistent ?? false))
		{
			TerritoryActivationOrder.Remove(pix.Id);
			TerritoryActivationOrder.Add(pix.Id);
		}
		Config.Save();
		ReevaluateCurrentTerritory(isUserAction: true, isTerritoryLoading: false);
	}

	public void Disable(IPix pix)
	{
		pix = ResolveRuntimePix(pix);
		PixVariant? variant = GetVariant(pix, create: true);
		variant.Active = false;
		variant.LastSeenUtc = DateTime.UtcNow;
		if (SpawnedPixs.TryGetValue(pix.Id, out IPix value))
		{
			SpawnedPixs.Remove(pix.Id);
			this.PixDespawned?.Invoke(value, arg2: true);
		}
		TerritoryActivationOrder.Remove(pix.Id);
		Config.Save();
		ReevaluateCurrentTerritory(isUserAction: true, isTerritoryLoading: false);
	}

	public void Toggle(IPix pix)
	{
		pix = ResolveRuntimePix(pix);
		if (IsActive(pix))
		{
			Disable(pix);
		}
		else
		{
			Enable(pix);
		}
	}

	public void Toggle(string pixId)
	{
		pixId = pixId.ToUpper();
		if (pixId.StartsWith("PIX") || pixId.StartsWith("PXS"))
		{
			IPix pix = GetRuntimePixs().FirstOrDefault((IPix x) => x.Id == pixId);
			if (pix != null)
			{
				Toggle(pix);
			}
		}
	}

	public IPix? GetPix(string? pixId)
	{
		if (pixId == null)
		{
			return null;
		}
		if (SyncedPixs.TryGetValue(pixId, out SyncedPix value))
		{
			return value;
		}
		return LocalPixs.FirstOrDefault((LocalPix x) => x.Id == pixId);
	}

	public PixVariant GetVariant(string pixId, bool create = false)
	{
		Dictionary<string, PixVariant> pixVariantsForCurrentCharacter = GetPixVariantsForCurrentCharacter();
		if (pixVariantsForCurrentCharacter.TryGetValue(pixId, out var value))
		{
			return value;
		}
		if (!create)
		{
			return null;
		}
		return pixVariantsForCurrentCharacter[pixId] = new PixVariant
		{
			LastSeenUtc = DateTime.UtcNow
		};
	}

	public PixVariant? GetVariant(IPix? pix, bool create = false)
	{
		if (pix == null)
		{
			return null;
		}
		string id = ResolveRuntimePix(pix).Id;
		Dictionary<string, PixVariant> pixVariantsForCurrentCharacter = GetPixVariantsForCurrentCharacter();
		if (pixVariantsForCurrentCharacter.TryGetValue(id, out var value))
		{
			return value;
		}
		if (!create)
		{
			return null;
		}
		return pixVariantsForCurrentCharacter[id] = new PixVariant
		{
			LastSeenUtc = DateTime.UtcNow
		};
	}

	public PixVariant? TryGetVariant(string pixId)
	{
		if (!GetPixVariantsForCurrentCharacter().TryGetValue(pixId, out PixVariant value))
		{
			return null;
		}
		return value;
	}

	public PixVariant? TryGetVariant(IPix? pix)
	{
		if (pix != null)
		{
			return TryGetVariant(ResolveRuntimePix(pix).Id);
		}
		return null;
	}

	public PixVariant EnsureVariant(string pixId)
	{
		Dictionary<string, PixVariant> pixVariantsForCurrentCharacter = GetPixVariantsForCurrentCharacter();
		if (pixVariantsForCurrentCharacter.TryGetValue(pixId, out var value))
		{
			return value;
		}
		return pixVariantsForCurrentCharacter[pixId] = new PixVariant
		{
			LastSeenUtc = DateTime.UtcNow
		};
	}

	public PixVariant EnsureVariant(IPix pix)
	{
		return EnsureVariant(ResolveRuntimePix(pix).Id);
	}

	private void SaveVariant(PixVariant variant, bool persist = true)
	{
		variant.LastSeenUtc = DateTime.UtcNow;
		variant.PruneEmpty();
		if (persist)
		{
			Config.Save();
		}
	}

	private void ReevaluateCurrentTerritory(bool isUserAction, bool isTerritoryLoading)
	{
		CleanupPixVariants();
		TerritoryData territory = StateService?.CurrentTerritory;
		if (territory == null)
		{
			DespawnAll();
			return;
		}
		List<IPix> pixs = (from p in (from p in GetActivePixs()
				where p.Territory.Matches(territory, (p as BasePix)?.Territory.Persistent ?? false)
				select p).ToList()
			orderby TerritoryActivationOrder.IndexOf(p.Id) descending
			select p).ToList().Take(PixSpawnLimit).ToList();
		ApplySpawnSet(pixs, isUserAction, isTerritoryLoading);
	}

	private List<IPix> GetActivePixs()
	{
		Dictionary<string, IPix> dictionary = GetRuntimePixs().ToDictionary((IPix p) => p.Id);
		List<IPix> list = new List<IPix>();
		foreach (KeyValuePair<string, PixVariant> item in GetPixVariantsForCurrentCharacter())
		{
			if (item.Value.Active && dictionary.TryGetValue(item.Key, out var value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	private IEnumerable<IPix> GetRuntimePixs()
	{
		foreach (LocalPix localPix in LocalPixs)
		{
			if (!localPix.Sync.IsSynced)
			{
				yield return localPix;
			}
		}
		foreach (SyncedPix value in SyncedPixs.Values)
		{
			yield return value;
		}
	}

	private IPix ResolveRuntimePix(IPix pix)
	{
		if (!(pix is BasePix basePix))
		{
			return pix;
		}
		if (!basePix.Sync.IsSynced)
		{
			return pix;
		}
		if (string.IsNullOrWhiteSpace(basePix.Sync.SyncedPixId))
		{
			return pix;
		}
		if (!SyncedPixs.TryGetValue(basePix.Sync.SyncedPixId, out SyncedPix value))
		{
			return pix;
		}
		return value;
	}

	private void ApplySpawnSet(List<IPix> pixs, bool isUserAction, bool isTerritoryLoading)
	{
		HashSet<string> hashSet = pixs.Select((IPix p) => p.Id).ToHashSet();
		foreach (KeyValuePair<string, IPix> item in SpawnedPixs.ToList())
		{
			if (!hashSet.Contains(item.Key))
			{
				SpawnedPixs.Remove(item.Key);
				this.PixDespawned?.Invoke(item.Value, isUserAction);
			}
		}
		if (isTerritoryLoading)
		{
			return;
		}
		foreach (IPix pix in pixs)
		{
			if (!SpawnedPixs.ContainsKey(pix.Id))
			{
				SpawnedPixs[pix.Id] = pix;
				this.PixSpawned?.Invoke(pix, isUserAction);
				TerritoryActivationOrder.Remove(pix.Id);
				TerritoryActivationOrder.Add(pix.Id);
			}
		}
	}

	private void DespawnAll()
	{
		if (SpawnedPixs.Count == 0)
		{
			return;
		}
		foreach (IPix value in SpawnedPixs.Values)
		{
			this.PixDespawned?.Invoke(value, arg2: false);
		}
		SpawnedPixs.Clear();
		TerritoryActivationOrder.Clear();
		this.AllPixDespawned?.Invoke();
	}

	public IReadOnlyList<TerritoryData>? GetPixTerritories()
	{
		if (StateService == null)
		{
			return null;
		}
		HashSet<TerritoryData> hashSet = new HashSet<TerritoryData>();
		foreach (IPix runtimePix in GetRuntimePixs())
		{
			hashSet.Add(StateService.GetTerritoryData(runtimePix.Territory, persistent: true));
		}
		return hashSet.ToList();
	}

	public IReadOnlyList<IPix> GetOrderedPixsForTerritory(TerritoryData territory, bool persistent)
	{
		return (from p in (from p in GetRuntimePixs()
				where p.Territory.Matches(territory, persistent)
				select p).ToList()
			orderby IsActive(p) descending
			select p).ToList();
	}

	public IPix CreateLocalPix()
	{
		LocalPix localPix = new LocalPix(GenerateId(), StateService);
		LocalPixs.Add(localPix);
		Enable(localPix);
		return localPix;
	}

	public void DeleteLocalPix(IPix? pix)
	{
		if (pix != null && !IsSpawned(pix) && pix is LocalPix item)
		{
			if (IsActive(pix))
			{
				Disable(pix);
			}
			LocalPixs.Remove(item);
			Config.Save();
		}
	}

	private void PromoteVariantToSynced(string localPixId, string syncedPixId)
	{
		Dictionary<string, PixVariant> pixVariantsForCurrentCharacter = GetPixVariantsForCurrentCharacter();
		if (string.IsNullOrWhiteSpace(localPixId) || string.IsNullOrWhiteSpace(syncedPixId))
		{
			return;
		}
		pixVariantsForCurrentCharacter.TryGetValue(localPixId, out var value);
		pixVariantsForCurrentCharacter.TryGetValue(syncedPixId, out var value2);
		if (value != null)
		{
			pixVariantsForCurrentCharacter.Remove(localPixId);
		}
		PixVariant pixVariant = value2 ?? value ?? new PixVariant();
		if (value2 != null && value != null)
		{
			pixVariant.Active = pixVariant.Active || value.Active;
			pixVariant.PersistentCache = pixVariant.PersistentCache || value.PersistentCache;
			pixVariant.SyncCookies = value.SyncCookies;
			pixVariant.ScreenInteraction = value.ScreenInteraction;
			PixVariant pixVariant2 = pixVariant;
			if (pixVariant2.Browser == null)
			{
				pixVariant2.Browser = value.Browser;
			}
			pixVariant2 = pixVariant;
			if (pixVariant2.Renderer == null)
			{
				pixVariant2.Renderer = value.Renderer;
			}
			pixVariant2 = pixVariant;
			if (pixVariant2.Light == null)
			{
				pixVariant2.Light = value.Light;
			}
			pixVariant2 = pixVariant;
			if (pixVariant2.Audio == null)
			{
				pixVariant2.Audio = value.Audio;
			}
		}
		pixVariant.IsSynced = true;
		pixVariant.LastSeenUtc = DateTime.UtcNow;
		pixVariant.PruneEmpty();
		pixVariantsForCurrentCharacter[syncedPixId] = pixVariant;
	}

	private void DemoteVariantToLocal(string syncedPixId, string localPixId)
	{
		Dictionary<string, PixVariant> pixVariantsForCurrentCharacter = GetPixVariantsForCurrentCharacter();
		if (!string.IsNullOrWhiteSpace(localPixId) && !string.IsNullOrWhiteSpace(syncedPixId))
		{
			if (pixVariantsForCurrentCharacter.TryGetValue(syncedPixId, out var value))
			{
				pixVariantsForCurrentCharacter.Remove(syncedPixId);
			}
			else
			{
				value = new PixVariant();
			}
			value.IsSynced = false;
			value.LastSeenUtc = DateTime.UtcNow;
			value.PruneEmpty();
			pixVariantsForCurrentCharacter[localPixId] = value;
		}
	}

	public SyncedPix? CreateSyncedPix(LocalPix localPix, SyncedPixCreateDto request, SyncedPixCreateSuccessDto result)
	{
		return CreateSyncedPixAsync(localPix, request, result).GetAwaiter().GetResult();
	}

	public async Task<SyncedPix?> CreateSyncedPixAsync(LocalPix localPix, SyncedPixCreateDto request, SyncedPixCreateSuccessDto result)
	{
		if (localPix == null)
		{
			return null;
		}
		bool wasActive = IsActive(localPix);
		if (wasActive)
		{
			Disable(localPix);
		}
		if (DataService != null)
		{
			await DataService.RenameUDFAsync(localPix.Id, result.PixId);
		}
		SyncedPix syncedPix = new SyncedPix
		{
			Id = result.PixId,
			SelfRank = PixRank.Owner
		};
		ApplyCreatedSyncedPixData(syncedPix, request.Pix, request.Meta);
		syncedPix.Sync.IsSynced = true;
		syncedPix.Sync.SyncedPixId = result.PixId;
		syncedPix.Sync.SecretKey = result.SecretKey;
		localPix.Sync.IsSynced = true;
		localPix.Sync.SyncedPixId = result.PixId;
		localPix.Sync.SecretKey = result.SecretKey;
		PromoteVariantToSynced(localPix.Id, result.PixId);
		SyncedPixs[result.PixId] = syncedPix;
		Config.Save();
		if (wasActive)
		{
			Enable(syncedPix);
		}
		return syncedPix;
	}

	public async Task<LocalPix?> RemoveSyncedPixAsync(string syncedPixId)
	{
		if (string.IsNullOrWhiteSpace(syncedPixId))
		{
			return null;
		}
		SyncedPixs.TryGetValue(syncedPixId, out SyncedPix value);
		LocalPix linkedLocal = LocalPixs.FirstOrDefault((LocalPix x) => x.Sync.IsSynced && x.Sync.SyncedPixId == syncedPixId);
		if (linkedLocal == null && value == null)
		{
			return null;
		}
		bool wasActive = (value != null && IsActive(value)) || (value == null && ((TryGetVariant(syncedPixId)?.Active ?? false) || SpawnedPixs.ContainsKey(syncedPixId)));
		if (wasActive && value != null)
		{
			Disable(value);
		}
		if (linkedLocal != null)
		{
			if (value != null)
			{
				linkedLocal.Version = value.Version;
				linkedLocal.Info = CloneOrNew(value.Info);
				linkedLocal.Browser = CloneOrNew(value.Browser);
				linkedLocal.Territory = CloneOrNew(value.Territory);
				linkedLocal.Renderer = CloneOrNew(value.Renderer);
				linkedLocal.Light = CloneOrNew(value.Light);
				linkedLocal.Audio = CloneOrNew(value.Audio);
			}
			linkedLocal.Sync.IsSynced = false;
			linkedLocal.Sync.SyncedPixId = string.Empty;
			linkedLocal.Sync.SecretKey = null;
			DemoteVariantToLocal(syncedPixId, linkedLocal.Id);
			if (DataService != null)
			{
				await DataService.RenameUDFAsync(syncedPixId, linkedLocal.Id);
			}
		}
		else
		{
			GetPixVariantsForCurrentCharacter().Remove(syncedPixId);
		}
		SyncedPixs.Remove(syncedPixId, out var _);
		Config.Save();
		if (wasActive && linkedLocal != null)
		{
			Enable(linkedLocal);
		}
		return linkedLocal;
	}

	public void RemoveSyncedSubscription(string syncedPixId)
	{
		if (!string.IsNullOrWhiteSpace(syncedPixId))
		{
			if (SpawnedPixs.TryGetValue(syncedPixId, out IPix value))
			{
				SpawnedPixs.Remove(syncedPixId);
				this.PixDespawned?.Invoke(value, arg2: false);
			}
			SyncedPixs.Remove(syncedPixId, out var _);
			if (GetPixVariantsForCurrentCharacter().Remove(syncedPixId))
			{
				Config.Save();
			}
			ReevaluateCurrentTerritory(isUserAction: false, isTerritoryLoading: false);
		}
	}

	private void ReconcileSyncedVariantLinks()
	{
		foreach (LocalPix localPix in LocalPixs)
		{
			if (localPix.Sync.IsSynced && !string.IsNullOrWhiteSpace(localPix.Sync.SyncedPixId))
			{
				PromoteVariantToSynced(localPix.Id, localPix.Sync.SyncedPixId);
			}
		}
	}

	public void AddOrUpdateSyncedPixs(IEnumerable<SubbedPixQueryItemDto> pixs)
	{
		DateTime utcNow = DateTime.UtcNow;
		Dictionary<string, SubbedPixQueryItemDto> dictionary = pixs.ToDictionary((SubbedPixQueryItemDto x) => x.PixId);
		ReconcileSyncedVariantLinks();
		foreach (KeyValuePair<string, SyncedPix> item in SyncedPixs.ToList())
		{
			if (!dictionary.TryGetValue(item.Key, out var value))
			{
				if (SpawnedPixs.TryGetValue(item.Key, out IPix value2))
				{
					SpawnedPixs.Remove(item.Key);
					this.PixDespawned?.Invoke(value2, arg2: false);
				}
				SyncedPixs.Remove(item.Key, out var _);
			}
			else
			{
				PixVariant variant = GetVariant(item.Key, create: true);
				variant.IsSynced = true;
				variant.LastSeenUtc = utcNow;
				ApplySyncedPixState(item.Value, value, variant);
			}
		}
		foreach (SubbedPixQueryItemDto pix in pixs)
		{
			if (!SyncedPixs.ContainsKey(pix.PixId))
			{
				PixVariant variant2 = GetVariant(pix.PixId, create: true);
				variant2.IsSynced = true;
				variant2.LastSeenUtc = utcNow;
				SyncedPix syncedPix = new SyncedPix
				{
					Id = pix.PixId,
					OwnerAlias = pix.OwnerAlias,
					OwnerAliasStyle = pix.OwnerAliasStyle,
					OwnerPixStyle = pix.OwnerPixStyle,
					SelfRank = pix.SelfRank
				};
				ApplySyncedPixState(syncedPix, pix, variant2);
				SyncedPixs[pix.PixId] = syncedPix;
			}
		}
		ReevaluateCurrentTerritory(isUserAction: false, isTerritoryLoading: false);
	}

	public void AddOrUpdateSyncedPix(SubbedPixQueryItemDto? pix)
	{
		if (pix != null)
		{
			DateTime utcNow = DateTime.UtcNow;
			ReconcileSyncedVariantLinks();
			if (!SyncedPixs.ContainsKey(pix.PixId))
			{
				PixVariant variant = GetVariant(pix.PixId, create: true);
				variant.IsSynced = true;
				variant.LastSeenUtc = utcNow;
				SyncedPix syncedPix = new SyncedPix
				{
					Id = pix.PixId,
					SelfRank = pix.SelfRank
				};
				ApplySyncedPixState(syncedPix, pix, variant);
				SyncedPixs[pix.PixId] = syncedPix;
				Enable(syncedPix);
			}
		}
	}

	public bool CanSyncEdit(IPix? pix)
	{
		if (pix is SyncedPix syncedPix)
		{
			return syncedPix.CanSyncEdit;
		}
		return false;
	}

	public void UpdateUri(IPix? pix, PixUpdateOrigin origin = PixUpdateOrigin.Local, bool performLocalUpdate = true)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.Uri, origin, editFinished: true, saveConfig: true, raiseEvent: true, performLocalUpdate);
		}
	}

	public void UpdateMediaState(IPix? pix, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.MediaState, origin, editFinished: true, saveConfig: false);
		}
	}

	public void UpdateTerritory(IPix? pix, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.Territory, origin, editFinished: true);
		}
	}

	public void UpdateBrowserProperties(IPix? pix, bool editFinished, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.BrowserProperties, origin, editFinished);
		}
	}

	public void UpdateRendererTransform(IPix? pix, bool editFinished, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.RendererTransform, origin, editFinished);
		}
	}

	public void UpdateRendererProperties(IPix? pix, bool editFinished, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.RendererProperties, origin, editFinished);
		}
	}

	public void UpdateLightTransform(IPix? pix, bool editFinished, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.LightTransform, origin, editFinished);
		}
	}

	public void UpdateLightProperties(IPix? pix, bool editFinished = true, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.LightProperties, origin, editFinished);
		}
	}

	public void UpdateAudio(IPix? pix, bool editFinished = true, PixUpdateOrigin origin = PixUpdateOrigin.Local)
	{
		if (pix != null)
		{
			PublishUpdate(pix, PixUpdateType.AudioProperties, origin, editFinished);
		}
	}

	private void PublishUpdate(IPix pix, PixUpdateType type, PixUpdateOrigin origin, bool editFinished, bool saveConfig = true, bool raiseEvent = true, bool performLocalUpdate = true)
	{
		if (saveConfig && editFinished)
		{
			Config.Save();
		}
		if (raiseEvent)
		{
			this.PixUpdated?.Invoke(new PixUpdate(pix, type, origin, editFinished, performLocalUpdate));
		}
	}

	public PixDto BuildPixDto(IPix pix)
	{
		return new PixDto
		{
			Version = pix.Version,
			Browser = pix.Browser.ToSynced(),
			Renderer = pix.Renderer.ToSynced(),
			Light = pix.Light.ToSynced(),
			Audio = pix.Audio.ToSynced()
		};
	}

	public bool ApplyPixPropertyUpdate(BaseSyncedPixUpdate update)
	{
		if (string.IsNullOrWhiteSpace(update.PixId))
		{
			return false;
		}
		if (!SyncedPixs.TryGetValue(update.PixId, out SyncedPix value))
		{
			return false;
		}
		if (value.SourcePix == null)
		{
			value.SourcePix = new PixDto();
		}
		switch (update.UpdateType)
		{
		case PixUpdateType.InfoProperties:
			(update as SyncedPixUpdateInfoProperties)?.Info?.ApplyTo(value.Info);
			break;
		case PixUpdateType.Uri:
		{
			SyncedPixUpdateUri syncedPixUpdateUri = update as SyncedPixUpdateUri;
			if (!Config.Global.Browser.SyncFileScheme && BrowserUtil.IsFileScheme(syncedPixUpdateUri?.Uri))
			{
				return false;
			}
			value.SourcePix.Browser.Uri = syncedPixUpdateUri?.Uri ?? value.SourcePix.Browser.Uri;
			break;
		}
		case PixUpdateType.BrowserProperties:
			value.SourcePix.Browser = CloneOrNew((update as SyncedPixUpdateBrowserProperties)?.Browser);
			break;
		case PixUpdateType.MediaState:
			value.Media = CloneOrNew((update as SyncedPixUpdateMediaState)?.Media);
			break;
		case PixUpdateType.RendererTransform:
		case PixUpdateType.RendererProperties:
			value.SourcePix.Renderer = CloneOrNew((update as SyncedPixUpdateRendererProperties)?.Renderer);
			break;
		case PixUpdateType.LightTransform:
		case PixUpdateType.LightProperties:
			value.SourcePix.Light = CloneOrNew((update as SyncedPixUpdateLightProperties)?.Light);
			break;
		case PixUpdateType.AudioProperties:
			value.SourcePix.Audio = CloneOrNew((update as SyncedPixUpdateAudioProperties)?.Audio);
			break;
		case PixUpdateType.SyncProperties:
			(update as SyncedPixUpdateSyncProperties)?.Sync?.ApplyTo(value.Sync);
			break;
		default:
		{
			SyncedPixUpdate syncedPixUpdate = update as SyncedPixUpdate;
			(update as SyncedPixUpdateInfoProperties)?.Info?.ApplyTo(value.Info);
			value.SourcePix.Browser = CloneOrNew(syncedPixUpdate?.Browser);
			value.SourcePix.Renderer = CloneOrNew(syncedPixUpdate?.Renderer);
			value.SourcePix.Light = CloneOrNew(syncedPixUpdate?.Light);
			value.SourcePix.Audio = CloneOrNew(syncedPixUpdate?.Audio);
			(update as SyncedPixUpdateSyncProperties)?.Sync?.ApplyTo(value.Sync);
			break;
		}
		}
		RebuildSyncedEffectiveState(value);
		Config.Save();
		if (IsSpawned(value))
		{
			this.PixUpdated?.Invoke(new PixUpdate(value, update.UpdateType, PixUpdateOrigin.Remote));
		}
		return true;
	}

	public void ApplyPixStyleUpdate(SubbedPixStyleUpdateDto styleUpdate)
	{
		foreach (SyncedPix value in SyncedPixs.Values)
		{
			if (value.OwnerId == styleUpdate.OwnerId)
			{
				value.OwnerAlias = styleUpdate.OwnerAlias;
				value.OwnerAliasStyle = styleUpdate.OwnerAliasStyle;
				value.OwnerPixStyle = styleUpdate.OwnerPixStyle;
			}
		}
	}

	private static PixDto ClonePixDto(PixDto source)
	{
		return new PixDto
		{
			Version = source.Version,
			Browser = CloneOrNew(source.Browser),
			Renderer = CloneOrNew(source.Renderer),
			Light = CloneOrNew(source.Light),
			Audio = CloneOrNew(source.Audio)
		};
	}

	private void ApplyCreatedSyncedPixData(BasePix target, PixDto source, SyncedPixMetaDto meta)
	{
		target.Version = source.Version;
		meta.ApplyTo(target.Info, target.Sync);
		meta.Territory?.ApplyTo(target.Territory);
		source.Browser?.ApplyTo(target.Browser);
		source.Renderer?.ApplyTo(target.Renderer);
		source.Light?.ApplyTo(target.Light);
		source.Audio?.ApplyTo(target.Audio);
		if (target is SyncedPix syncedPix)
		{
			syncedPix.SourcePix = ClonePixDto(source);
		}
	}

	private void ApplySyncedPixState(BasePix target, SubbedPixQueryItemDto source, PixVariant variant)
	{
		source.Meta.ApplyTo(target.Info, target.Sync);
		source.Meta.Territory.ApplyTo(target.Territory);
		if (target is SyncedPix syncedPix)
		{
			syncedPix.OwnerId = source.OwnerId;
			syncedPix.OwnerAlias = source.OwnerAlias;
			syncedPix.OwnerAliasStyle = source.OwnerAliasStyle;
			syncedPix.OwnerPixStyle = source.OwnerPixStyle;
			syncedPix.SelfRank = source.SelfRank;
			syncedPix.SourcePix = ClonePixDto(source.Pix);
		}
		source.Pix.Browser?.ApplyTo(target.Browser);
		source.Pix.Renderer?.ApplyTo(target.Renderer);
		source.Pix.Light?.ApplyTo(target.Light);
		source.Pix.Audio?.ApplyTo(target.Audio);
		variant.Browser?.ApplyTo(target.Browser);
		variant.Renderer?.ApplyTo(target.Renderer);
		variant.Light?.ApplyTo(target.Light);
		variant.Audio?.ApplyTo(target.Audio);
		target.Sync.IsSynced = true;
	}

	private void RebuildSyncedEffectiveState(SyncedPix synced)
	{
		synced.SourcePix.Browser?.ApplyTo(synced.Browser);
		synced.SourcePix.Renderer?.ApplyTo(synced.Renderer);
		synced.SourcePix.Light?.ApplyTo(synced.Light);
		synced.SourcePix.Audio?.ApplyTo(synced.Audio);
		PixVariant pixVariant = TryGetVariant(synced);
		if (pixVariant != null && pixVariant.Browser?.HasAny == true)
		{
			pixVariant?.Browser?.ApplyTo(synced.Browser);
		}
		if (pixVariant != null && pixVariant.Renderer?.HasAny == true)
		{
			pixVariant?.Renderer?.ApplyTo(synced.Renderer);
		}
		if (pixVariant != null && pixVariant.Light?.HasAny == true)
		{
			pixVariant?.Light?.ApplyTo(synced.Light);
		}
		if (pixVariant != null && pixVariant.Audio?.HasAny == true)
		{
			pixVariant?.Audio?.ApplyTo(synced.Audio);
		}
	}

	private void CleanupPixVariants()
	{
		if (StateService == null)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		bool flag = false;
		long localPlayerContentId = StateService.LocalPlayerContentId;
		if (!Config.PixVariants.TryGetValue(localPlayerContentId, out Dictionary<string, PixVariant> value))
		{
			return;
		}
		foreach (KeyValuePair<string, PixVariant> kv in value.ToList())
		{
			PixVariant value2 = kv.Value;
			if (value2.IsSynced)
			{
				if (!value2.Active && utcNow - value2.LastSeenUtc > SyncedVariantRetention)
				{
					value.Remove(kv.Key);
					flag = true;
				}
				continue;
			}
			bool flag2 = LocalPixs.Any((LocalPix x) => x.Id == kv.Key);
			if (!value2.Active && !flag2)
			{
				value.Remove(kv.Key);
				flag = true;
			}
		}
		if (flag)
		{
			Config.Save();
		}
	}

	public string CopyPixToClipboard(IPix? pix)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (pix == null)
		{
			return string.Empty;
		}
		string s = JsonSerializer.Serialize(new Pix
		{
			Version = pix.Version,
			Info = (pix.Info ?? new InfoPixProperties()),
			Browser = (pix.Browser ?? new BrowserPixProperties()),
			Territory = (pix.Territory ?? new TerritoryPixProperties()),
			Renderer = (pix.Renderer ?? new RendererPixProperties()),
			Light = (pix.Light ?? new LightPixProperties()),
			Audio = (pix.Audio ?? new AudioPixProperties())
		}, JsonOptions);
		string text = Convert.ToBase64String(GzipCompress(Encoding.UTF8.GetBytes(s)));
		string text2 = "PX1:" + text;
		try
		{
			ImGui.SetClipboardText(ImU8String.op_Implicit(text2));
		}
		catch
		{
		}
		return text2;
	}

	public IPix? PastePixFromClipboard(IPix? target = null)
	{
		string text = ImGui.GetClipboardText();
		if (string.IsNullOrWhiteSpace(text))
		{
			try
			{
				text = ImGui.GetClipboardText() ?? string.Empty;
			}
			catch
			{
				text = string.Empty;
			}
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (!text.StartsWith("PX1:", StringComparison.Ordinal))
		{
			return null;
		}
		string s = text.Substring("PX1:".Length);
		byte[] compressed;
		try
		{
			compressed = Convert.FromBase64String(s);
		}
		catch
		{
			return null;
		}
		byte[] bytes;
		try
		{
			bytes = GzipDecompress(compressed);
		}
		catch
		{
			return null;
		}
		Pix pix;
		try
		{
			pix = JsonSerializer.Deserialize<Pix>(Encoding.UTF8.GetString(bytes), JsonOptions);
		}
		catch
		{
			return null;
		}
		if (pix == null)
		{
			return null;
		}
		if (pix.Info == null && pix.Browser == null && (pix.Renderer == null || pix.Territory == null))
		{
			return null;
		}
		if (target != null)
		{
			if (target is BasePix target2)
			{
				ApplyExportToExisting(target2, pix);
				Config.Save();
				if (IsSpawned(target))
				{
					this.PixUpdated?.Invoke(new PixUpdate(target, PixUpdateType.All, PixUpdateOrigin.Local));
				}
				return target;
			}
			return null;
		}
		LocalPix localPix = new LocalPix(GenerateId(), StateService);
		ApplyExportToExisting(localPix, pix);
		LocalPixs.Add(localPix);
		Enable(localPix);
		Config.Save();
		return localPix;
	}

	public string GenerateId()
	{
		ulong contentId = Services.PlayerState.ContentId;
		ulong ticks = (ulong)DateTime.UtcNow.Ticks;
		ulong num = (ulong)Random.Shared.NextInt64();
		ulong num2 = contentId;
		num2 ^= (ulong)((long)ticks + -7046029254386353131L);
		num2 ^= num + (num2 << 6) + (num2 >> 2);
		Span<char> span = stackalloc char[9];
		for (int i = 0; i < span.Length; i++)
		{
			num2 ^= num2 >> 12;
			num2 ^= num2 << 25;
			num2 ^= num2 >> 27;
			num2 *= 2685821657736338717L;
			span[i] = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[(int)(num2 % 36)];
		}
		return "PIX" + new string(span);
	}

	private static T CloneOrNew<T>(T? source) where T : class, new()
	{
		if (source == null)
		{
			return new T();
		}
		return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source, JsonOptions), JsonOptions) ?? new T();
	}

	private static byte[] GzipCompress(byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			gZipStream.Write(data, 0, data.Length);
		}
		return memoryStream.ToArray();
	}

	private static byte[] GzipDecompress(byte[] compressed)
	{
		using MemoryStream stream = new MemoryStream(compressed);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private void ApplyExportToExisting(BasePix target, Pix export)
	{
		target.Version = export.Version;
		target.Info = CloneOrNew(export.Info);
		target.Browser = CloneOrNew(export.Browser);
		target.Territory = CloneOrNew(export.Territory);
		target.Renderer = CloneOrNew(export.Renderer);
		target.Light = CloneOrNew(export.Light);
		target.Audio = CloneOrNew(export.Audio);
	}

	private PixFieldBinding<T> BindField<TProps, TOverrides, T>(IPix pix, Func<BasePix, TProps> livePropsSelector, Func<PixVariant, TOverrides> ensureOverrides, Func<PixVariant, TOverrides?> tryOverrides, Func<TProps, T> liveGetter, Action<TProps, T> liveSetter, Func<TOverrides, T?> overrideGetter, Action<TOverrides, T?> overrideSetter, PixUpdateType updateType) where TOverrides : class where T : struct
	{
		BasePix runtime = (ResolveRuntimePix(pix) as BasePix) ?? throw new InvalidOperationException("Pix Invalid");
		SyncedPix synced = runtime as SyncedPix;
		bool flag = synced?.CanSyncEdit ?? false;
		bool useOverride = synced != null && !flag;
		PixVariant variant = (useOverride ? EnsureVariant(runtime) : null);
		TOverrides val = ((variant != null) ? tryOverrides(variant) : null);
		TProps arg = livePropsSelector(runtime);
		bool flag2 = val != null && overrideGetter(val).HasValue;
		return new PixFieldBinding<T>(flag2 ? overrideGetter(val).Value : liveGetter(arg), flag2, flag, delegate(T value, bool editFinished)
		{
			if (useOverride)
			{
				PixVariant pixVariant = EnsureVariant(runtime);
				TOverrides arg2 = ensureOverrides(pixVariant);
				overrideSetter(arg2, value);
				SaveVariant(pixVariant, editFinished);
				if (synced != null)
				{
					RebuildSyncedEffectiveState(synced);
				}
				PublishUpdate(runtime, updateType, PixUpdateOrigin.Local, editFinished, saveConfig: false);
			}
			else
			{
				liveSetter(livePropsSelector(runtime), value);
				PublishUpdate(runtime, updateType, PixUpdateOrigin.Local, editFinished);
			}
		}, delegate(bool editFinished)
		{
			if (useOverride && variant != null)
			{
				TOverrides val2 = tryOverrides(variant);
				if (val2 != null)
				{
					overrideSetter(val2, null);
					SaveVariant(variant, editFinished);
					if (synced != null)
					{
						RebuildSyncedEffectiveState(synced);
						PublishUpdate(synced, updateType, PixUpdateOrigin.Local, editFinished, saveConfig: false);
					}
				}
			}
		});
	}

	public PixFieldBinding<T> BindBrowserField<T>(IPix pix, Func<BrowserPixProperties, T> liveGetter, Action<BrowserPixProperties, T> liveSetter, Func<BrowserPixVariantOverrides, T?> overrideGetter, Action<BrowserPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Browser, (PixVariant v) => v.EnsureBrowser(), (PixVariant v) => v.Browser, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.BrowserProperties);
	}

	public PixFieldBinding<T> BindRendererTransformField<T>(IPix pix, Func<RendererPixProperties, T> liveGetter, Action<RendererPixProperties, T> liveSetter, Func<RendererPixVariantOverrides, T?> overrideGetter, Action<RendererPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Renderer, (PixVariant v) => v.EnsureRenderer(), (PixVariant v) => v.Renderer, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.RendererTransform);
	}

	public PixFieldBinding<T> BindRendererPropertyField<T>(IPix pix, Func<RendererPixProperties, T> liveGetter, Action<RendererPixProperties, T> liveSetter, Func<RendererPixVariantOverrides, T?> overrideGetter, Action<RendererPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Renderer, (PixVariant v) => v.EnsureRenderer(), (PixVariant v) => v.Renderer, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.RendererProperties);
	}

	public PixFieldBinding<T> BindLightTransformField<T>(IPix pix, Func<LightPixProperties, T> liveGetter, Action<LightPixProperties, T> liveSetter, Func<LightPixVariantOverrides, T?> overrideGetter, Action<LightPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Light, (PixVariant v) => v.EnsureLight(), (PixVariant v) => v.Light, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.LightTransform);
	}

	public PixFieldBinding<T> BindLightPropertyField<T>(IPix pix, Func<LightPixProperties, T> liveGetter, Action<LightPixProperties, T> liveSetter, Func<LightPixVariantOverrides, T?> overrideGetter, Action<LightPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Light, (PixVariant v) => v.EnsureLight(), (PixVariant v) => v.Light, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.LightProperties);
	}

	public PixFieldBinding<T> BindAudioField<T>(IPix pix, Func<AudioPixProperties, T> liveGetter, Action<AudioPixProperties, T> liveSetter, Func<AudioPixVariantOverrides, T?> overrideGetter, Action<AudioPixVariantOverrides, T?> overrideSetter) where T : struct
	{
		return BindField(pix, (BasePix p) => p.Audio, (PixVariant v) => v.EnsureAudio(), (PixVariant v) => v.Audio, liveGetter, liveSetter, overrideGetter, overrideSetter, PixUpdateType.AudioProperties);
	}

	public OwnerFieldBinding<T> BindOwnerField<T>(IPix pix, Func<BasePix, T> getter, Action<BasePix, T> setter)
	{
		BasePix runtime = (ResolveRuntimePix(pix) as BasePix) ?? throw new InvalidOperationException("Pix Invalid");
		bool canEdit = !(runtime is SyncedPix syncedPix) || syncedPix.CanSyncEdit;
		return new OwnerFieldBinding<T>(getter(runtime), canEdit, delegate(T value, bool editFinished)
		{
			if (canEdit)
			{
				setter(runtime, value);
				if (editFinished)
				{
					Config.Save();
				}
			}
		});
	}

	public void UpdateInfoProperties(IPix? pix, bool editFinished = true)
	{
		if (pix != null)
		{
			if (editFinished)
			{
				Config.Save();
			}
			if (editFinished && pix is SyncedPix { SelfRank: PixRank.Owner })
			{
				this.PixUpdated?.Invoke(new PixUpdate(pix, PixUpdateType.InfoProperties, PixUpdateOrigin.Local));
			}
		}
	}

	public void UpdateSyncProperties(IPix? pix, bool editFinished = true)
	{
		if (pix != null)
		{
			if (editFinished)
			{
				Config.Save();
			}
			if (editFinished && pix is SyncedPix { SelfRank: PixRank.Owner })
			{
				this.PixUpdated?.Invoke(new PixUpdate(pix, PixUpdateType.SyncProperties, PixUpdateOrigin.Local));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Game.NativeWrapper;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation.UIInput;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PartyPyon;

public class PFManager
{
	public unsafe delegate void OpenPartyFinderListingDelegate(void* agentLfg, ulong contentId);

	private readonly Plugin plugin;

	public bool Enabled;

	public DateTime PFRecruitChangeWait = DateTime.MinValue;

	public bool IsUpdating;

	private readonly Queue<Func<bool>> ActionQueue = new Queue<Func<bool>>();

	private int ActionDelay;

	public DateTime PFExpirationTime = DateTime.MinValue;

	private int BatchNum = -1;

	private string PreviousComment = "";

	public bool IsPFRecruiting => Plugin.Condition[(ConditionFlag)66];

	public bool IsPFRecruitingOrUpdating
	{
		get
		{
			if (!IsPFRecruiting)
			{
				return IsUpdating;
			}
			return true;
		}
	}

	public bool WasPFRecruiting { get; set; }

	public OpenPartyFinderListingDelegate OpenPartyFinderListing { get; init; }

	public unsafe AgentLookingForGroup* Instance => AgentLookingForGroup.Instance();

	public unsafe string Comment
	{
		get
		{
			return ((RecruitmentSub)(&((AgentLookingForGroup)Instance).StoredRecruitmentInfo)).CommentString;
		}
		set
		{
			((RecruitmentSub)(&((AgentLookingForGroup)Instance).StoredRecruitmentInfo)).Comment.Clear();
			((RecruitmentSub)(&((AgentLookingForGroup)Instance).StoredRecruitmentInfo)).CommentString = value;
		}
	}

	public bool IsProcessingActions => ActionQueue.Count > 0;

	public PFManager(Plugin plugin)
	{
		this.plugin = plugin;
		OpenPartyFinderListing = Marshal.GetDelegateForFunctionPointer<OpenPartyFinderListingDelegate>(Plugin.SigScanner.ScanText("40 53 48 83 EC 20 48 8B D9 E8 ?? ?? ?? ?? 84 C0 74 07 C6 83 ?? ?? ?? ?? ?? 48 83 C4 20 5B C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53"));
	}

	public void Enable()
	{
		ActionQueue.Clear();
		Enabled = true;
		if (!IsPFRecruitingOrUpdating)
		{
			PFExpirationTime = DateTime.MinValue;
		}
		if (!Enabled && plugin.Config.SelectedTemplate.HasValue)
		{
			plugin.Config.SelectedTemplate = null;
			plugin.Config.Save();
		}
		if (Enabled)
		{
			EnqueueActions();
		}
	}

	public void Disable()
	{
		ActionQueue.Clear();
		Enabled = false;
		if (!IsPFRecruitingOrUpdating)
		{
			PFExpirationTime = DateTime.MinValue;
			WasPFRecruiting = false;
		}
		PFRecruitChangeWait = DateTime.MinValue;
		if (!Enabled && plugin.Config.SelectedTemplate.HasValue)
		{
			plugin.Config.SelectedTemplate = null;
			plugin.Config.Save();
		}
	}

	public void SelectionChanged()
	{
		if (plugin.Config.SelectedTemplate.HasValue)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public void OnListing(IPartyFinderListing listing, IPartyFinderListingEventArgs args)
	{
		string textValue = listing.Name.TextValue;
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (textValue == ((localPlayer != null) ? ((IGameObject)localPlayer).Name.TextValue : null))
		{
			UpdateExpirationTime(listing.SecondsRemaining);
		}
	}

	private void UpdateExpirationTime(ushort? remaining)
	{
		PFExpirationTime = DateTime.Now.AddSeconds((remaining ?? 3600) - 30);
	}

	private void EnqueueActions()
	{
		PFExpirationTime = DateTime.MinValue;
		if (IsPFRecruiting)
		{
			ActionQueue.Enqueue(PFOpenListing);
			Guid guid = plugin.Config.SelectedTemplate ?? Guid.Empty;
			if (guid != Guid.Empty && plugin.Config.Templates.TryGetValue(guid, out string value) && Comment != value)
			{
				IsUpdating = true;
				ActionQueue.Enqueue(PFEndListing);
				ActionQueue.Enqueue(PFUpdateComment);
				ActionQueue.Enqueue(PFOpen);
				ActionQueue.Enqueue(PFOpenRecruit);
				ActionQueue.Enqueue(PFConfirmUpdateListing);
				ActionQueue.Enqueue(PFClose);
			}
			else
			{
				ActionQueue.Enqueue(PFOpenEditListing);
				ActionQueue.Enqueue(PFConfirmUpdateListing);
			}
		}
		else
		{
			IsUpdating = true;
			ActionQueue.Enqueue(PFUpdateComment);
			ActionQueue.Enqueue(PFOpen);
			ActionQueue.Enqueue(PFOpenRecruit);
			ActionQueue.Enqueue(PFConfirmUpdateListing);
			ActionQueue.Enqueue(PFClose);
		}
	}

	public void Framework_Update(IFramework framework)
	{
		if (Plugin.ObjectTable.LocalPlayer == null)
		{
			return;
		}
		if (!IsPFRecruitingOrUpdating && WasPFRecruiting)
		{
			WasPFRecruiting = false;
			PFRecruitChangeWait = DateTime.Now.AddSeconds(10.0);
		}
		if (IsPFRecruitingOrUpdating && !WasPFRecruiting)
		{
			WasPFRecruiting = true;
			PFRecruitChangeWait = DateTime.MinValue;
		}
		if (PFRecruitChangeWait != DateTime.MinValue)
		{
			if (DateTime.Now < PFRecruitChangeWait)
			{
				return;
			}
			PFRecruitChangeWait = DateTime.MinValue;
		}
		if (Enabled && (!IsPFRecruitingOrUpdating || !plugin.Config.SelectedTemplate.HasValue))
		{
			Disable();
		}
		if (IsPFRecruitingOrUpdating && !string.IsNullOrWhiteSpace(Comment) && PreviousComment != Comment && ((Window)plugin.MainWindow).IsOpen)
		{
			PreviousComment = Comment;
			if (!plugin.Config.Templates.FirstOrNull<KeyValuePair<Guid, string>>((KeyValuePair<Guid, string> x) => x.Value == Comment).HasValue)
			{
				Guid key = Guid.NewGuid();
				plugin.Config.Templates.Add(key, Comment);
				plugin.Config.Save();
			}
		}
		if (!Enabled || !IsPFRecruitingOrUpdating)
		{
			return;
		}
		if (PFExpirationTime != DateTime.MinValue && PFExpirationTime < DateTime.Now)
		{
			EnqueueActions();
		}
		if (ActionQueue.Count <= 0)
		{
			return;
		}
		if (ActionDelay > 0)
		{
			ActionDelay--;
			return;
		}
		Func<bool> result = null;
		try
		{
			if (ActionQueue.TryPeek(out result) && result())
			{
				ActionQueue.Dequeue();
			}
		}
		catch (Exception ex)
		{
			Plugin.PluginLog.Error("Action Failed [" + ((result == null) ? "UnknownAction" : result?.Method.Name) + "]: " + ex.Message, Array.Empty<object>());
		}
		ActionDelay = 20;
	}

	private unsafe bool TryGetAddonByName<T>(string Addon, out T* AddonPtr) where T : unmanaged
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		AtkUnitBasePtr addonByName = Plugin.GameGui.GetAddonByName(Addon, 1);
		if (addonByName == AtkUnitBasePtr.op_Implicit((IntPtr)IntPtr.Zero))
		{
			AddonPtr = null;
			return false;
		}
		AddonPtr = (T*)addonByName.Address;
		return true;
	}

	private unsafe bool IsAddonReady(AtkUnitBase* addon)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (((AtkUnitBase)addon).IsVisible)
		{
			return (int)((AtkUldManager)(&((AtkUnitBase)addon).UldManager)).LoadedState == 3;
		}
		return false;
	}

	public unsafe bool PFOpen()
	{
		((AgentLookingForGroup)Instance).SearchAreaTab = 0;
		((AgentLookingForGroup)Instance).CategoryTab = 16;
		if (!TryGetAddonByName<AtkUnitBase>("LookingForGroup", out AtkUnitBase* AddonPtr) || !IsAddonReady(AddonPtr))
		{
			((UIModule)((Framework)Framework.Instance()).GetUIModule()).ExecuteMainCommand(57u);
		}
		else
		{
			((AgentLookingForGroup)Instance).RequestListingsUpdate();
		}
		return true;
	}

	public unsafe bool PFOpenListing()
	{
		OpenPartyFinderListing(Instance, Plugin.PlayerState.ContentId);
		return true;
	}

	public unsafe bool PFClose()
	{
		if (TryGetAddonByName<AtkUnitBase>("LookingForGroup", out AtkUnitBase* AddonPtr) && IsAddonReady(AddonPtr))
		{
			((AtkUnitBase)AddonPtr).Close(true);
		}
		return true;
	}

	public unsafe bool PFOpenRecruit()
	{
		if (TryGetAddonByName<AtkUnitBase>("LookingForGroup", out AtkUnitBase* AddonPtr) && IsAddonReady(AddonPtr))
		{
			new AddonMaster.LookingForGroup(AddonPtr).RecruitMembersOrDetails();
			return true;
		}
		return false;
	}

	public bool PFUpdateComment()
	{
		Guid guid = plugin.Config.SelectedTemplate ?? Guid.Empty;
		if (guid != Guid.Empty && plugin.Config.Templates.TryGetValue(guid, out string value))
		{
			Comment = value;
			return Comment == value;
		}
		return true;
	}

	public unsafe bool PFEndListing()
	{
		if (TryGetAddonByName<AtkUnitBase>("LookingForGroupDetail", out AtkUnitBase* AddonPtr) && IsAddonReady(AddonPtr))
		{
			new AddonMaster.LookingForGroupDetail(AddonPtr).TellEnd();
			return true;
		}
		return false;
	}

	public unsafe bool PFOpenEditListing()
	{
		if (TryGetAddonByName<AtkUnitBase>("LookingForGroupDetail", out AtkUnitBase* AddonPtr) && IsAddonReady(AddonPtr))
		{
			new AddonMaster.LookingForGroupDetail(AddonPtr).JoinEdit();
			return true;
		}
		return false;
	}

	public unsafe bool PFConfirmUpdateListing()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetAddonByName<AtkUnitBase>("LookingForGroupCondition", out AtkUnitBase* AddonPtr) && IsAddonReady(AddonPtr))
		{
			AddonMaster.LookingForGroupCondition lookingForGroupCondition = new AddonMaster.LookingForGroupCondition(AddonPtr);
			AtkComponentButton* componentButtonById = ((AtkUnitBase)lookingForGroupCondition.Addon).GetComponentButtonById(113u);
			if (((AtkComponentButton)componentButtonById).IsEnabled)
			{
				(*componentButtonById).ClickAddonButton(lookingForGroupCondition.Base);
				UpdateExpirationTime((ushort)3600);
				IsUpdating = false;
				return true;
			}
		}
		return false;
	}

	public unsafe bool ConfirmUpdatePFListing_old()
	{
		if (!GenericHelpers.TryGetAddonMaster<AddonMaster.LookingForGroupCondition>(out var addonMaster) || !addonMaster.IsAddonReady)
		{
			return false;
		}
		if (!((AtkComponentButton)addonMaster.RecruitButton).IsEnabled)
		{
			return false;
		}
		addonMaster.Recruit();
		UpdateExpirationTime((ushort)3600);
		return true;
	}

	public void Initialize()
	{
		ActionQueue.Clear();
		plugin.Config.SelectedTemplate = null;
	}

	public void Dispose()
	{
		ActionQueue.Clear();
		plugin.Config.SelectedTemplate = null;
		plugin.Config.Save();
	}
}

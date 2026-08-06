using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class UpdatesWindow : BaseWindow
{
	private readonly Dictionary<uint, bool> ExpandedStates = new Dictionary<uint, bool>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Updates.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Updates.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(420f, 190f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	protected override bool ShowTitleBarSettingsButton => false;

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Updates.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Updates.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public UpdatesWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Changelog###PyonPixChangelog", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(600f, 360f) * ImGuiHelpers.GlobalScale;
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (((Window)this).IsOpen)
		{
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + WindowPadding);
			ImGui.BeginChild(ImU8String.op_Implicit("##container"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			DrawHeader();
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X - WindowPadding.X);
			ImGui.BeginChild(ImU8String.op_Implicit("##content"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			DrawChangelog();
			ImGui.EndChild();
			ImGui.EndChild();
		}
	}

	private void DrawHeader()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		float itemSpacing = ItemSpacing;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		if (ImGuiEx.Checkbox("Show Changelog on Update", ref Config.UI.Updates.ShowUpdates, disabled: false, null, null, LineHeight))
		{
			Config.Save();
		}
		float num2 = 6f;
		float num3 = 4f;
		Vector2 vector = UiUtil.CalcTextSize(UIShared.NormalIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61445));
		Vector2 vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit("Discord"), false, -1f);
		float num4 = vector.X + vector2.X + num3 + num2 * 2f;
		Vector2 vector3 = UiUtil.CalcTextSize(UIShared.NormalIconFont, FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61444));
		Vector2 vector4 = ImGui.CalcTextSize(ImU8String.op_Implicit("Ko-fi"), false, -1f);
		float num5 = vector3.X + vector4.X + num3 + num2 * 2f;
		float num6 = cursorScreenPos.X + num - num5;
		ImGui.SetCursorScreenPos(new Vector2(num6, cursorScreenPos.Y));
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)61444, "Ko-fi", "##kofi", disabled: false, "Support me on Ko-fi!", null, num5, LineHeight, 0.7f))
		{
			UiUtil.OpenKofi();
		}
		ImGui.SetCursorScreenPos(new Vector2(num6 - num4 - itemSpacing, cursorScreenPos.Y));
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)61445, "Discord", "##discord", disabled: false, "Join the Pyon Discord!", null, num4, LineHeight, 0.7f))
		{
			UiUtil.OpenDiscord();
		}
	}

	private void DrawChangelog()
	{
		if (BeginContainer("v1.3.0.0 - 2026.07.22 (Test Release)", isPrimary: true))
		{
			AddEntry("Refactor of Renderer");
			AddSubEntry("All shared renderer related properties have been removed, it should no longer be necessary to manually adjust things.");
			AddNotice("This is currently in early testing phase, some functionality like screen opacity & nameplate depth are not yet re-implemented.");
			AddNotice("If you are someone who previously needed to manually adjust renderer properties, please let me know on Discord if this version works without issue.");
			EndContainer();
		}
		if (BeginContainer("v1.2.0.4 - 2026.07.19"))
		{
			AddEntry("Fixed crash on plugin unload.");
			AddNotice("Thank you to those who helped with debugging this issue :3");
			EndContainer();
		}
		if (BeginContainer("v1.2.0.3 - 2026.07.19"))
		{
			AddHeader("Sync Service");
			AddEntry("Fixed syncing of navigated page.");
			AddEntry("Fixed issues with changes to a member's rank not reflecting until reconnection.");
			AddEntry("Fixed connection issue for clients with invalid auth.");
			EndContainer();
		}
		if (BeginContainer("v1.2.0.2 - 2026.07.18"))
		{
			AddHeader("Sync Service");
			AddEntry("Minor debugging changes.");
			AddHeader("Browser/Renderer");
			AddEntry("Fix issue causing navigation to repeat in some cases.");
			AddEntry("Made screen interactions optional with conditions.");
			AddNotice("Main Config > Shared Browser Properties\n• Configure which buttons a screen can capture.\n• Configure whether modifier key needs to be pressed to begin capture.\n• Configure whether interaction is only possible when clicking the front face of a screen.\n• Configure whether cursor can change when hovering over browser elements.");
			AddNotice("Pix Config > Browser Properties\n• Configure whether an individual pix screen can be interacted with.");
			AddNotice("By default, screen interaction via left click is enabled but requires Ctrl key to be held initially, and only on the front face of screen.");
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X - WindowPadding.X);
			AddWarn("Hopefully this is okay qwq\nYou can restore key input back to game by clicking outside of a screen.\nIf you somehow manage to get stuck, you can also alt+tab out then back into game to restore input.");
			EndContainer();
		}
		if (BeginContainer("v1.2.0.0 - 2026.07.18"))
		{
			AddHeader("Sync Service");
			AddEntry("Implemented means of syncing the media state (play/pause/seek) of non-streaming media content for those who prefer not having to use web services like watchparty.");
			AddSubEntry("Media state syncing is session based with no override.");
			AddSubEntry("Only extensively tested with Youtube, other websites may require specific handling.");
			AddNotice("Ads will likely cause desync, recommended to install ublock origin from the Extensions window.");
			AddNotice("If desynced, you can resync via button in browser toolbar & in the media controls.");
			AddNotice("You must have editing permissions of a synced pix in order to send sync updates to other viewers.");
			AddHeader("Browser/Renderer");
			AddEntry("Implemented automatic fullscreen of video content.");
			AddNotice("Can be toggled with keybind Ctrl+F, via button in browser toolbar & in the media controls.");
			AddEntry("Implemented interaction functionality with pix screens, so you no longer need to keep the browser window open.");
			AddNotice("Clicking on a screen will lock your key input to that screen, you can restore key input back to game by clicking outside the screen region.");
			AddNotice("Keybind Ctrl+E will display a uri input box for navigation.");
			AddHeader("General");
			AddEntry("Implemented static alias/pix styling which you can access from the little icon next to your alias in the main /pix window.");
			AddNotice("Animated styling is exclusive to supporters.");
			AddEntry("Increased synced pix max idle session time to 1 hour so media state can be retained while no viewers are present.");
			AddEntry("Fixed issue where certain navigation behaviours were not syncing correctly.");
			AddEntry("Fixed browser issues relating to navigation requests failing.");
			AddEntry("Removed disabled state of Pix spawn toggle button to address cases where the browser may hang & require respawning.");
			AddEntry("Removed function for transfering ownership of a synced pix, will be re-implemented in a later update when I get around to fixing it.");
			AddEntry("Various server side fixes & adjustments to connection logic.");
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X - WindowPadding.X);
			AddWarn("..can I take a break yet? ; w;");
			EndContainer();
		}
		if (BeginContainer("v1.1.0.1 - 2026.07.06"))
		{
			AddHeader("Sync Service");
			AddEntry("Tiny update to fix uri changes not syncing = w=");
			AddEntry("Also added a little bit of logging to debug connection issues maybe.");
			EndContainer();
		}
		if (BeginContainer("v1.1.0.0 - 2026.07.06"))
		{
			AddHeader("Sync Service");
			AddEntry("Sync Service is now available, the connection toggle button can be found in the main PyonPix window. Connection to the service is automatic upon character login until manually disconnecting from the service.");
			AddNotice("Do be aware this service provides a means of conveniently sharing/updating a Pix with others. It does not provide a means of streaming media itself, you will still need to use other 3rd party web services.");
			AddEntry("Upon initial connection, you'll be provided an AuthKey for registration on Discord in the #pyonpix channel (hover the key for instructions).");
			AddEntry("A 'Sync' tab has been added to the Pix config window for managing broadcasting of a Pix, which in turn promotes it from a 'Local Pix' to a 'Synced Pix'.");
			AddNotice("The territory a Pix resides in cannot be changed while broadcasted.");
			AddNotice("A synced Pix can be made to be private (requiring password), or unlisted/public.");
			AddEntry("A 'Sync Search' window has been added which can be accessed via the Search icon in the main PyonPix window. This window can be used for querying syncable Pix's & subscribing to them.");
			AddNotice("Whether a Pix is listed depends on its privacy & the filtering options.");
			AddEntry("A 'Pix Members' window has been added which can be accessed from a synced Pix's context menu. The owner of a synced Pix can adjust the rank of members from this window, for governing who can change the synced properties.");
			AddNotice("Members who do not have editing permission can still change most properties as a local override which isn't synced to other members.");
			AddEntry("A 'User Window' has been added which can be accessed from the small icon to the left of your Alias in the main PyonPix window for customizing Alias/Pix naming style.");
			AddNotice("Adjusting naming style is exclusive to supporters/subscribers for now, some limited styling will be available to non-supporters later.");
			AddHeader("Other Changes");
			AddEntry("Uhh.. A lot of minor stuff, I may have broken something!");
			ImGuiEx.Separator(ImGui.GetContentRegionAvail().X - WindowPadding.X);
			AddWarn("This was a lot of work, though I know some people will be disappointed due to expectations not being met qwq\nThere are still various known issues/improvements I need to work on.\nSorry for the long delay, I had a lot going on in life.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.7 - 2026.04.29"))
		{
			AddEntry("Api15 Update");
			AddEntry("Changed browser implementation to fix issues relating to 'DllNotFound' exception & conflicts with applications like ACT.");
			AddEntry("Fixed issue where screen would redraw when Penumbra redraws.");
			AddEntry("Fixed issue where Netflix would assume PyonPix to be an incompatible browser.");
			AddEntry("Fixed issue with uri incorrectly updating when an inactive tab is navigated.");
			AddEntry("Added 'Share Cookies' property to make cookie sharing optional as some web services like Youtube fail to persist sessions with this behaviour.");
			AddEntry("Various UI changes/fixes.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.6 - 2026.03.22"))
		{
			AddEntry("Added 'Depth Mode' property which you may need to adjust if there are depth related issues with the screen or to preserve transparency.");
			AddEntry("Added 'Shader Target' toggle which some people may need to enable if no combination of render format/binding type works. This option also allows shader effects from Gshade/Reshade to apply to the screen. You must set 'SourceAlpha' to 'Zero' if using this option.");
			AddEntry("Added various blend state options.");
			AddEntry("Debug tab no longer presents previews of dsv/rtv items & should no longer cause crashes.");
			AddEntry("Fixed various crash issues relating to resolution changes.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.5 - 2026.03.20"))
		{
			AddHeader("Renderer");
			AddEntry("Further adjustments to screen rendering.");
			AddEntry("Added render mode property which determines whether nameplates are obscured by the screen or not.");
			AddEntry("Fixed issue where cached render targets were not correctly reset on territory change.");
			AddHeader("Browser");
			AddEntry("Fixed HDR overexposure related issue with video content.");
			AddEntry("Fixed 'Device Creation Failed' issue for gpu's that do not support certain device flags.");
			AddEntry("Fixed issue causing crash to desktop when browser host exits on plugin unload.");
			AddHeader("General");
			AddEntry("Fixed issue where presets would persist without persistent territory option enabled.");
			AddEntry("Added constraint to window positions to prevent titlebar leaving game window.");
			AddEntry("Added open state of windows to config to persist state on plugin reload.");
			AddEntry("Maybe fixed gizmo window related issues.");
			AddEntry("Fixed issue where pix config window would remain visible for a removed pix preset.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.4 - 2026.03.17"))
		{
			AddEntry("Further fixes for screen rendering issues.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.3 - 2026.03.17"))
		{
			AddEntry("Fixed various issues with screen not rendering.");
			AddEntry("Fixed issue some users were experiencing which caused style changes to other dalamud/plugin windows.");
			AddEntry("Fixed visual naming issue of certain residential territories.");
			AddEntry("'Set to Current Territory' button will now also reposition the screen to your character.");
			AddEntry("Fixed issue where changes to Renderer properties via Colour/Drag controls were only applying when ending interaction.");
			AddEntry("Browser scaling changes will now only apply when ending interaction to prevent potential crashes.");
			AddEntry("Fixed disabling of spatial audio.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.2 - 2026.03.16"))
		{
			AddEntry("Added Render Format property to address screen visibility issues.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.1 - 2026.03.16"))
		{
			AddEntry("Fixed crash on initialize (relating to pixel shader compilation with some gpu's).");
			AddEntry("Fixed invisible screen (issue with how render target was set).");
			AddEntry("Fixed oddities when changing window resolution (from windowed/borderless fullscreen).");
			AddWarn("Fullscreen may not currently be working correctly, use borderless fullscreen instead.");
			EndContainer();
		}
		if (BeginContainer("v1.0.0.0 - 2026.03.16"))
		{
			AddEntry("Initial testing release.");
			EndContainer();
		}
	}

	private bool BeginContainer(string text, bool isPrimary = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		bool num = ImGuiEx.Container(ImU8String.op_Implicit(text), ExpandedStates, isPrimary, ImGui.GetContentRegionAvail().X - WindowPadding.X, isPrimary ? AnimationType.RainbowWave : AnimationType.Static);
		if (num)
		{
			ImGui.Indent();
		}
		return num;
	}

	private static void EndContainer()
	{
		ImGui.Unindent();
	}

	private static void AddHeader(string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ImGuiEx.StyledText(ImU8String.op_Implicit(text), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
	}

	private static void AddEntry(string text)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ImU8String text2 = default(ImU8String);
		((ImU8String)(ref text2))._002Ector(2, 1);
		((ImU8String)(ref text2)).AppendLiteral("• ");
		((ImU8String)(ref text2)).AppendFormatted<string>(text);
		ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.1f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue, multiline: true);
	}

	private static void AddSubEntry(string text)
	{
		ImGui.Indent();
		AddEntry(text);
		ImGui.Unindent();
	}

	private static void AddNotice(string text)
	{
		Highlighted(text, UIShared.AccentActive);
	}

	private static void AddWarn(string text)
	{
		Highlighted(text, UIShared.Warn);
	}

	private static void Highlighted(string text, Vector4 col)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		using (UIShared.SubFont.Push())
		{
			ImU8String text2 = ImU8String.op_Implicit(text);
			Vector3? colorA = col.AsVector3();
			float? wrapWidth = ImGui.GetContentRegionAvail().X;
			ImGuiEx.StyledText(text2, null, 0.8f, 0.2f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, wrapWidth);
		}
	}
}

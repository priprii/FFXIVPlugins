using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Structs.Browser;
using PyonPix.Structs.Ui;
using PyonPix.Utility;
using SharpDX.Direct3D11;

namespace PyonPix.Ui.Windows;

public class SetupWindow : BaseWindow
{
	private enum SetupStep
	{
		Welcome,
		MainWindow,
		Browser,
		Renderer,
		Extensions,
		Syncing
	}

	private SetupStep Step;

	private static readonly SetupStep[] Steps = new SetupStep[6]
	{
		SetupStep.Welcome,
		SetupStep.MainWindow,
		SetupStep.Browser,
		SetupStep.Renderer,
		SetupStep.Extensions,
		SetupStep.Syncing
	};

	private RendererService RendererService => Services.Get<RendererService>();

	private ExtensionsService ExtensionsService => Services.Get<ExtensionsService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.Setup.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.Setup.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(350f, 150f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	protected override bool ShowTitleBarSettingsButton => false;

	private SetupStep CurrentStep => Steps[(int)Step];

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		Config.UI.Setup.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.Setup.IsOpen = false;
		Config.UI.Setup.InitialSetup = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.Setup.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.Setup.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public SetupWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix Setup###PyonPixSetup", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(680f, 530f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (((Window)this).IsOpen)
		{
			Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
			ImGui.BeginChild(ImU8String.op_Implicit("##container"), contentRegionAvail, false, (ImGuiWindowFlags)24);
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float x = ImGui.GetContentRegionAvail().X;
			ImGui.SetCursorScreenPos(cursorScreenPos + new Vector2(WindowPadding.X, 0f));
			float num = x - WindowPadding.X * 2f;
			float num2 = contentRegionAvail.Y - WindowPadding.Y * 2f;
			float num3 = 48f * ImGuiHelpers.GlobalScale;
			float num4 = 52f * ImGuiHelpers.GlobalScale;
			float y = MathF.Max(0f, num2 - num3 - num4);
			ImGui.BeginChild(ImU8String.op_Implicit("##header"), new Vector2(num, num3), false, (ImGuiWindowFlags)8);
			DrawHeader(num, num3);
			ImGui.EndChild();
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(WindowPadding.X, 0f));
			ImGui.BeginChild(ImU8String.op_Implicit("##content"), new Vector2(num, y), false, (ImGuiWindowFlags)0);
			float x2 = ImGui.GetContentRegionAvail().X;
			ImGuiEx.SpacingY(12f);
			switch (CurrentStep)
			{
			case SetupStep.Welcome:
				DrawWelcome(x2);
				break;
			case SetupStep.MainWindow:
				DrawMainWindow(x2);
				break;
			case SetupStep.Browser:
				DrawBrowser(x2);
				break;
			case SetupStep.Renderer:
				DrawRenderer(x2);
				break;
			case SetupStep.Extensions:
				DrawExtensions(x2);
				break;
			case SetupStep.Syncing:
				DrawSyncing(x2);
				break;
			}
			ImGui.EndChild();
			ImGui.BeginChild(ImU8String.op_Implicit("##footer"), new Vector2(num, num4), false, (ImGuiWindowFlags)8);
			ImGuiEx.Separator(num, 0f, UIShared.SeparatorSpacing);
			float separatorSpacing = UIShared.SeparatorSpacing;
			float num5 = 32f * ImGuiHelpers.GlobalScale;
			float num6 = num4 - separatorSpacing;
			ImGui.SetCursorPosY(separatorSpacing + (num6 - num5) * 0.5f);
			DrawNavigationButtons();
			ImGui.EndChild();
			ImGui.EndChild();
		}
	}

	private void DrawHeader(float width, float height)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		SetupStep setupStep = Step + 1;
		int num = Steps.Length;
		string stepTitle = GetStepTitle(CurrentStep);
		string stepCommand = GetStepCommand(CurrentStep);
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = Vector2.Zero;
		using (UIShared.HeaderFont.Push())
		{
			ImU8String val = new ImU8String(4, 3);
			((ImU8String)(ref val)).AppendLiteral("[");
			((ImU8String)(ref val)).AppendFormatted<int>((int)setupStep);
			((ImU8String)(ref val)).AppendLiteral("/");
			((ImU8String)(ref val)).AppendFormatted<int>(num);
			((ImU8String)(ref val)).AppendLiteral("] ");
			((ImU8String)(ref val)).AppendFormatted<string>(stepTitle);
			vector = ImGui.CalcTextSize(val, false, -1f);
		}
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, cursorScreenPos.Y + (height - vector.Y) * 0.5f));
		using (UIShared.HeaderFont.Push())
		{
			ImU8String val2 = new ImU8String(4, 3);
			((ImU8String)(ref val2)).AppendLiteral("[");
			((ImU8String)(ref val2)).AppendFormatted<int>((int)setupStep);
			((ImU8String)(ref val2)).AppendLiteral("/");
			((ImU8String)(ref val2)).AppendFormatted<int>(num);
			((ImU8String)(ref val2)).AppendLiteral("] ");
			((ImU8String)(ref val2)).AppendFormatted<string>(stepTitle);
			ImU8String text = val2;
			Vector3? colorA = UIShared.AccentActive.AsVector3();
			ImGuiEx.StyledText(text, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
		float num2 = 100f;
		float x = cursorScreenPos.X + width - num2 - 8f * ImGuiHelpers.GlobalScale;
		DrawCommand(stepCommand, x, cursorScreenPos.Y, num2, height);
		float y = cursorScreenPos.Y + height - UIShared.SeparatorSpacing;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, y));
		ImGuiEx.Separator(width, 0f, 0f);
	}

	private string GetStepTitle(SetupStep step)
	{
		return step switch
		{
			SetupStep.Welcome => "Welcome", 
			SetupStep.MainWindow => "Main Window", 
			SetupStep.Browser => "Browser", 
			SetupStep.Renderer => "Renderer", 
			SetupStep.Extensions => "Extensions", 
			SetupStep.Syncing => "Syncing", 
			_ => string.Empty, 
		};
	}

	private string GetStepCommand(SetupStep step)
	{
		return step switch
		{
			SetupStep.Welcome => "/pix setup", 
			SetupStep.MainWindow => "/pix", 
			SetupStep.Browser => "/pix browser", 
			SetupStep.Renderer => "/pix config", 
			SetupStep.Extensions => "/pix extensions", 
			SetupStep.Syncing => "/pix sync", 
			_ => string.Empty, 
		};
	}

	private void DrawCommand(string command, float x, float y, float width, float height)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		string text = "COMMAND";
		Vector2 vector;
		Vector2 vector2;
		using (UIShared.SubFont.Push())
		{
			vector = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
			vector2 = ImGui.CalcTextSize(ImU8String.op_Implicit(command), false, -1f);
		}
		float num = 2f * ImGuiHelpers.GlobalScale;
		float num2 = vector.Y + num + vector2.Y;
		float num3 = y + (height - num2) * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(x + (width - vector.X) * 0.5f, num3));
		using (UIShared.SubFont.Push())
		{
			ImGuiEx.NoticeText(text);
		}
		ImGui.SetCursorScreenPos(new Vector2(x + (width - vector2.X) * 0.5f, num3 + vector.Y + num));
		using (UIShared.SubFont.Push())
		{
			ImU8String text2 = ImU8String.op_Implicit(command);
			Vector3? colorA = UIShared.AccentActive.AsVector3();
			ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
		}
	}

	private void DrawNavigationButtons()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		float x = ImGui.GetContentRegionAvail().X;
		float num = 90f * ImGuiHelpers.GlobalScale;
		ImGuiStylePtr style = ImGui.GetStyle();
		float x2 = ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		float num2 = num * 2f + x2;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (x - num2) * 0.5f);
		if (CurrentStep != SetupStep.Welcome)
		{
			if (ImGuiEx.IconTextButton((FontAwesomeIcon)61657, "Back", "##back", disabled: false, null, null, num))
			{
				Step--;
			}
			ImGui.SameLine();
		}
		else
		{
			if (ImGuiEx.IconTextButton((FontAwesomeIcon)61453, "Skip", "##skip", disabled: false, null, null, num))
			{
				FinishSetup();
			}
			ImGui.SameLine();
		}
		bool flag = (int)Step >= Steps.Length - 1;
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)(flag ? 61452 : 61658), flag ? "Finish" : "Continue", "##continue", disabled: false, null, null, num))
		{
			if (flag)
			{
				FinishSetup();
			}
			else
			{
				Step++;
			}
		}
	}

	private void DrawWelcome(float width)
	{
		DrawInfo("Welcome to PyonPix!");
		ImGuiEx.SpacingY(8f);
		DrawInfo("This setup will guide you through the core features of PyonPix and help ensure the renderer works correctly.");
		ImGuiEx.SpacingY(8f);
		if (ImGuiEx.BeginContainer("What does this plugin do?"))
		{
			DrawInfo("PyonPix is a plugin which enables you to create a 'Pix', primarily for watching movies/videos with others.");
			ImGuiEx.NoticeText("A 'Pix' is a screen spawned in the game world, the screen displays content from an ingame web browser.");
			ImGuiEx.EndContainer();
		}
		ImGuiEx.IconText("When you're ready, click [icon:CaretRight] Continue.. or click [icon:Times] Skip if you think you know what you're doing!");
		ImGuiEx.SpacingY(8f);
		ImGuiEx.NoticeText("You can always access this later with '/pix setup' command.");
		ImGuiEx.NoticeText("I will bonk you if you ask questions which this setup already covers!!");
	}

	private void DrawMainWindow(float width)
	{
		DrawInfo("The Main Window lists any Pix's you own or are subscribed to, categorized by territory.");
		ImGuiEx.NoticeText("You can open this window with the '/pix' command.");
		ImGuiEx.NoticeText("The first territory listed is where you currently are.");
		ImGuiEx.NoticeText("You can click a territory to toggle expanding it to view the list of Pix's residing in it.");
		ImGuiEx.SpacingY(8f);
		ImGuiEx.IconText("Let's start by creating a Pix! Just click that [icon:Plus] button in the top right of the Main Window.");
		ImGuiEx.NoticeText("Don't worry, you can delete this Pix later if you don't want it here.");
		ImGuiEx.IconText("After creating a Pix, you'll notice the Pix Config window opens where you can customize properties specific to that Pix. You can also access this Config window via the Pix context menu - click the [icon:EllipsisV] button to the right of the Pix listed in the Main Window.");
		ImGuiEx.NoticeText("For now, you can just close the Config window, we'll return to it later.");
		ImGuiEx.SpacingY(8f);
		ImGuiEx.IconText("You should notice a screen spawned at your location too, it may look weird at the moment - don't worry!");
		ImGuiEx.WarningText("If you don't see the screen at all, don't worry.. yet!");
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, UIShared.SeparatorSpacing);
		ImGuiEx.IconText("When you're ready to check out the Browser, go hit that [icon:CaretRight] Continue button!");
	}

	private void DrawBrowser(float width)
	{
		ImGuiEx.IconText("The Browser Window is where you can manage the content displayed on a spawned Pix, this functions like any typical web browser. You can open the Browser via the [icon:Globe] button in the Main Window.");
		ImGuiEx.NoticeText("You can also open it with the '/pix browser' command.");
		ImGuiEx.SpacingY(8f);
		DrawInfo("By default, you should hopefully see a fancy starry page, or whatever else you have set as your homepage.");
		ImGuiEx.WarningText("If you don't see anything in the Browser window & can't navigate, your webview2 version may be outdated, seek help on Discord!");
		DrawInfo("For now, stay on the starry page. If you're not already there, enter this as the uri: pix://starry");
		ImGuiEx.NoticeText("The starry page uses opacity which helps with understanding differences in how you can setup the Renderer.");
		ImGuiEx.SpacingY(8f);
		ImGuiEx.IconText("In the top right, you'll notice a [icon:EllipsisV] button which opens a context menu of Browser related options, ignore it for now, we'll come back to it later.");
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, UIShared.SeparatorSpacing);
		ImGuiEx.IconText("We'll next fiddle with the Renderer, poke that [icon:CaretRight] Continue button please");
	}

	private void DrawRenderer(float width)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		DrawInfo("The renderer is responsible for drawing the browser content to a screen in the world.");
		ImGuiEx.NoticeText("You can adjust the below Composition Index & Source Alpha until the screen behaves in a way you like, you should only need to do this once.");
		ImGuiEx.NoticeText("You can also adjust this later from 'Shared Renderer Properties' found in '/pix config'");
		ImGuiEx.SpacingY(12f);
		bool flag = Config.Global.Renderer.SourceAlphaBlend == BlendOption.One;
		if (ImGui.Checkbox(ImU8String.op_Implicit("Source Alpha"), ref flag))
		{
			Config.Global.Renderer.SourceAlphaBlend = ((!flag) ? BlendOption.Zero : BlendOption.One);
			ApplyRendererProperties();
		}
		ImGuiEx.SpacingY(4f);
		ref int compositionIndex = ref Config.Global.Renderer.CompositionIndex;
		float width2 = 200f * ImGuiHelpers.GlobalScale;
		if (ImGuiEx.Drag("Composition Index##compIndex", ref compositionIndex, 0.02f, 0, 20, 2, default(ImU8String), disabled: false, width2) == UIState.Ended)
		{
			ApplyRendererProperties();
		}
		ImGuiEx.SpacingY(12f);
		ImGuiEx.WarningText("Generally, things to consider:\n• Ignore any composition that does not draw the screen in the correct position.\n• Composition 0 does not typically support alpha but is the most depth stable & easiest to setup.\n• Earlier compositions are prior to post-processing effects, so will be influenced by shader effects.\n• Earlier compositions (after Index 0) may have lighting effects bleed through the screen.\n• Adjust your camera angle when testing, if the screen disappears at some angles, try an earlier composition.\n• If you are using AMD FSR, Composition 0 without alpha may display some ghosting on any pages with transparent backgrounds like the starry page, this shouldn't be an issue on most webpages.");
		ImGuiEx.SpacingY(4f);
		ImGuiEx.NoticeText("There are many screen styling properties you can customize in the Pix Config window.");
		ImGuiEx.NoticeText("You can also interact with the screen which you can customize in Shared Browser Properties.");
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, UIShared.SeparatorSpacing);
		ImGuiEx.IconText("If you're happy with the screen, [icon:CaretRight] Continue on to learn about Extensions.");
		ImGuiEx.NoticeText("If you're not happy, go complain to Luna on Discord!");
	}

	private void ApplyRendererProperties()
	{
		RendererGlobalProperties renderer = Config.Global.Renderer;
		renderer.IsBlendEnabled = true;
		renderer.AlphaToCoverageEnable = false;
		renderer.IndependentBlendEnable = false;
		renderer.SourceBlend = BlendOption.SourceAlpha;
		renderer.DestinationBlend = BlendOption.InverseSourceAlpha;
		renderer.BlendOperation = BlendOperation.Add;
		BlendOption sourceAlphaBlend = renderer.SourceAlphaBlend;
		if ((uint)(sourceAlphaBlend - 1) > 1u)
		{
			renderer.SourceAlphaBlend = ((renderer.CompositionIndex == 0) ? BlendOption.Zero : BlendOption.One);
		}
		renderer.DestinationAlphaBlend = BlendOption.Zero;
		renderer.AlphaBlendOperation = BlendOperation.Add;
		renderer.RenderTargetWriteMask = ColorWriteMaskFlags.All;
		Config.Save();
		RendererService.RebuildGlobalProperties(Config.Global.Renderer);
	}

	private void DrawExtensions(float width)
	{
		DrawInfo("The PyonPix Browser uses Microsoft WebView2, which supports installation of browser extensions retrieved from Microsoft Extensions Store.");
		ImGuiEx.IconText("The Extensions window can be opened from the [icon:EllipsisV] button mentioned earlier in the Browser window.");
		ImGuiEx.NoticeText("You can also open it with the '/pix extensions' command.");
		ImGuiEx.NoticeText("Do note that there are limitations, WebView2 does not yet expose a means of configuring extensions.");
		ImGuiEx.SpacingY(8f);
		if (ImGuiEx.BeginContainer("Recommended: Ad Blocking with uBlock Origin"))
		{
			DrawInfo("Most video websites will throw ads at you, it is strongly recommended to use an ad blocking extension like uBlock Origin.");
			ImGuiEx.SpacingY(4f);
			Dictionary<string, Extension> extensions = Config.Extensions;
			if (extensions.ContainsKey("odfafepnkmbhccpbejgmiehpchacaeak") || extensions.ContainsKey("cimighlppcgcoapaliogpjjdehbnofhn"))
			{
				Extension value;
				Extension extension = (extensions.TryGetValue("odfafepnkmbhccpbejgmiehpchacaeak", out value) ? value : extensions["cimighlppcgcoapaliogpjjdehbnofhn"]);
				if (!extension.IsEnabled)
				{
					ImGuiEx.NoticeText(extension.Name + " is downloaded, but not installed. Click the button below if you wish to install it now.");
					if (ImGuiEx.IconTextButton((FontAwesomeIcon)61465, "Install uBlock Origin", "##installublock", ExtensionsService.IsOperating))
					{
						ExtensionsService.DownloadOrUpdateAndInstallAsync(extension.CrxId ?? "");
					}
				}
				else
				{
					ImGuiEx.NoticeText(extension.Name + " is installed!");
				}
			}
			else
			{
				ImGuiEx.NoticeText("Click the button below if you wish to have it installed for you right now.");
				if (ImGuiEx.IconTextButton((FontAwesomeIcon)61465, "Install uBlock Origin", "##installublock", ExtensionsService.IsOperating))
				{
					ExtensionsService.DownloadOrUpdateAndInstallAsync("odfafepnkmbhccpbejgmiehpchacaeak");
				}
			}
			ImGuiEx.EndContainer();
		}
		ImGuiEx.SpacingY(8f);
		ImGuiEx.NoticeText("The Extensions Window has 2 tabs:\n• Extensions - Lists any extensions you currently have downloaded/installed & provides a means of auto-updating them.\n• Browse - Search for an extension to download.");
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, UIShared.SeparatorSpacing);
		ImGuiEx.IconText("Next we'll learn all there is to know about syncing.. after you click [icon:CaretRight] Continue.");
	}

	private void DrawSyncing(float width)
	{
		DrawInfo("The PyonPix Sync Service provides a means of syncing properties of a Pix & the playback state of media content between multiple users.");
		ImGuiEx.IconText("To begin using the Sync Service, you should first read the #pyonpix channel on Discord. You can join the Discord server by clicking the [icon:Star] button at the top of the Main Window.");
		ImGuiEx.IconText("When you are ready, you can click the [icon:Link] button in the Main Window to connect, initial connection will require authentication via the #pyonpix channel on Discord.");
		ImGuiEx.SpacingY(8f);
		if (ImGuiEx.BeginContainer("Broadcasting a Pix"))
		{
			DrawInfo("Open the Pix Config window of the Pix you wish to sync, provide it a suitable name & optional descrption on the 'Info Properties' tab, then on the 'Sync Properties' tab you can adjust the privacy & editing permissions, when you're ready you can click 'Sync Pix'.");
			ImGuiEx.WarningText("Be aware that the territory the Pix resides in cannot be changed once synced.");
			ImGuiEx.SpacingY(4f);
			ImGuiEx.IconText("'Privacy' controls whether your Pix will be listed in the Sync Search window, which you can open via the [icon:Search] button in the Main Window.");
			ImGuiEx.NoticeText("You can also open it with the '/pix sync' command.");
			ImGuiEx.NoticeText("If the Pix is private, you'll need to provide the Pix Id & password to those you wish to share it with. You can find a 'Copy PixId' button in the Pix context menu.");
			ImGuiEx.SpacingY(4f);
			DrawInfo("The editor rank controls who can make synced changes to the Pix, you can adjust the rank of subscribed members via the 'Pix Members' window which you can find in the Pix context menu.");
			ImGuiEx.NoticeText("An editor has full control of syncing Pix properties & the media playback state.");
			ImGuiEx.EndContainer();
		}
		if (ImGuiEx.BeginContainer("Subscribing to a Synced Pix"))
		{
			ImGuiEx.IconText("Open the Sync Search window via the [icon:Search] button in the Main Window.");
			ImGuiEx.NoticeText("You can also open it with the '/pix sync' command.");
			ImGuiEx.SpacingY(4f);
			DrawInfo("If the Pix you are joining is private, you'll need to enter its Pix Id & password in the fields, then click the Join button.");
			ImGuiEx.SpacingY(4f);
			DrawInfo("Alternatively, you can join any public/unlisted pix listed & use the filtering options to search for something specific.");
			ImGuiEx.NoticeText("An 'Unlisted' pix is only visible if you are currently in the same territory that it is spawned in.");
			ImGuiEx.NoticeText("When subscribed, a synced pix will be listed in the Main Window which you can toggle spawning of.");
			ImGuiEx.EndContainer();
		}
		if (ImGuiEx.BeginContainer("What is synced?"))
		{
			ImGuiEx.NoticeText("• Playback state of media (Play/Pause/Seek position)\n• All Pix properties in the Pix Config window, other than those listed as 'Local Properties'.\n• The navigated page.");
			ImGuiEx.SpacingY(4f);
			ImGuiEx.WarningText("Your browser session is not synced, any webpage requiring authentication will require other users to authenticate using their own accounts.");
			ImGuiEx.SpacingY(4f);
			ImGuiEx.WarningText("PyonPix does not stream your browser, syncing works by keeping track of playback state of media. The owner of a Pix is not required to be actively present in order for syncing to function.");
			ImGuiEx.EndContainer();
		}
		if (ImGuiEx.BeginContainer("Alias/Pix Styling & Account Recovery"))
		{
			ImGuiEx.IconText("The User Config window can be opened via the [icon:UserEdit] button to the left of your alias in the Main Window.");
			ImGuiEx.NoticeText("You can also open it with the '/pix user' command.");
			ImGuiEx.SpacingY(4f);
			ImGuiEx.NoticeText("If you ever reset plugin data or lose your AuthKey through other means, you can recover it from the #pyonpix channel on Discord & paste it in this window.");
			ImGuiEx.SpacingY(4f);
			DrawInfo("You can also customize your alias/pix style from this window.");
			ImGuiEx.NoticeText("Animated styling is only available to supporters who help keep the server running.");
			ImGuiEx.IconText("If you would like to support PyonPix, you can open the kofi page via the [icon:Heart] button at the top of the Main Window.");
			ImGuiEx.EndContainer();
		}
		ImGuiEx.SpacingY(8f);
		ImGuiEx.Separator(ImGui.GetContentRegionAvail().X, UIShared.SeparatorSpacing);
		ImGuiEx.IconText("This concludes the guided setup, I hope you learnt something qwq");
		ImGuiEx.NoticeText("If you need help with something else, try asking on the Discord!");
	}

	private void FinishSetup()
	{
		Config.UI.Setup.InitialSetup = false;
		Config.Save();
		((Window)this).IsOpen = false;
	}

	private void DrawInfo(string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ImU8String text2 = ImU8String.op_Implicit(text);
		float? wrapWidth = ImGui.GetContentRegionAvail().X;
		ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, wrapWidth, multiline: true);
	}
}

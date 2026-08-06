using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Structs;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Shared.Utility;
using PyonPix.Structs.Ui;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class UserWindow : BaseWindow
{
	private string? LastAliasError = string.Empty;

	private SyncService SyncService => Services.Get<SyncService>();

	private StateService StateService => Services.Get<StateService>();

	protected override bool ShowTitleBarSettingsButton => false;

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.User.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.User.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(380f, 190f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	public override void OnOpen()
	{
		((Window)this).OnOpen();
		Config.UI.User.IsOpen = true;
		Config.Save();
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
		Config.UI.User.IsOpen = false;
		Config.Save();
	}

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.User.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.User.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		((Window)this).IsOpen = false;
	}

	public UserWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("PyonPix User Config###PyonPixUserConfig", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(380f, 380f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
		SyncService.PremiumStatusChanged += delegate
		{
		};
		SyncService.StateChanged += delegate
		{
		};
		SyncService.StyleUpdateResponse += delegate(bool isSuccess)
		{
			if (isSuccess)
			{
				StatusBar.Show("Changes Applied", 2000, overlay: true);
			}
			else
			{
				LastAliasError = "Someone else stole that Alias.. qwq";
			}
		};
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (((Window)this).IsOpen && StateService.LocalPlayerContentId != 0L)
		{
			ImGui.BeginChild(ImU8String.op_Implicit("##container"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + WindowPadding);
			ImGui.BeginChild(ImU8String.op_Implicit("##content"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
			DrawProperties();
			ImGui.EndChild();
			ImGui.EndChild();
			if (!SyncService.IsConnectedAuth)
			{
				string text = ((SyncService.State != ConnectionState.Connected) ? "Disconnected" : ((!SyncService.Client.IsAuthenticated) ? "Authentication Required" : "Unavailable"));
				StatusBar.Show("Sync Service: " + text, 100, overlay: false, StatusType.Error);
			}
		}
	}

	private void DrawProperties()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0502: Unknown result type (might be due to invalid IL or missing references)
		//IL_088b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af5: Unknown result type (might be due to invalid IL or missing references)
		if (StateService.LocalPlayerContentId == 0L)
		{
			return;
		}
		bool flag = !SyncService.IsConnectedAuth;
		bool isSupporter = SyncService.Client.Premium.IsSupporter;
		bool isSubscriber = SyncService.Client.Premium.IsSubscriber;
		_ = ImGuiHelpers.GlobalScale;
		float num = ImGui.GetContentRegionAvail().X - WindowPadding.X;
		float width = num - IndentWidth;
		CharacterProperties currentCharacterProperties = Config.Sync.GetCurrentCharacterProperties(Config, StateService);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Client AuthKey"), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		ImGui.Indent(IndentWidth);
		if (ImGuiEx.StyledInput(ImU8String.op_Implicit("##secret"), ref Config.Sync.SecretKey, "AuthKey..", SyncService.IsConnectedAuth, 32, width, (ImGuiInputTextFlags)16, null, null, null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0) == UIState.Ended)
		{
			Config.Save();
		}
		Vector3? colorA;
		if (string.IsNullOrEmpty(Config.Sync.SecretKey) || SyncService.Client.IsSecretKeyInvalid)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text = ImU8String.op_Implicit("AuthKey can be retrieved using the 'Data' option in #pyonpix on Discord");
				colorA = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		ImGui.Unindent(IndentWidth);
		ImGuiEx.Separator(num);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Alias Style: "), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		ImGui.SameLine();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(string.IsNullOrEmpty(currentCharacterProperties.Alias) ? "Sample Alias" : currentCharacterProperties.Alias.Trim());
		((ImU8String)(ref val)).AppendLiteral("##sampleAlias");
		ImU8String text2 = val;
		AnimationType aliasAnimationType = currentCharacterProperties.AliasAnimationType;
		colorA = currentCharacterProperties.AliasColourA;
		Vector3? colorB = currentCharacterProperties.AliasColourB;
		Vector3? glowA = currentCharacterProperties.AliasGlowA;
		Vector3? glowB = currentCharacterProperties.AliasGlowB;
		ImGuiEx.StyledText(text2, null, 0.8f, 0f, 4f, 0.2f, aliasAnimationType, colorA, colorB, glowA, glowB, null, null, null, null, float.MaxValue);
		ImGui.Indent(IndentWidth);
		if (ImGuiEx.StyledInput(ImU8String.op_Implicit("##alias"), ref currentCharacterProperties.Alias, "Character Alias..", flag, 20, width, (ImGuiInputTextFlags)16, "Character Alias", "An alias to identify your current character as, visible to other connected users.\n\n- Only supporters are able to change alias.\n- Inappropriate alias may result in termination from the Sync Service.", null, (FontAwesomeIcon)0, null, (FontAwesomeIcon)0) != UIState.None)
		{
			LastAliasError = null;
			if (!NameUtil.ValidateAlias(currentCharacterProperties.Alias, SyncService.Client.Premium, out string error))
			{
				LastAliasError = error;
			}
		}
		if (!isSupporter)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text3 = ImU8String.op_Implicit("Changing Alias requires 'Supporter' role on Pyon Discord");
				glowB = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text3, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, glowB, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		else if (!string.IsNullOrEmpty(LastAliasError))
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text4 = ImU8String.op_Implicit(LastAliasError);
				glowB = new Vector3(0.6f, 0f, 0f);
				glowA = new Vector3(1f, 0f, 0f);
				ImGuiEx.StyledText(text4, null, 0.8f, 0.4f, 4f, 0.1f, AnimationType.Pulse, glowB, glowA, null, null, null, null, null, null, float.MaxValue);
			}
		}
		ImGuiEx.EnumCombo("##aliasAnimType", "Animation Type: ", ref currentCharacterProperties.AliasAnimationType, ComboButtonDisplayType.Items, flag, null, null, 6, width);
		ImGuiEx.ColorPicker3("ColourA##acolA", ref currentCharacterProperties.AliasColourA);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("ColourB##acolB", ref currentCharacterProperties.AliasColourB);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("GlowA##aglowA", ref currentCharacterProperties.AliasGlowA);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("GlowB##aglowB", ref currentCharacterProperties.AliasGlowB);
		if (!isSubscriber && currentCharacterProperties.AliasAnimationType != AnimationType.Static)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text5 = ImU8String.op_Implicit("Animated Alias Style requires 'Subscriber' role on Pyon Discord");
				glowA = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text5, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, glowA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		ImGui.Unindent(IndentWidth);
		ImGuiEx.Separator(num);
		ImGuiEx.StyledText(ImU8String.op_Implicit("Pix Style: "), null, 0.8f, 0f, 4f, 0.2f, AnimationType.Static, null, null, null, null, null, null, null, null, float.MaxValue);
		ImGui.SameLine();
		ImU8String text6 = ImU8String.op_Implicit("Preview Pix##samplePix");
		aliasAnimationType = currentCharacterProperties.PixAnimationType;
		glowA = currentCharacterProperties.PixColourA;
		glowB = currentCharacterProperties.PixColourB;
		colorB = currentCharacterProperties.PixGlowA;
		colorA = currentCharacterProperties.PixGlowB;
		ImGuiEx.StyledText(text6, null, 0.8f, 0f, 4f, 0.2f, aliasAnimationType, glowA, glowB, colorB, colorA, null, null, null, null, float.MaxValue);
		ImGui.Indent(IndentWidth);
		ImGuiEx.EnumCombo("##pixAnimType", "Animation Type: ", ref currentCharacterProperties.PixAnimationType, ComboButtonDisplayType.Items, flag, null, null, 6, width);
		ImGuiEx.ColorPicker3("ColourA##pcolA", ref currentCharacterProperties.PixColourA);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("ColourB##pcolB", ref currentCharacterProperties.PixColourB);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("GlowA##pglowA", ref currentCharacterProperties.PixGlowA);
		ImGui.SameLine();
		ImGuiEx.ColorPicker3("GlowB##pglowB", ref currentCharacterProperties.PixGlowB);
		if (!isSubscriber && currentCharacterProperties.PixAnimationType != AnimationType.Static)
		{
			using (UIShared.SubFont.Push())
			{
				ImU8String text7 = ImU8String.op_Implicit("Animated Pix Style requires 'Subscriber' role on Pyon Discord");
				colorA = UIShared.AccentActive.AsVector3();
				ImGuiEx.StyledText(text7, null, 0.8f, 0.3f, 4f, 0.1f, AnimationType.Static, colorA, null, null, null, null, null, null, null, float.MaxValue);
			}
		}
		ImGui.Unindent(IndentWidth);
		ImGuiEx.Separator(num);
		if (ImGuiEx.IconTextButton((FontAwesomeIcon)61587, "Apply", "##apply", flag || !string.IsNullOrEmpty(LastAliasError), "Apply Alias/Pix Style") && !currentCharacterProperties.Equals(SyncService.Client.Style))
		{
			if (NameUtil.ValidateAlias(currentCharacterProperties.Alias, SyncService.Client.Premium, out string error2))
			{
				Config.Save();
				SyncService.SendStyleUpdate();
				StatusBar.Show("Updating..", 1000, overlay: true);
			}
			else
			{
				LastAliasError = error2;
			}
		}
	}
}

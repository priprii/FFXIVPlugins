using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using PyonPix.Config;
using PyonPix.Config.Global.Properties;
using PyonPix.Services;
using PyonPix.Utility;

namespace PyonPix.Ui;

public static class UIShared
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Action<IFontAtlasBuildToolkitPreBuild> _003C_003E9__132_2;

		public static FontAtlasBuildStepDelegate _003C_003E9__132_0;

		public static Action<IFontAtlasBuildToolkitPreBuild> _003C_003E9__132_3;

		public static FontAtlasBuildStepDelegate _003C_003E9__132_1;

		internal void _003CInitialize_003Eb__132_0(IFontAtlasBuildToolkit e)
		{
			FontAtlasBuildToolkitUtilities.OnPreBuild(e, (Action<IFontAtlasBuildToolkitPreBuild>)delegate(IFontAtlasBuildToolkitPreBuild tk)
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				SafeFontConfig val = default(SafeFontConfig);
				((SafeFontConfig)(ref val))._002Ector();
				((SafeFontConfig)(ref val)).SizePx = NormalFontSize;
				tk.AddFontAwesomeIconFont(ref val);
			});
		}

		internal void _003CInitialize_003Eb__132_2(IFontAtlasBuildToolkitPreBuild tk)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			SafeFontConfig val = default(SafeFontConfig);
			((SafeFontConfig)(ref val))._002Ector();
			((SafeFontConfig)(ref val)).SizePx = NormalFontSize;
			tk.AddFontAwesomeIconFont(ref val);
		}

		internal void _003CInitialize_003Eb__132_1(IFontAtlasBuildToolkit e)
		{
			FontAtlasBuildToolkitUtilities.OnPreBuild(e, (Action<IFontAtlasBuildToolkitPreBuild>)delegate(IFontAtlasBuildToolkitPreBuild tk)
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				SafeFontConfig val = default(SafeFontConfig);
				((SafeFontConfig)(ref val))._002Ector();
				((SafeFontConfig)(ref val)).SizePx = SubFontSize;
				tk.AddFontAwesomeIconFont(ref val);
			});
		}

		internal void _003CInitialize_003Eb__132_3(IFontAtlasBuildToolkitPreBuild tk)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			SafeFontConfig val = default(SafeFontConfig);
			((SafeFontConfig)(ref val))._002Ector();
			((SafeFontConfig)(ref val)).SizePx = SubFontSize;
			tk.AddFontAwesomeIconFont(ref val);
		}
	}

	internal static Configuration Config = null;

	internal static IDalamudTextureWrap? GradientTexture;

	internal static IFontHandle HeaderFont = null;

	internal static float HeaderFontSize = 28f;

	internal static IFontHandle NormalFont = null;

	internal static float NormalFontSize = 16f;

	internal static IFontHandle SubFont = null;

	internal static float SubFontSize = 14f;

	internal static IFontHandle NormalIconFont = null;

	internal static IFontHandle SubIconFont = null;

	internal static Vector4 AccentHovered = UiUtil.RGBA(150, 110, 190, 240f);

	internal static Vector4 AccentActive = UiUtil.RGBA(180, 140, 220, 240f);

	internal static Vector4 Separator = UiUtil.RGBA(50, 42, 50, 220f);

	internal static Vector4 Error = UiUtil.RGBA(240, 40, 40, 240f);

	internal static Vector4 Warn = UiUtil.RGBA(240, 180, 40, 240f);

	public static Vector4 Normal = UiUtil.RGBA(230, 230, 230, 240f);

	public static Vector4 Dimmed = UiUtil.RGBA(200, 200, 200, 240f);

	public static Vector4 Muted = UiUtil.RGBA(170, 170, 170, 240f);

	internal static float WindowRounding = 6f * ImGuiHelpers.GlobalScale;

	internal static Vector4 WindowBgTint = UiUtil.RGBA(230, 150, 230, 250f);

	internal static Vector4 WindowTitle = UiUtil.RGBA(45, 38, 45, 250f);

	internal static Vector4 WindowBorder = UiUtil.RGBA(45, 38, 45, 250f);

	internal static Vector4 BrowserTabInactive = UiUtil.RGBA(180, 180, 180, 180f);

	internal static Vector4 TitleBarBg = UiUtil.RGBA(0, 0, 0, 40f);

	internal static Vector4 PixTerritoryBgNormal = Vector4.Zero;

	internal static Vector4 PixTerritoryBgHovered = UiUtil.RGBA(255, 255, 255, 15f);

	internal static Vector4 PixTerritoryBgActive = UiUtil.RGBA(255, 255, 255, 40f);

	internal static Vector4 PixTerritoryBgExpanded = UiUtil.RGBA(255, 255, 255, 10f);

	internal static Vector4 PixTerritoryBgExpandedHovered = UiUtil.RGBA(100, 100, 100, 50f);

	internal static Vector4 ItemBgHovered = UiUtil.RGBA(255, 255, 255, 10f);

	internal static Vector4 ItemBgActive = UiUtil.RGBA(255, 255, 255, 15f);

	internal static Vector4 ItemHeader = UiUtil.RGBA(240, 240, 240, 240f);

	internal static Vector4 ItemSubText = UiUtil.RGBA(200, 200, 200, 240f);

	internal static Vector4 ItemInactive = UiUtil.RGBA(150, 150, 150, 240f);

	internal static Vector4 ContextMenuBg = UiUtil.RGBA(15, 15, 15, 250f);

	internal static Vector4 ContextMenuBorder = UiUtil.RGBA(45, 38, 45, 230f);

	internal static Vector4 ContextItemBgHovered = UiUtil.RGBA(255, 255, 255, 10f);

	internal static Vector4 ContextItemBgActive = UiUtil.RGBA(255, 255, 255, 15f);

	internal static Vector4 ContextItemTextNormal = UiUtil.RGBA(160, 160, 160, 240f);

	internal static Vector4 ContextItemTextHovered = UiUtil.RGBA(235, 235, 235, 240f);

	internal static float TabRounding = 2f * ImGuiHelpers.GlobalScale;

	internal static Vector4 TabBg = UiUtil.RGBA(20, 20, 20, 100f);

	internal static Vector4 TabBgNormal = UiUtil.RGBA(20, 20, 20, 50f);

	internal static Vector4 TabBgHovered = UiUtil.RGBA(100, 100, 100, 50f);

	internal static Vector4 TabBgClicked = UiUtil.RGBA(255, 255, 255, 40f);

	internal static Vector4 TabBgActive = UiUtil.RGBA(100, 100, 100, 25f);

	internal static Vector4 TabTextNormal = UiUtil.RGBA(140, 140, 140, 240f);

	internal static Vector4 TabTextHovered = UiUtil.RGBA(255, 255, 255, 240f);

	internal static Vector4 TabTextClicked = UiUtil.RGBA(255, 255, 255, 240f);

	internal static Vector4 TabTextActive = UiUtil.RGBA(240, 240, 240, 240f);

	internal static float InputRounding = 4f * ImGuiHelpers.GlobalScale;

	internal static Vector2 InputPadding = new Vector2(10f, 5f);

	internal static Vector4 InputBgNormal = UiUtil.RGBA(60, 60, 60, 50f);

	internal static Vector4 InputBgHovered = UiUtil.RGBA(100, 100, 100, 50f);

	internal static Vector4 InputBgActive = UiUtil.RGBA(140, 140, 140, 50f);

	internal static Vector4 InputBgDisabled = UiUtil.RGBA(0, 0, 0, 80f);

	internal static Vector4 InputTextNormal = UiUtil.RGBA(200, 200, 200, 240f);

	internal static Vector4 InputTextHovered = UiUtil.RGBA(225, 225, 225, 240f);

	internal static Vector4 InputTextActive = UiUtil.RGBA(245, 245, 245, 240f);

	internal static Vector4 InputTextDisabled = UiUtil.RGBA(60, 60, 60, 240f);

	internal static Vector4 InputTextHint = UiUtil.RGBA(160, 160, 160, 240f);

	internal static Vector4 InputBgTextSelected = UiUtil.RGBA(100, 100, 100, 200f);

	internal static Vector4 IconNormal = UiUtil.RGBA(200, 200, 200, 240f);

	internal static Vector4 IconDisabled = UiUtil.RGBA(60, 60, 60, 240f);

	internal static Vector4 IconLabelNormal = UiUtil.RGBA(200, 200, 200, 240f);

	internal static Vector4 IconLabelHovered = UiUtil.RGBA(225, 225, 225, 240f);

	internal static Vector4 IconLabelActive = UiUtil.RGBA(245, 245, 245, 240f);

	internal static Vector4 IconLabelDisabled = UiUtil.RGBA(60, 60, 60, 240f);

	internal static Vector4 IconLabelToggled = UiUtil.RGBA(245, 245, 245, 240f);

	internal static float IconTextRounding = 4f * ImGuiHelpers.GlobalScale;

	internal static float IconTextPadding = 6f;

	internal static Vector4 IconTextNormal = UiUtil.RGBA(200, 200, 200, 240f);

	internal static Vector4 IconTextDisabled = UiUtil.RGBA(60, 60, 60, 240f);

	internal static Vector4 IconTextBgNormal = UiUtil.RGBA(70, 70, 70, 50f);

	internal static Vector4 IconTextBgHovered = UiUtil.RGBA(100, 100, 100, 50f);

	internal static Vector4 IconTextBgClicked = UiUtil.RGBA(255, 255, 255, 40f);

	internal static Vector4 IconTextBgActive = UiUtil.RGBA(100, 100, 100, 25f);

	internal static Vector2 TextBgPadding = new Vector2(4f, 2f) * ImGuiHelpers.GlobalScale;

	internal static float TooltipRounding = 2f * ImGuiHelpers.GlobalScale;

	internal static float TooltipBorderThickness = 1f * ImGuiHelpers.GlobalScale;

	internal static Vector2 TooltipPadding = new Vector2(4f * ImGuiHelpers.GlobalScale);

	internal static Vector4 TooltipBg = UiUtil.RGBA(22, 16, 22, 245f);

	internal static Vector4 TooltipBorder = UiUtil.RGBA(45, 38, 45, 245f);

	internal static Vector4 TooltipText = UiUtil.RGBA(225, 225, 225, 220f);

	internal static Vector4 TooltipSubText = UiUtil.RGBA(200, 200, 200, 220f);

	internal static Vector4 ScrollbarBg = Vector4.Zero;

	internal static Vector4 ScrollbarGrabNormal = UiUtil.RGBA(50, 50, 50, 200f);

	internal static Vector4 ScrollbarGrabHovered = UiUtil.RGBA(70, 70, 70, 200f);

	internal static Vector4 ScrollbarGrabActive = UiUtil.RGBA(60, 60, 60, 200f);

	internal static Vector4 PixTypeLocal = UiUtil.RGBA(200, 200, 200, 255f);

	internal static Vector4 PixRankCoOwner = UiUtil.RGBA(230, 230, 230, 255f);

	internal static Vector4 PixRankMember = UiUtil.RGBA(200, 200, 200, 255f);

	internal static float NormalIconSize => 16f * ImGuiHelpers.GlobalScale;

	internal static float SubIconSize => 14f * ImGuiHelpers.GlobalScale;

	internal static float LineHeight => 26f * ImGuiHelpers.GlobalScale;

	internal static float SeparatorSpacing => 6f * ImGuiHelpers.GlobalScale;

	internal static Vector4 BrowserTabFocused => WindowTitle;

	internal static Vector4 ContextItemTextActive => AccentActive;

	internal static Vector4 ToolBarSeparator => Separator;

	internal static float ComboItemPadding => 4f * ImGuiHelpers.GlobalScale;

	internal static Vector4 DragFgNormal => AccentActive;

	internal static Vector4 DragFgHovered => AccentHovered;

	internal static Vector4 DragFgActive => AccentActive;

	internal static Vector4 DragFgDisabled => InputTextDisabled;

	internal static Vector4 IconHovered => AccentHovered;

	internal static Vector4 IconActive => AccentActive;

	internal static Vector4 IconToggled => AccentActive;

	internal static Vector4 IconTextHovered => AccentHovered;

	internal static Vector4 IconTextActive => AccentActive;

	internal static Vector4 TooltipSeparator => Separator;

	internal static Vector4 PixTypeSynced => AccentActive;

	internal static Vector4 PixRankOwner => AccentActive;

	public static void Initialize(Configuration config, IServiceContext services)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		Config = config;
		Update();
		IUiBuilder uiBuilder = services.PluginInterface.UiBuilder;
		HeaderFont = uiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle((GameFontFamily)5, HeaderFontSize));
		NormalFont = uiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle((GameFontFamily)1, NormalFontSize));
		SubFont = uiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle((GameFontFamily)1, SubFontSize));
		IFontAtlas fontAtlas = uiBuilder.FontAtlas;
		object obj = _003C_003Ec._003C_003E9__132_0;
		if (obj == null)
		{
			FontAtlasBuildStepDelegate val = delegate(IFontAtlasBuildToolkit e)
			{
				FontAtlasBuildToolkitUtilities.OnPreBuild(e, (Action<IFontAtlasBuildToolkitPreBuild>)delegate(IFontAtlasBuildToolkitPreBuild tk)
				{
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					SafeFontConfig val3 = default(SafeFontConfig);
					((SafeFontConfig)(ref val3))._002Ector();
					((SafeFontConfig)(ref val3)).SizePx = NormalFontSize;
					tk.AddFontAwesomeIconFont(ref val3);
				});
			};
			_003C_003Ec._003C_003E9__132_0 = val;
			obj = (object)val;
		}
		NormalIconFont = fontAtlas.NewDelegateFontHandle((FontAtlasBuildStepDelegate)obj);
		IFontAtlas fontAtlas2 = uiBuilder.FontAtlas;
		object obj2 = _003C_003Ec._003C_003E9__132_1;
		if (obj2 == null)
		{
			FontAtlasBuildStepDelegate val2 = delegate(IFontAtlasBuildToolkit e)
			{
				FontAtlasBuildToolkitUtilities.OnPreBuild(e, (Action<IFontAtlasBuildToolkitPreBuild>)delegate(IFontAtlasBuildToolkitPreBuild tk)
				{
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					SafeFontConfig val3 = default(SafeFontConfig);
					((SafeFontConfig)(ref val3))._002Ector();
					((SafeFontConfig)(ref val3)).SizePx = SubFontSize;
					tk.AddFontAwesomeIconFont(ref val3);
				});
			};
			_003C_003Ec._003C_003E9__132_1 = val2;
			obj2 = (object)val2;
		}
		SubIconFont = fontAtlas2.NewDelegateFontHandle((FontAtlasBuildStepDelegate)obj2);
		GradientTexture = CreateGradientTexture(services);
	}

	public static void Update()
	{
		GeneralGlobalProperties general = Config.Global.General;
		WindowBgTint = general.AccentBg;
		WindowTitle = general.AccentTitle;
		AccentHovered = general.AccentHovered;
		AccentActive = general.AccentActive;
	}

	private static IDalamudTextureWrap CreateGradientTexture(IServiceContext services)
	{
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = new byte[8192];
		int[,] array2 = new int[4, 4]
		{
			{ 0, 8, 2, 10 },
			{ 12, 4, 14, 6 },
			{ 3, 11, 1, 9 },
			{ 15, 7, 13, 5 }
		};
		Vector4 value = UiUtil.RGBA(25, 25, 25, 255f);
		Vector4 value2 = new Vector4(value.X * 0.35f, value.Y * 0.35f, value.Z * 0.35f, 1f);
		for (int i = 0; i < 512; i++)
		{
			float amount = (float)i / 511f;
			Vector4 vector = Vector4.Lerp(value, value2, amount);
			for (int j = 0; j < 4; j++)
			{
				float num = ((float)array2[i % 4, j % 4] / 16f - 0.5f) * 0.01f;
				Vector4 vector2 = vector;
				vector2.X = Math.Clamp(vector2.X + num, 0f, 1f);
				vector2.Y = Math.Clamp(vector2.Y + num, 0f, 1f);
				vector2.Z = Math.Clamp(vector2.Z + num, 0f, 1f);
				int num2 = (i * 4 + j) * 4;
				array[num2] = (byte)(vector2.X * 255f);
				array[num2 + 1] = (byte)(vector2.Y * 255f);
				array[num2 + 2] = (byte)(vector2.Z * 255f);
				array[num2 + 3] = byte.MaxValue;
			}
		}
		return services.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(4, 512), (ReadOnlySpan<byte>)array, (string)null);
	}

	public static void Dispose()
	{
		((IDisposable)GradientTexture)?.Dispose();
	}
}

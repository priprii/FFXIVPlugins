using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using GLib.Widgets;
using Ktisis.Data.Files;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Popup;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Types;
using Ktisis.Services.Data;
using Ktisis.Structs.Characters;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.Windows.Editors;

public class SceneWindow : KtisisWindow
{
	private readonly SceneDataService _sceneDataService;

	private readonly ISceneManager _scene;

	private readonly IEditorContext _ctx;

	private readonly ITextureProvider _textureProvider;

	private readonly IDataManager _dataManager;

	private bool _autosave;

	private SceneFile? _sceneFile;

	private ISharedImmediateTexture? _texture;

	private Map _source;

	private SceneMCDFModal? _popupWindow;

	private bool _includeActors;

	private bool _includeLights;

	private bool _includeCameras;

	private bool _includeEnv;

	private bool _includeOverlays;

	private bool _preserveActors;

	public SceneWindow(IEditorContext ctx, ITextureProvider textureProvider, IDataManager dataManager)
		: base("Scene Editor", (ImGuiWindowFlags)0)
	{
		_sceneDataService = ctx.Scene.Data;
		_scene = ctx.Scene;
		_ctx = ctx;
		_sceneFile = null;
		_dataManager = dataManager;
		_textureProvider = textureProvider;
		_includeActors = (_includeCameras = (_includeLights = (_includeEnv = (_includeOverlays = true))));
		_preserveActors = false;
	}

	public override void PreOpenCheck()
	{
		if (!_scene.IsValid)
		{
			Ktisis.Log.Verbose("State for scene editor is stale, closing...");
			Close();
		}
	}

	public override void PreDraw()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).PreDraw();
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(400f, 400f);
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		((Window)this).SizeConstraints = value;
	}

	private void OpenPopupModal(SceneFile.ActorInfo entity)
	{
		_popupWindow = _ctx.Plugin.Gui.CreatePopup<SceneMCDFModal>(new object[2] { entity, _ctx });
		_popupWindow.SetScene(ref _sceneFile);
		_popupWindow.Open();
	}

	private void MapStuff()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		uint num = _sceneDataService.GetCurrentMapID();
		if (_sceneFile != null)
		{
			num = _sceneFile.MapID;
		}
		_dataManager.GetExcelSheet<Map>((ClientLanguage?)null, (string)null).TryGetRow(num, ref _source);
		TerritoryType val = default(TerritoryType);
		_dataManager.GetExcelSheet<TerritoryType>((ClientLanguage?)null, (string)null).TryGetRow(((Map)(ref _source)).TerritoryType.RowId, ref val);
		LoadingImage? valueNullable = ((TerritoryType)(ref val)).LoadingImage.ValueNullable;
		ReadOnlySeString? val2;
		LoadingImage valueOrDefault;
		if (!valueNullable.HasValue)
		{
			val2 = null;
		}
		else
		{
			valueOrDefault = valueNullable.GetValueOrDefault();
			val2 = ((LoadingImage)(ref valueOrDefault)).FileName;
		}
		ReadOnlySeString? val3 = val2;
		if (!val3.HasValue)
		{
			return;
		}
		ReadOnlySeString valueOrDefault2 = val3.GetValueOrDefault();
		if (!((ReadOnlySeString)(ref valueOrDefault2)).IsEmpty)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
			defaultInterpolatedStringHandler.AppendLiteral("ui/loadingimage/");
			valueNullable = ((TerritoryType)(ref val)).LoadingImage.ValueNullable;
			ReadOnlySeString? value;
			if (!valueNullable.HasValue)
			{
				value = null;
			}
			else
			{
				valueOrDefault = valueNullable.GetValueOrDefault();
				value = ((LoadingImage)(ref valueOrDefault)).FileName;
			}
			defaultInterpolatedStringHandler.AppendFormatted(value);
			defaultInterpolatedStringHandler.AppendLiteral("_hr1.tex");
			string text = defaultInterpolatedStringHandler.ToStringAndClear();
			_texture = _textureProvider.GetFromGame(text);
		}
	}

	public override void Draw()
	{
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0876: Unknown result type (might be due to invalid IL or missing references)
		//IL_087b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0891: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_099e: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_091d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c66: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0835: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c96: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0957: Unknown result type (might be due to invalid IL or missing references)
		//IL_095c: Unknown result type (might be due to invalid IL or missing references)
		//IL_096c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad2: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a22: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_079f: Unknown result type (might be due to invalid IL or missing references)
		MapStuff();
		float num = UiBuilder.DefaultFontSizePx * ImGuiHelpers.GlobalScale * 2f;
		Vector2 vector = new Vector2(num, num);
		int num2;
		int num3;
		int num4;
		int num5;
		bool flag;
		if (_sceneFile != null)
		{
			num2 = _sceneFile.Actors.Count;
			num3 = _sceneFile.Cameras.Count;
			num4 = _sceneFile.Lights.Count;
			num5 = _sceneFile.Overlays.Count;
			flag = _sceneFile.Environment.Override != 0;
		}
		else
		{
			num2 = _ctx.Scene.Children.Count((SceneEntity entity) => entity is CharaEntity);
			num4 = _ctx.Scene.Children.Count((SceneEntity entity) => entity is LightEntity);
			num5 = _ctx.Scene.Children.Count((SceneEntity entity) => entity is OverlayEntity);
			num3 = _ctx.Cameras.GetCameras().Count();
			flag = _ctx.Scene.GetModule<EnvModule>().Override > EnvOverride.None;
		}
		ImGui.BeginGroup();
		if (Buttons.IconButtonTooltip((FontAwesomeIcon)58683, "Load Scene file", vector * 1.5f))
		{
			_ctx.Interface.OpenSceneFile(delegate(string s)
			{
				_sceneFile = _ctx.Scene.Data.LoadFile(s);
			});
		}
		DisabledDisposable val = ImRaii.Disabled(_sceneFile != null);
		try
		{
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61639, ((_sceneFile == null) ? "Save Scene file" : "Unload current Scene before saving") ?? "", vector * 1.5f))
			{
				_ctx.Interface.ExportSceneFile(_ctx.Scene.Data.Save(_includeActors, _includeLights, _includeCameras, _includeEnv, _includeOverlays));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		ImGuiStylePtr style;
		if (_sceneFile != null)
		{
			float windowHeight = ImGui.GetWindowHeight();
			style = ImGui.GetStyle();
			float num6 = windowHeight + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y;
			float num7 = vector.Y * 1.5f;
			style = ImGui.GetStyle();
			float num8 = (num7 + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y) * 3f;
			style = ImGui.GetStyle();
			ImGui.SetCursorPosY(num6 - (num8 + ((ImGuiStylePtr)(ref style)).WindowPadding.Y));
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61453, "Unload File", vector * 1.5f))
			{
				_sceneFile = null;
			}
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)(_autosave ? 61612 : 58287), "Choose coordinate type\nCurrently: " + (_autosave ? "World space" : "Local space"), vector * 1.5f))
			{
				_autosave = !_autosave;
			}
			if (Buttons.IconButtonTooltip((FontAwesomeIcon)61452, "Apply Scene", vector * 1.5f))
			{
				_sceneDataService.Load(_sceneFile, _autosave, _includeActors, _includeLights, _includeCameras, _includeEnv, _includeOverlays, _preserveActors);
				_sceneFile = null;
			}
		}
		ImGui.EndGroup();
		ImGui.SameLine();
		ImGui.PushStyleColor((ImGuiCol)3, new Vector4(74f, 74f, 74f, 138f) / 255f);
		ImGui.PushStyleVar((ImGuiStyleVar)6, 4f);
		ImU8String val2 = ImU8String.op_Implicit("##SceneData");
		Vector2 vector2;
		if (!_ctx.Config.Editor.UseToolbar)
		{
			vector2 = Vector2.Zero;
		}
		else
		{
			float x = ImGui.GetContentRegionAvail().X - 1f;
			float y = ImGui.GetContentRegionAvail().Y;
			style = ImGui.GetStyle();
			vector2 = new Vector2(x, Math.Clamp(y - ((ImGuiStylePtr)(ref style)).WindowPadding.Y, 480f, 900f));
		}
		ChildDisposable val3 = ImRaii.Child(val2, vector2, false, (ImGuiWindowFlags)64);
		try
		{
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			if (val3.Success)
			{
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				if (_texture != null)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddImageRounded(_texture.GetWrapOrEmpty().Handle, cursorScreenPos, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().X * 0.563f) + cursorScreenPos, Vector2.Zero, Vector2.One, uint.MaxValue, 4f);
				}
				TextWrapDisposable val4 = ImRaii.TextWrapPos(ImGui.GetWindowContentRegionMax().X);
				try
				{
					if (_sceneFile == null)
					{
						ImGui.PushStyleColor((ImGuiCol)0, ImGuiColors.HealerGreen);
					}
					else
					{
						ImGui.PushStyleColor((ImGuiCol)0, (((Map)(ref _source)).RowId == _sceneDataService.GetCurrentMapID()) ? ImGuiColors.HealerGreen : ImGuiColors.DPSRed);
					}
					float num9 = ImGui.GetContentRegionAvail().X * 0.563f;
					style = ImGui.GetStyle();
					ImGui.SetCursorPosY(num9 + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y);
					ImGui.BeginGroup();
					ImU8String val5 = new ImU8String(6, 1);
					((ImU8String)(ref val5)).AppendLiteral("From: ");
					PlaceName value = ((Map)(ref _source)).PlaceName.Value;
					((ImU8String)(ref val5)).AppendFormatted<ReadOnlySeString>(((PlaceName)(ref value)).Name);
					ImGui.TextUnformatted(val5);
					ImGui.PopStyleColor();
					ImU8String val6 = new ImU8String(7, 1);
					((ImU8String)(ref val6)).AppendLiteral("Actors ");
					((ImU8String)(ref val6)).AppendFormatted<int>(num2);
					if (ImGui.CollapsingHeader(val6, (ImGuiTreeNodeFlags)0))
					{
						SceneFile sceneFile = _sceneFile;
						if (sceneFile != null)
						{
							List<SceneFile.ActorInfo> actors = sceneFile.Actors;
							if (actors != null && actors.Count > 0)
							{
								ImGui.Checkbox(ImU8String.op_Implicit("Load actors"), ref _includeActors);
								if (_includeActors)
								{
									ImGui.SameLine();
									ImGui.Checkbox(ImU8String.op_Implicit("Keep existing actors"), ref _preserveActors);
								}
								ImGui.Indent();
								foreach (SceneFile.ActorInfo actor in _sceneFile.Actors)
								{
									if (!_ctx.Config.Editor.IncognitoPlayerNames)
									{
										ImU8String val7 = new ImU8String(0, 1);
										((ImU8String)(ref val7)).AppendFormatted<string>(actor.Chara.Nickname);
										ImGui.TextUnformatted(val7);
									}
									else
									{
										ImU8String val8 = new ImU8String(3, 1);
										((ImU8String)(ref val8)).AppendLiteral("A ");
										((ImU8String)(ref val8)).AppendFormatted<string>(((int)actor.Chara.Race.Value <= 8) ? (((actor.Chara.Gender == Gender.Masculine) ? "♂" : "♀") + actor.Chara.Race.ToString()) : "Non-Humanoid Actor");
										((ImU8String)(ref val8)).AppendLiteral(" ");
										ImGui.TextUnformatted(val8);
									}
									if (!(actor.MCDF != string.Empty) || _sceneDataService.ValidMCDFPath(actor))
									{
										continue;
									}
									ImGui.SameLine();
									FontDisposable val9 = ImRaii.PushFont(UiBuilder.IconFont, true);
									try
									{
										Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
										ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61553)));
									}
									finally
									{
										((IDisposable)val9)?.Dispose();
									}
									if (ImGui.IsItemHovered())
									{
										TooltipDisposable val10 = ImRaii.Tooltip();
										try
										{
											ImGui.TextUnformatted(ImU8String.op_Implicit("MCDF wasnt found for this character\nPlease try applying manually after loading the scene"));
										}
										finally
										{
											((TooltipDisposable)(ref val10)).Dispose();
										}
									}
								}
								goto IL_0868;
							}
						}
						ImGui.Checkbox(ImU8String.op_Implicit("Save actors"), ref _includeActors);
						ImGui.Indent();
						foreach (SceneEntity item in _ctx.Scene.Children.Where((SceneEntity entity) => entity is CharaEntity))
						{
							ImU8String val11 = new ImU8String(0, 1);
							((ImU8String)(ref val11)).AppendFormatted<string>(item.Name);
							ImGui.TextUnformatted(val11);
						}
						goto IL_0868;
					}
					goto IL_086d;
					IL_0868:
					ImGui.Unindent();
					goto IL_086d;
					IL_0af0:
					ImGui.Unindent();
					goto IL_0af5;
					IL_0af5:
					if (num5 > 0)
					{
						ImU8String val12 = new ImU8String(9, 1);
						((ImU8String)(ref val12)).AppendLiteral("Overlays ");
						((ImU8String)(ref val12)).AppendFormatted<int>(num5);
						if (ImGui.CollapsingHeader(val12, (ImGuiTreeNodeFlags)0))
						{
							SceneFile sceneFile = _sceneFile;
							if (sceneFile != null)
							{
								List<SceneFile.OverlayInfo> overlays = sceneFile.Overlays;
								if (overlays != null && overlays.Count > 0)
								{
									ImGui.Checkbox(ImU8String.op_Implicit("Load Overlays"), ref _includeOverlays);
									ImGui.Indent();
									foreach (SceneFile.OverlayInfo overlay in _sceneFile.Overlays)
									{
										ImU8String val13 = new ImU8String(0, 1);
										((ImU8String)(ref val13)).AppendFormatted<string>(overlay.Name);
										ImGui.TextUnformatted(val13);
									}
									goto IL_0c57;
								}
							}
							ImGui.Checkbox(ImU8String.op_Implicit("Save Overlays"), ref _includeOverlays);
							ImGui.Indent();
							foreach (SceneEntity item2 in _ctx.Scene.Children.Where((SceneEntity entity) => entity is OverlayEntity))
							{
								ImU8String val14 = new ImU8String(0, 1);
								((ImU8String)(ref val14)).AppendFormatted<string>(item2.Name);
								ImGui.TextUnformatted(val14);
							}
							goto IL_0c57;
						}
					}
					goto IL_0c5c;
					IL_0c5c:
					if (flag)
					{
						ImU8String val15 = new ImU8String(11, 0);
						((ImU8String)(ref val15)).AppendLiteral("Environment");
						if (ImGui.CollapsingHeader(val15, (ImGuiTreeNodeFlags)0))
						{
							if (_sceneFile != null)
							{
								ImGui.Checkbox(ImU8String.op_Implicit("Load Environment"), ref _includeEnv);
								ImGui.Indent();
								EnvOverride envOverride = (EnvOverride)_sceneFile.Environment.Override;
								List<string> list = new List<string>();
								foreach (EnvOverride item3 in Enum.GetValues<EnvOverride>().Except(new global::_003C_003Ez__ReadOnlySingleElementList<EnvOverride>(EnvOverride.None)))
								{
									switch (envOverride)
									{
									case EnvOverride.SkyId:
										list.Add("Sky");
										continue;
									case EnvOverride.Dust:
										list.Add("Particles");
										continue;
									}
									if (envOverride.HasFlag(item3))
									{
										list.Add(Enum.GetName(item3));
									}
								}
								ImGui.TextUnformatted(ImU8String.op_Implicit(string.Join(", ", list)));
							}
							else
							{
								ImGui.Checkbox(ImU8String.op_Implicit("Save Environment"), ref _includeEnv);
								ImGui.Indent();
								EnvOverride envOverride2 = _ctx.Scene.GetModule<EnvModule>().Override;
								List<string> list2 = new List<string>();
								foreach (EnvOverride item4 in Enum.GetValues<EnvOverride>().Except(new global::_003C_003Ez__ReadOnlySingleElementList<EnvOverride>(EnvOverride.None)))
								{
									switch (envOverride2)
									{
									case EnvOverride.SkyId:
										list2.Add("Sky");
										continue;
									case EnvOverride.Dust:
										list2.Add("Particles");
										continue;
									}
									if (envOverride2.HasFlag(item4))
									{
										list2.Add(Enum.GetName(item4));
									}
								}
								ImGui.TextUnformatted(ImU8String.op_Implicit(string.Join(", ", list2)));
							}
							ImGui.Unindent();
						}
					}
					ImGui.EndGroup();
					goto end_IL_04e8;
					IL_0c57:
					ImGui.Unindent();
					goto IL_0c5c;
					IL_086d:
					if (num3 > 0)
					{
						ImU8String val16 = new ImU8String(8, 1);
						((ImU8String)(ref val16)).AppendLiteral("Cameras ");
						((ImU8String)(ref val16)).AppendFormatted<int>(num3);
						if (ImGui.CollapsingHeader(val16, (ImGuiTreeNodeFlags)0))
						{
							if (_sceneFile != null)
							{
								ImGui.Checkbox(ImU8String.op_Implicit("Load cameras"), ref _includeCameras);
								ImGui.Indent();
								foreach (SceneFile.CameraInfo camera in _sceneFile.Cameras)
								{
									ImU8String val17 = new ImU8String(0, 1);
									((ImU8String)(ref val17)).AppendFormatted<string>(camera.Name);
									ImGui.TextUnformatted(val17);
								}
							}
							else
							{
								ImGui.Checkbox(ImU8String.op_Implicit("Save cameras"), ref _includeCameras);
								ImGui.Indent();
								foreach (EditorCamera camera2 in _ctx.Cameras.GetCameras())
								{
									ImU8String val18 = new ImU8String(0, 1);
									((ImU8String)(ref val18)).AppendFormatted<string>(camera2.Name);
									ImGui.TextUnformatted(val18);
								}
							}
							ImGui.Unindent();
						}
					}
					if (num4 > 0)
					{
						ImU8String val19 = new ImU8String(7, 1);
						((ImU8String)(ref val19)).AppendLiteral("Lights ");
						((ImU8String)(ref val19)).AppendFormatted<int>(num4);
						if (ImGui.CollapsingHeader(val19, (ImGuiTreeNodeFlags)0))
						{
							SceneFile sceneFile = _sceneFile;
							if (sceneFile != null)
							{
								List<SceneFile.LightInfo> lights = sceneFile.Lights;
								if (lights != null && lights.Count > 0)
								{
									ImGui.Checkbox(ImU8String.op_Implicit("Load lights"), ref _includeLights);
									ImGui.Indent();
									foreach (SceneFile.LightInfo light in _sceneFile.Lights)
									{
										ImU8String val20 = new ImU8String(0, 1);
										((ImU8String)(ref val20)).AppendFormatted<string>(light.Name);
										ImGui.TextUnformatted(val20);
									}
									goto IL_0af0;
								}
							}
							ImGui.Checkbox(ImU8String.op_Implicit("Save lights"), ref _includeLights);
							ImGui.Indent();
							foreach (SceneEntity item5 in _ctx.Scene.Children.Where((SceneEntity entity) => entity is LightEntity))
							{
								ImU8String val21 = new ImU8String(0, 1);
								((ImU8String)(ref val21)).AppendFormatted<string>(item5.Name);
								ImGui.TextUnformatted(val21);
							}
							goto IL_0af0;
						}
					}
					goto IL_0af5;
					end_IL_04e8:;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			ImGui.Dummy(new Vector2(2f));
		}
		finally
		{
			((ChildDisposable)(ref val3)).Dispose();
		}
		ImGui.PopStyleColor();
		ImGui.PopStyleVar();
	}
}

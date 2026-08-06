using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using GLib.Widgets;
using Ktisis.Interface.Components.Environment;
using Ktisis.Interface.Components.Environment.Editors;
using Ktisis.Interface.Types;
using Ktisis.Interface.Widgets.Environment;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Types;
using Ktisis.Services.Environment;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Windows.Editors;

public class EnvWindow : KtisisWindow
{
	internal enum EnvEditorTab
	{
		None,
		Sky,
		Light,
		Fog,
		Rain,
		Particles,
		Stars,
		Wind,
		Water,
		Housing
	}

	private readonly ISceneManager _scene;

	private readonly IEnvModule _module;

	private readonly WeatherSelect _weatherSelect;

	private protected EnvEditorTab Current;

	private readonly Dictionary<EnvEditorTab, EditorBase> _editors = new Dictionary<EnvEditorTab, EditorBase>();

	public EnvWindow(ISceneManager scene, IEnvModule module, WeatherSelect weatherSelect, SkyEditor sky, LightingEditor lighting, FogEditor fog, RainEditor rain, ParticlesEditor dust, StarsEditor stars, WindEditor wind, WaterEditor water, HousingEditor housingEditor)
		: base("env_edit.title", (ImGuiWindowFlags)0, "###KtisisEnvWindow")
	{
		_scene = scene;
		_module = module;
		_weatherSelect = weatherSelect;
		Setup(EnvEditorTab.Sky, sky).Setup(EnvEditorTab.Light, lighting).Setup(EnvEditorTab.Fog, fog).Setup(EnvEditorTab.Rain, rain)
			.Setup(EnvEditorTab.Particles, dust)
			.Setup(EnvEditorTab.Stars, stars)
			.Setup(EnvEditorTab.Wind, wind)
			.Setup(EnvEditorTab.Water, water)
			.Setup(EnvEditorTab.Housing, housingEditor);
	}

	private EnvWindow Setup(EnvEditorTab id, EditorBase editor)
	{
		_editors.Add(id, editor);
		return this;
	}

	public override void PreOpenCheck()
	{
		if (!_scene.IsValid || !_module.IsInit)
		{
			Ktisis.Log.Verbose("State for env editor is stale, closing...");
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
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(400f, 300f);
		ImGuiIOPtr iO = ImGui.GetIO();
		((WindowSizeConstraints)(ref value)).MaximumSize = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		((Window)this).SizeConstraints = value;
	}

	public unsafe override void Draw()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		EnvManagerEx* ptr = EnvManagerEx.Instance();
		if (ptr != null)
		{
			DrawSideBar(ptr);
			if (Current != EnvEditorTab.None)
			{
				ImGuiStylePtr style = ImGui.GetStyle();
				ImGui.SameLine(0f, (((ImGuiStylePtr)(ref style)).ItemSpacing + ((ImGuiStylePtr)(ref style)).FramePadding / 2f).X);
				DrawAdvancedEditor(ptr);
			}
		}
	}

	private unsafe void DrawSideBar(EnvManagerEx* env)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		contentRegionAvail.X *= 0.35f;
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##EnvWeather"), contentRegionAvail);
		try
		{
			DrawWeatherTimeControls(env, contentRegionAvail.X);
			DrawAdvancedList();
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}

	private protected unsafe void DrawWeatherTimeControls(EnvManagerEx* env, float width)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.weather")));
		if (_weatherSelect.Draw(env, out WeatherInfo selected) && selected != null)
		{
			byte b = (byte)selected.RowId;
			_module.Weather = b;
			((EnvManager)(&env->_base)).ActiveWeather = b;
		}
		ImGui.Spacing();
		bool num = _module.Override.HasFlag(EnvOverride.TimeWeather);
		if (Buttons.IconButton((FontAwesomeIcon)(num ? 61475 : 61596)))
		{
			_module.Weather = ((EnvManager)(&env->_base)).ActiveWeather;
			_module.Time = ((EnvManager)(&env->_base)).DayTimeSeconds;
			_module.Day = DayTimeControls.CalculateDay(env);
			_module.Override ^= EnvOverride.TimeWeather;
		}
		ImGui.SameLine();
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.time")));
		DisabledDisposable val = ImRaii.Disabled(!num);
		try
		{
			if (DayTimeControls.DrawTime(env, out var time))
			{
				_module.Time = time;
			}
			ImGui.SetNextItemWidth(width);
			if (DayTimeControls.DrawDay(env, out var day))
			{
				_module.Day = day;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private protected void DrawAdvancedList()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("env_edit.advanced")));
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		ref float y = ref contentRegionAvail.Y;
		float num = y;
		ImGuiStylePtr style = ImGui.GetStyle();
		y = num - ((ImGuiStylePtr)(ref style)).WindowPadding.Y / 2f;
		ListBoxDisposable val = ImRaii.ListBox(ImU8String.op_Implicit("##AdvancedOptions"), contentRegionAvail);
		try
		{
			if (!val.Success)
			{
				return;
			}
			foreach (KeyValuePair<EnvEditorTab, EditorBase> editor in _editors)
			{
				editor.Deconstruct(out var key, out var value);
				EnvEditorTab envEditorTab = key;
				EditorBase editorBase = value;
				bool flag = editorBase.IsActivated(_module.Override);
				ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)0, 2147483647u, !flag);
				try
				{
					bool flag2 = envEditorTab == Current;
					if (ImGui.Selectable(ImU8String.op_Implicit(editorBase.Name), flag2, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						Current = ((!flag2) ? envEditorTab : EnvEditorTab.None);
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}
		finally
		{
			((ListBoxDisposable)(ref val)).Dispose();
		}
	}

	private protected unsafe void DrawAdvancedEditor(EnvManagerEx* env)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##AdvancedFrame"), _scene.Context.Config.Editor.UseToolbar ? new Vector2(300f * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y) : ImGui.GetContentRegionAvail());
		try
		{
			if (val.Success && _editors.TryGetValue(Current, out EditorBase value))
			{
				ImGui.Text(ImU8String.op_Implicit(value.Name));
				ImGui.Separator();
				ImGui.Spacing();
				if (value is WaterEditor waterEditor)
				{
					waterEditor.Draw();
				}
				else
				{
					value.Draw(_module, ref env->EnvState);
				}
			}
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}
}

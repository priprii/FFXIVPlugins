using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Interface.Components.Environment;
using Ktisis.Interface.Components.Environment.Editors;
using Ktisis.Interface.Windows.Editors;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Types;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Windows.ToolbarModules;

public class Env : EnvWindow
{
	public Env(ISceneManager scene, IEnvModule module, WeatherSelect weatherSelect, SkyEditor sky, LightingEditor lighting, FogEditor fog, RainEditor rain, ParticlesEditor dust, StarsEditor stars, WindEditor wind, WaterEditor water, HousingEditor housingEditor)
		: base(scene, module, weatherSelect, sky, lighting, fog, rain, dust, stars, wind, water, housingEditor)
	{
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = new Vector2(400f, 400f) * ImGuiHelpers.GlobalScale;
		vector.X *= 0.35f;
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##EnvWeather"), vector);
		try
		{
			DrawWeatherTimeControls(env, vector.X);
			DrawAdvancedList();
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using Ktisis.Core.Attributes;
using Ktisis.Services.Environment;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment;

[Transient]
public class WeatherSelect
{
	private class WeatherResource(WeatherService service)
	{
		private uint TerritoryId;

		private readonly List<WeatherInfo> Cached = new List<WeatherInfo>();

		private CancellationTokenSource? TokenSource;

		public IEnumerable<WeatherInfo> Get(uint territory)
		{
			if (territory != TerritoryId)
			{
				TerritoryId = territory;
				Fetch();
			}
			lock (Cached)
			{
				return Cached;
			}
		}

		public WeatherInfo? Find(int rowId)
		{
			lock (Cached)
			{
				return Cached.Find((WeatherInfo row) => row.RowId == rowId);
			}
		}

		private void Fetch()
		{
			TokenSource?.Dispose();
			TokenSource = new CancellationTokenSource();
			service.GetWeatherTypes(TokenSource.Token).ContinueWith(delegate(Task<IEnumerable<WeatherInfo>> task)
			{
				if (task.Exception != null)
				{
					Ktisis.Log.Error($"Failed to fetch weather:\n{task.Exception}");
					return;
				}
				lock (Cached)
				{
					Cached.Clear();
					Cached.AddRange(task.Result);
				}
			});
		}
	}

	private static readonly Vector2 WeatherIconSize = new Vector2(28f, 28f);

	private readonly IClientState _clientState;

	private readonly WeatherService _weather;

	private readonly WeatherResource _resource;

	public WeatherSelect(IClientState clientState, WeatherService weather)
	{
		_clientState = clientState;
		_weather = weather;
		_resource = new WeatherResource(weather);
	}

	public unsafe bool Draw(EnvManagerEx* env, out WeatherInfo? selected)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		selected = null;
		if (env == null)
		{
			return false;
		}
		IEnumerable<WeatherInfo> weathers = _resource.Get(_clientState.TerritoryType);
		byte activeWeather = ((EnvManager)(&env->_base)).ActiveWeather;
		WeatherInfo weatherInfo = _resource.Find(activeWeather);
		ImGuiStylePtr style = ImGui.GetStyle();
		float y = (((ImGuiStylePtr)(ref style)).FramePadding.Y + WeatherIconSize.Y) * ImGuiHelpers.GlobalScale - ImGui.GetFrameHeight();
		Vector2 framePadding = ((ImGuiStylePtr)(ref style)).FramePadding;
		framePadding.Y = y;
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)10, framePadding, true);
		try
		{
			bool result = DrawWeatherCombo(activeWeather, weatherInfo, weathers, out selected);
			if (weatherInfo != null)
			{
				DrawWeatherLabel(weatherInfo, adjustPad: false);
			}
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool DrawWeatherCombo(byte id, WeatherInfo? current, IEnumerable<WeatherInfo> weathers, out WeatherInfo? selected)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		selected = null;
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		ComboDisposable val = ImRaii.Combo(ImU8String.op_Implicit("##WeatherCombo"), ImU8String.op_Implicit((current != null) ? "##" : "Unknown"));
		try
		{
			if (!val.Success)
			{
				return false;
			}
			bool flag = false;
			foreach (WeatherInfo weather in weathers)
			{
				ImU8String val2 = new ImU8String(13, 1);
				((ImU8String)(ref val2)).AppendLiteral("##EnvWeather_");
				((ImU8String)(ref val2)).AppendFormatted<uint>(weather.RowId);
				bool flag2 = ImGui.Selectable(val2, weather.RowId == id, (ImGuiSelectableFlags)0, default(Vector2));
				DrawWeatherLabel(weather, adjustPad: true);
				if (flag2)
				{
					selected = weather;
				}
				flag = flag || flag2;
			}
			return flag;
		}
		finally
		{
			((ComboDisposable)(ref val)).Dispose();
		}
	}

	private void DrawWeatherLabel(WeatherInfo weather, bool adjustPad)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float frameHeight = ImGui.GetFrameHeight();
		if (weather.Icon != null)
		{
			ImGui.SameLine(0f, 0f);
			ImGui.SetCursorPosX(ImGui.GetCursorStartPos().X + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			float num = ImGui.GetCursorPosY() + frameHeight / 2f - WeatherIconSize.Y / 2f;
			if (adjustPad)
			{
				num -= ((ImGuiStylePtr)(ref style)).FramePadding.Y;
			}
			ImGui.SetCursorPosY(num);
			ImGui.Image(weather.Icon.GetWrapOrEmpty().Handle, WeatherIconSize);
			ImGui.SameLine();
		}
		ImGui.Text(ImU8String.op_Implicit(weather.Name));
	}
}

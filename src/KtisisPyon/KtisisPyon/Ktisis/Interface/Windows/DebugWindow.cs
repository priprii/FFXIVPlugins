using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Ktisis.Common.Utility;
using Ktisis.Data.Config;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Interface.Overlay;
using Ktisis.Interface.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Services.Plugin;
using Newtonsoft.Json;

namespace Ktisis.Interface.Windows;

public class DebugWindow : KtisisWindow
{
	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private readonly TransformTable _transformTable;

	private int _gameObjectId;

	private bool _hasClip;

	private (int, int)? _apiVersion;

	private bool? _isPosing;

	private bool? _lastPosingEventValue;

	private string? _lastPosingEventTime;

	private Transform _transform = new Transform();

	private string _boneName = string.Empty;

	private bool _useWorldSpace = true;

	private string _batchBoneNames = "j_kao";

	private readonly ICallGateSubscriber<(int, int)> _ktisisApiVersion;

	private readonly ICallGateSubscriber<bool> _ktisisRefreshActors;

	private readonly ICallGateSubscriber<bool> _ktisisIsPosing;

	private readonly ICallGateSubscriber<uint, string, Task<bool>> _ktisisLoadPose;

	private readonly ICallGateSubscriber<uint, Task<string?>> _ktisisSavePose;

	private readonly ICallGateSubscriber<Task<Dictionary<int, HashSet<string>>>> _ktisisSelectedBones;

	private readonly ICallGateSubscriber<bool, bool> _ktisisPosingChanged;

	private readonly ICallGateSubscriber<uint, string, bool, Task<Matrix4x4?>> _ktisisGetMatrix;

	private readonly ICallGateSubscriber<uint, string, Matrix4x4, bool, Task<bool>> _ktisisSetMatrix;

	private readonly ICallGateSubscriber<uint, List<string>, bool, Task<Dictionary<string, Matrix4x4?>>> _ktisisBatchGetMatrix;

	private readonly ICallGateSubscriber<uint, bool, Task<Dictionary<string, Matrix4x4?>>> _ktisisGetAllMatrices;

	private readonly ICallGateSubscriber<uint, Dictionary<string, Matrix4x4>, bool, Task<bool>> _ktisisBatchSetMatrix;

	public DebugWindow(IEditorContext ctx, GuiManager gui, IDalamudPluginInterface dpi, ConfigManager cfg, LocaleManager locale)
		: base("Debug Window", (ImGuiWindowFlags)0, "###KtisisDebug")
	{
		_ctx = ctx;
		_gui = gui;
		_ktisisApiVersion = dpi.GetIpcSubscriber<(int, int)>("Ktisis.ApiVersion");
		_ktisisRefreshActors = dpi.GetIpcSubscriber<bool>("Ktisis.RefreshActors");
		_ktisisIsPosing = dpi.GetIpcSubscriber<bool>("Ktisis.IsPosing");
		_ktisisLoadPose = dpi.GetIpcSubscriber<uint, string, Task<bool>>("Ktisis.LoadPose");
		_ktisisSavePose = dpi.GetIpcSubscriber<uint, Task<string>>("Ktisis.SavePose");
		_ktisisSelectedBones = dpi.GetIpcSubscriber<Task<Dictionary<int, HashSet<string>>>>("Ktisis.SelectedBones");
		_ktisisPosingChanged = dpi.GetIpcSubscriber<bool, bool>("Ktisis.PosingChanged");
		_ktisisPosingChanged.Subscribe((Action<bool>)SetLastPosingChanged);
		_ktisisGetMatrix = dpi.GetIpcSubscriber<uint, string, bool, Task<Matrix4x4?>>("Ktisis.GetMatrix");
		_ktisisSetMatrix = dpi.GetIpcSubscriber<uint, string, Matrix4x4, bool, Task<bool>>("Ktisis.SetMatrix");
		_ktisisBatchGetMatrix = dpi.GetIpcSubscriber<uint, List<string>, bool, Task<Dictionary<string, Matrix4x4?>>>("Ktisis.BatchGetMatrix");
		_ktisisGetAllMatrices = dpi.GetIpcSubscriber<uint, bool, Task<Dictionary<string, Matrix4x4?>>>("Ktisis.GetAllMatrices");
		_ktisisBatchSetMatrix = dpi.GetIpcSubscriber<uint, Dictionary<string, Matrix4x4>, bool, Task<bool>>("Ktisis.BatchSetMatrix");
		_transformTable = new TransformTable(cfg, locale);
	}

	public override void Draw()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!_ctx.IsValid)
		{
			Close();
			return;
		}
		TabBarDisposable val = ImRaii.TabBar(ImU8String.op_Implicit("##ConfigTabs"));
		try
		{
			if (val.Success)
			{
				DrawTab("IPC Provider", DrawProviderTab);
				DrawTab("IPC Manager", DrawManagerTab);
				DrawTab("Diagnostics", DrawDiagnosticsTab);
			}
		}
		finally
		{
			((TabBarDisposable)(ref val)).Dispose();
		}
	}

	private static void DrawTab(string name, Action handler)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TabItemDisposable val = ImRaii.TabItem(ImU8String.op_Implicit(name));
		try
		{
			if (val.Success)
			{
				ImGui.Spacing();
				handler();
			}
		}
		finally
		{
			((TabItemDisposable)(ref val)).Dispose();
		}
	}

	private async void DrawProviderTab()
	{
		ImU8String val = ImU8String.op_Implicit("GameObject Index");
		ref int gameObjectId = ref _gameObjectId;
		ImU8String val2 = default(ImU8String);
		ImGui.InputInt(val, ref gameObjectId, 0, 0, val2, (ImGuiInputTextFlags)0);
		val2 = new ImU8String(21, 1);
		((ImU8String)(ref val2)).AppendLiteral("Clipboard Pose Data: ");
		((ImU8String)(ref val2)).AppendFormatted<bool>(_hasClip);
		ImGui.Text(val2);
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.ApiVersion"));
		if (ImGui.Button(ImU8String.op_Implicit("GET##ApiVersion"), default(Vector2)))
		{
			_apiVersion = _ktisisApiVersion.InvokeFunc();
		}
		ImGui.SameLine();
		DisabledDisposable val3 = ImRaii.Disabled(!_apiVersion.HasValue);
		try
		{
			ImU8String val4 = new ImU8String(9, 1);
			((ImU8String)(ref val4)).AppendLiteral("Version: ");
			((ImU8String)(ref val4)).AppendFormatted<(int, int)?>(_apiVersion);
			ImGui.Text(val4);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.RefreshActors"));
		if (ImGui.Button(ImU8String.op_Implicit("APPLY##RefreshActors"), default(Vector2)))
		{
			_ktisisRefreshActors.InvokeFunc();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.IsPosing"));
		if (ImGui.Button(ImU8String.op_Implicit("GET##IsPosing"), default(Vector2)))
		{
			_isPosing = _ktisisIsPosing.InvokeFunc();
		}
		ImGui.SameLine();
		val3 = ImRaii.Disabled(!_isPosing.HasValue);
		try
		{
			ImU8String val5 = new ImU8String(8, 1);
			((ImU8String)(ref val5)).AppendLiteral("Posing: ");
			((ImU8String)(ref val5)).AppendFormatted<bool?>(_isPosing);
			ImGui.Text(val5);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.PosingChanged"));
		ImU8String val6 = new ImU8String(16, 1);
		((ImU8String)(ref val6)).AppendLiteral("Last Published: ");
		((ImU8String)(ref val6)).AppendFormatted<string>(_lastPosingEventTime);
		ImGui.Text(val6);
		ImU8String val7 = new ImU8String(12, 1);
		((ImU8String)(ref val7)).AppendLiteral("Last Value: ");
		((ImU8String)(ref val7)).AppendFormatted<bool?>(_lastPosingEventValue);
		ImGui.Text(val7);
		ImGui.Text(ImU8String.op_Implicit("Ktisis.LoadPose"));
		DisabledDisposable val8 = ImRaii.Disabled(_gameObjectId < 1 || !_hasClip);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit("APPLY (Clipboard)##LoadPose"), default(Vector2)))
			{
				_hasClip = CheckClipboard();
				if (_hasClip)
				{
					if (await _ktisisLoadPose.InvokeFunc((uint)_gameObjectId, ImGui.GetClipboardText()))
					{
						Ktisis.Log.Debug($"[DEBUG] Loaded clipboard pose to actor {_gameObjectId}");
					}
					else
					{
						Ktisis.Log.Warning($"[DEBUG] Failed clipboard pose application to actor {_gameObjectId}");
					}
				}
				else
				{
					Ktisis.Log.Warning("[DEBUG] Clipboard has invalid pose data, cannot apply");
				}
			}
		}
		finally
		{
			((IDisposable)val8)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.SavePose"));
		val8 = ImRaii.Disabled(_gameObjectId < 1);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit("GET (Clipboard)##SavePose"), default(Vector2)))
			{
				string text = await _ktisisSavePose.InvokeFunc((uint)_gameObjectId);
				ImGui.SetClipboardText(ImU8String.op_Implicit(text));
				_hasClip = true;
				Ktisis.Log.Debug($"[DEBUG] Exported pose to clipboard from actor {_gameObjectId}: {text}");
			}
		}
		finally
		{
			((IDisposable)val8)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Ktisis.SelectedBones"));
		if (ImGui.Button(ImU8String.op_Implicit("GET##SelectedBones"), default(Vector2)))
		{
			foreach (var (value, values) in await _ktisisSelectedBones.InvokeFunc())
			{
				Ktisis.Log.Debug($"[DEBUG] Actor {value} selected bones: {string.Join(", ", values)}");
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Text(ImU8String.op_Implicit("Bone Transform Get/Set"));
		ImGui.Checkbox(ImU8String.op_Implicit("Use World Space"), ref _useWorldSpace);
		ImGui.SameLine();
		ImGui.TextDisabled(ImU8String.op_Implicit(_useWorldSpace ? "(World → Default)" : "(Parent-Relative)"));
		ImGui.InputText(ImU8String.op_Implicit("Bone Name"), ref _boneName, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		val8 = ImRaii.Disabled(_gameObjectId < 1 || string.IsNullOrEmpty(_boneName));
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit("GET##GetBoneTransform"), default(Vector2)))
			{
				Matrix4x4? matrix4x = await _ktisisGetMatrix.InvokeFunc((uint)_gameObjectId, _boneName, _useWorldSpace);
				if (matrix4x.HasValue)
				{
					_transform = new Transform(matrix4x.Value);
					Ktisis.Log.Debug($"[DEBUG] Got matrix for bone {_boneName} on actor {_gameObjectId}");
				}
				else
				{
					Ktisis.Log.Warning($"[DEBUG] Failed to get matrix for bone {_boneName} on actor {_gameObjectId}");
				}
			}
			ImGui.Text(ImU8String.op_Implicit("Transform Matrix:"));
			if (_transformTable.Draw(_transform, out Transform transOut))
			{
				_transform = transOut;
			}
			if (ImGui.Button(ImU8String.op_Implicit("SET##SetBoneTransform"), default(Vector2)))
			{
				if (await _ktisisSetMatrix.InvokeFunc((uint)_gameObjectId, _boneName, _transform.ComposeMatrix(), _useWorldSpace))
				{
					Ktisis.Log.Debug($"[DEBUG] Set matrix for bone {_boneName} on actor {_gameObjectId}");
				}
				else
				{
					Ktisis.Log.Warning($"[DEBUG] Failed to set matrix for bone {_boneName} on actor {_gameObjectId}");
				}
			}
		}
		finally
		{
			((IDisposable)val8)?.Dispose();
		}
		ImGui.Spacing();
		ImGui.Text(ImU8String.op_Implicit("Batch Operations"));
		ImGui.InputText(ImU8String.op_Implicit("Batch Names"), ref _batchBoneNames, 256, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		val8 = ImRaii.Disabled(_gameObjectId < 1 || string.IsNullOrEmpty(_batchBoneNames));
		try
		{
			Matrix4x4? value2;
			if (ImGui.Button(ImU8String.op_Implicit("Batch GET"), default(Vector2)))
			{
				List<string> list = _batchBoneNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
				Dictionary<string, Matrix4x4?> dictionary = await _ktisisBatchGetMatrix.InvokeFunc((uint)_gameObjectId, list, _useWorldSpace);
				if (dictionary != null)
				{
					foreach (KeyValuePair<string, Matrix4x4?> item in dictionary)
					{
						LoggingService log = Ktisis.Log;
						string key = item.Key;
						value2 = item.Value;
						object obj;
						if (!value2.HasValue)
						{
							obj = "null";
						}
						else
						{
							value2 = item.Value;
							obj = value2.Value.ToString();
						}
						log.Debug("[DEBUG] Batch Get " + key + ": " + (string?)obj);
					}
				}
			}
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit("Batch SET (Current Transform)"), default(Vector2)))
			{
				List<string> list2 = _batchBoneNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
				Dictionary<string, Matrix4x4> dictionary2 = new Dictionary<string, Matrix4x4>();
				Matrix4x4 value3 = _transform.ComposeMatrix();
				foreach (string item2 in list2)
				{
					dictionary2[item2] = value3;
				}
				bool value4 = await _ktisisBatchSetMatrix.InvokeFunc((uint)_gameObjectId, dictionary2, _useWorldSpace);
				Ktisis.Log.Debug($"[DEBUG] Batch Set Result: {value4}");
			}
			ImGui.Spacing();
			if (!ImGui.Button(ImU8String.op_Implicit("Get All Matrices"), default(Vector2)))
			{
				return;
			}
			Dictionary<string, Matrix4x4?> dictionary3 = await _ktisisGetAllMatrices.InvokeFunc((uint)_gameObjectId, _useWorldSpace);
			if (dictionary3 != null)
			{
				Ktisis.Log.Debug($"[DEBUG] GetAllMatrices returned {dictionary3.Count} entries.");
				foreach (KeyValuePair<string, Matrix4x4?> item3 in dictionary3)
				{
					item3.Deconstruct(out var key2, out value2);
					string text2 = key2;
					Matrix4x4? matrix4x2 = value2;
					string text3 = (matrix4x2.HasValue ? matrix4x2.Value.ToString() : "null");
					Ktisis.Log.Debug("[DEBUG] " + text2 + ": " + text3);
				}
			}
			else
			{
				Ktisis.Log.Warning("[DEBUG] GetAllMatrices returned null.");
			}
		}
		finally
		{
			((IDisposable)val8)?.Dispose();
		}
	}

	private void DrawManagerTab()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Text(ImU8String.op_Implicit("TODO"));
	}

	private void DrawDiagnosticsTab()
	{
		_gui.Get<OverlayWindow>().DrawDebug(null);
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		DrawTransform();
	}

	private void DrawTransform()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		ITransformTarget target = _ctx.Transform.Target;
		if (target?.GetTransform() != null)
		{
			Transform transform = target.GetTransform();
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(8, 1);
			((ImU8String)(ref val)).AppendLiteral("Target: ");
			((ImU8String)(ref val)).AppendFormatted<string>(target.Primary?.Name);
			ImGui.Text(val);
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(24, 3);
			((ImU8String)(ref val2)).AppendLiteral("Position:\n\tX: ");
			((ImU8String)(ref val2)).AppendFormatted<float>(transform.Position.X);
			((ImU8String)(ref val2)).AppendLiteral("\n\tY: ");
			((ImU8String)(ref val2)).AppendFormatted<float>(transform.Position.Y);
			((ImU8String)(ref val2)).AppendLiteral("\n\tZ: ");
			((ImU8String)(ref val2)).AppendFormatted<float>(transform.Position.Z);
			ImGui.Text(val2);
			ImU8String val3 = default(ImU8String);
			((ImU8String)(ref val3))._002Ector(29, 4);
			((ImU8String)(ref val3)).AppendLiteral("Rotation:\n\tX: ");
			((ImU8String)(ref val3)).AppendFormatted<float>(transform.Rotation.X);
			((ImU8String)(ref val3)).AppendLiteral("\n\tY: ");
			((ImU8String)(ref val3)).AppendFormatted<float>(transform.Rotation.Y);
			((ImU8String)(ref val3)).AppendLiteral("\n\tZ: ");
			((ImU8String)(ref val3)).AppendFormatted<float>(transform.Rotation.Z);
			((ImU8String)(ref val3)).AppendLiteral("\n\tW: ");
			((ImU8String)(ref val3)).AppendFormatted<float>(transform.Rotation.W);
			ImGui.Text(val3);
			ImU8String val4 = default(ImU8String);
			((ImU8String)(ref val4))._002Ector(21, 3);
			((ImU8String)(ref val4)).AppendLiteral("Scale:\n\tX: ");
			((ImU8String)(ref val4)).AppendFormatted<float>(transform.Scale.X);
			((ImU8String)(ref val4)).AppendLiteral("\n\tY: ");
			((ImU8String)(ref val4)).AppendFormatted<float>(transform.Scale.Y);
			((ImU8String)(ref val4)).AppendLiteral("\n\tZ: ");
			((ImU8String)(ref val4)).AppendFormatted<float>(transform.Scale.Z);
			ImGui.Text(val4);
			if (_ctx.Selection.GetFirstSelected() is BoneNode boneNode)
			{
				Matrix4x4.Decompose(boneNode.GetMatrixModel() ?? Matrix4x4.Identity, out var scale, out var rotation, out var translation);
				Transform transform2 = boneNode.GetTransformModel() ?? new Transform();
				ImGui.Spacing();
				ImU8String val5 = default(ImU8String);
				((ImU8String)(ref val5))._002Ector(40, 0);
				((ImU8String)(ref val5)).AppendLiteral("Havok (Matrix Decompose / Raw Transform)");
				ImGui.Text(val5);
				ImU8String val6 = default(ImU8String);
				((ImU8String)(ref val6))._002Ector(33, 6);
				((ImU8String)(ref val6)).AppendLiteral("Position:\n\tX: ");
				((ImU8String)(ref val6)).AppendFormatted<float>(translation.X);
				((ImU8String)(ref val6)).AppendLiteral(" / ");
				((ImU8String)(ref val6)).AppendFormatted<float>(transform2.Position.X);
				((ImU8String)(ref val6)).AppendLiteral("\n\tY: ");
				((ImU8String)(ref val6)).AppendFormatted<float>(translation.Y);
				((ImU8String)(ref val6)).AppendLiteral(" / ");
				((ImU8String)(ref val6)).AppendFormatted<float>(transform2.Position.Y);
				((ImU8String)(ref val6)).AppendLiteral("\n\tZ: ");
				((ImU8String)(ref val6)).AppendFormatted<float>(translation.Z);
				((ImU8String)(ref val6)).AppendLiteral(" / ");
				((ImU8String)(ref val6)).AppendFormatted<float>(transform2.Position.Z);
				ImGui.Text(val6);
				ImU8String val7 = default(ImU8String);
				((ImU8String)(ref val7))._002Ector(41, 8);
				((ImU8String)(ref val7)).AppendLiteral("Rotation:\n\tX: ");
				((ImU8String)(ref val7)).AppendFormatted<float>(rotation.X);
				((ImU8String)(ref val7)).AppendLiteral(" / ");
				((ImU8String)(ref val7)).AppendFormatted<float>(transform2.Rotation.X);
				((ImU8String)(ref val7)).AppendLiteral("\n\tY: ");
				((ImU8String)(ref val7)).AppendFormatted<float>(rotation.Y);
				((ImU8String)(ref val7)).AppendLiteral(" / ");
				((ImU8String)(ref val7)).AppendFormatted<float>(transform2.Rotation.Y);
				((ImU8String)(ref val7)).AppendLiteral("\n\tZ: ");
				((ImU8String)(ref val7)).AppendFormatted<float>(rotation.Z);
				((ImU8String)(ref val7)).AppendLiteral(" / ");
				((ImU8String)(ref val7)).AppendFormatted<float>(transform2.Rotation.Z);
				((ImU8String)(ref val7)).AppendLiteral("\n\tW: ");
				((ImU8String)(ref val7)).AppendFormatted<float>(rotation.W);
				((ImU8String)(ref val7)).AppendLiteral(" / ");
				((ImU8String)(ref val7)).AppendFormatted<float>(transform2.Rotation.W);
				ImGui.Text(val7);
				ImU8String val8 = default(ImU8String);
				((ImU8String)(ref val8))._002Ector(30, 6);
				((ImU8String)(ref val8)).AppendLiteral("Scale:\n\tX: ");
				((ImU8String)(ref val8)).AppendFormatted<float>(scale.X);
				((ImU8String)(ref val8)).AppendLiteral(" / ");
				((ImU8String)(ref val8)).AppendFormatted<float>(transform2.Scale.X);
				((ImU8String)(ref val8)).AppendLiteral("\n\tY: ");
				((ImU8String)(ref val8)).AppendFormatted<float>(scale.Y);
				((ImU8String)(ref val8)).AppendLiteral(" / ");
				((ImU8String)(ref val8)).AppendFormatted<float>(transform2.Scale.Y);
				((ImU8String)(ref val8)).AppendLiteral("\n\tZ: ");
				((ImU8String)(ref val8)).AppendFormatted<float>(scale.Z);
				((ImU8String)(ref val8)).AppendLiteral(" / ");
				((ImU8String)(ref val8)).AppendFormatted<float>(transform2.Scale.Z);
				ImGui.Text(val8);
			}
		}
	}

	private bool CheckClipboard()
	{
		string clipboardText = ImGui.GetClipboardText();
		if (clipboardText != null)
		{
			try
			{
				if (JsonConvert.DeserializeObject<PoseFile>(clipboardText) != null)
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
		return false;
	}

	private void SetLastPosingChanged(bool status)
	{
		_lastPosingEventValue = status;
		_lastPosingEventTime = DateTime.Now.ToString("hh-mm-ss");
	}
}

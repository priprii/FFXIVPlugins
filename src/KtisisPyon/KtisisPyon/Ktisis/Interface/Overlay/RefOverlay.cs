using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Scene.Entities.Utility;

namespace Ktisis.Interface.Overlay;

[Transient]
public class RefOverlay
{
	private struct CallbackData
	{
		public float Ratio = 1f;

		public float Height = 0f;

		public CallbackData()
		{
		}
	}

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static ImGuiSizeCallback _003C0_003E__SetSizeCallback;
	}

	private readonly ConfigManager _cfg;

	private readonly ITextureProvider _tex;

	private static CallbackData _data = new CallbackData();

	public RefOverlay(ConfigManager cfg, ITextureProvider tex)
	{
		_cfg = cfg;
		_tex = tex;
	}

	public void DrawInstance(ReferenceImage image)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		bool visible = image.Visible;
		IDalamudTextureWrap val = default(IDalamudTextureWrap);
		Exception ex = default(Exception);
		if (!visible || !_tex.GetFromFile(image.Data.FilePath).TryGetWrap(ref val, ref ex))
		{
			return;
		}
		bool drawReferenceTitle = _cfg.File.Overlay.DrawReferenceTitle;
		ImGui.SetNextWindowSize(val.Size, (ImGuiCond)4);
		HandleImageAspectRatio(val.Size, drawReferenceTitle);
		StyleDisposable val2 = ImRaii.PushStyle((ImGuiStyleVar)1, Vector2.Zero, true);
		try
		{
			string text = image.Name + "###" + image.Data.Id;
			ImGuiWindowFlags val3 = (ImGuiWindowFlags)128;
			if (!drawReferenceTitle)
			{
				val3 = (ImGuiWindowFlags)(val3 | 1);
			}
			try
			{
				if (!ImGui.Begin(ImU8String.op_Implicit(text), ref visible, val3))
				{
					return;
				}
				Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
				Vector4 one = Vector4.One;
				one.W = image.Data.Opacity;
				Vector4 vector = one;
				ImGui.Image(val.Handle, contentRegionAvail, Vector2.Zero, Vector2.One, vector);
				HandlePopup(text, contentRegionAvail, image);
			}
			finally
			{
				ImGui.End();
			}
			if (!visible)
			{
				image.Visible = false;
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void HandlePopup(string id, Vector2 avail, ReferenceImage image)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		string text = id + "##popup";
		if (ImGui.IsItemClicked((ImGuiMouseButton)1) || (ImGui.IsItemClicked((ImGuiMouseButton)0) && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0)))
		{
			ImGui.OpenPopup(ImU8String.op_Implicit(text), (ImGuiPopupFlags)0);
			ImGui.SetNextWindowPos(ImGui.GetCursorScreenPos());
		}
		PopupDisposable val = ImRaii.Popup(ImU8String.op_Implicit(text));
		try
		{
			if (val.Success)
			{
				ImGui.SetNextItemWidth(avail.X);
				ImGui.SliderFloat(ImU8String.op_Implicit("##ref_opacity"), ref image.Data.Opacity, 0f, 1f, default(ImU8String), (ImGuiSliderFlags)0);
			}
		}
		finally
		{
			((PopupDisposable)(ref val)).Dispose();
		}
	}

	private unsafe static void HandleImageAspectRatio(Vector2 size, bool title)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		if (size.X == 0f || size.Y == 0f)
		{
			return;
		}
		float num = size.X / size.Y;
		ImGuiIOPtr iO = ImGui.GetIO();
		Vector2 vector = ((ImGuiIOPtr)(ref iO)).DisplaySize * 0.9f;
		Vector2 vector2 = new Vector2(vector.Y * num, vector.X / num);
		Vector2 vector3 = size * 0.1f;
		_data.Ratio = num;
		_data.Height = (title ? ImGui.GetFrameHeight() : 0f);
		fixed (CallbackData* data = &_data)
		{
			object obj = _003C_003EO._003C0_003E__SetSizeCallback;
			if (obj == null)
			{
				ImGuiSizeCallback val = SetSizeCallback;
				_003C_003EO._003C0_003E__SetSizeCallback = val;
				obj = (object)val;
			}
			ImGui.SetNextWindowSizeConstraints(vector3, vector2, (ImGuiSizeCallback)obj, (void*)data);
		}
	}

	private unsafe static void SetSizeCallback(ImGuiSizeCallbackData* data)
	{
		if (data != null)
		{
			CallbackData* userData = (CallbackData*)((ImGuiSizeCallbackData)data).UserData;
			if (userData != null)
			{
				((ImGuiSizeCallbackData)data).DesiredSize.Y = userData->Height + ((ImGuiSizeCallbackData)data).DesiredSize.X / userData->Ratio;
			}
		}
	}
}

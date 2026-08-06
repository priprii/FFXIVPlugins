using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Ktisis.Editor.Characters.Make;
using Ktisis.Editor.Characters.Types;

namespace Ktisis.Interface.Components.Chara.Popup;

public class FeatureSelectPopup
{
	private readonly ITextureProvider _tex;

	private MakeTypeFeature? Feature;

	private const int MaxColumns = 6;

	private const int MaxRows = 3;

	private static readonly Vector2 ButtonSize = new Vector2(64f, 64f);

	private bool _isOpening;

	private bool _isOpen;

	private string PopupId => $"##FeatureSelect_{GetHashCode():X}";

	public FeatureSelectPopup(ITextureProvider tex)
	{
		_tex = tex;
	}

	public void Open(MakeTypeFeature feature)
	{
		Feature = feature;
		_isOpening = true;
	}

	public void Draw(ICustomizeEditor editor)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (_isOpening)
		{
			_isOpening = false;
			ImGui.OpenPopup(ImU8String.op_Implicit(PopupId), (ImGuiPopupFlags)0);
		}
		if (!ImGui.IsPopupOpen(ImU8String.op_Implicit(PopupId), (ImGuiPopupFlags)0))
		{
			return;
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetNextWindowSizeConstraints(Vector2.Zero, new Vector2((ButtonSize.X + ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f + ((ImGuiStylePtr)(ref style)).ItemSpacing.X) * 6f + ((ImGuiStylePtr)(ref style)).ItemSpacing.X + ((ImGuiStylePtr)(ref style)).ScrollbarSize, (ButtonSize.Y + (((ImGuiStylePtr)(ref style)).FramePadding.X + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y) * 2f + UiBuilder.DefaultFontSizePx) * 3f + ((ImGuiStylePtr)(ref style)).WindowPadding.Y));
		PopupDisposable val = ImRaii.Popup(ImU8String.op_Implicit(PopupId), (ImGuiWindowFlags)64);
		try
		{
			if (!val.Success)
			{
				if (_isOpen)
				{
					OnClose();
				}
			}
			else
			{
				_isOpen = true;
				DrawParams(editor);
			}
		}
		finally
		{
			((PopupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawParams(ICustomizeEditor editor)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if (Feature == null)
		{
			return;
		}
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, 0u, true);
		try
		{
			bool flag = (int)Feature.Index == 24;
			byte customization = editor.GetCustomization(Feature.Index);
			int num = 0;
			MakeTypeParam[] array = Feature.Params;
			foreach (MakeTypeParam makeTypeParam in array)
			{
				if (num++ % 6 != 0 && num > 1)
				{
					ImGui.SameLine();
				}
				GroupDisposable val2 = ImRaii.Group();
				try
				{
					ImU8String val3 = new ImU8String(11, 2);
					((ImU8String)(ref val3)).AppendLiteral("##Feature_");
					((ImU8String)(ref val3)).AppendFormatted<byte>(makeTypeParam.Value);
					((ImU8String)(ref val3)).AppendLiteral("_");
					((ImU8String)(ref val3)).AppendFormatted<int>(num);
					IdDisposable val4 = ImRaii.PushId(val3, true);
					try
					{
						ISharedImmediateTexture val5 = null;
						if (makeTypeParam.Graphic != 0)
						{
							ITextureProvider tex = _tex;
							GameIconLookup val6 = GameIconLookup.op_Implicit(makeTypeParam.Graphic);
							tex.TryGetFromGameIcon(ref val6, ref val5);
						}
						Vector2 buttonSize = ButtonSize;
						ImGuiStylePtr style = ImGui.GetStyle();
						Vector2 vector = buttonSize + ((ImGuiStylePtr)(ref style)).FramePadding * 2f;
						bool flag2;
						if (val5 != null)
						{
							flag2 = ImGui.ImageButton(val5.GetWrapOrEmpty().Handle, ButtonSize);
							string text = makeTypeParam.Value.ToString();
							ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (vector.X - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X) / 2f);
							ImGui.Text(ImU8String.op_Implicit(text));
						}
						else
						{
							ImU8String val7 = new ImU8String(0, 1);
							((ImU8String)(ref val7)).AppendFormatted<byte>(makeTypeParam.Value);
							flag2 = ImGui.Button(val7, vector);
						}
						if (flag2)
						{
							editor.SetCustomization(Feature.Index, flag ? ((byte)(makeTypeParam.Value | (customization & 0x80))) : makeTypeParam.Value);
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((GroupDisposable)(ref val2)).Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OnClose()
	{
		_isOpen = false;
		Feature = null;
	}
}

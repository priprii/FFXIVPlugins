using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using GLib.Popups.Context;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;

namespace Ktisis.Interface.Windows;

public class TrayIcon : KtisisWindow
{
	private readonly ITextureProvider _tex;

	private readonly IEditorContext _ctx;

	private readonly GuiManager _gui;

	private readonly ISharedImmediateTexture SimpleIcon;

	private readonly ISharedImmediateTexture ColoredIcon;

	private bool _holding;

	private Vector2? _offset;

	public readonly StyleDisposable WindowStyle = new StyleDisposable();

	public readonly ColorDisposable WindowColor = new ColorDisposable();

	public TrayIcon(ITextureProvider tex, IEditorContext ctx, GuiManager gui, ImGuiWindowFlags flags = (ImGuiWindowFlags)2097323)
		: base("##TrayIcon", flags)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).Size = new Vector2(64f, 64f);
		_tex = tex;
		_ctx = ctx;
		_gui = gui;
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string name = executingAssembly.GetName().Name;
		SimpleIcon = _tex.GetFromManifestResource(executingAssembly, name + ".Data.Images.icon_simple.png");
		ColoredIcon = _tex.GetFromManifestResource(executingAssembly, name + ".Data.Images.icon_colored.png");
	}

	public override void PreDraw()
	{
		((Window)this).PreDraw();
		WindowStyle.Push((ImGuiStyleVar)1, Vector2.Zero);
		WindowStyle.Push((ImGuiStyleVar)3, 0f);
		WindowStyle.Push((ImGuiStyleVar)10, Vector2.Zero);
		WindowStyle.Push((ImGuiStyleVar)12, 0f);
		WindowColor.Push((ImGuiCol)21, 0u, true);
		WindowColor.Push((ImGuiCol)22, 0u, true);
		WindowColor.Push((ImGuiCol)23, 0u, true);
		IEditorContext ctx = _ctx;
		if (ctx == null || !ctx.IsGPosing || !ctx.IsValid)
		{
			Close();
		}
	}

	public override void PostDraw()
	{
		WindowColor.Dispose();
		WindowStyle.Dispose();
		((Window)this).PostDraw();
	}

	public override void OnClose()
	{
		WindowColor.Dispose();
		WindowStyle.Dispose();
		base.OnClose();
	}

	public unsafe override void Draw()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool num = ImGui.IsWindowHovered();
		if (num && !_holding)
		{
			ImGui.ImageButton(ColoredIcon.GetWrapOrEmpty().Handle, Vector2.Create(64f));
		}
		else
		{
			ImGui.ImageButton(SimpleIcon.GetWrapOrEmpty().Handle, Vector2.Create(64f));
		}
		ImGuiIOPtr iO = ImGui.GetIO();
		if (num)
		{
			if (((ImGuiIOPtr)(ref iO)).MouseReleased[0] && ((ImGuiIOPtr)(ref iO)).MouseDownDurationPrev[0] <= 0.25f)
			{
				_ctx.Interface.ToggleWorkspaceWindow();
				Close();
			}
			else if (((ImGuiIOPtr)(ref iO)).MouseDown[0] && ((ImGuiIOPtr)(ref iO)).MouseDownDuration[0] > 0.25f)
			{
				Vector2 valueOrDefault = _offset.GetValueOrDefault();
				if (!_offset.HasValue)
				{
					valueOrDefault = ImGui.GetMousePos() - ImGui.GetWindowPos();
					_offset = valueOrDefault;
				}
				_holding = true;
			}
			else if (ImGui.IsMouseClicked((ImGuiMouseButton)1))
			{
				flag = true;
			}
		}
		if (flag)
		{
			ContextMenu contextMenu = new ContextMenuBuilder().Action("Dismiss", base.Close).Action("Toggle Overlay", delegate
			{
				OverlayConfig overlay = _ctx.Config.Overlay;
				overlay.Visible = !overlay.Visible;
			}).Action("Offset Camera to Target Model", delegate
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				EditorCamera current = _ctx.Cameras.Current;
				if (current != null)
				{
					IGameObject val = _ctx.Cameras.ResolveOrbitTarget(current);
					if (val != null)
					{
						GameObject* address = (GameObject*)val.Address;
						DrawObject* drawObject = ((GameObject)address).DrawObject;
						if (drawObject != null)
						{
							current.RelativeOffset = Vector3.op_Implicit(((Object)(&((DrawObject)drawObject).Object)).Position - ((GameObject)address).Position);
						}
					}
				}
			})
				.Build($"TrayContextMenu_{((object)this).GetHashCode():X}");
			_gui.AddPopup(contextMenu.Open());
		}
		else if (_holding && _offset.HasValue)
		{
			if (!((ImGuiIOPtr)(ref iO)).MouseReleased[0])
			{
				ImGui.SetWindowPos(ImGui.GetMousePos() - _offset.Value, (ImGuiCond)0);
			}
			else
			{
				_offset = null;
				_holding = false;
			}
		}
		WindowColor.Dispose();
		WindowStyle.Dispose();
	}
}

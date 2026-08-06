using System.Numerics;
using Dalamud.Bindings.ImGui;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;

namespace Ktisis.Interface.Windows.ToolbarModules;

public class ChangeStatePopup : KtisisPopup
{
	private IEditorContext _ctx;

	private bool _state;

	public ChangeStatePopup(IEditorContext ctx, ImGuiWindowFlags flags = (ImGuiWindowFlags)134217732)
		: base("##ToolbarConfirmPopup", flags)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_ctx = ctx;
		_state = _ctx.Config.Editor.UseToolbar;
		_ctx.Config.Editor.UseToolbar = !_state;
	}

	protected override void OnDraw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		ImGuiP.SetWindowPos(ImGuiP.GetCurrentWindow(), ImGui.GetCenter(ImGui.GetWindowViewport()) - ImGui.GetWindowSize() / 2f);
		string text = Ktisis.Locale.Translate("toolbar.popup.close") + " " + (_state ? Ktisis.Locale.Translate("toolbar.popup.close_workspace") : Ktisis.Locale.Translate("toolbar.popup.close_toolbar"));
		string text2 = $"{Ktisis.Locale.Translate("toolbar.popup.state")} {(_state ? Ktisis.Locale.Translate("toolbar.popup.state_enable") : Ktisis.Locale.Translate("toolbar.popup.state_disable"))} {Ktisis.Locale.Translate("toolbar.popup.state_end")}";
		string text3 = Ktisis.Locale.Translate("toolbar.popup.yes");
		string text4 = Ktisis.Locale.Translate("toolbar.popup.no");
		float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		ImGui.SetCursorPosX((x - ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f).X) / 2f);
		ImGui.TextUnformatted(ImU8String.op_Implicit(text2));
		ImGui.TextUnformatted(ImU8String.op_Implicit(text));
		float num = ImGui.CalcTextSize(ImU8String.op_Implicit(text3), false, -1f).X + ImGui.CalcTextSize(ImU8String.op_Implicit(text4), false, -1f).X;
		ImGuiStylePtr style = ImGui.GetStyle();
		float num2 = num + ((ImGuiStylePtr)(ref style)).FramePadding.X * 4f;
		ImGui.SetCursorPosX((x - num2) / 2f);
		if (ImGui.Button(ImU8String.op_Implicit(text3), default(Vector2)))
		{
			_ctx.Plugin.Gui.ResetWorkspace();
			_ctx.Config.Editor.UseToolbar = _state;
			_ctx.Interface.Prepare();
			Close();
		}
		ImGui.SameLine();
		if (ImGui.Button(ImU8String.op_Implicit(text4), default(Vector2)))
		{
			Close();
		}
	}
}

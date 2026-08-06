using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Ktisis.Editor.Context.Types;

namespace Ktisis.Interface.Windows.ToolbarModules;

public class Workspace : WorkspaceWindow
{
	private IEditorContext _editorContext;

	public Workspace(IEditorContext ctx)
		: base(ctx)
	{
		_editorContext = ctx;
	}

	public override void PreDraw()
	{
	}

	public override void Draw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		_cameras.Draw();
		_workspace.DrawCompact();
		float num = (UiBuilder.DefaultFontSizePx + (((ImGuiStylePtr)(ref style)).ItemSpacing.Y + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f) * ImGuiHelpers.GlobalScale;
		float height = (ImGui.GetTextLineHeightWithSpacing() + 5f) * (float)(Math.Max(10, _editorContext.Scene.Children.Count()) + 5) - num;
		_sceneTree.Draw(height);
		ImGui.Spacing();
		DrawSceneTreeButtons();
	}
}

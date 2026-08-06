using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Types;

namespace Ktisis.Interface.Editor.Popup;

public class SceneMCDFModal(SceneFile.ActorInfo entity, IEditorContext context) : KtisisPopup("##PresetSave", (ImGuiWindowFlags)134217728)
{
	private SceneFile _sceneFile;

	protected override void OnDraw()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		TextWrapDisposable val = ImRaii.TextWrapPos(ImGui.GetWindowContentRegionMax().X);
		try
		{
			ImU8String val2 = new ImU8String(87, 1);
			((ImU8String)(ref val2)).AppendLiteral("The MCDF linked to the actor ");
			((ImU8String)(ref val2)).AppendFormatted<string>(entity.Chara.Nickname);
			((ImU8String)(ref val2)).AppendLiteral(" wasn't found, do you want select a file to load for them?");
			ImGui.TextUnformatted(val2);
			if (ImGui.Button(ImU8String.op_Implicit("Pick File"), default(Vector2)))
			{
				context.Interface.OpenMcdfFile(delegate(string s)
				{
					SceneFile.ActorInfo actorInfo2 = _sceneFile.Actors.Find((SceneFile.ActorInfo e) => e.Index == entity.Index);
					actorInfo2.MCDF = s;
				});
				Close();
			}
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit("Ignore"), default(Vector2)))
			{
				SceneFile.ActorInfo actorInfo = _sceneFile.Actors.Find((SceneFile.ActorInfo e) => e.Index == entity.Index);
				actorInfo.MCDF = string.Empty;
				Close();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SetScene(ref SceneFile sceneFile)
	{
		_sceneFile = sceneFile;
	}
}

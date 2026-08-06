using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Chara;
using Ktisis.Interface.Types;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Interface.Windows.Import;

public class CharaImportDialog : EntityEditWindow<ActorEntity>
{
	private readonly IEditorContext _ctx;

	private readonly CharaImportUI _import;

	public CharaImportDialog(IEditorContext ctx, CharaImportUI import)
		: base("chara_import.title", ctx, (ImGuiWindowFlags)64, "###CharaImportDialog")
	{
		_ctx = ctx;
		_import = import;
		_import.Context = ctx;
		CharaImportUI import2 = _import;
		import2.OnNpcSelected = (Action<CharaImportUI>)Delegate.Combine(import2.OnNpcSelected, new Action<CharaImportUI>(OnNpcSelected));
	}

	public void SetMethod(LoadMethod method)
	{
		_import.Method = method;
	}

	private void OnNpcSelected(CharaImportUI sender)
	{
		sender.ApplyTo(base.Target);
	}

	public override void Draw()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		UpdateTarget();
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(1, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(Ktisis.Locale.Translate("chara_import.header"));
		((ImU8String)(ref val)).AppendLiteral(" ");
		((ImU8String)(ref val)).AppendFormatted<string>(base.Target.Name);
		ImGui.Text(val);
		ImGui.Spacing();
		DrawEmbed();
	}

	public void DrawEmbed()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((Window)this).PreDraw();
		if (!Context.IsValid)
		{
			return;
		}
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(11, 1);
		((ImU8String)(ref val)).AppendLiteral("CharaEmbed_");
		((ImU8String)(ref val)).AppendFormatted<int>(((object)this).GetHashCode(), "X");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			_import.DrawLoadMethods();
			ImGui.Spacing();
			_import.DrawImport();
			ImGui.Spacing();
			DrawCharaApplication();
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawCharaApplication()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_import.DrawModesSelect();
		ImGui.Spacing();
		ImGui.Spacing();
		DisabledDisposable val = ImRaii.Disabled(!_import.HasSelection);
		try
		{
			if (ImGui.Button(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_import.apply")), default(Vector2)))
			{
				_import.ApplyTo(base.Target);
			}
			ImGui.Spacing();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

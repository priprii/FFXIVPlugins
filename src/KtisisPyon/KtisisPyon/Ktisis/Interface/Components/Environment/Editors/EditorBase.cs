using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Scene.Modules;
using Ktisis.Structs.Env;

namespace Ktisis.Interface.Components.Environment.Editors;

public abstract class EditorBase
{
	public abstract string Name { get; }

	public abstract bool IsActivated(EnvOverride flags);

	public abstract void Draw(IEnvModule module, ref EnvState state);

	protected DisabledDisposable Disable(IEnvModule module)
	{
		return ImRaii.Disabled(!IsActivated(module.Override));
	}

	protected bool DrawToggleCheckbox(string label, EnvOverride flag, IEnvModule module)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		bool flag2 = module.Override.HasFlag(flag);
		bool num = ImGui.Checkbox(ImU8String.op_Implicit(label), ref flag2);
		if (num)
		{
			module.Override ^= flag;
		}
		ImGui.Spacing();
		return num;
	}
}

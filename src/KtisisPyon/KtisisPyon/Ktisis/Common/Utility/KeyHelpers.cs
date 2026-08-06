using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;

namespace Ktisis.Common.Utility;

public static class KeyHelpers
{
	public static bool IsModifierKey(VirtualKey key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		if (key - 16 <= 2)
		{
			return true;
		}
		return false;
	}

	public static IEnumerable<VirtualKey> GetKeysDown()
	{
		ImGuiIOPtr io = ImGui.GetIO();
		if (((ImGuiIOPtr)(ref io)).KeyCtrl)
		{
			yield return (VirtualKey)17;
		}
		if (((ImGuiIOPtr)(ref io)).KeyShift)
		{
			yield return (VirtualKey)16;
		}
		if (((ImGuiIOPtr)(ref io)).KeyAlt)
		{
			yield return (VirtualKey)18;
		}
		for (int i = 0; i < ((ImGuiIOPtr)(ref io)).KeysDown.Length; i++)
		{
			if (((ImGuiIOPtr)(ref io)).KeysDown[i])
			{
				VirtualKey val = ImGuiHelpers.ImGuiKeyToVirtualKey((ImGuiKey)i);
				if (((int)val < 160 || (int)val > 165) && (int)val != 0)
				{
					yield return val;
				}
			}
		}
	}
}

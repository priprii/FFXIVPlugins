using System.Numerics;
using Dalamud.Interface.Colors;

namespace TargetPyon;

internal readonly struct JobInfo
{
	internal readonly uint Id;

	internal readonly string Name;

	internal readonly Vector4 JobColour;

	internal JobInfo(uint cj)
	{
		Id = cj;
		Name = "";
		Name = cj switch
		{
			0u => "", 
			1u => "PLD", 
			2u => "MNK", 
			3u => "WAR", 
			4u => "DRG", 
			5u => "BRD", 
			6u => "WHM", 
			7u => "BLM", 
			8u => "CRP", 
			9u => "BSM", 
			10u => "ARM", 
			11u => "GSM", 
			12u => "LTW", 
			13u => "WVR", 
			14u => "ALC", 
			15u => "CUL", 
			16u => "MIN", 
			17u => "BTN", 
			18u => "FSH", 
			19u => "PLD", 
			20u => "MNK", 
			21u => "WAR", 
			22u => "DRG", 
			23u => "BRD", 
			24u => "WHM", 
			25u => "BLM", 
			26u => "SMN", 
			27u => "SMN", 
			28u => "SCH", 
			29u => "NIN", 
			30u => "NIN", 
			31u => "MCH", 
			32u => "DRK", 
			33u => "AST", 
			34u => "SAM", 
			35u => "RDM", 
			36u => "BLU", 
			37u => "GNB", 
			38u => "DNC", 
			39u => "RPR", 
			40u => "SGE", 
			41u => "VPR", 
			42u => "PCT", 
			_ => "", 
		};
		JobColour = cj switch
		{
			0u => ImGuiColors.DalamudGrey, 
			1u => ImGuiColors.TankBlue, 
			2u => ImGuiColors.DPSRed, 
			3u => ImGuiColors.TankBlue, 
			4u => ImGuiColors.DPSRed, 
			5u => ImGuiColors.DPSRed, 
			6u => ImGuiColors.HealerGreen, 
			7u => ImGuiColors.DPSRed, 
			8u => ImGuiColors.DalamudYellow, 
			9u => ImGuiColors.DalamudYellow, 
			10u => ImGuiColors.DalamudYellow, 
			11u => ImGuiColors.DalamudYellow, 
			12u => ImGuiColors.DalamudYellow, 
			13u => ImGuiColors.DalamudYellow, 
			14u => ImGuiColors.DalamudYellow, 
			15u => ImGuiColors.DalamudYellow, 
			16u => ImGuiColors.DalamudYellow, 
			17u => ImGuiColors.DalamudYellow, 
			18u => ImGuiColors.DalamudYellow, 
			19u => ImGuiColors.TankBlue, 
			20u => ImGuiColors.DPSRed, 
			21u => ImGuiColors.TankBlue, 
			22u => ImGuiColors.DPSRed, 
			23u => ImGuiColors.DPSRed, 
			24u => ImGuiColors.HealerGreen, 
			25u => ImGuiColors.DPSRed, 
			26u => ImGuiColors.DPSRed, 
			27u => ImGuiColors.DPSRed, 
			28u => ImGuiColors.HealerGreen, 
			29u => ImGuiColors.DPSRed, 
			30u => ImGuiColors.DPSRed, 
			31u => ImGuiColors.DPSRed, 
			32u => ImGuiColors.TankBlue, 
			33u => ImGuiColors.HealerGreen, 
			34u => ImGuiColors.DPSRed, 
			35u => ImGuiColors.DPSRed, 
			36u => ImGuiColors.DPSRed, 
			37u => ImGuiColors.TankBlue, 
			38u => ImGuiColors.DPSRed, 
			39u => ImGuiColors.DPSRed, 
			40u => ImGuiColors.HealerGreen, 
			41u => ImGuiColors.DPSRed, 
			42u => ImGuiColors.DPSRed, 
			_ => ImGuiColors.DalamudGrey, 
		};
	}
}

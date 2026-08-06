using System.Linq;
using System.Runtime.CompilerServices;

namespace Ktisis.Common.Utility;

public static class PoseUtil
{
	public static readonly string[] BunnyEarBones = new string[16]
	{
		"j_zera_a_l", "j_zera_b_l", "j_zera_a_r", "j_zera_b_r", "j_zerb_a_l", "j_zerb_b_l", "j_zerb_a_r", "j_zerb_b_r", "j_zerc_a_l", "j_zerc_b_l",
		"j_zerc_a_r", "j_zerc_b_r", "j_zerd_a_l", "j_zerd_b_l", "j_zerd_a_r", "j_zerd_b_r"
	};

	[CompilerGenerated]
	private static string[] _003CEarBones_003Ek__BackingField = new string[6] { "j_mimi_l", "j_mimi_r", "n_ear_a_l", "n_ear_a_r", "n_ear_b_l", "n_ear_b_r" };

	public static string[] EarBones => _003CEarBones_003Ek__BackingField.Concat(BunnyEarBones).ToArray();
}

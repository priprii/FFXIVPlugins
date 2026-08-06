using System;
using Ktisis.Data.Files;

public static class LegacyPoseHelpers
{
	public static string ConvertLegacyPose(string file)
	{
		string text = "{\n";
		text += "\t\"FileExtension\": \".pose\",\n";
		text += "\t\"TypeName\": \"Anamnesis Pose\",\n";
		text += "\t\"Position\": \"0, 0, 0\",\n";
		text += "\t\"Rotation\": \"0, 0, 0, 1\",\n";
		text += "\t\"Scale\": \"1, 1, 1\",\n";
		text += "\t\"Bones\": {\n";
		string[] array = file.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.TrimEntries);
		for (int i = 7; i < array.Length - 1; i++)
		{
			text += ConvertLegacyBone(array[i]);
		}
		text = text.Substring(0, text.Length - 2);
		return text + "}\n\t}";
	}

	private static string? ConvertLegacyBone(string bone)
	{
		string text = "";
		string text2 = bone.Split(new char[1] { ':' }, 2)[0].Replace("\"", "");
		string text3 = bone.Split(new char[1] { ':' }, 2)[1].Replace("\"", "").Replace(",", "").Replace(" ", "");
		if (!PoseFile.AnamLegacyConversions.ContainsKey(text2) || text3.Contains("null"))
		{
			return null;
		}
		float[] array = new float[4];
		for (int i = 0; i < 4; i++)
		{
			byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(text3.Substring(i * 8, 8), 16));
			bytes.Reverse();
			array[i] = BitConverter.ToSingle(bytes, 0);
		}
		text = text + "\t\t\"" + text2 + "\": {\n";
		text += "\t\t\t\"Position\": \"0, 0, 0\",\n";
		text = text + "\t\t\t\"Rotation\": \"" + array[0] + ", " + array[1] + ", " + array[2] + ", " + array[3] + "\",\n";
		text += "\t\t\t\"Scale\": \"1, 1, 1\"\n";
		return text + "\t\t},\n";
	}
}

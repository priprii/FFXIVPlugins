using System.Globalization;
using System.IO;
using System.Numerics;
using System.Xml;
using Ktisis.Data.Config.Pose2D;

namespace Ktisis.Data.Serialization;

public static class PoseViewReader
{
	private const string ViewTag = "View";

	private const string ImageTag = "Image";

	private const string BoneTag = "Bone";

	public static PoseViewSchema ReadStream(Stream stream)
	{
		PoseViewSchema poseViewSchema = new PoseViewSchema();
		using XmlReader xmlReader = XmlReader.Create(stream);
		while (xmlReader.Read())
		{
			if (xmlReader.NodeType == XmlNodeType.Element && !(xmlReader.Name != "View"))
			{
				PoseViewEntry poseViewEntry = ReadView(xmlReader, poseViewSchema);
				poseViewSchema.Views.Add(poseViewEntry.Name, poseViewEntry);
			}
		}
		return poseViewSchema;
	}

	private static PoseViewEntry ReadView(XmlReader reader, PoseViewSchema schema)
	{
		PoseViewEntry poseViewEntry = new PoseViewEntry
		{
			Name = (reader.GetAttribute("name") ?? "INVALID")
		};
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
				if (reader.Name == "Image")
				{
					string attribute = reader.GetAttribute("file");
					if (attribute != null)
					{
						poseViewEntry.Images.Add(attribute);
					}
				}
				else if (reader.Name == "Bone")
				{
					if (!float.TryParse(reader.GetAttribute("x"), CultureInfo.InvariantCulture, out var result))
					{
						result = 0f;
					}
					if (!float.TryParse(reader.GetAttribute("y"), CultureInfo.InvariantCulture, out var result2))
					{
						result2 = 0f;
					}
					PoseViewBone item = new PoseViewBone
					{
						Label = (reader.GetAttribute("label") ?? string.Empty),
						Name = (reader.GetAttribute("name") ?? string.Empty),
						Position = new Vector2(result, result2)
					};
					poseViewEntry.Bones.Add(item);
				}
				break;
			case XmlNodeType.EndElement:
				if (reader.Name == "View")
				{
					return poseViewEntry;
				}
				break;
			}
		}
		return poseViewEntry;
	}
}

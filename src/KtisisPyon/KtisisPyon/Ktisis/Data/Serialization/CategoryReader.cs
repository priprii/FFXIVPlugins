using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dalamud.Utility;
using Ktisis.Data.Config.Bones;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Data.Serialization;

public static class CategoryReader
{
	private const string BonesTag = "Bones";

	private const string CategoryTag = "Category";

	private const string TwoJointsIkTag = "TwoJointsIK";

	private const string CcdIkTag = "CcdIK";

	private const string PresetTag = "Preset";

	public static CategoryConfig ReadStream(Stream stream)
	{
		CategoryConfig categoryConfig = new CategoryConfig();
		using XmlReader xmlReader = XmlReader.Create(stream);
		while (xmlReader.Read())
		{
			if (xmlReader.NodeType == XmlNodeType.Element && !(xmlReader.Name != "Category"))
			{
				ReadCategory(xmlReader, categoryConfig);
			}
		}
		return categoryConfig;
	}

	private static BoneCategory ReadCategory(XmlReader reader, CategoryConfig categories)
	{
		BoneCategory boneCategory = new BoneCategory(reader.GetAttribute("Id") ?? "Unknown")
		{
			IsNsfw = (reader.GetAttribute("IsNsfw") == "true"),
			IsDefault = (reader.GetAttribute("IsDefault") == "true")
		};
		categories.AddCategory(boneCategory);
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
				if (reader.Name == "Category")
				{
					ReadCategory(reader, categories).ParentCategory = boneCategory.Name;
				}
				else if (reader.Name == "Bones")
				{
					ReadBone(reader, boneCategory);
				}
				else if (reader.Name == "TwoJointsIK")
				{
					boneCategory.TwoJointsGroup = ReadTwoJointsIkGroup(reader);
				}
				else if (reader.Name == "CcdIK")
				{
					boneCategory.CcdGroup = ReadCcdIkGroup(reader);
				}
				else if (reader.Name == "Preset")
				{
					boneCategory.Presets.Add(ReadPresetTag(reader));
				}
				break;
			case XmlNodeType.EndElement:
				if (reader.Name == "Category")
				{
					return boneCategory;
				}
				break;
			}
		}
		return boneCategory;
	}

	private static void ReadBone(XmlReader reader, BoneCategory category)
	{
		reader.Read();
		if (reader.NodeType == XmlNodeType.Text)
		{
			IEnumerable<CategoryBone> collection = from ln in reader.Value.Split((char[]?)null)
				select ln.Trim() into bone
				where !StringExtensions.IsNullOrEmpty(bone)
				select new CategoryBone(bone);
			category.Bones.AddRange(collection);
		}
	}

	private static TwoJointsGroupParams ReadTwoJointsIkGroup(XmlReader reader)
	{
		TwoJointsGroupParams twoJointsGroupParams = new TwoJointsGroupParams();
		TwoJointsGroupParams twoJointsGroupParams2 = twoJointsGroupParams;
		string attribute = reader.GetAttribute("Type");
		TwoJointsType type = ((attribute == "Arm") ? TwoJointsType.Arm : ((attribute == "Leg") ? TwoJointsType.Leg : TwoJointsType.None));
		twoJointsGroupParams2.Type = type;
		TwoJointsGroupParams twoJointsGroupParams3 = twoJointsGroupParams;
		while (reader.Read() && (reader == null || reader.NodeType != XmlNodeType.EndElement || !(reader.Name == "TwoJointsIK")))
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string name = reader.Name;
				reader.Read();
				if (reader.NodeType == XmlNodeType.Text)
				{
					(name switch
					{
						"FirstBone" => twoJointsGroupParams3.FirstBone, 
						"FirstTwist" => twoJointsGroupParams3.FirstTwist, 
						"SecondBone" => twoJointsGroupParams3.SecondBone, 
						"SecondTwist" => twoJointsGroupParams3.SecondTwist, 
						"EndBone" => twoJointsGroupParams3.EndBone, 
						_ => throw new Exception("Encountered invalid IK bone parameter: " + name), 
					}).Add(reader.Value);
				}
			}
		}
		return twoJointsGroupParams3;
	}

	private static CcdGroupParams ReadCcdIkGroup(XmlReader reader)
	{
		CcdGroupParams ccdGroupParams = new CcdGroupParams();
		while (reader.Read() && (reader == null || reader.NodeType != XmlNodeType.EndElement || !(reader.Name == "CcdIK")))
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			string name = reader.Name;
			reader.Read();
			if (reader.NodeType != XmlNodeType.Text)
			{
				continue;
			}
			List<string> list;
			if (!(name == "StartBone"))
			{
				if (!(name == "EndBone"))
				{
					throw new Exception("Encountered invalid IK bone parameter: " + name);
				}
				list = ccdGroupParams.EndBone;
			}
			else
			{
				list = ccdGroupParams.StartBone;
			}
			list.Add(reader.Value);
		}
		return ccdGroupParams;
	}

	private static string ReadPresetTag(XmlReader reader)
	{
		return reader.GetAttribute("Name") ?? throw new InvalidDataException("Invalid name given to preset, please raise to the developers.");
	}
}

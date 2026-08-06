using Ktisis.Common.Utility;
using Ktisis.Data.Config.Gobos;
using Ktisis.Data.Config.Pose2D;
using Ktisis.Data.Config.Props;
using Ktisis.Data.Config.Sections;

namespace Ktisis.Data.Serialization;

public static class SchemaReader
{
	private const string CategorySchemaPath = "Data.Schema.Categories.xml";

	private const string ViewSchemaPath = "Data.Schema.Views.xml";

	private const string GoboSchemaPath = "Data.Library.gobos.csv";

	private const string PropSchemaPath = "Data.Library.props.json";

	public static CategoryConfig ReadCategories()
	{
		return CategoryReader.ReadStream(ResourceUtil.GetManifestResource("Data.Schema.Categories.xml"));
	}

	public static PoseViewSchema ReadPoseView()
	{
		return PoseViewReader.ReadStream(ResourceUtil.GetManifestResource("Data.Schema.Views.xml"));
	}

	public static GoboSchema ReadGobos()
	{
		return GoboReader.ReadStream(ResourceUtil.GetManifestResource("Data.Library.gobos.csv"));
	}

	public static PropSchema ReadProps()
	{
		return PropsReader.ReadStream(ResourceUtil.GetManifestResource("Data.Library.props.json"));
	}
}

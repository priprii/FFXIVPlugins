namespace Ktisis.Data.Config.Sections;

public class PoseViewConfig
{
	public string? BodyPath;

	public string? ArmorPath;

	public string? EarsPath;

	public string? FacePath;

	public string? HandsPath;

	public string? LipsPath;

	public string? MouthPath;

	public string? TailPath;

	public string? CustomPathFor(string viewName)
	{
		switch (viewName)
		{
		case "Body":
			return BodyPath;
		case "Face":
			return FacePath;
		case "Lips":
			return LipsPath;
		case "Mouth":
			return MouthPath;
		case "Hands":
			return HandsPath;
		case "Tail":
			return TailPath;
		case "Armor":
			return ArmorPath;
		case "Ears":
			return EarsPath;
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(viewName);
			string result = default(string);
			return result;
		}
		}
	}
}

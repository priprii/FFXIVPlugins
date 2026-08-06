using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PvPyon;

[JsonConverter(typeof(StringEnumConverter))]
public enum NameplateFreeCompanyVisibility
{
	Default,
	Never
}

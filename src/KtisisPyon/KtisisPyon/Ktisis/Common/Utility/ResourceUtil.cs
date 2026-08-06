using System.IO;
using System.Reflection;

namespace Ktisis.Common.Utility;

public static class ResourceUtil
{
	public static Stream GetManifestResource(string path)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		path = executingAssembly.GetName().Name + "." + path;
		return executingAssembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException(path);
	}
}

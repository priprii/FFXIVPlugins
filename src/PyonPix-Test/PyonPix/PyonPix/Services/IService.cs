using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace PyonPix.Services;

public interface IService
{
	Task Initialize();

	void Update(IFramework framework);

	Task Dispose();
}

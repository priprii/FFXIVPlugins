using System.Threading.Tasks;

namespace PyonPix.Services;

public interface IService
{
	Task Initialize();

	void Update();

	Task Dispose();
}

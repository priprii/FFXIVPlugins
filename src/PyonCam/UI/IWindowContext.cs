using PyonCam.Config;
using PyonCam.Services;

namespace PyonCam.UI;

public interface IWindowContext
{
	TWindow Register<TWindow>(TWindow window) where TWindow : class;

	TWindow Get<TWindow>() where TWindow : class;

	bool TryGet<TWindow>(out TWindow? window) where TWindow : class;

	void Initialize(Configuration config, IServiceContext services, bool isLoggedIn);

	void Draw();

	void OnCommand(string command, string args);

	void Dispose();
}

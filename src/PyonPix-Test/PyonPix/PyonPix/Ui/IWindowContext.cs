using PyonPix.Config;
using PyonPix.Services;

namespace PyonPix.Ui;

public interface IWindowContext
{
	TWindow Register<TWindow>(TWindow window) where TWindow : class;

	TWindow Get<TWindow>() where TWindow : class;

	bool TryGet<TWindow>(out TWindow? window) where TWindow : class;

	void Initialize(Configuration config, IServiceContext services);

	void Draw();

	void OnCommand(string command, string args);

	void Dispose();
}

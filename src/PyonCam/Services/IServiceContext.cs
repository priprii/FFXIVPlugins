using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PyonCam.Config;

namespace PyonCam.Services;

public interface IServiceContext
{
	IClientState ClientState { get; }

	ICommandManager CommandManager { get; }

	IDataManager DataManager { get; }

	IFramework Framework { get; }

	IGameInteropProvider GameInteropProvider { get; }

	IPluginLog Log { get; }

	IObjectTable Objects { get; }

	IPlayerState PlayerState { get; }

	IDalamudPluginInterface PluginInterface { get; }

	ISigScanner SigScanner { get; }

	ICondition Condition { get; }

	IGameGui GameGui { get; }

	ITargetManager TargetManager { get; }

	void Register<TService>(TService service) where TService : class;

	TService Get<TService>() where TService : class;

	bool TryGet<TService>(out TService? service) where TService : class;

	void Initialize(Configuration config);

	void Update(IFramework framework);

	void Dispose();
}

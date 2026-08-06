using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using PyonPix.Config;
using PyonPix.Extensions;
using PyonPix.Shared.Structs.Renderer;
using PyonPix.Ui;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace PyonPix.Services.Game;

public class DXService : BaseService
{
	public SwapChain? DXGISwapChain { get; private set; }

	public SharpDX.Direct3D11.Device? D3D11Device { get; private set; }

	public DeviceContext? D3D11Context { get; private set; }

	public nint SwapChainPtr { get; private set; }

	public LUID Luid { get; private set; }

	public DXService(PyonPix.Config.Configuration config, IServiceContext services, IWindowContext windows)
		: base(config, services, windows)
	{
	}

	public unsafe override Task Initialize()
	{
		Device* ptr = Device.Instance();
		SwapChainPtr = (nint)((SwapChain)((Device)ptr).SwapChain).DXGISwapChain;
		DXGISwapChain = CppObject.FromPointer<SwapChain>(SwapChainPtr);
		D3D11Device = DXGISwapChain.GetDevice<SharpDX.Direct3D11.Device>();
		D3D11Context = D3D11Device.ImmediateContext;
		SharpDX.DXGI.Device device = D3D11Device.QueryInterface<SharpDX.DXGI.Device>();
		Luid = device.Adapter.Description.Luid.ToLUID();
		return Task.CompletedTask;
	}

	public async Task<T> LoadShader<T>(string resourceName) where T : class
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PyonPix.Shaders." + resourceName + ".cso");
		byte[] bytes = new byte[stream.Length];
		await stream.ReadExactlyAsync(bytes);
		using ShaderBytecode shaderBytecode = new ShaderBytecode(bytes);
		return (typeof(T) == typeof(VertexShader)) ? (new VertexShader(D3D11Device, shaderBytecode) as T) : (new PixelShader(D3D11Device, shaderBytecode) as T);
	}
}

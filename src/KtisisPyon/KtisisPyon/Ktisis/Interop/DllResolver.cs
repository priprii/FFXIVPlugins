using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Dalamud.Plugin;
using Ktisis.Core.Attributes;

namespace Ktisis.Interop;

[Singleton]
public class DllResolver : IDisposable
{
	private readonly IDalamudPluginInterface _dpi;

	private readonly List<nint> Handles = new List<nint>();

	private AssemblyLoadContext? Context;

	public DllResolver(IDalamudPluginInterface dpi)
	{
		_dpi = dpi;
	}

	public void Create()
	{
		Ktisis.Log.Debug("Creating DLL resolver for unmanaged libraries");
		Context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
		if (Context != null)
		{
			Context.ResolvingUnmanagedDll += ResolveUnmanaged;
		}
	}

	private nint ResolveUnmanaged(Assembly assembly, string library)
	{
		string directoryName = Path.GetDirectoryName(_dpi.AssemblyLocation.FullName);
		if (directoryName == null)
		{
			Ktisis.Log.Warning("Failed to resolve location for native assembly!");
			return IntPtr.Zero;
		}
		string text = Path.Combine(directoryName, library);
		Ktisis.Log.Debug("Resolving native assembly path: " + text);
		if (NativeLibrary.TryLoad(text, out var handle) && handle != IntPtr.Zero)
		{
			Handles.Add(handle);
			Ktisis.Log.Debug($"Success, resolved library handle: {handle:X}");
		}
		else
		{
			Ktisis.Log.Warning("Failed to resolve native assembly path: " + text);
		}
		return handle;
	}

	public void Dispose()
	{
		Ktisis.Log.Debug("Disposing DLL resolver for unmanaged libraries");
		if (Context != null)
		{
			Context.ResolvingUnmanagedDll -= ResolveUnmanaged;
		}
		Context = null;
		Handles.ForEach(FreeHandle);
		Handles.Clear();
	}

	private void FreeHandle(nint handle)
	{
		Ktisis.Log.Debug($"Freeing library handle: {handle:X}");
		NativeLibrary.Free(handle);
	}
}

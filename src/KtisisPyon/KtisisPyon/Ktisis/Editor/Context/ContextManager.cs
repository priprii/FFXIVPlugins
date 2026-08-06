using System;
using System.Diagnostics;
using Ktisis.Core.Attributes;
using Ktisis.Core.Types;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Context.Types;
using Ktisis.Services.Game;
using KtisisPyon.Common.Utility;

namespace Ktisis.Editor.Context;

[Singleton]
public class ContextManager : IDisposable
{
	private readonly GPoseService _gpose;

	private readonly ContextBuilder _builder;

	private bool _isInit;

	private IPluginContext? _plugin;

	private IEditorContext? _context;

	public IEditorContext? Current
	{
		get
		{
			IEditorContext context = _context;
			if (context == null || !context.IsValid)
			{
				return null;
			}
			return context;
		}
	}

	public ContextManager(GPoseService gpose, ContextBuilder builder)
	{
		_gpose = gpose;
		_builder = builder;
	}

	public void Initialize(IPluginContext context)
	{
		if (_isInit)
		{
			throw new Exception("Attempted double initialization of ContextManager.");
		}
		_isInit = true;
		_plugin = context;
		_gpose.StateChanged += OnGPoseEvent;
		_gpose.Subscribe();
	}

	private void OnGPoseEvent(object sender, bool active)
	{
		if (!_isInit)
		{
			return;
		}
		if (_context != null && !active)
		{
			Win32.SetWinDefault(_context.Config.Pyon);
		}
		Destroy();
		if (active)
		{
			SetupEditor();
			if (_context != null)
			{
				PyonConfig pyon = _context.Config.Pyon;
				(pyon.DefaultPosition, pyon.DefaultSize, pyon.DefaultStyle, pyon.DefaultDeviceSize) = Win32.GetWinProperties();
			}
		}
	}

	public void SetupEditor()
	{
		if (!_isInit || _plugin == null)
		{
			throw new Exception("Attempted to setup uninitialized context.");
		}
		Ktisis.Log.Verbose("Creating new editor context...");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		try
		{
			_context = _builder.Create(_plugin);
			_context.Initialize();
			_gpose.Update += Update;
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"failed to initialize editor state:\n{value}");
			Destroy();
		}
		stopwatch.Stop();
		Ktisis.Log.Debug($"Editor context initialized in {stopwatch.Elapsed.TotalMilliseconds:00.00}ms");
	}

	private void Update()
	{
		if (!_isInit)
		{
			return;
		}
		IEditorContext context = _context;
		if (context != null)
		{
			if (context.IsValid)
			{
				context.Update();
			}
			else
			{
				Destroy();
			}
		}
	}

	private void Destroy()
	{
		try
		{
			_context?.Dispose();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to destroy editor state:\n{value}");
		}
		finally
		{
			_context = null;
		}
		_gpose.Update -= Update;
	}

	public void Dispose()
	{
		_isInit = false;
		Destroy();
		_gpose.StateChanged -= OnGPoseEvent;
	}
}

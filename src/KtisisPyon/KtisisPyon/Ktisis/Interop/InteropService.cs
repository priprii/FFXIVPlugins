using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Ktisis.Core;
using Ktisis.Core.Attributes;
using Ktisis.Interop.Hooking;

namespace Ktisis.Interop;

[Singleton]
public class InteropService : IDisposable
{
	private class HookMediator : IHookMediator
	{
		private readonly InteropService _interop;

		private HookModule? Module;

		public bool IsValid
		{
			get
			{
				if (!_interop.IsDisposing)
				{
					return Module?.IsInit ?? false;
				}
				return false;
			}
		}

		public HookMediator(InteropService interop)
		{
			_interop = interop;
		}

		public T Create<T>(params object[] param) where T : HookModule
		{
			return _interop.CreateModule<T>(param);
		}

		public bool Init(HookModule module)
		{
			return _interop.InitModule(module);
		}

		public bool Remove(HookModule module)
		{
			return _interop.RemoveModule(module);
		}
	}

	private readonly DIBuilder _di;

	private readonly IGameInteropProvider _interop;

	private readonly List<HookModule> Modules = new List<HookModule>();

	private bool IsDisposing;

	public InteropService(DIBuilder di, IGameInteropProvider interop)
	{
		_di = di;
		_interop = interop;
	}

	public T CreateModule<T>(params object[] param) where T : HookModule
	{
		HookMediator element = new HookMediator(this);
		return _di.Create<T>(param.Append(element).ToArray());
	}

	public HookScope CreateScope()
	{
		return new HookScope(new HookMediator(this));
	}

	private bool InitModule(HookModule module)
	{
		if (module.IsInit)
		{
			return true;
		}
		bool result;
		try
		{
			_interop.InitializeFromAttributes((object)module);
			result = true;
		}
		catch (Exception value)
		{
			result = false;
			Ktisis.Log.Error($"Failed to initialize module '{module.GetType().Name}'\n{value}");
		}
		return result;
	}

	private bool RemoveModule(HookModule module)
	{
		if (!IsDisposing)
		{
			return Modules.Remove(module);
		}
		return false;
	}

	public void Dispose()
	{
		IsDisposing = true;
		Modules.ForEach(delegate(HookModule mod)
		{
			mod.Dispose();
		});
		Modules.Clear();
		GC.SuppressFinalize(this);
	}
}

using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Events;

namespace Ktisis.Services.Game;

[Singleton]
public class GPoseService : IDisposable
{
	private readonly IClientState _clientState;

	private readonly IFramework _framework;

	private readonly ITargetManager _targets;

	private readonly Event<Action> _updateEvent;

	private readonly Event<Action<GPoseService, bool>> _gposeEvent;

	private bool _isActive;

	private bool _isSubscribed;

	public bool IsGPosing => _clientState.IsGPosing;

	public IGameObject? GPoseTarget => _targets.GPoseTarget;

	public event Action Update
	{
		add
		{
			_updateEvent.Add(value);
		}
		remove
		{
			_updateEvent.Remove(value);
		}
	}

	public event GPoseStateHandler StateChanged
	{
		add
		{
			_gposeEvent.Add(value.Invoke);
		}
		remove
		{
			_gposeEvent.Remove(value.Invoke);
		}
	}

	public GPoseService(IClientState clientState, IFramework framework, ITargetManager targets, Event<Action> updateEvent, Event<Action<GPoseService, bool>> gposeEvent)
	{
		_clientState = clientState;
		_framework = framework;
		_targets = targets;
		_updateEvent = updateEvent;
		_gposeEvent = gposeEvent;
	}

	public void Subscribe()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		if (!_isSubscribed)
		{
			_framework.Update += new OnUpdateDelegate(OnFrameworkUpdate);
			_isSubscribed = true;
		}
	}

	public void Reset()
	{
		_isActive = false;
	}

	private void OnFrameworkUpdate(IFramework sender)
	{
		bool isGPosing = IsGPosing;
		if (_isActive != isGPosing)
		{
			_isActive = isGPosing;
			Ktisis.Log.Info($"GPose state changed: {isGPosing}");
			_gposeEvent.Invoke(this, isGPosing);
		}
		if (isGPosing)
		{
			_updateEvent.Invoke();
		}
	}

	public void Dispose()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_framework.Update -= new OnUpdateDelegate(OnFrameworkUpdate);
		_isSubscribed = false;
	}
}

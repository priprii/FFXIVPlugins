using System;
using Hypostasis.Dalamud;

namespace Hypostasis;

public abstract class PluginModule
{
	private bool? isValid;

	public bool IsValid
	{
		get
		{
			try
			{
				if (!isValid.HasValue)
				{
					DalamudApi.SigScanner.Inject(this);
				}
				bool? flag = isValid;
				bool result;
				if (!flag.HasValue)
				{
					bool? flag2 = (isValid = Validate());
					result = flag2.Value;
				}
				else
				{
					result = flag == true;
				}
				return result;
			}
			catch (Exception exception)
			{
				DalamudApi.LogError($"Error validating {this}", exception);
				bool? flag = (isValid = false);
				return flag.Value;
			}
		}
		set
		{
			if (!value)
			{
				Invalidate();
			}
		}
	}

	public bool IsEnabled { get; set; }

	public virtual bool ShouldEnable => true;

	protected virtual bool Validate()
	{
		return true;
	}

	protected virtual void Enable()
	{
	}

	protected virtual void Disable()
	{
	}

	public virtual void Dispose()
	{
	}

	public void Toggle()
	{
		if (IsValid)
		{
			if (!IsEnabled)
			{
				Enable();
				IsEnabled = true;
			}
			else
			{
				Disable();
				IsEnabled = false;
			}
		}
	}

	private void Invalidate()
	{
		if (IsValid)
		{
			Disable();
			isValid = false;
		}
	}
}

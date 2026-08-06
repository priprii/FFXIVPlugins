using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PyonCam.Config;
using PyonCam.Config.Cam;
using PyonCam.Services;

namespace PyonCam;

public class FreeCam
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	public Vector3 Position;

	private unsafe GameCamera* gameCamera;

	private float speed = 0.5f;

	private bool onDeath;

	private bool onDeathActivated;

	private CameraConfigPreset? prevPreset;

	private float prevZoom;

	private float prevFoV;

	private readonly CameraConfigPreset FreeCamPreset = new CameraConfigPreset("FreeCam")
	{
		MinVRotation = -1.559f,
		MaxVRotation = 1.559f,
		MinZoom = 0.06f,
		MaxZoom = 0.06f,
		ZoomDelta = 0f
	};

	private PresetService PresetService => _services.Get<PresetService>();

	private CameraService CameraService => _services.Get<CameraService>();

	private InputService InputService => _services.Get<InputService>();

	public unsafe bool Enabled => gameCamera != null;

	public FreeCam(Configuration config, IServiceContext services)
	{
		_config = config;
		_services = services;
	}

	public unsafe void Toggle(bool death = false)
	{
		bool num = !Enabled;
		bool flag = !_services.Condition.Any();
		if (num)
		{
			gameCamera = (flag ? CameraService.MenuCamera : CameraService.Camera);
			speed = 0.5f;
			Position = new Vector3(gameCamera->viewX, gameCamera->viewY, gameCamera->viewZ);
			onDeath = death;
			prevPreset = PresetService.ActivePreset;
			prevZoom = gameCamera->currentZoom;
			prevFoV = gameCamera->currentFoV;
			FreeCamPreset.MinFoV = (FreeCamPreset.MaxFoV = gameCamera->currentFoV);
			CameraService.ApplyPreset(FreeCamPreset);
			gameCamera->mode = 1;
			CameraService.EnableNoClip();
			if (flag)
			{
				gameCamera->lockPosition = 0;
			}
		}
		else
		{
			if (!flag)
			{
				CameraService.ApplyPreset(PresetService.DefaultPreset);
				PresetService.CurrentPreset = prevPreset;
			}
			gameCamera->currentZoom = (gameCamera->interpolatedZoom = prevZoom);
			gameCamera->currentFoV = prevFoV;
			gameCamera = null;
			if (!_config.EnableCameraNoClippy)
			{
				CameraService.DisableNoClip();
			}
		}
		if (flag)
		{
			ToggleAddonVisible("_TitleRights");
			ToggleAddonVisible("_TitleRevision");
			ToggleAddonVisible("_TitleMenu");
			ToggleAddonVisible("_TitleLogo");
		}
		void ToggleAddonVisible(string name)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			AtkUnitBasePtr addonByName = _services.GameGui.GetAddonByName(name, 1);
			if (!(addonByName == AtkUnitBasePtr.op_Implicit((IntPtr)IntPtr.Zero)))
			{
				IntPtr address = addonByName.Address;
				((AtkUnitBase)(nint)address).IsVisible = !((AtkUnitBase)(nint)address).IsVisible;
			}
		}
	}

	public void CheckDeath()
	{
		bool flag = _services.Condition[(ConditionFlag)2];
		if (onDeathActivated)
		{
			onDeathActivated = flag;
			if (!onDeathActivated && onDeath && Enabled)
			{
				Toggle(death: true);
			}
		}
		else if (flag)
		{
			if (!Enabled)
			{
				Toggle(death: true);
			}
			onDeathActivated = true;
		}
	}

	public void Update()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_config.DeathCamMode == DeathCamSetting.FreeCam)
		{
			CheckDeath();
		}
		if (Enabled && !_services.ClientState.IsLoggedIn && _services.GameGui.GetAddonByName("Title", 1) == AtkUnitBasePtr.op_Implicit((IntPtr)IntPtr.Zero))
		{
			Toggle();
		}
	}

	public void UpdateControl()
	{
		if (InputService.IsKeyDown((VirtualKey)27))
		{
			if (Enabled)
			{
				Toggle();
			}
		}
		else if (Enabled)
		{
			UpdateInput();
		}
	}

	private unsafe void UpdateInput()
	{
		if (InputService.KeyData == null)
		{
			return;
		}
		Vector3 zero = Vector3.Zero;
		bool flag = InputService.IsMouseButtonHeld(MouseButton.Right);
		bool flag2 = InputService.IsMouseButtonHeld(MouseButton.Left, flag) && flag;
		if (InputService.IsKeyDown(KeybindInput.Forward) || flag2)
		{
			zero.X += 1f;
		}
		if (InputService.IsKeyDown(KeybindInput.Back))
		{
			zero.X -= 1f;
		}
		if (InputService.IsKeyDown(KeybindInput.Left))
		{
			zero.Z += 1f;
		}
		if (InputService.IsKeyDown(KeybindInput.Right))
		{
			zero.Z -= 1f;
		}
		if (InputService.IsKeyDown(KeybindInput.Ascend))
		{
			zero.Y += 1f;
		}
		if (InputService.IsKeyDown(KeybindInput.Descend))
		{
			zero.Y -= 1f;
		}
		int num = InputService.ScrollDelta();
		if (num != 0)
		{
			speed *= 1f + 0.2f * (float)num;
			speed = Math.Clamp(speed, 0.005f, 10f);
		}
		if (!(zero == Vector3.Zero))
		{
			zero *= (float)(_services.Framework.UpdateDelta.TotalSeconds * 20.0) * speed;
			if (InputService.IsKeyDown(KeybindInput.Fast_Speed, consume: false))
			{
				zero *= 5f;
			}
			if (InputService.IsKeyDown(KeybindInput.Slow_Speed, consume: false))
			{
				zero *= 0.5f;
			}
			float x = gameCamera->currentHRotation + (float)Math.PI / 2f;
			float currentVRotation = gameCamera->currentVRotation;
			Vector3 vector = new Vector3(MathF.Cos(x) * MathF.Cos(currentVRotation), MathF.Sin(currentVRotation), 0f - MathF.Sin(x) * MathF.Cos(currentVRotation)) * zero.X;
			float num2 = vector.X + zero.Z * MathF.Sin(gameCamera->currentHRotation - (float)Math.PI / 2f);
			float num3 = vector.Y + zero.Y;
			float num4 = vector.Z + zero.Z * MathF.Cos(gameCamera->currentHRotation - (float)Math.PI / 2f);
			if (_services.ClientState.IsLoggedIn)
			{
				Position.X += num2;
				Position.Y += num3;
				Position.Z += num4;
			}
			else
			{
				gameCamera->lookAtX += num2;
				gameCamera->lookAtY = (gameCamera->lookAtY2 += num3);
				gameCamera->lookAtZ += num4;
			}
		}
	}
}

using System.Linq;
using PyonCam.Config;
using PyonCam.Config.Cam;

namespace PyonCam.Services;

public class PresetService
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	private CameraService CameraService => _services.Get<CameraService>();

	public CameraConfigPreset CurrentPreset
	{
		get
		{
			return ActivePreset ?? DefaultPreset;
		}
		set
		{
			CameraService cameraService = CameraService;
			CameraConfigPreset preset = (ActivePreset = value);
			cameraService.ApplyPreset(preset);
		}
	}

	public CameraConfigPreset DefaultPreset { get; private set; }

	public CameraConfigPreset? ActivePreset { get; private set; }

	public PresetService(Configuration config, IServiceContext services)
	{
		_config = config;
		_services = services;
	}

	public void Initialize()
	{
		DefaultPreset = new CameraConfigPreset();
		CameraConfigPreset cameraConfigPreset = _config.Presets.FirstOrDefault((CameraConfigPreset x) => x.ID == _config.SelectedPresetID);
		CurrentPreset = ((cameraConfigPreset != null) ? cameraConfigPreset : DefaultPreset);
	}
}

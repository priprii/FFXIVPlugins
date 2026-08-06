using PyonPix.Config.UI.Properties;

namespace PyonPix.Config.UI;

public class UIProperties
{
	public MainUIProperties Main { get; set; } = new MainUIProperties();

	public BrowserUIProperties Browser { get; set; } = new BrowserUIProperties();

	public ExtensionsUIProperties Extensions { get; set; } = new ExtensionsUIProperties();

	public DataUIProperties Data { get; set; } = new DataUIProperties();

	public SyncSearchUIProperties SyncSearch { get; set; } = new SyncSearchUIProperties();

	public PixConfigUIProperties PixConfig { get; set; } = new PixConfigUIProperties();

	public PixMembersUIProperties PixMembers { get; set; } = new PixMembersUIProperties();

	public ConfigUIProperties Config { get; set; } = new ConfigUIProperties();

	public UserUIProperties User { get; set; } = new UserUIProperties();

	public UpdatesUIProperties Updates { get; set; } = new UpdatesUIProperties();
}

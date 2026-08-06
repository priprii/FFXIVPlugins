namespace PvPyon.Api.Icons;

public class JobIconSet
{
	private readonly int[] icons;

	public float IconScale { get; init; }

	public JobIconSet(int[] icons, float iconScale)
	{
		this.icons = icons;
		IconScale = iconScale;
	}

	public int GetIcon(uint jobID)
	{
		return icons[jobID - 1];
	}
}

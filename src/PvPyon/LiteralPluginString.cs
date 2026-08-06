namespace PvPyon;

public class LiteralPluginString : IPluginString
{
	private string m_Value;

	public string Value => m_Value;

	public LiteralPluginString(string value)
	{
		m_Value = value;
	}

	public override string ToString()
	{
		return Value;
	}
}

namespace PyonPix.Extensions;

public static class StringEx
{
	public static string TruncateMiddle(this string text, int max)
	{
		if (string.IsNullOrEmpty(text) || text.Length <= max)
		{
			return text;
		}
		int num = (max - 3) / 2;
		string text2 = text.Substring(0, num);
		int num2 = num;
		return text2 + "..." + text.Substring(text.Length - num2);
	}
}

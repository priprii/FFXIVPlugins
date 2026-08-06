using System;

namespace Ktisis.Common.Utility;

public static class PlayerNameUtil
{
	private static readonly string[] Single = new string[16]
	{
		"Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
		"Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen"
	};

	private static readonly string[] Tens = new string[8] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

	public static string CalcActorName(ushort index, string firstName = "Actor")
	{
		return firstName + " " + CalcActorNameWords(index);
	}

	private static string CalcActorNameWords(ushort index)
	{
		if (index >= 200)
		{
			index -= 200;
		}
		if (index < Single.Length)
		{
			return Single[index];
		}
		if (index < 20)
		{
			int num = index - 10;
			string text = Single[num];
			if (text.EndsWith('t'))
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 1);
			}
			return text + "teen";
		}
		int num2 = (int)Math.Floor((decimal)(index - 20) / 10m);
		int num3 = index % 10;
		string text3 = Tens[num2];
		if (num3 == 0)
		{
			return text3;
		}
		return text3 + "-" + Single[num3].ToLowerInvariant();
	}
}

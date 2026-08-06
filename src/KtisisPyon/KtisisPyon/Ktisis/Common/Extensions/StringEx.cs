using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Utility;

namespace Ktisis.Common.Extensions;

public static class StringEx
{
	public static string Truncate(this string str, int len, bool ellipsis = true)
	{
		if (str.Length <= len)
		{
			return str;
		}
		int num = Math.Min(len, str.Length);
		int num2 = Math.Min(len - 2, 3);
		if (num2 <= 1 || !ellipsis)
		{
			return str.Substring(0, num);
		}
		num -= num2;
		return str.Substring(0, num) + new string('.', num2);
	}

	public static string FitToWidth(this string str, float width, bool ellipsis = true)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		string text = str;
		int num = text.Length;
		bool flag = false;
		while (num > 0 && ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X > width)
		{
			flag = true;
			num--;
			text = text.Substring(0, num);
		}
		if (ellipsis && flag && num >= 5)
		{
			num -= 3;
			text = text.Substring(0, num) + new string('.', 3);
		}
		return text;
	}

	public static string? FormatName(this string name, sbyte article)
	{
		if (StringExtensions.IsNullOrEmpty(name))
		{
			return null;
		}
		if (article != 1)
		{
			return string.Join(' ', name.Split(' ').Select(delegate(string word, int index)
			{
				bool flag = word.Length <= 1;
				if (!flag)
				{
					bool flag2 = index > 0;
					if (flag2)
					{
						bool flag3;
						switch (word)
						{
						case "of":
						case "the":
						case "and":
							flag3 = true;
							break;
						default:
							flag3 = false;
							break;
						}
						flag2 = flag3;
					}
					flag = flag2;
				}
				return flag ? word : (word[0].ToString().ToUpper() + word.Substring(1));
			}));
		}
		return name;
	}
}

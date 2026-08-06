using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TriggerPyon;

public class DisGen
{
	public byte[] PB = new byte[64]
	{
		51, 48, 55, 51, 55, 55, 101, 57, 51, 55,
		98, 49, 50, 49, 54, 49, 53, 52, 98, 55,
		53, 99, 57, 50, 56, 52, 51, 102, 48, 57,
		100, 54, 102, 54, 48, 56, 49, 101, 52, 56,
		49, 51, 51, 99, 53, 57, 50, 52, 53, 100,
		100, 56, 56, 98, 99, 100, 56, 101, 50, 102,
		54, 98, 50, 98
	};

	private byte[] TB = new byte[152]
	{
		47, 50, 70, 69, 99, 55, 110, 54, 68, 103,
		50, 110, 68, 78, 85, 55, 69, 53, 120, 119,
		122, 84, 48, 84, 122, 50, 87, 82, 109, 79,
		105, 52, 101, 65, 47, 55, 43, 102, 85, 51,
		101, 109, 71, 83, 104, 48, 83, 48, 116, 50,
		106, 106, 77, 66, 122, 121, 57, 118, 70, 118,
		100, 120, 56, 83, 87, 74, 72, 119, 74, 122,
		121, 78, 54, 51, 72, 86, 79, 121, 47, 101,
		103, 86, 110, 49, 47, 108, 49, 70, 53, 121,
		49, 122, 116, 52, 89, 74, 80, 90, 117, 108,
		72, 55, 110, 54, 49, 50, 47, 81, 97, 66,
		122, 121, 99, 120, 122, 88, 120, 56, 105, 47,
		76, 103, 98, 110, 71, 70, 72, 49, 107, 68,
		87, 118, 115, 65, 85, 122, 121, 122, 69, 114,
		102, 75, 105, 57, 102, 112, 55, 118, 108, 81,
		61, 61
	};

	private string PB_S()
	{
		return Encoding.UTF8.GetString(PB);
	}

	private string TB_S()
	{
		return Encoding.UTF8.GetString(TB);
	}

	public string Dec()
	{
		byte[] array = Convert.FromBase64String(TB_S());
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(PB_S(), array.Take(16).ToArray(), 1000);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			BlockSize = 128,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		using ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rfc2898DeriveBytes.GetBytes(16), array.Skip(16).Take(16).ToArray());
		using MemoryStream stream = new MemoryStream(array.Skip(32).Take(array.Length - 32).ToArray());
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2, Encoding.UTF8);
		return streamReader.ReadToEnd();
	}

	private static byte[] EntGen()
	{
		byte[] array = new byte[16];
		using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		randomNumberGenerator.GetBytes(array);
		return array;
	}

	public string EncId(string id)
	{
		byte[] array = EntGen();
		byte[] array2 = EntGen();
		byte[] bytes = Encoding.UTF8.GetBytes(id);
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(PB_S(), array, 1000);
		byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.BlockSize = 128;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.PKCS7;
		using ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes2, array2);
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		byte[] inArray = array.Concat(array2).ToArray().Concat(memoryStream.ToArray())
			.ToArray();
		memoryStream.Close();
		cryptoStream.Close();
		return Convert.ToBase64String(inArray);
	}

	public string DecId(string id)
	{
		byte[] array = Convert.FromBase64String(id);
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(PB_S(), array.Take(16).ToArray(), 1000);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			BlockSize = 128,
			Mode = CipherMode.CBC,
			Padding = PaddingMode.PKCS7
		};
		using ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rfc2898DeriveBytes.GetBytes(16), array.Skip(16).Take(16).ToArray());
		using MemoryStream stream = new MemoryStream(array.Skip(32).Take(array.Length - 32).ToArray());
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2, Encoding.UTF8);
		return streamReader.ReadToEnd();
	}

	public string GenKey()
	{
		using RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[16];
		rNGCryptoServiceProvider.GetBytes(array);
		return new string(array.Select((byte b) => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[b % "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Length]).ToArray());
	}
}

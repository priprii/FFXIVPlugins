using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Hypostasis.Dalamud;

namespace Hypostasis;

public static class Util
{
	public class AssignableInfo(object o, MemberInfo info)
	{
		private readonly FieldInfo fieldInfo = info as FieldInfo;

		private readonly PropertyInfo propertyInfo = info as PropertyInfo;

		public object Object { get; init; } = o;

		public MemberInfo MemberInfo { get; init; } = info;

		public string Name => MemberInfo.Name;

		public Type Type => MemberInfo.GetObjectType();

		public object GetValue()
		{
			object obj = fieldInfo?.GetValue(Object);
			if (obj == null)
			{
				PropertyInfo obj2 = propertyInfo;
				if ((object)obj2 == null)
				{
					return null;
				}
				obj = obj2.GetValue(Object);
			}
			return obj;
		}

		public void SetValue(object v)
		{
			fieldInfo?.SetValue(Object, v);
			propertyInfo?.SetValue(Object, v);
		}
	}

	public const BindingFlags AllMembersBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static bool IsWindowFocused
	{
		get
		{
			nint foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return false;
			}
			int processId = Environment.ProcessId;
			GetWindowThreadProcessId(foregroundWindow, out var processId2);
			return processId2 == processId;
		}
	}

	public static bool IsAprilFools
	{
		get
		{
			DateTime now = DateTime.Now;
			if (now.Month == 4)
			{
				return now.Day == 1;
			}
			return false;
		}
	}

	public static Assembly Assembly => System.Reflection.Assembly.GetExecutingAssembly();

	public static Type[] AssemblyTypes => Assembly.GetTypes();

	public static AssemblyName AssemblyName => Assembly.GetName();

	[DllImport("user32.dll", ExactSpelling = true)]
	[LibraryImport("user32.dll")]
	private static extern nint GetForegroundWindow();

	[LibraryImport("user32.dll", SetLastError = true)]
	[GeneratedCode("Microsoft.Interop.LibraryImportGenerator", "10.0.14.15411")]
	[SkipLocalsInit]
	private unsafe static int GetWindowThreadProcessId(nint handle, out int processId)
	{
		processId = 0;
		int result;
		int lastSystemError;
		fixed (int* _processId_native = &processId)
		{
			Marshal.SetLastSystemError(0);
			result = __PInvoke(handle, _processId_native);
			lastSystemError = Marshal.GetLastSystemError();
		}
		Marshal.SetLastPInvokeError(lastSystemError);
		return result;
		[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", ExactSpelling = true)]
		static extern unsafe int __PInvoke(nint __handle_native, int* __processId_native);
	}

	public static IEnumerable<Type> GetTypes<T>(this Assembly assembly)
	{
		if (!typeof(T).IsInterface)
		{
			return from t in assembly.GetTypes()
				where !t.IsAbstract && t.IsSubclassOf(typeof(T))
				select t;
		}
		return from t in assembly.GetTypes()
			where !t.IsAbstract && t.IsAssignableTo(typeof(T))
			select t;
	}

	public static IEnumerable<(Type, T)> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute
	{
		return from t in assembly.GetTypes()
			let attribute = t.GetCustomAttribute<T>()
			where attribute != null
			select (t: t, attribute: attribute);
	}

	public static MemberInfo[] GetAllMembers(this IReflect type)
	{
		return type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static IEnumerable<(MemberInfo, T)> GetAllMembersWithAttribute<T>(this IReflect type) where T : Attribute
	{
		return from memberInfo in type.GetAllMembers()
			let attribute = memberInfo.GetCustomAttribute<T>()
			where attribute != null
			select (memberInfo: memberInfo, attribute: attribute);
	}

	public static FieldInfo[] GetAllFields(this IReflect type)
	{
		return type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static PropertyInfo[] GetAllProperties(this IReflect type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static MethodInfo[] GetAllMethods(this IReflect type)
	{
		return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static bool DeclaresMethod(this Type type, string method, Type[] types)
	{
		return type.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, types)?.DeclaringType == type;
	}

	public static bool DeclaresMethod(this Type type, string method)
	{
		return type.DeclaresMethod(method, Type.EmptyTypes);
	}

	public static Type GetObjectType(this MemberInfo memberInfo)
	{
		if (!(memberInfo is FieldInfo fieldInfo))
		{
			if (memberInfo is PropertyInfo propertyInfo)
			{
				return propertyInfo.PropertyType;
			}
			return null;
		}
		return fieldInfo.FieldType;
	}

	public static bool StartProcess(ProcessStartInfo startInfo)
	{
		try
		{
			Process.Start(startInfo);
			return true;
		}
		catch (Exception exception)
		{
			DalamudApi.LogError("Failed to start process!", exception);
			return false;
		}
	}

	public static bool StartProcess(string process, bool admin = false)
	{
		return StartProcess(new ProcessStartInfo
		{
			FileName = process,
			UseShellExecute = true,
			Verb = (admin ? "runas" : string.Empty)
		});
	}

	public static string CompressString(string s, string prefix = "")
	{
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return prefix + Convert.ToBase64String(memoryStream.ToArray());
	}

	public static string DecompressString(string s, string prefix = "")
	{
		if (!s.StartsWith(prefix))
		{
			throw new ApplicationException("This export is for a different plugin.");
		}
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(s.Substring(prefix.Length)));
		using GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}

	public static int ToMilliseconds(this float f)
	{
		return (int)(f * 1000f);
	}

	public static int ToMilliseconds(this double d)
	{
		return (int)(d * 1000.0);
	}

	public unsafe static nint ConvertObjectToIntPtr(object o)
	{
		if (!(o is Pointer ptr))
		{
			if (!(o is nint result))
			{
				if (!(o is nuint result2))
				{
					if (o != null && IsNumeric(o))
					{
						return (nint)Convert.ToInt64(o);
					}
					return IntPtr.Zero;
				}
				return (nint)result2;
			}
			return result;
		}
		return (nint)Pointer.Unbox(ptr);
	}

	public static bool IsNumeric(object o)
	{
		if (!(o is long))
		{
			if (!(o is ulong))
			{
				if (!(o is int))
				{
					if (!(o is uint))
					{
						if (!(o is short))
						{
							if (!(o is ushort))
							{
								if (!(o is sbyte))
								{
									if (!(o is byte))
									{
										if (!(o is double))
										{
											if (!(o is float))
											{
												if (o is decimal)
												{
													return true;
												}
												return false;
											}
											return true;
										}
										return true;
									}
									return true;
								}
								return true;
							}
							return true;
						}
						return true;
					}
					return true;
				}
				return true;
			}
			return true;
		}
		return true;
	}

	public static bool IsValidHookAddress(this nint address)
	{
		return DalamudApi.SigScanner.IsValidHookAddress(address);
	}

	public unsafe static T Deref<T>(this nint address, long offset = 0L) where T : unmanaged
	{
		return *(T*)(address + offset);
	}

	public static string ReadCString(this nint address)
	{
		return Marshal.PtrToStringUTF8(address);
	}

	public static string ReadCString(this nint address, int len)
	{
		return Marshal.PtrToStringUTF8(address, len);
	}

	public static void WriteCString(this nint address, string str)
	{
		try
		{
			for (int i = 0; i < str.Length; i++)
			{
				char value = str[i];
				Marshal.WriteByte(address + i, Convert.ToByte(value));
			}
		}
		catch
		{
		}
		Marshal.WriteByte(address + str.Length, 0);
	}

	public static object Cast(this Type type, object data)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "data");
		return Expression.Lambda(Expression.Block(Expression.Convert(Expression.Convert(parameterExpression, data.GetType()), type)), parameterExpression).Compile().DynamicInvoke(data);
	}

	public static Vector2 Rotate(this Vector2 v, float a)
	{
		float aCos = (float)Math.Cos(a);
		float aSin = (float)Math.Sin(a);
		return v.Rotate(aCos, aSin);
	}

	public static Vector2 Rotate(this Vector2 v, float aCos, float aSin)
	{
		return new Vector2(v.X * aCos - v.Y * aSin, v.X * aSin + v.Y * aCos);
	}

	public static Vector3 RotateAroundY(this Vector3 v, float a)
	{
		float aCos = (float)Math.Cos(a);
		float aSin = (float)Math.Sin(a);
		return v.RotateAroundY(aCos, aSin);
	}

	public static Vector3 RotateAroundY(this Vector3 v, float aCos, float aSin)
	{
		return new Vector3(v.X * aCos + v.Z * aSin, v.Y, v.Z * aCos - v.X * aSin);
	}

	public static void Shift(this IList list, int i, int amount)
	{
		int count = list.Count;
		if (i >= 0 && i < count)
		{
			object value = list[i];
			list.RemoveAt(i);
			list.Insert(Math.Min(Math.Max(i + amount, 0), list.Count), value);
		}
	}

	public static void Shift(this IList list, int i, float amount)
	{
		list.Shift(i, (int)amount);
	}

	public static IEnumerable<K> SelectKeys<K, V>(this Dictionary<K, V> dict)
	{
		return dict.Select<KeyValuePair<K, V>, K>((KeyValuePair<K, V> kv) => kv.Key);
	}

	public static IEnumerable<V> SelectValues<K, V>(this Dictionary<K, V> dict)
	{
		return dict.Select<KeyValuePair<K, V>, V>((KeyValuePair<K, V> kv) => kv.Value);
	}

	public static string GetDisplayName<T>(this T e) where T : struct, Enum
	{
		string name = Enum.GetName(e);
		return typeof(T).GetField(name).GetCustomAttribute<DisplayAttribute>()?.Name ?? name;
	}
}

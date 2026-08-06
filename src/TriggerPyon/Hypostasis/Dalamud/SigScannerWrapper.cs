using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace Hypostasis.Dalamud;

public class SigScannerWrapper(ISigScanner s) : IDisposable
{
	private readonly Dictionary<string, nint> sigCache = new Dictionary<string, nint>();

	private readonly Dictionary<string, nint> staticSigCache = new Dictionary<string, nint>();

	private readonly List<IDisposable> disposableHooks = new List<IDisposable>();

	public ISigScanner DalamudSigScanner { get; init; } = s;

	public ProcessModule Module => DalamudSigScanner.Module;

	public nint BaseAddress => Module.BaseAddress;

	public nint BaseTextAddress => (nint)(BaseAddress + DalamudSigScanner.TextSectionOffset);

	public nint BaseDataAddress => (nint)(BaseAddress + DalamudSigScanner.DataSectionOffset);

	public nint BaseRDataAddress => (nint)(BaseAddress + DalamudSigScanner.RDataSectionOffset);

	public nint Scan(nint address, int size, string signature)
	{
		int num;
		if (address >= BaseAddress)
		{
			num = ((address < BaseRDataAddress) ? 1 : 0);
			if (num != 0)
			{
				address = (nint)DalamudSigScanner.SearchBase + (address - BaseAddress);
			}
		}
		else
		{
			num = 0;
		}
		nint num2 = SigScanner.Scan((IntPtr)address, size, signature);
		if (num != 0 && num2 >= (nint)DalamudSigScanner.SearchBase)
		{
			num2 = BaseAddress + (num2 - (nint)DalamudSigScanner.SearchBase);
		}
		return num2;
	}

	public nint Scan(nint address, nint endAddress, string signature)
	{
		return Scan(address, (int)(endAddress - address), signature);
	}

	public bool TryScan(nint address, int size, string signature, out nint result)
	{
		bool flag = address >= BaseAddress && address < BaseRDataAddress;
		if (flag)
		{
			address = (nint)DalamudSigScanner.SearchBase + (address - BaseAddress);
		}
		bool result2 = SigScanner.TryScan((IntPtr)address, size, signature, ref result);
		if (flag && result >= (nint)DalamudSigScanner.SearchBase)
		{
			result = BaseAddress + (result - (nint)DalamudSigScanner.SearchBase);
		}
		return result2;
	}

	public bool TryScan(nint address, nint endAddress, string signature, out nint result)
	{
		return TryScan(address, (int)(endAddress - address), signature, out result);
	}

	public nint ScanText(string signature)
	{
		if (sigCache.TryGetValue(signature, out var value))
		{
			return value;
		}
		value = DalamudSigScanner.ScanText(signature);
		AddSignatureInfo(signature, value, 0, stc: false);
		return value;
	}

	public bool TryScanText(string signature, out nint result)
	{
		if (sigCache.TryGetValue(signature, out result))
		{
			return true;
		}
		bool result2 = DalamudSigScanner.TryScanText(signature, ref result);
		AddSignatureInfo(signature, result, 0, stc: false);
		return result2;
	}

	public nint ScanData(string signature)
	{
		if (sigCache.TryGetValue(signature, out var value))
		{
			return value;
		}
		value = DalamudSigScanner.ScanData(signature);
		AddSignatureInfo(signature, value, 0, stc: false);
		return value;
	}

	public bool TryScanData(string signature, out nint result)
	{
		if (sigCache.TryGetValue(signature, out result))
		{
			return true;
		}
		bool result2 = DalamudSigScanner.TryScanData(signature, ref result);
		AddSignatureInfo(signature, result, 0, stc: false);
		return result2;
	}

	public nint ScanModule(string signature)
	{
		if (sigCache.TryGetValue(signature, out var value))
		{
			return value;
		}
		value = DalamudSigScanner.ScanModule(signature);
		AddSignatureInfo(signature, value, 0, stc: false);
		return value;
	}

	public bool TryScanModule(string signature, out nint result)
	{
		if (sigCache.TryGetValue(signature, out result))
		{
			return true;
		}
		bool result2 = DalamudSigScanner.TryScanModule(signature, ref result);
		AddSignatureInfo(signature, result, 0, stc: false);
		return result2;
	}

	public nint ScanStaticAddress(string signature, int offset = 0)
	{
		if (offset == 0 && staticSigCache.TryGetValue(signature, out var value))
		{
			return value;
		}
		value = DalamudSigScanner.GetStaticAddressFromSig(signature, offset);
		AddSignatureInfo(signature, value, offset, stc: true);
		return value;
	}

	public bool TryScanStaticAddress(string signature, out nint result, int offset = 0)
	{
		if (offset == 0 && staticSigCache.TryGetValue(signature, out result))
		{
			return true;
		}
		bool result2 = DalamudSigScanner.TryGetStaticAddressFromSig(signature, ref result, offset);
		AddSignatureInfo(signature, result, offset, stc: true);
		return result2;
	}

	private Hook<T> HookAddress<T>(nint address, T detour, bool startEnabled = true, bool autoDispose = true, HookBackend backend = (HookBackend)0) where T : Delegate
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Hook<T> val = DalamudApi.GameInteropProvider.HookFromAddress<T>((IntPtr)address, detour, backend);
		AddHook<T>(val, startEnabled, autoDispose);
		return val;
	}

	private Hook<T> HookSignature<T>(string signature, T detour, bool scanModule = false, bool startEnabled = true, bool autoDispose = true, HookBackend backend = (HookBackend)0) where T : Delegate
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		nint num = ((!scanModule) ? DalamudSigScanner.ScanText(signature) : DalamudSigScanner.ScanModule(signature));
		Hook<T> val = DalamudApi.GameInteropProvider.HookFromAddress<T>((IntPtr)num, detour, backend);
		AddSignatureInfo(signature, num, 0, stc: false);
		AddHook<T>(val, startEnabled, autoDispose);
		return val;
	}

	private void AddSignatureInfo(string signature, nint ptr, int offset, bool stc)
	{
		if (!stc)
		{
			sigCache[signature] = ptr;
		}
		else
		{
			staticSigCache[signature] = ptr;
		}
	}

	public void InjectSignatures()
	{
		foreach (var item2 in Util.Assembly.GetTypesWithAttribute<HypostasisInjectionAttribute>())
		{
			Type item = item2.Item1;
			Inject(item);
		}
	}

	public void Inject(Type type, object o = null)
	{
		foreach (MemberInfo item in type.GetAllMembers().Where(delegate(MemberInfo memberInfo)
		{
			MemberTypes memberType = memberInfo.MemberType;
			return (memberType == MemberTypes.Field || memberType == MemberTypes.Property) ? true : false;
		}))
		{
			InjectMember(o, item);
		}
	}

	public void Inject(object o)
	{
		Inject(o.GetType(), o);
	}

	public void InjectMember(object o, MemberInfo memberInfo)
	{
		HypostasisMemberInjectionAttribute customAttribute = memberInfo.GetCustomAttribute<HypostasisMemberInjectionAttribute>();
		if (customAttribute == null)
		{
			return;
		}
		if (!(customAttribute is HypostasisSignatureInjectionAttribute sigAttribute))
		{
			if (customAttribute is HypostasisClientStructsInjectionAttribute csAttribute)
			{
				InjectClientStructs(o, memberInfo, csAttribute);
			}
		}
		else
		{
			InjectSignature(o, memberInfo, sigAttribute);
		}
	}

	private void InjectSignature(object o, MemberInfo memberInfo, HypostasisSignatureInjectionAttribute sigAttribute)
	{
		Util.AssignableInfo assignableInfo = new Util.AssignableInfo(o, memberInfo);
		string signature = sigAttribute.Signature;
		bool flag = sigAttribute.Static;
		nint address = default(nint);
		if ((!flag) ? (!DalamudSigScanner.TryScanText(signature, ref address)) : (!DalamudSigScanner.TryGetStaticAddressFromSig(signature, ref address, 0)))
		{
			LogInjectError(memberInfo, $"Failed to find signature: \"{signature}\" (Static: {flag})", sigAttribute.Required);
		}
		else
		{
			InjectAddress(assignableInfo, address, sigAttribute);
		}
	}

	private void InjectClientStructs(object o, MemberInfo memberInfo, HypostasisClientStructsInjectionAttribute csAttribute)
	{
		string name = (memberInfo.Name.EndsWith("Hook") ? memberInfo.Name.Replace("Hook", string.Empty) : csAttribute.MemberName);
		MemberInfo memberInfo2 = csAttribute.ClientStructsType.GetMember(name)[0];
		Util.AssignableInfo assignableInfo = new Util.AssignableInfo(o, memberInfo);
		object obj;
		if (!(memberInfo2 is FieldInfo fieldInfo))
		{
			if (!(memberInfo2 is PropertyInfo propertyInfo))
			{
				if (!(memberInfo2 is MethodInfo methodInfo))
				{
					throw new ApplicationException("Member type is unsupported");
				}
				obj = methodInfo.Invoke(null, Array.Empty<object>());
			}
			else
			{
				obj = propertyInfo.GetValue(null);
			}
		}
		else
		{
			obj = fieldInfo.GetValue(null);
		}
		object o2 = obj;
		InjectAddress(assignableInfo, Util.ConvertObjectToIntPtr(o2), csAttribute);
	}

	private void InjectAddress(Util.AssignableInfo assignableInfo, nint address, HypostasisMemberInjectionAttribute attribute)
	{
		address += attribute.Offset;
		Type type = assignableInfo.Type;
		if (type == typeof(nint) || type.IsPointer || type.IsFunctionPointer)
		{
			assignableInfo.SetValue(address);
		}
		else if (type.IsAssignableTo(typeof(Delegate)))
		{
			assignableInfo.SetValue(Marshal.GetDelegateForFunctionPointer(address, type));
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hook<>))
		{
			InjectHook(assignableInfo, address, attribute);
		}
		else if (type.IsPrimitive)
		{
			assignableInfo.SetValue(Marshal.PtrToStructure(address, type));
		}
		else
		{
			LogInjectError(assignableInfo.MemberInfo, "Failed to determine how to inject member", attribute.Required);
		}
	}

	private void InjectHook(Util.AssignableInfo assignableInfo, nint address, HypostasisMemberInjectionAttribute attribute)
	{
		Type reflectedType = assignableInfo.MemberInfo.ReflectedType;
		object o = assignableInfo.Object;
		Type type = assignableInfo.Type;
		Type type2 = type.GenericTypeArguments[0];
		if (!IsValidHookAddress(address))
		{
			LogInjectError(assignableInfo.MemberInfo, $"Attempted to place hook on invalid location {address:X}", attribute.Required);
			return;
		}
		Delegate obj = GetMethodDelegate(reflectedType, type2, o, assignableInfo.Name.Replace("Hook", "Detour"));
		if ((object)obj == null)
		{
			string detourName = attribute.DetourName;
			if (detourName != null)
			{
				obj = GetMethodDelegate(reflectedType, type2, o, detourName);
				if ((object)obj == null)
				{
					LogInjectError(assignableInfo.MemberInfo, "Detour not found or was incompatible with delegate \"" + detourName + "\" " + type2.Name, attribute.Required);
					return;
				}
			}
			else
			{
				Delegate[] methodDelegates = GetMethodDelegates(reflectedType, type2, o);
				if (methodDelegates.Length != 1)
				{
					LogInjectError(assignableInfo.MemberInfo, $"Found {methodDelegates.Length} matching detours: specify a detour name", attribute.Required);
					return;
				}
				obj = methodDelegates[0];
			}
		}
		object obj2 = type.GetMethod("FromAddress", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, new object[3] { address, obj, false });
		assignableInfo.SetValue(obj2);
		if (attribute.EnableHook)
		{
			type.GetMethod("Enable")?.Invoke(obj2, null);
		}
		if (attribute.DisposeHook)
		{
			disposableHooks.Add(obj2 as IDisposable);
		}
	}

	private static Delegate GetMethodDelegate(Type ownerType, Type delegateType, object o, string methodName)
	{
		MethodInfo method = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		return CreateDelegate(delegateType, o, method);
	}

	private static Delegate[] GetMethodDelegates(IReflect ownerType, Type delegateType, object o)
	{
		return (from methodInfo in ownerType.GetAllMethods()
			select CreateDelegate(delegateType, o, methodInfo) into del
			where (object)del != null
			select del).ToArray();
	}

	private static Delegate CreateDelegate(Type delegateType, object o, MethodInfo delegateMethod)
	{
		if (delegateType == null)
		{
			return null;
		}
		if (!delegateMethod.IsStatic)
		{
			return Delegate.CreateDelegate(delegateType, o, delegateMethod, throwOnBindFailure: false);
		}
		return Delegate.CreateDelegate(delegateType, delegateMethod, throwOnBindFailure: false);
	}

	public void AddHook<T>(Hook<T> hook, bool enable = true, bool dispose = true) where T : Delegate
	{
		if (enable)
		{
			hook.Enable();
		}
		if (dispose)
		{
			disposableHooks.Add((IDisposable)hook);
		}
	}

	public void InjectMember(Type type, object o, string member)
	{
		InjectMember(o, type.GetMember(member, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)[0]);
	}

	private static void LogInjectError(MemberInfo memberInfo, string message, bool required)
	{
		string message2 = $"Error injecting {memberInfo.ReflectedType?.FullName}.{memberInfo.Name}:\n{message}";
		if (required)
		{
			throw new ApplicationException(message2);
		}
		DalamudApi.LogWarning(message2);
	}

	public unsafe bool IsValidHookAddress(nint address)
	{
		if (address != BaseTextAddress)
		{
			if (address > BaseTextAddress && address < BaseRDataAddress && *(byte*)address != 204)
			{
				return *(byte*)(address - 1) == 204;
			}
			return false;
		}
		return true;
	}

	public void Dispose()
	{
		foreach (IDisposable disposableHook in disposableHooks)
		{
			disposableHook?.Dispose();
		}
		GC.SuppressFinalize(this);
	}
}

using System;
using System.Reflection;
using Dalamud.Hooking;
using Hypostasis.Dalamud;
using Hypostasis.Game;
using Newtonsoft.Json;

namespace Hypostasis.Debug;

public sealed class HypostasisMemberDebugInfo
{
	public enum MemberDebugType
	{
		None,
		Pointer,
		Primitive,
		Hook,
		AsmHook,
		AsmPatch,
		GameFunction
	}

	public Util.AssignableInfo AssignableInfo { get; set; }

	[JsonIgnore]
	public HypostasisMemberInjectionAttribute InjectionAttribute => (HypostasisMemberInjectionAttribute)(((object)SignatureInjectionAttribute) ?? ((object)CSInjectionAttribute));

	public HypostasisSignatureInjectionAttribute SignatureInjectionAttribute { get; set; }

	public HypostasisClientStructsInjectionAttribute CSInjectionAttribute { get; set; }

	[JsonIgnore]
	public string Signature
	{
		get
		{
			if (SignatureInjectionAttribute != null)
			{
				return SignatureInjectionAttribute.Signature;
			}
			try
			{
				object obj = AssignableInfo?.GetValue();
				if (obj == null)
				{
					return string.Empty;
				}
				MemberDebugType debugType = DebugType;
				if ((uint)(debugType - 5) <= 1u)
				{
					object obj2 = obj.GetType().GetProperty("Signature")?.GetValue(obj);
					return (obj2 != null) ? ((string)obj2) : string.Empty;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	[JsonIgnore]
	public nint Address
	{
		get
		{
			try
			{
				object obj = AssignableInfo?.GetValue();
				if (obj == null)
				{
					return IntPtr.Zero;
				}
				switch (DebugType)
				{
				case MemberDebugType.Pointer:
				case MemberDebugType.Primitive:
					return Util.ConvertObjectToIntPtr(obj);
				case MemberDebugType.Hook:
				case MemberDebugType.AsmHook:
				case MemberDebugType.AsmPatch:
				case MemberDebugType.GameFunction:
				{
					object obj2 = obj.GetType().GetProperty("Address")?.GetValue(obj);
					return (obj2 != null) ? ((nint)obj2) : IntPtr.Zero;
				}
				}
			}
			catch
			{
			}
			return IntPtr.Zero;
		}
	}

	public MemberDebugType DebugType { get; set; }

	public HypostasisMemberDebugInfo()
	{
	}

	public HypostasisMemberDebugInfo(MemberInfo memberInfo)
	{
		HypostasisMemberInjectionAttribute customAttribute = memberInfo.GetCustomAttribute<HypostasisMemberInjectionAttribute>();
		SignatureInjectionAttribute = customAttribute as HypostasisSignatureInjectionAttribute;
		CSInjectionAttribute = customAttribute as HypostasisClientStructsInjectionAttribute;
		Type objectType = memberInfo.GetObjectType();
		if (objectType == typeof(nint) || objectType.IsPointer || objectType.IsAssignableTo(typeof(Delegate)))
		{
			DebugType = MemberDebugType.Pointer;
		}
		else if (objectType.IsGenericType)
		{
			Type genericTypeDefinition = objectType.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(Hook<>))
			{
				DebugType = MemberDebugType.Hook;
			}
			else if (genericTypeDefinition == typeof(GameFunction<>))
			{
				DebugType = MemberDebugType.GameFunction;
			}
		}
		else if (objectType.IsPrimitive)
		{
			DebugType = MemberDebugType.Primitive;
		}
		else if (objectType == typeof(AsmHook))
		{
			DebugType = MemberDebugType.AsmHook;
		}
		else if (objectType == typeof(AsmPatch))
		{
			DebugType = MemberDebugType.AsmPatch;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Hypostasis.Dalamud;

namespace Hypostasis.Debug;

public static class DebugIPC
{
	public const string HypostasisTag = "_HYPOSTASISPLUGINS";

	private static readonly List<HypostasisMemberDebugInfo> debugInfos = new List<HypostasisMemberDebugInfo>();

	private static readonly Dictionary<int, (object, MemberInfo)> memberInfos = new Dictionary<int, (object, MemberInfo)>();

	private static readonly Dictionary<Type, object> injectedObjects = new Dictionary<Type, object>();

	public static bool DebugHypostasis { get; set; }

	public static ICallGateProvider<IDalamudPlugin> GetPluginProvider { get; private set; }

	public static ICallGateProvider<Hypostasis.PluginState> GetPluginStateProvider { get; private set; }

	public static ICallGateProvider<List<HypostasisMemberDebugInfo>> GetDebugInfosProvider { get; private set; }

	public static ICallGateProvider<Dictionary<int, (object, MemberInfo)>> GetMemberInfosProvider { get; private set; }

	[Conditional("DEBUG")]
	public static void Initialize(IDalamudPlugin plugin)
	{
		string pluginName = Hypostasis.PluginName;
		GetPluginProvider = DalamudApi.PluginInterface.GetIpcProvider<IDalamudPlugin>(pluginName + ".Hypostasis.GetPlugin");
		GetPluginProvider.RegisterFunc((Func<IDalamudPlugin>)(() => plugin));
		GetPluginStateProvider = DalamudApi.PluginInterface.GetIpcProvider<Hypostasis.PluginState>(pluginName + ".Hypostasis.GetPluginState");
		GetPluginStateProvider.RegisterFunc((Func<Hypostasis.PluginState>)(() => Hypostasis.State));
		GetDebugInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<List<HypostasisMemberDebugInfo>>(pluginName + ".Hypostasis.GetDebugInfos");
		GetDebugInfosProvider.RegisterFunc((Func<List<HypostasisMemberDebugInfo>>)(() => debugInfos));
		GetMemberInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<Dictionary<int, (object, MemberInfo)>>(pluginName + ".Hypostasis.GetMemberInfos");
		GetMemberInfosProvider.RegisterFunc((Func<Dictionary<int, (object, MemberInfo)>>)(() => memberInfos));
		DalamudApi.Framework.RunOnTick((Action)EnableDebugging, default(TimeSpan), 0, default(CancellationToken));
	}

	private static void EnableDebugging()
	{
		HashSet<string> orCreateData = DalamudApi.PluginInterface.GetOrCreateData<HashSet<string>>("_HYPOSTASISPLUGINS", (Func<HashSet<string>>)(() => new HashSet<string>()));
		lock (orCreateData)
		{
			orCreateData.Add(Hypostasis.PluginName);
		}
	}

	private static void DisableDebugging()
	{
		HashSet<string> hashSet = default(HashSet<string>);
		if (!DalamudApi.PluginInterface.TryGetData<HashSet<string>>("_HYPOSTASISPLUGINS", ref hashSet))
		{
			return;
		}
		lock (hashSet)
		{
			hashSet.Remove(Hypostasis.PluginName);
		}
	}

	[Conditional("DEBUG")]
	public static void AddInjectedObject(object o)
	{
		injectedObjects[o.GetType()] = o;
	}

	[Conditional("DEBUG")]
	public static void SetupDebugMembers()
	{
		HashSet<Type> debuggableTypes = (from t in Util.Assembly.GetTypesWithAttribute<HypostasisDebuggableAttribute>()
			select t.Item1).ToHashSet();
		IEnumerable<Type> enumerable;
		if (!DebugHypostasis)
		{
			enumerable = Util.AssemblyTypes.Where(delegate(Type type)
			{
				string text = type.Namespace;
				return text != null && !text.Contains("Hypostasis");
			});
		}
		else
		{
			IEnumerable<Type> assemblyTypes = Util.AssemblyTypes;
			enumerable = assemblyTypes;
		}
		foreach (Type item in enumerable)
		{
			foreach (MemberInfo item2 in item.GetAllMembers().Where(delegate(MemberInfo memberInfo)
			{
				MemberTypes memberType = memberInfo.MemberType;
				if ((memberType == MemberTypes.Field || memberType == MemberTypes.Property) ? true : false)
				{
					if (memberInfo.GetCustomAttribute<HypostasisMemberInjectionAttribute>() == null && memberInfo.GetCustomAttribute<HypostasisDebuggableAttribute>() == null)
					{
						Type objectType = memberInfo.GetObjectType();
						if ((object)objectType != null)
						{
							return debuggableTypes.Contains((!objectType.IsGenericType) ? objectType : objectType.GetGenericTypeDefinition());
						}
						return false;
					}
					return true;
				}
				return false;
			}))
			{
				AddDebugMember(item2);
			}
		}
	}

	private static void AddDebugMember(MemberInfo memberInfo)
	{
		Type reflectedType = memberInfo.ReflectedType;
		if (!(reflectedType == null))
		{
			injectedObjects.TryGetValue(reflectedType, out object value);
			memberInfos.Add(debugInfos.Count, (value, memberInfo));
			debugInfos.Add(new HypostasisMemberDebugInfo(memberInfo));
		}
	}

	[Conditional("DEBUG")]
	public static void Dispose()
	{
		DisableDebugging();
		DalamudApi.PluginInterface.RelinquishData("_HYPOSTASISPLUGINS");
		ICallGateProvider<IDalamudPlugin> getPluginProvider = GetPluginProvider;
		if (getPluginProvider != null)
		{
			((ICallGateProvider)getPluginProvider).UnregisterFunc();
		}
		ICallGateProvider<Hypostasis.PluginState> getPluginStateProvider = GetPluginStateProvider;
		if (getPluginStateProvider != null)
		{
			((ICallGateProvider)getPluginStateProvider).UnregisterFunc();
		}
		ICallGateProvider<List<HypostasisMemberDebugInfo>> getDebugInfosProvider = GetDebugInfosProvider;
		if (getDebugInfosProvider != null)
		{
			((ICallGateProvider)getDebugInfosProvider).UnregisterFunc();
		}
		ICallGateProvider<Dictionary<int, (object, MemberInfo)>> getMemberInfosProvider = GetMemberInfosProvider;
		if (getMemberInfosProvider != null)
		{
			((ICallGateProvider)getMemberInfosProvider).UnregisterFunc();
		}
	}
}

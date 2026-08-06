using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Configuration;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using InteropGenerator.Runtime;

namespace TargetPyon;

public static class GameConfig
{
	public class EntryWrapper
	{
		public string Name { get; }

		public unsafe ConfigEntry* Entry { get; }

		public unsafe object? Value
		{
			get
			{
				return ((ConfigEntry)Entry).Type switch
				{
					2 => ((ConfigValue)(&((ConfigEntry)Entry).Value)).UInt, 
					3 => ((ConfigValue)(&((ConfigEntry)Entry).Value)).Float, 
					4 => ((object)(*(Utf8String*)((ConfigValue)(&((ConfigEntry)Entry).Value)).String)/*cast due to constrained. prefix*/).ToString(), 
					_ => null, 
				};
			}
			set
			{
				switch (((ConfigEntry)Entry).Type)
				{
				case 2:
					if (value is uint num)
					{
						if (!((ConfigEntry)Entry).SetValueUInt(num, 1u))
						{
							throw new Exception("Failed");
						}
						break;
					}
					goto default;
				case 3:
					if (value is float valueFloat)
					{
						if (!((ConfigEntry)Entry).SetValueFloat(valueFloat))
						{
							throw new Exception("Failed");
						}
						break;
					}
					goto default;
				case 4:
					if (value is string valueString)
					{
						if (!((ConfigEntry)Entry).SetValueString(valueString))
						{
							throw new Exception("Failed");
						}
						break;
					}
					goto default;
				default:
					throw new ArgumentException("Invalid Value");
				}
			}
		}

		public unsafe EntryWrapper(ConfigEntry* entry, string name)
		{
			Name = name;
			Entry = entry;
		}
	}

	public class GameConfigSection
	{
		private unsafe readonly ConfigBase* configBase;

		private readonly Dictionary<string, uint> indexMap = new Dictionary<string, uint>();

		private readonly Dictionary<uint, string> nameMap = new Dictionary<uint, string>();

		private string[] ignoredNames = Array.Empty<string>();

		public unsafe uint ConfigCount => ((ConfigBase)configBase).ConfigCount;

		public unsafe EntryWrapper? this[uint i]
		{
			get
			{
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				if (i >= ((ConfigBase)configBase).ConfigCount)
				{
					return null;
				}
				ConfigEntry* configEntry = ((ConfigBase)configBase).ConfigEntry;
				configEntry = (ConfigEntry*)((byte*)configEntry + i * Unsafe.SizeOf<ConfigEntry>());
				if (!((CStringPointer)(&((ConfigEntry)configEntry).Name)).HasValue)
				{
					return null;
				}
				if (!nameMap.TryGetValue(i, out string value))
				{
					value = MemoryHelper.ReadStringNullTerminated((IntPtr)new IntPtr(CStringPointer.op_Implicit(((ConfigEntry)configEntry).Name)));
					nameMap.TryAdd(i, value);
					indexMap.TryAdd(value, i);
				}
				return new EntryWrapper(configEntry, value);
			}
		}

		public unsafe EntryWrapper? this[string name]
		{
			get
			{
				if (!TryGetIndex(name, out var index))
				{
					return null;
				}
				ConfigEntry* configEntry = ((ConfigBase)configBase).ConfigEntry;
				configEntry = (ConfigEntry*)((byte*)configEntry + index * Unsafe.SizeOf<ConfigEntry>());
				if (!((CStringPointer)(&((ConfigEntry)configEntry).Name)).HasValue)
				{
					return null;
				}
				return new EntryWrapper(configEntry, name);
			}
		}

		public unsafe GameConfigSection(ConfigBase* configBase, string[]? ignoredNames = null)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			this.configBase = configBase;
			if (ignoredNames != null)
			{
				this.ignoredNames = ignoredNames;
			}
			ConfigEntry* ptr = ((ConfigBase)configBase).ConfigEntry;
			uint num = 0u;
			while (num < ((ConfigBase)configBase).ConfigCount)
			{
				if (((CStringPointer)(&((ConfigEntry)ptr).Name)).HasValue)
				{
					string key = MemoryHelper.ReadStringNullTerminated((IntPtr)new IntPtr(CStringPointer.op_Implicit(((ConfigEntry)ptr).Name)));
					if (!indexMap.ContainsKey(key))
					{
						indexMap.Add(key, num);
					}
				}
				num++;
				ptr = (ConfigEntry*)((byte*)ptr + Unsafe.SizeOf<ConfigEntry>());
			}
		}

		public unsafe bool TryGetEntry(string name, out EntryWrapper? result, StringComparison? nameComparison = null)
		{
			result = null;
			if (!TryGetIndex(name, out var index, nameComparison))
			{
				return false;
			}
			ConfigEntry* configEntry = ((ConfigBase)configBase).ConfigEntry;
			configEntry = (ConfigEntry*)((byte*)configEntry + index * Unsafe.SizeOf<ConfigEntry>());
			if (!((CStringPointer)(&((ConfigEntry)configEntry).Name)).HasValue)
			{
				return false;
			}
			result = new EntryWrapper(configEntry, name);
			return true;
		}

		public unsafe bool TryGetName(uint index, out string? name)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			name = null;
			if (index >= ((ConfigBase)configBase).ConfigCount)
			{
				return false;
			}
			if (nameMap.TryGetValue(index, out name))
			{
				return name != null;
			}
			ConfigEntry* configEntry = ((ConfigBase)configBase).ConfigEntry;
			configEntry = (ConfigEntry*)((byte*)configEntry + index * Unsafe.SizeOf<ConfigEntry>());
			if (!((CStringPointer)(&((ConfigEntry)configEntry).Name)).HasValue)
			{
				return false;
			}
			name = MemoryHelper.ReadStringNullTerminated((IntPtr)new IntPtr(CStringPointer.op_Implicit(((ConfigEntry)configEntry).Name)));
			indexMap.TryAdd(name, index);
			nameMap.TryAdd(index, name);
			return true;
		}

		public unsafe bool TryGetIndex(string name, out uint index, StringComparison? stringComparison = null)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (indexMap.TryGetValue(name, out index))
			{
				return true;
			}
			ConfigEntry* ptr = ((ConfigBase)configBase).ConfigEntry;
			uint num = 0u;
			while (num < ((ConfigBase)configBase).ConfigCount)
			{
				if (((CStringPointer)(&((ConfigEntry)ptr).Name)).HasValue && MemoryHelper.ReadStringNullTerminated((IntPtr)new IntPtr(CStringPointer.op_Implicit(((ConfigEntry)ptr).Name))).Equals(name))
				{
					indexMap.TryAdd(name, num);
					nameMap.TryAdd(num, name);
					index = num;
					return true;
				}
				num++;
				ptr = (ConfigEntry*)((byte*)ptr + Unsafe.SizeOf<ConfigEntry>());
			}
			index = 0u;
			return false;
		}

		private unsafe bool TryGetEntry(uint index, out ConfigEntry* entry)
		{
			entry = null;
			if (((ConfigBase)configBase).ConfigEntry == null || index >= ((ConfigBase)configBase).ConfigCount)
			{
				return false;
			}
			entry = ((ConfigBase)configBase).ConfigEntry;
			entry = (ConfigEntry*)((byte*)entry + index * Unsafe.SizeOf<ConfigEntry>());
			return true;
		}

		public unsafe bool TryGetBool(string name, out bool value)
		{
			value = false;
			if (!TryGetIndex(name, out var index))
			{
				return false;
			}
			if (!TryGetEntry(index, out var entry))
			{
				return false;
			}
			value = ((ConfigValue)(&((ConfigEntry)entry).Value)).UInt != 0;
			return true;
		}

		public bool GetBool(string name)
		{
			if (!TryGetBool(name, out var value))
			{
				throw new Exception("Failed to get Bool '" + name + "'");
			}
			return value;
		}

		public unsafe void Set(string name, bool value)
		{
			if (TryGetIndex(name, out var index) && TryGetEntry(index, out var entry))
			{
				((ConfigEntry)entry).SetValue(value ? 1u : 0u, 1u);
			}
		}

		public unsafe bool TryGetUInt(string name, out uint value)
		{
			value = 0u;
			if (!TryGetIndex(name, out var index))
			{
				return false;
			}
			if (!TryGetEntry(index, out var entry))
			{
				return false;
			}
			value = ((ConfigValue)(&((ConfigEntry)entry).Value)).UInt;
			return true;
		}

		public uint GetUInt(string name)
		{
			if (!TryGetUInt(name, out var value))
			{
				throw new Exception("Failed to get UInt '" + name + "'");
			}
			return value;
		}

		public unsafe void Set(string name, uint value)
		{
			if (TryGetIndex(name, out var index) && TryGetEntry(index, out var entry))
			{
				((ConfigEntry)entry).SetValue(value, 1u);
			}
		}

		public unsafe bool TryGetFloat(string name, out float value)
		{
			value = 0f;
			if (!TryGetIndex(name, out var index))
			{
				return false;
			}
			if (!TryGetEntry(index, out var entry))
			{
				return false;
			}
			value = ((ConfigValue)(&((ConfigEntry)entry).Value)).Float;
			return true;
		}

		public float GetFloat(string name)
		{
			if (!TryGetFloat(name, out var value))
			{
				throw new Exception("Failed to get Float '" + name + "'");
			}
			return value;
		}

		public unsafe void Set(string name, float value)
		{
			if (TryGetIndex(name, out var index) && TryGetEntry(index, out var entry))
			{
				((ConfigEntry)entry).SetValue(value);
			}
		}

		public unsafe bool TryGetString(string name, out string value)
		{
			value = string.Empty;
			if (!TryGetIndex(name, out var index))
			{
				return false;
			}
			if (!TryGetEntry(index, out var entry))
			{
				return false;
			}
			if (((ConfigEntry)entry).Type != 4)
			{
				return false;
			}
			if (((ConfigValue)(&((ConfigEntry)entry).Value)).String == null)
			{
				return false;
			}
			value = ((object)(*(Utf8String*)((ConfigValue)(&((ConfigEntry)entry).Value)).String)/*cast due to constrained. prefix*/).ToString();
			return true;
		}

		public string GetString(string name)
		{
			if (!TryGetString(name, out string value))
			{
				throw new Exception("Failed to get String '" + name + "'");
			}
			return value;
		}

		public unsafe void Set(string name, string value)
		{
			if (TryGetIndex(name, out var index) && TryGetEntry(index, out var entry))
			{
				((ConfigEntry)entry).SetValue(value);
			}
		}
	}

	public static GameConfigSection System;

	public static GameConfigSection UiConfig;

	public static GameConfigSection UiControl;

	unsafe static GameConfig()
	{
		System = new GameConfigSection(&((SystemConfig)(&((SystemConfig)(&((Framework)Framework.Instance()).SystemConfig)).SystemConfigBase)).ConfigBase, new string[1] { "PadMode" });
		UiConfig = new GameConfigSection(&((SystemConfig)(&((SystemConfig)(&((Framework)Framework.Instance()).SystemConfig)).SystemConfigBase)).UiConfig);
		UiControl = new GameConfigSection(&((SystemConfig)(&((SystemConfig)(&((Framework)Framework.Instance()).SystemConfig)).SystemConfigBase)).UiControlConfig);
	}
}

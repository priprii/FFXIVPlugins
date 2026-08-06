using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Newtonsoft.Json;
using PvPyon.Api.Icons;

namespace PvPyon;

public class Tag
{
	[JsonProperty("Parent")]
	private Tag? m_Parent;

	[JsonIgnore]
	private Dictionary<string, IInheritable>? m_Inheritables;

	public InheritableValue<bool> IsSelected = new InheritableValue<bool>(value: false)
	{
		Behavior = InheritableBehavior.Enabled
	};

	public InheritableValue<bool> IsExpanded = new InheritableValue<bool>(value: false)
	{
		Behavior = InheritableBehavior.Enabled
	};

	public InheritableReference<string> GameObjectNamesToApplyTo = new InheritableReference<string>("");

	public InheritableValue<Guid> CustomId = new InheritableValue<Guid>(Guid.Empty);

	[InheritableCategory("IconCategory")]
	public InheritableValue<BitmapFontIcon> Icon = new InheritableValue<BitmapFontIcon>((BitmapFontIcon)66);

	[InheritableCategory("IconCategory")]
	public InheritableValue<bool> IsRoleIconVisibleInChat = new InheritableValue<bool>(value: false);

	[InheritableCategory("IconCategory")]
	public InheritableValue<bool> IsRoleIconVisibleInNameplates = new InheritableValue<bool>(value: false);

	[InheritableCategory("IconCategory")]
	public InheritableValue<bool> IsJobIconVisibleInNameplates = new InheritableValue<bool>(value: false);

	[InheritableCategory("IconCategory")]
	public InheritableValue<JobIconSetName> JobIconSet = new InheritableValue<JobIconSetName>(JobIconSetName.Framed);

	[InheritableCategory("TextCategory")]
	public InheritableReference<string> Text = new InheritableReference<string>("");

	[InheritableCategory("TextCategory")]
	public InheritableValue<ushort> TextColor = new InheritableValue<ushort>(6);

	[InheritableCategory("TextCategory")]
	public InheritableValue<ushort> TextGlowColor = new InheritableValue<ushort>(6);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextItalic = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextVisibleInChat = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextVisibleInNameplates = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextColorAppliedToChatName = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextColorAppliedToNameplateName = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextColorAppliedToNameplateTitle = new InheritableValue<bool>(value: false);

	[InheritableCategory("TextCategory")]
	public InheritableValue<bool> IsTextColorAppliedToNameplateFreeCompany = new InheritableValue<bool>(value: false);

	[InheritableCategory("PositionCategory")]
	public InheritableValue<TagPosition> TagPositionInChat = new InheritableValue<TagPosition>(TagPosition.Before);

	[InheritableCategory("PositionCategory")]
	public InheritableValue<bool> InsertBehindNumberPrefixInChat = new InheritableValue<bool>(value: true);

	[InheritableCategory("PositionCategory")]
	public InheritableValue<TagPosition> TagPositionInNameplates = new InheritableValue<TagPosition>(TagPosition.Before);

	[InheritableCategory("PositionCategory")]
	public InheritableValue<NameplateElement> TagTargetInNameplates = new InheritableValue<NameplateElement>(NameplateElement.Name);

	[InheritableCategory("ActivityCategory")]
	public InheritableValue<bool> IsVisibleInPveDuties = new InheritableValue<bool>(value: false);

	[InheritableCategory("ActivityCategory")]
	public InheritableValue<bool> IsVisibleInPvpDuties = new InheritableValue<bool>(value: false);

	[InheritableCategory("ActivityCategory")]
	public InheritableValue<bool> IsVisibleInOverworld = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForSelf = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForFriendPlayers = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForPartyPlayers = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForAlliancePlayers = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForEnemyPlayers = new InheritableValue<bool>(value: false);

	[InheritableCategory("PlayerCategory")]
	public InheritableValue<bool> IsVisibleForOtherPlayers = new InheritableValue<bool>(value: false);

	[InheritableCategory("ChatFeatureCategory")]
	public InheritableReference<List<XivChatType>> TargetChatTypes = new InheritableReference<List<XivChatType>>(new List<XivChatType>(Enum.GetValues<XivChatType>()));

	[InheritableCategory("ChatFeatureCategory")]
	public InheritableValue<bool> TargetChatTypesIncludeUndefined = new InheritableValue<bool>(value: true);

	private Tag? m_Defaults;

	private static readonly Dictionary<string, string> ObsulteInheritableStringMap = new Dictionary<string, string>
	{
		{ "IsIconVisibleInChat", "IsRoleIconVisibleInChat" },
		{ "IsIconVisibleInNameplate", "IsRoleIconVisibleInNameplates" },
		{ "IsIconVisibleInNameplates", "IsRoleIconVisibleInNameplates" }
	};

	public IPluginString Name { get; init; }

	[JsonIgnore]
	public Tag? Parent
	{
		get
		{
			return m_Parent;
		}
		set
		{
			if (m_Parent == value)
			{
				return;
			}
			if (m_Parent != null && m_Parent.Children.Contains(this))
			{
				m_Parent.Children.Remove(this);
			}
			m_Parent = value;
			if (m_Parent == null)
			{
				return;
			}
			m_Parent.Children.Add(this);
			foreach (KeyValuePair<string, IInheritable> inheritable in Inheritables)
			{
				inheritable.Deconstruct(out var key, out var value2);
				string key2 = key;
				value2.Parent = m_Parent.Inheritables[key2];
			}
		}
	}

	public List<Tag> Children { get; } = new List<Tag>();

	[JsonIgnore]
	public IEnumerable<Tag> Descendents
	{
		get
		{
			IEnumerable<Tag> enumerable = Children.Prepend(this);
			foreach (Tag child in Children)
			{
				enumerable = enumerable.Union(child.Descendents);
			}
			return enumerable.Distinct();
		}
	}

	[JsonIgnore]
	public Dictionary<string, IInheritable> Inheritables
	{
		get
		{
			if (m_Inheritables == null)
			{
				m_Inheritables = new Dictionary<string, IInheritable>();
				foreach (FieldInfo item in from field in GetType().GetFields()
					where typeof(IInheritable).IsAssignableFrom(field.FieldType)
					select field)
				{
					if (item.GetValue(this) is IInheritable value)
					{
						Inheritables[item.Name] = value;
					}
				}
			}
			return m_Inheritables;
		}
	}

	[JsonProperty]
	[Obsolete]
	private InheritableValue<bool> IsIconVisibleInChat
	{
		set
		{
			IsRoleIconVisibleInChat = value;
		}
	}

	[JsonProperty]
	[Obsolete]
	private InheritableValue<bool> IsIconVisibleInNameplate
	{
		set
		{
			IsRoleIconVisibleInNameplates = value;
		}
	}

	[JsonIgnore]
	public string[] IdentitiesToAddTo
	{
		get
		{
			if (GameObjectNamesToApplyTo == null || GameObjectNamesToApplyTo.InheritedValue == null)
			{
				return new string[0];
			}
			return (from item in GameObjectNamesToApplyTo.InheritedValue.Split(new char[2] { ';', ',' })
				where !string.IsNullOrEmpty(item)
				select item.Trim()).ToArray();
		}
	}

	[JsonIgnore]
	public bool HasDefaults => m_Defaults != null;

	public Tag()
	{
		Name = new LiteralPluginString("");
		m_Defaults = null;
	}

	public Tag(IPluginString name)
	{
		Name = name;
		m_Defaults = null;
	}

	public Tag(IPluginString name, Tag defaults)
	{
		Name = name;
		m_Defaults = defaults;
		SetChanges(defaults.GetChanges());
	}

	public Dictionary<string, InheritableData> GetChanges(Dictionary<string, InheritableData>? defaultChanges = null)
	{
		Dictionary<string, InheritableData> dictionary = new Dictionary<string, InheritableData>();
		foreach (var (key, inheritable2) in Inheritables)
		{
			if (defaultChanges != null && defaultChanges.TryGetValue(key, out var value))
			{
				InheritableData data = inheritable2.GetData();
				if (data.Behavior != value.Behavior || !EqualsInheritableData(data, value))
				{
					dictionary[key] = inheritable2.GetData();
				}
			}
			else if (inheritable2.Behavior != InheritableBehavior.Inherit)
			{
				dictionary[key] = inheritable2.GetData();
			}
		}
		return dictionary;
	}

	private static bool EqualsInheritableData(InheritableData data1, InheritableData data2)
	{
		if (data1.Value is List<XivChatType>)
		{
			return EqualsInheritableDataListXivChatType<XivChatType>(data1, data2);
		}
		return data1.Value.Equals(data2.Value);
	}

	private static bool EqualsInheritableDataListXivChatType<TEnum>(InheritableData data1, InheritableData data2)
	{
		List<TEnum> list = data1.Value as List<TEnum>;
		List<TEnum> list2 = data2.Value as List<TEnum>;
		if (list == null || list2 == null || list.Count != list2.Count)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].Equals(list2[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static string FixObsuleteInheritableStringName(string name)
	{
		if (ObsulteInheritableStringMap.ContainsKey(name))
		{
			return ObsulteInheritableStringMap[name];
		}
		return name;
	}

	public void SetChanges(IEnumerable<KeyValuePair<string, InheritableData>> changes)
	{
		foreach (KeyValuePair<string, InheritableData> change in changes)
		{
			change.Deconstruct(out var key, out var value);
			string name = key;
			InheritableData data = value;
			string key2 = FixObsuleteInheritableStringName(name);
			Inheritables[key2].SetData(data);
		}
	}

	private Dictionary<string, InheritableData> GetAllAsChanges()
	{
		Dictionary<string, InheritableData> dictionary = new Dictionary<string, InheritableData>();
		foreach (KeyValuePair<string, IInheritable> inheritable2 in Inheritables)
		{
			inheritable2.Deconstruct(out var key, out var value);
			string key2 = key;
			IInheritable inheritable = value;
			dictionary[key2] = inheritable.GetData();
		}
		return dictionary;
	}

	public void SetDefaults()
	{
		if (m_Defaults != null)
		{
			SetChanges(from change in m_Defaults.GetAllAsChanges()
				where change.Key != "IsSelected" && change.Key != "IsExpanded"
				select change);
		}
	}
}

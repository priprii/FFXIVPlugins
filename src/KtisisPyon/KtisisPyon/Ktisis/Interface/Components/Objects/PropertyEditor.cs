using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Ktisis.Core;
using Ktisis.Core.Attributes;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Components.Objects;

[Transient]
public class PropertyEditor
{
	private class PropertyListBuilder : IPropertyListBuilder
	{
		public class PropertyHeader
		{
			public required string Name;

			public required Action Callback;

			public required int Priority;
		}

		private readonly List<PropertyHeader> _headers = new List<PropertyHeader>();

		public void Clear()
		{
			_headers.Clear();
		}

		public void AddHeader(string name, Action callback, int priority = int.MinValue)
		{
			_headers.Add(new PropertyHeader
			{
				Name = name,
				Callback = callback,
				Priority = ((priority == int.MinValue) ? _headers.Count : priority)
			});
		}

		public IReadOnlyList<PropertyHeader> Build()
		{
			_headers.Sort((PropertyHeader a, PropertyHeader b) => a.Priority - b.Priority);
			return _headers.AsReadOnly();
		}
	}

	private readonly DIBuilder _di;

	private readonly PropertyListBuilder _builder = new PropertyListBuilder();

	private readonly List<ObjectPropertyList> _editors = new List<ObjectPropertyList>();

	public PropertyEditor(DIBuilder di)
	{
		_di = di;
	}

	public void Prepare(IEditorContext ctx, GuiManager gui)
	{
		Create<ActorPropertyList>(new object[2] { ctx, gui }).Create<PosePropertyList>(new object[2] { ctx, gui }).Create<LightPropertyList>(new object[1] { ctx }).Create<OverlayPropertyList>(new object[1] { ctx })
			.Create<ImagePropertyList>(new object[1] { ctx })
			.Create<WeaponPropertyList>(Array.Empty<object>());
	}

	private PropertyEditor Create<T>(params object[] parameters) where T : ObjectPropertyList
	{
		Ktisis.Log.Verbose("Creating property editor: " + typeof(T).Name);
		_editors.Add(_di.Create<T>(parameters));
		return this;
	}

	public void Draw(SceneEntity entity)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		_builder.Clear();
		foreach (ObjectPropertyList editor in _editors)
		{
			editor.Invoke(_builder, entity);
		}
		foreach (PropertyListBuilder.PropertyHeader item in _builder.Build())
		{
			if (ImGui.CollapsingHeader(ImU8String.op_Implicit(item.Name), (ImGuiTreeNodeFlags)0))
			{
				try
				{
					item.Callback();
				}
				catch (Exception ex)
				{
					Ktisis.Log.Error("Error on '" + item.Name + "':\n" + ex.Message);
					ImGui.Text(ImU8String.op_Implicit("Encountered a UI error!\nPlease submit a bug report."));
				}
				ImGui.Spacing();
			}
		}
	}
}

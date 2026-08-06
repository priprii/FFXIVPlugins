using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Pose2D;
using Ktisis.Editor.Selection;
using Ktisis.Interface.Components.Posing.Types;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Interface.Components.Posing;

public class PoseViewRenderer
{
	private class ViewFrame : IViewFrame
	{
		private readonly PoseViewRenderer _render;

		private readonly List<ViewData> Views = new List<ViewData>();

		public ViewFrame(PoseViewRenderer render)
		{
			_render = render;
		}

		public void DrawView(PoseViewEntry entry, float width = 1f, float height = 1f, IDictionary<string, string>? templates = null)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			string file = entry.Images.First();
			IDalamudTextureWrap wrapOrDefault = _render.GetTexture(file, entry).GetWrapOrDefault((IDalamudTextureWrap)null);
			if (wrapOrDefault != null)
			{
				Vector2 windowContentRegionMax = ImGui.GetWindowContentRegionMax();
				ref float x = ref windowContentRegionMax.X;
				float num = x;
				ImGuiStylePtr style = ImGui.GetStyle();
				x = num - ((ImGuiStylePtr)(ref style)).ItemSpacing.X * (float)(Views.Count + 1);
				Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
				float num2 = Math.Min(windowContentRegionMax.X * width / wrapOrDefault.Size.X, windowContentRegionMax.Y * height / wrapOrDefault.Size.Y);
				Vector2 vector = wrapOrDefault.Size * num2;
				ImGui.Image(wrapOrDefault.Handle, vector);
				Views.Add(new ViewData
				{
					Entry = entry,
					ScreenPos = cursorScreenPos,
					Size = vector,
					Templates = templates
				});
			}
		}

		public void DrawBones(EntityPose pose)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			bool flag = ImGui.IsWindowHovered();
			BoneNode boneNode = null;
			foreach (ViewData view in Views)
			{
				bool flag2 = flag && ImGui.IsMouseHoveringRect(view.ScreenPos, view.ScreenPos + view.Size);
				foreach (PoseViewBone bone in view.Entry.Bones)
				{
					string text = bone.Name;
					if (view.Templates != null)
					{
						foreach (KeyValuePair<string, string> template in view.Templates)
						{
							template.Deconstruct(out var key, out var value);
							string oldValue = key;
							string newValue = value;
							text = text.Replace(oldValue, newValue);
						}
					}
					BoneNode boneNode2 = pose.FindBoneByName(text);
					if (boneNode2 != null)
					{
						Vector2 vector = view.Size * bone.Position;
						Vector2 vector2 = view.ScreenPos + vector;
						float num = MathF.Max(MathF.Min(9f, view.Size.X * 0.04f), 6f);
						Vector2 vector3 = new Vector2(num, num);
						bool flag3 = flag2 && boneNode == null && ImGui.IsMouseHoveringRect(vector2 - vector3, vector2 + vector3);
						uint num2 = _render._cfg.GetEntityDisplay(boneNode2).Color;
						if (!flag3 && !boneNode2.IsSelected)
						{
							num2 = num2.SetAlpha(100);
						}
						((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(vector2, num, num2, 64);
						((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector2, num, 4278190080u, 64, flag3 ? 2f : 1.5f);
						if (flag3)
						{
							boneNode = boneNode2;
						}
					}
				}
			}
			if (boneNode != null)
			{
				ImDrawListPtr foregroundDrawList = ImGui.GetForegroundDrawList();
				Vector2 vector4 = new Vector2(5f, 5f);
				Vector2 vector5 = ImGui.GetMousePos() + new Vector2(20f, 0f);
				((ImDrawListPtr)(ref foregroundDrawList)).AddRectFilled(vector5 - vector4, vector5 + ImGui.CalcTextSize(ImU8String.op_Implicit(boneNode.Name), false, -1f) + vector4, 4278190080u, 5f);
				((ImDrawListPtr)(ref foregroundDrawList)).AddText(vector5, uint.MaxValue, ImU8String.op_Implicit(boneNode.Name));
				if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
				{
					SelectMode selectMode = GuiHelpers.GetSelectMode();
					boneNode.Select(selectMode);
				}
			}
		}
	}

	private record ViewData
	{
		public required PoseViewEntry Entry;

		public required Vector2 ScreenPos;

		public required Vector2 Size;

		public IDictionary<string, string>? Templates;

		[CompilerGenerated]
		[SetsRequiredMembers]
		protected ViewData(ViewData original)
		{
			Entry = original.Entry;
			ScreenPos = original.ScreenPos;
			Size = original.Size;
			Templates = original.Templates;
		}
	}

	private readonly Configuration _cfg;

	private readonly ITextureProvider _tex;

	private readonly Dictionary<string, ISharedImmediateTexture> Textures = new Dictionary<string, ISharedImmediateTexture>();

	public PoseViewRenderer(Configuration cfg, ITextureProvider tex)
	{
		_cfg = cfg;
		_tex = tex;
	}

	public IViewFrame StartFrame()
	{
		return new ViewFrame(this);
	}

	public IDictionary<string, string> BuildTemplate(ActorEntity actor)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (actor.TryGetEarIdAsChar(out var id))
		{
			dictionary.Add("$I", id.ToString());
		}
		return dictionary;
	}

	private ISharedImmediateTexture GetTexture(string file, PoseViewEntry entry)
	{
		if (Textures.TryGetValue(file, out ISharedImmediateTexture value))
		{
			return value;
		}
		string text = _cfg.PoseView.CustomPathFor(entry.Name);
		if (!string.IsNullOrEmpty(text))
		{
			value = _tex.GetFromFile(text);
			Textures.Add(file, value);
			return value;
		}
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string name = executingAssembly.GetName().Name;
		value = _tex.GetFromManifestResource(executingAssembly, name + ".Data.Images." + file);
		Textures.Add(file, value);
		return value;
	}
}

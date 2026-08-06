using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Interface.Widgets;

namespace Ktisis.Interface.Components.Environment;

[Transient]
public class SetTextureSelect
{
	public delegate string ResolvePathHandler(uint id);

	private class OptionsPopupResource : IDisposable
	{
		private readonly CancellationTokenSource Source = new CancellationTokenSource();

		private readonly List<Option> List = new List<Option>();

		public IEnumerable<Option> Get()
		{
			lock (List)
			{
				return List.ToList();
			}
		}

		public void Load(ITextureProvider tex, ResolvePathHandler resolve)
		{
			IEnumerable<Option> values = (from opt in (from i in Enumerable.Range(0, 1000)
					select (uint)i).Select(delegate(uint i)
				{
					string text = resolve(i);
					ISharedImmediateTexture fromGame = tex.GetFromGame(text);
					return (fromGame == null && i != 0) ? null : new Option
					{
						Value = i,
						Texture = fromGame
					};
				})
				where opt != null
				select opt).Cast<Option>();
			LoadAsync(values, Source.Token).ContinueWith(delegate(Task task)
			{
				if (task.Exception != null)
				{
					Ktisis.Log.Error(task.Exception.ToString());
				}
			});
		}

		private async Task LoadAsync(IEnumerable<Option> values, CancellationToken token)
		{
			await Task.Yield();
			Stopwatch t = new Stopwatch();
			t.Start();
			foreach (Option[] item in values.Chunk(5))
			{
				double totalMilliseconds = t.Elapsed.TotalMilliseconds;
				lock (List)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}
					List.AddRange(item);
					goto IL_017d;
				}
				IL_017d:
				await Task.Delay(Math.Min((int)totalMilliseconds, 100), token);
				t.Restart();
			}
			token.ThrowIfCancellationRequested();
		}

		public void Dispose()
		{
			lock (List)
			{
				Source.Cancel();
				List.Clear();
			}
			Source.Dispose();
		}
	}

	private class Option
	{
		public required uint Value;

		public required ISharedImmediateTexture? Texture;
	}

	private readonly ITextureProvider _texture;

	private static readonly Vector2 ButtonSize = new Vector2(48f, 48f);

	private static readonly Vector2 OptionSize = new Vector2(64f, 64f);

	private bool _opening;

	private OptionsPopupResource? Options;

	public SetTextureSelect(ITextureProvider texture)
	{
		_texture = texture;
	}

	public bool Draw(string name, ref uint value, ResolvePathHandler resolve)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(12, 1);
		((ImU8String)(ref val)).AppendLiteral("##TexSelect_");
		((ImU8String)(ref val)).AppendFormatted<string>(name);
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			bool flag = false;
			string text = resolve(value);
			ISharedImmediateTexture fromGame = _texture.GetFromGame(text);
			if (DrawButton(value, fromGame, ButtonSize))
			{
				OpenPopup(name, resolve);
			}
			flag |= DrawPopup(name, ref value);
			ImGui.SameLine();
			GroupDisposable val3 = ImRaii.Group();
			try
			{
				ImGui.Text(ImU8String.op_Implicit(name));
				ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - ImGui.GetCursorPosX());
				return flag | InputUInt.Draw("##" + name, ref value);
			}
			finally
			{
				((GroupDisposable)(ref val3)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private bool DrawButton(uint value, ISharedImmediateTexture? image, Vector2 size)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, 0u, true);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)22, 1815755322u, true);
			try
			{
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)23, 2621061690u, true);
				try
				{
					StyleDisposable val4 = ImRaii.PushStyle((ImGuiStyleVar)10, Vector2.Zero, true);
					try
					{
						if (image == null)
						{
							ImU8String val5 = new ImU8String(0, 1);
							((ImU8String)(ref val5)).AppendFormatted<uint>(value, "D3");
							return ImGui.Button(val5, size);
						}
						return ImGui.ImageButton(image.GetWrapOrEmpty().Handle, size);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OpenPopup(string name, ResolvePathHandler resolve)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Options?.Dispose();
		Options = null;
		Options = new OptionsPopupResource();
		Options.Load(_texture, resolve);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(6, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(name);
		((ImU8String)(ref val)).AppendLiteral("_Popup");
		ImGui.OpenPopup(val, (ImGuiPopupFlags)0);
		_opening = true;
	}

	private bool DrawPopup(string name, ref uint value)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (Options == null)
		{
			return false;
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetNextWindowSizeConstraints(Vector2.Zero, new Vector2((OptionSize.X + ((ImGuiStylePtr)(ref style)).ItemSpacing.X) * 6f + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X + ((ImGuiStylePtr)(ref style)).ScrollbarSize, (OptionSize.Y + ((ImGuiStylePtr)(ref style)).ItemSpacing.Y) * 4f + ((ImGuiStylePtr)(ref style)).WindowPadding.Y));
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(6, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(name);
		((ImU8String)(ref val)).AppendLiteral("_Popup");
		PopupDisposable val2 = ImRaii.Popup(val, (ImGuiWindowFlags)64);
		try
		{
			if (!val2.Success)
			{
				if (_opening)
				{
					return false;
				}
				Options?.Dispose();
				Options = null;
				return false;
			}
			_opening = false;
			int num = 0;
			bool result = false;
			foreach (Option item in Options.Get())
			{
				if (num++ % 6 != 0 && num > 1)
				{
					ImGui.SameLine();
				}
				if (DrawButton(item.Value, item.Texture, OptionSize))
				{
					value = item.Value;
					result = true;
				}
			}
			return result;
		}
		finally
		{
			((PopupDisposable)(ref val2)).Dispose();
		}
	}
}

using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Editor.Characters.Types;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Editor.Characters.Handlers;

public class CustomizeEditor(ActorEntity actor) : ICustomizeEditor
{
	private class CustomizeBatch(CustomizeEditor editor) : ICustomizeBatch
	{
		private readonly Dictionary<CustomizeIndex, byte> Values = new Dictionary<CustomizeIndex, byte>();

		private uint? ModelId;

		public ICustomizeBatch SetCustomization(CustomizeIndex index, byte value)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Values[index] = value;
			return this;
		}

		public ICustomizeBatch SetIfNotNull(CustomizeIndex index, byte? value)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (!value.HasValue)
			{
				return this;
			}
			SetCustomization(index, value.Value);
			return this;
		}

		public ICustomizeBatch SetModelId(uint id)
		{
			ModelId = id;
			return this;
		}

		public void Apply()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			bool flag = false;
			foreach (var (index, value) in Values)
			{
				if (!editor.IsCurrentValue(index, value))
				{
					flag |= editor.SetCustomizeValue(index, value) && IsRedrawRequired(index);
				}
			}
			if (ModelId.HasValue)
			{
				flag |= editor.ModelIdDiffers(ModelId.Value);
				editor.SetModelId(ModelId.Value, redraw: false);
			}
			editor.UpdateCustomizeData(flag);
		}
	}

	private bool _isHetero;

	private bool _isHeteroGet;

	public unsafe byte GetCustomization(CustomizeIndex index)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		if (!actor.IsValid)
		{
			return 0;
		}
		if (TryGetFromState(index, out var value))
		{
			return value;
		}
		if (actor.CharacterBaseEx == null)
		{
			return 0;
		}
		return actor.CharacterBaseEx->Customize[(uint)(int)index];
	}

	private bool TryGetFromState(CustomizeIndex index, out byte value)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		value = byte.MaxValue;
		if (!actor.Appearance.Customize.IsSet(index))
		{
			return false;
		}
		value = actor.Appearance.Customize[index];
		return true;
	}

	public void SetCustomization(CustomizeIndex index, byte value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (SetCustomizeValue(index, value))
		{
			UpdateCustomizeData(IsRedrawRequired(index));
		}
	}

	private unsafe bool IsCurrentValue(CustomizeIndex index, byte value)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		bool flag = true;
		byte value2;
		bool num = TryGetFromState(index, out value2);
		if (num)
		{
			flag = flag && value == value2;
		}
		bool flag2 = actor.CharacterBaseEx != null;
		if (flag2)
		{
			flag &= value == actor.CharacterBaseEx->Customize[(uint)(int)index];
		}
		return (num || flag2) && flag;
	}

	private unsafe bool SetCustomizeValue(CustomizeIndex index, byte value)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected I4, but got Unknown
		if (!actor.IsValid)
		{
			return false;
		}
		if ((int)index == 22 && actor.IsViera())
		{
			if (value > 4)
			{
				value = (byte)((value != byte.MaxValue) ? 1u : 4u);
			}
			actor.Pose?.Refresh();
		}
		actor.Appearance.Customize[index] = value;
		CharacterBase* character = actor.GetCharacter();
		if (character == null)
		{
			return false;
		}
		if ((int)((CharacterBase)character).GetModelType() == 1)
		{
			Human* ptr = (Human*)character;
			((CustomizeData)(&((Human)ptr).Customize)).Data[(int)index] = value;
			return true;
		}
		return false;
	}

	private unsafe void UpdateCustomizeData(bool redraw)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Human* human = actor.GetHuman();
		if (!redraw && human != null)
		{
			DrawData val = new DrawData
			{
				CustomizeData = ((Human)human).Customize
			};
			redraw = !((Human)human).UpdateDrawData(&val, true);
		}
		if (redraw)
		{
			actor.Redraw();
		}
	}

	private static bool IsRedrawRequired(CustomizeIndex index)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)index <= 1 || index - 4 <= 1)
		{
			return true;
		}
		return false;
	}

	public void SetHeterochromia(bool enabled)
	{
		_isHetero = enabled;
		_isHeteroGet = true;
		if (!enabled)
		{
			byte customization = GetCustomization((CustomizeIndex)15);
			SetCustomization((CustomizeIndex)9, customization);
		}
	}

	public bool GetHeterochromia()
	{
		byte customization = GetCustomization((CustomizeIndex)9);
		byte customization2 = GetCustomization((CustomizeIndex)15);
		if (!_isHeteroGet)
		{
			_isHetero = customization != customization2;
			_isHeteroGet = true;
		}
		else
		{
			_isHetero |= customization != customization2;
		}
		return _isHetero;
	}

	public void SetEyeColor(byte value)
	{
		ICustomizeBatch customizeBatch = Prepare().SetCustomization((CustomizeIndex)9, value);
		if (!GetHeterochromia())
		{
			customizeBatch.SetCustomization((CustomizeIndex)15, value);
		}
		customizeBatch.Apply();
	}

	public unsafe uint GetModelId()
	{
		if (!actor.IsValid)
		{
			throw new Exception("Actor entity '" + actor.Name + "' is invalid.");
		}
		return actor.Appearance.ModelId ?? GetGameModel(actor.Character);
	}

	public void SetModelId(uint id, bool redraw = true)
	{
		if (!actor.IsValid)
		{
			throw new Exception("Actor entity '" + actor.Name + "' is invalid.");
		}
		redraw &= ModelIdDiffers(id);
		actor.Appearance.ModelId = id;
		if (redraw)
		{
			actor.Redraw();
		}
	}

	private unsafe bool ModelIdDiffers(uint id)
	{
		return id != (actor.Appearance.ModelId ?? GetGameModel(actor.Character));
	}

	private unsafe static uint GetGameModel(Character* chara)
	{
		if (chara == null)
		{
			throw new Exception("Character is null.");
		}
		return (uint)((ModelContainer)(&((Character)chara).ModelContainer)).ModelCharaId;
	}

	public unsafe void ApplyStateToGameObject()
	{
		if (actor.IsValid && actor.Character != null)
		{
			for (int i = 0; i < 26; i++)
			{
				byte customization = GetCustomization((CustomizeIndex)i);
				((CustomizeData)(&((DrawDataContainer)(&((Character)actor.Character).DrawData)).CustomizeData)).Data[i] = customization;
			}
		}
	}

	public ICustomizeBatch Prepare()
	{
		return new CustomizeBatch(this);
	}
}

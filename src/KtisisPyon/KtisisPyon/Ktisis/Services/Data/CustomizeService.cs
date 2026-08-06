using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Ktisis.Core.Attributes;
using Ktisis.Structs.Characters;

namespace Ktisis.Services.Data;

[Singleton]
public class CustomizeService
{
	private readonly IDataManager _data;

	public CustomizeService(IDataManager data)
	{
		_data = data;
	}

	public ushort CalcDataIdFor(Tribe tribe, Gender gender)
	{
		bool flag = gender == Gender.Masculine;
		int num;
		switch (tribe)
		{
		case Tribe.Midlander:
			num = (flag ? 101 : 201);
			break;
		case Tribe.Highlander:
			num = (flag ? 301 : 401);
			break;
		default:
		{
			Race race = (Race)(byte)Math.Floor(((decimal)(byte)tribe + 1m) / 2m);
			num = race switch
			{
				Race.Elezen => flag ? 501 : 601, 
				Race.Miqote => flag ? 701 : 801, 
				Race.Roegadyn => flag ? 901 : 1001, 
				Race.Lalafell => flag ? 1101 : 1201, 
				_ => 1301 + (int)(race - 6) * 200 + ((!flag) ? 100 : 0), 
			};
			break;
		}
		}
		return (ushort)num;
	}

	public bool IsFaceIdValidFor(ushort dataId, int faceId)
	{
		return _data.FileExists(ResolveFacePath(dataId, faceId));
	}

	public IEnumerable<byte> GetFaceTypes(ushort dataId)
	{
		for (int i = 0; i <= 255; i++)
		{
			if (IsFaceIdValidFor(dataId, i))
			{
				yield return (byte)i;
			}
		}
	}

	public byte FindBestFaceTypeFor(ushort dataId, byte current)
	{
		bool flag = false;
		for (int i = 0; i < 255; i++)
		{
			bool flag2 = IsFaceIdValidFor(dataId, i);
			if (!flag2 && i < 90)
			{
				flag2 |= IsFaceIdValidFor(dataId, i + 100);
			}
			if (flag2)
			{
				if (!flag)
				{
					flag = true;
					if (i > current)
					{
						return (byte)i;
					}
				}
			}
			else if (flag)
			{
				return (byte)(i - 1);
			}
		}
		return current;
	}

	public bool IsHairIdValidFor(ushort dataId, int hairId)
	{
		return _data.FileExists(ResolveHairPath(dataId, hairId));
	}

	public IEnumerable<byte> GetHairTypes(ushort dataId)
	{
		for (int i = 0; i <= 255; i++)
		{
			if (IsHairIdValidFor(dataId, i))
			{
				yield return (byte)i;
			}
		}
	}

	private static string ResolveFacePath(ushort dataId, int faceId)
	{
		return string.Format("chara/human/c{0:D4}/obj/face/f{1:D4}/model/c{0:D4}f{1:D4}_fac.mdl", dataId, faceId);
	}

	private static string ResolveHairPath(ushort dataId, int hairId)
	{
		return string.Format("chara/human/c{0:D4}/obj/hair/h{1:D4}/model/c{0:D4}h{1:D4}_hir.mdl", dataId, hairId);
	}
}

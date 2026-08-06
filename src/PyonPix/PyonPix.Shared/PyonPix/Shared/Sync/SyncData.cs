using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto;

namespace PyonPix.Shared.Sync;

public static class SyncData
{
	public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		Converters = { (JsonConverter)new JsonStringEnumConverter() }
	};

	public static byte[] CreateMessageBuffer(MessageType type, object? data)
	{
		string s = JsonSerializer.Serialize(new SocketMessage(type, (data == null) ? ((JsonElement?)null) : new JsonElement?(JsonSerializer.SerializeToElement(data, JsonOptions))), JsonOptions);
		return Encoding.UTF8.GetBytes(s);
	}

	public static bool TryGetMessage(string? json, out SocketMessage message)
	{
		message = null;
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		try
		{
			message = JsonSerializer.Deserialize<SocketMessage>(json, JsonOptions);
			return message != null;
		}
		catch
		{
			return false;
		}
	}

	public static bool TryGetObject<T>(JsonElement? data, out T dto)
	{
		dto = default(T);
		if (!data.HasValue)
		{
			return false;
		}
		try
		{
			dto = data.Value.Deserialize<T>(JsonOptions);
			return dto != null;
		}
		catch
		{
			return false;
		}
	}

	public static bool TryGetSyncedPixUpdate(JsonElement? data, out BaseSyncedPixUpdate update)
	{
		update = null;
		if (!data.HasValue)
		{
			return false;
		}
		try
		{
			if (!data.Value.TryGetProperty("PixId", out var value))
			{
				return false;
			}
			if (!data.Value.TryGetProperty("UpdateType", out var value2))
			{
				return false;
			}
			if (string.IsNullOrWhiteSpace(value.GetString()))
			{
				return false;
			}
			if (!Enum.TryParse<PixUpdateType>(value2.GetString(), ignoreCase: true, out var result))
			{
				return false;
			}
			BaseSyncedPixUpdate baseSyncedPixUpdate;
			switch (result)
			{
			case PixUpdateType.All:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdate>(JsonOptions);
				break;
			case PixUpdateType.Uri:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateUri>(JsonOptions);
				break;
			case PixUpdateType.InfoProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateInfoProperties>(JsonOptions);
				break;
			case PixUpdateType.BrowserProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateBrowserProperties>(JsonOptions);
				break;
			case PixUpdateType.MediaState:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateMediaState>(JsonOptions);
				break;
			case PixUpdateType.RendererTransform:
			case PixUpdateType.RendererProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateRendererProperties>(JsonOptions);
				break;
			case PixUpdateType.LightTransform:
			case PixUpdateType.LightProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateLightProperties>(JsonOptions);
				break;
			case PixUpdateType.AudioProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateAudioProperties>(JsonOptions);
				break;
			case PixUpdateType.SyncProperties:
				baseSyncedPixUpdate = data.Value.Deserialize<SyncedPixUpdateSyncProperties>(JsonOptions);
				break;
			default:
				baseSyncedPixUpdate = null;
				break;
			}
			BaseSyncedPixUpdate baseSyncedPixUpdate2 = baseSyncedPixUpdate;
			if (baseSyncedPixUpdate2 == null)
			{
				return false;
			}
			update = baseSyncedPixUpdate2;
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{ex.Source} Failed: {ex}");
			return false;
		}
	}
}

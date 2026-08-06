using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PyonPix.Config;
using PyonPix.Structs.Browser;
using PyonPix.Ui;

namespace PyonPix.Services.Core;

public class ExtensionsService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private HttpClient Client;

	private readonly object _lock = new object();

	public bool IsOperating;

	private string UnpackedExtensionRootPath => Path.Combine(Config.GetConfigPath(), "Data", "Extensions");

	public event Action<string[]>? OnAutoCompleteResult;

	public event Action<List<ExtensionProductDetails>>? OnSearchResult;

	public event Action<string, string>? InstallExtensionRequest;

	public event Action<string, string>? UninstallExtensionRequest;

	public event Action<string, string>? EnableExtensionRequest;

	public event Action<string, string>? DisableExtensionRequest;

	public override Task Initialize()
	{
		Directory.CreateDirectory(UnpackedExtensionRootPath);
		ResolveUnknownExtensions();
		HttpClientHandler handler = new HttpClientHandler
		{
			AllowAutoRedirect = true
		};
		Client = new HttpClient(handler);
		Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0");
		return Task.CompletedTask;
	}

	private string GetDownloadPath(string extensionId)
	{
		return Path.Combine(UnpackedExtensionRootPath, "_" + extensionId);
	}

	private string GetInstallPath(string extensionId)
	{
		return Path.Combine(UnpackedExtensionRootPath, extensionId);
	}

	public void ResolveUnknownExtensions()
	{
		if (!Directory.Exists(UnpackedExtensionRootPath))
		{
			return;
		}
		foreach (string item in Directory.EnumerateDirectories(UnpackedExtensionRootPath))
		{
			string text = Path.GetFileName(item) ?? string.Empty;
			if (GetExtension(text) != null)
			{
				continue;
			}
			string path = Path.Combine(item, "manifest.json");
			if (!File.Exists(path))
			{
				continue;
			}
			try
			{
				string text2 = File.ReadAllText(path);
				if (!string.IsNullOrWhiteSpace(text2))
				{
					ExtensionManifest extensionManifest = JsonSerializer.Deserialize<ExtensionManifest>(text2);
					if (extensionManifest != null)
					{
						Extension e = new Extension
						{
							CrxId = text,
							Name = (extensionManifest.Name ?? string.Empty),
							Developer = (extensionManifest.Developer ?? string.Empty),
							Version = (extensionManifest.Version ?? string.Empty),
							LastUpdated = DateTime.UtcNow,
							IsDownloaded = true
						};
						AddOrUpdateConfigExtension(e);
					}
				}
			}
			catch
			{
			}
		}
	}

	public Extension? GetExtension(string extensionId)
	{
		if (string.IsNullOrWhiteSpace(extensionId))
		{
			return null;
		}
		lock (_lock)
		{
			Config.Extensions.TryGetValue(extensionId, out Extension value);
			return value;
		}
	}

	public string[] GetExtensionsToInstall()
	{
		lock (_lock)
		{
			string[] array = (from kv in Config.Extensions
				where kv.Value.IsInstalled
				select kv.Key).ToArray();
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (Config.Extensions.TryGetValue(key, out Extension value))
				{
					value.IsInstalled = true;
				}
			}
			Config.Save();
			return array ?? Array.Empty<string>();
		}
	}

	private Extension EnsureExtension(string crxId)
	{
		lock (_lock)
		{
			if (!Config.Extensions.TryGetValue(crxId, out Extension value))
			{
				value = new Extension
				{
					CrxId = crxId
				};
				Config.Extensions[crxId] = value;
			}
			return value;
		}
	}

	public void AddOrUpdateConfigExtension(Extension e)
	{
		lock (_lock)
		{
			Config.Extensions[e.CrxId] = e;
			Config.Save();
		}
	}

	public void RemoveConfigExtension(string extensionId)
	{
		lock (_lock)
		{
			if (Config.Extensions.Remove(extensionId))
			{
				Config.Save();
			}
		}
	}

	public void InstallExtension(string crxId)
	{
		Extension extension = GetExtension(crxId);
		if (extension != null)
		{
			IsOperating = true;
			this.InstallExtensionRequest?.Invoke(crxId, extension.Name);
			extension.IsInstalled = true;
			extension.IsEnabled = true;
			AddOrUpdateConfigExtension(extension);
		}
	}

	public void UninstallExtension(string crxId)
	{
		Extension extension = GetExtension(crxId);
		if (extension != null)
		{
			IsOperating = true;
			this.UninstallExtensionRequest?.Invoke(crxId, extension.Name);
			extension.IsInstalled = false;
			extension.IsEnabled = false;
			AddOrUpdateConfigExtension(extension);
		}
	}

	public void EnableExtension(string crxId)
	{
		Extension extension = GetExtension(crxId);
		if (extension != null)
		{
			IsOperating = true;
			this.EnableExtensionRequest?.Invoke(crxId, extension.Name);
			extension.IsEnabled = true;
			AddOrUpdateConfigExtension(extension);
		}
	}

	public void DisableExtension(string crxId)
	{
		Extension extension = GetExtension(crxId);
		if (extension != null)
		{
			IsOperating = true;
			this.DisableExtensionRequest?.Invoke(crxId, extension.Name);
			extension.IsEnabled = false;
			AddOrUpdateConfigExtension(extension);
		}
	}

	public void RemoveExtension(string crxId)
	{
		if (GetExtension(crxId) != null)
		{
			string installPath = GetInstallPath(crxId);
			if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
			{
				RemoveConfigExtension(crxId);
				return;
			}
			IsOperating = true;
			TryDeleteDirectory(installPath);
			RemoveConfigExtension(crxId);
			IsOperating = false;
		}
	}

	public async Task CheckUpdateAllAsync(bool autoUpdate, CancellationToken ct = default(CancellationToken))
	{
		if (IsOperating || Config.Extensions.Count == 0)
		{
			return;
		}
		IsOperating = true;
		foreach (KeyValuePair<string, Extension> extension in Config.Extensions)
		{
			await CheckUpdateAsync(extension.Key, autoUpdate, ct);
		}
		IsOperating = false;
	}

	private async Task CheckUpdateAsync(string crxId, bool autoUpdate, CancellationToken ct = default(CancellationToken))
	{
		Extension ext = GetExtension(crxId);
		if (ext == null)
		{
			return;
		}
		if (!ext.IsUpdateAvailable)
		{
			ExtensionProductDetails extensionProductDetails = await GetProductDetailsAsync(crxId, ct);
			if (extensionProductDetails == null || string.Equals(ext.Version, extensionProductDetails.Version, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			ext.IsUpdateAvailable = true;
		}
		if (autoUpdate)
		{
			await UpdateAsync(crxId, ct);
		}
		else
		{
			AddOrUpdateConfigExtension(ext);
		}
	}

	public async Task UpdateAsync(string crxId, CancellationToken ct = default(CancellationToken))
	{
		await DownloadAndExtractCrxAsync(crxId, ct).ConfigureAwait(continueOnCapturedContext: false);
		Extension extension = GetExtension(crxId);
		if (extension != null && extension.IsInstalled)
		{
			IsOperating = true;
			this.InstallExtensionRequest?.Invoke(crxId, extension.Name);
			extension.IsEnabled = true;
		}
		AddOrUpdateConfigExtension(extension ?? EnsureExtension(crxId));
	}

	public async Task<ExtensionAutoCompleteResult?> AutoCompleteAsync(string query, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(query) || query.Length <= 2)
		{
			this.OnAutoCompleteResult?.Invoke(Array.Empty<string>());
			return null;
		}
		string requestUri = "https://microsoftedge.microsoft.com/edgestorewebautocomplete/v1/search?q=" + Uri.EscapeDataString(query);
		using HttpResponseMessage resp = await Client.GetAsync(requestUri, ct).ConfigureAwait(continueOnCapturedContext: false);
		resp.EnsureSuccessStatusCode();
		using Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		ExtensionAutoCompleteResult extensionAutoCompleteResult = await JsonSerializer.DeserializeAsync<ExtensionAutoCompleteResult>(stream, JsonSerializerOptions.Default, ct).ConfigureAwait(continueOnCapturedContext: false);
		this.OnAutoCompleteResult?.Invoke((extensionAutoCompleteResult == null || extensionAutoCompleteResult.AutoCompleteList == null) ? Array.Empty<string>() : extensionAutoCompleteResult.AutoCompleteList);
		return extensionAutoCompleteResult;
	}

	public async Task<ExtensionSearchResult[]> SearchAsync(string query, int page = 1, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(query) || query.Length <= 2)
		{
			this.OnSearchResult?.Invoke(new List<ExtensionProductDetails>());
			return Array.Empty<ExtensionSearchResult>();
		}
		string requestUri = $"https://microsoftedge.microsoft.com/addons/v4/getfilteredorderedsearch?filteredCategories=Edge-Extensions&filteredAddon=0&filterFeaturedAddons=false&filteredRating=0&sortBy=Relevance&pgNo={page}&Query={Uri.EscapeDataString(query)}";
		using HttpResponseMessage resp = await Client.GetAsync(requestUri, ct).ConfigureAwait(continueOnCapturedContext: false);
		resp.EnsureSuccessStatusCode();
		using Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		ExtensionSearchRoot root = await JsonSerializer.DeserializeAsync<ExtensionSearchRoot>(stream, JsonSerializerOptions.Default, ct).ConfigureAwait(continueOnCapturedContext: false);
		if (root?.Results == null)
		{
			this.OnSearchResult?.Invoke(new List<ExtensionProductDetails>());
			return Array.Empty<ExtensionSearchResult>();
		}
		List<ExtensionProductDetails> detailResults = new List<ExtensionProductDetails>();
		ExtensionSearchResult[] results = root.Results;
		foreach (ExtensionSearchResult extensionSearchResult in results)
		{
			ExtensionProductDetails extensionProductDetails = await GetProductDetailsAsync(extensionSearchResult.CrxId, ct);
			if (extensionProductDetails != null)
			{
				detailResults.Add(extensionProductDetails);
			}
		}
		this.OnSearchResult?.Invoke(detailResults);
		return root.Results;
	}

	private async Task<ExtensionProductDetails?> GetProductDetailsAsync(string crxId, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(crxId))
		{
			return null;
		}
		string requestUri = "https://microsoftedge.microsoft.com/addons/getproductdetailsbycrxid/" + Uri.EscapeDataString(crxId);
		using HttpResponseMessage resp = await Client.GetAsync(requestUri, ct).ConfigureAwait(continueOnCapturedContext: false);
		resp.EnsureSuccessStatusCode();
		using Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		ExtensionProductDetails extensionProductDetails = await JsonSerializer.DeserializeAsync<ExtensionProductDetails>(stream, JsonSerializerOptions.Default, ct).ConfigureAwait(continueOnCapturedContext: false);
		if (extensionProductDetails != null && extensionProductDetails.ShortDescription != null)
		{
			extensionProductDetails.ShortDescription = extensionProductDetails.ShortDescription.Replace("\n", " ");
		}
		return extensionProductDetails;
	}

	public async Task DownloadAndExtractCrxAsync(string crxId, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(crxId))
		{
			throw new ArgumentNullException("crxId");
		}
		IsOperating = true;
		string requestUri = "https://edge.microsoft.com/extensionwebstorebase/v1/crx?response=redirect&x=id%3D" + Uri.EscapeDataString(crxId) + "%26installsource%3Dondemand%26uc";
		using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUri);
		req.Headers.Accept.ParseAdd("*/*");
		HttpResponseMessage resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(continueOnCapturedContext: false);
		if (resp.StatusCode == HttpStatusCode.Found)
		{
			if (resp.Headers?.Location == null)
			{
				throw new InvalidOperationException("Redirect without Location header");
			}
			resp.Dispose();
			resp = await Client.GetAsync(resp.Headers.Location, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			try
			{
				resp.EnsureSuccessStatusCode();
			}
			catch (Exception ex)
			{
				Services.Log.Error(ex, "Extension Download Failed", Array.Empty<object>());
				IsOperating = false;
			}
		}
		using Stream crxStream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		string tempDir = GetDownloadPath(crxId);
		Directory.CreateDirectory(tempDir);
		try
		{
			await ExtractCrxToDirectoryAsync(crxStream, tempDir, ct).ConfigureAwait(continueOnCapturedContext: false);
			string installPath = GetInstallPath(crxId);
			if (Directory.Exists(installPath))
			{
				TryDeleteDirectory(installPath);
			}
			Directory.Move(tempDir, installPath);
			Extension ext = EnsureExtension(crxId);
			ext.IsDownloaded = true;
			ext.IsUpdateAvailable = false;
			ExtensionProductDetails extensionProductDetails = await GetProductDetailsAsync(crxId, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (extensionProductDetails != null)
			{
				ext.Name = extensionProductDetails.Name ?? ext.Name;
				ext.Description = extensionProductDetails.ShortDescription ?? ext.Description;
				ext.Developer = extensionProductDetails.DeveloperName ?? ext.Developer;
				ext.Version = extensionProductDetails.Version ?? ext.Version;
				ext.LastUpdated = ((!extensionProductDetails.LastUpdateDate.HasValue) ? ((DateTimeOffset)DateTime.UtcNow) : DateTimeOffset.FromUnixTimeSeconds((long)extensionProductDetails.LastUpdateDate.Value));
			}
			AddOrUpdateConfigExtension(ext);
		}
		finally
		{
			if (Directory.Exists(tempDir))
			{
				TryDeleteDirectory(tempDir);
			}
			resp?.Dispose();
			IsOperating = false;
		}
	}

	private static void TryDeleteDirectory(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch
		{
		}
	}

	private static async Task ExtractCrxToDirectoryAsync(Stream crxStream, string targetDirectory, CancellationToken ct)
	{
		using MemoryStream ms = new MemoryStream();
		await crxStream.CopyToAsync(ms, 81920, ct).ConfigureAwait(continueOnCapturedContext: false);
		byte[] array = ms.ToArray();
		int num = -1;
		for (int i = 0; i < array.Length - 4; i++)
		{
			if (array[i] == 80 && array[i + 1] == 75 && array[i + 2] == 3 && array[i + 3] == 4)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		using MemoryStream stream = new MemoryStream(array, num, array.Length - num);
		using ZipArchive source = new ZipArchive(stream, ZipArchiveMode.Read);
		source.ExtractToDirectory(targetDirectory);
	}

	public override Task Dispose()
	{
		Client.Dispose();
		return Task.CompletedTask;
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Structs.Data;
using PyonPix.Ui;

namespace PyonPix.Services.Core;

public class DataService(Configuration config, IServiceContext services, IWindowContext windows) : BaseService(config, services, windows)
{
	private readonly object _lock = new object();

	private List<UDF> UDFCache = new List<UDF>();

	private readonly ConcurrentDictionary<string, CancellationTokenSource> PendingRemovals = new ConcurrentDictionary<string, CancellationTokenSource>();

	private readonly int RemovalAttempts = 5;

	private readonly int RenameAttempts = 8;

	private readonly int InitialRemovalDelay = 200;

	private readonly int InitialRenameDelay = 250;

	private readonly int MaxRemovalDelay = 3000;

	private readonly int MaxRenameDelay = 3000;

	private PixService? PixService => Services.Get<PixService>();

	private string DataRootPath => Path.Combine(Config.GetConfigPath(), "Data", "Profiles");

	public event Action? OnUDFCacheUpdated;

	public event Action<string, UDFRemovalResult>? OnUDFRemovalCompleted;

	public override Task Initialize()
	{
		return Task.CompletedTask;
	}

	public List<UDF> GetUDFSnapshot()
	{
		lock (_lock)
		{
			return UDFCache.Select((UDF c) => new UDF
			{
				FolderName = c.FolderName,
				FolderPath = c.FolderPath,
				PixId = c.PixId,
				PixName = c.PixName,
				PersistentCache = c.PersistentCache,
				SizeBytes = c.SizeBytes,
				LastWriteUtc = c.LastWriteUtc,
				IsRemoving = c.IsRemoving,
				PixExists = c.PixExists
			}).ToList();
		}
	}

	public async Task RefreshCacheAsync(CancellationToken? token = null)
	{
		CancellationToken ct = token ?? CancellationToken.None;
		try
		{
			List<UDF> newList = new List<UDF>();
			if (!Directory.Exists(DataRootPath))
			{
				lock (_lock)
				{
					UDFCache = newList;
				}
				this.OnUDFCacheUpdated?.Invoke();
				return;
			}
			foreach (string item in Directory.EnumerateDirectories(DataRootPath))
			{
				ct.ThrowIfCancellationRequested();
				string text = Path.GetFileName(item) ?? "";
				if (!string.Equals(text, "PIX", StringComparison.OrdinalIgnoreCase))
				{
					IPix pix = PixService?.GetPix(text);
					PixVariant pixVariant = PixService?.GetVariant(pix);
					UDF uDF = new UDF
					{
						FolderName = text,
						FolderPath = item,
						PixId = text,
						PixName = pix?.Info.Name,
						PersistentCache = (pixVariant?.PersistentCache ?? false),
						SizeBytes = -1L,
						LastWriteUtc = null,
						IsRemoving = false,
						PixExists = (pix != null)
					};
					try
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(item);
						uDF.LastWriteUtc = directoryInfo.LastWriteTimeUtc;
					}
					catch
					{
						uDF.LastWriteUtc = null;
					}
					newList.Add(uDF);
				}
			}
			await Task.WhenAll(newList.Select((UDF e) => Task.Run(async delegate
			{
				try
				{
					UDF uDF4 = e;
					uDF4.SizeBytes = await ComputeDirectorySizeAsync(e.FolderPath, ct).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex4)
				{
					Services.Log.Error(ex4, "[Data] Size calc failed for " + e.FolderPath, Array.Empty<object>());
					e.SizeBytes = -1L;
				}
			}, ct)).ToArray()).ConfigureAwait(continueOnCapturedContext: false);
			lock (_lock)
			{
				foreach (UDF existing in UDFCache)
				{
					if (existing.IsRemoving)
					{
						UDF? uDF2 = newList.FirstOrDefault((UDF x) => x.FolderName == existing.FolderName);
						if (uDF2 != null)
						{
							uDF2.IsRemoving = true;
						}
					}
				}
				foreach (KeyValuePair<string, CancellationTokenSource> kv in PendingRemovals)
				{
					UDF? uDF3 = newList.FirstOrDefault((UDF x) => x.FolderName == kv.Key);
					if (uDF3 != null)
					{
						uDF3.IsRemoving = true;
					}
				}
				UDFCache = newList.OrderByDescending((UDF x) => x.LastWriteUtc).ToList();
			}
			this.OnUDFCacheUpdated?.Invoke();
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			Services.Log.Error(ex2, "[Data] RefreshCacheAsync failed", Array.Empty<object>());
		}
	}

	public void SetPersistent(string pixId, bool persistent)
	{
		if (string.IsNullOrWhiteSpace(pixId))
		{
			return;
		}
		PixVariant pixVariant = PixService?.GetVariant(pixId);
		if (pixVariant != null)
		{
			pixVariant.PersistentCache = persistent;
			Config.Save();
		}
		lock (_lock)
		{
			UDF? uDF = UDFCache.FirstOrDefault((UDF x) => x.PixId == pixId);
			if (uDF != null)
			{
				uDF.PersistentCache = persistent;
			}
		}
		this.OnUDFCacheUpdated?.Invoke();
	}

	public void RemoveUDF(string pixId)
	{
		if (string.IsNullOrWhiteSpace(pixId))
		{
			return;
		}
		lock (_lock)
		{
			UDF uDF = UDFCache.FirstOrDefault((UDF x) => x.PixId == pixId);
			if (uDF != null)
			{
				if (uDF != null)
				{
					uDF.IsRemoving = true;
				}
			}
			else
			{
				IPix pix = PixService?.GetPix(pixId);
				PixVariant pixVariant = PixService?.GetVariant(pix);
				UDFCache.Add(new UDF
				{
					FolderName = pixId,
					FolderPath = Path.Combine(DataRootPath, pixId),
					PixId = pixId,
					PixName = pix?.Info.Name,
					PersistentCache = (pixVariant?.PersistentCache ?? false),
					SizeBytes = -1L,
					LastWriteUtc = null,
					IsRemoving = true,
					PixExists = (pix != null)
				});
				UDFCache = UDFCache.OrderByDescending((UDF x) => x.LastWriteUtc).ToList();
			}
		}
		this.OnUDFCacheUpdated?.Invoke();
		CancellationTokenSource cts = new CancellationTokenSource();
		if (!PendingRemovals.TryAdd(pixId, cts))
		{
			try
			{
				cts.Dispose();
				return;
			}
			catch
			{
				return;
			}
		}
		Task.Run(async delegate
		{
			UDFRemovalResult result = UDFRemovalResult.Failed;
			try
			{
				result = ((!(await RemoveUDFWithRetriesAsync(pixId, cts.Token).ConfigureAwait(continueOnCapturedContext: false))) ? UDFRemovalResult.Failed : UDFRemovalResult.Success);
			}
			catch (OperationCanceledException)
			{
				result = UDFRemovalResult.Cancelled;
			}
			catch
			{
				result = UDFRemovalResult.Failed;
			}
			finally
			{
				if (PendingRemovals.TryRemove(pixId, out CancellationTokenSource value))
				{
					try
					{
						value.Dispose();
					}
					catch
					{
					}
				}
				lock (_lock)
				{
					UDF uDF2 = UDFCache.FirstOrDefault((UDF x) => x.PixId == pixId);
					if (result == UDFRemovalResult.Success)
					{
						if (uDF2 != null)
						{
							UDFCache.Remove(uDF2);
						}
					}
					else if (uDF2 != null)
					{
						uDF2.IsRemoving = false;
					}
				}
				this.OnUDFRemovalCompleted?.Invoke(pixId, result);
				this.OnUDFCacheUpdated?.Invoke();
			}
		}, cts.Token);
	}

	public void CancelPendingRemoval(string pixId)
	{
		if (string.IsNullOrWhiteSpace(pixId))
		{
			return;
		}
		if (PendingRemovals.TryRemove(pixId, out CancellationTokenSource value))
		{
			try
			{
				value.Cancel();
			}
			catch
			{
			}
			finally
			{
				try
				{
					value.Dispose();
				}
				catch
				{
				}
			}
		}
		lock (_lock)
		{
			UDF? uDF = UDFCache.FirstOrDefault((UDF x) => x.PixId == pixId);
			if (uDF != null)
			{
				uDF.IsRemoving = false;
			}
		}
		this.OnUDFCacheUpdated?.Invoke();
	}

	public async Task<bool> RenameUDFAsync(string fromPixId, string toPixId, CancellationToken? token = null)
	{
		if (string.IsNullOrWhiteSpace(fromPixId) || string.IsNullOrWhiteSpace(toPixId))
		{
			return false;
		}
		if (string.Equals(fromPixId, toPixId, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		CancellationToken ct = token ?? CancellationToken.None;
		string fromPath = Path.Combine(DataRootPath, fromPixId);
		string toPath = Path.Combine(DataRootPath, toPixId);
		Directory.CreateDirectory(DataRootPath);
		if (!Directory.Exists(fromPath))
		{
			return !Directory.Exists(toPath);
		}
		if (Directory.Exists(toPath))
		{
			Services.Log.Warning("[Data] Rename skipped, destination already exists: " + toPath, Array.Empty<object>());
			return false;
		}
		int attempt = 0;
		int delay = InitialRenameDelay;
		while (!ct.IsCancellationRequested)
		{
			attempt++;
			if (TryRenameUDF(fromPath, toPath))
			{
				lock (_lock)
				{
					UDF uDF = UDFCache.FirstOrDefault((UDF x) => x.PixId == fromPixId);
					if (uDF != null)
					{
						uDF.PixId = toPixId;
						uDF.FolderName = toPixId;
						uDF.FolderPath = toPath;
					}
				}
				this.OnUDFCacheUpdated?.Invoke();
				return true;
			}
			if (attempt >= RenameAttempts)
			{
				return false;
			}
			try
			{
				await Task.Delay(delay, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				ct.ThrowIfCancellationRequested();
			}
			delay = Math.Min(delay * 2, MaxRenameDelay);
		}
		ct.ThrowIfCancellationRequested();
		return false;
	}

	private static bool TryRenameUDF(string fromPath, string toPath)
	{
		try
		{
			if (!Directory.Exists(fromPath))
			{
				return true;
			}
			if (Directory.Exists(toPath))
			{
				return false;
			}
			Directory.Move(fromPath, toPath);
			return Directory.Exists(toPath);
		}
		catch
		{
			return false;
		}
	}

	private bool TryRemoveUDF(string folderPath)
	{
		try
		{
			if (!Directory.Exists(folderPath))
			{
				return true;
			}
			Directory.Delete(folderPath, recursive: true);
			return !Directory.Exists(folderPath);
		}
		catch
		{
			return false;
		}
	}

	private async Task<bool> RemoveUDFWithRetriesAsync(string pixId, CancellationToken ct)
	{
		if (ct.IsCancellationRequested)
		{
			ct.ThrowIfCancellationRequested();
		}
		string udfPath = Path.Combine(DataRootPath, pixId);
		int attempt = 0;
		int delay = InitialRemovalDelay;
		while (!ct.IsCancellationRequested)
		{
			attempt++;
			if (!Directory.Exists(udfPath))
			{
				return true;
			}
			if (TryRemoveUDF(udfPath))
			{
				return true;
			}
			if (attempt >= RemovalAttempts)
			{
				return false;
			}
			try
			{
				await Task.Delay(delay, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				ct.ThrowIfCancellationRequested();
			}
			delay = Math.Min(delay * 2, MaxRemovalDelay);
		}
		ct.ThrowIfCancellationRequested();
		return false;
	}

	private static async Task<long> ComputeDirectorySizeAsync(string path, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
		{
			return 0L;
		}
		return await Task.Run(delegate
		{
			long num = 0L;
			try
			{
				foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
				{
					ct.ThrowIfCancellationRequested();
					try
					{
						FileInfo fileInfo = new FileInfo(item);
						num += fileInfo.Length;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			return num;
		}, ct).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override Task Dispose()
	{
		foreach (KeyValuePair<string, CancellationTokenSource> pendingRemoval in PendingRemovals)
		{
			try
			{
				pendingRemoval.Value.Cancel();
				pendingRemoval.Value.Dispose();
			}
			catch
			{
			}
		}
		PendingRemovals.Clear();
		return Task.CompletedTask;
	}
}

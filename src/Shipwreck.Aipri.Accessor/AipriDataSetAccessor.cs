using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Shipwreck.Aipri.Accessor;

public sealed class AipriDataSetAccessor : IDisposable
{
    private const string URL = "https://github.com/pgrho/aipri-items.git";
    private const string JSON_URL = "https://raw.githubusercontent.com/pgrho/aipri-items/refs/heads/master/output/data.json";
    private readonly HttpClient _Http;
    private readonly DirectoryInfo _Directory;

    public AipriDataSetAccessor(string directoryPath)
    {
        _Http = new();
        _Directory = new DirectoryInfo(Path.Combine(directoryPath, "aipri-items"));
    }

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    private Task<AipriGitDataSet>? _Task;
    private string? _Etag;
    private DateTime _LastRefreshedAt;

    public Task<AipriGitDataSet> GetAsync(CancellationToken cancellationToken = default)
    {
        var t = _Task;
        if (t == null
            || t.Status < TaskStatus.RanToCompletion
            || t.Status == TaskStatus.Canceled
            || _LastRefreshedAt + RefreshInterval < DateTime.UtcNow)
        {
            if (t?.IsCompletedSuccessfully == true && _Etag is string etag)
            {
                _Task = t = ValidateEtagAsync(t, etag, cancellationToken);
            }
            else
            {
                _Task = t = GetAsyncCore(cancellationToken);
            }
        }

        return t;
    }

    private async Task<string?> GetEtagAsync(CancellationToken cancellationToken)
    {
        try
        {
            var res = await _Http.SendAsync(new HttpRequestMessage(HttpMethod.Head, JSON_URL)).ConfigureAwait(false);
            if (res.IsSuccessStatusCode)
            {
                return res.Headers.ETag?.Tag;
            }
        }
        catch { }
        return null;
    }

    private async Task<AipriGitDataSet> ValidateEtagAsync(Task<AipriGitDataSet> task, string etag, CancellationToken cancellationToken)
    {
        if (etag == await GetEtagAsync(cancellationToken).ConfigureAwait(false))
        {
            _Task = task;
            _LastRefreshedAt = DateTime.UtcNow;
            return task.Result;
        }
        else
        {
            _Task = task = GetAsyncCore(cancellationToken);
            var r = await _Task.ConfigureAwait(false);
            _Task = Task.FromResult(r);
            return r;
        }
    }

    private async Task<AipriGitDataSet> GetAsyncCore(CancellationToken cancellationToken)
    {
        var fn = await Task.Run(GetFileName, cancellationToken).ConfigureAwait(false);

        using var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);

        var ds = await JsonSerializer.DeserializeAsync<AipriGitDataSet>(fs, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException();

        ds.FileName = fn;

        _Etag = await GetEtagAsync(cancellationToken).ConfigureAwait(false);
        _LastRefreshedAt = DateTime.UtcNow;

        return ds;
    }

    public void Invalidate()
    {
        _Task = null;
        _LastRefreshedAt = DateTime.MinValue;
    }

    public Task<string> GetFileNameAsync(CancellationToken cancellationToken = default)
        => Task.Run(GetFileName, cancellationToken);

    private string GetFileName()
    {
        if (_Directory.Exists)
        {
            try
            {
                using (var repo = new Repository(_Directory.FullName))
                {
                    if (repo.Network.Remotes.ToList() is var remotes
                        && remotes.Count == 1
                        && remotes[0] is var origin
                        && origin.Url == URL
                        && repo.Branches.FirstOrDefault(e => !e.IsRemote && e.FriendlyName == "master") is Branch master)
                    {
                        repo.Reset(ResetMode.Hard, repo.Head.Tip);
                        master = Commands.Checkout(repo, master);

                        var po = new PullOptions();
                        po.FetchOptions = new()
                        {
                            CertificateCheck = (_, _, _) => true
                        };
                        Commands.Pull(repo, new Signature("p", "u", DateTimeOffset.Now), po);

                        return Path.Combine(_Directory.FullName, "output", "data.json");
                    }
                }
            }
            catch { }
            DeleteDirectoryRecursive(_Directory.FullName);
        }

        var pd = _Directory.Parent;
        if (!pd!.Exists)
        {
            pd.Create();
        }

        var co = new CloneOptions() { };
        co.FetchOptions.CertificateCheck = (_, _, _) => true;
        Repository.Clone(URL, _Directory.FullName, co);
        _Directory.Refresh();

        return Path.Combine(_Directory.FullName, "output", "data.json");
    }

    void IDisposable.Dispose()
    {
    }

    internal static void DeleteDirectoryRecursive(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }
        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        Directory.Delete(directoryPath, true);
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Shipwreck.Aipri.Accessor;

public sealed class AipriVerseDataSetAccessor : IDisposable
{
    private const string URL = "https://github.com/pgrho/aipri-items.git";
    private readonly DirectoryInfo _Directory;

    public AipriVerseDataSetAccessor(string directoryPath)
    {
        _Directory = new DirectoryInfo(Path.Combine(directoryPath, "aipri-items"));
    }

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    private Task<AipriVerseGitDataSet>? _Task;
    private DateTime _LastRefreshedAt;

    public Task<AipriVerseGitDataSet> GetAsync(CancellationToken cancellationToken)
    {
        var t = _Task;
        if (t == null
            || t.Status < TaskStatus.RanToCompletion
            || t.Status == TaskStatus.Canceled
            || _LastRefreshedAt + RefreshInterval < DateTime.UtcNow)
        {
            _Task = t = GetAsyncCore(cancellationToken);
        }

        return t;
    }

    private async Task<AipriVerseGitDataSet> GetAsyncCore(CancellationToken cancellationToken)
    {
        var fn = await Task.Run(GetFileName, cancellationToken).ConfigureAwait(false);

        using var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);

        var ds = await JsonSerializer.DeserializeAsync<AipriVerseGitDataSet>(fs, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException();

        ds.FileName = fn;
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

                        Commands.Pull(repo, new Signature("p", "u", DateTimeOffset.Now), new PullOptions());

                        return Path.Combine(_Directory.FullName, "output", "verse.json");
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

        Repository.Clone(URL, _Directory.FullName);
        _Directory.Refresh();

        return Path.Combine(_Directory.FullName, "src", "Shipwreck.AipriDownloader", "output", "verse.json");
    }

    void IDisposable.Dispose()
    {
    }

    internal static void DeleteDirectoryRecursive(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        Directory.Delete(directoryPath, true);
    }
}
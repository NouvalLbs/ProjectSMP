using ProjectSMP.Plugins.Anticheat.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.Anticheat.Utilities;

public class AcLogger : IDisposable
{
    private readonly string _path;
    private readonly BlockingCollection<string> _queue = new(1024);
    private readonly Task _writer;
    private readonly CancellationTokenSource _cts = new();

    public AcLogger(AnticheatConfig config)
    {
        _path = config.LogPath;
        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (dir is not null) Directory.CreateDirectory(dir);
        _writer = Task.Run(WriteLoop);
    }

    public void Log(string msg) => Enqueue("INFO", msg);
    public void LogWarn(string msg) => Enqueue("WARN", msg);
    public void LogKick(int pid, string check) => Enqueue("KICK", $"P:{pid} | {check}");
    public void LogBan(int pid, string check) => Enqueue("BAN", $"P:{pid} | {check}");
    public void LogCheat(int pid, string check, int count, string details = "") =>
        Enqueue("CHEAT", $"P:{pid} | {check} | W:{count}" + (details.Length > 0 ? $" | {details}" : ""));

    private void Enqueue(string level, string msg) =>
        _queue.TryAdd($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {msg}");

    private void WriteLoop()
    {
        foreach (var line in _queue.GetConsumingEnumerable(_cts.Token))
        {
            try { File.AppendAllText(_path, line + "\n"); }
            catch { }
        }
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
        _writer.Wait(TimeSpan.FromSeconds(3));
        _cts.Dispose();
        _queue.Dispose();
    }
}
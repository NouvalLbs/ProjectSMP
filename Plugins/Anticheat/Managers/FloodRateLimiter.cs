using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.Managers;

public class FloodRateLimiter
{
    private readonly ConcurrentDictionary<string, Queue<long>> _windows = new();

    public static readonly (int IntervalMs, int MaxCalls)[] CallbackLimits =
    {
        (120,8),(800,2),(150,8),(250,8),(50,11),(400,8),(50,8),(50,11),
        (50,11),(150,8),(120,8),(600,8),(450,2),(450,2),(450,2),(300,1),
        (450,8),(150,8),(150,8),(150,8),(120,8),(150,8),(150,8),(50,11),
        (60,9),(150,8),(150,8),(50, 11)
    };

    public bool Check(int playerId, int callbackId)
    {
        if ((uint)callbackId >= (uint)CallbackLimits.Length) return true;
        var (ms, max) = CallbackLimits[callbackId];
        return Allow($"{playerId}:{callbackId}", max, ms);
    }

    public bool Allow(string key, int maxCount, int windowMs)
    {
        var q = _windows.GetOrAdd(key, _ => new Queue<long>());
        long now = Environment.TickCount64;
        lock (q)
        {
            while (q.Count > 0 && now - q.Peek() > windowMs) q.Dequeue();
            if (q.Count >= maxCount) return false;
            q.Enqueue(now);
            return true;
        }
    }

    public void ClearPlayer(int playerId)
    {
        string prefix = $"{playerId}:";
        foreach (var k in _windows.Keys.Where(k => k.StartsWith(prefix)))
            _windows.TryRemove(k, out _);
    }
}
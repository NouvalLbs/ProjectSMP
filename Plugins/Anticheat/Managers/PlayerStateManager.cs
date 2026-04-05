#nullable enable
using ProjectSMP.Plugins.Anticheat.State;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Managers;

public class PlayerStateManager
{
    private readonly ConcurrentDictionary<int, PlayerAcState> _states = new();

    public PlayerAcState GetOrCreate(int playerId) =>
        _states.GetOrAdd(playerId, _ => new PlayerAcState());

    public PlayerAcState? Get(int playerId) =>
        _states.TryGetValue(playerId, out var s) ? s : null;

    public void Remove(int playerId) => _states.TryRemove(playerId, out _);

    public bool Exists(int playerId) => _states.ContainsKey(playerId);

    public IEnumerable<KeyValuePair<int, PlayerAcState>> All => _states;
}
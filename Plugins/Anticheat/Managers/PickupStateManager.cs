#nullable enable
using ProjectSMP.Plugins.Anticheat.State;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Managers;

public class PickupStateManager
{
    private readonly ConcurrentDictionary<int, PickupAcState> _states = new();

    public PickupAcState GetOrCreate(int pickupId) =>
        _states.GetOrAdd(pickupId, _ => new PickupAcState());

    public PickupAcState? Get(int pickupId) =>
        _states.TryGetValue(pickupId, out var s) ? s : null;

    public void Register(int pickupId, float x, float y, float z, int type = 0, int weapon = 0, int amount = 0)
    {
        var st = GetOrCreate(pickupId);
        st.X = x; st.Y = y; st.Z = z;
        st.Type = type; st.Weapon = weapon; st.Amount = amount;
    }

    public void Remove(int pickupId) => _states.TryRemove(pickupId, out _);

    public IEnumerable<KeyValuePair<int, PickupAcState>> All => _states;
}
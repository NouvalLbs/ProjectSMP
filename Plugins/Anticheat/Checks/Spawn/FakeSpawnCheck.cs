using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Spawn;

public class FakeSpawnCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FakeSpawnCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled) return;
        if (!_config.GetCheck("FakeSpawn").Enabled) return;

        long now = Environment.TickCount64;

        bool validSpawn = st.IsDead
                       || st.SpawnTick == 0
                       || st.SpawnSetFlag > 0
                       || st.PendingClassResult
                       || now - st.SpawnTick > 30_000;

        if (!validSpawn)
            _warnings.AddWarning(player.Id, "FakeSpawn",
                $"isDead={st.IsDead} setFlag={st.SpawnSetFlag}");
        else if (st.HasSpawnPos)
        {
            var pos = player.Position;
            float dist = MathF.Sqrt(
                (pos.X - st.SpawnPosX) * (pos.X - st.SpawnPosX) +
                (pos.Y - st.SpawnPosY) * (pos.Y - st.SpawnPosY));
            if (dist > 5f)
                _warnings.AddWarning(player.Id, "FakeSpawn",
                    $"spawnPos dist={dist:F1}");
        }

        st.IsDead = false;
        st.SpawnSetFlag = 0;
        st.PendingClassResult = false;
        st.SpawnTick = now;
        st.Health = player.Health;
        st.Armour = player.Armour;
        st.Money = player.Money;
    }
}
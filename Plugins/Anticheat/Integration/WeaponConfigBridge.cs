#nullable enable
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.WeaponConfig;
using System;

namespace ProjectSMP.Plugins.Anticheat.Integration;

public static class WeaponConfigBridge
{
    private static PlayerStateManager? _playerManager;
    private static bool _initialized = false;

    public static void Initialize(PlayerStateManager playerManager)
    {
        if (_initialized) return;

        _playerManager = playerManager;

        WeaponConfigService.PlayerDamage += OnPlayerDamage;
        WeaponConfigService.PlayerDamageDone += OnPlayerDamageDone;
        WeaponConfigService.PlayerPrepareDeath += OnPlayerPrepareDeath;
        WeaponConfigService.PlayerDeathFinished += OnPlayerDeathFinished;

        _initialized = true;
    }

    private static void OnPlayerDamage(object? sender, PlayerDamageArgs e)
    {
        if (_playerManager == null) return;

        var st = _playerManager.Get(e.Player.Id);
        if (st == null) return;

        st.Health = WeaponConfigWrappers.GetPlayerHealth(e.Player);
        st.Armour = WeaponConfigWrappers.GetPlayerArmour(e.Player);
        st.SetHealthTick = Environment.TickCount64;
        st.SetArmourTick = Environment.TickCount64;
    }

    private static void OnPlayerDamageDone(object? sender, PlayerDamageArgs e)
    {
        if (_playerManager == null) return;

        var st = _playerManager.Get(e.Player.Id);
        if (st != null)
        {
            st.SetPosTick = Environment.TickCount64;
        }
    }

    private static void OnPlayerPrepareDeath(object? sender, PrepareDeathArgs e)
    {
        if (_playerManager == null) return;

        var st = _playerManager.Get(e.Player.Id);
        if (st != null)
        {
            st.IsDead = true;
            st.SetPosTick = Environment.TickCount64 + 5000;
            st.SetHealthTick = Environment.TickCount64 + 5000;
            st.SetArmourTick = Environment.TickCount64 + 5000;
        }
    }

    private static void OnPlayerDeathFinished(object? sender, DeathFinishedArgs e)
    {
        if (_playerManager == null) return;

        var st = _playerManager.Get(e.Player.Id);
        if (st != null)
        {
            st.IsDead = false;
            st.SpawnTick = Environment.TickCount64;
        }
    }

    public static void OnPlayerResync(int playerId)
    {
        if (_playerManager == null) return;

        var st = _playerManager.Get(playerId);
        if (st != null)
        {
            st.SetPosTick = Environment.TickCount64 + 1000;
            st.PlayerVelocityTick = Environment.TickCount64 + 1000;
        }
    }

    public static void Shutdown()
    {
        if (!_initialized) return;

        WeaponConfigService.PlayerDamage -= OnPlayerDamage;
        WeaponConfigService.PlayerDamageDone -= OnPlayerDamageDone;
        WeaponConfigService.PlayerPrepareDeath -= OnPlayerPrepareDeath;
        WeaponConfigService.PlayerDeathFinished -= OnPlayerDeathFinished;

        _initialized = false;
        _playerManager = null;
    }
}
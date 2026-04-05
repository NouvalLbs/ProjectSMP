#nullable enable
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Core;
using ProjectSMP.Plugins.Anticheat.Utilities;
using System;

namespace ProjectSMP.Plugins.Anticheat.Managers;

public class WarningManager
{
    private readonly PlayerStateManager _players;
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public Action<int, string, PunishAction>? PunishmentRequired;

    public WarningManager(PlayerStateManager players, AnticheatConfig config, AcLogger logger)
    {
        _players = players;
        _config = config;
        _logger = logger;
    }

    public CheckResult AddWarning(int playerId, string checkName, string details = "")
    {
        if (!_config.Enabled) return CheckResult.Pass;

        var state = _players.GetOrCreate(playerId);
        var cfg = _config.GetCheck(checkName);

        if (!cfg.Enabled || !state.IsCheckEnabled(checkName)) return CheckResult.Pass;

        int count = state.AddWarning(checkName);
        _logger.LogCheat(playerId, checkName, count, details);

        if (count < cfg.MaxWarnings) return CheckResult.Warn;

        state.ResetWarning(checkName);
        PunishmentRequired?.Invoke(playerId, checkName, cfg.Action);

        return cfg.Action switch
        {
            PunishAction.Ban => CheckResult.Ban,
            PunishAction.Kick => CheckResult.Kick,
            _ => CheckResult.Warn
        };
    }

    public void Reset(int playerId, string? checkName = null)
    {
        var state = _players.Get(playerId);
        if (state is null) return;
        if (checkName is not null) state.ResetWarning(checkName);
        else state.ResetAllWarnings();
    }

    public void DisableCheckForPlayer(int playerId, string checkName)
        => _players.GetOrCreate(playerId).DisableCheck(checkName);

    public void EnableCheckForPlayer(int playerId, string checkName)
        => _players.Get(playerId)?.EnableCheck(checkName);

    public int GetCount(int playerId, string checkName) =>
        _players.Get(playerId)?.GetWarning(checkName) ?? 0;
}
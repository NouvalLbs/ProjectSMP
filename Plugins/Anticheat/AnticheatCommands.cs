using SampSharp.GameMode.World;
using SampSharp.GameMode.SAMP.Commands;
using System.Collections.Generic;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Plugins.Anticheat;

[CommandGroup("ac")]
public class AnticheatCommands
{
    private readonly AnticheatPlugin _ac;
    private readonly HashSet<int> _adminIds;

    public AnticheatCommands(AnticheatPlugin ac, HashSet<int> adminIds) {
        _ac = ac;
        _adminIds = adminIds;
    }

    // ══════════════════════════════════════════════════════════════
    // PERMISSION CHECK
    // ══════════════════════════════════════════════════════════════
    private bool HasPermission(BasePlayer player) {
        if (_adminIds.Contains(player.Id)) return true;

        player.SendClientMessage(Color.Red, "[AC] You don't have permission to use this command.");
        return false;
    }

    // ══════════════════════════════════════════════════════════════
    // /accheck [id] - Check player warnings
    // ══════════════════════════════════════════════════════════════
    [Command("check")]
    public bool CheckCommand(BasePlayer player, int targetId) {
        if (!HasPermission(player)) return true;

        var target = BasePlayer.Find(targetId);
        if (target == null) {
            player.SendClientMessage(Color.Red, "[AC] Player not found.");
            return true;
        }

        var state = _ac.Players.Get(targetId);
        if (state == null) {
            player.SendClientMessage(Color.Red, "[AC] Player state not found.");
            return true;
        }

        var warnings = new System.Text.StringBuilder($"{{FFFF00}}[AC] Player {targetId} warnings: ");

        if (state.WarningCounts.Count == 0) {
            warnings.Append("{{00FF00}}None");
        } else {
            foreach (var (check, count) in state.WarningCounts)
            {
                warnings.Append($"{{FF6600}}{check}={{FFFFFF}}{count} ");
            }
        }

        player.SendClientMessage(Color.Yellow, warnings.ToString());
        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /acreset [id] - Reset all warnings for player
    // ══════════════════════════════════════════════════════════════
    [Command("reset")]
    public bool ResetCommand(BasePlayer player, int targetId) {
        if (!HasPermission(player)) return true;

        var target = BasePlayer.Find(targetId);
        if (target == null) {
            player.SendClientMessage(Color.Red, "[AC] Player not found.");
            return true;
        }

        _ac.Warnings.Reset(targetId);
        player.SendClientMessage(Color.LimeGreen, $"[AC] All warnings reset for player {targetId}.");
        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /acexempt [id] [check] - Exempt player from specific check
    // ══════════════════════════════════════════════════════════════
    [Command("exempt")]
    public bool ExemptCommand(BasePlayer player, int targetId, string checkName) {
        if (!HasPermission(player)) return true;

        var target = BasePlayer.Find(targetId);
        if (target == null) {
            player.SendClientMessage(Color.Red, "[AC] Player not found.");
            return true;
        }

        _ac.Warnings.DisableCheckForPlayer(targetId, checkName);
        player.SendClientMessage(Color.LimeGreen,
            $"[AC] Player {targetId} exempted from '{checkName}'.");
        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /acunexempt [id] [check] - Remove exemption
    // ══════════════════════════════════════════════════════════════
    [Command("unexempt")]
    public bool UnexemptCommand(BasePlayer player, int targetId, string checkName) {
        if (!HasPermission(player)) return true;

        var target = BasePlayer.Find(targetId);
        if (target == null) {
            player.SendClientMessage(Color.Red, "[AC] Player not found.");
            return true;
        }

        _ac.Warnings.EnableCheckForPlayer(targetId, checkName);
        player.SendClientMessage(Color.LimeGreen,
            $"[AC] Player {targetId} exemption removed from '{checkName}'.");
        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /acreload - Reload anticheat config
    // ══════════════════════════════════════════════════════════════
    [Command("reload")]
    public bool ReloadCommand(BasePlayer player) {
        if (!HasPermission(player)) return true;

        try {
            var fresh = AnticheatPlugin.LoadConfig();

            _ac.Config.Enabled = fresh.Enabled;
            _ac.Config.MaxPing = fresh.MaxPing;
            _ac.Config.LogPath = fresh.LogPath;
            _ac.Config.SpeedHackVehResetDelay = fresh.SpeedHackVehResetDelay;
            _ac.Config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
            _ac.Config.MinReconnectSeconds = fresh.MinReconnectSeconds;

            foreach (var (key, value) in fresh.Checks)
            {
                _ac.Config.Checks[key] = value;
            }

            player.SendClientMessage(Color.LimeGreen, "[AC] Configuration reloaded successfully.");
        } catch (System.Exception ex) {
            player.SendClientMessage(Color.Red, $"[AC] Reload failed: {ex.Message}");
        }

        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /acstats [id] - Show detailed stats for player (BONUS)
    // ══════════════════════════════════════════════════════════════
    [Command("stats")]
    public bool StatsCommand(BasePlayer player, int targetId) {
        if (!HasPermission(player)) return true;

        var target = BasePlayer.Find(targetId);
        if (target == null) {
            player.SendClientMessage(Color.Red, "[AC] Player not found.");
            return true;
        }

        var state = _ac.Players.Get(targetId);
        if (state == null) {
            player.SendClientMessage(Color.Red, "[AC] Player state not found.");
            return true;
        }

        player.SendClientMessage(Color.Yellow, $"═══ AC Stats for {target.Name} (ID:{targetId}) ═══");
        player.SendClientMessage(Color.White, $"IP: {state.IpAddress}");
        player.SendClientMessage(Color.White, $"Online: {state.IsOnline} | Dead: {state.IsDead}");
        player.SendClientMessage(Color.White, $"Vehicle: {state.VehicleId} | Interior: {target.Interior}");
        player.SendClientMessage(Color.White, $"Position: ({state.X:F1}, {state.Y:F1}, {state.Z:F1})");
        player.SendClientMessage(Color.White, $"Health: {target.Health:F1} | Armour: {target.Armour:F1}");
        player.SendClientMessage(Color.White, $"Money: {state.Money}");

        var warningCount = state.WarningCounts.Count;
        player.SendClientMessage(Color.Yellow, $"Total Warning Types: {warningCount}");

        return true;
    }

    // ══════════════════════════════════════════════════════════════
    // /achelp - Show help
    // ══════════════════════════════════════════════════════════════
    [Command("help")]
    public bool HelpCommand(BasePlayer player) {
        if (!HasPermission(player)) return true;

        player.SendClientMessage(Color.Yellow, "═══════════ Anticheat Commands ═══════════");
        player.SendClientMessage(Color.White, "/accheck [id] - Check player warnings");
        player.SendClientMessage(Color.White, "/acreset [id] - Reset all warnings");
        player.SendClientMessage(Color.White, "/acexempt [id] [check] - Exempt from check");
        player.SendClientMessage(Color.White, "/acunexempt [id] [check] - Remove exemption");
        player.SendClientMessage(Color.White, "/acreload - Reload configuration");
        player.SendClientMessage(Color.White, "/acstats [id] - Show detailed stats");
        player.SendClientMessage(Color.White, "/achelp - Show this help");

        return true;
    }
}
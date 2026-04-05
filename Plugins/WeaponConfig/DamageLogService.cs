using ProjectSMP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class DamageLogService
    {
        private const string Table = "players_damage";
        private const int MaxLogsPerPlayer = 100;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task LogDamageAsync(string citizenId, string issuerName, int weapon, float amount, int bodypart)
        {
            try
            {
                var logs = await GetLogsAsync(citizenId);

                logs.Insert(0, new DamageLogEntry
                {
                    Issuer = issuerName,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Weapon = weapon,
                    Amount = amount,
                    Bodypart = bodypart
                });

                if (logs.Count > MaxLogsPerPlayer)
                    logs.RemoveRange(MaxLogsPerPlayer, logs.Count - MaxLogsPerPlayer);

                var json = JsonSerializer.Serialize(logs, JsonOpts);

                var exists = await DatabaseManager.ExistsAsync(
                    $"SELECT COUNT(*) FROM `{Table}` WHERE citizenId = @Id",
                    new { Id = citizenId });

                if (exists)
                {
                    await DatabaseManager.ExecuteAsync(
                        $"UPDATE `{Table}` SET damage = @Data WHERE citizenId = @Id",
                        new { Id = citizenId, Data = json });
                }
                else
                {
                    await DatabaseManager.ExecuteAsync(
                        $"INSERT INTO `{Table}` (citizenId, damage) VALUES (@Id, @Data)",
                        new { Id = citizenId, Data = json });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DamageLog] Error: {ex.Message}");
            }
        }

        public static async Task<List<DamageLogEntry>> GetLogsAsync(string citizenId)
        {
            try
            {
                var json = await DatabaseManager.QueryFirstAsync<string>(
                    $"SELECT damage FROM `{Table}` WHERE citizenId = @Id LIMIT 1",
                    new { Id = citizenId });

                if (string.IsNullOrEmpty(json))
                    return new List<DamageLogEntry>();

                return JsonSerializer.Deserialize<List<DamageLogEntry>>(json, JsonOpts)
                    ?? new List<DamageLogEntry>();
            }
            catch
            {
                return new List<DamageLogEntry>();
            }
        }
    }
}
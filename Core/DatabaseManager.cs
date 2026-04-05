using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace ProjectSMP.Core {
    internal static class DatabaseManager {
        private static string _connectionString;

        // ── Init ──────────────────────────────────────────────────────────────
        public static async Task InitAsync() {
            var db = ConfigManager.Game.Database;

            _connectionString = $"Server={db.Host};Port={db.Port};Database={db.Name};User={db.User};Password={db.Password};";

            try {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                Console.WriteLine("[+] MariaDB - Koneksi ke MariaDB berhasil.");
            } catch (Exception ex) {
                Console.WriteLine($"[+] MariaDB - Koneksi gagal: {ex.Message}");
                throw;
            }
        }

        private static MySqlConnection Connection() => new MySqlConnection(_connectionString);

        // ── Query — ambil banyak data ─────────────────────────────────────────
        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) {
            await using var conn = Connection();
            return await conn.QueryAsync<T>(sql, param);
        }

        // ── QueryFirst — ambil satu data ──────────────────────────────────────
        public static async Task<T> QueryFirstAsync<T>(string sql, object param = null) {
            await using var conn = Connection();
            return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        // ── Execute — insert / update / delete ────────────────────────────────
        public static async Task<int> ExecuteAsync(string sql, object param = null) {
            await using var conn = Connection();
            return await conn.ExecuteAsync(sql, param);
        }

        // ── ExecuteScalar — ambil nilai tunggal (COUNT, LAST_INSERT_ID, dll) ──
        public static async Task<T> ExecuteScalarAsync<T>(string sql, object param = null) {
            await using var conn = Connection();
            return await conn.ExecuteScalarAsync<T>(sql, param);
        }

        // ── Exists — cek apakah data ada ─────────────────────────────────────
        public static async Task<bool> ExistsAsync(string sql, object param = null) {
            await using var conn = Connection();
            var count = await conn.ExecuteScalarAsync<int>(sql, param);
            return count > 0;
        }
    }
}
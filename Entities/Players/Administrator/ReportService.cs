using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Data;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class ReportService
    {
        private const int MaxReports = 1000;
        private const int ExpireTimeSeconds = 300;
        private const int CooldownSeconds = 180;

        private static readonly List<ReportEntry> _reports = new();
        private static readonly Dictionary<int, int> _playerCooldown = new();
        private static Timer _timer;

        public static void Initialize()
        {
            for (var i = 0; i < MaxReports; i++)
                _reports.Add(new ReportEntry { Id = i });

            _timer = new Timer(1000, true);
            _timer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _reports.Clear();
            _playerCooldown.Clear();
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var report in _reports.Where(r => r.InUse))
            {
                report.TimeToExpire--;
                if (report.TimeToExpire <= 0)
                {
                    var reporter = BasePlayer.Find(report.ReporterId) as Player;
                    reporter?.SendClientMessage(Color.White, $"{Msg.Report} Your report has expired. You can attempt to report again if you wish.");
                    ClearReport(report);
                }
            }

            foreach (var playerId in _playerCooldown.Keys.ToList())
            {
                _playerCooldown[playerId]--;
                if (_playerCooldown[playerId] <= 0)
                    _playerCooldown.Remove(playerId);
            }
        }

        public static bool CanReport(Player player)
        {
            return !_playerCooldown.ContainsKey(player.Id);
        }

        public static int GetCooldown(Player player)
        {
            return _playerCooldown.TryGetValue(player.Id, out var cd) ? cd : 0;
        }

        public static void AddReport(Player player, string message)
        {
            var report = _reports.FirstOrDefault(r => !r.InUse);
            if (report == null)
            {
                ClearAllReports();
                report = _reports.First(r => !r.InUse);
            }

            report.InUse = true;
            report.Message = message;
            report.ReporterId = player.Id;
            report.ReporterName = player.CharInfo.Username;
            report.TimeToExpire = ExpireTimeSeconds;
            report.CreatedAt = DateTime.Now;
            report.CheckingBy = -1;

            _playerCooldown[player.Id] = CooldownSeconds;
            Utilities.SendStaffMessage(-1, "{0}[{1}] {2}: {3}", Msg.Report, player.Id, player.CharInfo.Username, message);
        }

        public static void AcceptReport(Player admin, int reportId)
        {
            if (reportId < 0 || reportId >= MaxReports) return;

            var report = _reports[reportId];
            if (!report.InUse) return;

            var reporter = BasePlayer.Find(report.ReporterId) as Player;
            if (reporter == null || !reporter.IsConnected)
            {
                ClearReport(report);
                return;
            }

            Utilities.SendStaffMessage(-1, "{0}[{1}] {{ff0000}}{2} {{fff8bf}}has responded to {{ffff66}}{3}{{fff8bf}}'s report", Msg.Report, reporter.Id, admin.Ucp, reporter.CharInfo.Username);
            reporter.SendClientMessage(Color.White, $"{Msg.Report} Your report is currently being handled by {{ff0000}}{admin.Ucp}{{fff8bf}}. Please wait patiently.");
            ClearReport(report);
        }

        public static void DeleteReport(Player admin, int reportId)
        {
            if (reportId < 0 || reportId >= MaxReports) return;

            var report = _reports[reportId];
            if (!report.InUse) return;

            var reporter = BasePlayer.Find(report.ReporterId) as Player;
            var reporterName = reporter?.CharInfo.Username ?? report.ReporterName;
            var reporterId = reporter?.Id ?? -1;

            Utilities.SendStaffMessage(-1, "{0}[{1}] has trashed {{ffff66}}{2}{{fff8bf}}'s report", Msg.Report_R, reporterId, reporterName);
            ClearReport(report);
        }

        public static void ClearAllReports()
        {
            foreach (var report in _reports)
                ClearReport(report);
        }

        private static void ClearReport(ReportEntry report)
        {
            report.InUse = false;
            report.Message = "";
            report.ReporterId = -1;
            report.ReporterName = "";
            report.TimeToExpire = 0;
            report.CheckingBy = -1;
        }

        public static List<ReportEntry> GetActiveReports()
        {
            return _reports.Where(r => r.InUse).ToList();
        }
    }
}
using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Data;
using ProjectSMP.Extensions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class AskService
    {
        private const int MaxAsks = 1000;
        private const int ExpireTimeSeconds = 120;
        private const int CooldownSeconds = 120;

        private static readonly List<AskData> _asks = new();
        private static readonly Dictionary<int, int> _playerCooldown = new();
        private static Timer _timer;

        public static void Initialize()
        {
            for (var i = 0; i < MaxAsks; i++)
                _asks.Add(new AskData { Id = i });

            _timer = new Timer(1000, true);
            _timer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _asks.Clear();
            _playerCooldown.Clear();
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var ask in _asks.Where(a => a.InUse))
            {
                ask.TimeToExpire--;
                if (ask.TimeToExpire <= 0)
                {
                    var player = BasePlayer.Find(ask.PlayerId) as Player;
                    player?.SendClientMessage(Color.White, $"{Msg.Ask} Tidak ada yang menjawab pertanyaanmu, sekarang kamu dapat menggunakan /ask lagi.");
                    ClearAsk(ask);
                }
            }

            foreach (var playerId in _playerCooldown.Keys.ToList())
            {
                _playerCooldown[playerId]--;
                if (_playerCooldown[playerId] <= 0)
                    _playerCooldown.Remove(playerId);
            }
        }

        public static bool CanAsk(Player player)
        {
            return !_playerCooldown.ContainsKey(player.Id) && GetPlayerAskCount(player) == 0;
        }

        public static int GetCooldown(Player player)
        {
            return _playerCooldown.TryGetValue(player.Id, out var cd) ? cd : 0;
        }

        public static int GetPlayerAskCount(Player player)
        {
            return _asks.Count(a => a.InUse && a.PlayerId == player.Id);
        }

        public static void AddAsk(Player player, string question)
        {
            var ask = _asks.FirstOrDefault(a => !a.InUse);
            if (ask == null)
            {
                ClearAllAsks();
                ask = _asks.FirstOrDefault(a => !a.InUse);

                if (ask == null)
                {
                    player.SendClientMessage(Color.White, $"{Msg.Error} Daftar ask penuh. Silakan tunggu sebentar.");
                    return;
                }
            }

            ask.InUse = true;
            ask.PlayerId = player.Id;
            ask.PlayerName = player.CharInfo.Username;
            ask.Question = question;
            ask.TimeToExpire = ExpireTimeSeconds;
            ask.CreatedAt = DateTime.Now;

            _playerCooldown[player.Id] = CooldownSeconds;
            Utilities.SendStaffMessage(-1, "{0} {1}[{2}]:{{ffff66}} {3}", Msg.Ask, player.CharInfo.Username, player.Id, question);
        }

        public static void AnswerAsk(Player admin, int targetId, string answer)
        {
            var target = BasePlayer.Find(targetId) as Player;
            if (target == null || !target.IsConnected) return;

            Utilities.SendStaffMessage(-1, "{0} {1}:{{ffffff}} to ID {2}:{{ffff66}} {3}", Msg.Ask_A, admin.Ucp, target.Id, answer);
            target.SendClientMessage(-1, $"{Msg.Ask_A} {admin.Ucp}{{ffffff}} replied:{{ffff66}} {answer}");

            ClearPlayerAsks(target);
        }

        public static void ClearPlayerAsks(Player player)
        {
            foreach (var ask in _asks.Where(a => a.InUse && a.PlayerId == player.Id))
                ClearAsk(ask);

            _playerCooldown.Remove(player.Id);
        }

        public static void ClearAllAsks()
        {
            foreach (var ask in _asks)
                ClearAsk(ask);
        }

        private static void ClearAsk(AskData ask)
        {
            ask.InUse = false;
            ask.PlayerId = -1;
            ask.PlayerName = "";
            ask.Question = "";
            ask.TimeToExpire = 0;
        }

        public static List<AskData> GetActiveAsks()
        {
            return _asks.Where(a => a.InUse).ToList();
        }

        public static int GetAskCount()
        {
            return _asks.Count(a => a.InUse);
        }
    }
}
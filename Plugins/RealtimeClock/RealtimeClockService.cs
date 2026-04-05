using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;

namespace ProjectSMP.Plugins.RealtimeClock
{
    public static class RealtimeClockService
    {
        private static int _hour;
        private static int _minute;
        private static int _second;
        private static int _interval = 10000;
        private static bool _syncWithServer;
        private static Timer _timer;
        private static readonly HashSet<int> _frozenPlayers = new();

        public static event EventHandler<WorldTimeUpdateEventArgs> WorldTimeUpdate;

        public static void Init()
        {
            _timer = new Timer(_interval) { AutoReset = true };
            _timer.Elapsed += OnTimerElapsed;
            WeatherManager.Init();
        }

        public static void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _frozenPlayers.Clear();
        }

        public static int GetHour() => _hour;
        public static int GetMinute() => _minute;
        public static int GetSecond() => _second;
        public static int GetInterval() => _interval;

        public static void SetHour(int hour, bool broadcast = true)
        {
            _hour = hour % 24;
            UpdateAllPlayers();
            if (broadcast) BroadcastTimeUpdate();
        }

        public static void SetMinute(int minute, bool broadcast = true)
        {
            _minute = minute % 60;
            UpdateAllPlayers();
            if (broadcast) BroadcastTimeUpdate();
        }

        public static void SetSecond(int second, bool broadcast = true)
        {
            _second = second % 60;
            if (broadcast) BroadcastTimeUpdate();
        }

        public static void SetWorldTime(int hour, int minute, int second = 0, bool restartTimer = true, bool broadcast = true)
        {
            _hour = hour % 24;
            _minute = minute % 60;
            _second = second % 60;
            UpdateAllPlayers();
            if (restartTimer) StartTime();
            if (broadcast) BroadcastTimeUpdate();
        }

        public static void SetInterval(int interval, bool restartTimer = true)
        {
            _interval = interval;
            _syncWithServer = false;
            _timer.Interval = _interval;
            if (restartTimer) StartTime();
        }

        public static void Sync(bool serverTime = true)
        {
            _syncWithServer = serverTime;
            if (_syncWithServer) _interval = 1000;

            StartTime();

            var now = DateTime.Now;
            _hour = now.Hour;
            _minute = now.Minute;
            _second = now.Second;

            UpdateAllPlayers();
            BroadcastTimeUpdate();
        }

        public static void StartTime()
        {
            _timer.Stop();
            _timer.Interval = _interval;
            _timer.Start();
        }

        public static void StopTime()
        {
            _timer.Stop();
        }

        public static bool IsPlayerFrozen(int playerId)
        {
            return _frozenPlayers.Contains(playerId);
        }

        public static void FreezeForPlayer(int playerId)
        {
            _frozenPlayers.Add(playerId);
        }

        public static void UnfreezeForPlayer(int playerId)
        {
            _frozenPlayers.Remove(playerId);
            SyncPlayerTime(playerId);
        }

        public static void SyncPlayerTime(int playerId)
        {
            var player = BasePlayer.Find(playerId);
            if (player != null && !player.IsDisposed)
            {
                player.SetTime(_hour, _minute);
            }
        }

        public static void OnPlayerConnect(int playerId)
        {
            UnfreezeForPlayer(playerId);
        }

        public static void OnPlayerSpawn(int playerId, bool show)
        {
            ClockTextDrawManager.SetVisible(playerId, show);
            WeatherManager.SyncPlayerWeather(playerId);
        }

        public static void OnPlayerDisconnect(int playerId)
        {
            ClockTextDrawManager.Destroy(playerId);
            _frozenPlayers.Remove(playerId);
        }

        private static void UpdateAllPlayers()
        {
            foreach (var player in BasePlayer.All)
            {
                if (!_frozenPlayers.Contains(player.Id))
                {
                    SyncPlayerTime(player.Id);
                }
            }
        }

        private static void BroadcastTimeUpdate()
        {
            ClockTextDrawManager.UpdateAll(_hour, _minute, _second);
            WeatherManager.OnTimeUpdate(_hour, _minute, _second);
            WorldTimeUpdate?.Invoke(null, new WorldTimeUpdateEventArgs(_hour, _minute, _second));
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_syncWithServer)
            {
                var now = DateTime.Now;
                if (now.Hour != _hour || now.Minute != _minute || now.Second != _second)
                {
                    SetWorldTime(now.Hour, now.Minute, now.Second, false, true);
                }
            }
            else
            {
                _second++;
                if (_second >= 60)
                {
                    _second = 0;
                    _minute++;
                    if (_minute >= 60)
                    {
                        _minute = 0;
                        _hour = (_hour + 1) % 24;
                    }
                }
                UpdateAllPlayers();
                BroadcastTimeUpdate();
            }
        }
    }
}
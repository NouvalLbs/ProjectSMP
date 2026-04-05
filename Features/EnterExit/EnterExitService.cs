using ProjectSMP.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Features.EnterExit
{
    public static class EnterExitService
    {
        private static readonly Dictionary<int, CancellationTokenSource> _activeSessions = new();

        public static void ProcessEnterExit(Player player, Action onComplete)
        {
            Cancel(player);

            var delaySeconds = player.Settings.EnterExit;
            if (delaySeconds <= 0)
            {
                onComplete?.Invoke();
                return;
            }

            player.ToggleControllableSafe(false);

            var cts = new CancellationTokenSource();
            _activeSessions[player.Id] = cts;

            DelayedUnfreezeAsync(player, delaySeconds * 1000, onComplete, cts.Token);
        }

        public static void Cancel(Player player)
        {
            if (!_activeSessions.TryGetValue(player.Id, out var cts)) return;
            cts.Cancel();
            _activeSessions.Remove(player.Id);
        }

        public static void Cleanup(Player player)
        {
            Cancel(player);
        }

        private static async void DelayedUnfreezeAsync(Player player, int delayMs, Action onComplete, CancellationToken ct)
        {
            try
            {
                await Task.Delay(delayMs, ct);
                if (player.IsDisposed) return;

                player.ToggleControllableSafe(true);
                _activeSessions.Remove(player.Id);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnterExit] Error: {ex.Message}");
            }
        }
    }
}
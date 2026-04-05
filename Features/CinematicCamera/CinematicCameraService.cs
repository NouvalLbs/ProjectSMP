using ProjectSMP.Extensions;
using ProjectSMP.Plugins.Streamer;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Features.CinematicCamera {
    internal sealed record CameraLocation(
        Vector3 PlayerPosition,
        Vector3 CameraFrom,
        Vector3 CameraTo,
        Vector3 LookAtFrom,
        Vector3 LookAtTo
    );

    internal static class CinematicCameraService {
        private const int InterpolateDuration = 10000;
        private const int InitialDelay = 1000;
        private const int CycleInterval = 10000;

        private static readonly Random Rng = new();
        private static readonly Dictionary<int, CancellationTokenSource> Sessions = new();

        private static readonly CameraLocation[] Locations = new CameraLocation[] {
            new(new Vector3(1506.4854f, -1668.4895f, 10.0469f),
                new Vector3(1334.8841f, -1568.8443f, 75.0140f),
                new Vector3(1497.7893f, -1673.8750f, 44.5872f),
                new Vector3(1339.4245f, -1570.4919f, 73.7218f),
                new Vector3(1502.4501f, -1674.0040f, 42.7819f)),

            new(new Vector3(1478.3308f, -1836.1998f, 8.4815f),
                new Vector3(1497.7844f, -1673.8718f, 44.5881f),
                new Vector3(1482.8688f, -1736.7686f, 54.7054f),
                new Vector3(1502.4880f, -1673.9885f, 42.8961f),
                new Vector3(1482.7272f, -1740.8701f, 51.8491f)),

            new(new Vector3(1772.7957f, -1746.2915f, 7.5488f),
                new Vector3(1672.5948f, -1611.0191f, 73.9456f),
                new Vector3(1826.9919f, -1735.1242f, 52.9647f),
                new Vector3(1675.4504f, -1615.0129f, 72.9993f),
                new Vector3(1823.4168f, -1737.4779f, 50.3802f)),

            new(new Vector3(1128.7858f, -1457.9923f, 11.7969f),
                new Vector3(1200.3928f, -1596.3179f, 78.6117f),
                new Vector3(1091.8239f, -1326.4683f, 34.0386f),
                new Vector3(1198.4396f, -1591.8211f, 77.6300f),
                new Vector3(1093.9477f, -1330.9379f, 33.3230f)),

            new(new Vector3(1161.8888f, -1333.7797f, 25.0926f),
                new Vector3(1038.2705f, -1197.8133f, 57.7264f),
                new Vector3(1221.3920f, -1325.8160f, 47.3336f),
                new Vector3(1041.8807f, -1201.2148f, 57.0969f),
                new Vector3(1216.9973f, -1325.7799f, 44.9493f)),

            new(new Vector3(665.5995f, -1355.9509f, 13.5391f),
                new Vector3(417.5477f, -1312.9903f, 52.1189f),
                new Vector3(620.6255f, -1309.8242f, 47.0491f),
                new Vector3(422.5266f, -1313.3835f, 52.3548f),
                new Vector3(623.6400f, -1312.9886f, 44.6203f)),
        };

        public static void Start(BasePlayer player) {
            Stop(player);

            player.SetSpawnInfoSafe(255, 1, 0f, 0f, -5f, 0f);
            player.ToggleControllableSafe(false);

            if (player is Player p)
                p.ToggleSpectatingSafe(true);
            else
                player.ToggleSpectating(true);

            player.Color = new Color(255, 255, 255);

            var cts = new CancellationTokenSource();
            Sessions[player.Id] = cts;
            RunLoopAsync(player, cts.Token);
        }

        public static void Stop(BasePlayer player) {
            if (!Sessions.TryGetValue(player.Id, out var cts)) return;
            cts.Cancel();
            Sessions.Remove(player.Id);
        }

        private static async void RunLoopAsync(BasePlayer player, CancellationToken ct)
        {
            try
            {
                await Task.Delay(InitialDelay, ct);
                Apply(player, Locations[0]);

                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(CycleInterval, ct);
                    Apply(player, Locations[Rng.Next(Locations.Length)]);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Console.WriteLine($"[CinematicCamera] RunLoop: {ex.Message}"); }
        }

        private static void Apply(BasePlayer player, CameraLocation loc) {
            if (player.IsDisposed) return;

            player.Interior = 0;

            if (player is Player p) {
                p.SetVirtualWorldSafe(0);
                p.SetPositionSafe(loc.PlayerPosition);
            } else {
                player.SetVirtualWorldSafe(0);
                player.SetPositionSafe(loc.PlayerPosition);
            }

            player.InterpolateCameraPosition(loc.CameraFrom, loc.CameraTo, InterpolateDuration, CameraCut.Cut);
            player.InterpolateCameraLookAt(loc.LookAtFrom, loc.LookAtTo, InterpolateDuration, CameraCut.Cut);

            StreamerNatives.Instance.Streamer_UpdateEx(player.Id, loc.LookAtTo.X, loc.LookAtTo.Y, loc.LookAtTo.Z);
            StreamerNatives.Instance.Streamer_Update(player.Id);
        }
    }
}
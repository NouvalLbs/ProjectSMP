using ProjectSMP.Entities.Players.Administrator.Data;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class NoClipService
    {
        private const float MoveSpeed = 100.0f;
        private const float AccelRate = 0.03f;
        private static readonly Dictionary<int, NoClipData> Sessions = new();

        public static void Start(Player player)
        {
            var pos = player.Position;
            var data = new NoClipData();

            data.FlyObject = new PlayerObject(player, 19300, pos, Vector3.Zero);
            player.ToggleSpectatingSafe(true);
            data.FlyObject.AttachCameraToObject();

            data.Mode = NoClipMode.Fly;
            Sessions[player.Id] = data;
        }

        public static void Stop(Player player)
        {
            if (!Sessions.TryGetValue(player.Id, out var data)) return;

            player.ToggleSpectatingSafe(false);
            var pos = player.CameraPosition;
            player.SetPositionSafe(pos.X, pos.Y, pos.Z + 0.3f);
            data.FlyObject?.Dispose();

            Sessions.Remove(player.Id);
        }

        public static void OnPlayerUpdate(Player player)
        {
            if (!Sessions.TryGetValue(player.Id, out var data)) return;
            if (data.Mode != NoClipMode.Fly) return;

            player.GetKeys(out var keys, out var ud, out var lr);

            if (data.LastLR == lr && data.LastUD == ud && Environment.TickCount - data.LastMove < 100)
                return;

            data.LastLR = lr;
            data.LastUD = ud;

            if (lr == 0 && ud == 0)
            {
                data.AccelMultiplier = 0;
                return;
            }

            var direction = GetDirection(ud, lr);
            data.Direction = direction;
            MoveCamera(player, data);
        }

        private static MoveDirection GetDirection(int ud, int lr)
        {
            if (lr < 0)
            {
                if (ud < 0) return MoveDirection.ForwardLeft;
                if (ud > 0) return MoveDirection.BackLeft;
                return MoveDirection.Left;
            }
            if (lr > 0)
            {
                if (ud < 0) return MoveDirection.ForwardRight;
                if (ud > 0) return MoveDirection.BackRight;
                return MoveDirection.Right;
            }
            if (ud < 0) return MoveDirection.Forward;
            if (ud > 0) return MoveDirection.Back;
            return MoveDirection.Forward;
        }

        private static void MoveCamera(Player player, NoClipData data)
        {
            if (data.AccelMultiplier <= 1) data.AccelMultiplier += AccelRate;

            var speed = MoveSpeed * data.AccelMultiplier;
            var cp = player.CameraPosition;
            var fv = player.CameraFrontVector;

            var next = GetNextPosition(data.Direction, cp, fv);
            data.FlyObject.Move(next, speed);
            data.LastMove = Environment.TickCount;
        }

        private static Vector3 GetNextPosition(MoveDirection dir, Vector3 cp, Vector3 fv)
        {
            const float offset = 6000.0f;
            var ox = fv.X * offset;
            var oy = fv.Y * offset;
            var oz = fv.Z * offset;

            return dir switch
            {
                MoveDirection.Forward => new Vector3(cp.X + ox, cp.Y + oy, cp.Z + oz),
                MoveDirection.Back => new Vector3(cp.X - ox, cp.Y - oy, cp.Z - oz),
                MoveDirection.Left => new Vector3(cp.X - oy, cp.Y + ox, cp.Z),
                MoveDirection.Right => new Vector3(cp.X + oy, cp.Y - ox, cp.Z),
                MoveDirection.ForwardLeft => new Vector3(cp.X + ox - oy, cp.Y + oy + ox, cp.Z + oz),
                MoveDirection.ForwardRight => new Vector3(cp.X + ox + oy, cp.Y + oy - ox, cp.Z + oz),
                MoveDirection.BackLeft => new Vector3(cp.X - ox - oy, cp.Y - oy + ox, cp.Z - oz),
                MoveDirection.BackRight => new Vector3(cp.X - ox + oy, cp.Y - oy - ox, cp.Z - oz),
                _ => cp
            };
        }

        public static void Cleanup(Player player)
        {
            if (Sessions.ContainsKey(player.Id))
                Stop(player);
        }
    }
}
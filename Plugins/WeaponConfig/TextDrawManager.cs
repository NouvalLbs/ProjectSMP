#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class TextDrawManager
    {
        private static readonly HashSet<int> InternalTextDrawIds = new();
        private static readonly Dictionary<int, HashSet<int>> InternalPlayerTextDrawIds = new();

        public static void Init()
        {
            InternalTextDrawIds.Clear();
            InternalPlayerTextDrawIds.Clear();
        }

        public static void RegisterInternalTextDraw(TextDraw td)
        {
            if (td != null) InternalTextDrawIds.Add(td.Id);
        }

        public static void RegisterInternalPlayerTextDraw(int playerId, PlayerTextDraw ptd)
        {
            if (ptd == null) return;
            if (!InternalPlayerTextDrawIds.ContainsKey(playerId))
                InternalPlayerTextDrawIds[playerId] = new HashSet<int>();
            InternalPlayerTextDrawIds[playerId].Add(ptd.Id);
        }

        public static void UnregisterInternalTextDraw(TextDraw td)
        {
            if (td != null) InternalTextDrawIds.Remove(td.Id);
        }

        public static void UnregisterInternalPlayerTextDraw(int playerId, PlayerTextDraw ptd)
        {
            if (ptd == null || !InternalPlayerTextDrawIds.TryGetValue(playerId, out var set)) return;
            set.Remove(ptd.Id);
        }

        public static void CleanupPlayer(int playerId)
        {
            InternalPlayerTextDrawIds.Remove(playerId);
        }

        public static bool IsInternalTextDraw(TextDraw? td)
        {
            return td != null && InternalTextDrawIds.Contains(td.Id);
        }

        public static bool IsInternalPlayerTextDraw(int playerId, PlayerTextDraw? ptd)
        {
            return ptd != null && InternalPlayerTextDrawIds.TryGetValue(playerId, out var set) && set.Contains(ptd.Id);
        }

        public static bool CanDestroy(TextDraw? td) => !IsInternalTextDraw(td);
        public static bool CanDestroy(int playerId, PlayerTextDraw? ptd) => !IsInternalPlayerTextDraw(playerId, ptd);
        public static bool CanModify(TextDraw? td) => !IsInternalTextDraw(td);
        public static bool CanModify(int playerId, PlayerTextDraw? ptd) => !IsInternalPlayerTextDraw(playerId, ptd);

        public static TextDraw CreateSafe(Vector2 position, string text)
        {
            return new TextDraw(position, text);
        }

        public static TextDraw CreateInternal(Vector2 position, string text)
        {
            var td = new TextDraw(position, text);
            RegisterInternalTextDraw(td);
            return td;
        }

        public static PlayerTextDraw CreatePlayerSafe(BasePlayer player, Vector2 position, string text)
        {
            return new PlayerTextDraw(player, position, text);
        }

        public static PlayerTextDraw CreatePlayerInternal(BasePlayer player, Vector2 position, string text)
        {
            var ptd = new PlayerTextDraw(player, position, text);
            RegisterInternalPlayerTextDraw(player.Id, ptd);
            return ptd;
        }

        public static bool DestroySafe(TextDraw? td)
        {
            if (td == null || !CanDestroy(td)) return false;
            td.Dispose();
            return true;
        }

        public static bool DestroySafe(int playerId, PlayerTextDraw? ptd)
        {
            if (ptd == null || !CanDestroy(playerId, ptd)) return false;
            ptd.Dispose();
            return true;
        }
    }
}
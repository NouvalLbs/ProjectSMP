#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.ColAndreas
{
    public static class ColAndreasDynamicObjectManager
    {
        private class Entry
        {
            public DynamicObject? DynamicObject;
            public int CollisionId = -1;
        }

        private static readonly Dictionary<int, Entry> Objects = new();
        private static int _counter;

        /// <summary>Static Collision — object created but not tracked (not removable by index)</summary>
        public static DynamicObject? CreateObject_SC(int modelid, Vector3 pos, Vector3 rot,
            float streamDistance = 200f, float drawDistance = 0f,
            int worldid = -1, int interiorid = -1, BasePlayer? player = null)
        {
            var obj = new DynamicObject(modelid, pos, rot, worldid, interiorid, player, streamDistance, drawDistance);
            ColAndreasService.CreateObject(modelid, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, false);
            return obj;
        }

        /// <summary>Dynamic Collision — tracked, removable via returned index</summary>
        public static int CreateObject_DC(int modelid, Vector3 pos, Vector3 rot,
            float streamDistance = 200f, float drawDistance = 0f,
            int worldid = -1, int interiorid = -1, BasePlayer? player = null)
        {
            var obj = new DynamicObject(modelid, pos, rot, worldid, interiorid, player, streamDistance, drawDistance);
            int colId = ColAndreasService.CreateObject(modelid, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, true);
            int index = _counter++;
            Objects[index] = new Entry { DynamicObject = obj, CollisionId = colId };
            return index;
        }

        public static bool DestroyObject(int index)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            e.DynamicObject?.Dispose();
            if (e.CollisionId >= 0) ColAndreasService.DestroyObject(e.CollisionId);
            Objects.Remove(index);
            return true;
        }

        public static bool SetObjectPos(int index, Vector3 pos)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            if (e.DynamicObject != null) e.DynamicObject.Position = pos;
            if (e.CollisionId >= 0) ColAndreasService.SetObjectPos(e.CollisionId, pos.X, pos.Y, pos.Z);
            return true;
        }

        public static bool SetObjectRot(int index, Vector3 rot)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            if (e.DynamicObject != null) e.DynamicObject.Rotation = rot;
            if (e.CollisionId >= 0) ColAndreasService.SetObjectRot(e.CollisionId, rot.X, rot.Y, rot.Z);
            return true;
        }

        public static void DestroyAll()
        {
            foreach (var key in Objects.Keys.ToList())
                DestroyObject(key);
        }

        public static int GetDynamicObjectId(int index) => Objects.TryGetValue(index, out var e) ? e.DynamicObject?.Id ?? -1 : -1;
        public static int GetCollisionId(int index) => Objects.TryGetValue(index, out var e) ? e.CollisionId : -1;
        public static bool IsValid(int index) => Objects.ContainsKey(index);
        public static DynamicObject? GetDynamicObject(int index)
            => Objects.TryGetValue(index, out var e) ? e.DynamicObject : null;
    }
}
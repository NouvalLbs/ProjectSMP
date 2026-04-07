using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.ColAndreas
{
    public static class ColAndreasObjectManager
    {
        private class Entry
        {
            public GlobalObject? GameObject;
            public int CollisionId = -1;
        }

        private static readonly Dictionary<int, Entry> Objects = new();
        private static int _counter;

        public static int CreateObject(int modelid, Vector3 pos, Vector3 rot, float drawDistance = 300f)
        {
            var go = new GlobalObject(modelid, pos, rot, drawDistance);
            if (go == null) return -1;

            int colId = ColAndreasService.CreateObject(modelid, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, true);
            int index = _counter++;
            Objects[index] = new Entry { GameObject = go, CollisionId = colId };
            return index;
        }

        public static bool DestroyObject(int index)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            e.GameObject?.Dispose();
            if (e.CollisionId >= 0) ColAndreasService.DestroyObject(e.CollisionId);
            Objects.Remove(index);
            return true;
        }

        public static bool SetObjectPos(int index, Vector3 pos)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            if (e.GameObject != null) e.GameObject.Position = pos;
            if (e.CollisionId >= 0) ColAndreasService.SetObjectPos(e.CollisionId, pos.X, pos.Y, pos.Z);
            return true;
        }

        public static bool SetObjectRot(int index, Vector3 rot)
        {
            if (!Objects.TryGetValue(index, out var e)) return false;
            if (e.GameObject != null) e.GameObject.Rotation = rot;
            if (e.CollisionId >= 0) ColAndreasService.SetObjectRot(e.CollisionId, rot.X, rot.Y, rot.Z);
            return true;
        }

        public static void DestroyAll()
        {
            foreach (var key in Objects.Keys.ToList())
                DestroyObject(key);
        }

        public static int GetGameObjectId(int index) => Objects.TryGetValue(index, out var e) ? e.GameObject?.Id ?? -1 : -1;
        public static int GetCollisionId(int index) => Objects.TryGetValue(index, out var e) ? e.CollisionId : -1;
        public static bool IsValid(int index) => Objects.ContainsKey(index);
    }
}
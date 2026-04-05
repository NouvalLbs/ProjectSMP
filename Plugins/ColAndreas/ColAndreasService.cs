using SampSharp.GameMode;

namespace ProjectSMP.Plugins.ColAndreas
{
    public static class ColAndreasService
    {
        public const int WaterObject = 20000;
        public const int MaxMulticastSize = 99;

        private static ColAndreasNatives N => ColAndreasNatives.Instance;

        public static int Init() => N.CA_Init();

        public static int RemoveBuilding(int modelid, float x, float y, float z, float radius)
            => N.CA_RemoveBuilding(modelid, x, y, z, radius);

        public static int RestoreBuilding(int modelid, float x, float y, float z, float radius)
            => N.CA_RestoreBuilding(modelid, x, y, z, radius);

        public static int RayCastLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLine(startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastLineID(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLineID(startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastLineExtraID(int type, float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLineExtraID(type, startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastMultiLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float[] retX, out float[] retY, out float[] retZ, out float[] retDist, out int[] modelIds, int size = MaxMulticastSize)
            => N.CA_RayCastMultiLine(startX, startY, startZ, endX, endY, endZ, out retX, out retY, out retZ, out retDist, out modelIds, size);

        public static int RayCastLineAngle(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz)
            => N.CA_RayCastLineAngle(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz);

        public static int RayCastReflectionVector(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz)
            => N.CA_RayCastReflectionVector(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out nx, out ny, out nz);

        public static int RayCastLineNormal(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz)
            => N.CA_RayCastLineNormal(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out nx, out ny, out nz);

        public static int ContactTest(int modelid, float x, float y, float z, float rx, float ry, float rz)
            => N.CA_ContactTest(modelid, x, y, z, rx, ry, rz);

        public static int EulerToQuat(float rx, float ry, float rz, out float x, out float y, out float z, out float w)
            => N.CA_EulerToQuat(rx, ry, rz, out x, out y, out z, out w);

        public static int QuatToEuler(float x, float y, float z, float w, out float rx, out float ry, out float rz)
            => N.CA_QuatToEuler(x, y, z, w, out rx, out ry, out rz);

        public static int GetModelBoundingSphere(int modelid, out float offX, out float offY, out float offZ, out float radius)
            => N.CA_GetModelBoundingSphere(modelid, out offX, out offY, out offZ, out radius);

        public static int GetModelBoundingBox(int modelid, out float minX, out float minY, out float minZ, out float maxX, out float maxY, out float maxZ)
            => N.CA_GetModelBoundingBox(modelid, out minX, out minY, out minZ, out maxX, out maxY, out maxZ);

        public static int SetObjectExtraID(int index, int type, int data)
            => N.CA_SetObjectExtraID(index, type, data);

        public static int GetObjectExtraID(int index, int type)
            => N.CA_GetObjectExtraID(index, type);

        public static int RayCastLineEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float rw, out float cx, out float cy, out float cz)
            => N.CA_RayCastLineEx(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz, out rw, out cx, out cy, out cz);

        public static int RayCastLineAngleEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float ocX, out float ocY, out float ocZ, out float orX, out float orY, out float orZ)
            => N.CA_RayCastLineAngleEx(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz, out ocX, out ocY, out ocZ, out orX, out orY, out orZ);

        public static int LoadFromDff(int newid, string dffFileName)
            => N.CA_LoadFromDff(newid, dffFileName);

        public static int CreateObject(int modelid, float x, float y, float z, float rx, float ry, float rz, bool add = false)
            => N.CA_CreateObject(modelid, x, y, z, rx, ry, rz, add);

        public static int DestroyObject(int index) => N.CA_DestroyObject(index);

        public static int IsValidObject(int index) => N.CA_IsValidObject(index);

        public static int SetObjectPos(int index, float x, float y, float z)
            => N.CA_SetObjectPos(index, x, y, z);

        public static int SetObjectRot(int index, float rx, float ry, float rz)
            => N.CA_SetObjectRot(index, rx, ry, rz);

        public static bool FindZFor2DCoord(float x, float y, out float z)
        {
            z = 0f;
            return RayCastLine(x, y, 700f, x, y, -1000f, out x, out _, out z) != 0;
        }

        public static bool IsPlayerOnSurface(Vector3 position, float tolerance = 1.5f)
        {
            return RayCastLine(position.X, position.Y, position.Z, position.X, position.Y, position.Z - tolerance, out _, out _, out _) != 0;
        }

        public static bool IsVehicleOnSurface(Vector3 position, float tolerance = 1.5f)
            => IsPlayerOnSurface(position, tolerance);
    }
}
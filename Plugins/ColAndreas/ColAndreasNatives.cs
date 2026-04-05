using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.ColAndreas
{
    public class ColAndreasNatives : NativeObjectSingleton<ColAndreasNatives>
    {
        [NativeMethod] public virtual int CA_Init() => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RemoveBuilding(int modelid, float x, float y, float z, float radius) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RestoreBuilding(int modelid, float x, float y, float z, float radius) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineID(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineExtraID(int type, float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z) => throw new NativeNotImplementedException();

        [NativeMethod(11, 11, 11, 11, 11)]
        public virtual int CA_RayCastMultiLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float[] retX, out float[] retY, out float[] retZ, out float[] retDist, out int[] modelIds, int size) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineAngle(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastReflectionVector(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineNormal(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_ContactTest(int modelid, float x, float y, float z, float rx, float ry, float rz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_EulerToQuat(float rx, float ry, float rz, out float x, out float y, out float z, out float w) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_QuatToEuler(float x, float y, float z, float w, out float rx, out float ry, out float rz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_GetModelBoundingSphere(int modelid, out float offX, out float offY, out float offZ, out float radius) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_GetModelBoundingBox(int modelid, out float minX, out float minY, out float minZ, out float maxX, out float maxY, out float maxZ) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_SetObjectExtraID(int index, int type, int data) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_GetObjectExtraID(int index, int type) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float rw, out float cx, out float cy, out float cz) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_RayCastLineAngleEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float ocX, out float ocY, out float ocZ, out float orX, out float orY, out float orZ) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_LoadFromDff(int newid, string dffFileName) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_CreateObject(int modelid, float x, float y, float z, float rx, float ry, float rz, bool add = false) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_DestroyObject(int index) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_IsValidObject(int index) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_SetObjectPos(int index, float x, float y, float z) => throw new NativeNotImplementedException();

        [NativeMethod] public virtual int CA_SetObjectRot(int index, float rx, float ry, float rz) => throw new NativeNotImplementedException();
    }
}
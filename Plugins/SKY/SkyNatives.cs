using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.SKY
{
    public class SkyNatives : NativeObjectSingleton<SkyNatives>
    {
        [NativeMethod] public virtual int SetMaxInvalidPacketsThreshold(int threshold) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int GetMaxInvalidPacketsThreshold() => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetInvalidPacketTimeout(int durationMs) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int GetInvalidPacketTimeout() => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetPacketRateLimit(int maxPackets, int timeWindowMs) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int GetPacketRateLimit(out int maxPackets, out int timeWindowMs) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int ResetPacketRateLimits() => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int FreezeSyncPacket(int playerid, int type, bool toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SendLastSyncPacket(int playerid, int toplayerid, int type, int animation) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetDisableSyncBugs(int toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetInfiniteAmmoSync(int playerid, int toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetKeySyncBlocked(int playerid, int toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetKnifeSync(int toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetFakeHealth(int playerid, int health) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetFakeArmour(int playerid, int armour) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetFakeFacingAngle(int playerid, float angle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SpawnPlayerForWorld(int playerid) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SendDeath(int playerid) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SetLastAnimationData(int playerid, int data) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int ClearAnimationsForPlayer(int playerid, int forplayerid) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int TextDrawSetPosition(int text, float x, float y) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int PlayerTextDrawSetPosition(int playerid, int text, float x, float y) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int TextDrawSetStrForPlayer(int text, int playerid, string str) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int FreezeSyncData(int playerid, bool toggle) => throw new NativeNotImplementedException();
        [NativeMethod] public virtual int SendLastSyncData(int playerid, int toplayerid, int animation) => throw new NativeNotImplementedException();
    }
}
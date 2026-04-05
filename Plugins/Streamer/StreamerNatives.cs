using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.Streamer {
    public class StreamerNatives : NativeObjectSingleton<StreamerNatives> {
        [NativeMethod]
        public virtual int Streamer_Update(int playerid, int type = -1) {
            throw new NativeNotImplementedException();
        }

        [NativeMethod]
        public virtual int Streamer_UpdateEx(int playerid,
            float x, float y, float z,
            int virtualworld = -1, int interior = -1,
            int type = -1, int compensatedtime = -1, int freezeplayer = 0)
        {
            throw new NativeNotImplementedException();
        }

        [NativeMethod]
        public virtual int Streamer_SetVisibleItems(int type, int items, int playerid = -1) {
            throw new NativeNotImplementedException();
        }

        [NativeMethod]
        public virtual int Streamer_SetRadiusMultiplier(int type, float multiplier, int playerid = -1) {
            throw new NativeNotImplementedException();
        }
    }
}
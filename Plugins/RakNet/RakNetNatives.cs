using SampSharp.Core.Natives.NativeObjects;

namespace ProjectSMP.Plugins.RakNet
{
    public class RakNetNatives : NativeObjectSingleton<RakNetNatives>
    {
        [NativeMethod]
        public virtual int CallRemoteFunction(string function, string format, params object[] args)
            => throw new NativeNotImplementedException();
    }
}
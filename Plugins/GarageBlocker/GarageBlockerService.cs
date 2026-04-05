using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;

namespace ProjectSMP.Plugins.GarageBlocker {
    public static class GarageBlockerService {
        private static readonly (float X, float Y, float Z, float R)[] Locations = {
            (  1968.74219f,   2162.49219f,  12.09380f,    0f ),
            (  2006.00000f,   2303.72656f,  11.31250f,   90f ),
            (  2006.00000f,   2317.60156f,  11.31250f,  -90f ),
            (   720.01563f,   -462.52341f,  16.85940f,   90f ),
            (  -100.00000f,   1111.41406f,  21.14060f,   90f ),
            ( -1420.52686f,   2591.15747f,  57.24220f,  -90f ),
            (  1843.36719f,  -1856.32031f,  13.87500f,    0f ),
            ( -2716.35156f,    217.47659f,   5.38280f,  180f ),
            ( -1935.85938f,    239.53130f,  35.35160f,   90f ),
            ( -1904.53125f,    277.89841f,  42.45310f,   90f ),
            ( -1786.81250f,   1209.42188f,  25.83590f,   90f ),
            (  1798.68750f,  -2146.73438f,  14.00000f,    0f ),
            (  2644.85938f,  -2039.23438f,  14.03910f,  -90f ),
            (  2071.47656f,  -1831.42188f,  14.56250f,  180f ),
            (   488.28131f,  -1734.69531f,  12.39060f, -101f ),
            ( -2425.72656f,   1027.99219f,  51.78130f,  -90f ),
            (  2393.76563f,   1483.68750f,  12.21090f,   90f ),
            (  2386.65625f,   1043.60156f,  11.59380f,   90f ),
            (  1025.01672f,  -1029.21533f,  33.11600f,   90f ),
            (  1041.35254f,  -1025.90967f,  32.67188f,   90f ),
        };

        public static void Init() {
            foreach (var loc in Locations) {
                var obj = new DynamicObject(19325,
                    new Vector3(loc.X, loc.Y, loc.Z),
                    new Vector3(0f, 0f, loc.R));

                obj.SetMaterialText(0, "DISABLED",
                    ObjectMaterialSize.X128X128,
                    "Arial", 16, true,
                    Color.Red,
                    new Color(0, 0, 0, 0),
                    ObjectMaterialTextAlign.Center);
            }
        }
    }
}
using System;

namespace ProjectSMP.Plugins.RealtimeClock
{
    public class WorldTimeUpdateEventArgs : EventArgs
    {
        public int Hour { get; }
        public int Minute { get; }
        public int Second { get; }

        public WorldTimeUpdateEventArgs(int hour, int minute, int second = 0)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
        }
    }
}
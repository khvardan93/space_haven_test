using System;

namespace Save
{
    [Serializable]
    public sealed class GameState
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public double[] Balances = new double[0];
        public RoomState[] Rooms = new RoomState[0];
        public int LaserCurrent;
        public double LaserTimer;
        public long LastSaveUtcTicks;

        public DateTime LastSaveUtc => new(LastSaveUtcTicks, DateTimeKind.Utc);
    }
}

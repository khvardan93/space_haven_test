using System;

namespace Save
{
    /// <summary>
    /// Plain serializable snapshot of the game. Public fields so Unity's JsonUtility can write it.
    /// Writing to disk lives in the Services layer; the model only defines the shape.
    /// </summary>
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

        public DateTime LastSaveUtc
        {
            get { return new DateTime(LastSaveUtcTicks, DateTimeKind.Utc); }
        }
    }

    [Serializable]
    public sealed class RoomState
    {
        public int Slot;
        public string SpecId;
        public int Level;
        public double Progress;
        public double BoostRemaining;
        public double BoostMultiplier;
    }
}

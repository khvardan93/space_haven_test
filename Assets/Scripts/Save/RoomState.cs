using System;

namespace Save
{
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

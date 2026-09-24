using System;
using System.Collections.Generic;
using Configs;
using Economy;

namespace Core
{
    public sealed class GameSetup
    {
        public int SlotCount { get; set; } = 8;
        public IReadOnlyList<Resource> StartingBalances { get; set; } = new List<Resource>();
        public LaserSettings Laser { get; set; } = new LaserSettings();
        public IReadOnlyList<RoomConfigs> Rooms { get; set; } = new List<RoomConfigs>();
        public TimeSpan OfflineCap { get; set; } = TimeSpan.FromHours(2);
    }
}

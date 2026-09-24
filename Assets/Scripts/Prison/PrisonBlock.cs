using System;
using System.Collections.Generic;
using Configs;
using Economy;
using Laser;

namespace Prison
{
    /// <summary>
    /// The cellblock: a fixed number of slots, each empty or holding a room.
    /// Every action validates resources and laser energy before spending either, so a failure never spends half.
    /// Can* and Try* share the same validation so the UI and the logic always agree.
    /// </summary>
    public sealed class PrisonBlock
    {
        private readonly Room[] _slots;
        private readonly List<Room> _rooms = new List<Room>();
        private readonly GameEconomy _economy;
        private readonly LaserEnergy _laser;
        private readonly LaserSettings _laserSpec;

        public int SlotCount
        {
            get { return _slots.Length; }
        }

        /// <summary>Built rooms in build order. Used by the simulation loop, so no allocation per tick.</summary>
        public IReadOnlyList<Room> Rooms => _rooms; 

        /// <summary>Raised when a room is built, upgraded or boosted in a slot.</summary>
        public event Action<int> SlotChanged;
        /// <summary>Raised when a new room object exists, so systems can subscribe to its events.</summary>
        public event Action<Room> RoomAdded;

        public PrisonBlock(int slotCount, GameEconomy economy, LaserEnergy laser, LaserSettings laserSpec)
        {
            if (slotCount < 1) throw new ArgumentOutOfRangeException(nameof(slotCount));
            if (economy == null) throw new ArgumentNullException(nameof(economy));
            if (laser == null) throw new ArgumentNullException(nameof(laser));

            _slots = new Room[slotCount];
            _economy = economy;
            _laser = laser;
            _laserSpec = laserSpec;
        }

        public bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < _slots.Length;
        }

        public Room GetRoom(int slot)
        {
            return IsValidSlot(slot) ? _slots[slot] : null;
        }

        public int CountOf(string specId)
        {
            int count = 0;
            foreach (var room in _rooms)
            {
                if (room.Spec.Id == specId)
                    count++;
            }
            return count;
        }

        // ---------- Build ----------

        public ActionResult CanBuild(int slot, RoomConfigs spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (!IsValidSlot(slot)) return ActionResult.InvalidSlot;
            if (_slots[slot] != null) return ActionResult.Occupied;
            return CheckCosts(spec.BuildCost, spec.LaserBuildCost);
        }

        public ActionResult TryBuild(int slot, RoomConfigs spec)
        {
            var result = CanBuild(slot, spec);
            if (result != ActionResult.Ok)
                return result;

            Pay(spec.BuildCost, spec.LaserBuildCost);
            PlaceRoom(slot, new Room(spec));
            return ActionResult.Ok;
        }

        // ---------- Upgrade ----------

        public ActionResult CanUpgrade(int slot)
        {
            if (!IsValidSlot(slot)) return ActionResult.InvalidSlot;
            var room = _slots[slot];
            if (room == null) return ActionResult.Empty;
            if (room.IsMaxLevel) return ActionResult.MaxLevel;
            return CheckCosts(room.Spec.GetUpgradeCost(room.Level), room.Spec.LaserUpgradeCost);
        }

        public ActionResult TryUpgrade(int slot)
        {
            var result = CanUpgrade(slot);
            if (result != ActionResult.Ok)
                return result;

            var room = _slots[slot];
            Pay(room.Spec.GetUpgradeCost(room.Level), room.Spec.LaserUpgradeCost);
            room.SetLevel(room.Level + 1);
            RaiseSlotChanged(slot);
            return ActionResult.Ok;
        }

        /// <summary>Upgrade cost for the room in this slot, or null when empty or maxed. For the popup.</summary>
        public Resource[] GetUpgradeCost(int slot)
        {
            var room = GetRoom(slot);
            if (room == null || room.IsMaxLevel)
                return null;
            return room.Spec.GetUpgradeCost(room.Level);
        }

        // ---------- Boost ----------

        public ActionResult CanBoost(int slot)
        {
            if (!IsValidSlot(slot)) return ActionResult.InvalidSlot;
            if (_slots[slot] == null) return ActionResult.Empty;
            return _laser.CanSpend(_laserSpec.BoostCost) ? ActionResult.Ok : ActionResult.NotEnoughLaser;
        }

        public ActionResult TryBoost(int slot)
        {
            var result = CanBoost(slot);
            if (result != ActionResult.Ok)
                return result;

            _laser.TrySpend(_laserSpec.BoostCost);
            _slots[slot].ApplyBoost(_laserSpec.BoostDuration, _laserSpec.BoostMultiplier);
            RaiseSlotChanged(slot);
            return ActionResult.Ok;
        }

        // ---------- Internals ----------

        /// <summary>Puts a room into a slot without paying. Used by TryBuild and by save loading.</summary>
        internal void PlaceRoom(int slot, Room room)
        {
            if (!IsValidSlot(slot)) throw new ArgumentOutOfRangeException(nameof(slot));
            if (_slots[slot] != null) throw new InvalidOperationException("Slot " + slot + " is occupied.");

            _slots[slot] = room;
            _rooms.Add(room);

            var added = RoomAdded;
            if (added != null) added(room);
            RaiseSlotChanged(slot);
        }

        internal int IndexOf(Room room)
        {
            return Array.IndexOf(_slots, room);
        }

        private ActionResult CheckCosts(IReadOnlyList<Resource> cost, int laserCost)
        {
            if (!_economy.CanAfford(cost)) return ActionResult.NotEnoughResources;
            if (!_laser.CanSpend(laserCost)) return ActionResult.NotEnoughLaser;
            return ActionResult.Ok;
        }

        private void Pay(IReadOnlyList<Resource> cost, int laserCost)
        {
            // Both were validated just before, so neither can fail here.
            _economy.TrySpend(cost);
            _laser.TrySpend(laserCost);
        }

        private void RaiseSlotChanged(int slot)
        {
            var handler = SlotChanged;
            if (handler != null) handler(slot);
        }
    }
}

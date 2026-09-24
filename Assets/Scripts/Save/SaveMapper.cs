using System;
using System.Collections.Generic;
using Configs;
using Core;
using Prison;

namespace Save
{
    /// <summary>Converts between the live model and GameState. Restore is forgiving: bad entries are skipped and reported.</summary>
    public static class SaveMapper
    {
        public static GameState Capture(GameModel model, DateTime nowUtc)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var rooms = new List<RoomState>();
            for (int slot = 0; slot < model.Prison.SlotCount; slot++)
            {
                var room = model.Prison.GetRoom(slot);
                if (room == null)
                    continue;

                rooms.Add(new RoomState
                {
                    Slot = slot,
                    SpecId = room.Spec.Id,
                    Level = room.Level,
                    Progress = room.Progress,
                    BoostRemaining = room.BoostRemaining,
                    BoostMultiplier = room.BoostMultiplier
                });
            }

            return new GameState
            {
                Version = GameState.CurrentVersion,
                Balances = model.Economy.Snapshot(),
                Rooms = rooms.ToArray(),
                LaserCurrent = model.Laser.Current,
                LaserTimer = model.Laser.RegenTimer,
                LastSaveUtcTicks = nowUtc.ToUniversalTime().Ticks
            };
        }

        /// <summary>
        /// Applies a saved state to a freshly created model (empty prison).
        /// Returns warnings for anything that could not be restored, e.g. a room type removed in an update.
        /// </summary>
        public static List<string> Restore(GameState state, GameModel model)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (model == null) throw new ArgumentNullException(nameof(model));

            var warnings = new List<string>();

            if (state.Version > GameState.CurrentVersion)
                warnings.Add("Save version " + state.Version + " is newer than supported " + GameState.CurrentVersion + ".");
            // Future: migrate older versions here before reading fields.

            model.Economy.SetAll(state.Balances);
            model.Laser.RestoreState(state.LaserCurrent, state.LaserTimer);

            if (state.Rooms == null)
                return warnings;

            foreach (var saved in state.Rooms)
            {
                if (saved == null)
                    continue;

                if (!model.Catalog.TryGet(saved.SpecId, out var spec))
                {
                    warnings.Add("Unknown room '" + saved.SpecId + "' in slot " + saved.Slot + ", skipped.");
                    continue;
                }
                if (!model.Prison.IsValidSlot(saved.Slot) || model.Prison.GetRoom(saved.Slot) != null)
                {
                    warnings.Add("Slot " + saved.Slot + " is invalid or taken, skipped '" + saved.SpecId + "'.");
                    continue;
                }

                var room = new Room(spec, saved.Level);
                room.RestoreState(saved.Progress, saved.BoostRemaining, saved.BoostMultiplier);
                model.Prison.PlaceRoom(saved.Slot, room);
            }

            return warnings;
        }
    }
}

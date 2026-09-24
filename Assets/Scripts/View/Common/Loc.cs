using System.Collections.Generic;

namespace View
{
    /// <summary>
    /// Minimal text lookup. Every visible string goes through a key, so switching to
    /// Unity Localization (MVP: 5 languages) means replacing this class, not touching the views.
    /// </summary>
    public static class Loc
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            // Resources
            { "res_dark_matter", "Dark Matter" },
            { "res_dark_matter_short", "DM" },
            { "res_glowing_plasma", "Glowing Plasma" },
            { "res_glowing_plasma_short", "PL" },
            { "res_kordium", "Kordium Crystals" },
            { "res_kordium_short", "KC" },
            { "laser_energy", "Laser Energy" },
            { "laser_energy_short", "LE" },

            // Rooms
            { "room_prison_cell", "Prison Cell" },
            { "room_prison_cell_desc", "Inmates mine Dark Matter." },
            { "room_energy_room", "Energy Room" },
            { "room_energy_room_desc", "Generates Glowing Plasma." },
            { "room_refinery", "Kordium Refinery" },
            { "room_refinery_desc", "Fuses Dark Matter and Plasma into Kordium Crystals." },

            // Slot states
            { "slot_empty", "Empty cell" },
            { "slot_tap_to_build", "Tap to build" },
            { "slot_starved", "NO INPUT" },
            { "slot_level", "Lv {0}" },

            // Popup
            { "popup_build_title", "Build" },
            { "popup_upgrade", "Upgrade" },
            { "popup_boost", "Boost x{0} for {1}s" },
            { "popup_max_level", "Max level" },
            { "popup_output_now", "Now: {0}" },
            { "popup_output_next", "Next: {0}" },
            { "popup_cost", "Cost: {0}" },

            // Action results
            { "result_ok", "" },
            { "result_not_enough_resources", "Not enough resources" },
            { "result_not_enough_laser", "Not enough Laser Energy" },
            { "result_max_level", "Already at max level" },
            { "result_occupied", "Cell is occupied" },
            { "result_empty", "Cell is empty" },
            { "result_invalid_slot", "Invalid cell" }
        };

        public static string Get(string key)
        {
            if (key != null && English.TryGetValue(key, out var value))
                return value;
            return key ?? string.Empty;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}

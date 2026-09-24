using System.Collections.Generic;
using Configs;
using Economy;
using UnityEngine;

namespace View
{
    /// <summary>
    /// Finds the ScriptableObject behind a model id, for icons and colors.
    /// The model only knows ids and numbers; presentation data stays on this side.
    /// </summary>
    public sealed class ContentLookup
    {
        private readonly Dictionary<string, RoomConfigs> _rooms = new ();
        private readonly ResourceConfigs[] _resources = new ResourceConfigs[ResourceTypes.Count];

        public ContentLookup(GameConfigs config)
        {
            if (config.Rooms != null)
            {
                foreach (var room in config.Rooms)
                {
                    if (room != null)
                        _rooms[room.Id] = room;
                }
            }

            if (config.Resources != null)
            {
                foreach (var resource in config.Resources)
                {
                    if (resource != null)
                        _resources[(int)resource.Type] = resource;
                }
            }
        }

        public RoomConfigs GetRoom(string id)
        {
            return id != null && _rooms.TryGetValue(id, out var room) ? room : null;
        }

        public ResourceConfigs GetResource(ResourceType type)
        {
            return _resources[(int)type];
        }

        public Color GetColor(ResourceType type)
        {
            var resource = GetResource(type);
            return resource != null ? resource.Color : Color.white;
        }

        public string GetShortName(ResourceType type)
        {
            var resource = GetResource(type);
            return resource != null ? Loc.Get(resource.DisplayName + "_short") : type.ToString();
        }

        public string GetName(ResourceType type)
        {
            var resource = GetResource(type);
            return resource != null ? Loc.Get(resource.DisplayName) : type.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using Configs;

namespace Prison
{
    /// <summary>All room types available in the game, looked up by id (used by the build menu and save loading).</summary>
    public sealed class RoomCatalog
    {
        private readonly Dictionary<string, RoomConfigs> _byId = new ();
        private readonly List<RoomConfigs> _all = new ();

        public IReadOnlyList<RoomConfigs> All => _all;
        
        public RoomCatalog(IReadOnlyList<RoomConfigs> specs)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            foreach (var spec in specs)
            {
                var errors = spec.Validate();
                if (errors.Count > 0)
                    throw new ArgumentException("Invalid room spec: " + string.Join(" ", errors.ToArray()));
                if (_byId.ContainsKey(spec.Id))
                    throw new ArgumentException("Duplicate room id: " + spec.Id);

                _byId.Add(spec.Id, spec);
                _all.Add(spec);
            }
        }

        public bool TryGet(string id, out RoomConfigs spec)
        {
            if (id == null)
            {
                spec = null;
                return false;
            }
            return _byId.TryGetValue(id, out spec);
        }

        public RoomConfigs Get(string id)
        {
            RoomConfigs spec;
            if (!TryGet(id, out spec))
                throw new KeyNotFoundException("Unknown room id: " + id);
            return spec;
        }
    }
}

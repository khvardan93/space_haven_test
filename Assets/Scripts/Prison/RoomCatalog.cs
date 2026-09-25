using System;
using System.Collections.Generic;
using Configs;

namespace Prison
{
    public sealed class RoomCatalog
    {
        private readonly Dictionary<string, RoomConfigs> _byId = new();
        private readonly List<RoomConfigs> _all = new();

        public IReadOnlyList<RoomConfigs> All => _all;
        
        public RoomCatalog(IReadOnlyList<RoomConfigs> specs)
        {
            if (specs == null)
                throw new ArgumentNullException(nameof(specs));

            foreach (var spec in specs)
            {
                var errors = spec.Validate();
                if (errors.Count > 0)
                    throw new ArgumentException($"Invalid room spec: {string.Join(" ", errors.ToArray())}");
                if (!_byId.TryAdd(spec.Id, spec))
                    throw new ArgumentException($"Duplicate room id: {spec.Id}");

                _all.Add(spec);
            }
        }

        public bool TryGet(string id, out RoomConfigs spec)
        {
            if (id != null) return _byId.TryGetValue(id, out spec);
            spec = null;
            return false;
        }

        public RoomConfigs Get(string id)
        {
            return !TryGet(id, out var spec) ? throw new KeyNotFoundException($"Unknown room id: {id}") : spec;
        }
    }
}

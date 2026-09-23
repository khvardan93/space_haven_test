using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    [Serializable]
    public struct LaserSettings
    {
        [SerializeField] private int _laserMax;
        [SerializeField] private int _laserStart;
        [SerializeField] private float _laserRegenSeconds;
        [SerializeField] private int _boostCost;
        [SerializeField] private float _boostDuration;
        [SerializeField] private float _boostMultiplier;

        public int LaserMax => _laserMax;
        public int LaserStart => _laserStart;
        public float LaserRegenSeconds => _laserRegenSeconds;
        public int BoostCost => _boostCost;
        public float BoostDuration => _boostDuration;
        public float BoostMultiplier => _boostMultiplier;
    }

    [CreateAssetMenu(fileName = "GameConfig", menuName = "Project/Config/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Content")] [SerializeField] private ResourceDefinition[] _resources;
        [SerializeField] private RoomDefinition[] _rooms;

        [Header("Starting Balances")]
        [Range(1, 30)] [SerializeField] private int _slotCount = 8;

        [SerializeField] private ResourceAmount[] _startingBalances;

        [Header("Laser")] 
        [SerializeField] private LaserSettings _laserSettings;

        [Header("Offline")] 
        [SerializeField] private float _offlineCapHours = 2f;

        public IReadOnlyList<ResourceDefinition> Resources => _resources;
        public IReadOnlyList<RoomDefinition> Rooms => _rooms;

        public int SlotCount => _slotCount;
        public IReadOnlyList<ResourceAmount> StartingBalances => _startingBalances;

        public LaserSettings LaserSettings => _laserSettings;

        public float OfflineCapHours => _offlineCapHours;

        private void OnValidate()
        {
            var seenResourceTypes = new HashSet<string>();
            if (_resources != null)
            {
                foreach (var resource in _resources)
                {
                    if (resource == null)
                    {
                        Debug.LogError($"{name}: null entry in Resources.", this);
                        continue;
                    }

                    if (string.IsNullOrEmpty(resource.Id))
                    {
                        Debug.LogError($"{name}: resource '{resource.name}' has a missing ResourceType.", this);
                    }
                    else if (!seenResourceTypes.Add(resource.Id))
                    {
                        Debug.LogError($"{name}: duplicate ResourceType '{resource.Id}' in Resources.", this);
                    }
                }
            }

            var seenRoomIds = new HashSet<string>();
            if (_rooms != null)
            {
                foreach (var room in _rooms)
                {
                    if (room == null)
                    {
                        Debug.LogError($"{name}: null entry in Rooms.", this);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(room.Id) && !seenRoomIds.Add(room.Id))
                    {
                        Debug.LogError($"{name}: duplicate room Id '{room.Id}' in Rooms.", this);
                    }
                }
            }

            if (_startingBalances != null)
            {
                foreach (var balance in _startingBalances)
                {
                    if (balance.Resource == null)
                    {
                        Debug.LogError($"{name}: null Resource in StartingBalances entry.", this);
                    }
                }
            }

            if (_laserSettings.LaserStart > _laserSettings.LaserMax)
            {
                Debug.LogError($"{name}: LaserStart ({_laserSettings.LaserStart}) exceeds LaserMax ({_laserSettings.LaserMax}).", this);
            }
        }
    }
}

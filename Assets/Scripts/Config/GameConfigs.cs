using System;
using System.Collections.Generic;
using Core;
using Economy;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Project/Config/Game Config")]
    public class GameConfigs : ScriptableObject
    {
        [Header("Content")]
        [SerializeField] private ResourceConfigs[] _resources;
        [SerializeField] private RoomConfigs[] _rooms;

        [Header("Starting Balances")]
        [Range(1, 30)] [SerializeField] private int _slotCount = 8;

        [SerializeField] private ResourceAmount[] _startingBalances;

        [Header("Laser")]
        [SerializeField] private LaserSettings _laserSettings;

        [Header("Offline")]
        [SerializeField] private float _offlineCapHours = 2f;

        public IReadOnlyList<ResourceConfigs> Resources => _resources;
        
        public IReadOnlyList<RoomConfigs> Rooms => _rooms;

        public int SlotCount => _slotCount;
        
        public IReadOnlyList<ResourceAmount> StartingBalances => _startingBalances;

        public LaserSettings LaserSettings => _laserSettings;

        public float OfflineCapHours => _offlineCapHours;

        public GameSetup ToSetup()
        {
            return new GameSetup
            {
                SlotCount = SlotCount,
                StartingBalances = ResourceAmount.ToModel(_startingBalances),
                Laser = LaserSettings,
                Rooms = Rooms,
                OfflineCap = TimeSpan.FromHours(OfflineCapHours)
            };
        }
        
        private void OnValidate()
        {
            var seenResourceTypes = new HashSet<ResourceTypeEnum>();
            if (_resources != null)
            {
                foreach (var resource in _resources)
                {
                    if (resource == null)
                    {
                        Debug.LogError($"{name}: null entry in Resources.", this);
                        continue;
                    }

                    if (!seenResourceTypes.Add(resource.TypeEnum))
                    {
                        Debug.LogError($"{name}: duplicate ResourceType '{resource.TypeEnum}' in Resources.", this);
                    }

                    foreach (var error in resource.Validate())
                    {
                        Debug.LogError($"{name}: {error}", resource);
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

            if (_laserSettings.LaserStart > _laserSettings.Max)
            {
                Debug.LogError(
                    $"{name}: LaserStart ({_laserSettings.LaserStart}) exceeds LaserMax ({_laserSettings.Max}).", this);
            }
        }
    }
}

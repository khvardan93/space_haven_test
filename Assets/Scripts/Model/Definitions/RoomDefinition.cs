using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "RoomDefinition", menuName = "Project/Definitions/Room Definition")]
    public class RoomDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;

        [Header("Build Cost")]
        [SerializeField] private ResourceAmount[] _cost;
        [SerializeField] private float _laserCost;

        [Header("Production")]
        [SerializeField] private ResourceAmount[] _inputs;
        [SerializeField] private ResourceAmount[] _outputs;
        [SerializeField] private float _cycleTime;

        [Header("Upgrades")]
        [SerializeField] private int _maxLevel;
        [SerializeField] private ResourceAmount[] _upgradeBaseCost;
        [SerializeField] private float _upgradeCostGrowth;
        [SerializeField] private float _outputGrowthPerLevel;
        [SerializeField] private float _laserUpgradeCost;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;

        public IReadOnlyList<ResourceAmount> Cost => _cost;
        public float LaserCost => _laserCost;

        public IReadOnlyList<ResourceAmount> Inputs => _inputs;
        public IReadOnlyList<ResourceAmount> Outputs => _outputs;
        public float CycleTime => _cycleTime;

        public int MaxLevel => _maxLevel;
        public IReadOnlyList<ResourceAmount> UpgradeBaseCost => _upgradeBaseCost;
        public float UpgradeCostGrowth => _upgradeCostGrowth;
        public float OutputGrowthPerLevel => _outputGrowthPerLevel;
        public float LaserUpgradeCost => _laserUpgradeCost;
    }
}

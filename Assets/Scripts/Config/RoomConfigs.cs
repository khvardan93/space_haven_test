using System;
using System.Collections.Generic;
using Economy;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "RoomDefinition", menuName = "Project/Definitions/Room Definition")]
    public class RoomConfigs : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color;

        [Header("Build Cost")]
        [SerializeField] private ResourceAmount[] _cost;
        [SerializeField] private int _laserCost;

        [Header("Production")]
        [SerializeField] private ResourceAmount[] _inputs;
        [SerializeField] private ResourceAmount[] _outputs;
        [SerializeField] private float _cycleTime;

        [Header("Upgrades")]
        [SerializeField] private int _maxLevel;
        [SerializeField] private ResourceAmount[] _upgradeBaseCost;
        [SerializeField] private float _upgradeCostGrowth;
        [SerializeField] private float _outputGrowthPerLevel;
        [SerializeField] private int _laserUpgradeCost;

        private readonly List<Resource> _buildCost = new();
        private readonly List<Resource> _inputResources = new();
        private readonly List<Resource> _outputResources = new();
        private readonly List<Resource> _upgradeBaseCostResources = new();
        
        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Color Color => _color;
        public float CycleTime => _cycleTime;
        public int MaxLevel => _maxLevel;
        public float UpgradeCostGrowth => _upgradeCostGrowth;
        public float OutputGrowthPerLevel => _outputGrowthPerLevel;
        public int LaserUpgradeCost => _laserUpgradeCost;

        public IReadOnlyList<Resource> BuildCost
        {
            get
            {
                if (_cost.Length != _buildCost.Count)
                {
                    foreach (var cost in _cost)
                    {
                        _buildCost.Add(new Resource(cost.Resource.TypeEnum, cost.Amount));
                    }
                }

                return _buildCost;
            }
        }

        public int LaserBuildCost => _laserCost;

        public IReadOnlyList<Resource> Inputs
        {
            get
            {
                if (_inputs.Length != _inputResources.Count)
                {
                    foreach (var input in _inputs)
                    {
                        _inputResources.Add(new Resource(input.Resource.TypeEnum, input.Amount));
                    }
                }

                return _inputResources;
            }
        }
        
        public IReadOnlyList<Resource> Outputs
        {
            get
            {
                if (_outputs.Length != _outputResources.Count)
                {
                    foreach (var output in _outputs)
                    {
                        _outputResources.Add(new Resource(output.Resource.TypeEnum, output.Amount));
                    }
                }

                return _outputResources;
            }
        }
        
        public IReadOnlyList<Resource> UpgradeBaseCost  {
            get
            {
                if (_upgradeBaseCost.Length != _upgradeBaseCostResources.Count)
                {
                    foreach (var cost in _upgradeBaseCost)
                    {
                        _upgradeBaseCostResources.Add(new Resource(cost.Resource.TypeEnum, cost.Amount));
                    }
                }

                return _upgradeBaseCostResources;
            }
        }
        
        public Resource[] GetUpgradeCost(int currentLevel)
        {
            if (currentLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(currentLevel));

            var factor = Math.Pow(UpgradeCostGrowth, currentLevel - 1);
            return Resource.Scale(UpgradeBaseCost, factor);
        }
        
        public double GetOutputMultiplier(int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level));

            return 1 + OutputGrowthPerLevel * (level - 1);
        }
        
        public List<string> Validate()
        {
            var errors = new List<string>();
            var label = string.IsNullOrEmpty(Id) ? "<no id>" : Id;

            if (string.IsNullOrWhiteSpace(Id)) errors.Add("Room has no Id.");
            if (_cycleTime <= 0) errors.Add($"{label}: CycleSeconds must be > 0.");
            if (_maxLevel < 1) errors.Add($"{label}: MaxLevel must be >= 1.");
            if (_upgradeCostGrowth < 1) errors.Add($"{label}: UpgradeCostGrowth must be >= 1.");
            if (_outputGrowthPerLevel < 0) errors.Add($"{label}: OutputGrowthPerLevel must be >= 0.");
            if (_laserCost < 0 || _laserUpgradeCost < 0) errors.Add($"{label}: laser costs must be >= 0.");
            if (_outputs == null || _outputs.Length == 0) errors.Add($"{label}: room produces nothing.");

            CheckDuplicates(_cost, "BuildCost", label, errors);
            CheckDuplicates(_inputs, "Inputs", label, errors);
            CheckDuplicates(_outputs, "Outputs", label, errors);
            CheckDuplicates(_upgradeBaseCost, "UpgradeBaseCost", label, errors);
            return errors;
        }
        
        private static void CheckDuplicates(ResourceAmount[] list, string field, string label, List<string> errors)
        {
            if (list == null)
                return;

            var seen = new HashSet<ResourceTypeEnum>();
            foreach (var item in list)
            {
                if (!seen.Add(item.Resource.TypeEnum))
                    errors.Add($"{label}: {field} lists {item.Resource.TypeEnum} more than once.");
            }
        }
    }
}

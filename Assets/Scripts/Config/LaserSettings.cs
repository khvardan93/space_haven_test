using System;
using UnityEngine;

namespace Configs
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

        public int Max => _laserMax;
        public int LaserStart => _laserStart;
        public float RegenSeconds => _laserRegenSeconds;
        public int BoostCost => _boostCost;
        public float BoostDuration => _boostDuration;
        public float BoostMultiplier => _boostMultiplier;
    }
}

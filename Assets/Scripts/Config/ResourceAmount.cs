using System;
using UnityEngine;

namespace Configs
{
    [Serializable]
    public struct ResourceAmount
    {
        [SerializeField] private ResourceConfigs _resource;
        [SerializeField] private float _amount;

        public ResourceConfigs Resource => _resource;
        public float Amount => _amount;
    }
}

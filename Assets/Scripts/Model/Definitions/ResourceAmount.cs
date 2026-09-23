using System;
using UnityEngine;

namespace Model
{
    [Serializable]
    public struct ResourceAmount
    {
        [SerializeField] private ResourceDefinition _resource;
        [SerializeField] private float _amount;

        public ResourceDefinition Resource => _resource;
        public float Amount => _amount;
    }
}

using System;
using Economy;
using UnityEngine;

namespace Configs
{
    [Serializable]
    public struct ResourceAmount
    {
        [SerializeField] private ResourceConfigs _resource;
        [SerializeField] private int _amount;

        public ResourceConfigs Resource => _resource;
        public int Amount => _amount;
        
        public Resource ToModel()
        {
            return new Resource(_resource.Type, Math.Max(0, Amount));
        }

        public static Resource[] ToModel(ResourceAmount[] source)
        {
            if (source == null)
                return new Resource[0];

            var result = new Resource[source.Length];
            for (var i = 0; i < source.Length; i++)
                result[i] = source[i].ToModel();
            return result;
        }
    }
}

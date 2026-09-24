using System.Collections.Generic;
using Economy;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "ResourceDefinition", menuName = "Project/Definitions/Resource Definition")]
    public class ResourceConfigs : ScriptableObject
    {
        [SerializeField] private ResourceType _type;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color;
        [SerializeField, TextArea] private string _description;

        public ResourceType Type => _type;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Color Color => _color;
        public string Description => _description;

        public List<string> Validate()
        {
            var errors = new List<string>();
            var label = string.IsNullOrEmpty(_displayName) ? _type.ToString() : _displayName;

            if (string.IsNullOrWhiteSpace(_displayName)) errors.Add(label + ": DisplayName is empty.");
            if (_icon == null) errors.Add(label + ": Icon is not assigned.");
            return errors;
        }
    }
}

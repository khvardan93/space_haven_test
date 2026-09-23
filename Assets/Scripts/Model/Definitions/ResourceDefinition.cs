using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "ResourceDefinition", menuName = "Project/Definitions/Resource Definition")]
    public class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color;
        [SerializeField, TextArea] private string _description;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Color Color => _color;
        public string Description => _description;
    }
}

using System;
using Configs;
using Prison;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    /// <summary>One row in the build list: icon, name, what it produces, cost, and a Build button.</summary>
    public sealed class BuildOptionView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _descriptionLabel;
        [SerializeField] private TMP_Text _productionLabel;
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.5f;

        private GameContext _context;
        private RoomConfigs _spec;

        public event Action<RoomConfigs> Selected;

        public RoomConfigs Spec => _spec; 

        public void Setup(RoomConfigs  spec, GameContext context)
        {
            _spec = spec;
            _context = context;

            var definition = context.Content.GetRoom(spec.Id);
            _icon.sprite = definition != null ? definition.Icon : null;
            _icon.enabled = _icon.sprite;

            _nameLabel.text = spec.DisplayName;
            if (_descriptionLabel)
                _descriptionLabel.text = definition ? definition.DisplayName : string.Empty;
            _productionLabel.text = CostFormatter.Production(spec, spec.Outputs, context);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }

        public void Refresh(int slot)
        {
            var result = _context.Model.Prison.CanBuild(slot, _spec);
            var available = result == ActionResultEnum.Ok;

            _button.interactable = available;
            if (_canvasGroup)
                _canvasGroup.alpha = available ? 1f : _disabledAlpha;

            _costLabel.text = CostFormatter.Cost(_spec.BuildCost, _spec.LaserBuildCost, _context);
        }

        private void OnClick()
        {
            Selected?.Invoke(Spec);
        }
    }
}

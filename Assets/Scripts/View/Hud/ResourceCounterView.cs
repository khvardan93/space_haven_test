using Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class ResourceCounterView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountLabel;
        [SerializeField] private TMP_Text _rateLabel;

        private string _shownAmount;
        private string _shownRate;

        public ResourceTypeEnum TypeEnum { get; private set; }

        public void Setup(ResourceTypeEnum typeEnum, GameContext context)
        {
            TypeEnum = typeEnum;

            var definition = context.Content.GetResource(typeEnum);
            _icon.sprite = definition != null ? definition.Icon : null;
            _icon.enabled = _icon.sprite != null;
            _amountLabel.color = context.Content.GetColor(typeEnum);
        }

        public void SetAmount(double amount)
        {
            // Only touch TMP when the visible string changes; rebuilding text meshes every frame is wasteful on mobile.
            var text = NumberFormat.Short(amount);
            if (text == _shownAmount)
                return;

            _shownAmount = text;
            _amountLabel.text = text;
        }

        public void SetRate(double perSecond)
        {
            var text = NumberFormat.Rate(perSecond);
            if (text == _shownRate)
                return;

            _shownRate = text;
            _rateLabel.text = text;
            _rateLabel.color = UiColors.ForRate(perSecond);
        }
    }
}

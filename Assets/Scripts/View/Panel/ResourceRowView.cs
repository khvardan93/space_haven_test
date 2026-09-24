using Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class ResourceRowView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _balanceLabel;
        [SerializeField] private TMP_Text _productionLabel;
        [SerializeField] private TMP_Text _consumptionLabel;
        [SerializeField] private TMP_Text _netLabel;

        public ResourceTypeEnum TypeEnum { get; private set; }

        public void Setup(ResourceTypeEnum typeEnum, GameContext context)
        {
            TypeEnum = typeEnum;

            var definition = context.Content.GetResource(typeEnum);
            _icon.sprite = definition != null ? definition.Icon : null;
            _icon.enabled = _icon.sprite != null;
            _nameLabel.text = context.Content.GetName(typeEnum);
            _nameLabel.color = context.Content.GetColor(typeEnum);
            _productionLabel.color = UiColors.Positive;
            _consumptionLabel.color = UiColors.Negative;
        }

        public void Refresh(GameContext context)
        {
            var rates = context.Model.Rates;
            var production = rates.GetProduction(TypeEnum);
            var consumption = rates.GetConsumption(TypeEnum);
            var net = production - consumption;

            _balanceLabel.text = NumberFormat.Short(context.Model.Economy.Get(TypeEnum));
            _productionLabel.text = $"+{NumberFormat.Rate(production)}";
            _consumptionLabel.text = consumption > 0.005 ? NumberFormat.Rate(-consumption) : "0/s";
            _netLabel.text = $"Net {NumberFormat.Rate(net)}";
            _netLabel.color = UiColors.ForRate(net);
        }

        private void OnValidate()
        {
            this.RequireAssigned(_icon, nameof(_icon));
            this.RequireAssigned(_nameLabel, nameof(_nameLabel));
            this.RequireAssigned(_balanceLabel, nameof(_balanceLabel));
            this.RequireAssigned(_productionLabel, nameof(_productionLabel));
            this.RequireAssigned(_consumptionLabel, nameof(_consumptionLabel));
            this.RequireAssigned(_netLabel, nameof(_netLabel));
        }
    }
}

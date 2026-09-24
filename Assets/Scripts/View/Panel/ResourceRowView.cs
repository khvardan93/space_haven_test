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

        public ResourceType Type { get; private set; }

        public void Setup(ResourceType type, GameContext context)
        {
            Type = type;

            var definition = context.Content.GetResource(type);
            _icon.sprite = definition != null ? definition.Icon : null;
            _icon.enabled = _icon.sprite != null;
            _nameLabel.text = context.Content.GetName(type);
            _nameLabel.color = context.Content.GetColor(type);
            _productionLabel.color = UiColors.Positive;
            _consumptionLabel.color = UiColors.Negative;
        }

        public void Refresh(GameContext context)
        {
            var rates = context.Model.Rates;
            var production = rates.GetProduction(Type);
            var consumption = rates.GetConsumption(Type);
            var net = production - consumption;

            _balanceLabel.text = NumberFormat.Short(context.Model.Economy.Get(Type));
            _productionLabel.text = Loc.Format("panel_prod", NumberFormat.Rate(production));
            _consumptionLabel.text = Loc.Format("panel_use", consumption > 0.005 ? NumberFormat.Rate(-consumption) : "0/s");
            _netLabel.text = Loc.Format("panel_net", NumberFormat.Rate(net));
            _netLabel.color = UiColors.ForRate(net);
        }
    }
}

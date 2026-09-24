using System.Collections.Generic;
using Economy;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private ResourceCounterView _counterPrefab;
        [SerializeField] private RectTransform _counterContainer;
        [SerializeField] private LaserEnergyView _laserView;
        [SerializeField] private Button _openPanelButton;
        [SerializeField] private EconomyPanelView _economyPanel;

        private readonly List<ResourceCounterView> _counters = new ();
        private GameContext _context;
        private bool _amountsDirty;
        private bool _ratesDirty;

        public void Init(GameContext context)
        {
            _context = context;

            for (var i = _counterContainer.childCount - 1; i >= 0; i--)
                Destroy(_counterContainer.GetChild(i).gameObject);

            foreach (var type in ResourceTypes.All)
            {
                var counter = Instantiate(_counterPrefab, _counterContainer);
                counter.name = $"Counter_{type}";
                counter.Setup(type, context);
                _counters.Add(counter);
            }

            _laserView.Init(context);

            _openPanelButton.onClick.AddListener(_economyPanel.Toggle);

            _context.Model.Economy.Changed += OnEconomyChanged;
            _context.Model.Rates.Updated += OnRatesUpdated;

            _amountsDirty = true;
            _ratesDirty = true;
        }

        private void LateUpdate()
        {
            if (_context == null)
                return;

            if (_amountsDirty)
            {
                _amountsDirty = false;
                foreach (var counter in _counters)
                    counter.SetAmount(_context.Model.Economy.Get(counter.TypeEnum));
            }

            if (_ratesDirty)
            {
                _ratesDirty = false;
                foreach (var counter in _counters)
                    counter.SetRate(_context.Model.Rates.GetNet(counter.TypeEnum));
            }
        }

        private void OnDestroy()
        {
            if (_context == null)
                return;

            _context.Model.Economy.Changed -= OnEconomyChanged;
            _context.Model.Rates.Updated -= OnRatesUpdated;
            _openPanelButton.onClick.RemoveListener(_economyPanel.Toggle);
        }

        private void OnEconomyChanged(ResourceTypeEnum typeEnum, double value)
        {
            _amountsDirty = true;
        }

        private void OnRatesUpdated()
        {
            _ratesDirty = true;
        }

        private void OnValidate()
        {
            this.RequireAssigned(_counterPrefab, nameof(_counterPrefab));
            this.RequireAssigned(_counterContainer, nameof(_counterContainer));
            this.RequireAssigned(_laserView, nameof(_laserView));
            this.RequireAssigned(_openPanelButton, nameof(_openPanelButton));
            this.RequireAssigned(_economyPanel, nameof(_economyPanel));
        }
    }
}

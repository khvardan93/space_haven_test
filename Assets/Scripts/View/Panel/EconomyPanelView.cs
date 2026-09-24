using System.Collections.Generic;
using Economy;
using SpacePrison.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class EconomyPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _dimBackground;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _summaryLabel;

        [Header("Resources")]
        [SerializeField] private ResourceRowView _resourceRowPrefab;
        [SerializeField] private RectTransform _resourceContainer;

        [Header("Rooms")]
        [SerializeField] private RoomStatRowView _roomRowPrefab;
        [SerializeField] private RectTransform _roomContainer;

        [Header("Refresh")]
        [SerializeField, Range(1f, 30f)] private float _refreshPerSecond = 4f;

        private readonly List<ResourceRowView> _resourceRows = new ();
        private readonly List<RoomStatRowView> _roomRows = new ();
        private GameContext _context;
        private float _timer;

        public bool IsOpen => _root.activeSelf;

        public void Init(GameContext context)
        {
            _context = context;

            Clear(_resourceContainer);
            foreach (var type in ResourceTypes.All)
            {
                var row = Instantiate(_resourceRowPrefab, _resourceContainer);
                row.name = $"Resource_{type}";
                row.Setup(type, context);
                _resourceRows.Add(row);
            }

            Clear(_roomContainer);
            foreach (var spec in context.Model.Catalog.All)
            {
                var row = Instantiate(_roomRowPrefab, _roomContainer);
                row.name = $"Room_{spec.Id}";
                row.Setup(spec, context);
                _roomRows.Add(row);
            }

            _dimBackground.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
            Close();
        }

        public void Open()
        {
            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            _root.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        private void Update()
        {
            if (_context == null || !IsOpen)
                return;

            _timer += Time.unscaledDeltaTime;
            if (_timer >= 1f / _refreshPerSecond)
            {
                _timer = 0;
                Refresh();
            }
        }

        private void OnDestroy()
        {
            _dimBackground.onClick.RemoveListener(Close);
            _closeButton.onClick.RemoveListener(Close);
        }

        private void OnValidate()
        {
            this.RequireAssigned(_root, nameof(_root));
            this.RequireAssigned(_dimBackground, nameof(_dimBackground));
            this.RequireAssigned(_closeButton, nameof(_closeButton));
            this.RequireAssigned(_summaryLabel, nameof(_summaryLabel));
            this.RequireAssigned(_resourceRowPrefab, nameof(_resourceRowPrefab));
            this.RequireAssigned(_resourceContainer, nameof(_resourceContainer));
            this.RequireAssigned(_roomRowPrefab, nameof(_roomRowPrefab));
            this.RequireAssigned(_roomContainer, nameof(_roomContainer));
        }

        private void Refresh()
        {
            foreach (var row in _resourceRows)
                row.Refresh(_context);
            foreach (var row in _roomRows)
                row.Refresh(_context);

            var model = _context.Model;
            _summaryLabel.text =
                $"{model.Prison.Rooms.Count} / {model.Prison.SlotCount} rooms   {model.Laser.Current} / {model.Laser.Max} LE   x{model.Simulation.TimeScale:0}";
        }

        private static void Clear(RectTransform container)
        {
            for (var i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}

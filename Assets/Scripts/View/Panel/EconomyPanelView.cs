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
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _summaryLabel;

        [Header("Resources")]
        [SerializeField] private TMP_Text _resourcesHeader;
        [SerializeField] private ResourceRowView _resourceRowPrefab;
        [SerializeField] private RectTransform _resourceContainer;

        [Header("Rooms")]
        [SerializeField] private TMP_Text _roomsHeader;
        [SerializeField] private RoomStatRowView _roomRowPrefab;
        [SerializeField] private RectTransform _roomContainer;

        [Header("Refresh")]
        [SerializeField, Range(1f, 30f)] private float _refreshPerSecond = 4f;

        private readonly List<ResourceRowView> _resourceRows = new List<ResourceRowView>();
        private readonly List<RoomStatRowView> _roomRows = new List<RoomStatRowView>();
        private GameContext _context;
        private float _timer;

        public bool IsOpen
        {
            get { return _root.activeSelf; }
        }

        public void Init(GameContext context)
        {
            _context = context;

            _titleLabel.text = Loc.Get("panel_title");
            if (_resourcesHeader != null) _resourcesHeader.text = Loc.Get("panel_resources");
            if (_roomsHeader != null) _roomsHeader.text = Loc.Get("panel_rooms");

            Clear(_resourceContainer);
            foreach (var type in ResourceTypes.All)
            {
                var row = Instantiate(_resourceRowPrefab, _resourceContainer);
                row.name = "Resource_" + type;
                row.Setup(type, context);
                _resourceRows.Add(row);
            }

            Clear(_roomContainer);
            foreach (var spec in context.Model.Catalog.All)
            {
                var row = Instantiate(_roomRowPrefab, _roomContainer);
                row.name = "Room_" + spec.Id;
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
            if (_dimBackground != null) _dimBackground.onClick.RemoveListener(Close);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
        }

        private void Refresh()
        {
            foreach (var row in _resourceRows)
                row.Refresh(_context);
            foreach (var row in _roomRows)
                row.Refresh(_context);

            var model = _context.Model;
            _summaryLabel.text = Loc.Format("panel_summary",
                model.Prison.Rooms.Count, model.Prison.SlotCount,
                model.Laser.Current, model.Laser.Max,
                model.Simulation.TimeScale.ToString("0"));
        }

        private static void Clear(RectTransform container)
        {
            for (var i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}

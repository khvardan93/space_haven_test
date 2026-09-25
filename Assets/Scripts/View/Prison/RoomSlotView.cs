using System;
using Prison;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    
    public sealed class RoomSlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [Header("States")]
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _builtState;

        [Header("Built")]
        [SerializeField] private Image _frame;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _outputLabel;
        [SerializeField] private Image _progressFill;

        [Header("Status")]
        [SerializeField] private GameObject _starvedBadge;
        [SerializeField] private GameObject _boostBadge;
        [SerializeField] private TMP_Text _boostLabel;
        [SerializeField] private Color _starvedTint = new Color(1f, 0.35f, 0.35f, 1f);

        private GameContext _context;
        private Room _room;
        private int _slot;
        private int _shownBoostSeconds = -1;

        public event Action<int> Clicked;

        public int Slot => _slot; 

        public void Init(int slot, GameContext context)
        {
            _slot = slot;
            _context = context;

            _button.onClick.AddListener(OnClick);
            _context.Model.Prison.SlotChanged += OnSlotChanged;

            BindRoom(_context.Model.Prison.GetRoom(_slot));
        }

        private void Update()
        {
            if (_room == null)
                return;

            _progressFill.fillAmount = _room.Progress01;

            if (_room.IsBoosted)
            {
                var seconds = (int)Math.Ceiling(_room.BoostRemaining);
                if (seconds != _shownBoostSeconds)
                {
                    _shownBoostSeconds = seconds;
                    _boostLabel.text = $"x{_room.BoostMultiplier:0}  {NumberFormat.Duration(seconds)}";
                }
            }
        }

        private void OnDestroy()
        {
            if (_context != null)
                _context.Model.Prison.SlotChanged -= OnSlotChanged;
            if (_room != null)
                _room.StateChanged -= OnRoomStateChanged;
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            Clicked?.Invoke(_slot);
        }

        private void OnSlotChanged(int slot)
        {
            if (slot == _slot)
                BindRoom(_context.Model.Prison.GetRoom(_slot));
        }

        private void BindRoom(Room room)
        {
            if (_room != room)
            {
                if (_room != null) _room.StateChanged -= OnRoomStateChanged;
                _room = room;
                if (_room != null) _room.StateChanged += OnRoomStateChanged;
            }
            Refresh();
        }

        private void OnRoomStateChanged(Room room)
        {
            Refresh();
        }

        private void Refresh()
        {
            var built = _room != null;
            _emptyState.SetActive(!built);
            _builtState.SetActive(built);

            if (!built)
                return;

            var definition = _context.Content.GetRoom(_room.Spec.Id);
            var accent = definition ? definition.Color : Color.white;

            _icon.sprite = definition ? definition.Icon : null;
            _icon.enabled = _icon.sprite;
            _nameLabel.text = _room.Spec.DisplayName;
            _levelLabel.text = $"Lv {_room.Level}";
            _outputLabel.text = CostFormatter.Production(_room.Spec, _room.CurrentOutputs, _context);

            _starvedBadge.SetActive(_room.IsStarved);
            _boostBadge.SetActive(_room.IsBoosted);
            _shownBoostSeconds = -1;

            _frame.color = _room.IsStarved ? _starvedTint : accent;
            _progressFill.color = _room.IsStarved ? _starvedTint : accent;
            _progressFill.fillAmount = _room.Progress01;
        }

        private void OnValidate()
        {
            this.RequireAssigned(_button, nameof(_button));
            this.RequireAssigned(_emptyState, nameof(_emptyState));
            this.RequireAssigned(_builtState, nameof(_builtState));
            this.RequireAssigned(_frame, nameof(_frame));
            this.RequireAssigned(_icon, nameof(_icon));
            this.RequireAssigned(_nameLabel, nameof(_nameLabel));
            this.RequireAssigned(_levelLabel, nameof(_levelLabel));
            this.RequireAssigned(_outputLabel, nameof(_outputLabel));
            this.RequireAssigned(_progressFill, nameof(_progressFill));
            this.RequireAssigned(_starvedBadge, nameof(_starvedBadge));
            this.RequireAssigned(_boostBadge, nameof(_boostBadge));
            this.RequireAssigned(_boostLabel, nameof(_boostLabel));
        }
    }
}

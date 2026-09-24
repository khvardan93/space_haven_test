using System;
using Prison;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    /// <summary>
    /// One cell of the cellblock. Static info (name, level, output, starved) refreshes on model events;
    /// only the progress bar and boost countdown update every frame, since they change continuously.
    /// </summary>
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
        [SerializeField] private TMP_Text _starvedLabel;
        [SerializeField] private GameObject _boostBadge;
        [SerializeField] private TMP_Text _boostLabel;
        [SerializeField] private Color _starvedTint = new Color(1f, 0.35f, 0.35f, 1f);

        [Header("Empty")]
        [SerializeField] private TMP_Text _emptyLabel;

        private GameContext _context;
        private Room _room;
        private int _slot;
        private int _shownBoostSeconds = -1;

        public event Action<int> Clicked;

        public int Slot
        {
            get { return _slot; }
        }

        public void Init(int slot, GameContext context)
        {
            _slot = slot;
            _context = context;

            _button.onClick.AddListener(OnClick);
            _context.Model.Prison.SlotChanged += OnSlotChanged;

            if (_emptyLabel != null)
                _emptyLabel.text = Loc.Get("slot_tap_to_build");
            if (_starvedLabel != null)
                _starvedLabel.text = Loc.Get("slot_starved");

            BindRoom(_context.Model.Prison.GetRoom(_slot));
        }

        private void Update()
        {
            if (_room == null)
                return;

            _progressFill.fillAmount = _room.Progress01;

            if (_room.IsBoosted)
            {
                int seconds = (int)Math.Ceiling(_room.BoostRemaining);
                if (seconds != _shownBoostSeconds)
                {
                    _shownBoostSeconds = seconds;
                    _boostLabel.text = "x" + _room.BoostMultiplier.ToString("0") + "  " + NumberFormat.Duration(seconds);
                }
            }
        }

        private void OnDestroy()
        {
            if (_context != null)
                _context.Model.Prison.SlotChanged -= OnSlotChanged;
            if (_room != null)
                _room.StateChanged -= OnRoomStateChanged;
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            var handler = Clicked;
            if (handler != null) handler(_slot);
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
            bool built = _room != null;
            _emptyState.SetActive(!built);
            _builtState.SetActive(built);

            if (!built)
                return;

            var definition = _context.Content.GetRoom(_room.Spec.Id);
            var accent = definition != null ? definition.Color : Color.white;

            _icon.sprite = definition != null ? definition.Icon : null;
            _icon.enabled = _icon.sprite != null;
            _nameLabel.text = Loc.Get(_room.Spec.Id);
            _levelLabel.text = Loc.Format("slot_level", _room.Level);
            _outputLabel.text = CostFormatter.Production(_room.Spec, _room.CurrentOutputs, _context);

            _starvedBadge.SetActive(_room.IsStarved);
            _boostBadge.SetActive(_room.IsBoosted);
            _shownBoostSeconds = -1;

            _frame.color = _room.IsStarved ? _starvedTint : accent;
            _progressFill.color = _room.IsStarved ? _starvedTint : accent;
            _progressFill.fillAmount = _room.Progress01;
        }
    }
}

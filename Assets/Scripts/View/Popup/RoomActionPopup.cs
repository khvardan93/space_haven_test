using System;
using System.Collections.Generic;
using Configs;
using Economy;
using Prison;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class RoomActionPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _dimBackground;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _messageLabel;

        [Header("Build mode")]
        [SerializeField] private GameObject _buildPanel;
        [SerializeField] private RectTransform _optionContainer;
        [SerializeField] private BuildOptionView _optionPrefab;

        [Header("Room mode")]
        [SerializeField] private GameObject _roomPanel;
        [SerializeField] private Image _roomIcon;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _currentOutputLabel;
        [SerializeField] private TMP_Text _nextOutputLabel;
        [SerializeField] private TMP_Text _upgradeCostLabel;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonLabel;
        [SerializeField] private Button _boostButton;
        [SerializeField] private TMP_Text _boostButtonLabel;
        [SerializeField] private TMP_Text _boostCostLabel;

        private readonly List<BuildOptionView> _options = new List<BuildOptionView>();
        private GameContext _context;
        private int _slot = -1;
        private bool _dirty;

        /// <summary>Raised after any build/upgrade/boost attempt. Phase 8 hooks feedback (punch scale, sound) here.</summary>
        public event Action<int, ActionResult> ActionPerformed;

        public bool IsOpen
        {
            get { return _root.activeSelf; }
        }

        public void Init(GameContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;

            CreateBuildOptions();

            _dimBackground.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _boostButton.onClick.AddListener(OnBoostClicked);

            var model = _context.Model;
            model.Economy.Changed += OnEconomyChanged;
            model.Laser.Changed += OnLaserChanged;
            model.Prison.SlotChanged += OnSlotChanged;

            Close();
        }

        public void Open(int slot)
        {
            if (!_context.Model.Prison.IsValidSlot(slot))
                return;

            _slot = slot;
            _messageLabel.text = string.Empty;
            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            _root.SetActive(false);
            _slot = -1;
        }

        private void LateUpdate()
        {
            if (_dirty && IsOpen)
                Refresh();
            _dirty = false;
        }

        private void OnDestroy()
        {
            if (_context == null)
                return;

            var model = _context.Model;
            model.Economy.Changed -= OnEconomyChanged;
            model.Laser.Changed -= OnLaserChanged;
            model.Prison.SlotChanged -= OnSlotChanged;
        }

        // ---------- Setup ----------

        private void CreateBuildOptions()
        {
            for (var i = _optionContainer.childCount - 1; i >= 0; i--)
                Destroy(_optionContainer.GetChild(i).gameObject);

            foreach (var spec in _context.Model.Catalog.All)
            {
                var option = Instantiate(_optionPrefab, _optionContainer);
                option.gameObject.SetActive(true);
                option.name = "Option_" + spec.Id;
                option.Setup(spec, _context);
                option.Selected += OnBuildSelected;
                _options.Add(option);
            }
        }

        // ---------- Refresh ----------

        private void Refresh()
        {
            if (_slot < 0)
                return;

            var room = _context.Model.Prison.GetRoom(_slot);
            _buildPanel.SetActive(room == null);
            _roomPanel.SetActive(room != null);

            if (room == null)
                RefreshBuildMode();
            else
                RefreshRoomMode(room);
        }

        private void RefreshBuildMode()
        {
            _titleLabel.text = "Build";
            foreach (var option in _options)
                option.Refresh(_slot);
        }

        private void RefreshRoomMode(Room room)
        {
            var spec = room.Spec;
            var prison = _context.Model.Prison;
            var definition = _context.Content.GetRoom(spec.Id);

            _titleLabel.text = spec.DisplayName;
            _roomIcon.sprite = definition != null ? definition.Icon : null;
            _roomIcon.enabled = _roomIcon.sprite != null;
            _levelLabel.text = "Lv " + room.Level + " / " + spec.MaxLevel;
            _currentOutputLabel.text = "Now: " + CostFormatter.Production(spec, room.CurrentOutputs, _context);

            // Upgrade
            if (room.IsMaxLevel)
            {
                _nextOutputLabel.text = string.Empty;
                _upgradeCostLabel.text = string.Empty;
                _upgradeButtonLabel.text = "Max level";
                _upgradeButton.interactable = false;
            }
            else
            {
                var nextOutputs = Resource.Scale(spec.Outputs, spec.GetOutputMultiplier(room.Level + 1));
                _nextOutputLabel.text = "Next: " + CostFormatter.Production(spec, nextOutputs, _context);
                _upgradeCostLabel.text = "Cost: " + CostFormatter.Cost(prison.GetUpgradeCost(_slot), spec.LaserUpgradeCost, _context);
                _upgradeButtonLabel.text = "Upgrade";
                _upgradeButton.interactable = prison.CanUpgrade(_slot) == ActionResult.Ok;
            }

            // Boost
            var laserSpec = _context.Model.Setup.Laser;
            _boostButtonLabel.text = "Boost x" + laserSpec.BoostMultiplier.ToString("0") + " for " + laserSpec.BoostDuration.ToString("0") + "s";
            _boostCostLabel.text = CostFormatter.Cost(null, laserSpec.BoostCost, _context);
            _boostButton.interactable = prison.CanBoost(_slot) == ActionResult.Ok;
        }

        // ---------- Actions ----------

        private void OnBuildSelected(RoomConfigs spec)
        {
            var slot = _slot;
            var result = _context.Model.Prison.TryBuild(slot, spec);
            Report(slot, result);

            if (result == ActionResult.Ok)
                Close();
        }

        private void OnUpgradeClicked()
        {
            // Stays open so the player can chain upgrades.
            Report(_slot, _context.Model.Prison.TryUpgrade(_slot));
        }

        private void OnBoostClicked()
        {
            var slot = _slot;
            var result = _context.Model.Prison.TryBoost(slot);
            Report(slot, result);

            if (result == ActionResult.Ok)
                Close();
        }

        private void Report(int slot, ActionResult result)
        {
            _messageLabel.text = Message(result);

            var handler = ActionPerformed;
            if (handler != null) handler(slot, result);
        }

        private static string Message(ActionResult result)
        {
            switch (result)
            {
                case ActionResult.Ok: return "";
                case ActionResult.NotEnoughResources: return "Not enough resources";
                case ActionResult.NotEnoughLaser: return "Not enough Laser Energy";
                case ActionResult.MaxLevel: return "Already at max level";
                case ActionResult.Occupied: return "Cell is occupied";
                case ActionResult.Empty: return "Cell is empty";
                default: return "Invalid cell";
            }
        }

        // ---------- Model events ----------

        private void OnEconomyChanged(ResourceType type, double value)
        {
            _dirty = true;
        }

        private void OnLaserChanged(int value)
        {
            _dirty = true;
        }

        private void OnSlotChanged(int slot)
        {
            if (slot == _slot)
                _dirty = true;
        }
    }
}

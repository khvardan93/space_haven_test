using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class LaserEnergyView : MonoBehaviour
    {
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private TMP_Text _timerLabel;

        private GameContext _context;
        private int _shownTimerSeconds = -1;

        public void Init(GameContext context)
        {
            _context = context;
            _context.Model.Laser.Changed += OnLaserChanged;
            _fill.color = UiColors.LaserFill;
            Refresh();
        }

        private void Update()
        {
            if (_context == null)
                return;

            var laser = _context.Model.Laser;
            var seconds = laser.IsFull ? 0 : (int)Math.Ceiling(laser.SecondsToNext);
            if (seconds == _shownTimerSeconds)
                return;

            _shownTimerSeconds = seconds;
            _timerLabel.text = laser.IsFull
                ? "Full"
                : $"Next in {NumberFormat.Duration(seconds)}";
        }

        private void OnDestroy()
        {
            if (_context != null)
                _context.Model.Laser.Changed -= OnLaserChanged;
        }

        private void OnLaserChanged(int value)
        {
            Refresh();
        }

        private void Refresh()
        {
            var laser = _context.Model.Laser;
            _fill.fillAmount = (float)laser.Current / laser.Max;
            _valueLabel.text = $"{laser.Current} / {laser.Max}";
            _shownTimerSeconds = -1;
        }

        private void OnValidate()
        {
            this.RequireAssigned(_fill, nameof(_fill));
            this.RequireAssigned(_valueLabel, nameof(_valueLabel));
            this.RequireAssigned(_timerLabel, nameof(_timerLabel));
        }
    }
}

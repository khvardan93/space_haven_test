using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class TimeScaleView : MonoBehaviour
    {
        [SerializeField] private Button _normalButton;
        [SerializeField] private Button _fastButton;
        [SerializeField] private TMP_Text _fastLabel;
        [SerializeField, Min(1f)] private float _fastScale = 5f;

        private GameContext _context;

        public void Init(GameContext context)
        {
            _context = context;
            _normalButton.onClick.AddListener(SetNormal);
            _fastButton.onClick.AddListener(SetFast);
            if (_fastLabel)
                _fastLabel.text = $"x{_fastScale:0}";

            Apply(1f);
        }

        private void OnDestroy()
        {
            if (_normalButton != null) _normalButton.onClick.RemoveListener(SetNormal);
            if (_fastButton != null) _fastButton.onClick.RemoveListener(SetFast);
        }

        private void SetNormal()
        {
            Apply(1f);
        }

        private void SetFast()
        {
            Apply(_fastScale);
        }

        private void Apply(float scale)
        {
            _context.Model.Simulation.TimeScale = scale;
            _normalButton.interactable = scale > 1f;
            _fastButton.interactable = scale <= 1f;
        }
    }
}

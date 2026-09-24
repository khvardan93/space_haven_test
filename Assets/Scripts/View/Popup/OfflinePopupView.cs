using System.Text;
using Economy;
using Offline;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public sealed class OfflinePopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _elapsedLabel;
        [SerializeField] private TMP_Text _gainsLabel;
        [SerializeField] private Button _collectButton;
        [SerializeField] private TMP_Text _collectLabel;

        private static readonly StringBuilder Builder = new StringBuilder(128);
        private GameContext _context;

        public void Init(GameContext context)
        {
            _context = context;
            _collectButton.onClick.AddListener(Close);
            _titleLabel.text = "Welcome back";
            if (_collectLabel != null)
                _collectLabel.text = "Collect";
            Close();
        }

        public void Show(OfflineReport report)
        {
            if (report == null || !report.HasGains)
                return;

            var elapsed = NumberFormat.Duration(report.Elapsed.TotalSeconds);
            _elapsedLabel.text = report.WasCapped
                ? "While away for " + elapsed + " (capped)"
                : "While away for " + elapsed;

            Builder.Length = 0;
            foreach (var type in ResourceTypes.All)
            {
                var change = report.Get(type);
                if (change < 0.5 && change > -0.5)
                    continue;

                if (Builder.Length > 0)
                    Builder.Append('\n');

                var color = CostFormatter.Hex(change > 0 ? _context.Content.GetColor(type) : UiColors.Negative);
                Builder.Append("<color=").Append(color).Append('>')
                       .Append(change > 0 ? "+" : "-")
                       .Append(NumberFormat.Short(System.Math.Abs(change)))
                       .Append("  ").Append(_context.Content.GetName(type))
                       .Append("</color>");
            }
            _gainsLabel.text = Builder.ToString();

            _root.SetActive(true);
        }

        public void Close()
        {
            _root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_collectButton != null)
                _collectButton.onClick.RemoveListener(Close);
        }
    }
}

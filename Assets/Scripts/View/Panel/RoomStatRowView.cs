using System.Text;
using Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using View;

namespace SpacePrison.View
{
    public sealed class RoomStatRowView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private TMP_Text _detailLabel;

        private static readonly StringBuilder Builder = new StringBuilder(64);

        public RoomConfigs Spec { get; private set; }

        public void Setup(RoomConfigs spec, GameContext context)
        {
            Spec = spec;

            var definition = context.Content.GetRoom(spec.Id);
            _icon.sprite = definition ? definition.Icon : null;
            _icon.enabled = _icon.sprite;
            _nameLabel.text = spec.DisplayName;
            if (definition)
                _nameLabel.color = definition.Color;
        }

        public void Refresh(GameContext context)
        {
            var count = 0;
            var working = 0;
            var starved = 0;
            var boosted = 0;
            var totalLevels = 0;
            var maxLevel = 0;

            var rooms = context.Model.Prison.Rooms;
            for (var i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                if (room.Spec != Spec)
                    continue;

                count++;
                totalLevels += room.Level;
                if (room.Level > maxLevel) maxLevel = room.Level;
                if (room.IsStarved) starved++; else working++;
                if (room.IsBoosted) boosted++;
            }

            _countLabel.text = $"x{count}";

            Builder.Length = 0;
            if (count == 0)
            {
                Builder.Append("None built");
            }
            else
            {
                Builder.Append("Lv ").Append(totalLevels).Append('/').Append(maxLevel);
                Builder.Append("   ").Append(working).Append(" working");
                if (starved > 0)
                    Builder.Append("   <color=#FF5A5A>").Append(starved).Append(" starved").Append("</color>");
                if (boosted > 0)
                    Builder.Append("   <color=#FFC23D>").Append(boosted).Append(" boosted").Append("</color>");
            }
            _detailLabel.text = Builder.ToString();
        }
    }
}

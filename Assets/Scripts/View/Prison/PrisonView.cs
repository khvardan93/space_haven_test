using System;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public sealed class PrisonView : MonoBehaviour
    {
        [SerializeField] private RoomSlotView _slotPrefab;
        [SerializeField] private RectTransform _slotContainer;

        private readonly List<RoomSlotView> _slots = new List<RoomSlotView>();

        public event Action<int> SlotClicked;

        public void Init(GameContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            for (var i = _slotContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotContainer.GetChild(i).gameObject);
            _slots.Clear();

            var count = context.Model.Prison.SlotCount;
            for (var slot = 0; slot < count; slot++)
            {
                var view = Instantiate(_slotPrefab, _slotContainer);
                view.gameObject.SetActive(true);
                view.name = $"Slot_{slot}";
                view.Init(slot, context);
                view.Clicked += OnSlotClicked;
                _slots.Add(view);
            }
        }

        public RoomSlotView GetSlotView(int slot)
        {
            return slot >= 0 && slot < _slots.Count ? _slots[slot] : null;
        }

        private void OnSlotClicked(int slot)
        {
            SlotClicked?.Invoke(slot);
        }

        private void OnDestroy()
        {
            foreach (var view in _slots)
            {
                if (view)
                    view.Clicked -= OnSlotClicked;
            }
        }

        private void OnValidate()
        {
            this.RequireAssigned(_slotPrefab, nameof(_slotPrefab));
            this.RequireAssigned(_slotContainer, nameof(_slotContainer));
        }
    }
}

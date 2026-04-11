using System;
using System.Collections.Generic;
using infrastructure.services.clan;
using Sirenix.Utilities;
using UnityEngine;

namespace ui.clan.history
{
    public class HistoryAdapter : Adapter
    {
        [SerializeField] private HistoryElement _historyElement;

        private List<ClanHistoryEntry> _history;
        
        public override event Action OnDataChange;
        public override int GetItemCount()
        {
            return _history.IsNullOrEmpty() ? 0 : _history.Count;
        }

        public override GameObject CreateView(int index, Transform parent)
        {
            var history = Instantiate(_historyElement, parent);
            return history.gameObject;
        }

        public override void BindView(GameObject view, int index)
        {
            var historyElement = view.GetComponent<HistoryElement>();
            historyElement.Bind(_history[index]);
        }

        public void SetHistory(List<ClanHistoryEntry> history)
        {
            _history = history;
            OnDataChange?.Invoke();
        }
    }
}
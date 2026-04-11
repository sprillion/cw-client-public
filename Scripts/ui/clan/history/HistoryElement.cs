using infrastructure.services.clan;
using TMPro;
using UnityEngine;

namespace ui.clan.history
{
    public class HistoryElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _dateText;

        public void Bind(ClanHistoryEntry historyEntry)
        {
            _text.text = historyEntry.HistoryType.ToString();
            _dateText.text = historyEntry.CreationDate.ToString("g");
        }
    }
}
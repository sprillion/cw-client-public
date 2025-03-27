using System.Text;
using TMPro;
using UnityEngine;

namespace ui.notifications
{
    public class Notification : PooledObject
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public RectTransform Rect => (RectTransform)transform;
        public CanvasGroup CanvasGroup => _canvasGroup;
        
        public void SetValue(Notify notify, bool isPositive, int value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(isPositive ? "+" : "");
            _stringBuilder.Append(value);
            _stringBuilder.Append(notify.IconType.ToIcon());
            _text.text = _stringBuilder.ToString();
            _text.color = isPositive ? notify.PositiveColor : notify.NegativeColor;
        }
    }
}
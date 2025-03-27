using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ui.tools
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _secondValueText;

        [SerializeField] private bool _showValue;
        [SerializeField] private bool _showMaxValue;

        public float MinValue { get; private set; } = 0;
        public int MaxValue { get; private set; } = 1;
        public float Value { get; private set; }

        private void Start()
        {
            if (!_showValue)
            {
                _valueText.gameObject.SetActive(false);
                _secondValueText.gameObject.SetActive(false);
            }
        }

        public void SetValue(float value, float duration = 0)
        {
            Value = value;
            _fillImage.DOFillAmount(Value / (MaxValue - MinValue), duration).SetEase(Ease.Linear);
            SetText();
        }

        public void SetMaxValue(int maxValue)
        {
            MaxValue = maxValue;
        }

        public void SetMinValue(float minValue)
        {
            MinValue = minValue;
        }
        
        private void SetText()
        {
            if (!_showValue) return;
            _valueText.text = _showMaxValue ? $"{Value}/{MaxValue}" : $"{Value}";
            _secondValueText.text = _valueText.text;
        }
    }
}
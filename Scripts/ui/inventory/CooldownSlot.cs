using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ui.inventory
{
    public class CooldownSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        public void StartCooldown(float cooldown)
        {
            gameObject.SetActive(true);
            DOVirtual.Float(cooldown, 0, cooldown, value =>
            {
                _timerText.text = $"{value:F1}";
            }).SetEase(Ease.Linear).OnComplete(StopCooldown);
        }

        public void StopCooldown()
        {
            gameObject.SetActive(false);
        }
    }
}
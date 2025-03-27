using TMPro;
using ui.tools;
using UnityEngine;

namespace ui.worldUi
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private float _changedHealthDuration;
        
        [SerializeField] private ProgressBar _healthBar;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _nicknameText;
        [SerializeField] private TMP_Text _clanText;

        public void Initialize()
        {
            _clanText.gameObject.SetActive(false);
        }

        public void SetMaxHealth(int value)
        {
            _healthBar.SetMaxValue(value);
        }

        public void SetHealth(int value)
        {
            _healthBar.SetValue(value, value == _healthBar.MaxValue ? 0 : _changedHealthDuration);
        }

        public void SetNickname(string nickname)
        {
            _nicknameText.text = nickname;
        }

        public void SetLevel(int level)
        {
            _levelText.text = level.ToString();
        }

        public void SetClan(string clan)
        {
            _clanText.text = clan;
            _clanText.gameObject.SetActive(true);
        }
    }
}
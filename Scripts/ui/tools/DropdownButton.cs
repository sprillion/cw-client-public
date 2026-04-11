using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ui.tools
{
    public class DropdownButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [SerializeField] private TMP_Text _title;
        
        [SerializeField] private GameObject _content;
        [SerializeField] private GameObject _openedIcon;
        [SerializeField] private GameObject _closedIcon;

        [SerializeField] private bool _opened;
        
        private void Awake()
        {
            _button.onClick.AddListener(Toggle);
            SetActive();
        }

        public void SetTitle(string title)
        {
            _title.text = title;
        }

        private void Toggle()
        {
            _opened = !_opened;
            SetActive();
        }

        private void SetActive()
        {
            _content.SetActive(_opened);
            _openedIcon.SetActive(_opened);
            _closedIcon.SetActive(!_opened);
        }
    }
}
using TMPro;
using ui.popup;
using UnityEngine;
using UnityEngine.UI;

namespace ui.clan
{
    public class SettingsView : Popup
    {
        [SerializeField] private TMP_InputField _clanNameInput;
        [SerializeField] private TMP_InputField _shortClanNameInput;
        [SerializeField] private Transform _iconsParent;
        [SerializeField] private TMP_Text _changePriceText;
        [SerializeField] private Button _iconsButton;
        [SerializeField] private Button _changeButton;
    }
}
using ui.popup;
using UnityEngine;

namespace ui.inventory.character
{
    public class CharacterPopup : Popup
    {
        [SerializeField] private Slot[] _armorSlots = new Slot[4];
    }
}
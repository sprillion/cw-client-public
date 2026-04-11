using System;
using character;
using infrastructure.services.players;
using ui.inventory.equipSlot;
using ui.popup;
using UnityEngine;
using Zenject;

namespace ui.inventory.character
{
    public class CharacterPopup : Popup
    {
        [SerializeField] private EquipSlot[] _equipSlots = new EquipSlot[8];
        [SerializeField] private CharacterSkin _characterSkin;

        private ICharacterService _characterService;
        
        public EquipSlot[] EquipSlots => _equipSlots;
        
        [Inject]
        public void Construct(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public override void Initialize()
        {
            _characterSkin.Initialize();
            
            foreach (ArmorPlaceType armorPlaceType in Enum.GetValues(typeof(ArmorPlaceType)))
            {
                SetArmor(armorPlaceType);
            }
            _characterService.CurrentCharacter.CharacterSkin.OnArmorChanged += SetArmor;
        }

        public override void Show()
        {
            SetSkin();
            base.Show();
        }

        private void SetArmor(ArmorPlaceType armorPlaceType)
        {
            var armorType = armorPlaceType switch
            {
                ArmorPlaceType.Head => _characterService.CurrentCharacter.CharacterSkin.CurrentArmorHead,
                ArmorPlaceType.Body => _characterService.CurrentCharacter.CharacterSkin.CurrentArmorBody,
                ArmorPlaceType.Legs => _characterService.CurrentCharacter.CharacterSkin.CurrentArmorLegs,
                ArmorPlaceType.Foot => _characterService.CurrentCharacter.CharacterSkin.CurrentArmorFoot,
            };
            _characterSkin.SetArmor(armorType, armorPlaceType);
        }

        private void SetSkin()
        {
            _characterSkin.SetSkin(_characterService.CurrentCharacter.CharacterSkin.SkinId);
        }
    }
}
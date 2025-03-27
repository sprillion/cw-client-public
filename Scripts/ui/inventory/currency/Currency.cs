using System.Globalization;
using infrastructure.services.players;
using TMPro;
using UnityEngine;
using Zenject;

namespace ui.inventory.currency
{
    public class Currency : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _diamondsText;

        private ICharacterService _characterService;

        [Inject]
        public void Construct(ICharacterService characterService)
        {
            _characterService = characterService;
            
            _characterService.CurrentCharacter.CharacterStats.OnGoldChanged += OnGoldChanged;
            _characterService.CurrentCharacter.CharacterStats.OnDiamondsChanged += OnDiamondsChanged;
            _characterService.CurrentCharacter.CharacterStats.OnPurchasedDiamondsChanged += OnDiamondsChanged;
        }
        
        private void OnGoldChanged(int value)
        {
            _goldText.text =
                _characterService.CurrentCharacter.CharacterStats.Gold.ToString("N0",
                    CultureInfo.GetCultureInfo("de-DE"));
        }

        private void OnDiamondsChanged(int value)
        {
            _diamondsText.text =
                (_characterService.CurrentCharacter.CharacterStats.Diamonds +
                 _characterService.CurrentCharacter.CharacterStats.PurchasedDiamonds)
                .ToString("N0", CultureInfo.GetCultureInfo("de-DE"));
        }
    }
}
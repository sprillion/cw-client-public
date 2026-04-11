using infrastructure.services.players;
using TMPro;
using ui.tools;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ui.quickPanel
{
    public class StatsPanel : MonoBehaviour
    {
        [SerializeField] private ProgressBar _healthBar;
        [SerializeField] private ProgressBar _experienceBar;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _experienceText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _armorText;
        [SerializeField] private TMP_Text _attackSpeedText;
        
        [SerializeField] private RectTransform _statsTransform;
        
        private ICharacterService _characterService;

        [Inject]
        public void Construct(ICharacterService characterService)
        {
            _characterService = characterService;
            
            _characterService.CurrentCharacter.CharacterStats.OnMaxHealthChanged += _ => SetMaxHealth();
            _characterService.CurrentCharacter.CharacterStats.OnCurrentHealthChanged += _ => SetCurrentHealth();
            _characterService.CurrentCharacter.CharacterStats.OnNeededExperienceChanged += _ => SetNeededExperience();
            _characterService.CurrentCharacter.CharacterStats.OnExperienceChanged += _ => SetExperience();
            _characterService.CurrentCharacter.CharacterStats.OnLevelChanged += _ => SetLevel();
            _characterService.CurrentCharacter.CharacterStats.OnDamageChanged += _ => SetDamage();
            _characterService.CurrentCharacter.CharacterStats.OnArmorChanged += _ => SetArmor();
            _characterService.CurrentCharacter.CharacterStats.OnAttackSpeedChanged += _ => SetAttackSpeed();
        }

        public void Initialize()
        {
            SetMaxHealth();
            SetCurrentHealth();
            SetNeededExperience();
            SetExperience();
            SetLevel();
            SetArmor();
        }

        private void SetMaxHealth()
        {
            _healthBar.SetMaxValue(_characterService.CurrentCharacter.CharacterStats.MaxHealth);
        }

        private void SetCurrentHealth()
        {
            _healthBar.SetValue(_characterService.CurrentCharacter.CharacterStats.CurrentHealth, 0.1f);
        }

        private void SetNeededExperience()
        {
            _experienceBar.SetMaxValue(_characterService.CurrentCharacter.CharacterStats.NeededExperience);
        }

        private void SetExperience()
        {
            _experienceBar.SetValue(_characterService.CurrentCharacter.CharacterStats.Experience, 0.1f);
            var percent = (float)_characterService.CurrentCharacter.CharacterStats.Experience /
                _characterService.CurrentCharacter.CharacterStats.NeededExperience * 100;
            _experienceText.text = $"{percent:F2}%";
        }

        private void SetLevel()
        {
            _levelText.text = _characterService.CurrentCharacter.CharacterStats.Level.ToString();
        }

        private void SetArmor()
        {
            _armorText.text = $"{IconType.Armor.ToIcon()} {_characterService.CurrentCharacter.CharacterStats.Armor}%";
            UpdateStatsLayout();
        }
        private void SetDamage()
        {
            _damageText.text = $"{IconType.Damage.ToIcon()} {_characterService.CurrentCharacter.CharacterStats.Damage}";
            UpdateStatsLayout();
        }
        
        private void SetAttackSpeed()
        {
            _attackSpeedText.text = $"{IconType.AttackSpeed.ToIcon()} {_characterService.CurrentCharacter.CharacterStats.AttackSpeed}";
            UpdateStatsLayout();
        }

        private void UpdateStatsLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_statsTransform);
        }
    }
}
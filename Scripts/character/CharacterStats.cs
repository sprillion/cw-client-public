using System;
using infrastructure.services.npc;

namespace character
{
    public class CharacterStats
    {
        public int Gold { get; private set; } 
        public int Diamonds { get; private set; } 
        public int PurchasedDiamonds { get; private set; } 
        public int MaxHealth { get; private set; } 
        public int CurrentHealth { get; private set; } 
        public int Level { get; private set; } 
        public int NeededExperience { get; private set; }
        public int Experience { get; private set; }
        public int Armor { get; private set; }
        public int Damage { get; private set; }
        public float AttackSpeed { get; private set; }

        public event Action<int> OnGoldChanged; 
        public event Action<int> OnDiamondsChanged; 
        public event Action<int> OnPurchasedDiamondsChanged; 
        public event Action<int> OnMaxHealthChanged; 
        public event Action<int> OnCurrentHealthChanged; 
        public event Action<int> OnLevelChanged; 
        public event Action<int> OnNeededExperienceChanged; 
        public event Action<int> OnExperienceChanged; 
        public event Action<int> OnArmorChanged; 
        public event Action<int> OnDamageChanged; 
        public event Action<float> OnAttackSpeedChanged; 

        public void SetGold(int value)
        {
            var offset = value - Gold;
            Gold = value;
            OnGoldChanged?.Invoke(offset);
        }

        public void SetDiamonds(int value)
        {
            var offset = value - Diamonds;
            Diamonds = value;
            OnDiamondsChanged?.Invoke(offset);
        }

        public void SetPurchasedDiamonds(int value)
        {
            var offset = value - PurchasedDiamonds;
            PurchasedDiamonds = value;
            OnPurchasedDiamondsChanged?.Invoke(offset);
        }

        public void SetMaxHealth(int value)
        {
            var offset = value - MaxHealth;
            MaxHealth = value;
            OnMaxHealthChanged?.Invoke(offset);
        }
        
        public void SetCurrentHealth(int value)
        {
            var offset = value - CurrentHealth;
            if (offset == 0) return;
            CurrentHealth = value;
            OnCurrentHealthChanged?.Invoke(offset);
        }

        public void SetLevel(int value)
        {
            var offset = value - Level;
            Level = value;
            OnLevelChanged?.Invoke(offset);
        }
        
        public void SetNeededExperience(int value)
        {
            var offset = value - NeededExperience;
            NeededExperience = value;
            OnNeededExperienceChanged?.Invoke(offset);
        }

        public void SetExperience(int value)
        {
            var offset = value - Experience;
            Experience = value;
            OnExperienceChanged?.Invoke(offset);
        }
        
        public void SetArmor(int value)
        {
            var offset = value - Armor;
            Armor = value;
            OnArmorChanged?.Invoke(offset);
        }
        
        public void SetDamage(int value)
        {
            var offset = value - Damage;
            Damage = value;
            OnDamageChanged?.Invoke(offset);
        }
        
        public void SetAttackSpeed(float value)
        {
            var offset = value - AttackSpeed;
            AttackSpeed = value;
            OnAttackSpeedChanged?.Invoke(offset);
        }

        public bool HaveCurrency(CurrencyType currencyType, int value)
        {
            switch (currencyType)
            {
                case CurrencyType.Gold:
                    return Gold >= value;
                case CurrencyType.Diamonds:
                    return Diamonds + PurchasedDiamonds >= value;
                default:
                    return false;
            }
        }
    }
}
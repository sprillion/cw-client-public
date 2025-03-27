using character;
using character.handItems;
using infrastructure.services.inventory.items;

namespace factories.characters
{
    public interface ICharacterFactory
    {
        Character CreateCharacter();
        EnemyCharacter CreateEnemy(string nickname);
        CharacterStats CreateCharacterStats();
        Potion GetPotion(Item item);
    }
}
using infrastructure.services.inventory.items;

namespace character.handItems
{
    public class Potion : PooledObject
    {
        public Item CurrentItem { get; set; }

    }
}
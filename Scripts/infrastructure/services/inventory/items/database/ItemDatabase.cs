using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace infrastructure.services.inventory.items.database
{
    [CreateAssetMenu(menuName = "Data/Database/Item Database")]
    public class ItemDatabase : ScriptableDatabase<ItemData>
    {
        
    }
}
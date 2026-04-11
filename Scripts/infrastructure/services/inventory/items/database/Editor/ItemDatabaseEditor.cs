using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;

namespace infrastructure.services.inventory.items.database.Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : ScriptableItemDefaultEditor<ItemDatabaseEditorWindow,
        ItemDatabase, ItemData>
    {
        
    }
}
using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace infrastructure.services.inventory.items.database.Editor
{
    public class ItemDatabaseEditorWindow : DatabaseEditorWindow<ItemDatabase, ItemData>
    {
        
        private Button _duplicateButton;
        
        [MenuItem("Window/Database/Items Editor")]
        public static void OpenEditor()
        {
            ItemDatabaseEditorWindow wnd = GetWindow<ItemDatabaseEditorWindow>();
            wnd.titleContent = new GUIContent(wnd.GetWindowTitle());
        }
        
        public override string GetWindowTitle() => "Items Editor";

        public override void CreateGUI()
        {
            base.CreateGUI();
            _duplicateButton = rootVisualElement.Q<Button>("DuplicateElement");
            _duplicateButton.clicked += () => DuplicateEntry(null);
        }
    }
}
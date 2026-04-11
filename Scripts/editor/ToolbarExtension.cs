using network;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace editor
{
    [Overlay(typeof(SceneView), "Server Connection")]
    public class ToolbarExtension : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var data = GameResources.Data.connection_data<ConnectionData>();
            
            var root = new VisualElement();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            
            var toggle = new Toggle("Connect To Local Server")
            {
                value = data.ConnectToLocalServer
            };

            toggle.RegisterValueChangedCallback(e =>
            {
                data.ConnectToLocalServer = e.newValue;
                EditorUtility.SetDirty(data);
            });
            
            var enumField = new EnumField("Accaunt", data.AccountType);
            enumField.RegisterValueChangedCallback(evt =>
            {
                data.AccountType = (AccountType)evt.newValue;
                EditorUtility.SetDirty(data);
            });
            
            root.Add(toggle);
            root.Add(enumField);
            
            return root;
        }
    }
}
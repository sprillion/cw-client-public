using infrastructure.services.mapMarkers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ui.map
{
    public class MapIconToggle : PooledObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Toggle _toggle;

        public void Initialize(MapIconType type, IMapMarkerService service)
        {
            var data = service.GetIconData(type);
            if (data != null)
            {
                _icon.sprite = data.Icon;
                _icon.color = data.Color;
            }
            _label.text = $"Game/Map/{type}".Loc();
            _toggle.isOn = service.IsTypeVisible(type);
            _toggle.onValueChanged.AddListener(v => service.SetTypeVisible(type, v));
        }
    }
}

using infrastructure.services.house;
using infrastructure.services.interior;
using ui.house;
using UnityEngine;

namespace environment.interior
{
    public class Interior : MonoBehaviour
    {
        [SerializeField] private InteriorType _type;
        [SerializeField] private Vector3 _worldPosition;
        [SerializeField] private bool _disablePlayer;
        [SerializeField] private bool _hideOtherCharactersAndMobs;

        public InteriorType Type => _type;
        public bool DisablePlayer => _disablePlayer;
        public bool HideOtherCharactersAndMobs => _hideOtherCharactersAndMobs;

        public void Initialize(IInteriorService interiorService, IHouseService houseService = null)
        {
            transform.position = _worldPosition;

            foreach (var exit in GetComponentsInChildren<InteriorExit>())
                exit.Initialize(interiorService);

            if (houseService != null)
            {
                houseService.GetHouse();
                houseService.GetCurrentHouseUpgrades();

                var housePlaceButtonsView = GetComponentInChildren<HousePlaceButtonsView>(true);
                housePlaceButtonsView?.Initialize(houseService);
            }
        }
    }
}

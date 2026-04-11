using infrastructure.services.house;
using UnityEngine;

namespace environment.house
{
    public class HousePlaceObject : MonoBehaviour
    {
        [SerializeField] private HousePlaceType _type;
        [SerializeField] private Transform _buttonAnchor;

        // index 0 → level 1, index 1 → level 2, и т.д.
        // Если массив пуст — включается/выключается сам GameObject.
        [SerializeField] private GameObject[] _levelObjects;

        public HousePlaceType Type => _type;
        public Transform ButtonAnchor => _buttonAnchor;

        public void Refresh(int level)
        {
            if (_levelObjects == null || _levelObjects.Length == 0)
            {
                gameObject.SetActive(level >= 1);
                return;
            }

            for (int i = 0; i < _levelObjects.Length; i++)
            {
                if (_levelObjects[i] == null) continue;
                _levelObjects[i].SetActive(level >= 1 && i == level - 1);
            }
        }

        public bool CanShowButton(HousePlaceInfo info, int houseLevel)
        {
            if (info == null) return false;
            return info.RequiredHouseLevel == 0 || houseLevel >= info.RequiredHouseLevel;
        }
    }
}

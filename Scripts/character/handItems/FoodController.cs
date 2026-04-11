using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using UnityEngine;
using Zenject;

namespace character.handItems
{
    public class FoodController : MonoBehaviour
    {
        [SerializeField] private float _duration = 1f;
        
        [SerializeField] private Food _food;
        [SerializeField] private CharacterAnimator _characterAnimator;

        private IInputService _inputService;
        private IInventoryService _inventoryService;

        private float _timer;
        private bool _started;

        public Item CurrentItem { get; private set; }

        [Inject]
        public void Construct(IInputService inputService, IInventoryService inventoryService)
        {
            _inputService = inputService;
            _inventoryService = inventoryService;
        }
        
        private void Update()
        {
            if (CurrentItem == null) return;
            if (CurrentItem.IsCooldown) return;
            if (!_inputService.CursorIsLocked) return;
            if (!_started) return;
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                StopUse();
            }
        }

        private void OnEnable()
        {
            _food.gameObject.SetActive(true);
            _inputService.OnAttackEvent += StartUse;
            _inputService.OnStopAttackEvent += StopUse;
        }

        private void OnDisable()
        {
            _food.gameObject.SetActive(false);
            _inputService.OnAttackEvent -= StartUse;
            _inputService.OnStopAttackEvent -= StopUse;
        }

        public void SetItem(Item item)
        {
            CurrentItem = item;
            _food.SetFood(CurrentItem);
        }

        private void StartUse()
        {
            if (CurrentItem.IsCooldown) return;
            if (!_inputService.CursorIsLocked) return;
            _timer = 0;
            _started = true;
        }

        private void StopUse()
        {
            if (CurrentItem.IsCooldown) return;
            if (!_inputService.CursorIsLocked) return;
            _started = false;
            if (_timer >= _duration)
            {
                Use();
            }
        }

        private void Use()
        {
            CurrentItem.Use();
            _inventoryService.UseItem(CurrentItem);
        }   
    }
}
using character.handItems;
using DG.Tweening;
using factories.characters;
using infrastructure.factories;
using infrastructure.services.input;
using infrastructure.services.inventory;
using infrastructure.services.inventory.items;
using infrastructure.services.players;
using network;
using tools;
using UnityEngine;
using Zenject;

namespace character
{
    public class PotionThrowController : MonoBehaviour
    {
        [Header("Settings")] public float maxThrowDistance = 10f;
        public float throwHeight = 3f;
        public int trajectoryPoints = 30;
        public float throwSpeed = 5f;
        public LayerMask terrainMask;

        [Header("References")] public LineRenderer trajectoryLine;
        public GameObject landingMarker;
        public Transform throwOrigin;

        [SerializeField] private Transform _potionParent;
        [SerializeField] private CharacterAnimator _characterAnimator;

        private Camera _mainCamera;
        private Vector3 _targetPosition;
        private bool _isThrowing;
        private bool _validTarget;
        
        private Potion _currentPotion;

        private IInputService _inputService;
        private ICharacterFactory _characterFactory;
        private INetworkManager _networkManager;
        
        public Item CurrentItem { get; private set; }

        [Inject]
        public void Construct(IInputService inputService, ICharacterFactory characterFactory, INetworkManager networkManager)
        {
            _inputService = inputService;
            _characterFactory = characterFactory;
            _networkManager = networkManager;

            _characterAnimator.OnThrowingChanged += SendThrowing;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            trajectoryLine.positionCount = trajectoryPoints;
            landingMarker.SetActive(false);
            trajectoryLine.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isThrowing) return;
            if (CurrentItem.IsCooldown) return;
            if (!_inputService.CursorIsLocked) return;

            CreatePotion();
            UpdateTargetPosition();
            DrawTrajectory();
        }

        private void OnEnable()
        {
            _inputService.OnAttackEvent += LaunchThrow;
        }

        private void OnDisable()
        {
            _inputService.OnAttackEvent -= LaunchThrow;
            _characterAnimator.SetThrowing(false);
            if (!_isThrowing)
            {
                _currentPotion?.Release();
                _currentPotion = null;
            }
        }

        public void SetItem(Item item)
        {
            CurrentItem = item;
            if (CurrentItem.IsCooldown)
            {
                landingMarker.SetActive(false);
                trajectoryLine.gameObject.SetActive(false);
            }
        }

        private void CreatePotion()
        {
            if (_currentPotion != null) return;

            _currentPotion = Pool.Get<Potion>();
            _currentPotion.CurrentItem = CurrentItem;
            _currentPotion.SetParentPreserveScale(_potionParent);
            _currentPotion.transform.localPosition = Vector3.zero;
            _currentPotion.transform.localRotation = Quaternion.identity;
        }

        private void UpdateTargetPosition()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, maxThrowDistance, terrainMask))
            {
                _targetPosition = hit.point;
                landingMarker.SetActive(true);
                trajectoryLine.gameObject.SetActive(true);
                landingMarker.transform.position = _targetPosition;
                _validTarget = true;
            }
            else
            {
                landingMarker.SetActive(false);
                trajectoryLine.gameObject.SetActive(false);
                _validTarget = false;
            }
        }

        private void DrawTrajectory()
        {
            if (!_validTarget) return;

            _characterAnimator.SetThrowing(true);

            Vector3 start = throwOrigin.position;
            Vector3 end = _targetPosition;

            for (int i = 0; i < trajectoryPoints; i++)
            {
                float t = (float)i / (trajectoryPoints - 1);
                trajectoryLine.SetPosition(i, CalculateArcPoint(start, end, t));
            }
        }

        private Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t)
        {
            float parabola = 1 - Mathf.Pow(2 * t - 1, 2);
            Vector3 verticalOffset = Vector3.up * (parabola * throwHeight);
            return Vector3.Lerp(start, end, t) + verticalOffset;
        }

        private void LaunchThrow()
        {
            if (!gameObject.activeSelf || _isThrowing || !_validTarget || CurrentItem.IsCooldown ||
                !_inputService.CursorIsLocked) return;
            _characterAnimator.Throw();
            //ThrowObject().Forget();
            ThrowObjectTween();
        }

        private void ThrowObjectTween()
        {
            _isThrowing = true;
            landingMarker.SetActive(false);
            trajectoryLine.gameObject.SetActive(false);
            
            _currentPotion.transform.SetParent(null);
            _currentPotion.CurrentItem.Use();

            _currentPotion.transform.position = throwOrigin.position;

            Vector3[] pathPoints = new Vector3[trajectoryPoints];
            for (int i = 0; i < trajectoryPoints; i++)
            {
                float t = (float)i / (trajectoryPoints - 1);
                pathPoints[i] = CalculateArcPoint(throwOrigin.position, _targetPosition, t);
            }

            var distance = Vector3.Distance(throwOrigin.position, _targetPosition);
            var duration = distance / throwSpeed;

            _currentPotion.transform.DOPath(
                    pathPoints,
                    duration,
                    PathType.CatmullRom,
                    PathMode.Full3D,
                    10
                )
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _isThrowing = false;
                    _currentPotion = null;
                });

            var rotation = Mathf.Lerp(0, 720, Mathf.InverseLerp(5, maxThrowDistance, distance));
            _currentPotion.transform.DORotate(new Vector3(rotation, 0, 0), duration, RotateMode.FastBeyond360);
            
            SendThrowPotion(duration);
        }
        
        private void SendThrowing(bool isThrowing)
        {
            var message = new Message(MessageType.Character);
            message.AddByte(CharacterService.FromClientMessage.Throwing.ToByte())
                .AddBool(isThrowing);
            
            _networkManager.SendMessage(message);
        }

        private void SendThrowPotion(float duration)
        {
            var message = new Message(MessageType.Inventory);
            message.AddByte(InventoryService.FromClientMessages.UseItem.ToByte());
            message.AddInt(CurrentItem.Id)
                .AddInt(CurrentItem.Slot)
                .AddVector3(throwOrigin.position)
                .AddVector3(_targetPosition)
                .AddFloat(duration);

            _networkManager.SendMessage(message);
        }

        // private async UniTaskVoid ThrowObject()
        // {
        //     isThrowing = true;
        //     landingMarker.SetActive(false);
        //     trajectoryLine.gameObject.SetActive(false);
        //
        //     Vector3 startPos = throwOrigin.position;
        //     Vector3 endPos = targetPosition;
        //     float duration = Vector3.Distance(startPos, endPos) / throwSpeed;
        //     float elapsed = 0;
        //     
        //     _currentPotion.transform.transform.SetParent(null);
        //     _currentPotion.CurrentItem.Use();
        //
        //     while (elapsed < duration)
        //     {
        //         elapsed += Time.deltaTime;
        //         float t = elapsed / duration;
        //         _currentPotion.transform.position = CalculateArcPoint(startPos, endPos, t);
        //         await UniTask.DelayFrame(1);
        //     }
        //
        //     _currentPotion.transform.position = endPos;
        //     _currentPotion = null;
        //     isThrowing = false;
        // }
    }
}
using System;
using character;
using Cysharp.Threading.Tasks;
using environment.interior;
using infrastructure.services.house;
using infrastructure.services.input;
using infrastructure.services.mobs;
using infrastructure.services.players;
using infrastructure.services.ui;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace infrastructure.services.interior
{
    public class InteriorService : IInteriorService
    {
        private readonly ICharacterService _characterService;
        private readonly IMobService _mobService;
        private readonly IHouseService _houseService;
        private readonly IInputService _inputService;
        private readonly IUiService _uiService;

        private AsyncOperationHandle<SceneInstance> _loadedSceneHandle;
        private Interior _currentInterior;
        private Camera _cachedMainCamera;

        public bool IsInInterior { get; private set; }
        public event Action OnEntered;
        public event Action OnExited;

        public InteriorService(ICharacterService characterService, IMobService mobService,
                               IHouseService houseService, IInputService inputService,
                               IUiService uiService)
        {
            _characterService = characterService;
            _mobService = mobService;
            _houseService = houseService;
            _inputService = inputService;
            _uiService = uiService;
        }

        public async UniTask Enter(InteriorType interiorType)
        {
            if (IsInInterior) return;

            _loadedSceneHandle = Addressables.LoadSceneAsync(
                $"Interior/{interiorType}",
                LoadSceneMode.Additive,
                activateOnLoad: false
            );
            await _loadedSceneHandle.Task;

            if (_loadedSceneHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[InteriorService] Failed to load interior scene: {interiorType}");
                return;
            }

            // Activate after load so SceneContext resolves parent contract name (MainSceneContext)
            await _loadedSceneHandle.Result.ActivateAsync();

            var scene = _loadedSceneHandle.Result.Scene;
            _currentInterior = FindInteriorInScene(scene);

            if (_currentInterior == null)
            {
                Debug.LogError($"[InteriorService] No Interior component found in scene: {interiorType}");
                return;
            }

            if (_currentInterior.DisablePlayer)
                DisablePlayer();

            if (_currentInterior.DisableAllCanvases)
                _uiService.SetAllCanvasesActive(false);

            if (_currentInterior.HideOtherCharactersAndMobs)
                SetOthersVisible(false);

            if (_currentInterior.HasOwnCamera)
                SetMainCameraActive(false);

            _inputService.UnlockCursor(true);
            _inputService.DisableFullInput();
            
            IsInInterior = true;
            OnEntered?.Invoke();
        }

        public void Exit()
        {
            if (!IsInInterior) return;

            if (_currentInterior != null)
            {
                if (_currentInterior.DisablePlayer)
                    EnablePlayer();

                if (_currentInterior.DisableAllCanvases)
                    _uiService.SetAllCanvasesActive(true);

                if (_currentInterior.HideOtherCharactersAndMobs)
                    SetOthersVisible(true);

                if (_currentInterior.HasOwnCamera)
                    SetMainCameraActive(true);
            }

            _inputService.LockCursor(true);
            _inputService.EnableFullInput();

            Addressables.UnloadSceneAsync(_loadedSceneHandle);

            _currentInterior = null;
            IsInInterior = false;
            OnExited?.Invoke();
        }

        private Interior FindInteriorInScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var interior = root.GetComponentInChildren<Interior>(true);
                if (interior != null) return interior;
            }
            return null;
        }

        private void DisablePlayer()
        {
            _characterService.CurrentCharacter?.CharacterMovement.DisableMovement();
        }

        private void EnablePlayer()
        {
            _characterService.CurrentCharacter?.CharacterMovement.EnableMovement();
        }

        private void SetMainCameraActive(bool active)
        {
            if (active)
            {
                if (_cachedMainCamera != null)
                    _cachedMainCamera.gameObject.SetActive(true);
            }
            else
            {
                _cachedMainCamera = Camera.main;
                _cachedMainCamera?.gameObject.SetActive(false);
            }
        }

        private void SetOthersVisible(bool visible)
        {
            foreach (var pair in _characterService.OtherCharacters)
                pair.Value.SetVisible(visible);

            foreach (var pair in _mobService.Mobs)
                pair.Value.gameObject.SetActive(visible);
        }
    }
}

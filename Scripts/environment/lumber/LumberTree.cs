using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using infrastructure.services.lumber;
using UnityEngine;
using Zenject;

namespace environment.lumber
{
    public class LumberTree : PooledObject, IInteractable
    {
        [SerializeField] private GameObject _grownTree;
        [SerializeField] private GameObject _sprout;

        [field: SerializeField] public bool DisablePanel { get; private set; }
        [field: SerializeField] public bool DisableButton { get; private set; }
        public int TreeId { get; set; }

        public event Action<IInteractable> OnDestroyed;

        private ILumberService _lumberService;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(ILumberService lumberService)
        {
            _lumberService = lumberService;
        }

        private void OnEnable()
        {
            if (_lumberService == null) return;
            _lumberService.OnInfoUpdated += OnServiceUpdated;
            _lumberService.OnCooldown += OnServiceUpdated;
        }

        private void OnDisable()
        {
            if (_lumberService == null) return;
            _lumberService.OnInfoUpdated -= OnServiceUpdated;
            _lumberService.OnCooldown -= OnServiceUpdated;

            // CancelCooldown();
        }

        public void Interact()
        {
        }

        public override void Release()
        {
            CancelCooldown();
            base.Release();
        }

        private void OnServiceUpdated()
        {
            if (_lumberService.CurrentTreeId != TreeId) return;

            CancelCooldown();

            var hasCooldown = _lumberService.CooldownRemaining > 0;
            SetState(hasCooldown);

            if (hasCooldown)
            {
                _cts = new CancellationTokenSource();
                WaitCooldown(_lumberService.CooldownRemaining, _cts.Token).Forget();
            }
        }

        private void SetState(bool hasCooldown)
        {
            _grownTree.SetActive(!hasCooldown);
            _sprout.SetActive(hasCooldown);
        }

        private async UniTaskVoid WaitCooldown(float duration, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
            SetState(false);
        }

        private void CancelCooldown()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}

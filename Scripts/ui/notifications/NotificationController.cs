using DG.Tweening;
using infrastructure.services.loading;
using infrastructure.services.players;
using UnityEngine;
using Zenject;

namespace ui.notifications
{
    public class NotificationController : MonoBehaviour
    {
        private ICharacterService _characterService;
        private ILoadingService _loadingService;

        private ObjectPool _pool;
        private Notification _notificationPrefab;
        private NotificationData _notificationData;

        private int _currentCount;

        [Inject]
        public void Construct(ICharacterService characterService, ILoadingService loadingService)
        {
            _characterService = characterService;
            _loadingService = loadingService;
            _notificationPrefab = Resources.Load<Notification>("Prefabs/Ui/Notifications/Notification");
            _notificationData = Resources.Load<NotificationData>("Data/Notifications/NotificationData");

            _pool = new ObjectPool(_notificationPrefab, 0, transform);

            _characterService.CurrentCharacter.CharacterStats.OnCurrentHealthChanged += OnHealthChanged;
            _characterService.CurrentCharacter.CharacterStats.OnGoldChanged += OnGoldChanged;
            _characterService.CurrentCharacter.CharacterStats.OnDiamondsChanged += OnDiamondsChanged;
            _characterService.CurrentCharacter.CharacterStats.OnPurchasedDiamondsChanged += OnDiamondsChanged;
            _characterService.CurrentCharacter.CharacterStats.OnExperienceChanged += OnExperienceAdded;
        }

        private void OnHealthChanged(int value)
        {
            AddNotification(NotificationType.Health, value >= 0, value);
        }
        
        private void OnGoldChanged(int value)
        {
            AddNotification(NotificationType.Gold, value >= 0, value);
        }
        
        private void OnDiamondsChanged(int value)
        {
            AddNotification(NotificationType.Diamond, value >= 0, value);
        }

        private void OnExperienceAdded(int value)
        {
            AddNotification(NotificationType.Exp, true, value);
        }

        private void AddNotification(NotificationType notificationType, bool isPositive, int value)
        {
            if (_loadingService.IsLoading) return;
            
            _currentCount++;
            
            var notification = _pool.GetObject<Notification>();
            notification.SetValue(_notificationData.GetNotificationData(notificationType), isPositive, value);
            
            LaunchNotification(notification);
        }

        private void LaunchNotification(Notification notification)
        {
            notification.Rect.anchoredPosition = ((RectTransform)transform).anchoredPosition + Vector2.down * 40 * _currentCount;
            notification.CanvasGroup.alpha = 1;

            notification.Rect.DOAnchorPos(notification.Rect.anchoredPosition + Vector2.up * 300, 3)
                .SetEase(Ease.OutCubic).OnComplete(
                    () =>
                    {
                        notification.Release();
                        _currentCount--;
                    });
            notification.CanvasGroup.DOFade(0, 1).SetDelay(2).SetEase(Ease.Linear);
        }
    }
}
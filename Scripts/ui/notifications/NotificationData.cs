using System;
using System.Collections.Generic;
using System.Linq;
using ui.tools;
using UnityEngine;

namespace ui.notifications
{
    [CreateAssetMenu(fileName = "NotificationData", menuName = "Data/Notifications/NotificationData")]
    public class NotificationData : ScriptableObject
    {
        [SerializeField] private List<Notify> _notifies;

        public Notify GetNotificationData(NotificationType type)
        {
            return _notifies.First(n => n.NotificationType == type);
        }
    }

    public enum NotificationType
    {
        Gold,
        Diamond,
        Health,
        Exp,
        Level
    }

    [Serializable]
    public class Notify
    {
        public NotificationType NotificationType;
        public IconType IconType;
        public Color PositiveColor;
        public Color NegativeColor;
    }
}
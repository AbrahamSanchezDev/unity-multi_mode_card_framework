using System;

namespace CardFramework.Core.Models {
    public enum NotificationType {
        Mail,
        Achievement,
        Event
    }

    public class NotificationItem {
        public string Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RewardAmount { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
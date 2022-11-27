using System;
namespace SloperMobile
{
    public enum NotificationTypes { OverlayProgress = 1, DMProgress }

    public interface INotificationService
    {
        NotificationTypes NotificationType { get; set; }
        void UpdateProgress(string title, float progress, string contentText = null);
        void EndProgress();
    }
}

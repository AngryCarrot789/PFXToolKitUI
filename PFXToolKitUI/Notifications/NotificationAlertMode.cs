namespace PFXToolKitUI.Notifications;

/// <summary>
/// An alert state for a notification, e.g. something went wrong, so the alert state will be set to pull the user's attention
/// </summary>
public enum NotificationAlertMode {
    /// <summary>
    /// No alert
    /// </summary>
    None,
    /// <summary>
    /// Continue alert until user interacts with the notification (e.g. cursor over)
    /// </summary>
    UntilUserInteraction,
    /// <summary>
    /// Alert runs forever until manually disabled (e.g. set to <see cref="None"/>) or the notification is hidden
    /// </summary>
    Forever
}
namespace PFXToolKitUI.Notifications;

/// <summary>
/// A mode for a notification's alert state, signaling a warning notification
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
    /// Alert runs forever until manually disabled (e.g. set to <see cref="None"/>)
    /// </summary>
    Forever
}
namespace PFXToolKitUI.Services;

/// <summary>
/// A service for showing an "About this application" popup to the user
/// </summary>
public interface IAboutService {
    /// <summary>
    /// Shows the "about" information. This may show as a dialog,
    /// in which case, the task is completed when the dialog closes
    /// </summary>
    /// <returns></returns>
    Task ShowDialog();
}
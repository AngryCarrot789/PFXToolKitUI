using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Services;

/// <summary>
/// A service available on desktop platforms, e.g. windows. Provides utilities for accessing windows, absolute cursor positions, etc.
/// </summary>
public interface IDesktopService {
    /// <summary>
    /// Tries to get the application's desktop service instance. Returns false if the app isn't running on desktop
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static bool TryGetInstance([NotNullWhen(true)] out IDesktopService? service) {
        return ApplicationPFX.Instance.ServiceManager.TryGetService(out service);
    }
    
    /// <summary>
    /// Tries to get the window that is currently activated (focused), falling back to the main window if allowed
    /// </summary>
    /// <param name="window">The found window</param>
    /// <param name="fallbackToMainWindow">True to use the main window if no focused window is found</param>
    /// <returns>True if a window was found</returns>
    bool TryGetActiveWindow([NotNullWhen(true)] out Window? window, bool fallbackToMainWindow = true);

    /// <summary>
    /// Sets the absolute mouse cursor position
    /// </summary>
    /// <param name="x">New Mouse X position</param>
    /// <param name="y">New Mouse X position</param>
    void SetCursorPosition(int x, int y);
}
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Interactivity.Windowing;

/// <summary>
/// An interface that manages components for a top-level. Top levels are abstractions around "windows"
/// </summary>
public interface ITopLevelComponentManager : IComponentManager {
    public static readonly DataKey<ITopLevelComponentManager> TLCManagerDataKey = DataKey<ITopLevelComponentManager>.Create(nameof(ITopLevelComponentManager));

    /// <summary>
    /// Tries to get the clipboard component
    /// </summary>
    /// <param name="clipboard">The clipboard</param>
    /// <returns>True if the top level supports a clipboard</returns>
    bool TryGetClipboard([NotNullWhen(true)] out IClipboardService? clipboard) => this.TryGetComponent(out clipboard);

    /// <summary>
    /// Tries to get the web launcher
    /// </summary>
    /// <param name="launcer">The launcher</param>
    /// <returns>True if the top level supports launching things in a web browser</returns>
    bool TryGetWebLauncher([NotNullWhen(true)] out IWebLauncher? launcher) => this.TryGetComponent(out launcher);
}
using System.Diagnostics.CodeAnalysis;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.DesktopImpl;

public sealed class DesktopTopLevelComponentManagerImpl : ITopLevelComponentManager {
    public IComponentManager ComponentManager { get; }
    
    public DesktopTopLevelComponentManagerImpl(DesktopWindowImpl window) {
        this.ComponentManager = window;
    }

    public bool TryGetClipboard([NotNullWhen(true)] out IClipboardService? clipboard) {
        return this.ComponentManager.TryGetComponent(out clipboard);
    }

    public bool TryGetWebLauncher([NotNullWhen(true)] out IWebLauncher? launcher) {
        return this.ComponentManager.TryGetComponent(out launcher);
    }
}
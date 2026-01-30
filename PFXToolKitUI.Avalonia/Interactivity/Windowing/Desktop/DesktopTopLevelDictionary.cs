using PFXToolKitUI.Interactivity.Windowing;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;

public class DesktopTopLevelDictionary<T> : TopLevelDictionary<T> where T : class, IDesktopWindow {
    private EventHandler<WindowCloseEventArgs>? m_WindowOnClosed;

    public DesktopTopLevelDictionary() {
    }

    protected override void OnTopLevelAdded(TopLevelIdentifier identifier, T topLevel) {
        topLevel.Closed += this.m_WindowOnClosed ??= this.WindowOnClosed;
        base.OnTopLevelAdded(identifier, topLevel);
    }

    protected override void OnTopLevelRemoved(TopLevelIdentifier identifier, T topLevel) {
        topLevel.Closed -= this.m_WindowOnClosed;
        base.OnTopLevelRemoved(identifier, topLevel);
    }
    
    private void WindowOnClosed(object? sender, WindowCloseEventArgs e) {
        this.RemoveTopLevel(new KeyValuePair<TopLevelIdentifier, T>(this.GetIdentifier((T) sender!), (T) sender!));
    }
}
using System.Runtime.CompilerServices;
using PFXToolKitUI.Avalonia.Shortcuts.Converters;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public class DynamicShortcutToGestureExtension {
    public DynamicShortcutToGestureExtension() {
    }

    public DynamicShortcutToGestureExtension(string shortcutId) => this.ShortcutId = shortcutId;

    public string? ShortcutId { get; set; }

    public object? ProvideValue(IServiceProvider serviceProvider) {
        return ProvideValue(serviceProvider, this.ShortcutId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object? ProvideValue(IServiceProvider serviceProvider, object? resourceKey) {
        if (resourceKey == null)
            throw new ArgumentException("DynamicShortcutsExtension.ResourceKey must be set.");

        if (ShortcutIdToGestureConverter.ShortcutIdToGesture(resourceKey.ToString() ?? "", null, out string? gesture))
            return gesture;

        return "";
    }
}
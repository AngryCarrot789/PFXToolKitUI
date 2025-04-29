using System.Runtime.CompilerServices;
using PFXToolKitUI.Avalonia.Shortcuts.Converters;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public class DynamicCommandToGestureExtension {
    public DynamicCommandToGestureExtension() {
    }

    public DynamicCommandToGestureExtension(string commandId) => this.CommandId = commandId;

    public string? CommandId { get; set; }

    public object? ProvideValue(IServiceProvider serviceProvider) {
        return ProvideValue(serviceProvider, this.CommandId);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object? ProvideValue(IServiceProvider serviceProvider, object? cmdId) {
        if (cmdId == null)
            throw new ArgumentException("DynamicShortcutsExtension.ResourceKey must be set.");

        if (CommandIdToGestureConverter.CommandIdToGesture(cmdId.ToString() ?? "", null, out string? gesture))
            return gesture;

        return "";
    }
}
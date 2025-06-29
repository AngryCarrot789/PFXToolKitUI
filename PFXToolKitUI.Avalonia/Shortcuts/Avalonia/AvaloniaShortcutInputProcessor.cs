// #define PRINT_DEBUG_KEYSTROKES

using Avalonia;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public class AvaloniaShortcutInputProcessor : ShortcutInputProcessor {
    internal bool isProcessingKey;

    public new AvaloniaShortcutManager Manager => (AvaloniaShortcutManager) base.Manager;

    /// <summary>
    /// The dependency object that was involved during the input event. This is usually the focused element
    /// during a key event, or the element the mouse was over during a mouse event (via hit testing)
    /// </summary>
    public AvaloniaObject? CurrentTargetObject { get; private set; }

    private IContextData? lazyCurrentContextData;

    public AvaloniaShortcutInputProcessor(AvaloniaShortcutManager manager) : base(manager) {
    }

    public void BeginInputProcessing(AvaloniaObject target) {
        this.CurrentTargetObject = target;
    }

    private void EndInputProcessing() {
        this.lazyCurrentContextData = null;
        this.CurrentTargetObject = null;
    }

    public void OnInputSourceKeyEvent(AvaloniaShortcutInputProcessor processor, InputElement focused, KeyEventArgs e, Key key, bool isRelease, bool isRepeat) {
        KeyModifiers mods = ShortcutUtils.IsModifierKey(key) ? KeyModifiers.None : e.KeyModifiers;
        KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);

        // Some controls don't want to allow processing shortcuts that have no modifier keys,
        // e.g. a text box, if a shortcut is activated by pressing G, you can never type G into it.
        // If there's a shortcut CTRL+C, we try to run a shortcut first, then fallback to letting the TB handle it
        if (stroke.Modifiers == 0 && UIInputManager.GetIsKeyShortcutProcessingBlocked(focused)) {
            return;
        }

        try {
            this.isProcessingKey = true;
            this.BeginInputProcessing(focused);

            e.Handled = processor.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, isRepeat);
        }
        finally {
            this.isProcessingKey = false;
            this.EndInputProcessing();
        }
    }

    public override IContextData? GetCurrentContext() {
        if (this.lazyCurrentContextData != null)
            return this.lazyCurrentContextData;

        if (this.CurrentTargetObject == null)
            return null;

        return this.lazyCurrentContextData = DataManager.GetFullContextData(this.CurrentTargetObject);
    }
}
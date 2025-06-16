// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PFXToolKitUI.Avalonia.Utils;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

/// <summary>
/// A class which manages the WPF inputs and global focus scopes. This class also dispatches input events to the shortcut system
/// </summary>
public class UIInputManager {
    public delegate void FocusedPathChangedEventHandler(string? oldPath, string? newPath, bool isCausedByFocusPathPropertyChanging);

    private static readonly AttachedProperty<AvaloniaShortcutInputProcessor?> ShortcutProcessorProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, AvaloniaShortcutInputProcessor?>("ShortcutProcessor");

    /// <summary>
    /// A dependency property for a control's focus path. This is the full path, and
    /// should usually be set such that it is relative to a parent control's focus path
    /// </summary>
    public static readonly AttachedProperty<string?> FocusPathProperty = AvaloniaProperty.RegisterAttached<UIInputManager, AvaloniaObject, string?>("FocusPath", inherits: true);

    /// <summary>
    /// A property used to determine if shortcut processing is disabled on an object. Default value is false,
    /// but you can override the metadata for specific controls and set the default value to true.
    /// <para>
    /// This is overridden to true for things like text boxes and text presenters
    /// </para>
    /// </summary>
    public static readonly AttachedProperty<bool> IsKeyShortcutProcessingBlockedProperty = AvaloniaProperty.RegisterAttached<UIInputManager, AvaloniaObject, bool>("IsKeyShortcutProcessingBlocked");

    public static readonly AttachedProperty<WeakReference?> CurrentFocusedElementProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, WeakReference?>("CurrentFocusedElement");
    public static readonly AttachedProperty<WeakReference?> LastFocusedElementProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, WeakReference?>("LastFocusedElement");

    /// <summary>
    /// A property for whether a control (whose <see cref="FocusPathProperty"/> has a value) currently has focus
    /// </summary>
    public static readonly AttachedProperty<bool> IsFocusedProperty = AvaloniaProperty.RegisterAttached<UIInputManager, AvaloniaObject, bool>("IsFocused");

    public static InputElement? GetCurrentFocusedElement(TopLevel obj) => obj.GetValue(CurrentFocusedElementProperty)?.Target as InputElement;
    public static InputElement? GetLastFocusedElement(TopLevel obj) => obj.GetValue(LastFocusedElementProperty)?.Target as InputElement;

    public static void SetIsFocused(AvaloniaObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

    public static bool GetIsFocused(AvaloniaObject obj) => obj.GetValue(IsFocusedProperty);

    // /// <summary>
    // /// A dependency property which is used to tells the input system that shortcuts key strokes can be processed when the focused element is the base WPF text editor control
    // /// </summary>
    // public static readonly AvaloniaProperty IsInheritedFocusAllowedProperty = AvaloniaProperty.RegisterAttached("IsInheritedFocusAllowed", typeof(bool), typeof(UIInputManager), BoolBox.True);
    //
    // /// <summary>
    // /// A dependency property used to check if a control blocks shortcut key processing. This is false by default for most
    // /// standard controls, but is true for things like text boxes and rich text boxes (but can be set back to false explicitly)
    // /// </summary>
    // public static readonly AvaloniaProperty IsKeyShortcutProcessingBlockedProperty = AvaloniaProperty.RegisterAttached("IsKeyShortcutProcessingBlocked", typeof(bool), typeof(UIInputManager), BoolBox.False);
    //
    // /// <summary>
    // /// A dependency property which is the same as <see cref="IsKeyShortcutProcessingBlockedProperty"/> except for mouse strokes
    // /// </summary>
    // public static readonly AvaloniaProperty IsMouseProcessingBlockedProperty = AvaloniaProperty.RegisterAttached("IsMouseProcessingBlocked", typeof(bool), typeof(UIInputManager), new PropertyMetadata(default(bool)));

    public static UIInputManager Instance { get; } = new UIInputManager();


    public static event FocusedPathChangedEventHandler? OnFocusedPathChanged;

    private static readonly WeakReference currentlyFocusedObject = new WeakReference(null);

    public static object? CurrentFocusedObject => currentlyFocusedObject.Target;

    public string? FocusedPath { get; private set; }

    private static readonly SortedList<Key, int> RepeatCounter;

    private UIInputManager() {
        if (Instance != null)
            throw new InvalidOperationException();
    }

    static UIInputManager() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        WindowBase.IsActiveProperty.Changed.AddClassHandler<WindowBase, bool>((o, args) => OnWindowActivated(o, args.NewValue.GetValueOrDefault()));
        InputElement.GotFocusEvent.AddClassHandler<TopLevel>((s, e) => OnGotOrLostFocus(s, e, false), handledEventsToo: true);
        InputElement.LostFocusEvent.AddClassHandler<TopLevel>((s, e) => OnGotOrLostFocus(s, e, true), handledEventsToo: true);

        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnTopLevelPreviewKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnTopLevelKeyDown, handledEventsToo: true);
        InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnTopLevelPreviewKeyUp, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.KeyUpEvent.AddClassHandler<TopLevel>(OnTopLevelKeyUp, handledEventsToo: true);

        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(OnTopLevelPreviewPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerPressedEvent.AddClassHandler<TopLevel>(OnTopLevelPointerPressed, handledEventsToo: true);
        InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(OnTopLevelPreviewPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(OnTopLevelPointerReleased, handledEventsToo: true);

        FocusPathProperty.Changed.AddClassHandler<AvaloniaObject, string?>(OnFocusPathChanged);

        IsKeyShortcutProcessingBlockedProperty.OverrideMetadata(typeof(TextBox), new StyledPropertyMetadata<bool>(true));
        IsKeyShortcutProcessingBlockedProperty.OverrideMetadata(typeof(TextPresenter), new StyledPropertyMetadata<bool>(true));
        RepeatCounter = new SortedList<Key, int>();
        // AvalonEdit
        // IsKeyShortcutProcessingBlockedProperty.OverrideMetadata(typeof(TextEditor), new StyledPropertyMetadata<bool>(true));
        // IsKeyShortcutProcessingBlockedProperty.OverrideMetadata(typeof(TextArea), new StyledPropertyMetadata<bool>(true));
    }

    private static void OnTopLevelPreviewPointerPressed(TopLevel sender, PointerPressedEventArgs e) {
        if (!(e.Source is InputElement element)) {
            return;
        }
    }

    // Mouse Events

    private static void OnTopLevelPointerPressed(TopLevel sender, PointerPressedEventArgs e) {
    }

    private static void OnTopLevelPreviewPointerReleased(TopLevel sender, PointerReleasedEventArgs e) {
        // Example: A button click will be handled after this method returns
    }

    private static void OnTopLevelPointerReleased(TopLevel sender, PointerReleasedEventArgs e) {
        // Example: A button click will have been processed before this method
    }

    // Key Events

    private static void OnTopLevelPreviewKeyDown(TopLevel sender, KeyEventArgs e) => OnTopLevelKeyEvent(sender, e, isRelease: false, isPreview: true);

    private static void OnTopLevelKeyDown(TopLevel sender, KeyEventArgs e) => OnTopLevelKeyEvent(sender, e, isRelease: false, isPreview: false);

    private static void OnTopLevelPreviewKeyUp(TopLevel sender, KeyEventArgs e) => OnTopLevelKeyEvent(sender, e, isRelease: true, isPreview: true);

    private static void OnTopLevelKeyUp(TopLevel sender, KeyEventArgs e) => OnTopLevelKeyEvent(sender, e, isRelease: true, isPreview: false);

    private static void OnTopLevelKeyEvent(TopLevel sender, KeyEventArgs e, bool isRelease, bool isPreview) {
        // we only want to handle shortcuts in preview events, since we will
        // have the target element but its non-preview typical handler will
        // not have been delivered yet
        if (isPreview || e.Handled)
            return;

        Key key = e.Key;
        if (key == Key.DeadCharProcessed || key == Key.System || key == Key.None)
            return;

        AvaloniaShortcutInputProcessor processor = GetShortcutProcessor(sender);
        if (processor.isProcessingKey)
            return;

        bool isRepeat = false;
        if (isRelease) {
            RepeatCounter[key] = 0;
        }
        else {
            int keyIndex = RepeatCounter.IndexOfKey(key);
            if (keyIndex == -1) {
                RepeatCounter[key] = 1;
                isRepeat = false;
            }
            else {
                int count = RepeatCounter.GetValueAtIndex(keyIndex);
                RepeatCounter.SetValueAtIndex(keyIndex, count + 1);
                isRepeat = count > 0;
            }
        }

        InputElement? focused = GetCurrentFocusedElement(sender);
        if (!ReferenceEquals(e.Source, sender)) {
            if (focused != null && !ReferenceEquals(focused, e.Source)) {
                // hmm
                // Debugger.Break();
                Debug.WriteLine($"Focused element (tracked by {nameof(UIInputManager)}) differs from key event source. {focused} != {e.Source}");
            }
        }

        InputElement? element = focused ?? e.Source as InputElement;
        if (element != null) {
            processor.OnInputSourceKeyEvent(processor, element, e, key, isRelease, isRepeat);
        }

        // If OnInputSourceKeyEvent becomes async for some reason,
        // e.Handled will be set to true in assumption that work is being done
        // if (processor.isProcessingKey)
        //     e.Handled = true;
    }

    private static AvaloniaShortcutInputProcessor GetShortcutProcessor(TopLevel topLevel) {
        AvaloniaShortcutInputProcessor? processor = topLevel.GetValue(ShortcutProcessorProperty);
        if (processor == null)
            topLevel.SetValue(ShortcutProcessorProperty, processor = new AvaloniaShortcutInputProcessor(AvaloniaShortcutManager.AvaloniaInstance));
        return processor;
    }

    public static void SetIsKeyShortcutProcessingBlocked(AvaloniaObject obj, bool value) => obj.SetValue(IsKeyShortcutProcessingBlockedProperty, value);
    public static bool GetIsKeyShortcutProcessingBlocked(AvaloniaObject obj) => obj.GetValue(IsKeyShortcutProcessingBlockedProperty);

    #region Focus managing

    private static void OnWindowActivated(WindowBase window, bool isActive) {
        if (isActive && GetCurrentFocusedElement(window) == null) {
            bool isFocusable = window.Focusable;
            if (!isFocusable) {
                window.Focusable = true;
            }

            window.Focus(NavigationMethod.Pointer);
            if (!isFocusable) {
                window.Focusable = false;
            }
        }
    }

    private static void OnGotOrLostFocus(TopLevel topLevel, RoutedEventArgs e, bool lost) {
        InputElement? element = e.Source as InputElement;
        if (element == null) {
            return;
        }

        string? oldFocusPath = Instance.FocusedPath, newFocusPath;

        WeakReference? last = topLevel.GetValue(LastFocusedElementProperty);
        if (last == null)
            topLevel.SetValue(LastFocusedElementProperty, last = new WeakReference(null));

        WeakReference? curr = topLevel.GetValue(CurrentFocusedElementProperty);
        if (curr == null)
            topLevel.SetValue(CurrentFocusedElementProperty, curr = new WeakReference(null));

        object? lastTarget = curr.Target;
        last.Target = lastTarget;
        if (lost) {
            newFocusPath = null;
            curr.Target = null;
            string msg = "";
            if (!ReferenceEquals(lastTarget, element)) {
                msg = $" (error: differing lastTarget from LostFocus source '{element.GetType().Name}')";
            }

            Debug.WriteLine($"Focus LOST: '{lastTarget?.GetType().Name ?? "null"}'{msg}");
        }
        else {
            curr.Target = element;
            string msg = "";
            if (lastTarget != null) {
                msg = $" (ERROR: lastTarget still valid '{lastTarget.GetType().Name}')";
            }

            newFocusPath = GetFocusPath(element);
            Debug.WriteLine($"Focus GAINED: null -> '{curr.Target?.GetType().Name ?? "null"}{(newFocusPath != null ? $" (FPath: {newFocusPath})" : "")}'{msg}");
        }

        if (oldFocusPath != newFocusPath) {
            Instance.FocusedPath = newFocusPath;
            OnFocusedPathChanged?.Invoke(oldFocusPath, newFocusPath, false);
            UpdateCurrentlyFocusedObject(element, newFocusPath);
        }
    }

    private static void OnFocusPathChanged(AvaloniaObject obj, AvaloniaPropertyChangedEventArgs<string?> e) {
        string? oldPath = e.OldValue.GetValueOrDefault();
        if (Instance.FocusedPath != oldPath) {
            return;
        }

        string? newPath = e.NewValue.GetValueOrDefault();
        if (oldPath == newPath) {
            return;
        }

        TopLevel? topLevel = obj as TopLevel ?? (obj as Visual)?.GetVisualRoot() as TopLevel;
        if (topLevel != null && GetCurrentFocusedElement(topLevel) == obj) {
            Instance.FocusedPath = newPath;
            OnFocusedPathChanged?.Invoke(oldPath, newPath, false);
            UpdateCurrentlyFocusedObject(obj, newPath);
            Debug.WriteLine($"Focus Path swapped for '{obj.GetType().Name}'");
        }
    }

    /// <summary>
    /// Sets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element and its visual tree
    /// </summary>
    public static void SetFocusPath(AvaloniaObject element, string value) => element.SetValue(FocusPathProperty, value);

    /// <summary>
    /// Gets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element and its visual tree
    /// </summary>
    public static string? GetFocusPath(AvaloniaObject element) => element.GetValue(FocusPathProperty);

    /// <summary>
    /// Looks through the given dependency object's parent chain for an element that has the <see cref="FocusPathProperty"/> explicitly
    /// set, assuming that means it is a primary focus group, and then sets the <see cref="IsPathFocusedProperty"/> to true for
    /// that element, and false for the last element that was focused
    /// </summary>
    /// <param name="target">Target/focused element which now has focus</param>
    /// <param name="newPath"></param>
    public static void UpdateCurrentlyFocusedObject(AvaloniaObject target, string? newPath) {
        object? lastFocused = currentlyFocusedObject.Target;
        if (lastFocused != null) {
            currentlyFocusedObject.Target = null;
            ((AvaloniaObject) lastFocused).SetValue(IsFocusedProperty, false);
        }

        if (string.IsNullOrEmpty(newPath))
            return;

        AvaloniaObject? root = VisualTreeUtils.FindNearestInheritedPropertyDefinition(FocusPathProperty, target as StyledElement);
        if (root != null) {
            currentlyFocusedObject.Target = root;
            root.SetValue(IsFocusedProperty, true);
        }
        else {
            Debug.WriteLine("Failed to find root control that owns the FocusPathProperty of '" + GetFocusPath(target) + "' on control " + target.GetType().Name);
        }
    }

    #endregion
}
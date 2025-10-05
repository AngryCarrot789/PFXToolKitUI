// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of PFXToolKitUI.
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
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Utils.Collections;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public delegate void FocusedPathChangedEventHandler(string? oldPath, string? newPath, bool isCausedByFocusPathPropertyChanging);

/// <summary>
/// A class which manages the WPF inputs and global focus scopes. This class also dispatches input events to the shortcut system
/// </summary>
public class UIInputManager {
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

    private static readonly AttachedProperty<AvaloniaShortcutInputProcessor?> ShortcutProcessorProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, AvaloniaShortcutInputProcessor?>("ShortcutProcessor");

    /// <summary>
    /// A dependency property for a control's focus path. This is the full path, and
    /// should usually be set such that it is relative to a parent control's focus path
    /// </summary>
    public static readonly AttachedProperty<string?> FocusPathProperty = AvaloniaProperty.RegisterAttached<UIInputManager, AvaloniaObject, string?>("FocusPath", inherits: true);

    public static readonly AttachedProperty<InputElement?> CurrentFocusedElementProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, InputElement?>("CurrentFocusedElement");
    public static readonly AttachedProperty<InputElement?> LastFocusedElementProperty = AvaloniaProperty.RegisterAttached<UIInputManager, TopLevel, InputElement?>("LastFocusedElement");

    /// <summary>
    /// A property for whether a control (whose <see cref="FocusPathProperty"/> has a value) currently has focus
    /// </summary>
    public static readonly AttachedProperty<bool> IsFocusedProperty = AvaloniaProperty.RegisterAttached<UIInputManager, AvaloniaObject, bool>("IsFocused");

    private static readonly InheritanceDictionary<Func<AvaloniaObject, KeyStroke, bool>> blockInputHandlers;
    private static readonly SortedList<Key, int> keyRepeatCounter;

    public static UIInputManager Instance { get; } = new UIInputManager();

    /// <summary>
    /// Gets the element whose <see cref="IsFocusedProperty"/> is set to true
    /// </summary>
    public static StyledElement? CurrentFocusedObject { get; private set; }

    /// <summary>
    /// Gets the current <see cref="FocusPathProperty"/> for <see cref="CurrentFocusedObject"/>
    /// </summary>
    public string? FocusedPath { get; private set; }

    /// <summary>
    /// An event fired when <see cref="FocusedPath"/> changed
    /// </summary>
    public static event FocusedPathChangedEventHandler? FocusedPathChanged;

    private UIInputManager() {
        if (Instance != null)
            throw new InvalidOperationException();
    }

    static UIInputManager() {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();
        keyRepeatCounter = new SortedList<Key, int>();
        blockInputHandlers = new InheritanceDictionary<Func<AvaloniaObject, KeyStroke, bool>>(4);

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


        Func<AvaloniaObject, KeyStroke, bool> handler_av = (c, ks) => {
            TextBox? tb = c as TextBox ?? VisualTreeUtils.FindLogicalParent<TextBox>(c);
            return KeyUtils.ShouldBlockKeyStrokeForTextInput(ks, tb != null && tb.AcceptsReturn, tb != null && tb.AcceptsTab);
        };

        RegisterBlockInputHandler(typeof(TextBox), handler_av);
        RegisterBlockInputHandler(typeof(TextPresenter), handler_av);
        RegisterBlockInputHandler(typeof(TextEditor), (c, ks) => KeyUtils.ShouldBlockKeyStrokeForTextInput(ks, true, false));
        RegisterBlockInputHandler(typeof(TextArea), (c, ks) => KeyUtils.ShouldBlockKeyStrokeForTextInput(ks, true, false));
    }

    public static bool GetIsKeyShortcutProcessingBlocked(AvaloniaObject obj, KeyStroke stroke) {
        foreach (ITypeEntry<Func<AvaloniaObject, KeyStroke, bool>> entry in blockInputHandlers.GetLocalValueEnumerable(obj.GetType())) {
            if (entry.LocalValue(obj, stroke)) {
                return true;
            }
        }

        return false;
    }

    public static void RegisterBlockInputHandler(Type typeOfObject, Func<AvaloniaObject, KeyStroke, bool> handler) {
        blockInputHandlers[typeOfObject] = handler;
    }

    public static InputElement? GetCurrentFocusedElement(TopLevel obj) => obj.GetValue(CurrentFocusedElementProperty);

    public static InputElement? GetLastFocusedElement(TopLevel obj) => obj.GetValue(LastFocusedElementProperty);

    public static void SetIsFocused(AvaloniaObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

    public static bool GetIsFocused(AvaloniaObject obj) => obj.GetValue(IsFocusedProperty);

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
            keyRepeatCounter[key] = 0;
        }
        else {
            int keyIndex = keyRepeatCounter.IndexOfKey(key);
            if (keyIndex == -1) {
                keyRepeatCounter[key] = 1;
                isRepeat = false;
            }
            else {
                int count = keyRepeatCounter.GetValueAtIndex(keyIndex);
                keyRepeatCounter.SetValueAtIndex(keyIndex, count + 1);
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
        if (e.Source is InputElement element) {
            OnGotOrLostFocus(topLevel, element, lost);
        }
    }

    private static void OnGotOrLostFocus(TopLevel topLevel, InputElement element, bool lost) {
        string? oldFocusPath = Instance.FocusedPath, newFocusPath;

#if DEBUG
        InputElement? lastFocused = topLevel.GetValue(LastFocusedElementProperty);
#endif

        InputElement? currFocused = topLevel.GetValue(CurrentFocusedElementProperty);
        topLevel.SetValue(LastFocusedElementProperty, currFocused);
        if (lost) {
            if (!ReferenceEquals(currFocused, element)) {
                Debug.Fail("Fatal error: Last focused element does not match element that just lost focus");
                throw new Exception();
            }

            newFocusPath = null;
            topLevel.SetValue(CurrentFocusedElementProperty, null);

            Debug.WriteLine($"Focus LOST: '{ReadableControlName(element, topLevel)}'");

            element.DetachedFromVisualTree -= OnElementDetachedFromVisualTree;
        }
        else {
            topLevel.SetValue(CurrentFocusedElementProperty, element);
            newFocusPath = GetFocusPath(element);

            string msg = "";
            if (currFocused != null) {
                currFocused.DetachedFromVisualTree -= OnElementDetachedFromVisualTree;
#if DEBUG
                msg = $" (ERROR: LastFocused still valid '{ReadableControlName(currFocused, topLevel)}')";
#endif
            }

            Debug.WriteLine($"Focus GAINED: null -> '{ReadableControlName(element, topLevel)}'{msg}");

            element.DetachedFromVisualTree += OnElementDetachedFromVisualTree;
        }

        if (oldFocusPath != newFocusPath) {
            Instance.FocusedPath = newFocusPath;
            FocusedPathChanged?.Invoke(oldFocusPath, newFocusPath, false);
            UpdateCurrentlyFocusedObject(element, newFocusPath);
        }
    }

    private static string ReadableControlName(InputElement e, TopLevel? topLevel = null) {
        StringBuilder sb = new StringBuilder().Append(e.GetType().Name);
        if (e.Name != null)
            sb.Append(" [NAME=\"").Append(e.Name).Append("\"]");
        if (e is HeaderedItemsControl hic && hic.Header is string header1)
            sb.Append(" [HEADER=\"").Append(header1).Append("\"]");
        else if (e is HeaderedSelectingItemsControl hsic && hsic.Header is string header2)
            sb.Append(" [HEADER=\"").Append(header2).Append("\"]");
        if (GetFocusPath(e) is string focusPath)
            sb.Append(" [FPath=\"").Append(focusPath).Append("\"]");
        sb.Append(" [TopLevel=\"").Append((topLevel ?? TopLevel.GetTopLevel(e))?.GetType().Name).Append("\"]");
        return sb.ToString();
    }

    private static void OnElementDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        if (e.Root is TopLevel topLevel && sender != null) {
            InputElement? focused = GetCurrentFocusedElement(topLevel);
            if (focused == sender) {
                Debug.WriteLine("Control removed from Visual Tree, forcing lost focus for " + ReadableControlName(focused, topLevel));
                OnGotOrLostFocus(topLevel, focused, lost: true);
            }
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
            FocusedPathChanged?.Invoke(oldPath, newPath, false);
            UpdateCurrentlyFocusedObject(obj, newPath);
            Debug.WriteLine($"Focus Path swapped for '{obj.GetType().Name}'");
        }
    }

    /// <summary>
    /// Sets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element and its visual tree
    /// </summary>
    public static void SetFocusPath(AvaloniaObject element, string? value) => element.SetValue(FocusPathProperty, value);

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
        object? lastFocused = CurrentFocusedObject;
        if (lastFocused != null) {
            CurrentFocusedObject = null;
            ((AvaloniaObject) lastFocused).SetValue(IsFocusedProperty, false);
        }

        if (string.IsNullOrEmpty(newPath))
            return;

        StyledElement? root = VisualTreeUtils.FindNearestInheritedPropertyDefinitionForLogical(FocusPathProperty, target as StyledElement);
        if (root != null) {
            CurrentFocusedObject = root;
            root.SetValue(IsFocusedProperty, true);
        }
        else {
            Debug.WriteLine("Failed to find root control that owns the FocusPathProperty of '" + GetFocusPath(target) + "' on control " + target.GetType().Name);
        }
    }

    #endregion
}
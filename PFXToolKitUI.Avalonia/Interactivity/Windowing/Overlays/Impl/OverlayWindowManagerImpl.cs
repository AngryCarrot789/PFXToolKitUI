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
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Avalonia.Utils;
using InvalidOperationException = System.InvalidOperationException;

namespace PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays.Impl;

public sealed class OverlayWindowManagerImpl : IOverlayWindowManager {
    internal readonly OverlayWindowHostImpl overlayWindowHost;
    private readonly List<OverlayWindowImpl> topLevelWindows;
    private readonly List<OverlayWindowImpl> allWindows;
    private readonly Stack<WeakReference<InputElement>?> focusStack;

    /// <summary>
    /// Gets the control that this popup dialog manager presents popups over
    /// </summary>
    public PopupOverlayContentHost OwnerControl { get; }

    public IEnumerable<IOverlayWindow> TopLevelWindows { get; }

    public IEnumerable<IOverlayWindow> AllWindows { get; }

    public IOverlayWindowHost OverlayWindowHost => this.overlayWindowHost;

    public event EventHandler<OverlayWindowEventArgs>? PopupOpened;

    public OverlayWindowManagerImpl(PopupOverlayContentHost ownerControl) {
        this.topLevelWindows = new List<OverlayWindowImpl>();
        this.allWindows = new List<OverlayWindowImpl>();
        this.TopLevelWindows = this.topLevelWindows.AsReadOnly();
        this.AllWindows = this.allWindows.AsReadOnly();
        this.focusStack = new Stack<WeakReference<InputElement>>();

        this.OwnerControl = ownerControl;
        this.OwnerControl.SetupForPopupDialogManager(this);
        this.overlayWindowHost = new OverlayWindowHostImpl(ownerControl);
    }

    public IOverlayWindow CreateWindow(OverlayWindowBuilder builder) {
        if (builder.Parent != null) {
            if (!(builder.Parent is OverlayWindowImpl parent))
                throw new InvalidOperationException("Attempt to use invalid parent popup");
            if (parent.myManager != this)
                throw new InvalidOperationException("Attempt to use parent popup that belongs to a different popup manager");
        }

        return new OverlayWindowImpl(this, builder.Parent as OverlayWindowImpl, builder);
    }

    public bool TryGetActivePopup([NotNullWhen(true)] out IOverlayWindow? popup) {
        return (popup = this.allWindows.LastOrDefault(x => x.OpenState == OpenState.Open)) != null;
    }

    public bool TryGetPopupFromVisual(Visual visual, [NotNullWhen(true)] out IOverlayWindow? popup) {
        PopupOverlayControlImpl? d = VisualTreeUtils.FindLogicalParent<PopupOverlayControlImpl>(visual, includeSelf: true);
        return (popup = d?.overlayWindow) != null;
    }

    public bool TryGetTopLevel([NotNullWhen(true)] out TopLevel? topLevel) {
        return (topLevel = TopLevel.GetTopLevel(this.overlayWindowHost.control)) != null;
    }

    internal void OnPopupOpening(OverlayWindowImpl overlayWindow) {
        Debug.Assert(overlayWindow.myManager == this);
        if (overlayWindow.myParent != null)
            overlayWindow.myParent.myOwnedPopups.Add(overlayWindow);
        else
            this.topLevelWindows.Add(overlayWindow);

        this.allWindows.Add(overlayWindow);

        // Push currently focused element onto the stack so we can re-focus it later
        InputElement? element = this.TryGetTopLevel(out TopLevel? topLevel)
            ? UIInputManager.GetCurrentFocusedElement(topLevel)
            : null;
            
        this.focusStack.Push(element != null ? new WeakReference<InputElement>(element) : null);

        InputElement content = (InputElement) this.OwnerControl.Content!;
        content.IsEnabled = false;
    }

    internal void OnPopupOpened(OverlayWindowImpl overlayWindow) {
        Debug.Assert(overlayWindow.myManager == this);
        this.PopupOpened?.Invoke(this, new OverlayWindowEventArgs(overlayWindow));
    }

    internal void OnPopupClosed(OverlayWindowImpl overlayWindow) {
        Debug.Assert(overlayWindow.myManager == this);
        bool debugRemoved;
        if (overlayWindow.myParent != null) {
            debugRemoved = overlayWindow.myParent.myOwnedPopups.Remove(overlayWindow);
            Debug.Assert(debugRemoved, "Failed to remove self from parent's owners list");
        }
        else {
            debugRemoved = this.topLevelWindows.Remove(overlayWindow);
            Debug.Assert(debugRemoved, "Failed to remove self from window manager's top-level window list");
        }

        debugRemoved = this.allWindows.Remove(overlayWindow);
        Debug.Assert(debugRemoved, "Failed to remove self from window manager's window list");
        ((InputElement) this.OwnerControl.Content!).IsEnabled = this.allWindows.Count < 1;
        if (this.focusStack.TryPop(out WeakReference<InputElement>? reference)) {
            if (reference != null && reference.TryGetTarget(out InputElement? lastTarget)) {
                lastTarget.Focus();
            }
        }
    }

    public void AddPopupToVisualTree(OverlayWindowImpl overlayWindow) {
        this.overlayWindowHost.AddPopupToVisualTree(overlayWindow);
    }

    public void RemovePopupFromVisualTree(OverlayWindowImpl overlayWindow) {
        this.overlayWindowHost.RemovePopupFromVisualTree(overlayWindow);
    }
}
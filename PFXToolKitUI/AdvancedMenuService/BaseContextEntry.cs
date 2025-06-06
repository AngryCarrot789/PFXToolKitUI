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
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate void BaseContextEntryEventHandler(BaseContextEntry sender);

public delegate void BaseContextEntryIconChangedEventHandler(BaseContextEntry sender, Icon? oldIcon, Icon? newIcon);

/// <summary>
/// The base class for a menu item model. Contains standard properties like display name, tooltip description and an icon.
/// <para>
/// It also provides access to the captured context of the top-level menu when this entry has become visible in the UI,
/// allowing you to access context information and hook onto events. With this information, you can do things like update
/// the header and icon, and also raise <see cref="CanExecuteChanged"/> when the executability may have changed
/// </para>
/// </summary>
public abstract class BaseContextEntry : IContextObject {
    private string? displayName, description;
    private Icon? icon;
    private bool isInUse;

    /// <summary>
    /// Gets or sets the header of the menu item
    /// </summary>
    public string? DisplayName {
        get => this.displayName;
        set {
            if (this.displayName != value) {
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets the tooltip
    /// </summary>
    public string? Description {
        get => this.description;
        set {
            if (this.description != value) {
                this.description = value;
                this.DescriptionChanged?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon presented in the menu gutter (left side)
    /// </summary>
    public Icon? Icon {
        get => this.icon;
        set {
            Icon? oldIcon = this.icon;
            if (!ReferenceEquals(oldIcon, value)) {
                this.icon = value;
                this.IconChanged?.Invoke(this, oldIcon, value);
            }
        }
    }
    
    /// <summary>
    /// Gets the current captured context from the UI menu item. This is null when the menu item is not visible onscreen
    /// </summary>
    public IContextData? CapturedContext { get; private set; }

    public event BaseContextEntryEventHandler? DisplayNameChanged;
    public event BaseContextEntryEventHandler? DescriptionChanged;
    public event BaseContextEntryIconChangedEventHandler? IconChanged;

    /// <summary>
    /// Fired when the executability of this entry as a command has changed. This
    /// is listened to by the UI to update the enabled and visibility states
    /// </summary>
    public event BaseContextEntryEventHandler? CanExecuteChanged;
    
    public BaseContextEntry() {
    }

    public BaseContextEntry(string displayName, string? description = null, Icon? icon = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        this.displayName = displayName;
        this.description = description;
        this.icon = icon;
    }
    
    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event 
    /// </summary>
    public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this);

    /// <summary>
    /// Invoked when <see cref="CapturedContext"/> changes. Note that this is only called when the UI menu item's visibility
    /// changes. Therefore, objects from the context should not be used for anything except observing the states of
    /// </summary>
    protected virtual void OnContextChanged() {
        
    }

    public static void OnUserAdded(BaseContextEntry entry, IContextData capturedContext) {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(capturedContext);
        if (entry.isInUse)
            throw new InvalidOperationException("Context entry already in use");

        entry.isInUse = true;
        entry.CapturedContext = capturedContext;
        entry.OnContextChanged();
        
        Debug.WriteLine($"[ADVANCEDMENUSYSTEM] ENTRY VISIBLE - {entry.GetType().Name} (\"{entry.displayName}\")");
    }

    public static void OnUserRemoved(BaseContextEntry entry) {
        ArgumentNullException.ThrowIfNull(entry);
        if (!entry.isInUse)
            throw new InvalidOperationException("Context not in use");

        entry.isInUse = false;
        entry.CapturedContext = null;
        entry.OnContextChanged();
        
        Debug.WriteLine($"[ADVANCEDMENUSYSTEM] ENTRY HIDDEN - {entry.GetType().Name} (\"{entry.displayName}\")");
    }
}
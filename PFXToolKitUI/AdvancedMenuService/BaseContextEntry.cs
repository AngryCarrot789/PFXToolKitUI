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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate void BaseContextEntryEventHandler(BaseContextEntry sender);

public delegate void BaseContextEntryIconChangedEventHandler(BaseContextEntry sender, Icon? oldIcon, Icon? newIcon);

public delegate void BaseContextEntryCapturedContextChangedEventHandler(BaseContextEntry sender, IContextData? oldCapturedContext, IContextData? newCapturedContext);

public delegate DisabledHintInfo? BaseContextEntryProvideDisabledHint(IContextData context, ContextRegistry? sourceContextMenu);

/// <summary>
/// The base class for a menu item model. Contains standard properties like display name, tooltip description and an icon.
/// <para>
/// It also provides access to the captured context of the top-level menu when this entry becomes visible in the UI,
/// allowing you to access context information and hook onto events. With this information, you can do things like update
/// the header and icon, and also raise <see cref="CanExecuteChanged"/> when the executability may have changed
/// </para>
/// </summary>
public abstract class BaseContextEntry : IContextObject, IUserLocalContext, IDisabledHintProvider {
    /// <summary>
    /// Gets the general data key for accessing a context entry. UI controls for menu items will use
    /// this to update their local context with the entry they are currently attached to
    /// </summary>
    public static readonly DataKey<BaseContextEntry> DataKey = DataKeys.Create<BaseContextEntry>(nameof(BaseContextEntry));

    private string? displayName, description;
    private Icon? icon, disabledIcon;
    private bool isInUse;
    private IContextData? capturedContext;
    private Func<BaseContextEntry, bool>? isCheckedFunction;

    /// <summary>
    /// Gets or sets the header of the menu item
    /// </summary>
    public string? DisplayName {
        get => this.displayName;
        set => PropertyHelper.SetAndRaiseINE(ref this.displayName, value, this, static t => t.DisplayNameChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the tooltip
    /// </summary>
    public string? Description {
        get => this.description;
        set => PropertyHelper.SetAndRaiseINE(ref this.description, value, this, static t => t.DescriptionChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets the icon presented in the menu gutter (left side), specifically when this entry is 
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
    /// Gets or sets the icon presented in the menu gutter (left side)
    /// </summary>
    public Icon? DisabledIcon {
        get => this.disabledIcon;
        set {
            Icon? oldIcon = this.disabledIcon;
            if (!ReferenceEquals(oldIcon, value)) {
                this.disabledIcon = value;
                this.DisabledIconChanged?.Invoke(this, oldIcon, value);
            }
        }
    }

    /// <summary>
    /// Gets the current captured context from the UI menu item. This changes when the entry's visibility on screen changes or
    /// the captured context data, of the context menu this entry exists in, changes.
    /// <para>
    /// Note, objects from the context should ideally only be used for state observation
    /// </para>
    /// </summary>
    public IContextData? CapturedContext {
        get => this.capturedContext;
        private set => PropertyHelper.SetAndRaiseINE(ref this.capturedContext, value, this, static (t, o, n) => t.CapturedContextChanged?.Invoke(t, o, n));
    }

    /// <summary>
    /// Gets or sets a function that specifies if this context entry is checked or not.
    /// <para>
    /// Note, changing the value of this property will invoke <see cref="RaiseIsCheckedChanged"/>
    /// </para>
    /// </summary>
    public Func<BaseContextEntry, bool>? IsCheckedFunction {
        get => this.isCheckedFunction;
        set {
            if (this.isCheckedFunction != value) {
                this.isCheckedFunction = value;
                this.IsCheckedFunctionChanged?.Invoke(this);
                this.RaiseIsCheckedChanged();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the disabled hint info provider for this group
    /// </summary>
    public BaseContextEntryProvideDisabledHint? ProvideDisabledHint { get; set; }

    /// <summary>
    /// Gets the custom user context for this context entry. This is only used for advanced customizations
    /// of the entry instance, and has no relation to <see cref="CapturedContext"/>
    /// </summary>
    public IMutableContextData UserContext { get; } = new ContextData();

    public event BaseContextEntryEventHandler? DisplayNameChanged;
    public event BaseContextEntryEventHandler? DescriptionChanged;
    public event BaseContextEntryIconChangedEventHandler? IconChanged, DisabledIconChanged;
    public event BaseContextEntryCapturedContextChangedEventHandler? CapturedContextChanged;
    public event BaseContextEntryEventHandler? IsCheckedFunctionChanged;
    public event BaseContextEntryEventHandler? IsCheckedChanged;

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

    public void RaiseIsCheckedChanged() {
        this.IsCheckedChanged?.Invoke(this);
    }
    
    DisabledHintInfo? IDisabledHintProvider.ProvideDisabledHint(IContextData context, ContextRegistry? sourceContextMenu) {
        return this.ProvideDisabledHint?.Invoke(context, sourceContextMenu);
    }

    public static void InternalOnBecomeVisible(BaseContextEntry entry, IContextData capturedContext) {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(capturedContext);
        if (entry.isInUse)
            throw new InvalidOperationException("Context entry already in use");

        entry.isInUse = true;
        entry.CapturedContext = capturedContext;
    }

    public static void InternalOnBecomeHidden(BaseContextEntry entry) {
        ArgumentNullException.ThrowIfNull(entry);
        if (!entry.isInUse)
            throw new InvalidOperationException("Context not in use");

        entry.isInUse = false;
        entry.CapturedContext = null;
    }
}
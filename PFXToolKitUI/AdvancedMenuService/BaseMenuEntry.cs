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
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate DisabledHintInfo? BaseContextEntryProvideDisabledHint(IContextData context, ContextRegistry? sourceContextMenu);

/// <summary>
/// The base class for a menu item model. Contains standard properties like display name, tooltip description and an icon.
/// <para>
/// It also provides access to the captured context of the top-level menu when this entry becomes visible in the UI,
/// allowing you to access context information and hook onto events. With this information, you can do things like update
/// the header and icon, and also raise <see cref="CanExecuteChanged"/> when the executability may have changed
/// </para>
/// </summary>
public abstract class BaseMenuEntry : IMenuEntry, IUserLocalContext, IDisabledHintProvider {
    /// <summary>
    /// Gets the general data key for accessing a menu entry. UI controls for menu items will use
    /// this to update their local context with the entry they are currently attached to
    /// </summary>
    public static readonly DataKey<BaseMenuEntry> DataKey = DataKeys.Create<BaseMenuEntry>(nameof(BaseMenuEntry));

    private bool isInUse;

    /// <summary>
    /// Gets or sets the header of the menu item
    /// </summary>
    public string? DisplayName {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DisplayNameChanged);
    }

    /// <summary>
    /// Gets or sets the tooltip
    /// </summary>
    public string? Description {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DescriptionChanged);
    }

    /// <summary>
    /// Gets or sets the icon presented in the menu gutter (left side), specifically when this entry is 
    /// </summary>
    public Icon? Icon {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IconChanged);
    }

    /// <summary>
    /// Gets or sets the icon presented in the menu gutter (left side) when this menu entry is disabled
    /// </summary>
    public Icon? DisabledIcon {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DisabledIconChanged);
    }

    /// <summary>
    /// Gets the current captured context from the UI menu item. This changes when the entry's visibility on screen changes or
    /// the captured context data, of the context menu this entry exists in, changes.
    /// <para>
    /// Note, objects from the context should ideally only be used for state observation
    /// </para>
    /// </summary>
    public IContextData? CapturedContext {
        get => field;
        private set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.CapturedContextChanged);
    }

    /// <summary>
    /// Gets or sets a function that specifies if this menu entry is checked or not.
    /// <para>
    /// Note, changing the value of this property will invoke <see cref="RaiseIsCheckedChanged"/>
    /// </para>
    /// </summary>
    public Func<BaseMenuEntry, bool>? IsCheckedFunction {
        get => field;
        set {
            if (field != value) {
                field = value;
                this.IsCheckedFunctionChanged?.Invoke(this, EventArgs.Empty);
                this.RaiseIsCheckedChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the disabled hint info provider for this group
    /// </summary>
    public BaseContextEntryProvideDisabledHint? ProvideDisabledHint { get; set; }

    /// <summary>
    /// Gets the custom user context for this menu entry. This is only used for advanced customizations
    /// of the entry instance, and has no relation to <see cref="CapturedContext"/>
    /// </summary>
    public IMutableContextData UserContext { get; } = new ContextData();

    public event EventHandler? DisplayNameChanged;
    public event EventHandler? DescriptionChanged;
    public event EventHandler<ValueChangedEventArgs<Icon?>>? IconChanged, DisabledIconChanged;
    public event EventHandler<ValueChangedEventArgs<IContextData?>>? CapturedContextChanged;
    public event EventHandler? IsCheckedFunctionChanged;
    public event EventHandler? IsCheckedChanged;

    /// <summary>
    /// Fired when the executability of this entry as a command has changed. This
    /// is listened to by the UI to update the enabled and visibility states
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    public BaseMenuEntry() {
    }

    public BaseMenuEntry(string displayName, string? description = null, Icon? icon = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        this.DisplayName = displayName;
        this.Description = description;
        this.Icon = icon;
    }

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event 
    /// </summary>
    public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void RaiseIsCheckedChanged() {
        this.IsCheckedChanged?.Invoke(this, EventArgs.Empty);
    }
    
    DisabledHintInfo? IDisabledHintProvider.ProvideDisabledHint(IContextData context, ContextRegistry? sourceContextMenu) {
        return this.ProvideDisabledHint?.Invoke(context, sourceContextMenu);
    }

    public static void InternalOnBecomeVisible(BaseMenuEntry entry, IContextData capturedContext) {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(capturedContext);
        if (entry.isInUse)
            throw new InvalidOperationException("Menu entry already in use");

        entry.isInUse = true;
        entry.CapturedContext = capturedContext;
    }

    public static void InternalOnBecomeHidden(BaseMenuEntry entry) {
        ArgumentNullException.ThrowIfNull(entry);
        if (!entry.isInUse)
            throw new InvalidOperationException("Context not in use");

        entry.isInUse = false;
        entry.CapturedContext = null;
    }
}
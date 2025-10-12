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

using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate void ContextRegistryEventHandler(ContextRegistry registry);

public delegate void ContextRegistryContextEventHandler(ContextRegistry registry, IContextData context);

/// <summary>
/// A class which stores a menu entry hierarchy for use in context menus.
/// <para>
/// The idea is that there are known identifiable "groups" for certain actions,
/// so that when plugins are available they can inject their own commands
/// into the right group... hopefully
/// </para>
/// </summary>
public class ContextRegistry {
    private readonly SortedList<int, Dictionary<string, IWeightedMenuEntryGroup>> groups;
    private string? caption;
    private string? objectName;

    /// <summary>
    /// Gets the groups in our registry
    /// </summary>
    public IEnumerable<KeyValuePair<string, IWeightedMenuEntryGroup>> Groups => this.groups.Select(x => x.Value).SelectMany(x => x);

    /// <summary>
    /// Gets or sets this registry's caption
    /// </summary>
    public string? Caption {
        get => this.caption;
        set => PropertyHelper.SetAndRaiseINE(ref this.caption, value, this, static t => t.CaptionChanged?.Invoke(t));
    }

    /// <summary>
    /// Gets or sets an optional secondary caption, such as the readable name of an object,
    /// and is shown in less obvious text (i.e. darker foreground brush with a dark themes)
    /// </summary>
    public string? ObjectName {
        get => this.objectName;
        set => PropertyHelper.SetAndRaiseINE(ref this.objectName, value, this, static t => t.ObjectNameChanged?.Invoke(t));
    }
    
    public bool IsOpened { get; private set; }

    public event ContextRegistryEventHandler? CaptionChanged;
    public event ContextRegistryEventHandler? ObjectNameChanged;
    public event ContextRegistryContextEventHandler? Opened;
    public event ContextRegistryEventHandler? Closed;
    
    /// <summary>
    /// Notifies the handler to try to close the context menu
    /// </summary>
    public event ContextRegistryEventHandler? RequestClose;

    public ContextRegistry(string caption) {
        this.groups = new SortedList<int, Dictionary<string, IWeightedMenuEntryGroup>>();
        this.Caption = caption;
    }
    
    /// <summary>
    /// Raises the <see cref="RequestClose"/> event
    /// </summary>
    public void RaiseRequestClose() {
        if (this.IsOpened)
            this.RequestClose?.Invoke(this);
    }

    public void OnOpened(IContextData context) {
        ArgumentNullException.ThrowIfNull(context);
        if (this.IsOpened)
            throw new InvalidOperationException("Already open");

        this.IsOpened = true;
        this.Opened?.Invoke(this, context);
    }

    public void OnClosed() {
        if (!this.IsOpened)
            throw new InvalidOperationException("Not opened");

        this.IsOpened = false;
        this.Closed?.Invoke(this);
    }

    public FixedWeightedMenuEntryGroup GetFixedGroup(string name, int weight = 0) {
        if (!this.GetDictionary(weight).TryGetValue(name, out IWeightedMenuEntryGroup? group))
            this.SetDictionary(weight, name, group = new FixedWeightedMenuEntryGroup());
        else if (!(group is FixedWeightedMenuEntryGroup))
            throw new InvalidOperationException("Context group is not fixed: " + name);
        return (FixedWeightedMenuEntryGroup) group;
    }

    public DynamicWeightedMenuEntryGroup CreateDynamicGroup(string name, DynamicGenerateContextFunction generate, int weight = 0) {
        if (!this.GetDictionary(weight).TryGetValue(name, out IWeightedMenuEntryGroup? group))
            this.SetDictionary(weight, name, group = new DynamicWeightedMenuEntryGroup(generate));
        else if (!(group is DynamicWeightedMenuEntryGroup))
            throw new InvalidOperationException("Context group is not dynamic: " + name);
        return (DynamicWeightedMenuEntryGroup) group;
    }

    private Dictionary<string, IWeightedMenuEntryGroup> GetDictionary(int weight) {
        if (!this.groups.TryGetValue(weight, out Dictionary<string, IWeightedMenuEntryGroup>? dict))
            this.groups[weight] = dict = new Dictionary<string, IWeightedMenuEntryGroup>();
        return dict;
    }

    private void SetDictionary(int weight, string name, IWeightedMenuEntryGroup group) {
        this.GetDictionary(weight)[name] = group;
    }
}
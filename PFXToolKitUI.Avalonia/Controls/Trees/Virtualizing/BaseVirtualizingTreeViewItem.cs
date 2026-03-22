// 
// Copyright (c) 2026-2026 REghZy
// 
// This file is part of MemoryEngine360.
// 
// MemoryEngine360 is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// MemoryEngine360 is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MemoryEngine360. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PFXToolKitUI.Avalonia.Controls.Trees.Virtualizing;

[TemplatePart("PART_HeaderPresenter", typeof(ContentPresenter))]
public class BaseVirtualizingTreeViewItem : HeaderedContentControl {
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<BaseVirtualizingTreeViewItem, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<bool> IsSelectedProperty = SelectingItemsControl.IsSelectedProperty.AddOwner<BaseVirtualizingTreeViewItem>();
    public static readonly DirectProperty<BaseVirtualizingTreeViewItem, int> LevelProperty = AvaloniaProperty.RegisterDirect<BaseVirtualizingTreeViewItem, int>(nameof(Level), o => o.Level);
    public static readonly RoutedEvent<RoutedEventArgs> ExpandedEvent = RoutedEvent.Register<BaseVirtualizingTreeViewItem, RoutedEventArgs>(nameof(Expanded), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    public static readonly RoutedEvent<RoutedEventArgs> CollapsedEvent = RoutedEvent.Register<BaseVirtualizingTreeViewItem, RoutedEventArgs>(nameof(Collapsed), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    public bool IsExpanded {
        get => this.GetValue(IsExpandedProperty);
        set => this.SetValue(IsExpandedProperty, value);
    }

    public bool IsSelected {
        get => this.GetValue(IsSelectedProperty);
        set => this.SetValue(IsSelectedProperty, value);
    }

    public int Level {
        get => field;
        internal set => this.SetAndRaise(LevelProperty, ref field, value);
    }

    public event EventHandler<RoutedEventArgs>? Expanded {
        add => this.AddHandler(ExpandedEvent, value);
        remove => this.RemoveHandler(ExpandedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? Collapsed {
        add => this.AddHandler(CollapsedEvent, value);
        remove => this.RemoveHandler(CollapsedEvent, value);
    }

    public BaseVirtualizingTreeViewItem() {
    }

    static BaseVirtualizingTreeViewItem() {
        SelectableMixin.Attach<BaseVirtualizingTreeViewItem>(IsSelectedProperty);
        IsExpandedProperty.Changed.AddClassHandler<BaseVirtualizingTreeViewItem, bool>((x, e) => x.OnIsExpandedChanged(e));
    }
    
    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);

        if (e.Key == Key.Right) {
            this.IsExpanded = true;
        }
        else if (e.Key == Key.Left) {
            this.IsExpanded = false;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.ClickCount % 2 == 0 && e.Properties.IsLeftButtonPressed) {
            this.IsExpanded = !this.IsExpanded;
        }
    }
    
    private void OnIsExpandedChanged(AvaloniaPropertyChangedEventArgs<bool> args) {
        RoutedEvent<RoutedEventArgs> routedEvent = args.NewValue.Value ? ExpandedEvent : CollapsedEvent;
        this.RaiseEvent(new RoutedEventArgs() { RoutedEvent = routedEvent, Source = this });
        
        this.OnIsExpandedChanged();
    }

    protected virtual void OnIsExpandedChanged() {
        
    }
}
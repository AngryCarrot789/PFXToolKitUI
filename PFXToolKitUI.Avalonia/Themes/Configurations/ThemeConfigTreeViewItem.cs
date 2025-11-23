// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using PFXToolKitUI.Avalonia.AdvancedMenuService;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Themes.Contexts;
using PFXToolKitUI.Utils.Destroying;

namespace PFXToolKitUI.Avalonia.Themes.Configurations;

public class ThemeConfigTreeViewItem : TreeViewItemEx, IThemeConfigEntryTreeOrNode {
    public static readonly StyledProperty<bool> IsNonGroupProperty = AvaloniaProperty.Register<ThemeConfigTreeViewItem, bool>(nameof(IsNonGroup));

    public bool IsNonGroup {
        get => this.GetValue(IsNonGroupProperty);
        set => this.SetValue(IsNonGroupProperty, value);
    }

    public ThemeConfigTreeView? ThemeConfigTree { get; private set; }

    public ThemeConfigTreeViewItem? ParentNode { get; private set; }

    public IThemeTreeEntry? Entry { get; private set; }

    public int GroupCounter { get; private set; }

    private bool wasSetVisibleWithoutEntry;

    private Ellipse? PART_IsInheritedIndicator;
    private Rectangle? PART_ThemeColourPreview;
    private DynamicAvaloniaColourBrush? myDynamicBrush;
    private IDisposable? myDynamicBrushSubscription;
    private IBrush? myCurrentDynamicBrush;

    public ThemeConfigTreeViewItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_IsInheritedIndicator = e.NameScope.GetTemplateChild<Ellipse>(nameof(this.PART_IsInheritedIndicator));
        this.PART_ThemeColourPreview = e.NameScope.GetTemplateChild<Rectangle>(nameof(this.PART_ThemeColourPreview));
        this.PART_ThemeColourPreview.Fill = this.myCurrentDynamicBrush;

        this.UpdateIsInheritedIndicator();
    }

    protected override void OnIsReallyVisibleChanged() {
        base.OnIsReallyVisibleChanged();

        if (this.Entry != null) {
            this.GenerateHeader();
        }
        else {
            this.wasSetVisibleWithoutEntry = true;
        }

        this.UpdateSubscription();
        this.UpdateIsInheritedIndicator();
        
        if (this.PART_ThemeColourPreview != null) {
            this.PART_ThemeColourPreview.Fill = this.IsReallyVisible ? this.myCurrentDynamicBrush : null;
        }
    }

    private void OnDynamicBrushChanged(IBrush? obj) {
        this.myCurrentDynamicBrush = obj;
        if (this.PART_ThemeColourPreview != null) {
            this.PART_ThemeColourPreview.Fill = this.myCurrentDynamicBrush;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, ThemeContextRegistry.Registry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    private void GenerateHeader() {
        if (this.Entry is ThemeConfigEntry entry) {
            this.IsNonGroup = true;
            // theme should not really be null
            Theme? theme = this.ThemeConfigTree?.ThemeConfigurationPage?.TargetTheme;
            if (theme == null || theme.IsThemeKeyValid(entry.ThemeKey)) {
                this.Header = this.Entry!.DisplayName;
            }
            else {
                TextBlock tb = new TextBlock();
                tb.Inlines ??= new InlineCollection();

                Run run = new Run(this.Entry!.DisplayName) { TextDecorations = TextDecorations.Strikethrough };
                if (this.Foreground is ISolidColorBrush brush) {
                    run.Foreground = new ImmutableSolidColorBrush(brush.Color, 0.7);
                }

                tb.Inlines.Add(run);
                tb.Inlines.Add(" (invalid)");
                this.Header = tb;
            }
        }
        else {
            this.Header = this.Entry!.DisplayName;
            this.IsNonGroup = false;
        }
    }

    private void UpdateSubscription() {
        if (this.IsReallyVisible) {
            if (this.myDynamicBrush != null) {
                DisposableUtils.Dispose(ref this.myDynamicBrushSubscription);
                this.myDynamicBrushSubscription = this.myDynamicBrush.Subscribe(static (b, s) => ((ThemeConfigTreeViewItem) s!).OnDynamicBrushChanged(b.CurrentBrush), this);
            }
        }
        else {
            DisposableUtils.Dispose(ref this.myDynamicBrushSubscription);
        }
    }

    #region Model Connection

    public virtual void OnAdding(ThemeConfigTreeView tree, ThemeConfigTreeViewItem? parentNode, IThemeTreeEntry entry) {
        this.ThemeConfigTree = tree;
        this.ParentNode = parentNode;
        this.Entry = entry;
    }

    public virtual void OnAdded() {
        if (this.Entry is ThemeConfigEntryGroup myGroup) {
            int i = 0;
            foreach (ThemeConfigEntryGroup entry in myGroup.Groups) {
                this.InsertGroup(entry, i++);
            }

            i = 0;
            foreach (ThemeConfigEntry entry in myGroup.Entries) {
                this.InsertEntry(entry, i++);
            }
        }

        if (this.Entry is ThemeConfigEntry configEntry) {
            if (!string.IsNullOrWhiteSpace(configEntry.ThemeKey)) {
                ToolTipEx.SetTip(this, configEntry.Description);
                this.myDynamicBrush = (DynamicAvaloniaColourBrush) BrushManager.Instance.GetDynamicThemeBrush(configEntry.ThemeKey);
            }

            configEntry.InheritedFromKeyChanged += this.OnInheritedFromKeyChanged;
        }

        this.UpdateIsInheritedIndicator();
        if (this.wasSetVisibleWithoutEntry) {
            this.wasSetVisibleWithoutEntry = false;
            this.GenerateHeader();
            this.UpdateSubscription();
            if (this.PART_ThemeColourPreview != null) {
                this.PART_ThemeColourPreview.Fill = this.myCurrentDynamicBrush;
            }
        }

        DataManager.GetContextData(this).Set(ThemeContextRegistry.ThemeTreeEntryKey, this.Entry!);
    }

    private void OnInheritedFromKeyChanged(object? o, EventArgs eventArgs) {
        this.UpdateIsInheritedIndicator();
    }

    private void UpdateIsInheritedIndicator() {
        if (this.PART_IsInheritedIndicator == null) {
            return;
        }

        if (this.Entry is ThemeConfigEntry entry) {
            this.PART_IsInheritedIndicator.IsVisible = entry.InheritedFromKey != null;
            if (entry.InheritedFromKey != null) {
                this.PART_IsInheritedIndicator.Fill = new ImmutableSolidColorBrush(entry.InheritanceDepth == 0 ? Colors.DodgerBlue : Colors.Yellow);
            }
        }
        else {
            this.PART_IsInheritedIndicator.IsVisible = false;
        }
    }

    public virtual void OnRemoving() {
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--)
            this.RemoveNodeInternal(i);

        this.GroupCounter = 0;

        if (this.PART_ThemeColourPreview != null) {
            this.PART_ThemeColourPreview.Fill = null;
        }

        DisposableUtils.Dispose(ref this.myDynamicBrushSubscription);
        this.myDynamicBrush = null;

        if (this.Entry is ThemeConfigEntry entry)
            entry.InheritedFromKeyChanged -= this.OnInheritedFromKeyChanged;
    }

    public virtual void OnRemoved() {
        this.ThemeConfigTree = null;
        this.ParentNode = null;
        this.Entry = null;
        DataManager.GetContextData(this).Remove(ThemeContextRegistry.ThemeTreeEntryKey);
    }

    #endregion

    #region Model to Control objects

    public ThemeConfigTreeViewItem GetNodeAt(int index) => (ThemeConfigTreeViewItem) this.Items[index]!;

    public void InsertGroup(ThemeConfigEntryGroup entry, int index) {
        this.GroupCounter++;
        this.InsertNodeInternal(entry, index);
    }

    public void InsertEntry(ThemeConfigEntry entry, int index) {
        this.InsertNodeInternal(entry, index + this.GroupCounter);
    }

    public void RemoveGroup(int index, bool canCache = true) {
        this.GroupCounter--;
        this.RemoveNodeInternal(index, canCache);
    }

    public void RemoveEntry(int index, bool canCache = true) {
        this.RemoveNodeInternal(index + this.GroupCounter, canCache);
    }

    public void InsertNodeInternal(IThemeTreeEntry layer, int index) {
        ThemeConfigTreeView? tree = this.ThemeConfigTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");

        ThemeConfigTreeViewItem control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.OnAdded();
    }

    public void RemoveNodeInternal(int index, bool canCache = true) {
        ThemeConfigTreeView? tree = this.ThemeConfigTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ThemeConfigTreeViewItem control = (ThemeConfigTreeViewItem) this.Items[index]!;
        IThemeTreeEntry resource = control.Entry ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    #endregion

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.ClickCount % 2) == 0) {
            if (!isToggle) {
                this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                e.Handled = true;
            }
        }
        else if ((this.IsFocused || this.Focus())) {
            e.Pointer.Capture(this);
            this.ThemeConfigTree?.SetSelection(this);
            e.Handled = true;
        }
    }
}
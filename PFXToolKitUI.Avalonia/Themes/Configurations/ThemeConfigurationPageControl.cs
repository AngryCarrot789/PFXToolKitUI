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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.Controls;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Configurations.UI;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Themes.Contexts;
using PFXToolKitUI.Utils.Events;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes.Configurations;

public class ThemeConfigurationPageControl : BaseConfigurationPageControl {
    private readonly ManualBinderEx<ThemeConfigTreeView, ThemeConfigurationPage> treePageBinder =
        new ManualBinderEx<ThemeConfigTreeView, ThemeConfigurationPage>((c, m) => c.ThemeConfigurationPage = m, (c, m) => c.ThemeConfigurationPage = null);

    private readonly LazyHelper2<Button, bool> isResetEnabledBinder = new LazyHelper2<Button, bool>((btn, isEnabled, hasBoth) => btn.IsEnabled = isEnabled && hasBoth);
    private readonly LazyHelper2<TextBlock, bool> showBuiltInThemeWarningBinder = new LazyHelper2<TextBlock, bool>((tb, show, hasBoth) => tb.IsVisible = show && hasBoth);
    private readonly LazyHelper2<GroupBox, string> gbHeaderBinder = new LazyHelper2<GroupBox, string>((gb, txt, hasBoth) => gb.Header = hasBoth ? txt : "");

    private readonly LazyHelper2<ColorPicker, Color?> pickerColourBinder = new LazyHelper2<ColorPicker, Color?>((picker, colour, hasBoth) => {
        if (hasBoth) {
            if (colour.HasValue) {
                picker.Color = colour.Value;
                if (!picker.IsEnabled)
                    picker.IsEnabled = true;
            }
            else {
                picker.Color = Colors.Transparent;
                if (picker.IsEnabled)
                    picker.IsEnabled = false;
            }
        }
        else if (picker.IsEnabled) {
            picker.IsEnabled = false;
        }
    });

    public new ThemeConfigurationPage? Page => (ThemeConfigurationPage?) base.Page;

    private ThemeConfigTreeView? PART_ThemeConfigTree;
    private TextBox? PART_ThemeKeyTextBox;
    private Grid? PART_SelectedItemGrid;
    private ColorPicker? PART_ColorPicker;
    private GroupBox? PART_GroupBox;
    private BindingExpressionBase? colourBinding;
    private TextBlock? PART_WarnEditingBuiltInTheme;
    private Button? PART_ResetButton;

    private TextBox? PART_InheritedFromTextBox;
    private Button? PART_SetInheritedFromKeyButton;
    private Button? PART_NavigateToInheritedKeyButton;

    private bool ignoreSpectrumColourChange;
    private string? activeThemeKey;
    private DynamicAvaloniaColourBrush? myActiveBrush;
    private IDisposable? disposeMyActiveBrush;
    private bool isExpandingSubTree;
    private ThemeConfigEntry? currentTCE;

    public ThemeConfigurationPageControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ThemeConfigTree = e.NameScope.GetTemplateChild<ThemeConfigTreeView>("PART_ThemeConfigTree");
        this.PART_ThemeKeyTextBox = e.NameScope.GetTemplateChild<TextBox>("PART_ThemeKeyTextBox");
        this.PART_WarnEditingBuiltInTheme = e.NameScope.GetTemplateChild<TextBlock>("PART_WarnEditingBuiltInTheme");
        this.PART_SelectedItemGrid = e.NameScope.GetTemplateChild<Grid>("PART_SelectedItemGrid");
        this.PART_ColorPicker = e.NameScope.GetTemplateChild<ColorPicker>("PART_ColorPicker");
        this.PART_GroupBox = e.NameScope.GetTemplateChild<GroupBox>("PART_GroupBox");
        this.PART_ResetButton = e.NameScope.GetTemplateChild<Button>("PART_ResetButton");
        this.PART_InheritedFromTextBox = e.NameScope.GetTemplateChild<TextBox>("PART_InheritedFromTextBox");
        this.PART_SetInheritedFromKeyButton = e.NameScope.GetTemplateChild<Button>("PART_SetInheritedFromKeyButton");
        this.PART_NavigateToInheritedKeyButton = e.NameScope.GetTemplateChild<Button>("PART_NavigateToInheritedKeyButton");
        DataManager.GetContextData(this).Set(IThemeConfigurationTreeElement.TreeElementKey, this.PART_ThemeConfigTree);

        this.PART_ThemeConfigTree.SelectionChanged += this.PART_ThemeConfigTreeOnSelectionChanged;
        this.PART_ColorPicker.ColorChanged += this.PART_ColorPickerOnColourChanged;
        this.PART_ResetButton.Click += this.PART_ResetButtonOnClick;
        this.PART_SetInheritedFromKeyButton.Click += this.PART_SetInheritedFromKeyButtonOnClick;
        this.PART_NavigateToInheritedKeyButton.Click += this.PART_InheritedFromHyperlinkOnClick;
        this.PART_InheritedFromTextBox.TextChanged += this.PART_InheritedFromTextBoxOnTextChanged;

        this.UpdateCanSetInheritedKeyButton();

        this.treePageBinder.AttachControl(this.PART_ThemeConfigTree);
        this.isResetEnabledBinder.Value1 = this.PART_ResetButton;
        this.showBuiltInThemeWarningBinder.Value1 = this.PART_WarnEditingBuiltInTheme;
        this.gbHeaderBinder.Value1 = this.PART_GroupBox;
        this.pickerColourBinder.Value1 = this.PART_ColorPicker;
    }

    private void UpdateCanResetValue() {
        ThemeConfigurationPage? page = this.Page;
        if (page != null && page.TargetTheme != null && this.activeThemeKey != null) {
            this.isResetEnabledBinder.Value2 = page.HasThemeKeyChanged(page.TargetTheme, this.activeThemeKey);
        }
        else {
            this.isResetEnabledBinder.Value2 = false;
        }
    }

    private void OnTargetThemeChanged(ThemeConfigurationPage sender, Theme? oldTheme, Theme? newTheme) {
        this.UpdateGroupBoxAndWarningMessage();
        this.UpdateCanResetValue();
    }

    private void OnThemeEntryModified(ThemeConfigurationPage sender, string key, bool isAdded) {
        // we only need to update if the changed key is the one we're viewing.
        // It should be the one we're viewing anyway
        if (key == this.activeThemeKey)
            this.UpdateCanResetValue();
    }

    private void OnThemeModifiedThemeEntriesCleared(ThemeConfigurationPage sender, Dictionary<string, ISavedThemeEntry> oldItems) {
        this.UpdateCanResetValue();
    }

    private void UpdateGroupBoxAndWarningMessage() {
        Theme? theme = this.Page?.TargetTheme;
        if (theme != null) {
            this.showBuiltInThemeWarningBinder.Value2 = theme.IsBuiltIn;
            this.gbHeaderBinder.Value2 = $"Current theme: {theme.Name}";
            this.IsEnabled = true;
            if (this.activeThemeKey != null && ((ThemeManagerImpl.ThemeImpl) theme).TryFindBrushInHierarchy(this.activeThemeKey, out IBrush? brush)) {
                this.OnColourChangedInTheme(brush);
            }
        }
        else {
            this.showBuiltInThemeWarningBinder.Value2 = false;
            this.gbHeaderBinder.Value2 = "<No Theme Selected>";
            this.IsEnabled = false;
            this.OnColourChangedInTheme(null);
        }
    }

    private void OnColourChangedInTheme(IBrush? obj) {
        if (obj is ISolidColorBrush bruh) {
            this.ignoreSpectrumColourChange = true;
            this.pickerColourBinder.Value2 = bruh.Color;
            this.ignoreSpectrumColourChange = false;
        }
        else {
            this.pickerColourBinder.Value2 = null;
        }
    }

    private void OnInheritedFromKeyChanged(ThemeConfigEntry sender) {
        this.UpdateCanSetInheritedKeyButton();
    }

    private void SetActiveThemeKey(string? themeKey) {
        if (themeKey == this.activeThemeKey) {
            return;
        }

        if (this.disposeMyActiveBrush != null) {
            this.disposeMyActiveBrush.Dispose();
            this.disposeMyActiveBrush = null;
            this.myActiveBrush = null;
            this.activeThemeKey = null;
        }

        if (themeKey != null) {
            this.activeThemeKey = themeKey;
            this.myActiveBrush = ((BrushManagerImpl) BrushManager.Instance).GetDynamicThemeBrush(themeKey);
            this.disposeMyActiveBrush = this.myActiveBrush.Subscribe(this.OnColourChangedInTheme);
            this.UpdateCanResetValue();
        }
        else {
            this.isResetEnabledBinder.Value2 = false;
        }
    }

    public override void OnConnected() {
        base.OnConnected();
        this.treePageBinder.AttachModel(this.Page!);
        this.Page!.TargetThemeChanged += this.OnTargetThemeChanged;
        this.Page!.ThemeEntryModified += this.OnThemeEntryModified;
        this.Page!.ModifiedThemeEntriesCleared += this.OnThemeModifiedThemeEntriesCleared;
        this.UpdateGroupBoxAndWarningMessage();
        this.UpdateCanResetValue();
        this.UpdateCanSetInheritedKeyButton();
        DataManager.GetContextData(this).Set(ThemeContextRegistry.ThemeConfigurationPageKey, this.Page!);
    }

    public override void OnDisconnected() {
        base.OnDisconnected();
        this.treePageBinder.DetachModel();
        this.Page!.TargetThemeChanged -= this.OnTargetThemeChanged;
        this.Page!.ThemeEntryModified -= this.OnThemeEntryModified;
        this.Page!.ModifiedThemeEntriesCleared -= this.OnThemeModifiedThemeEntriesCleared;
        DataManager.GetContextData(this).Remove(ThemeContextRegistry.ThemeConfigurationPageKey);
    }

    #region CONTROL USAGE SAFE

    private void UpdateCanSetInheritedKeyButton() {
        if (this.PART_SetInheritedFromKeyButton == null) {
            return;
        }

        if (this.currentTCE == null) {
            this.PART_SetInheritedFromKeyButton.IsEnabled = false;
            return;
        }

        Theme? theme;
        ThemeConfigurationPage? page = this.Page;
        if (page == null || (theme = page.TargetTheme) == null) {
            this.PART_SetInheritedFromKeyButton.IsEnabled = false;
        }
        else {
            string? themeKey = this.PART_InheritedFromTextBox!.Text;
            if (string.IsNullOrWhiteSpace(themeKey)) {
                this.PART_SetInheritedFromKeyButton.IsEnabled = this.currentTCE!.InheritedFromKey != null;
            }
            else {
                this.PART_SetInheritedFromKeyButton.IsEnabled = theme.IsThemeKeyValid(themeKey);
            }
        }
    }

    private void PART_InheritedFromTextBoxOnTextChanged(object? sender, TextChangedEventArgs e) {
        this.UpdateCanSetInheritedKeyButton();
    }

    private void PART_SetInheritedFromKeyButtonOnClick(object? sender, RoutedEventArgs e) {
        Theme? currentTheme;
        if (this.currentTCE == null || (currentTheme = this.Page!.TargetTheme) == null) {
            return;
        }

        string? newKey = this.PART_InheritedFromTextBox!.Text;
        if (string.IsNullOrWhiteSpace(newKey)) {
            newKey = null;
        }

        this.PART_InheritedFromTextBox!.Text = newKey;
        BugFix.TextBox_UpdateSelection(this.PART_InheritedFromTextBox);

        string? oldKey = this.currentTCE.InheritedFromKey;

        // we're setting the inheritance directly on the theme the entry is representing, so 0 depth is what we want
        currentTheme.SetInheritance(this.currentTCE.ThemeKey, newKey);
        this.currentTCE.SetInheritedFromKey(newKey, 0);
        this.Page!.OnChangedInheritance(this.currentTCE, oldKey, newKey);
    }

    private void PART_InheritedFromHyperlinkOnClick(object? sender, RoutedEventArgs e) {
        if (this.isExpandingSubTree || this.currentTCE == null) {
            return;
        }

        string? inheritedKey = this.currentTCE.InheritedFromKey;
        if (!string.IsNullOrEmpty(inheritedKey) && this.Page!.TryGetThemeEntryFromThemeKey(inheritedKey, out ThemeConfigEntry? dstEntry)) {
            ThemeConfigTreeViewItem dstTreeItem = this.PART_ThemeConfigTree!.ItemMap.GetControl(dstEntry);
            List<ThemeConfigTreeViewItem> list = new List<ThemeConfigTreeViewItem>();
            for (ThemeConfigTreeViewItem? node = dstTreeItem.ParentNode; node != null && !node.IsExpanded; node = node.ParentNode)
                list.Add(node);

            this.isExpandingSubTree = true;
            try {
                for (int i = list.Count - 1; i >= 0; i--)
                    ApplicationPFX.Instance.Dispatcher.Invoke(() => list[i].IsExpanded = true, DispatchPriority.Loaded);

                this.PART_ThemeConfigTree.SelectedItem = dstTreeItem;
                this.PART_ThemeConfigTree.ScrollIntoView(dstTreeItem);
            }
            finally {
                this.isExpandingSubTree = false;
            }
        }
    }

    private void PART_ResetButtonOnClick(object? sender, RoutedEventArgs e) {
        ThemeConfigurationPage? page = this.Page;
        if (page != null && page.TargetTheme != null && this.activeThemeKey != null) {
            page.ReverseChangeFor(page.TargetTheme, this.activeThemeKey);
        }
    }

    private void PART_ColorPickerOnColourChanged(object? sender, ColorChangedEventArgs e) {
        if (this.ignoreSpectrumColourChange) {
            return;
        }

        if (this.activeThemeKey != null && this.Page!.TargetTheme is Theme theme) {
            this.Page!.OnChangingThemeColour(this.activeThemeKey);

            Color c = e.NewColor;
            theme.SetThemeColour(this.activeThemeKey, new SKColor(c.R, c.G, c.B, c.A));
        }
    }

    private void PART_ThemeConfigTreeOnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.currentTCE != null) {
            this.currentTCE.InheritedFromKeyChanged -= this.OnInheritedFromKeyChanged;
            this.currentTCE = null;
        }

        if (this.PART_ThemeConfigTree!.SelectedItem is ThemeConfigTreeViewItem item) {
            if (item.Entry is ThemeConfigEntry configEntry) {
                this.currentTCE = configEntry;
                this.currentTCE.InheritedFromKeyChanged += this.OnInheritedFromKeyChanged;

                this.PART_ThemeKeyTextBox!.Text = configEntry.ThemeKey;
                this.PART_ThemeKeyTextBox.IsEnabled = true;

                string? inheritedKey = configEntry.InheritedFromKey;
                if (!string.IsNullOrEmpty(inheritedKey)) {
                    this.PART_NavigateToInheritedKeyButton!.IsEnabled = true;
                    this.PART_InheritedFromTextBox!.Text = inheritedKey;
                }
                else {
                    this.PART_NavigateToInheritedKeyButton!.IsEnabled = false;
                    this.PART_InheritedFromTextBox!.Text = null;
                }

                BugFix.TextBox_UpdateSelection(this.PART_InheritedFromTextBox);
                this.PART_ColorPicker!.IsEnabled = true;
                this.SetActiveThemeKey(configEntry.ThemeKey);
            }
            else {
                this.PART_ThemeKeyTextBox!.Text = "-";
                this.PART_ThemeKeyTextBox.IsEnabled = false;
                this.PART_ColorPicker!.IsEnabled = false;
                this.SetActiveThemeKey(null);
            }

            BugFix.TextBox_UpdateSelection(this.PART_ThemeKeyTextBox);
            this.PART_SelectedItemGrid!.IsEnabled = true;
        }
        else {
            this.PART_SelectedItemGrid!.IsEnabled = false;
            this.PART_ColorPicker!.IsEnabled = false;
            this.SetActiveThemeKey(null);
        }

        this.colourBinding?.Dispose();
        this.colourBinding = null;
    }

    #endregion
}
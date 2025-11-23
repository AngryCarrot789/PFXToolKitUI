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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.Configurations.Trees;
using PFXToolKitUI.Avalonia.ToolTips;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Avalonia.Configurations;

public partial class ConfigurationPanelControl : UserControl {
    public static readonly StyledProperty<ConfigurationManager?> ConfigurationManagerProperty = AvaloniaProperty.Register<ConfigurationPanelControl, ConfigurationManager?>(nameof(ConfigurationManager));

    private ConfigurationEntry? connectedEntry;
    private ConfigurationContext? activeContext;
    private readonly Dictionary<Type, BaseConfigurationPageControl> pageControlCache = new Dictionary<Type, BaseConfigurationPageControl>();

    public ConfigurationManager? ConfigurationManager {
        get => this.GetValue(ConfigurationManagerProperty);
        set => this.SetValue(ConfigurationManagerProperty, value);
    }

    public ConfigurationContext? ActiveContext {
        get => this.activeContext;
        set => PropertyHelper.SetAndRaiseINE(ref this.activeContext, value, this, this.ActiveContextChanged);
    }

    public bool IsConfigurationManagerChanging { get; private set; }

    public event EventHandler<ValueChangedEventArgs<ConfigurationContext?>>? ActiveContextChanged;

    public ConfigurationPanelControl() {
        this.InitializeComponent();
        this.PART_ConfigurationTree.SelectionMode = SelectionMode.Single;
        this.PART_ConfigurationTree.SelectionChanged += this.OnTreeSelectionChanged;
    }

    static ConfigurationPanelControl() {
        ConfigurationManagerProperty.Changed.AddClassHandler<ConfigurationPanelControl, ConfigurationManager?>((d, e) => d.OnConfigurationManagerChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.ActiveContext != null) {
            this.OnSelectionChanged();
        }
    }

    private void ClearSelection() {
        this.PART_ConfigurationTree.UnselectAll();
    }

    private void OnSelectionChanged() {
        if (this.activeContext == null) {
            bool a = this.PART_ConfigurationTree.SelectedItem == null;
            bool b = this.PART_PagePresenter.Content == null;
            Debug.Assert((!a && !b) || (a && b), "Expected page to be disconnected with no active context");
        }
        else if (this.PART_ConfigurationTree.SelectedItem is ConfigurationTreeViewItem item && item.Entry != null) {
            if (this.connectedEntry == item.Entry) {
                return;
            }

            ConfigurationPage? page = item.Entry.Page;
            if (page == null) {
                ConfigurationEntry? firstChild = item.Entry.Items.FirstOrDefault(x => x.Page != null);
                if (firstChild != null) {
                    page = firstChild.Page;
                }
            }

            if (page != null) {
                this.DisconnectPage();

                if (!this.pageControlCache.TryGetValue(page.GetType(), out BaseConfigurationPageControl? control))
                    control = ConfigurationPageRegistry.Registry.NewInstance(page, false);

                this.ConnectPage(page, control);
                this.connectedEntry = item.Entry;
                this.UpdateNavigationHeading();
            }
        }
        else {
            this.DisconnectPage();
            this.connectedEntry = null;
            this.UpdateNavigationHeading();
        }
    }

    private void UpdateNavigationHeading() {
        List<ConfigurationEntry> entries = new List<ConfigurationEntry>();
        for (ConfigurationEntry? entry = this.connectedEntry; entry != null && !entry.IsRoot; entry = entry.Parent) {
            entries.Add(entry);
        }

        if (entries.Count < 1) {
            this.PART_NavigationPathTextBlock.Inlines = null;
            return;
        }

        InlineCollection inlines = this.PART_NavigationPathTextBlock.Inlines ??= new InlineCollection();
        inlines.Clear();

        int i = entries.Count - 1;
        this.ApplyInline(inlines, entries[i--]);
        while (i >= 0) {
            ApplyInlineSeparator(inlines);
            this.ApplyInline(inlines, entries[i--]);
        }
    }

    private static void ApplyInlineSeparator(InlineCollection collection) {
        collection.Add(new Run(" / ") { BaselineAlignment = BaselineAlignment.Center });
    }

    private class HyperlinkTagInfo {
        private readonly WeakReference entry;
        private readonly WeakReference editor;

        public ConfigurationEntry? Entry => (ConfigurationEntry?) this.entry.Target;

        public ConfigurationPanelControl? Editor => (ConfigurationPanelControl?) this.editor.Target;

        public HyperlinkTagInfo(ConfigurationEntry entry, ConfigurationPanelControl editor) {
            this.entry = new WeakReference(entry);
            this.editor = new WeakReference(editor);
        }
    }

    private void ApplyInline(InlineCollection collection, ConfigurationEntry entry) {
        HyperlinkButton hyperlinkButton = new HyperlinkButton() {
            Content = entry.DisplayName ?? "Unnamed Configuration",
            ClickMode = ClickMode.Press,
            Tag = entry.FullIdPath
        };

        ToolTipEx.SetTip(hyperlinkButton, "Navigate to " + (entry.DisplayName ?? entry.FullIdPath ?? "this unnamed entry"));
        hyperlinkButton.Click += this.OnHyperlinkClicked;
        collection.Add(hyperlinkButton);
    }

    private void OnHyperlinkClicked(object? sender, RoutedEventArgs e) {
        if (((HyperlinkButton?) sender)?.Tag is string fullId) {
            ConfigurationManager? manager = this.ConfigurationManager;
            if (manager != null && manager.RootEntry.TryFindEntry(fullId, out ConfigurationEntry? entry)) {
                if (this.PART_ConfigurationTree.ItemMap.TryGetControl(entry, out ConfigurationTreeViewItem? treeItem)) {
                    treeItem.ResourceTree?.SetSelection(treeItem);
                }
            }
        }
    }

    private void DisconnectPage() {
        if (this.PART_PagePresenter.Content is BaseConfigurationPageControl page) {
            this.pageControlCache[page.Page!.GetType()] = page;
            page.Disconnect();
        }

        this.PART_PagePresenter.Content = null;
        this.ActiveContext!.SetViewingPage(null);
    }

    private void ConnectPage(ConfigurationPage configPage, BaseConfigurationPageControl page) {
        this.PART_PagePresenter.Content = page;
        page.Connect(configPage);
        this.ActiveContext!.SetViewingPage(configPage);
    }

    // ReSharper disable once AsyncVoidMethod -- we try-catch the parts that might throw
    private async void OnConfigurationManagerChanged(ConfigurationManager? oldValue, ConfigurationManager? newValue) {
        // Process() should return after Loaded events
        const DispatchPriority ProcessPriority = DispatchPriority.Default;

        this.SetLoadingState(true);
        await ApplicationPFX.Instance.Dispatcher.Process(ProcessPriority);

        try {
            if (oldValue != null) {
                Debug.Assert(this.ActiveContext != null, "Context could not be null if we have an old value");
                this.ClearSelection();
                await ConfigurationManager.InternalUnloadContext(oldValue, this.ActiveContext);
            }
        }
        catch (Exception ex) {
            await IMessageDialogService.Instance.ShowMessage("Error", "Error unloading settings properties", ex.ToString());
            throw;
        }

        // Manager being set to null, so set selected page's tree node
        // to null, which should disconnect the page, otherwise we got a memory leak
        if (newValue == null && this.connectedEntry != null) {
            this.DisconnectPage();
            this.connectedEntry = null;
            this.PART_NavigationPathTextBlock.Inlines = null;
        }

        this.ActiveContext?.OnDestroyed();
        this.ActiveContext = null;
        this.PART_ConfigurationTree.RootConfigurationEntry = null;

        if (newValue != null) {
            try {
                this.ActiveContext = new ConfigurationContext();
                await ConfigurationManager.InternalLoadContext(newValue, this.ActiveContext);
            }
            catch (Exception ex) {
                await IMessageDialogService.Instance.ShowMessage("Error", "Error loading settings properties", ex.ToString());
                throw;
            }

            this.ActiveContext!.OnCreated();
            this.PART_ConfigurationTree.RootConfigurationEntry = newValue.RootEntry;
            if (this.PART_ConfigurationTree.Items.Count > 0)
                this.PART_ConfigurationTree.SelectedItem = this.PART_ConfigurationTree.Items[0];
        }

        await ApplicationPFX.Instance.Dispatcher.Process(ProcessPriority);
        ApplicationPFX.Instance.Dispatcher.Post(() => {
            this.SetLoadingState(false);
            this.OnSelectionChanged();
        }, DispatchPriority.SystemIdle);
    }

    private void SetLoadingState(bool isLoading) {
        this.IsConfigurationManagerChanging = isLoading;
        if (isLoading) {
            this.PART_CoreContentGrid.Opacity = 0.5;
            this.PART_LoadingBorder.IsVisible = true;
        }
        else {
            this.PART_CoreContentGrid.Opacity = 1.0;
            this.PART_LoadingBorder.IsVisible = false;
        }
    }
}
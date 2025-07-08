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

using Avalonia.Controls;
using PFXToolKitUI.Avalonia.AvControls.ListBoxes;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Logging;

namespace PFXToolKitUI.Avalonia.Services;

public partial class LogsWindow : DesktopWindow {
    public LogsWindow() {
        this.InitializeComponent();
        this.PART_ListBox.SelectionChanged += this.OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (this.PART_ListBox.SelectedModel is LogEntry entry) {
            this.PART_StackTrace.Watermark = null;
            this.PART_StackTrace.Text = entry.StackTrace;
        }
        else {
            this.PART_StackTrace.Watermark = "Select a log entry to see the stack trace";
            this.PART_StackTrace.Text = null;
        }
    }

    protected override void OnOpenedCore() {
        base.OnOpenedCore();
        this.PART_ListBox.SetItemsSource(AppLogger.Instance.Entries);
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        this.PART_ListBox.SetItemsSource(null);
    }
}

public class LogEntryListBox : ModelBasedListBox<LogEntry> {
    protected override Type StyleKeyOverride => typeof(ListBox);
    protected override ModelBasedListBoxItem<LogEntry> CreateItem() => new LogEntryListBoxItem();
}

public class LogEntryListBoxItem : ModelBasedListBoxItem<LogEntry> {
    private readonly TextBlock tb;

    protected override Type StyleKeyOverride => typeof(ListBoxItem);
    
    public LogEntryListBoxItem() {
        this.Content = this.tb = new TextBlock();
    }

    protected override void OnAddingToList() {
    }

    protected override void OnAddedToList() {
        LogEntry m = this.Model!;
        this.tb.Text = $"[{m.LogTime:hh:mm:ss ff}] {m.Content}";
    }

    protected override void OnRemovingFromList() {
        this.tb.Text = null;
    }

    protected override void OnRemovedFromList() {
    }
}
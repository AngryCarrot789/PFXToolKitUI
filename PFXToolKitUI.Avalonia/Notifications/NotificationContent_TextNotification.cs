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
using Avalonia.Controls.Primitives;
using AvaloniaEdit;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Notifications;

namespace PFXToolKitUI.Avalonia.Notifications;

internal class NotificationContent_TextNotification : TemplatedControl, INotificationContent {
    private readonly TextNotification notification;

    private IconControl? PART_IconControl;
    private TextEditor? PART_Message;

    public NotificationContent_TextNotification(TextNotification textNotification) {
        this.notification = textNotification;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_IconControl = e.NameScope.GetTemplateChild<IconControl>(nameof(this.PART_IconControl));
        this.PART_Message = e.NameScope.GetTemplateChild<TextEditor>(nameof(this.PART_Message));
        this.SetText();
        this.SetIcon();
    }

    private void OnTextChanged(Notification sender) {
        Debug.Assert(this.notification == sender);
        this.SetText();
    }

    private void OnIconChanged(TextNotification sender, Icon? oldIcon, Icon? newIcon) {
        Debug.Assert(this.notification == sender);
        this.SetIcon();
    }

    public void OnShown() {
        this.notification.TextChanged += this.OnTextChanged;
        this.notification.IconChanged += this.OnIconChanged;
        this.SetText();
        this.SetIcon();
    }

    public void OnHidden() {
        this.notification.TextChanged -= this.OnTextChanged;
        this.notification.IconChanged -= this.OnIconChanged;
        if (this.PART_IconControl != null) {
            this.PART_IconControl.Icon = null;
        }
    }
    
    private void SetText() {
        if (this.PART_Message != null)
            this.PART_Message.Text = this.notification.Text;
    }

    private void SetIcon() {
        if (this.PART_IconControl != null) {
            this.PART_IconControl.Icon = this.notification.Icon;
            this.PART_IconControl.IsVisible = this.notification.Icon != null;
        }
    }
}
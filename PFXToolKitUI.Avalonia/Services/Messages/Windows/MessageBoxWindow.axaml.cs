// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.Avalonia.Services.Messages.Windows;

public partial class MessageBoxWindow : DesktopWindow {
    public static readonly StyledProperty<MessageBoxInfo?> MessageBoxDataProperty = AvaloniaProperty.Register<MessageBoxWindow, MessageBoxInfo?>("MessageBoxData");

    public MessageBoxInfo? MessageBoxData {
        get => this.GetValue(MessageBoxDataProperty);
        set => this.SetValue(MessageBoxDataProperty, value);
    }
    
    private readonly IBinder<MessageBoxInfo> captionBinder = new EventPropertyBinder<MessageBoxInfo>(nameof(MessageBoxInfo.CaptionChanged), (b) => ((MessageBoxWindow) b.Control).Title = b.Model.Caption ?? "Alert");
    
    public MessageBoxWindow() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.SizeToContent = SizeToContent.Manual;
    }

    static MessageBoxWindow() {
        MessageBoxDataProperty.Changed.AddClassHandler<MessageBoxWindow, MessageBoxInfo?>((o, e) => o.OnMessageBoxDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnMessageBoxDataChanged(MessageBoxInfo? oldInfo, MessageBoxInfo? newInfo) {
        this.captionBinder.SwitchModel(newInfo);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && !e.Handled) {
            this.Close();
        }
    }
}
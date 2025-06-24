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
using Avalonia.Input;
using Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Bindings;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Services.UserInputs;

namespace PFXToolKitUI.Avalonia.Services.UserInputs;

public partial class UserInputDialogWindow : DesktopWindow {
    private readonly IBinder<UserInputInfo> captionBinder = new EventUpdateBinder<UserInputInfo>(nameof(UserInputInfo.CaptionChanged), b => b.Control.SetValue(TitleProperty, b.Model.Caption));
    
    public static readonly StyledProperty<UserInputInfo?> UserInputInfoProperty = AvaloniaProperty.Register<UserInputDialogWindow, UserInputInfo?>("UserInputInfo");

    public UserInputInfo? UserInputInfo {
        get => this.GetValue(UserInputInfoProperty);
        set => this.SetValue(UserInputInfoProperty, value);
    }
    
    public UserInputDialogWindow() {
        this.InitializeComponent();
        this.captionBinder.AttachControl(this);
    }

    static UserInputDialogWindow() {
        UserInputInfoProperty.Changed.AddClassHandler<UserInputDialogWindow, UserInputInfo?>((o, e) => o.OnUserInputDataChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    protected void OnKeyDown(object? sender, KeyEventArgs e) {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Escape) {
            this.PART_UserInputDialogView.TryCloseDialog(false);
        }
    }

    private void OnUserInputDataChanged(UserInputInfo? oldInfo, UserInputInfo? newInfo) {
        this.captionBinder.SwitchModel(newInfo);
    }

    protected override void OnOpenedCore() {
        this.AddHandler(KeyDownEvent, this.OnKeyDown, RoutingStrategies.Tunnel);
        this.CanResize = false;
        this.PART_UserInputDialogView.OnWindowOpened();
    }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        this.PART_UserInputDialogView.OnWindowClosed();
    }
}
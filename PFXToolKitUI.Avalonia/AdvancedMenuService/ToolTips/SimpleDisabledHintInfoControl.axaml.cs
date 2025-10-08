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
using PFXToolKitUI.CommandSystem;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService.ToolTips;

public partial class SimpleDisabledHintInfoControl : UserControl, IDisabledHintInfoControl {
    public SimpleDisabledHintInfoControl() {
        this.InitializeComponent();
    }

    public void OnConnected(DisabledHintInfo _info) {
        SimpleDisabledHintInfo info = (SimpleDisabledHintInfo) _info;
        this.PART_Caption.IsVisible = !string.IsNullOrWhiteSpace(info.Caption);
        this.PART_Caption.Text = info.Caption;

        this.PART_MainText.IsVisible = !string.IsNullOrWhiteSpace(info.Message);
        this.PART_MainText.Text = info.Message;
    }

    public void OnDisconnected(DisabledHintInfo _info) {
        SimpleDisabledHintInfo info = (SimpleDisabledHintInfo) _info;
    }
}
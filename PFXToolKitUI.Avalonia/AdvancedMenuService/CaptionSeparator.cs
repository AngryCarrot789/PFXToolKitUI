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

using Avalonia;
using Avalonia.Controls.Primitives;
using PFXToolKitUI.AdvancedMenuService;

namespace PFXToolKitUI.Avalonia.AdvancedMenuService;

public class CaptionSeparator : TemplatedControl, IAdvancedEntryConnection {
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<CaptionSeparator, string?>(nameof(Text));

    public string? Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public CaptionEntry? Entry { get; private set; }

    IContextObject? IAdvancedEntryConnection.Entry => this.Entry;

    public CaptionSeparator() {
    }

    public void OnAdding(IAdvancedMenu menu, IAdvancedMenuOrItem parent, IContextObject entry) {
        this.Entry = (CaptionEntry) entry;
    }

    public void OnAdded() {
        this.Entry!.TextChanged += this.OnTextChanged;
        this.OnTextChanged(this.Entry);
    }

    public void OnRemoving() {
        this.Entry!.TextChanged -= this.OnTextChanged;
    }

    public void OnRemoved() {
        this.Entry = null;
    }

    private void OnTextChanged(CaptionEntry sender) {
        this.Text = sender.Text;
    }
}
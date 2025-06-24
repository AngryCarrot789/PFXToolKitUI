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

using PFXToolKitUI.Utils;

namespace PFXToolKitUI.AdvancedMenuService;

public delegate void GroupCaptionEntryEventHandler(CaptionEntry sender);

/// <summary>
/// A context entry that is placed above a group of items. This is not necessarily related to <see cref="IContextGroup"/> objects
/// </summary>
public class CaptionEntry : IContextObject {
    private string? text;

    /// <summary>
    /// Gets or sets the text of this caption entry
    /// </summary>
    public string? Text {
        get => this.text;
        set => PropertyHelper.SetAndRaiseINE(ref this.text, value, this, static t => t.TextChanged?.Invoke(t));
    }

    public event GroupCaptionEntryEventHandler? TextChanged;

    public CaptionEntry(string text) {
        this.text = text;
    }
}
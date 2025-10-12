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

public delegate void GroupCaptionEntryEventHandler(CaptionSeparatorEntry sender);

/// <summary>
/// A separator entry that also has text placed in between the separator line
/// </summary>
public class CaptionSeparatorEntry : IMenuEntry {
    private string? text;

    /// <summary>
    /// Gets or sets our text. Note, overly long text may be clipped or trimmed with ellipses
    /// if it were to take up an unreasonable amount of UI space.
    /// Therefore, this should ideally have no more than around 20 characters
    /// </summary>
    public string? Text {
        get => this.text;
        set => PropertyHelper.SetAndRaiseINE(ref this.text, value, this, static t => t.TextChanged?.Invoke(t));
    }

    public event GroupCaptionEntryEventHandler? TextChanged;

    public CaptionSeparatorEntry(string text) {
        this.text = text;
    }
}
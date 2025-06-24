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

using PFXToolKitUI.DataTransfer;

namespace PFXToolKitUI.Themes.Configurations;

public delegate void ThemeConfigEntryEventHandler(ThemeConfigEntry sender);

/// <summary>
/// An entry for a colour in a theme
/// </summary>
public class ThemeConfigEntry : IThemeTreeEntry, ITransferableData {
    /// <summary>
    /// Gets the name of this entry. Cannot contain the '/' character
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the application theme key for this entry. This is what the UI uses to query a colour
    /// </summary>
    public string ThemeKey { get; }

    /// <summary>
    /// Gets a description of what the <see cref="ThemeKey"/> is used for
    /// </summary>
    public string? Description { get; internal set; }
    
    /// <summary>
    /// Gets the depth that our <see cref="InheritedFromKey"/> is
    /// </summary>
    public int InheritanceDepth { get; private set; }

    /// <summary>
    /// Gets the key that this theme entry inherits from. Can only be null or a non-empty and non-whitespace-containing string.
    /// <para>
    /// Do not set this property, it should only be set by internal utilities
    /// </para>
    /// </summary>
    public string? InheritedFromKey { get; set; }

    public TransferableData TransferableData { get; }

    /// <summary>
    /// Raised when <see cref="InheritedFromKey"/> changes. This is only fired internally
    /// </summary>
    public event ThemeConfigEntryEventHandler? InheritedFromKeyChanged;
    
    public ThemeConfigEntry(string displayName, string themeKey) {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(themeKey);
        if (displayName.Contains('/'))
            throw new ArgumentException("Display name cannot contain a forward slash");

        this.TransferableData = new TransferableData(this);
        this.ThemeKey = themeKey;
        this.DisplayName = displayName;
    }

    public void SetInheritedFromKey(string? inheritFrom, int depth) {
        if (string.IsNullOrWhiteSpace(inheritFrom))
            inheritFrom = null;
        if (this.InheritanceDepth == depth && EqualityComparer<string?>.Default.Equals(this.InheritedFromKey, inheritFrom))
            return;
        if (inheritFrom == null && depth != 0)
            throw new ArgumentException("Expected depth to be 0 when inheritFrom value is empty");

        this.InheritedFromKey = inheritFrom;
        this.InheritanceDepth = depth;
        this.InheritedFromKeyChanged?.Invoke(this);
    }
}
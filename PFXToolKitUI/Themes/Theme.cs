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

using SkiaSharp;

namespace PFXToolKitUI.Themes;

public delegate void ThemeColourChangedEventHandler(Theme theme, string key, SKColor newColour);

/// <summary>
/// Represents a theme 
/// </summary>
public abstract class Theme {
    /// <summary>
    /// Gets the theme manager associated with this theme
    /// </summary>
    public abstract ThemeManager ThemeManager { get; }

    /// <summary>
    /// Gets the name of this theme
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the theme that this theme is based on. Null for built-in themes
    /// </summary>
    public abstract Theme? BasedOn { get; }

    /// <summary>
    /// Gets the theme keys for this theme. This may get modified by <see cref="SetThemeColour"/>
    /// </summary>
    public abstract IEnumerable<string> ThemeKeys { get; }

    /// <summary>
    /// Gets the inheritance entries. This maps a theme key to the key it wants to inherit from (themeKey->inheritFrom).
    /// This is the reverse of what <see cref="CollectKeysInheritedBy"/> provides
    /// </summary>
    public abstract IEnumerable<KeyValuePair<string, string>> InheritanceEntries { get; }

    /// <summary>
    /// Returns true if this is a built-in theme, which is not saved at runtime since the
    /// resource dictionary is loaded from an AXAML file.
    /// </summary>
    public bool IsBuiltIn { get; }

    protected Theme(bool isBuiltIn) {
        this.IsBuiltIn = isBuiltIn;
    }

    /// <summary>
    /// Sets a theme colour to the given value. This method may
    /// trigger UI rendering if the theme key is in use
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="colour">The new colour</param>
    public abstract void SetThemeColour(string key, SKColor colour);

    /// <summary>
    /// Sets a theme colour to the given brush. This method may trigger UI rendering if
    /// the theme key is in use. Using this method is not recommended since the brush may
    /// not have any solid colour associated with it, though this might be fine for the specific key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="colour">The new brush</param>
    public abstract void SetThemeBrush(string key, IColourBrush brush);

    /// <summary>
    /// Returns true if the theme key is a real key that would return a colour
    /// </summary>
    /// <param name="themeKey">The key</param>
    /// <param name="allowInherited">True to allow searching this and parent themes, False to only allow searching this theme</param>
    /// <returns>See above</returns>
    public abstract bool IsThemeKeyValid(string themeKey, bool allowInherited = true);

    /// <summary>
    /// Sets or removes the theme colour as inherited from the given inherited key
    /// </summary>
    /// <param name="themeKey">The key to update the inheritance of</param>
    /// <param name="inheritFromKey">The new inheritance source, or null, to remove inheritance</param>
    public abstract void SetInheritance(string themeKey, string? inheritFromKey);
    
    /// <summary>
    /// Bulk operation for <see cref="SetInheritance(string,string?)"/> since that can be expensive
    /// when modifying built in themes or densely modified themes in general
    /// </summary>
    /// <param name="entries">The entries. A null value means we remove inheritance</param>
    public abstract void SetInheritance(IEnumerable<KeyValuePair<string, string?>> entries);
    
    /// <summary>
    /// Gets the key that the given key inherits the colour value from
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <param name="depth">The base theme depth the inheritance is. -1 when we return null</param>
    /// <returns></returns>
    public abstract string? GetInheritedBy(string key, out int depth);

    /// <summary>
    /// Collects all theme keys that inherit from the given key, optionally supports scanning the based on themes too
    /// </summary>
    /// <param name="inheritFrom">The key that other keys inherit from</param>
    /// <param name="keys">The sink set</param>
    /// <param name="includeBasedOnThemes">True to include base themes too</param>
    public abstract void CollectKeysInheritedBy(string inheritFrom, HashSet<string> keys, bool includeBasedOnThemes = true);

    /// <summary>
    /// Creates a saved theme entry that stores the current state of the colour
    /// </summary>
    /// <param name="themeKey">The key</param>
    /// <returns>A saved theme entry</returns>
    public abstract ISavedThemeEntry SaveThemeEntry(string themeKey);

    /// <summary>
    /// Applies a saved theme entry into this theme 
    /// </summary>
    /// <param name="themeKey">The key</param>
    /// <param name="entry">The entry</param>
    public abstract void ApplyThemeEntry(string themeKey, ISavedThemeEntry entry);
}
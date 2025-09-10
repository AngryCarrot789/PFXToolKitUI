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

using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Themes;

public delegate void ThemeManagerActiveThemeChangedEventHandler(ThemeManager manager, Theme oldTheme, Theme newTheme);

public abstract class ThemeManager {
    public static ThemeManager Instance => ApplicationPFX.GetService<ThemeManager>();

    /// <summary>
    /// Gets the themes that currently exist
    /// </summary>
    public abstract ReadOnlyObservableList<Theme> Themes { get; }

    /// <summary>
    /// Gets the theme that is currently active
    /// </summary>
    public abstract Theme ActiveTheme { get; }

    public ThemeConfigurationPage ThemeConfigurationPage { get; }
    
    /// <summary>
    /// An event fired when the active theme changes. This is not fired when the initial
    /// active theme is set up, which happens after application startup
    /// </summary>
    public event ThemeManagerActiveThemeChangedEventHandler? ActiveThemeChanged;

    protected ThemeManager() {
        ThemeConfigurationPage p = this.ThemeConfigurationPage = new ThemeConfigurationPage();
        // Standard theme options since 2020 when WPFDarkTheme was made
        p.AssignMapping("Base/Foreground (Static)", "ABrush.Foreground.Static");
        p.AssignMapping("Base/Foreground (Deeper)", "ABrush.Foreground.Deeper");
        p.AssignMapping("Base/Foreground (Disabled)", "ABrush.Foreground.Disabled");
        p.AssignMapping("Base/Glyphs/Glyph (Static)", "ABrush.Glyph.Static");
        p.AssignMapping("Base/Glyphs/Glyph (Disabled)", "ABrush.Glyph.Disabled");
        p.AssignMapping("Base/Glyphs/Glyph (MouseOver)", "ABrush.Glyph.MouseOver");
        p.AssignMapping("Base/Glyphs/Glyph (MouseDown)", "ABrush.Glyph.MouseDown");
        p.AssignMapping("Base/Glyphs/Glyph (Selected)", "ABrush.Glyph.Selected");
        p.AssignMapping("Base/Glyphs/Glyph (Selected Inactive)", "ABrush.Glyph.Selected.Inactive");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (Static)", "ABrush.ColourfulGlyph.Static");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (Disabled)", "ABrush.ColourfulGlyph.Disabled");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (MouseOver)", "ABrush.ColourfulGlyph.MouseOver");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (MouseDown)", "ABrush.ColourfulGlyph.MouseDown");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (Selected)", "ABrush.ColourfulGlyph.Selected");
        p.AssignMapping("Base/Glyphs/ColourfulGlyph (Selected Inactive)", "ABrush.ColourfulGlyph.Selected.Inactive");
        p.AssignMapping("Base/AccentTone1/Background (Static)", "ABrush.AccentTone1.Background.Static");
        p.AssignMapping("Base/AccentTone1/Border (Static)", "ABrush.AccentTone1.Border.Static");
        p.AssignMapping("Base/AccentTone2/Background (Static)", "ABrush.AccentTone2.Background.Static");
        p.AssignMapping("Base/AccentTone2/Border (Static)", "ABrush.AccentTone2.Border.Static");
        p.AssignMapping("Base/AccentTone3/Background (Static)", "ABrush.AccentTone3.Background.Static");
        p.AssignMapping("Base/AccentTone3/Border (Static)", "ABrush.AccentTone3.Border.Static");
        p.AssignMapping("Base/Panels/Tone0 Background (Static)", "ABrush.Tone0.Background.Static");
        p.AssignMapping("Base/Panels/Tone0 Border (Static)", "ABrush.Tone0.Border.Static");
        p.AssignMapping("Base/Panels/Tone1 Background (Static)", "ABrush.Tone1.Background.Static");
        p.AssignMapping("Base/Panels/Tone1 Border (Static)", "ABrush.Tone1.Border.Static");
        p.AssignMapping("Base/Panels/Tone2 Background (Static)", "ABrush.Tone2.Background.Static");
        p.AssignMapping("Base/Panels/Tone2 Border (Static)", "ABrush.Tone2.Border.Static");
        p.AssignMapping("Base/Panels/Tone3 Background (Static)", "ABrush.Tone3.Background.Static");
        p.AssignMapping("Base/Panels/Tone3 Border (Static)", "ABrush.Tone3.Border.Static");
        p.AssignMapping("Base/Panels/Tone4 Background (Static)", "ABrush.Tone4.Background.Static");
        p.AssignMapping("Base/Panels/Tone4 Border (Static)", "ABrush.Tone4.Border.Static");
        p.AssignMapping("Base/Panels/Tone4 Background (MouseOver)", "ABrush.Tone4.Background.MouseOver");
        p.AssignMapping("Base/Panels/Tone4 Border (MouseOver)", "ABrush.Tone4.Border.MouseOver");
        p.AssignMapping("Base/Panels/Tone4 Background (MouseDown)", "ABrush.Tone4.Background.MouseDown");
        p.AssignMapping("Base/Panels/Tone4 Border (MouseDown)", "ABrush.Tone4.Border.MouseDown");
        p.AssignMapping("Base/Panels/Tone4 Background (Selected)", "ABrush.Tone4.Background.Selected");
        p.AssignMapping("Base/Panels/Tone4 Background (Selected Inactive)", "ABrush.Tone4.Background.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone4 Border (Selected)", "ABrush.Tone4.Border.Selected");
        p.AssignMapping("Base/Panels/Tone4 Border (Selected Inactive)", "ABrush.Tone4.Border.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone4 Background (Disabled)", "ABrush.Tone4.Background.Disabled");
        p.AssignMapping("Base/Panels/Tone4 Border (Disabled)", "ABrush.Tone4.Border.Disabled");
        p.AssignMapping("Base/Panels/Tone5 Background (Static)", "ABrush.Tone5.Background.Static");
        p.AssignMapping("Base/Panels/Tone5 Border (Static)", "ABrush.Tone5.Border.Static");
        p.AssignMapping("Base/Panels/Tone5 Background (MouseOver)", "ABrush.Tone5.Background.MouseOver");
        p.AssignMapping("Base/Panels/Tone5 Border (MouseOver)", "ABrush.Tone5.Border.MouseOver");
        p.AssignMapping("Base/Panels/Tone5 Background (MouseDown)", "ABrush.Tone5.Background.MouseDown");
        p.AssignMapping("Base/Panels/Tone5 Border (MouseDown)", "ABrush.Tone5.Border.MouseDown");
        p.AssignMapping("Base/Panels/Tone5 Background (Selected)", "ABrush.Tone5.Background.Selected");
        p.AssignMapping("Base/Panels/Tone5 Background (Selected Inactive)", "ABrush.Tone5.Background.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone5 Border (Selected)", "ABrush.Tone5.Border.Selected");
        p.AssignMapping("Base/Panels/Tone5 Border (Selected Inactive)", "ABrush.Tone5.Border.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone5 Background (Disabled)", "ABrush.Tone5.Background.Disabled");
        p.AssignMapping("Base/Panels/Tone5 Border (Disabled)", "ABrush.Tone5.Border.Disabled");
        p.AssignMapping("Base/Panels/Tone6 Background (Static)", "ABrush.Tone6.Background.Static");
        p.AssignMapping("Base/Panels/Tone6 Border (Static)", "ABrush.Tone6.Border.Static");
        p.AssignMapping("Base/Panels/Tone6 Background (MouseOver)", "ABrush.Tone6.Background.MouseOver");
        p.AssignMapping("Base/Panels/Tone6 Border (MouseOver)", "ABrush.Tone6.Border.MouseOver");
        p.AssignMapping("Base/Panels/Tone6 Background (MouseDown)", "ABrush.Tone6.Background.MouseDown");
        p.AssignMapping("Base/Panels/Tone6 Border (MouseDown)", "ABrush.Tone6.Border.MouseDown");
        p.AssignMapping("Base/Panels/Tone6 Background (Selected)", "ABrush.Tone6.Background.Selected");
        p.AssignMapping("Base/Panels/Tone6 Background (Selected Inactive)", "ABrush.Tone6.Background.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone6 Border (Selected)", "ABrush.Tone6.Border.Selected");
        p.AssignMapping("Base/Panels/Tone6 Border (Selected Inactive)", "ABrush.Tone6.Border.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone6 Background (Disabled)", "ABrush.Tone6.Background.Disabled");
        p.AssignMapping("Base/Panels/Tone6 Border (Disabled)", "ABrush.Tone6.Border.Disabled");
        p.AssignMapping("Base/Panels/Tone7 Background (Static)", "ABrush.Tone7.Background.Static");
        p.AssignMapping("Base/Panels/Tone7 Border (Static)", "ABrush.Tone7.Border.Static");
        p.AssignMapping("Base/Panels/Tone7 Background (MouseOver)", "ABrush.Tone7.Background.MouseOver");
        p.AssignMapping("Base/Panels/Tone7 Border (MouseOver)", "ABrush.Tone7.Border.MouseOver");
        p.AssignMapping("Base/Panels/Tone7 Background (MouseDown)", "ABrush.Tone7.Background.MouseDown");
        p.AssignMapping("Base/Panels/Tone7 Border (MouseDown)", "ABrush.Tone7.Border.MouseDown");
        p.AssignMapping("Base/Panels/Tone7 Background (Selected)", "ABrush.Tone7.Background.Selected");
        p.AssignMapping("Base/Panels/Tone7 Background (Selected Inactive)", "ABrush.Tone7.Background.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone7 Border (Selected)", "ABrush.Tone7.Border.Selected");
        p.AssignMapping("Base/Panels/Tone7 Border (Selected Inactive)", "ABrush.Tone7.Border.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone7 Background (Disabled)", "ABrush.Tone7.Background.Disabled");
        p.AssignMapping("Base/Panels/Tone7 Border (Disabled)", "ABrush.Tone7.Border.Disabled");
        p.AssignMapping("Base/Panels/Tone8 Background (Static)", "ABrush.Tone8.Background.Static");
        p.AssignMapping("Base/Panels/Tone8 Border (Static)", "ABrush.Tone8.Border.Static");
        p.AssignMapping("Base/Panels/Tone8 Background (MouseOver)", "ABrush.Tone8.Background.MouseOver");
        p.AssignMapping("Base/Panels/Tone8 Border (MouseOver)", "ABrush.Tone8.Border.MouseOver");
        p.AssignMapping("Base/Panels/Tone8 Background (MouseDown)", "ABrush.Tone8.Background.MouseDown");
        p.AssignMapping("Base/Panels/Tone8 Border (MouseDown)", "ABrush.Tone8.Border.MouseDown");
        p.AssignMapping("Base/Panels/Tone8 Background (Selected)", "ABrush.Tone8.Background.Selected");
        p.AssignMapping("Base/Panels/Tone8 Background (Selected Inactive)", "ABrush.Tone8.Background.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone8 Border (Selected)", "ABrush.Tone8.Border.Selected");
        p.AssignMapping("Base/Panels/Tone8 Border (Selected Inactive)", "ABrush.Tone8.Border.Selected.Inactive");
        p.AssignMapping("Base/Panels/Tone8 Background (Disabled)", "ABrush.Tone8.Background.Disabled");
        p.AssignMapping("Base/Panels/Tone8 Border (Disabled)", "ABrush.Tone8.Border.Disabled");
    }

    /// <summary>
    /// Sets the current application theme
    /// </summary>
    /// <param name="theme">The theme to be set as the active theme</param>
    public abstract void SetTheme(Theme theme);

    /// <summary>
    /// Gets a theme by its name. Theme names are case-insensitive (compared with <see cref="StringComparison.OrdinalIgnoreCase"/>)
    /// </summary>
    /// <param name="name">The theme name</param>
    /// <returns>The found theme, or null</returns>
    public Theme? GetTheme(string name) {
        return this.Themes.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates a new theme that is based on the given theme
    /// </summary>
    /// <param name="name">The new name of the theme</param>
    /// <param name="basedOn">A theme whose colours are copied into the new one</param>
    /// <param name="copyAllKeys">True to copy all keys to create effectively a complete clone</param>
    /// <returns>The new theme</returns>
    /// <exception cref="ArgumentException">Theme name is null or whitespaces</exception>
    /// <exception cref="ArgumentNullException">Based on theme is null</exception>
    /// <exception cref="InvalidOperationException">Theme already exists with the name</exception>
    public abstract Theme RegisterTheme(string name, Theme basedOn, bool copyAllKeys = false);

    /// <summary>
    /// Returns a collection of themes that are built in themes. See <see cref="Theme.IsBuiltIn"/> for more info.
    /// This is just a convenience method for iterating <see cref="Themes"/> and filtering built in ones
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<Theme> GetBuiltInThemes() => this.Themes.Where(x => x.IsBuiltIn);

    /// <summary>
    /// Raises the <see cref="ActiveThemeChanged"/> event
    /// </summary>
    protected void RaiseActiveThemeChanged(Theme oldTheme, Theme newTheme) {
        ArgumentNullException.ThrowIfNull(oldTheme);
        ArgumentNullException.ThrowIfNull(newTheme);
        this.ActiveThemeChanged?.Invoke(this, oldTheme, newTheme);
    } 
}
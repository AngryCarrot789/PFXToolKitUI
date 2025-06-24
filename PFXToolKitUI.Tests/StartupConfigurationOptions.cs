// 
// Copyright (c) 2025 REghZy
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

using PFXToolKitUI.Persistence;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Tests;

public class StartupConfigurationOptions : PersistentConfiguration {
    public static StartupConfigurationOptions Instance => ApplicationPFX.Instance.PersistentStorageManager.GetConfiguration<StartupConfigurationOptions>();

    public static readonly PersistentProperty<string> StartupThemeProperty = PersistentProperty.RegisterString<StartupConfigurationOptions>(nameof(StartupTheme), "Dark", x => x.startupTheme ?? "", (x, y) => x.startupTheme = y, true);

    private string? startupTheme;

    public string StartupTheme {
        get => StartupThemeProperty.GetValue(this);
        set => StartupThemeProperty.SetValue(this, value);
    }

    static StartupConfigurationOptions() {
        StartupThemeProperty.DescriptionLines.Add("The theme the application loads with by default. If the theme does not exist at startup, the default theme is used");
    }

    public void ApplyTheme() {
        if (!string.IsNullOrWhiteSpace(this.startupTheme)) {
            if (ThemeManager.Instance.GetTheme(this.startupTheme) is Theme theme) {
                ThemeManager.Instance.SetTheme(theme);
            }
        }
    }

    public void SetStartupThemeAsCurrent() {
        this.StartupTheme = ThemeManager.Instance.ActiveTheme.Name;
    }
}
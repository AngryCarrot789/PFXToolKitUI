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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Utils.Collections.Observable;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes;

public class ThemeManagerImpl : ThemeManager {
    private readonly ObservableList<Theme> themes;
    private readonly List<ThemeImpl> builtInThemes = new List<ThemeImpl>(2);
    private Theme myActiveTheme = null!;

    public Application App { get; }

    public ResourceDictionary ApplicationResources => (ResourceDictionary) this.App.Resources;

    public IDictionary<ThemeVariant, IThemeVariantProvider> ThemeDictionaries => this.ApplicationResources.ThemeDictionaries;

    public override ReadOnlyObservableList<Theme> Themes { get; }

    public override Theme ActiveTheme => this.myActiveTheme ?? throw new InvalidOperationException("ActiveTheme property used too early. Must be used after app is setup");

    public ThemeManagerImpl(Application app) {
        this.App = app;
        this.App.ActualThemeVariantChanged += this.OnActiveVariantChanged;
        this.themes = new ObservableList<Theme>();
        this.Themes = new ReadOnlyObservableList<Theme>(this.themes);
    }

    private void OnActiveVariantChanged(object? sender, EventArgs e) {
        Theme? oldTheme = this.myActiveTheme;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (oldTheme == null) {
            AppLogger.Instance.WriteLine("WARNING: The current active theme is null. App ThemeVariant changed before app is fully setup?");
            Debugger.Break();
        }

        this.myActiveTheme = this.GetThemeByVariant(this.App.ActualThemeVariant) ?? throw new Exception("Active theme variant is invalid");

        if (oldTheme != null) {
            this.RaiseActiveThemeChanged(oldTheme, this.myActiveTheme);
        }
    }

    public Theme? GetThemeByVariant(ThemeVariant variant) {
        foreach (Theme theme in this.themes) {
            if (((ThemeImpl) theme).Variant == variant) {
                return theme;
            }
        }

        return null;
    }

    public void SetupBuiltInThemes() {
        ApplicationPFX.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);

        foreach (KeyValuePair<ThemeVariant, IThemeVariantProvider> entry in this.ThemeDictionaries) {
            string? themeName = entry.Key.Key.ToString();
            if (string.IsNullOrWhiteSpace(themeName)) {
                continue;
            }

            if (!(entry.Value is ResourceDictionary dictionary)) {
                continue;
            }

            ThemeImpl theme = new ThemeImpl(this, themeName, dictionary, null);
            theme.LoadKeysFromDictionary();
            this.themes.Add(theme);
            this.builtInThemes.Add(theme);
        }

        SetupGeneralMappingsAndInheritance(this);
        this.myActiveTheme = this.GetThemeByVariant(this.App.ActualThemeVariant) ?? throw new Exception("Active theme variant is invalid. Are any themes defined in App.axaml?");
    }

    public override void SetTheme(Theme theme) {
        if (!(theme is ThemeImpl impl) || !this.themes.Contains(impl))
            throw new InvalidOperationException("Theme is not registered");

        this.App.RequestedThemeVariant = impl.Variant;
    }

    public override Theme RegisterTheme(string name, Theme basedOn, bool copyAllKeys = false) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(basedOn);
        if (this.GetTheme(name) != null) {
            throw new InvalidOperationException($"Theme already exists with the name '{name}'");
        }

        ThemeImpl newTheme = new ThemeImpl(this, name, new ResourceDictionary(), (ThemeImpl) basedOn);
        if (copyAllKeys) {
            newTheme.CopyKeysFrom(newTheme.BasedOn!);
        }

        this.themes.Add(newTheme);
        this.ThemeDictionaries[newTheme.Variant] = newTheme.Resources;
        return newTheme;
    }

    public override IEnumerable<Theme> GetBuiltInThemes() => this.builtInThemes;

    public static bool TryFindBrushInApplicationResources(string themeKey, [NotNullWhen(true)] out IBrush? brush) {
        if (themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey = themeKey.Substring(0, themeKey.Length - ThemeImpl.ColourSuffix.Length);
        }

        if (Application.Current!.TryGetResource(themeKey, Application.Current.ActualThemeVariant, out object? value)) {
            return (brush = value as IBrush) != null;
        }

        brush = null;
        return false;
    }

    public static bool TryFindColourInApplicationResources(string themeKey, out Color colour) {
        if (!themeKey.EndsWith(ThemeImpl.ColourSuffix)) {
            themeKey += ThemeImpl.ColourSuffix;
        }

        if (Application.Current!.TryGetResource(themeKey, Application.Current.ActualThemeVariant, out object? value)) {
            if (value is Color c) {
                colour = c;
                return true;
            }
        }

        colour = default;
        return false;
    }

    private static void SetupGeneralMappingsAndInheritance(ThemeManagerImpl manager) {
        // This method sets up the mappings and inheritance for ControlColours.axaml
        List<(string, string, string?)> items = 
        [ // "Full Mapping Path",                                            "ThemeKey",                                         "InheritFrom"
            ("General/PanelBorderBrush",                                     "PanelBorderBrush",                                 "ABrush.Tone1.Border.Static"),
            ("General/PanelBackground0",                                     "PanelBackground0",                                 "ABrush.Tone0.Background.Static"),
            ("General/PanelBackground1",                                     "PanelBackground1",                                 "ABrush.Tone1.Background.Static"),
            ("General/PanelBackground2",                                     "PanelBackground2",                                 "ABrush.Tone2.Background.Static"),
            ("General/PanelBackground3",                                     "PanelBackground3",                                 "ABrush.Tone3.Background.Static"),
            ("General/PanelBackground4",                                     "PanelBackground4",                                 "ABrush.Tone4.Background.Static"),
            ("General/Button/Background",                                    "Button.Static.Background",                         "ABrush.Tone5.Background.Static"),
            ("General/Button/Border",                                        "Button.Static.Border",                             "ABrush.Tone5.Border.Static"),
            ("General/Button/Background (MouseOver)",                        "Button.MouseOver.Background",                      "ABrush.Tone5.Background.MouseOver"),
            ("General/Button/Border (MouseOver)",                            "Button.MouseOver.Border",                          "ABrush.Tone5.Border.MouseOver"),
            ("General/Button/Background (Pressed)",                          "Button.Pressed.Background",                        "ABrush.Tone5.Background.MouseDown"),
            ("General/Button/Border (Pressed)",                              "Button.Pressed.Border",                            "ABrush.Tone5.Border.MouseDown"),
            ("General/Button/Background (Disabled)",                         "Button.Disabled.Background",                       "ABrush.Tone5.Background.Disabled"),
            ("General/Button/Border (Disabled)",                             "Button.Disabled.Border",                           "ABrush.Tone5.Border.Disabled"),
            ("General/Button/Foreground",                                    "Button.Static.Foreground",                         "ABrush.Foreground.Static"),
            ("General/Button/Disabled/Foreground",                           "Button.Disabled.Foreground",                       "ABrush.Foreground.Disabled"),
            ("General/Button/Background (Defaulted)",                        "Button.Defaulted.Background",                      "ABrush.ColourfulGlyph.Static"),
            ("General/Button/Border (Defaulted)",                            "Button.Defaulted.Border",                          "ABrush.ColourfulGlyph.Disabled"),
            ("General/ButtonSpinner/Border (Error)",                         "ButtonSpinner.Error.Border",                       null),
            ("General/ToggleButton/Background (Checked)",                    "ToggleButton.IsChecked.Background",                "ABrush.AccentTone2.Background.Static"),
            ("General/ToggleButton/Border (Checked)",                        "ToggleButton.IsChecked.Border",                    "ABrush.AccentTone2.Border.Static"),
            ("General/ToggleButton/Background (Checked, Pressed)",           "ToggleButton.Pressed.IsChecked.Background",        "ABrush.Tone6.Border.Selected"),
            ("General/ToggleButton/Border (Checked, Pressed)",               "ToggleButton.Pressed.IsChecked.Border",            "ABrush.Tone6.Border.Selected"),
            ("General/ToggleButton/Background (Checked, MouseOver)",         "ToggleButton.MouseOver.IsChecked.Background",      "ABrush.AccentTone1.Background.Static"),
            ("General/ToggleButton/Border (Checked, MouseOver)",             "ToggleButton.MouseOver.IsChecked.Border",          "ABrush.AccentTone1.Border.Static"),
            ("General/ComboBox/Background",                                  "ComboBox.Static.Background",                       "ABrush.Tone4.Background.Static"),
            ("General/ComboBox/Border",                                      "ComboBox.Static.Border",                           "ABrush.Tone6.Border.Static"),
            ("General/ComboBox/Foreground",                                  "ComboBox.Static.Foreground",                       "ABrush.Foreground.Static"),
            ("General/ComboBox (Editable)/Background",                       "ComboBox.Static.Editable.Background",              "ABrush.Tone4.Background.Static"),
            ("General/ComboBox (Editable)/Border",                           "ComboBox.Static.Editable.Border",                  "ABrush.Tone4.Border.Static"),
            ("General/ComboBoxButton (Editable)/Background",                 "ComboBox.Static.Editable.Button.Background",       null),
            ("General/ComboBoxButton (Editable)/Border",                     "ComboBox.Static.Editable.Button.Border",           null),
            ("General/ComboBoxButton/Background",                            "ComboBox.Button.Static.Background",                "ABrush.Tone5.Background.Static"),
            ("General/ComboBoxButton/Border",                                "ComboBox.Button.Static.Border",                    "ABrush.Tone6.Border.Static"),
            ("General/ComboBox/Glyph (MouseOver)",                           "ComboBox.MouseOver.Glyph",                         "ABrush.Glyph.MouseOver"),
            ("General/ComboBox/Background (MouseOver)",                      "ComboBox.MouseOver.Background",                    "ABrush.Tone4.Background.MouseOver"),
            ("General/ComboBox/Border (MouseOver)",                          "ComboBox.MouseOver.Border",                        "ABrush.Tone4.Border.MouseOver"),
            ("General/ComboBox (Editable)/Background (MouseOver)",           "ComboBox.MouseOver.Editable.Background",           "ABrush.Tone4.Background.MouseOver"),
            ("General/ComboBox (Editable)/Border (MouseOver)",               "ComboBox.MouseOver.Editable.Border",               "ABrush.Tone4.Border.MouseOver"),
            ("General/ComboBoxButton (Editable)/Background (MouseOver)",     "ComboBox.MouseOver.Editable.Button.Background",    null),
            ("General/ComboBoxButton (Editable)/Border (MouseOver)",         "ComboBox.MouseOver.Editable.Button.Border",        null),
            ("General/ComboBox/Glyph (Pressed)",                             "ComboBox.Pressed.Glyph",                           "ABrush.Glyph.MouseDown"),
            ("General/ComboBox/Background (Pressed)",                        "ComboBox.Pressed.Background",                      "ABrush.Tone4.Background.MouseDown"),
            ("General/ComboBox/Border (Pressed)",                            "ComboBox.Pressed.Border",                          "ABrush.Tone4.Border.MouseDown"),
            ("General/ComboBox (Editable)/Background (Pressed)",             "ComboBox.Pressed.Editable.Background",             "ABrush.Tone4.Background.MouseDown"),
            ("General/ComboBox (Editable)/Border (Pressed)",                 "ComboBox.Pressed.Editable.Border",                 "ABrush.Tone4.Border.MouseDown"),
            ("General/ComboBoxButton (Editable)/Background (Pressed)",       "ComboBox.Pressed.Editable.Button.Background",      null),
            ("General/ComboBoxButton (Editable)/Border (Pressed)",           "ComboBox.Pressed.Editable.Button.Border",          null),
            ("General/ComboBox/Glyph (Disabled)",                            "ComboBox.Disabled.Glyph",                          "ABrush.Glyph.Disabled"),
            ("General/ComboBox/Background (Disabled)",                       "ComboBox.Disabled.Background",                     "ABrush.Tone4.Background.Disabled"),
            ("General/ComboBox/Border (Disabled)",                           "ComboBox.Disabled.Border",                         "ABrush.Tone4.Border.Disabled"),
            ("General/ComboBox (Editable)/Background (Disabled)",            "ComboBox.Disabled.Editable.Background",            "ABrush.Tone4.Background.Disabled"),
            ("General/ComboBox (Editable)/Border (Disabled)",                "ComboBox.Disabled.Editable.Border",                "ABrush.Tone4.Border.Disabled"),
            ("General/ComboBoxButton (Editable)/Background (Disabled)",      "ComboBox.Disabled.Editable.Button.Background",     null),
            ("General/ComboBoxButton (Editable)/Border (Disabled)",          "ComboBox.Disabled.Editable.Button.Border",         null),
            ("General/ComboBox/Glyph",                                       "ComboBox.Static.Glyph",                            "ABrush.Glyph.Static"),
            ("General/ComboBoxItem/ItemView/Background (Hover)",             "ComboBoxItem.ItemView.Hover.Background",           "ABrush.Tone5.Background.MouseOver"),
            ("General/ComboBoxItem/ItemView/Border (Hover)",                 "ComboBoxItem.ItemView.Hover.Border",               "ABrush.Tone5.Border.MouseOver"),
            ("General/ComboBoxItem/ItemView/Background (Selected)",          "ComboBoxItem.ItemView.Selected.Background",        "ABrush.Tone5.Background.Selected"),
            ("General/ComboBoxItem/ItemView/Border (Selected)",              "ComboBoxItem.ItemView.Selected.Border",            "ABrush.Tone5.Border.Selected"),
            ("General/ComboBoxItem/ItemView/Background (Selected, Hover)",   "ComboBoxItem.ItemView.SelectedHover.Background",   "ABrush.Tone5.Background.Selected"),
            ("General/ComboBoxItem/ItemView/Border (Selected, Hover)",       "ComboBoxItem.ItemView.SelectedHover.Border",       "ABrush.Tone5.Border.Selected"),
            ("General/ComboBoxItem/ItemView/Background (Selected, NoFocus)", "ComboBoxItem.ItemView.SelectedNoFocus.Background", "ABrush.Tone5.Background.Selected.Inactive"),
            ("General/ComboBoxItem/ItemView/Border (Selected, NoFocus)",     "ComboBoxItem.ItemView.SelectedNoFocus.Border",     "ABrush.Tone5.Border.Selected.Inactive"),
            ("General/ComboBoxItem/ItemView/Border (Focused)",               "ComboBoxItem.ItemView.Focus.Border",               "ABrush.Tone5.Border.Static"),
            ("General/ComboBoxItem/ItemView/Background (Hover, Focused)",    "ComboBoxItem.ItemView.HoverFocus.Background",      "ABrush.Tone5.Background.MouseOver"),
            ("General/ComboBoxItem/ItemView/Border (Hover, Focused)",        "ComboBoxItem.ItemView.HoverFocus.Border",          "ABrush.Tone5.Border.MouseOver"),
            ("General/TextBox/Background",                                   "TextBox.Static.Background",                        "ABrush.Tone4.Background.Static"),
            ("General/TextBox/Border",                                       "TextBox.Static.Border",                            "ABrush.Tone5.Border.Static"),
            ("General/TextBox/Border (MouseOver)",                           "TextBox.MouseOver.Border",                         "ABrush.Tone5.Border.MouseOver"),
            ("General/TextBox/Border (Focused)",                             "TextBox.Focus.Border",                             "ABrush.ColourfulGlyph.Static"),
            ("General/TextBox/Selection (Inactive)",                         "TextBox.Selection.Inactive",                       "ABrush.ColourfulGlyph.Selected.Inactive"),
            ("General/TextBox/Selection",                                    "TextBox.Selection",                                "ABrush.ColourfulGlyph.Selected"),
            ("General/TextBox/Border (Error)",                               "TextBox.Error.Border",                             null),
            ("General/ListBox/Background",                                   "ListBox.Static.Background",                        "ABrush.Tone4.Background.Static"),
            ("General/ListBox/Border",                                       "ListBox.Static.Border",                            "ABrush.Tone4.Border.Static"),
            ("General/ListBox/Background (Disabled)",                        "ListBox.Disabled.Background",                      "ABrush.Tone4.Background.Disabled"),
            ("General/ListBox/Border (Disabled)",                            "ListBox.Disabled.Border",                          "ABrush.Tone4.Border.Disabled"),
            ("General/ListBoxItem/Background (MouseOver)",                   "ListBoxItem.MouseOver.Background",                 "ABrush.Tone5.Background.MouseOver"),
            ("General/ListBoxItem/Border (MouseOver)",                       "ListBoxItem.MouseOver.Border",                     "ABrush.Tone5.Border.MouseOver"),
            ("General/ListBoxItem/Background (Selected, Inactive)",          "ListBoxItem.SelectedInactive.Background",          "ABrush.Tone5.Background.Selected.Inactive"),
            ("General/ListBoxItem/Border (Selected, Inactive)",              "ListBoxItem.SelectedInactive.Border",              "ABrush.Tone5.Border.Selected.Inactive"),
            ("General/ListBoxItem/Background (Selected)",                    "ListBoxItem.SelectedActive.Background",            "ABrush.Tone5.Background.Selected"),
            ("General/ListBoxItem/Border (Selected)",                        "ListBoxItem.SelectedActive.Border",                "ABrush.Tone5.Border.Selected"),
            ("General/CheckBox/Background",                                  "CheckBox.Static.Background",                       "ABrush.Tone5.Background.Static"),
            ("General/CheckBox/Border",                                      "CheckBox.Static.Border",                           "ABrush.Tone5.Border.Static"),
            ("General/CheckBox/Background (MouseOver)",                      "CheckBox.MouseOver.Background",                    "ABrush.Tone5.Background.MouseOver"),
            ("General/CheckBox/Border (MouseOver)",                          "CheckBox.MouseOver.Border",                        "ABrush.Tone5.Border.MouseOver"),
            ("General/CheckBox/Background (Disabled)",                       "CheckBox.Disabled.Background",                     "ABrush.Tone5.Background.Disabled"),
            ("General/CheckBox/Border (Disabled)",                           "CheckBox.Disabled.Border",                         "ABrush.Tone5.Border.Disabled"),
            ("General/CheckBox/Background (Pressed)",                        "CheckBox.Pressed.Background",                      "ABrush.Tone5.Background.MouseDown"),
            ("General/CheckBox/Border (Pressed)",                            "CheckBox.Pressed.Border",                          "ABrush.Tone5.Border.MouseDown"),
            ("General/CheckBox/Glyph",                                       "CheckBox.Static.Glyph",                            "ABrush.Glyph.Static"),
            ("General/CheckBox/Glyph (Pressed)",                             "CheckBox.Pressed.Glyph",                           "ABrush.Glyph.MouseDown"),
            ("General/CheckBox/Glyph (MouseOver)",                           "CheckBox.MouseOver.Glyph",                         "ABrush.Glyph.MouseOver"),
            ("General/CheckBox/Glyph (Disabled)",                            "CheckBox.Disabled.Glyph",                          "ABrush.Glyph.Disabled"),
            ("General/RadioButton/Background",                               "RadioButton.Static.Background",                    "ABrush.Tone5.Background.Static"),
            ("General/RadioButton/Border",                                   "RadioButton.Static.Border",                        "ABrush.Tone5.Border.Static"),
            ("General/RadioButton/Background (MouseOver)",                   "RadioButton.MouseOver.Background",                 "ABrush.Tone5.Background.MouseOver"),
            ("General/RadioButton/Border (MouseOver)",                       "RadioButton.MouseOver.Border",                     "ABrush.Tone5.Border.MouseOver"),
            ("General/RadioButton/Background (Pressed)",                     "RadioButton.Pressed.Background",                   "ABrush.Tone5.Background.MouseDown"),
            ("General/RadioButton/Border (Pressed)",                         "RadioButton.Pressed.Border",                       "ABrush.Tone5.Border.MouseDown"),
            ("General/RadioButton/Background (Disabled)",                    "RadioButton.Disabled.Background",                  "ABrush.Tone5.Background.Disabled"),
            ("General/RadioButton/Border (Disabled)",                        "RadioButton.Disabled.Border",                      "ABrush.Tone5.Border.Disabled"),
            ("General/RadioButton/Glyph",                                    "RadioButton.Static.Glyph",                         "ABrush.Glyph.Static"),
            ("General/RadioButton/Glyph (MouseOver)",                        "RadioButton.MouseOver.Glyph",                      "ABrush.Glyph.MouseOver"),
            ("General/RadioButton/Glyph (Pressed)",                          "RadioButton.Pressed.Glyph",                        "ABrush.Glyph.MouseDown"),
            ("General/RadioButton/Glyph (Disabled)",                         "RadioButton.Disabled.Glyph",                       "ABrush.Glyph.Disabled"),
            ("General/ScrollBar/Background",                                 "ScrollBar.Static.Background",                      "ABrush.Tone5.Background.Static"),
            ("General/ScrollBar/Border",                                     "ScrollBar.Static.Border",                          "ABrush.Tone5.Border.Static"),
            ("General/ScrollBarButton/Background",                           "ScrollBarButton.Static.Background",                "ABrush.Tone6.Background.Static"),
            ("General/ScrollBarButton/Border",                               "ScrollBarButton.Static.Border",                    "ABrush.Tone5.Border.Static"),
            ("General/ScrollBar/Glyph (Pressed)",                            "ScrollBar.Pressed.Glyph",                          "ABrush.Glyph.MouseDown"),
            ("General/ScrollBar/Glyph (MouseOver)",                          "ScrollBar.MouseOver.Glyph",                        "ABrush.Glyph.MouseOver"),
            ("General/ScrollBar/Glyph (Disabled)",                           "ScrollBar.Disabled.Glyph",                         "ABrush.Glyph.Disabled"),
            ("General/ScrollBar/Glyph",                                      "ScrollBar.Static.Glyph",                           "ABrush.Glyph.Static"),
            ("General/ScrollBarSink/Background (MouseOver)",                 "ScrollBarSink.MouseOver.Background",               "ABrush.Tone1.Background.Static"),
            ("General/ScrollBar/Background (MouseOver)",                     "ScrollBar.MouseOver.Background",                   "ABrush.Tone5.Background.MouseOver"),
            ("General/ScrollBar/Border (MouseOver)",                         "ScrollBar.MouseOver.Border",                       "ABrush.Tone5.Border.MouseOver"),
            ("General/ScrollBar/Background (Pressed)",                       "ScrollBar.Pressed.Background",                     "ABrush.Tone5.Background.MouseDown"),
            ("General/ScrollBar/Border (Pressed)",                           "ScrollBar.Pressed.Border",                         "ABrush.Tone5.Border.MouseDown"),
            ("General/ScrollBar/Background (Disabled)",                      "ScrollBar.Disabled.Background",                    "ABrush.Tone5.Background.Disabled"),
            ("General/ScrollBar/Border (Disabled)",                          "ScrollBar.Disabled.Border",                        "ABrush.Tone5.Border.Disabled"),
            ("General/ScrollBar/Thumb (MouseOver)",                          "ScrollBar.MouseOver.Thumb",                        "ABrush.Tone7.Background.MouseOver"),
            ("General/ScrollBar/Thumb (Pressed)",                            "ScrollBar.Pressed.Thumb",                          "ABrush.Tone7.Background.MouseDown"),
            ("General/ScrollBar/Thumb",                                      "ScrollBar.Static.Thumb",                           "ABrush.Tone7.Background.Static"),
            ("General/TabItem/Background (Selected)",                        "TabItem.Selected.Background",                      "ABrush.Tone6.Background.Static"),
            ("General/TabItem/Border (Selected)",                            "TabItem.Selected.Border",                          "ABrush.Tone5.Border.Selected"),
            ("General/TabItem/Background",                                   "TabItem.Static.Background",                        "ABrush.Tone5.Background.Static"),
            ("General/TabItem/Border",                                       "TabItem.Static.Border",                            "ABrush.Tone5.Border.Static"),
            ("General/TabItem/Background (MouseOver)",                       "TabItem.MouseOver.Background",                     "ABrush.Tone5.Background.MouseOver"),
            ("General/TabItem/Border (MouseOver)",                           "TabItem.MouseOver.Border",                         "ABrush.Tone5.Border.MouseOver"),
            ("General/TabItem/Background (Disabled)",                        "TabItem.Disabled.Background",                      "ABrush.Tone5.Background.Disabled"),
            ("General/TabItem/Border (Disabled)",                            "TabItem.Disabled.Border",                          "ABrush.Tone5.Border.Disabled"),
            ("General/Expander/Circle/Fill (MouseOver)",                     "Expander.MouseOver.Circle.Fill",                   "ABrush.Tone5.Background.MouseOver"),
            ("General/Expander/Circle/Stroke (MouseOver)",                   "Expander.MouseOver.Circle.Stroke",                 "ABrush.Tone5.Border.MouseOver"),
            ("General/Expander/Circle/Fill (Pressed)",                       "Expander.Pressed.Circle.Fill",                     "ABrush.Tone5.Background.MouseDown"),
            ("General/Expander/Circle/Stroke (Pressed)",                     "Expander.Pressed.Circle.Stroke",                   "ABrush.Tone5.Border.MouseDown"),
            ("General/Expander/Circle/Fill (Disabled)",                      "Expander.Disabled.Circle.Fill",                    "ABrush.Tone5.Background.Disabled"),
            ("General/Expander/Circle/Stroke (Disabled)",                    "Expander.Disabled.Circle.Stroke",                  "ABrush.Tone5.Border.Disabled"),
            ("General/Expander/Circle/Fill",                                 "Expander.Static.Circle.Fill",                      "ABrush.Tone5.Background.Static"),
            ("General/Expander/Circle/Stroke",                               "Expander.Static.Circle.Stroke",                    "ABrush.Tone5.Border.Static"),
            ("General/Expander/Arrow/Stroke",                                "Expander.Static.Arrow.Stroke",                     "ABrush.Glyph.Static"),
            ("General/Expander/Arrow/Stroke (MouseOver)",                    "Expander.MouseOver.Arrow.Stroke",                  "ABrush.Glyph.MouseOver"),
            ("General/Expander/Arrow/Stroke (Pressed)",                      "Expander.Pressed.Arrow.Stroke",                    "ABrush.Glyph.MouseDown"),
            ("General/Expander/Arrow/Stroke (Disabled)",                     "Expander.Disabled.Arrow.Stroke",                   "ABrush.Glyph.Disabled"),
            ("General/GridSplitter/Background",                              "GridSplitter.Static.Background",                   "ABrush.Tone5.Background.Static"),
            ("General/GroupBox/Background",                                  "GroupBox.Static.Background",                       null),
            ("General/GroupBox/Border",                                      "GroupBox.Static.Border",                           "ABrush.Tone6.Border.Static"),
            ("General/GroupBox/Header/Background",                           "GroupBox.Header.Static.Background",                "ABrush.Tone5.Background.Static"),
            ("General/GroupBox/Header/Border",                               "GroupBox.Header.Static.Border",                    "ABrush.Tone6.Border.Static"),
            ("General/GroupBox/Header/Foreground",                           "GroupBox.Header.Foreground",                       "ABrush.Foreground.Static"),
            ("General/GroupBox/Header/Background (Disabled)",                "GroupBox.Header.Background.Disabled",              "ABrush.Tone5.Background.Disabled"),
            ("General/GroupBox/Header/Border (Disabled)",                    "GroupBox.Header.Static.Border.Disabled",           "ABrush.Tone6.Border.Disabled"),
            ("General/GroupBox/Header/Foreground (Disabled)",                "GroupBox.Header.Foreground.Disabled",              "ABrush.Foreground.Disabled"),
            ("General/Menu/Background",                                      "Menu.Static.Background",                           null),
            ("General/MenuItem/Background",                                  "MenuItem.Static.Background",                       "ABrush.Tone5.Background.Static"),
            ("General/MenuItem/IconBar/Background",                          "MenuItem.IconBar.Static.Background",               "ABrush.Tone6.Background.Static"),
            ("General/MenuItem/IconBar/Border",                              "MenuItem.IconBar.Static.Border",                   "ABrush.Tone6.Border.Static"),
            ("General/MenuItem/Square/Background",                           "MenuItem.Square.Static.Background",                "ABrush.Tone6.Background.Static"),
            ("General/MenuItem/Square/Border",                               "MenuItem.Square.Static.Border",                    "ABrush.Tone8.Border.Static"),
            ("General/MenuItem/Background (MouseOver)",                      "MenuItem.MouseOver.Background",                    "ABrush.AccentTone2.Background.Static"),
            ("General/MenuItem/Border (MouseOver)",                          "MenuItem.MouseOver.Border",                        "ABrush.Tone5.Border.MouseOver"),
            ("General/MenuItem/Background (Disabled)",                       "MenuItem.Disabled.Background",                     "ABrush.Tone5.Background.MouseOver"),
            ("General/Popup/Background",                                     "Popup.Static.Background",                          "ABrush.Tone5.Background.Static"),
            ("General/Popup/Border",                                         "Popup.Static.Border",                              "ABrush.Tone7.Border.Static"),
            ("General/Popup/Foreground",                                     "Popup.Static.Foreground",                          "ABrush.Foreground.Static"),
            ("General/Popup/Background (Disabled)",                          "Popup.Disabled.Background",                        "ABrush.Tone4.Background.Disabled"),
            ("General/Popup/Border (Disabled)",                              "Popup.Disabled.Border",                            "ABrush.Tone4.Border.Disabled"),
            ("General/Popup/Foreground (Disabled)",                          "Popup.Disabled.Foreground",                        "ABrush.Foreground.Disabled"),
            ("General/MenuItem/Glyph",                                       "MenuItem.Static.Glyph",                            "ABrush.Glyph.Static"),
            ("General/MenuItem/Glyph (Disabled)",                            "MenuItem.Disabled.Glyph",                          "ABrush.Glyph.Disabled"),
            ("General/SliderThumb/Background (MouseOver)",                   "SliderThumb.MouseOver.Background",                 "ABrush.Tone5.Background.MouseOver"),
            ("General/SliderThumb/Border (MouseOver)",                       "SliderThumb.MouseOver.Border",                     "ABrush.Tone5.Border.MouseOver"),
            ("General/SliderThumb/Background (Pressed)",                     "SliderThumb.Pressed.Background",                   "ABrush.Tone5.Background.MouseDown"),
            ("General/SliderThumb/Border (Pressed)",                         "SliderThumb.Pressed.Border",                       "ABrush.Tone5.Border.MouseDown"),
            ("General/SliderThumb/Background (Disabled)",                    "SliderThumb.Disabled.Background",                  "ABrush.Tone5.Background.Disabled"),
            ("General/SliderThumb/Border (Disabled)",                        "SliderThumb.Disabled.Border",                      "ABrush.Tone5.Border.Disabled"),
            ("General/SliderThumb/Background",                               "SliderThumb.Static.Background",                    "ABrush.Tone5.Background.Static"),
            ("General/SliderThumb/Border",                                   "SliderThumb.Static.Border",                        "ABrush.Tone5.Border.Static"),
            ("General/SliderThumb/Track/Background",                         "SliderThumb.Track.Background",                     "ABrush.Tone5.Background.Static"),
            ("General/SliderThumb/Track/Border",                             "SliderThumb.Track.Border",                         "ABrush.Tone5.Border.Static"),
            ("General/SliderThumb/Foreground",                               "SliderThumb.Static.Foreground",                    "ABrush.Glyph.Disabled"),
            ("General/SliderThumb/Track/Progress/Background",                "SliderThumb.Track.Progress.Background",            "ABrush.ColourfulGlyph.Static"),
            ("General/SliderThumb/Track/Progress/Border",                    "SliderThumb.Track.Progress.Border",                "ABrush.Tone5.Border.Static"),
            ("General/ProgressBar/Progress",                                 "ProgressBar.Progress",                             "ABrush.ColourfulGlyph.Static"),
            ("General/ProgressBar/Background",                               "ProgressBar.Background",                           "ABrush.Tone5.Background.Static"),
            ("General/ProgressBar/Border",                                   "ProgressBar.Border",                               "ABrush.Tone5.Border.Static"),
            ("General/TreeView/Background",                                  "TreeView.Static.Background",                       "ABrush.Tone4.Background.Static"),
            ("General/TreeView/Border",                                      "TreeView.Static.Border",                           "ABrush.Tone4.Border.Static"),
            ("General/TreeView/Background (Disabled)",                       "TreeView.Disabled.Background",                     "ABrush.Tone4.Background.Disabled"),
            ("General/TreeView/Border (Disabled)",                           "TreeView.Disabled.Border",                         "ABrush.Tone4.Border.Disabled"),
            ("General/TreeViewItem/Background",                              "TreeViewItem.Static.Background",                   null),
            ("General/TreeViewItem/Border",                                  "TreeViewItem.Static.Border",                       "ABrush.Tone5.Border.Static"),
            ("General/TreeViewItem/Background (Selected)",                   "TreeViewItem.Selected.Background",                 "ABrush.Tone7.Background.Selected"),
            ("General/TreeViewItem/Border (Selected)",                       "TreeViewItem.Selected.Border",                     "ABrush.Tone7.Border.Selected"),
            ("General/TreeViewItem/Background (Selected, Inactive)",         "TreeViewItem.Selected.Inactive.Background",        "ABrush.Tone6.Background.Selected.Inactive"),
            ("General/TreeViewItem/Border (Selected, Inactive)",             "TreeViewItem.Selected.Inactive.Border",            "ABrush.Tone6.Border.Selected.Inactive"),
            ("General/TreeViewItem/Background (MouseOver)",                  "TreeViewItem.MouseOver.Background",                "ABrush.Tone6.Background.MouseOver"),
            ("General/TreeViewItem/Border (MouseOver)",                      "TreeViewItem.MouseOver.Border",                    "ABrush.Tone6.Border.MouseOver"),
            ("General/TreeViewItem/Background (MouseDown)",                  "TreeViewItem.MouseDown.Background",                "ABrush.Tone6.Background.MouseDown"),
            ("General/TreeViewItem/Border (MouseDown)",                      "TreeViewItem.MouseDown.Border",                    "ABrush.Tone6.Border.MouseDown"),
            ("General/DataGrid/Background",                                  "DataGrid.Static.Background",                       "ABrush.Tone4.Background.Static"),
            ("General/DataGrid/Border",                                      "DataGrid.Static.Border",                           "ABrush.Tone4.Border.Static"),
            ("General/DataGrid/HorizontalSeparatorBrush",                    "DataGrid.HorizontalSeparatorBrush",                "ABrush.Tone7.Background.Static"),
            ("General/DataGrid/VerticalSeparatorBrush",                      "DataGrid.VerticalSeparatorBrush",                  "ABrush.Tone7.Background.Static"),
            ("General/DataGrid/HeaderItem/Background",                       "DataGrid.HeaderItem.Static.Background",            "ABrush.Tone6.Background.Static"),
            ("General/DataGrid/HeaderItem/Background (MouseOver)",           "DataGrid.HeaderItem.MouseOver.Background",         "ABrush.Tone6.Background.MouseOver"),
            ("General/DataGrid/HeaderItem/Background (MouseDown)",           "DataGrid.HeaderItem.MouseDown.Background",         "ABrush.Tone6.Background.MouseDown"),
            ("General/DataGrid/RowHeader/Background",                        "DataGrid.RowHeader.Static.Background",             "ABrush.Tone6.Background.Static"),
            ("General/DataGrid/Row/Background (Selected)",                   "DataGrid.Row.Selected.Background",                 "ABrush.Tone5.Background.Selected"),
            ("General/DataGrid/CellItem/Background",                         "DataGrid.CellItem.Static.Background",              "ABrush.Tone5.Background.Static"),
            ("General/DataGrid/CellItem/Background (MouseOver)",             "DataGrid.CellItem.MouseOver.Background",           "ABrush.Tone5.Background.MouseOver"),
            ("General/Window/Background",                                    "Window.Static.Background",                         "ABrush.Tone3.Background.Static"),
            ("General/Window/Border",                                        "Window.Static.Border",                             "ABrush.Tone5.Border.Static"),
            ("General/Window/Foreground",                                    "Window.Static.Foreground",                         "ABrush.Foreground.Static"),
            ("General/Window/Title/Background",                              "Window.Static.Title.Background",                   "ABrush.Tone6.Background.Static"),
        ];

        List<KeyValuePair<string, string?>> inheritMap = items.Select(x => new KeyValuePair<string, string?>(x.Item2, x.Item3)).ToList();
        foreach (ThemeImpl theme in manager.builtInThemes) {
            theme.SetInheritance(inheritMap);
        }

        ThemeConfigurationPage p = manager.ThemeConfigurationPage;
        foreach ((string path, string theme, string? inherit) item in items) {
            p.AssignMapping(item.path, item.theme);
        }
    }

    public sealed class ThemeImpl : Theme {
        public const string ColourSuffix = ".Color";

        private readonly HashSet<string> registeredBrushKeys;
        private readonly ThemeVariant? variantNotInherited;
        private List<ThemeImpl>? derivedList;

        private readonly Dictionary<string, HashSet<string>> inheritMap_inheritFrom2themeKey; // map "inheritFrom" to the keys that we need to refresh.
        private readonly Dictionary<string, string> inheritMap_themeKey2inheritFrom; // map the key to update when "inheritFrom" changes.

        // a cached version of inheritMap_inheritFrom2themeKey that is built from the very root to the top derived.
        // this is needed to figure out a full list of theme keys to update when the key's value changes
        private Dictionary<string, HashSet<string>>? cachedInheritMap_InheritFrom2dstKey;

        private Dictionary<string, HashSet<string>> GeneratedInheritanceMap {
            get {
                if (this.cachedInheritMap_InheritFrom2dstKey != null)
                    return this.cachedInheritMap_InheritFrom2dstKey;

                Dictionary<string, HashSet<string>> map = new Dictionary<string, HashSet<string>>();
                List<ThemeImpl> list = this.GetInheritance();
                for (int i = list.Count - 1; i >= 0; i--) { // iterate root base to top derived
                    foreach (KeyValuePair<string, HashSet<string>> entry in list[i].inheritMap_inheritFrom2themeKey) {
                        if (!map.TryGetValue(entry.Key, out HashSet<string>? set)) {
                            map[entry.Key] = new HashSet<string>(entry.Value);
                        }
                        else {
                            foreach (string key in entry.Value)
                                set.Add(key);
                        }
                    }
                }

                return this.cachedInheritMap_InheritFrom2dstKey = map;
            }
        }

        public override string Name { get; }

        public override ThemeImpl? BasedOn { get; }

        public ThemeVariant Variant { get; }

        public override ThemeManagerImpl ThemeManager { get; }

        public override IEnumerable<string> ThemeKeys => this.registeredBrushKeys;

        public override IEnumerable<KeyValuePair<string, string>> InheritanceEntries => this.inheritMap_themeKey2inheritFrom;

        /// <summary>
        /// Gets the live resource dictionary that contains all the theme keys for this theme
        /// </summary>
        public ResourceDictionary Resources { get; }

        public ThemeImpl(ThemeManagerImpl manager, string name, ResourceDictionary resources, ThemeImpl? basedOn) : base(basedOn == null) {
            this.Name = name;
            this.BasedOn = basedOn;
            this.ThemeManager = manager;
            this.Variant = new ThemeVariant(name, basedOn?.Variant);
            this.variantNotInherited = new ThemeVariant(name, null);
            this.Resources = resources;
            this.registeredBrushKeys = new HashSet<string>(32);
            this.inheritMap_inheritFrom2themeKey = new Dictionary<string, HashSet<string>>();
            this.inheritMap_themeKey2inheritFrom = new Dictionary<string, string>();

            if (basedOn != null)
                (basedOn.derivedList ??= []).Add(this);
        }

        private List<ThemeImpl> GetInheritance() {
            List<ThemeImpl> list = new List<ThemeImpl>(3);
            for (ThemeImpl? theme = this; theme != null; theme = theme.BasedOn)
                list.Add(theme);
            return list;
        }

        private static void GetKeys(string key, out string brushKey, out string colourKey) {
            if (key.EndsWith(ColourSuffix)) {
                colourKey = key;
                brushKey = key.Substring(0, key.Length - ColourSuffix.Length);
                AppLogger.Instance.WriteLine("Attempt to set theme colour using '.Color' suffix. This should not be done");
                Debugger.Break();
            }
            else {
                colourKey = key + ColourSuffix;
                brushKey = key;
            }
        }

        public void CopyKeysFrom(ThemeImpl theme) {
            using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);
            foreach (string key in theme.registeredBrushKeys) {
                if (theme.Resources.TryGetResource(key, null, out object? brush)) {
                    // assert brush is IBrush

                    this.registeredBrushKeys.Add(key);
                    if (theme.Resources.TryGetResource(key + ColourSuffix, null, out object? colour)) {
                        change[key] = colour;
                    }

                    change[key] = brush;
                }
            }
        }

        public override void SetThemeColour(string key, SKColor colour) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            Color avColour = new Color(colour.Alpha, colour.Red, colour.Green, colour.Blue);
            IBrush avBrush = new ImmutableSolidColorBrush(avColour);
            object avColourBoxed = avColour;

            this.registeredBrushKeys.Add(brushKey);
            using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);
            change[colourKey] = avColourBoxed;
            change[brushKey] = avBrush;
            this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, avColourBoxed, brushKey, avBrush);
        }

        public override void SetThemeBrush(string key, IColourBrush brush) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            this.registeredBrushKeys.Add(brushKey);

            using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);

            if (brush is ConstantAvaloniaColourBrush i) {
                object boxedColour = i.Brush.Color;
                change[colourKey] = boxedColour;
                change[brushKey] = i.Brush;
                this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, boxedColour, brushKey, i.Brush);
            }
            else if (brush is AvaloniaColourBrush bruh) {
                if (bruh.Brush is ISolidColorBrush colourBrush) {
                    object boxedColour = colourBrush.Color;
                    change[colourKey] = boxedColour;
                    this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, boxedColour, null, null);
                }
                else {
                    AppLogger.Instance.WriteLine($"Could not update colour value for theme key '{key}', because the brush was not solid");
                }

                change[brushKey] = bruh.Brush;
                this.UpdateResourcesFromLatestInheritanceMap(change, null, null, brushKey, bruh.Brush);
            }
        }

        public void SetBrushInternal(string themeKey, IImmutableBrush brush) {
            GetKeys(themeKey, out string brushKey, out string colourKey);

            using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);

            this.registeredBrushKeys.Add(brushKey);
            if (brush is ISolidColorBrush scb) {
                object boxedColour = scb.Color;
                change[colourKey] = boxedColour;
                change[brushKey] = scb;
                this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, boxedColour, brushKey, scb);
            }
            else {
                change[brushKey] = brush;
                this.UpdateResourcesFromLatestInheritanceMap(change, null, null, brushKey, brush);
            }
        }

        public bool TryFindBrushInHierarchy(string themeKey, [NotNullWhen(true)] out IBrush? brush) {
            if (themeKey.EndsWith(ColourSuffix)) {
                themeKey = themeKey.Substring(0, themeKey.Length - ColourSuffix.Length);
            }

            return this.TryFindObjectInHierarchy(themeKey, out brush);
        }

        public bool TryFindColourInHierarchy(string themeKey, out Color colour) {
            if (!themeKey.EndsWith(ColourSuffix)) {
                themeKey += ColourSuffix;
            }

            return this.TryFindObjectInHierarchy(themeKey, out colour);
        }

        public bool TryFindObjectInHierarchy<T>(string themeKey, [MaybeNullWhen(false)] out T val, bool canSearchHierarchy = true) {
            for (ThemeImpl? theme = this; canSearchHierarchy && theme != null; theme = theme.BasedOn) {
                // We create a new ThemeVariant without a parent because we don't need to scan theme dictionaries
                if (theme.Resources.TryGetResource(themeKey, theme.variantNotInherited, out object? value)) {
                    if (value is T t) {
                        val = t;
                        return true;
                    }
                }
            }

            val = default;
            return false;
        }

        public override bool IsThemeKeyValid(string themeKey, bool allowInherited = true) {
            return this.TryFindObjectInHierarchy<object>(themeKey, out _, allowInherited);
        }

        private void UpdateResourcesFromLatestInheritanceMap(AvUtils.RDMultiChange change, string? colourKey, object? colour, string? brushKey, IBrush? brush) {
            HashSet<string>? dstKeys;
            if (colourKey != null) {
                if (this.GeneratedInheritanceMap.TryGetValue(colourKey, out dstKeys)) {
                    foreach (string dstKey in dstKeys) {
                        if (colour != null)
                            change[dstKey] = colour;
                        else
                            change.Remove(dstKey);
                    }
                }
            }

            if (brushKey != null && this.GeneratedInheritanceMap.TryGetValue(brushKey, out dstKeys)) {
                foreach (string dstKey in dstKeys) {
                    if (brush != null)
                        change[dstKey] = brush;
                    else
                        change.Remove(dstKey);
                }
            }
        }

        public override void SetInheritance(IEnumerable<KeyValuePair<string, string?>> entries) {
            List<KeyValuePair<string, string?>> entryList = entries as List<KeyValuePair<string, string?>> ?? entries.ToList();
            if (entryList.Count < 1) {
                return;
            }

            this.InvalidateDerivedInheritMaps(true);
            using (AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources)) {
                foreach (KeyValuePair<string, string?> entry in entryList) {
                    string themeKey = entry.Key;
                    string? inheritFromKey = entry.Value;
                    if (string.IsNullOrWhiteSpace(inheritFromKey)) {
                        inheritFromKey = null;
                    }

                    GetKeys(themeKey, out string dstBrushKey, out string dstColourKey);
                    if (inheritFromKey == null) {
                        if (this.inheritMap_themeKey2inheritFrom.Remove(dstBrushKey, out string? oldInheritFrom)) {
                            GetKeys(oldInheritFrom, out string inheritedBrushKey, out string inheritedColourKey);
                            if (this.inheritMap_inheritFrom2themeKey.TryGetValue(inheritedBrushKey, out HashSet<string>? set)) {
                                set.Remove(dstBrushKey);
                            }

                            // Remove local entries of the resources
                            change.Remove(inheritedBrushKey);
                            change.Remove(inheritedColourKey);
                        }
                    }
                    else {
                        GetKeys(inheritFromKey, out string inheritedBrushKey, out string inheritedColourKey);
                        if (!this.inheritMap_inheritFrom2themeKey.TryGetValue(inheritedBrushKey, out HashSet<string>? set))
                            this.inheritMap_inheritFrom2themeKey[inheritedBrushKey] = set = [];
                        set.Add(dstBrushKey);
                        this.inheritMap_themeKey2inheritFrom.TryAdd(dstBrushKey, inheritedBrushKey);
                    }
                }
            }

            using (AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources)) {
                foreach (KeyValuePair<string, string?> entry in entryList) {
                    string themeKey = entry.Key;
                    string? inheritFromKey = entry.Value;
                    if (string.IsNullOrWhiteSpace(inheritFromKey)) {
                        inheritFromKey = null;
                    }

                    GetKeys(themeKey, out string dstBrushKey, out string dstColourKey);
                    if (inheritFromKey == null) {
                        this.TryFindBrushInHierarchy(dstBrushKey, out IBrush? foundBrush);
                        object? colour = this.TryFindColourInHierarchy(dstColourKey, out Color foundColour) ? foundColour : null;
                        this.UpdateResourcesFromLatestInheritanceMap(change, dstColourKey, colour, dstBrushKey, foundBrush);
                    }
                    else {
                        GetKeys(inheritFromKey, out string inheritedBrushKey, out string inheritedColourKey);
                        this.TryFindBrushInHierarchy(inheritedBrushKey, out IBrush? foundBrush);
                        object? colour = this.TryFindColourInHierarchy(inheritedColourKey, out Color foundColour) ? foundColour : null;
                        if (foundBrush != null)
                            change[dstBrushKey] = foundBrush;
                        if (colour != null)
                            change[dstColourKey] = colour;
                        this.UpdateResourcesFromLatestInheritanceMap(change, dstColourKey, colour, dstBrushKey, foundBrush);
                    }
                }
            }
        }

        public override void SetInheritance(string themeKey, string? inheritFromKey) {
            GetKeys(themeKey, out string brushKey, out string colourKey);
            if (inheritFromKey == null) {
                if (this.inheritMap_themeKey2inheritFrom.Remove(brushKey, out string? oldInheritFrom)) {
                    GetKeys(oldInheritFrom, out string inheritedBrushKey, out string inheritedColourKey);
                    if (this.inheritMap_inheritFrom2themeKey.TryGetValue(inheritedBrushKey, out HashSet<string>? set)) {
                        set.Remove(brushKey);
                    }

                    // Remove local entries of the resources
                    using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);
                    change.Remove(inheritedBrushKey);
                    change.Remove(inheritedColourKey);
                    this.InvalidateDerivedInheritMaps(true);

                    // Evaluate new values and update
                    this.TryFindBrushInHierarchy(brushKey, out IBrush? foundBrush);
                    object? colour = this.TryFindColourInHierarchy(colourKey, out Color foundColour) ? foundColour : null;
                    this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, colour, brushKey, foundBrush);
                }
            }
            else {
                GetKeys(inheritFromKey, out string inheritedBrushKey, out string inheritedColourKey);
                if (!this.inheritMap_inheritFrom2themeKey.TryGetValue(inheritedBrushKey, out HashSet<string>? set))
                    this.inheritMap_inheritFrom2themeKey[inheritedBrushKey] = set = [];
                set.Add(brushKey);

                this.inheritMap_themeKey2inheritFrom.TryAdd(brushKey, inheritedBrushKey);
                this.TryFindBrushInHierarchy(inheritedBrushKey, out IBrush? foundBrush);
                object? colour = this.TryFindColourInHierarchy(inheritedColourKey, out Color foundColour) ? foundColour : null;
                using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);
                if (foundBrush != null || colour != null) {
                    if (foundBrush != null)
                        change[brushKey] = foundBrush;
                    if (colour != null)
                        change[colourKey] = colour;
                }

                this.InvalidateDerivedInheritMaps(true);
                this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, colour, brushKey, foundBrush);
            }
        }

        private void InvalidateDerivedInheritMaps(bool self = true) {
            if (self) {
                this.cachedInheritMap_InheritFrom2dstKey = null;
            }

            if (this.derivedList != null) {
                foreach (ThemeImpl theme in this.derivedList) {
                    theme.InvalidateDerivedInheritMaps(true);
                }
            }
        }

        public override string? GetInheritedBy(string key, out int depth) {
            (string?, int) result = this.GetInheritedByImpl(key, 0);
            depth = result.Item2;
            return result.Item1;
        }

        public (string?, int) GetInheritedByImpl(string key, int depth) {
            if (this.inheritMap_themeKey2inheritFrom.TryGetValue(key, out string? inheritedKey)) {
                return (inheritedKey, depth);
            }

            return this.BasedOn?.GetInheritedByImpl(key, depth + 1) ?? (null, -1);
        }

        public override void CollectKeysInheritedBy(string inheritFrom, HashSet<string> keys, bool includeBasedOnThemes = true) {
            if (this.inheritMap_inheritFrom2themeKey.TryGetValue(inheritFrom, out HashSet<string>? set)) {
                foreach (string key in set) {
                    keys.Add(key);
                }
            }

            if (includeBasedOnThemes) {
                this.BasedOn?.CollectKeysInheritedBy(inheritFrom, keys);
            }
        }

        public override ISavedThemeEntry SaveThemeEntry(string themeKey) {
            this.TryFindBrushInHierarchy(themeKey, out IBrush? brush);
            Color? col = this.TryFindColourInHierarchy(themeKey, out Color colour) ? colour : null;
            return new SavedThemeEntryImpl(this, brush?.ToImmutable(), col);
        }

        public override void ApplyThemeEntry(string themeKey, ISavedThemeEntry entry) {
            SavedThemeEntryImpl impl = (SavedThemeEntryImpl) entry;
            GetKeys(themeKey, out string brushKey, out string colourKey);

            using AvUtils.RDMultiChange change = AvUtils.BeginMultiChange(this.Resources);
            if (impl.Colour.HasValue) {
                object obj = impl.Colour.Value;
                change[colourKey] = obj;
                this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, obj, null, null);
            }

            if (impl.Brush != null) {
                change[brushKey] = impl.Brush;
                this.registeredBrushKeys.Add(brushKey);
                this.UpdateResourcesFromLatestInheritanceMap(change, null, null, brushKey, impl.Brush);
            }
        }

        private class SavedThemeEntryImpl : ISavedThemeEntry {
            public Theme Theme { get; }

            public IImmutableBrush? Brush { get; }

            public Color? Colour { get; }

            public SavedThemeEntryImpl(ThemeImpl theme, IImmutableBrush? brush, Color? colour) {
                this.Theme = theme;
                this.Brush = brush;
                this.Colour = colour;
            }
        }

        public void LoadKeysFromDictionary() {
            ApplicationPFX.Instance.EnsureBeforePhase(ApplicationStartupPhase.Running);
            foreach (object key in this.Resources.Keys) {
                if (key is string keyString) {
                    // Only load brush keys. We do colours along the side
                    if (!keyString.EndsWith(ColourSuffix)) {
                        this.registeredBrushKeys.Add(keyString);
                    }
                }
            }
        }
    }
}
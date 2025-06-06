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

using PFXToolKitUI.Themes.Configurations;
using PFXToolKitUI.Utils.Collections.Observable;

namespace PFXToolKitUI.Themes;

public abstract class ThemeManager {
    public static ThemeManager Instance => ApplicationPFX.Instance.ServiceManager.GetService<ThemeManager>();

    /// <summary>
    /// Gets the themes that currently exist
    /// </summary>
    public abstract ReadOnlyObservableList<Theme> Themes { get; }

    /// <summary>
    /// Gets the theme that is currently active
    /// </summary>
    public abstract Theme ActiveTheme { get; }

    public ThemeConfigurationPage ThemeConfigurationPage { get; }

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
        
        p.AssignMapping("Controls/PanelBorderBrush",                                  "PanelBorderBrush",                                       "");
        p.AssignMapping("Controls/PanelBackground0",                                  "PanelBackground0",                                       "");
        p.AssignMapping("Controls/PanelBackground1",                                  "PanelBackground1",                                       "");
        p.AssignMapping("Controls/PanelBackground2",                                  "PanelBackground2",                                       "");
        p.AssignMapping("Controls/PanelBackground3",                                  "PanelBackground3",                                       "");
        p.AssignMapping("Controls/PanelBackground4",                                  "PanelBackground4",                                       "");
        p.AssignMapping("Controls/Button/Background",                                 "Button.Static.Background",                               "");
        p.AssignMapping("Controls/Button/Border",                                     "Button.Static.Border",                                   "");
        p.AssignMapping("Controls/Button/Background (MouseOver)",                     "Button.MouseOver.Background",                            "");
        p.AssignMapping("Controls/Button/Border (MouseOver)",                         "Button.MouseOver.Border",                                "");
        p.AssignMapping("Controls/Button/Background (Pressed)",                       "Button.Pressed.Background",                              "");
        p.AssignMapping("Controls/Button/Border (Pressed)",                           "Button.Pressed.Border",                                  "");
        p.AssignMapping("Controls/Button/Background (Disabled)",                      "Button.Disabled.Background",                             "");
        p.AssignMapping("Controls/Button/Border (Disabled)",                          "Button.Disabled.Border",                                 "");
        p.AssignMapping("Controls/Button/Foreground",                                 "Button.Static.Foreground",                               "");
        p.AssignMapping("Controls/Button/Foreground (Disabled)",                      "Button.Disabled.Foreground",                             "");
        p.AssignMapping("Controls/Button/Background (Defaulted)",                     "Button.Defaulted.Background",                            "");
        p.AssignMapping("Controls/Button/Border (Defaulted)",                         "Button.Defaulted.Border",                                "");
        p.AssignMapping("Controls/ButtonSpinner/Border (Error)",                      "ButtonSpinner.Error.Border",                             "");
        p.AssignMapping("Controls/ToggleButton/Background (IsChecked)",               "ToggleButton.IsChecked.Background",                      "");
        p.AssignMapping("Controls/ToggleButton/Border (IsChecked)",                   "ToggleButton.IsChecked.Border",                          "");
        p.AssignMapping("Controls/ToggleButton/Background (Pressed + IsChecked)",     "ToggleButton.Pressed.IsChecked.Background",              "");
        p.AssignMapping("Controls/ToggleButton/Border (Pressed + IsChecked)",         "ToggleButton.Pressed.IsChecked.Border",                  "");
        p.AssignMapping("Controls/ToggleButton/Background (MouseOver + IsChecked)",   "ToggleButton.MouseOver.IsChecked.Background",            "");
        p.AssignMapping("Controls/ToggleButton/Border (MouseOver + IsChecked)",       "ToggleButton.MouseOver.IsChecked.Border",                "");
        p.AssignMapping("Controls/ComboBox/Background",                               "ComboBox.Static.Background",                             "");
        p.AssignMapping("Controls/ComboBox/Border",                                   "ComboBox.Static.Border",                                 "");
        p.AssignMapping("Controls/ComboBox/Foreground",                               "ComboBox.Static.Foreground",                             "");
        p.AssignMapping("Controls/ComboBox/Background (Editable)",                    "ComboBox.Static.Editable.Background",                    "");
        p.AssignMapping("Controls/ComboBox/Border (Editable)",                        "ComboBox.Static.Editable.Border",                        "");
        p.AssignMapping("Controls/ComboBox/Button Background (Editable)",             "ComboBox.Static.Editable.Button.Background",             "");
        p.AssignMapping("Controls/ComboBox/Button Border (Editable)",                 "ComboBox.Static.Editable.Button.Border",                 "");
        p.AssignMapping("Controls/ComboBox/Button Background",                        "ComboBox.Button.Static.Background",                      "");
        p.AssignMapping("Controls/ComboBox/Button Border",                            "ComboBox.Button.Static.Border",                          "");
        p.AssignMapping("Controls/ComboBox/Glyph (MouseOver)",                        "ComboBox.MouseOver.Glyph",                               "");
        p.AssignMapping("Controls/ComboBox/Background (MouseOver)",                   "ComboBox.MouseOver.Background",                          "");
        p.AssignMapping("Controls/ComboBox/Border (MouseOver)",                       "ComboBox.MouseOver.Border",                              "");
        p.AssignMapping("Controls/ComboBox/(Editable) Background (MouseOver)",        "ComboBox.MouseOver.Editable.Background",                 "");
        p.AssignMapping("Controls/ComboBox/(Editable) Border",                        "ComboBox.MouseOver.Editable.Border",                     "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Background (MouseOver)", "ComboBox.MouseOver.Editable.Button.Background",          "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Border (MouseOver)",     "ComboBox.MouseOver.Editable.Button.Border",              "");
        p.AssignMapping("Controls/ComboBox/Glyph (Pressed)",                          "ComboBox.Pressed.Glyph",                                 "");
        p.AssignMapping("Controls/ComboBox/Background (Pressed)",                     "ComboBox.Pressed.Background",                            "");
        p.AssignMapping("Controls/ComboBox/Border (Pressed)",                         "ComboBox.Pressed.Border",                                "");
        p.AssignMapping("Controls/ComboBox/(Editable) Background (Pressed)",          "ComboBox.Pressed.Editable.Background",                   "");
        p.AssignMapping("Controls/ComboBox/(Editable) Border (Pressed)",              "ComboBox.Pressed.Editable.Border",                       "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Background (Pressed)",   "ComboBox.Pressed.Editable.Button.Background",            "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Border (Pressed)",       "ComboBox.Pressed.Editable.Button.Border",                "");
        p.AssignMapping("Controls/ComboBox/Glyph (Disabled)",                         "ComboBox.Disabled.Glyph",                                "");
        p.AssignMapping("Controls/ComboBox/Background (Disabled)",                    "ComboBox.Disabled.Background",                           "");
        p.AssignMapping("Controls/ComboBox/Border (Disabled)",                        "ComboBox.Disabled.Border",                               "");
        p.AssignMapping("Controls/ComboBox/(Editable) Background (Disabled)",         "ComboBox.Disabled.Editable.Background",                  "");
        p.AssignMapping("Controls/ComboBox/(Editable) Border (Disabled)",             "ComboBox.Disabled.Editable.Border",                      "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Background (Disabled)",  "ComboBox.Disabled.Editable.Button.Background",           "");
        p.AssignMapping("Controls/ComboBox/(Editable) Button Border (Disabled)",      "ComboBox.Disabled.Editable.Button.Border",               "");
        p.AssignMapping("Controls/ComboBox/Glyph",                                    "ComboBox.Static.Glyph",                                  "");
        p.AssignMapping("Controls/ComboBoxItem/Background (Hover)",                   "ComboBoxItem.ItemView.Hover.Background",                 "");
        p.AssignMapping("Controls/ComboBoxItem/Border (Hover)",                       "ComboBoxItem.ItemView.Hover.Border",                     "");
        p.AssignMapping("Controls/ComboBoxItem/Background (Selected)",                "ComboBoxItem.ItemView.Selected.Background",              "");
        p.AssignMapping("Controls/ComboBoxItem/Border (Selected)",                    "ComboBoxItem.ItemView.Selected.Border",                  "");
        p.AssignMapping("Controls/ComboBoxItem/Background (Selected + Hover)",        "ComboBoxItem.ItemView.SelectedHover.Background",         "");
        p.AssignMapping("Controls/ComboBoxItem/Border (Selected + Hover)",            "ComboBoxItem.ItemView.SelectedHover.Border",             "");
        p.AssignMapping("Controls/ComboBoxItem/Background (Selected + Not Focused)",  "ComboBoxItem.ItemView.SelectedNoFocus.Background",       "");
        p.AssignMapping("Controls/ComboBoxItem/Border (Selected + Not Focused)",      "ComboBoxItem.ItemView.SelectedNoFocus.Border",           "");
        p.AssignMapping("Controls/ComboBoxItem/Border (Focus)",                       "ComboBoxItem.ItemView.Focus.Border",                     "");
        p.AssignMapping("Controls/ComboBoxItem/Background (HoverFocus)",              "ComboBoxItem.ItemView.HoverFocus.Background",            "");
        p.AssignMapping("Controls/ComboBoxItem/Border (HoverFocus)",                  "ComboBoxItem.ItemView.HoverFocus.Border",                "");
        p.AssignMapping("Controls/TextBox/Background",                                "TextBox.Static.Background",                              "");
        p.AssignMapping("Controls/TextBox/Border",                                    "TextBox.Static.Border",                                  "");
        p.AssignMapping("Controls/TextBox/Border (MouseOver)",                        "TextBox.MouseOver.Border",                               "");
        p.AssignMapping("Controls/TextBox/Border (Focus)",                            "TextBox.Focus.Border",                                   "");
        p.AssignMapping("Controls/TextBox/Selection (Inactive)",                      "TextBox.Selection.Inactive",                             "");
        p.AssignMapping("Controls/TextBox/Selection",                                 "TextBox.Selection",                                      "");
        p.AssignMapping("Controls/TextBox/Border (Error)",                            "TextBox.Error.Border",                                   "");
        p.AssignMapping("Controls/ListBox/Background",                                "ListBox.Static.Background",                              "");
        p.AssignMapping("Controls/ListBox/Border",                                    "ListBox.Static.Border",                                  "");
        p.AssignMapping("Controls/ListBox/Background (Disabled)",                     "ListBox.Disabled.Background",                            "");
        p.AssignMapping("Controls/ListBox/Border (Disabled)",                         "ListBox.Disabled.Border",                                "");
        p.AssignMapping("Controls/ListBoxItem/Background (MouseOver)",                "Item.MouseOver.Background",                              "");
        p.AssignMapping("Controls/ListBoxItem/Border (MouseOver)",                    "Item.MouseOver.Border",                                  "");
        p.AssignMapping("Controls/ListBoxItem/Background (SelectedInactive)",         "Item.SelectedInactive.Background",                       "");
        p.AssignMapping("Controls/ListBoxItem/Border (SelectedInactive)",             "Item.SelectedInactive.Border",                           "");
        p.AssignMapping("Controls/ListBoxItem/Background (SelectedActive)",           "Item.SelectedActive.Background",                         "");
        p.AssignMapping("Controls/ListBoxItem/Border (SelectedActive)",               "Item.SelectedActive.Border",                             "");
        p.AssignMapping("Controls/CheckBox/Background",                               "CheckBox.Static.Background",                             "");
        p.AssignMapping("Controls/CheckBox/Border",                                   "CheckBox.Static.Border",                                 "");
        p.AssignMapping("Controls/CheckBox/Background (MouseOver)",                   "CheckBox.MouseOver.Background",                          "");
        p.AssignMapping("Controls/CheckBox/Border (MouseOver)",                       "CheckBox.MouseOver.Border",                              "");
        p.AssignMapping("Controls/CheckBox/Background (Disabled)",                    "CheckBox.Disabled.Background",                           "");
        p.AssignMapping("Controls/CheckBox/Border (Disabled)",                        "CheckBox.Disabled.Border",                               "");
        p.AssignMapping("Controls/CheckBox/Background (Pressed)",                     "CheckBox.Pressed.Background",                            "");
        p.AssignMapping("Controls/CheckBox/Border (Pressed)",                         "CheckBox.Pressed.Border",                                "");
        p.AssignMapping("Controls/CheckBox/Glyph",                                    "CheckBox.Static.Glyph",                                  "");
        p.AssignMapping("Controls/CheckBox/Glyph (Pressed)",                          "CheckBox.Pressed.Glyph",                                 "");
        p.AssignMapping("Controls/CheckBox/Glyph (MouseOver)",                        "CheckBox.MouseOver.Glyph",                               "");
        p.AssignMapping("Controls/CheckBox/Glyph (Disabled)",                         "CheckBox.Disabled.Glyph",                                "");
        p.AssignMapping("Controls/RadioButton/Background",                            "RadioButton.Static.Background",                          "");
        p.AssignMapping("Controls/RadioButton/Border",                                "RadioButton.Static.Border",                              "");
        p.AssignMapping("Controls/RadioButton/Background (MouseOver)",                "RadioButton.MouseOver.Background",                       "");
        p.AssignMapping("Controls/RadioButton/Border (MouseOver)",                    "RadioButton.MouseOver.Border",                           "");
        p.AssignMapping("Controls/RadioButton/Background (Pressed)",                  "RadioButton.Pressed.Background",                         "");
        p.AssignMapping("Controls/RadioButton/Border (Pressed)",                      "RadioButton.Pressed.Border",                             "");
        p.AssignMapping("Controls/RadioButton/Background (Disabled)",                 "RadioButton.Disabled.Background",                        "");
        p.AssignMapping("Controls/RadioButton/Border (Disabled)",                     "RadioButton.Disabled.Border",                            "");
        p.AssignMapping("Controls/RadioButton/Glyph",                                 "RadioButton.Static.Glyph",                               "");
        p.AssignMapping("Controls/RadioButton/Glyph (MouseOver)",                     "RadioButton.MouseOver.Glyph",                            "");
        p.AssignMapping("Controls/RadioButton/Glyph (Pressed)",                       "RadioButton.Pressed.Glyph",                              "");
        p.AssignMapping("Controls/RadioButton/Glyph (Disabled)",                      "RadioButton.Disabled.Glyph",                             "");
        p.AssignMapping("Controls/ScrollBar/Background",                              "ScrollBar.Static.Background",                            "");
        p.AssignMapping("Controls/ScrollBar/Border",                                  "ScrollBar.Static.Border",                                "");
        p.AssignMapping("Controls/ScrollBarButton/Background",                        "ScrollBarButton.Static.Background",                      "");
        p.AssignMapping("Controls/ScrollBarButton/Border",                            "ScrollBarButton.Static.Border",                          "");
        p.AssignMapping("Controls/ScrollBar/Glyph (Pressed)",                         "ScrollBar.Pressed.Glyph",                                "");
        p.AssignMapping("Controls/ScrollBar/Glyph (MouseOver)",                       "ScrollBar.MouseOver.Glyph",                              "");
        p.AssignMapping("Controls/ScrollBar/Glyph (Disabled)",                        "ScrollBar.Disabled.Glyph",                               "");
        p.AssignMapping("Controls/ScrollBar/Glyph",                                   "ScrollBar.Static.Glyph",                                 "");
        p.AssignMapping("Controls/ScrollBarSink/Background (MouseOver)",              "ScrollBarSink.MouseOver.Background",                     "");
        p.AssignMapping("Controls/ScrollBar/Background (MouseOver)",                  "ScrollBar.MouseOver.Background",                         "");
        p.AssignMapping("Controls/ScrollBar/Border (MouseOver)",                      "ScrollBar.MouseOver.Border",                             "");
        p.AssignMapping("Controls/ScrollBar/Background (Pressed)",                    "ScrollBar.Pressed.Background",                           "");
        p.AssignMapping("Controls/ScrollBar/Border (Pressed)",                        "ScrollBar.Pressed.Border",                               "");
        p.AssignMapping("Controls/ScrollBar/Background (Disabled)",                   "ScrollBar.Disabled.Background",                          "");
        p.AssignMapping("Controls/ScrollBar/Border (Disabled)",                       "ScrollBar.Disabled.Border",                              "");
        p.AssignMapping("Controls/ScrollBar/Thumb (MouseOver)",                       "ScrollBar.MouseOver.Thumb",                              "");
        p.AssignMapping("Controls/ScrollBar/Thumb (Pressed)",                         "ScrollBar.Pressed.Thumb",                                "");
        p.AssignMapping("Controls/ScrollBar/Thumb",                                   "ScrollBar.Static.Thumb",                                 "");
        p.AssignMapping("Controls/TabItem/Background (Selected)",                     "TabItem.Selected.Background",                            "");
        p.AssignMapping("Controls/TabItem/Border (Selected)",                         "TabItem.Selected.Border",                                "");
        p.AssignMapping("Controls/TabItem/Background",                                "TabItem.Static.Background",                              "");
        p.AssignMapping("Controls/TabItem/Border",                                    "TabItem.Static.Border",                                  "");
        p.AssignMapping("Controls/TabItem/Background (MouseOver)",                    "TabItem.MouseOver.Background",                           "");
        p.AssignMapping("Controls/TabItem/Border (MouseOver)",                        "TabItem.MouseOver.Border",                               "");
        p.AssignMapping("Controls/TabItem/Background (Disabled)",                     "TabItem.Disabled.Background",                            "");
        p.AssignMapping("Controls/TabItem/Border (Disabled)",                         "TabItem.Disabled.Border",                                "");
        p.AssignMapping("Controls/Expander/Circle Fill (MouseOver)",                  "Expander.MouseOver.Circle.Fill",                         "");
        p.AssignMapping("Controls/Expander/Circle Stroke (MouseOver)",                "Expander.MouseOver.Circle.Stroke",                       "");
        p.AssignMapping("Controls/Expander/Circle Fill (Pressed)",                    "Expander.Pressed.Circle.Fill",                           "");
        p.AssignMapping("Controls/Expander/Circle Stroke (Pressed)",                  "Expander.Pressed.Circle.Stroke",                         "");
        p.AssignMapping("Controls/Expander/Circle Fill (Disabled)",                   "Expander.Disabled.Circle.Fill",                          "");
        p.AssignMapping("Controls/Expander/Circle Stroke (Disabled)",                 "Expander.Disabled.Circle.Stroke",                        "");
        p.AssignMapping("Controls/Expander/Circle Fill",                              "Expander.Static.Circle.Fill",                            "");
        p.AssignMapping("Controls/Expander/Circle Stroke",                            "Expander.Static.Circle.Stroke",                          "");
        p.AssignMapping("Controls/Expander/Arrow Stroke",                             "Expander.Static.Arrow.Stroke",                           "");
        p.AssignMapping("Controls/Expander/Arrow Stroke (MouseOver)",                 "Expander.MouseOver.Arrow.Stroke",                        "");
        p.AssignMapping("Controls/Expander/Arrow Stroke (Pressed)",                   "Expander.Pressed.Arrow.Stroke",                          "");
        p.AssignMapping("Controls/Expander/Arrow Stroke (Disabled)",                  "Expander.Disabled.Arrow.Stroke",                         "");
        p.AssignMapping("Controls/GridSplitter/Background",                           "GridSplitter.Static.Background",                         "");
        p.AssignMapping("Controls/GroupBox/Background",                               "GroupBox.Static.Background",                             "");
        p.AssignMapping("Controls/GroupBox/Border",                                   "GroupBox.Static.Border",                                 "");
        p.AssignMapping("Controls/GroupBox/Header Background",                        "GroupBox.Header.Static.Background",                      "");
        p.AssignMapping("Controls/GroupBox/Header Border",                            "GroupBox.Header.Static.Border",                          "");
        p.AssignMapping("Controls/GroupBox/Header Foreground",                        "GroupBox.Header.Foreground",                             "");
        p.AssignMapping("Controls/GroupBox/Header Background (Disabled)",             "GroupBox.Header.Background.Disabled",                    "");
        p.AssignMapping("Controls/GroupBox/Header Border (Disabled)",                 "GroupBox.Header.Static.Border.Disabled",                 "");
        p.AssignMapping("Controls/GroupBox/Header Foreground (Disabled)",             "GroupBox.Header.Foreground.Disabled",                    "");
        p.AssignMapping("Controls/Menu/Background",                                   "Menu.Static.Background",                                 "");
        p.AssignMapping("Controls/MenuItem/Background",                               "MenuItem.Static.Background",                             "");
        p.AssignMapping("Controls/MenuItem/IconBar Background",                       "MenuItem.IconBar.Static.Background",                     "");
        p.AssignMapping("Controls/MenuItem/IconBar Border",                           "MenuItem.IconBar.Static.Border",                         "");
        p.AssignMapping("Controls/MenuItem/Square Background",                        "MenuItem.Square.Static.Background",                      "");
        p.AssignMapping("Controls/MenuItem/Square Border",                            "MenuItem.Square.Static.Border",                          "");
        p.AssignMapping("Controls/MenuItem/Background (MouseOver)",                   "MenuItem.MouseOver.Background",                          "");
        p.AssignMapping("Controls/MenuItem/Border (MouseOver)",                       "MenuItem.MouseOver.Border",                              "");
        p.AssignMapping("Controls/MenuItem/Background (Disabled)",                    "MenuItem.Disabled.Background",                           "");
        p.AssignMapping("Controls/Popup/Background",                                  "Popup.Static.Background",                                "");
        p.AssignMapping("Controls/Popup/Border",                                      "Popup.Static.Border",                                    "");
        p.AssignMapping("Controls/Popup/Foreground",                                  "Popup.Static.Foreground",                                "");
        p.AssignMapping("Controls/Popup/Background (Disabled)",                       "Popup.Disabled.Background",                              "");
        p.AssignMapping("Controls/Popup/Border (Disabled)",                           "Popup.Disabled.Border",                                  "");
        p.AssignMapping("Controls/Popup/Foreground (Disabled)",                       "Popup.Disabled.Foreground",                              "");
        p.AssignMapping("Controls/MenuItem/Glyph",                                    "MenuItem.Static.Glyph",                                  "");
        p.AssignMapping("Controls/MenuItem/Glyph (Disabled)",                         "MenuItem.Disabled.Glyph",                                "");
        p.AssignMapping("Controls/SliderThumb/Background (MouseOver)",                "SliderThumb.MouseOver.Background",                       "");
        p.AssignMapping("Controls/SliderThumb/Border (MouseOver)",                    "SliderThumb.MouseOver.Border",                           "");
        p.AssignMapping("Controls/SliderThumb/Background (Pressed)",                  "SliderThumb.Pressed.Background",                         "");
        p.AssignMapping("Controls/SliderThumb/Border (Pressed)",                      "SliderThumb.Pressed.Border",                             "");
        p.AssignMapping("Controls/SliderThumb/Background (Disabled)",                 "SliderThumb.Disabled.Background",                        "");
        p.AssignMapping("Controls/SliderThumb/Border (Disabled)",                     "SliderThumb.Disabled.Border",                            "");
        p.AssignMapping("Controls/SliderThumb/Background",                            "SliderThumb.Static.Background",                          "");
        p.AssignMapping("Controls/SliderThumb/Border",                                "SliderThumb.Static.Border",                              "");
        p.AssignMapping("Controls/SliderThumb/Background (Track)",                    "SliderThumb.Track.Background",                           "");
        p.AssignMapping("Controls/SliderThumb/Border (Track)",                        "SliderThumb.Track.Border",                               "");
        p.AssignMapping("Controls/SliderThumb/Foreground",                            "SliderThumb.Static.Foreground",                          "");
        p.AssignMapping("Controls/SliderThumb/Progress Background (Track)",           "SliderThumb.Track.Progress.Background",                  "");
        p.AssignMapping("Controls/SliderThumb/Progress Border (Track)",               "SliderThumb.Track.Progress.Border",                      "");
        p.AssignMapping("Controls/ProgressBar/Progress",                              "ProgressBar.Progress",                                   "");
        p.AssignMapping("Controls/ProgressBar/Background",                            "ProgressBar.Background",                                 "");
        p.AssignMapping("Controls/ProgressBar/Border",                                "ProgressBar.Border",                                     "");
        p.AssignMapping("Controls/TreeView/Background",                               "TreeView.Static.Background",                             "");
        p.AssignMapping("Controls/TreeView/Border",                                   "TreeView.Static.Border",                                 "");
        p.AssignMapping("Controls/TreeView/Background (Disabled)",                    "TreeView.Disabled.Background",                           "");
        p.AssignMapping("Controls/TreeView/Border (Disabled)",                        "TreeView.Disabled.Border",                               "");
        p.AssignMapping("Controls/TreeViewItem/Background",                           "TreeViewItem.Static.Background",                         "");
        p.AssignMapping("Controls/TreeViewItem/Border",                               "TreeViewItem.Static.Border",                             "");
        p.AssignMapping("Controls/TreeViewItem/Background (Selected)",                "TreeViewItem.Selected.Background",                       "");
        p.AssignMapping("Controls/TreeViewItem/Border (Selected)",                    "TreeViewItem.Selected.Border",                           "");
        p.AssignMapping("Controls/TreeViewItem/Inactive Background (Selected)",       "TreeViewItem.Selected.Inactive.Background",              "");
        p.AssignMapping("Controls/TreeViewItem/Inactive Border (Selected)",           "TreeViewItem.Selected.Inactive.Border",                  "");
        p.AssignMapping("Controls/TreeViewItem/MouseOver Background",                 "TreeViewItem.MouseOver.Background",                      "");
        p.AssignMapping("Controls/TreeViewItem/MouseOver Border",                     "TreeViewItem.MouseOver.Border",                          "");
        p.AssignMapping("Controls/TreeViewItem/MouseDown Background",                 "TreeViewItem.MouseDown.Background",                      "");
        p.AssignMapping("Controls/TreeViewItem/MouseDown Border",                     "TreeViewItem.MouseDown.Border",                          "");
        p.AssignMapping("Controls/Window/Background",                                 "Window.Static.Background",                               "");
        p.AssignMapping("Controls/Window/Border",                                     "Window.Static.Border",                                   "");
        p.AssignMapping("Controls/Window/Foreground",                                 "Window.Static.Foreground",                               "");
        p.AssignMapping("Controls/Window/Title Background",                           "Window.Static.Title.Background",                         "");
    }

    /// <summary>
    /// Sets the current application theme
    /// </summary>
    /// <param name="themeName"></param>
    public abstract void SetTheme(Theme theme);

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
    public abstract Theme RegisterTheme(string name, Theme basedOn, bool copyAllKeys = false);
}
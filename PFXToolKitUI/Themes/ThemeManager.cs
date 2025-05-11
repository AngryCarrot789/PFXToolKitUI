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
        p.AssignMapping("Standard2020/Foreground (Static)", ThemeKeys.S20Foreground_Static);
        p.AssignMapping("Standard2020/Foreground (Deeper)", ThemeKeys.S20Foreground_Deeper);
        p.AssignMapping("Standard2020/Foreground (Disabled)", ThemeKeys.S20Foreground_Disabled);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Static)", ThemeKeys.S20Glyph_Static);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Disabled)", ThemeKeys.S20Glyph_Disabled);
        p.AssignMapping("Standard2020/Glyphs/Glyph (MouseOver)", ThemeKeys.S20Glyph_MouseOver);
        p.AssignMapping("Standard2020/Glyphs/Glyph (MouseDown)", ThemeKeys.S20Glyph_MouseDown);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Selected)", ThemeKeys.S20Glyph_Selected);
        p.AssignMapping("Standard2020/Glyphs/Glyph (Selected Inactive)", ThemeKeys.S20Glyph_Selected_Inactive);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Static)", ThemeKeys.S20ColourfulGlyph_Static);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Disabled)", ThemeKeys.S20ColourfulGlyph_Disabled);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseOver)", ThemeKeys.S20ColourfulGlyph_MouseOver);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (MouseDown)", ThemeKeys.S20ColourfulGlyph_MouseDown);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected)", ThemeKeys.S20ColourfulGlyph_Selected);
        p.AssignMapping("Standard2020/Glyphs/ColourfulGlyph (Selected Inactive)", ThemeKeys.S20ColourfulGlyph_Selected_Inactive);
        p.AssignMapping("Standard2020/AccentTone1/Background (Static)", ThemeKeys.S20AccentTone1_Background_Static);
        p.AssignMapping("Standard2020/AccentTone1/Border (Static)", ThemeKeys.S20AccentTone1_Border_Static);
        p.AssignMapping("Standard2020/AccentTone2/Background (Static)", ThemeKeys.S20AccentTone2_Background_Static);
        p.AssignMapping("Standard2020/AccentTone2/Border (Static)", ThemeKeys.S20AccentTone2_Border_Static);
        p.AssignMapping("Standard2020/AccentTone3/Background (Static)", ThemeKeys.S20AccentTone3_Background_Static);
        p.AssignMapping("Standard2020/AccentTone3/Border (Static)", ThemeKeys.S20AccentTone3_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone0 Background (Static)", ThemeKeys.S20Tone0_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone0 Border (Static)", ThemeKeys.S20Tone0_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone1 Background (Static)", ThemeKeys.S20Tone1_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone1 Border (Static)", ThemeKeys.S20Tone1_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone2 Background (Static)", ThemeKeys.S20Tone2_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone2 Border (Static)", ThemeKeys.S20Tone2_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone3 Background (Static)", ThemeKeys.S20Tone3_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone3 Border (Static)", ThemeKeys.S20Tone3_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Static)", ThemeKeys.S20Tone4_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Static)", ThemeKeys.S20Tone4_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (MouseOver)", ThemeKeys.S20Tone4_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (MouseOver)", ThemeKeys.S20Tone4_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (MouseDown)", ThemeKeys.S20Tone4_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (MouseDown)", ThemeKeys.S20Tone4_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Selected)", ThemeKeys.S20Tone4_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Selected Inactive)", ThemeKeys.S20Tone4_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Selected)", ThemeKeys.S20Tone4_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Selected Inactive)", ThemeKeys.S20Tone4_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone4 Background (Disabled)", ThemeKeys.S20Tone4_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone4 Border (Disabled)", ThemeKeys.S20Tone4_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Static)", ThemeKeys.S20Tone5_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Static)", ThemeKeys.S20Tone5_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (MouseOver)", ThemeKeys.S20Tone5_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (MouseOver)", ThemeKeys.S20Tone5_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (MouseDown)", ThemeKeys.S20Tone5_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (MouseDown)", ThemeKeys.S20Tone5_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Selected)", ThemeKeys.S20Tone5_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Selected Inactive)", ThemeKeys.S20Tone5_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Selected)", ThemeKeys.S20Tone5_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Selected Inactive)", ThemeKeys.S20Tone5_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone5 Background (Disabled)", ThemeKeys.S20Tone5_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone5 Border (Disabled)", ThemeKeys.S20Tone5_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Static)", ThemeKeys.S20Tone6_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Static)", ThemeKeys.S20Tone6_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (MouseOver)", ThemeKeys.S20Tone6_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (MouseOver)", ThemeKeys.S20Tone6_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (MouseDown)", ThemeKeys.S20Tone6_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (MouseDown)", ThemeKeys.S20Tone6_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Selected)", ThemeKeys.S20Tone6_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Selected Inactive)", ThemeKeys.S20Tone6_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Selected)", ThemeKeys.S20Tone6_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Selected Inactive)", ThemeKeys.S20Tone6_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone6 Background (Disabled)", ThemeKeys.S20Tone6_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone6 Border (Disabled)", ThemeKeys.S20Tone6_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Static)", ThemeKeys.S20Tone7_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Static)", ThemeKeys.S20Tone7_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (MouseOver)", ThemeKeys.S20Tone7_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (MouseOver)", ThemeKeys.S20Tone7_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (MouseDown)", ThemeKeys.S20Tone7_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (MouseDown)", ThemeKeys.S20Tone7_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Selected)", ThemeKeys.S20Tone7_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Selected Inactive)", ThemeKeys.S20Tone7_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Selected)", ThemeKeys.S20Tone7_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Selected Inactive)", ThemeKeys.S20Tone7_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone7 Background (Disabled)", ThemeKeys.S20Tone7_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone7 Border (Disabled)", ThemeKeys.S20Tone7_Border_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Static)", ThemeKeys.S20Tone8_Background_Static);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Static)", ThemeKeys.S20Tone8_Border_Static);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (MouseOver)", ThemeKeys.S20Tone8_Background_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (MouseOver)", ThemeKeys.S20Tone8_Border_MouseOver);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (MouseDown)", ThemeKeys.S20Tone8_Background_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (MouseDown)", ThemeKeys.S20Tone8_Border_MouseDown);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Selected)", ThemeKeys.S20Tone8_Background_Selected);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Selected Inactive)", ThemeKeys.S20Tone8_Background_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Selected)", ThemeKeys.S20Tone8_Border_Selected);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Selected Inactive)", ThemeKeys.S20Tone8_Border_Selected_Inactive);
        p.AssignMapping("Standard2020/Panels/Tone8 Background (Disabled)", ThemeKeys.S20Tone8_Background_Disabled);
        p.AssignMapping("Standard2020/Panels/Tone8 Border (Disabled)", ThemeKeys.S20Tone8_Border_Disabled);

        // MemEngine specific
        p.AssignMapping("Memory Engine/Status Bar Background", ThemeKeys.PFX_Editor_StatusBar_Background, "The background of the status bar (at the very bottom of the editor window)");
        p.AssignMapping("Memory Engine/Property Editor/Separator Line (Mouse Over)", ThemeKeys.PFX_PropertyEditor_Separator_MouseOverBrush, "The background of the play head thumb");

        p.AssignMapping("Standard2020 Specific/PanelBorderBrush",                                  "PanelBorderBrush",                                       "");
        p.AssignMapping("Standard2020 Specific/PanelBackground0",                                  "PanelBackground0",                                       "");
        p.AssignMapping("Standard2020 Specific/PanelBackground1",                                  "PanelBackground1",                                       "");
        p.AssignMapping("Standard2020 Specific/PanelBackground2",                                  "PanelBackground2",                                       "");
        p.AssignMapping("Standard2020 Specific/PanelBackground3",                                  "PanelBackground3",                                       "");
        p.AssignMapping("Standard2020 Specific/PanelBackground4",                                  "PanelBackground4",                                       "");
        p.AssignMapping("Standard2020 Specific/Button/Background",                                 "Button.Static.Background",                               "");
        p.AssignMapping("Standard2020 Specific/Button/Border",                                     "Button.Static.Border",                                   "");
        p.AssignMapping("Standard2020 Specific/Button/Background (MouseOver)",                     "Button.MouseOver.Background",                            "");
        p.AssignMapping("Standard2020 Specific/Button/Border (MouseOver)",                         "Button.MouseOver.Border",                                "");
        p.AssignMapping("Standard2020 Specific/Button/Background (Pressed)",                       "Button.Pressed.Background",                              "");
        p.AssignMapping("Standard2020 Specific/Button/Border (Pressed)",                           "Button.Pressed.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/Button/Background (Disabled)",                      "Button.Disabled.Background",                             "");
        p.AssignMapping("Standard2020 Specific/Button/Border (Disabled)",                          "Button.Disabled.Border",                                 "");
        p.AssignMapping("Standard2020 Specific/Button/Foreground",                                 "Button.Static.Foreground",                               "");
        p.AssignMapping("Standard2020 Specific/Button/Foreground (Disabled)",                      "Button.Disabled.Foreground",                             "");
        p.AssignMapping("Standard2020 Specific/Button/Background (Defaulted)",                     "Button.Defaulted.Background",                            "");
        p.AssignMapping("Standard2020 Specific/Button/Border (Defaulted)",                         "Button.Defaulted.Border",                                "");
        p.AssignMapping("Standard2020 Specific/ButtonSpinner/Border (Error)",                      "ButtonSpinner.Error.Border",                             "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Background (IsChecked)",               "ToggleButton.IsChecked.Background",                      "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Border (IsChecked)",                   "ToggleButton.IsChecked.Border",                          "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Background (Pressed + IsChecked)",     "ToggleButton.Pressed.IsChecked.Background",              "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Border (Pressed + IsChecked)",         "ToggleButton.Pressed.IsChecked.Border",                  "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Background (MouseOver + IsChecked)",   "ToggleButton.MouseOver.IsChecked.Background",            "");
        p.AssignMapping("Standard2020 Specific/ToggleButton/Border (MouseOver + IsChecked)",       "ToggleButton.MouseOver.IsChecked.Border",                "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Background",                               "ComboBox.Static.Background",                             "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Border",                                   "ComboBox.Static.Border",                                 "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Foreground",                               "ComboBox.Static.Foreground",                             "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Background (Editable)",                    "ComboBox.Static.Editable.Background",                    "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Border (Editable)",                        "ComboBox.Static.Editable.Border",                        "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Button Background (Editable)",             "ComboBox.Static.Editable.Button.Background",             "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Button Border (Editable)",                 "ComboBox.Static.Editable.Button.Border",                 "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Button Background",                        "ComboBox.Button.Static.Background",                      "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Button Border",                            "ComboBox.Button.Static.Border",                          "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Glyph (MouseOver)",                        "ComboBox.MouseOver.Glyph",                               "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Background (MouseOver)",                   "ComboBox.MouseOver.Background",                          "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Border (MouseOver)",                       "ComboBox.MouseOver.Border",                              "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Background (MouseOver)",        "ComboBox.MouseOver.Editable.Background",                 "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Border",                        "ComboBox.MouseOver.Editable.Border",                     "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Background (MouseOver)", "ComboBox.MouseOver.Editable.Button.Background",          "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Border (MouseOver)",     "ComboBox.MouseOver.Editable.Button.Border",              "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Glyph (Pressed)",                          "ComboBox.Pressed.Glyph",                                 "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Background (Pressed)",                     "ComboBox.Pressed.Background",                            "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Border (Pressed)",                         "ComboBox.Pressed.Border",                                "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Background (Pressed)",          "ComboBox.Pressed.Editable.Background",                   "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Border (Pressed)",              "ComboBox.Pressed.Editable.Border",                       "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Background (Pressed)",   "ComboBox.Pressed.Editable.Button.Background",            "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Border (Pressed)",       "ComboBox.Pressed.Editable.Button.Border",                "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Glyph (Disabled)",                         "ComboBox.Disabled.Glyph",                                "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Background (Disabled)",                    "ComboBox.Disabled.Background",                           "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Border (Disabled)",                        "ComboBox.Disabled.Border",                               "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Background (Disabled)",         "ComboBox.Disabled.Editable.Background",                  "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Border (Disabled)",             "ComboBox.Disabled.Editable.Border",                      "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Background (Disabled)",  "ComboBox.Disabled.Editable.Button.Background",           "");
        p.AssignMapping("Standard2020 Specific/ComboBox/(Editable) Button Border (Disabled)",      "ComboBox.Disabled.Editable.Button.Border",               "");
        p.AssignMapping("Standard2020 Specific/ComboBox/Glyph",                                    "ComboBox.Static.Glyph",                                  "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Background (Hover)",                   "ComboBoxItem.ItemView.Hover.Background",                 "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (Hover)",                       "ComboBoxItem.ItemView.Hover.Border",                     "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Background (Selected)",                "ComboBoxItem.ItemView.Selected.Background",              "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (Selected)",                    "ComboBoxItem.ItemView.Selected.Border",                  "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Background (Selected + Hover)",        "ComboBoxItem.ItemView.SelectedHover.Background",         "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (Selected + Hover)",            "ComboBoxItem.ItemView.SelectedHover.Border",             "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Background (Selected + Not Focused)",  "ComboBoxItem.ItemView.SelectedNoFocus.Background",       "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (Selected + Not Focused)",      "ComboBoxItem.ItemView.SelectedNoFocus.Border",           "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (Focus)",                       "ComboBoxItem.ItemView.Focus.Border",                     "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Background (HoverFocus)",              "ComboBoxItem.ItemView.HoverFocus.Background",            "");
        p.AssignMapping("Standard2020 Specific/ComboBoxItem/Border (HoverFocus)",                  "ComboBoxItem.ItemView.HoverFocus.Border",                "");
        p.AssignMapping("Standard2020 Specific/TextBox/Background",                                "TextBox.Static.Background",                              "");
        p.AssignMapping("Standard2020 Specific/TextBox/Border",                                    "TextBox.Static.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/TextBox/Border (MouseOver)",                        "TextBox.MouseOver.Border",                               "");
        p.AssignMapping("Standard2020 Specific/TextBox/Border (Focus)",                            "TextBox.Focus.Border",                                   "");
        p.AssignMapping("Standard2020 Specific/TextBox/Selection (Inactive)",                      "TextBox.Selection.Inactive",                             "");
        p.AssignMapping("Standard2020 Specific/TextBox/Selection",                                 "TextBox.Selection",                                      "");
        p.AssignMapping("Standard2020 Specific/TextBox/Border (Error)",                            "TextBox.Error.Border",                                   "");
        p.AssignMapping("Standard2020 Specific/ListBox/Background",                                "ListBox.Static.Background",                              "");
        p.AssignMapping("Standard2020 Specific/ListBox/Border",                                    "ListBox.Static.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/ListBox/Background (Disabled)",                     "ListBox.Disabled.Background",                            "");
        p.AssignMapping("Standard2020 Specific/ListBox/Border (Disabled)",                         "ListBox.Disabled.Border",                                "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Background (MouseOver)",                "Item.MouseOver.Background",                              "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Border (MouseOver)",                    "Item.MouseOver.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Background (SelectedInactive)",         "Item.SelectedInactive.Background",                       "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Border (SelectedInactive)",             "Item.SelectedInactive.Border",                           "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Background (SelectedActive)",           "Item.SelectedActive.Background",                         "");
        p.AssignMapping("Standard2020 Specific/ListBoxItem/Border (SelectedActive)",               "Item.SelectedActive.Border",                             "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Background",                             "OptionMark.Static.Background",                           "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Border",                                 "OptionMark.Static.Border",                               "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Background (MouseOver)",                 "OptionMark.MouseOver.Background",                        "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Border (MouseOver)",                     "OptionMark.MouseOver.Border",                            "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Background (Disabled)",                  "OptionMark.Disabled.Background",                         "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Border (Disabled)",                      "OptionMark.Disabled.Border",                             "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Background (Pressed)",                   "OptionMark.Pressed.Background",                          "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Border (Pressed)",                       "OptionMark.Pressed.Border",                              "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Glyph",                                  "OptionMark.Static.Glyph",                                "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Glyph (Pressed)",                        "OptionMark.Pressed.Glyph",                               "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Glyph (MouseOver)",                      "OptionMark.MouseOver.Glyph",                             "");
        p.AssignMapping("Standard2020 Specific/OptionMark/Glyph (Disabled)",                       "OptionMark.Disabled.Glyph",                              "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Background",                              "ScrollBar.Static.Background",                            "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Border",                                  "ScrollBar.Static.Border",                                "");
        p.AssignMapping("Standard2020 Specific/ScrollBarButton/Background",                        "ScrollBarButton.Static.Background",                      "");
        p.AssignMapping("Standard2020 Specific/ScrollBarButton/Border",                            "ScrollBarButton.Static.Border",                          "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Glyph (Pressed)",                         "ScrollBar.Pressed.Glyph",                                "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Glyph (MouseOver)",                       "ScrollBar.MouseOver.Glyph",                              "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Glyph (Disabled)",                        "ScrollBar.Disabled.Glyph",                               "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Glyph",                                   "ScrollBar.Static.Glyph",                                 "");
        p.AssignMapping("Standard2020 Specific/ScrollBarSink/Background (MouseOver)",              "ScrollBarSink.MouseOver.Background",                     "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Background (MouseOver)",                  "ScrollBar.MouseOver.Background",                         "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Border (MouseOver)",                      "ScrollBar.MouseOver.Border",                             "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Background (Pressed)",                    "ScrollBar.Pressed.Background",                           "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Border (Pressed)",                        "ScrollBar.Pressed.Border",                               "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Background (Disabled)",                   "ScrollBar.Disabled.Background",                          "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Border (Disabled)",                       "ScrollBar.Disabled.Border",                              "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Thumb (MouseOver)",                       "ScrollBar.MouseOver.Thumb",                              "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Thumb (Pressed)",                         "ScrollBar.Pressed.Thumb",                                "");
        p.AssignMapping("Standard2020 Specific/ScrollBar/Thumb",                                   "ScrollBar.Static.Thumb",                                 "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Background",                            "RadioButton.Static.Background",                          "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Border",                                "RadioButton.Static.Border",                              "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Background (MouseOver)",                "RadioButton.MouseOver.Background",                       "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Border (MouseOver)",                    "RadioButton.MouseOver.Border",                           "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Background (Pressed)",                  "RadioButton.Pressed.Background",                         "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Border (Pressed)",                      "RadioButton.Pressed.Border",                             "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Background (Disabled)",                 "RadioButton.Disabled.Background",                        "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Border (Disabled)",                     "RadioButton.Disabled.Border",                            "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Glyph",                                 "RadioButton.Static.Glyph",                               "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Glyph (MouseOver)",                     "RadioButton.MouseOver.Glyph",                            "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Glyph (Pressed)",                       "RadioButton.Pressed.Glyph",                              "");
        p.AssignMapping("Standard2020 Specific/RadioButton/Glyph (Disabled)",                      "RadioButton.Disabled.Glyph",                             "");
        p.AssignMapping("Standard2020 Specific/TabItem/Background (Selected)",                     "TabItem.Selected.Background",                            "");
        p.AssignMapping("Standard2020 Specific/TabItem/Border (Selected)",                         "TabItem.Selected.Border",                                "");
        p.AssignMapping("Standard2020 Specific/TabItem/Background",                                "TabItem.Static.Background",                              "");
        p.AssignMapping("Standard2020 Specific/TabItem/Border",                                    "TabItem.Static.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/TabItem/Background (MouseOver)",                    "TabItem.MouseOver.Background",                           "");
        p.AssignMapping("Standard2020 Specific/TabItem/Border (MouseOver)",                        "TabItem.MouseOver.Border",                               "");
        p.AssignMapping("Standard2020 Specific/TabItem/Background (Disabled)",                     "TabItem.Disabled.Background",                            "");
        p.AssignMapping("Standard2020 Specific/TabItem/Border (Disabled)",                         "TabItem.Disabled.Border",                                "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Fill (MouseOver)",                  "Expander.MouseOver.Circle.Fill",                         "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Stroke (MouseOver)",                "Expander.MouseOver.Circle.Stroke",                       "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Fill (Pressed)",                    "Expander.Pressed.Circle.Fill",                           "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Stroke (Pressed)",                  "Expander.Pressed.Circle.Stroke",                         "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Fill (Disabled)",                   "Expander.Disabled.Circle.Fill",                          "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Stroke (Disabled)",                 "Expander.Disabled.Circle.Stroke",                        "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Fill",                              "Expander.Static.Circle.Fill",                            "");
        p.AssignMapping("Standard2020 Specific/Expander/Circle Stroke",                            "Expander.Static.Circle.Stroke",                          "");
        p.AssignMapping("Standard2020 Specific/Expander/Arrow Stroke",                             "Expander.Static.Arrow.Stroke",                           "");
        p.AssignMapping("Standard2020 Specific/Expander/Arrow Stroke (MouseOver)",                 "Expander.MouseOver.Arrow.Stroke",                        "");
        p.AssignMapping("Standard2020 Specific/Expander/Arrow Stroke (Pressed)",                   "Expander.Pressed.Arrow.Stroke",                          "");
        p.AssignMapping("Standard2020 Specific/Expander/Arrow Stroke (Disabled)",                  "Expander.Disabled.Arrow.Stroke",                         "");
        p.AssignMapping("Standard2020 Specific/GridSplitter/Background",                           "GridSplitter.Static.Background",                         "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Background",                               "GroupBox.Static.Background",                             "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Border",                                   "GroupBox.Static.Border",                                 "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Background",                        "GroupBox.Header.Static.Background",                      "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Border",                            "GroupBox.Header.Static.Border",                          "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Foreground",                        "GroupBox.Header.Foreground",                             "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Background (Disabled)",             "GroupBox.Header.Background.Disabled",                    "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Border (Disabled)",                 "GroupBox.Header.Static.Border.Disabled",                 "");
        p.AssignMapping("Standard2020 Specific/GroupBox/Header Foreground (Disabled)",             "GroupBox.Header.Foreground.Disabled",                    "");
        p.AssignMapping("Standard2020 Specific/Menu/Background",                                   "Menu.Static.Background",                                 "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Background",                               "MenuItem.Static.Background",                             "");
        p.AssignMapping("Standard2020 Specific/MenuItem/IconBar Background",                       "MenuItem.IconBar.Static.Background",                     "");
        p.AssignMapping("Standard2020 Specific/MenuItem/IconBar Border",                           "MenuItem.IconBar.Static.Border",                         "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Square Background",                        "MenuItem.Square.Static.Background",                      "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Square Border",                            "MenuItem.Square.Static.Border",                          "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Background (MouseOver)",                   "MenuItem.MouseOver.Background",                          "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Border (MouseOver)",                       "MenuItem.MouseOver.Border",                              "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Background (Disabled)",                    "MenuItem.Disabled.Background",                           "");
        p.AssignMapping("Standard2020 Specific/Popup/Background",                                  "Popup.Static.Background",                                "");
        p.AssignMapping("Standard2020 Specific/Popup/Border",                                      "Popup.Static.Border",                                    "");
        p.AssignMapping("Standard2020 Specific/Popup/Foreground",                                  "Popup.Static.Foreground",                                "");
        p.AssignMapping("Standard2020 Specific/Popup/Background (Disabled)",                       "Popup.Disabled.Background",                              "");
        p.AssignMapping("Standard2020 Specific/Popup/Border (Disabled)",                           "Popup.Disabled.Border",                                  "");
        p.AssignMapping("Standard2020 Specific/Popup/Foreground (Disabled)",                       "Popup.Disabled.Foreground",                              "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Glyph",                                    "MenuItem.Static.Glyph",                                  "");
        p.AssignMapping("Standard2020 Specific/MenuItem/Glyph (Disabled)",                         "MenuItem.Disabled.Glyph",                                "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Background (MouseOver)",                "SliderThumb.MouseOver.Background",                       "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Border (MouseOver)",                    "SliderThumb.MouseOver.Border",                           "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Background (Pressed)",                  "SliderThumb.Pressed.Background",                         "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Border (Pressed)",                      "SliderThumb.Pressed.Border",                             "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Background (Disabled)",                 "SliderThumb.Disabled.Background",                        "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Border (Disabled)",                     "SliderThumb.Disabled.Border",                            "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Background",                            "SliderThumb.Static.Background",                          "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Border",                                "SliderThumb.Static.Border",                              "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Background (Track)",                    "SliderThumb.Track.Background",                           "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Border (Track)",                        "SliderThumb.Track.Border",                               "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Foreground",                            "SliderThumb.Static.Foreground",                          "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Progress Background (Track)",           "SliderThumb.Track.Progress.Background",                  "");
        p.AssignMapping("Standard2020 Specific/SliderThumb/Progress Border (Track)",               "SliderThumb.Track.Progress.Border",                      "");
        p.AssignMapping("Standard2020 Specific/ProgressBar/Progress",                              "ProgressBar.Progress",                                   "");
        p.AssignMapping("Standard2020 Specific/ProgressBar/Background",                            "ProgressBar.Background",                                 "");
        p.AssignMapping("Standard2020 Specific/ProgressBar/Border",                                "ProgressBar.Border",                                     "");
        p.AssignMapping("Standard2020 Specific/TreeView/Background",                               "TreeView.Static.Background",                             "");
        p.AssignMapping("Standard2020 Specific/TreeView/Border",                                   "TreeView.Static.Border",                                 "");
        p.AssignMapping("Standard2020 Specific/TreeView/Background (Disabled)",                    "TreeView.Disabled.Background",                           "");
        p.AssignMapping("Standard2020 Specific/TreeView/Border (Disabled)",                        "TreeView.Disabled.Border",                               "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Background",                           "TreeViewItem.Static.Background",                         "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Border",                               "TreeViewItem.Static.Border",                             "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Background (Selected)",                "TreeViewItem.Selected.Background",                       "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Border (Selected)",                    "TreeViewItem.Selected.Border",                           "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Inactive Background (Selected)",       "TreeViewItem.Selected.Inactive.Background",              "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/Inactive Border (Selected)",           "TreeViewItem.Selected.Inactive.Border",                  "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/MouseOver Background",                 "TreeViewItem.MouseOver.Background",                      "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/MouseOver Border",                     "TreeViewItem.MouseOver.Border",                          "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/MouseDown Background",                 "TreeViewItem.MouseDown.Background",                      "");
        p.AssignMapping("Standard2020 Specific/TreeViewItem/MouseDown Border",                     "TreeViewItem.MouseDown.Border",                          "");
        p.AssignMapping("Standard2020 Specific/Window/Background",                                 "Window.Static.Background",                               "");
        p.AssignMapping("Standard2020 Specific/Window/Border",                                     "Window.Static.Border",                                   "");
        p.AssignMapping("Standard2020 Specific/Window/Foreground",                                 "Window.Static.Foreground",                               "");
        p.AssignMapping("Standard2020 Specific/Window/Title Background",                           "Window.Static.Title.Background",                         "");
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
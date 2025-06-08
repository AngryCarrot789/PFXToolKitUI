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
using PFXToolKitUI.Utils.Collections.Observable;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes;

public class ThemeManagerImpl : ThemeManager {
    private readonly ObservableList<Theme> themes;
    private readonly List<Theme> builtInThemes = new List<Theme>(2);
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
            if (themeName == null) {
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

    public override IEnumerable<Theme> GetBuiltInThemes() {
        return this.builtInThemes;
    }

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

    public class ThemeImpl : Theme {
        public const string ColourSuffix = ".Color";

        private readonly HashSet<string> registeredBrushKeys;
        private List<ThemeImpl>? derivedList;

        private readonly Dictionary<string, HashSet<string>> inheritMap_inheritFrom2themeKey; // map "inheritFrom" to the keys that we need to refresh.
        private readonly Dictionary<string, string> inheritMap_themeKey2inheritFrom; // map the key to update when "inheritFrom" changes.
        private Dictionary<string, HashSet<string>>? cachedInheritMap_InheritFrom2dstKey;

        private Dictionary<string, HashSet<string>> GeneratedInheritanceMap {
            get {
                if (this.cachedInheritMap_InheritFrom2dstKey != null)
                    return this.cachedInheritMap_InheritFrom2dstKey;

                Dictionary<string, HashSet<string>> map = new Dictionary<string, HashSet<string>>();
                List<ThemeImpl> list = this.GetInheritance();
                for (int i = list.Count - 1; i >= 0; i--) { // iterate root base to top derived
                    foreach (KeyValuePair<string, HashSet<string>> entry in list[i].inheritMap_inheritFrom2themeKey) {
                        if (!map.TryGetValue(entry.Key, out HashSet<string>? set))
                            map[entry.Key] = set = new HashSet<string>();
                        foreach (string key in entry.Value)
                            set.Add(key);
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
            }
            else {
                colourKey = key + ColourSuffix;
                brushKey = key;
            }
        }

        public void CopyKeysFrom(ThemeImpl theme) {
            using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);
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
            using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);
            change[colourKey] = avColourBoxed;
            change[brushKey] = avBrush;
            this.UpdateResourcesFromLatestInheritanceMap(change, colourKey, avColourBoxed, brushKey, avBrush);
        }

        public override void SetThemeBrush(string key, IColourBrush brush) {
            // Our theme library creates a colour and brush, because some of the UI may use both
            GetKeys(key, out string brushKey, out string colourKey);

            this.registeredBrushKeys.Add(brushKey);

            using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);

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

            using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);

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
                if (theme.Resources.TryGetResource(themeKey, new ThemeVariant(theme.Name, null), out object? value)) {
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

        private void UpdateResourcesFromLatestInheritanceMap(AvUtils.ResourceDictionaryMultiChangeToken change, string? colourKey, object? colour, string? brushKey, IBrush? brush) {
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

        // A more optimised version
        public void SetInheritanceFromMap(Dictionary<string, string?> map) {
            this.SetInheritance(map);
        }

        public override void SetInheritance(IEnumerable<KeyValuePair<string, string?>> entries) {
            List<KeyValuePair<string, string?>> entryList = entries.ToList();
            if (entryList.Count < 1) {
                return;
            }

            this.InvalidateDerivedInheritMaps(true);
            using (AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources)) {
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

            using (AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources)) {
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
                    using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);
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
                using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);
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

            using AvUtils.ResourceDictionaryMultiChangeToken change = AvUtils.BeginMultiChange(this.Resources);
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

            // do not set up inheritance for derived themes, since there's no point
            if (!this.IsBuiltIn) {
                return;
            }

            Dictionary<string, string?> inheritMap = new Dictionary<string, string?>() {
                ["PanelBorderBrush"] = "ABrush.Tone1.Border.Static",
                ["PanelBackground0"] = "ABrush.Tone0.Background.Static",
                ["PanelBackground1"] = "ABrush.Tone1.Background.Static",
                ["PanelBackground2"] = "ABrush.Tone2.Background.Static",
                ["PanelBackground3"] = "ABrush.Tone3.Background.Static",
                ["PanelBackground4"] = "ABrush.Tone4.Background.Static",
                ["Button.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["Button.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["Button.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["Button.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["Button.Pressed.Background"] = "ABrush.Tone5.Background.MouseDown",
                ["Button.Pressed.Border"] = "ABrush.Tone5.Border.MouseDown",
                ["Button.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["Button.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["Button.Static.Foreground"] = "ABrush.Foreground.Static",
                ["Button.Disabled.Foreground"] = "ABrush.Foreground.Disabled",
                ["Button.Defaulted.Background"] = "ABrush.ColourfulGlyph.Static",
                ["Button.Defaulted.Border"] = "ABrush.ColourfulGlyph.Disabled",
                ["ToggleButton.IsChecked.Background"] = "ABrush.AccentTone2.Background.Static",
                ["ToggleButton.IsChecked.Border"] = "ABrush.AccentTone2.Border.Static",
                ["ToggleButton.Pressed.IsChecked.Background"] = "ABrush.Tone6.Border.Selected",
                ["ToggleButton.Pressed.IsChecked.Border"] = "ABrush.Tone6.Border.Selected",
                ["ToggleButton.MouseOver.IsChecked.Background"] = "ABrush.AccentTone1.Background.Static",
                ["ToggleButton.MouseOver.IsChecked.Border"] = "ABrush.AccentTone1.Border.Static",
                ["ComboBox.Static.Background"] = "ABrush.Tone4.Background.Static",
                ["ComboBox.Static.Border"] = "ABrush.Tone6.Border.Static",
                ["ComboBox.Static.Foreground"] = "ABrush.Foreground.Static",
                ["ComboBox.Static.Editable.Background"] = "ABrush.Tone4.Background.Static",
                ["ComboBox.Static.Editable.Border"] = "ABrush.Tone4.Border.Static",
                ["ComboBox.Button.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["ComboBox.Button.Static.Border"] = "ABrush.Tone6.Border.Static",
                ["ComboBox.MouseOver.Glyph"] = "ABrush.Glyph.MouseOver",
                ["ComboBox.MouseOver.Background"] = "ABrush.Tone4.Background.MouseOver",
                ["ComboBox.MouseOver.Border"] = "ABrush.Tone4.Border.MouseOver",
                ["ComboBox.MouseOver.Editable.Background"] = "ABrush.Tone4.Background.MouseOver",
                ["ComboBox.MouseOver.Editable.Border"] = "ABrush.Tone4.Border.MouseOver",
                ["ComboBox.Pressed.Glyph"] = "ABrush.Glyph.MouseDown",
                ["ComboBox.Pressed.Background"] = "ABrush.Tone4.Background.MouseDown",
                ["ComboBox.Pressed.Border"] = "ABrush.Tone4.Border.MouseDown",
                ["ComboBox.Pressed.Editable.Background"] = "ABrush.Tone4.Background.MouseDown",
                ["ComboBox.Pressed.Editable.Border"] = "ABrush.Tone4.Border.MouseDown",
                ["ComboBox.Disabled.Glyph"] = "ABrush.Glyph.Disabled",
                ["ComboBox.Disabled.Background"] = "ABrush.Tone4.Background.Disabled",
                ["ComboBox.Disabled.Border"] = "ABrush.Tone4.Border.Disabled",
                ["ComboBox.Disabled.Editable.Background"] = "ABrush.Tone4.Background.Disabled",
                ["ComboBox.Disabled.Editable.Border"] = "ABrush.Tone4.Border.Disabled",
                ["ComboBox.Static.Glyph"] = "ABrush.Glyph.Static",
                ["ComboBoxItem.ItemView.Hover.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["ComboBoxItem.ItemView.Hover.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["ComboBoxItem.ItemView.Selected.Background"] = "ABrush.Tone5.Background.Selected",
                ["ComboBoxItem.ItemView.Selected.Border"] = "ABrush.Tone5.Border.Selected",
                ["ComboBoxItem.ItemView.SelectedHover.Background"] = "ABrush.Tone5.Background.Selected",
                ["ComboBoxItem.ItemView.SelectedHover.Border"] = "ABrush.Tone5.Border.Selected",
                ["ComboBoxItem.ItemView.SelectedNoFocus.Background"] = "ABrush.Tone5.Background.Selected.Inactive",
                ["ComboBoxItem.ItemView.SelectedNoFocus.Border"] = "ABrush.Tone5.Border.Selected.Inactive",
                ["ComboBoxItem.ItemView.Focus.Border"] = "ABrush.Tone5.Border.Static",
                ["ComboBoxItem.ItemView.HoverFocus.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["ComboBoxItem.ItemView.HoverFocus.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["TextBox.Static.Background"] = "ABrush.Tone4.Background.Static",
                ["TextBox.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["TextBox.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["TextBox.Focus.Border"] = "ABrush.ColourfulGlyph.Static",
                ["TextBox.Selection.Inactive"] = "ABrush.ColourfulGlyph.Selected.Inactive",
                ["TextBox.Selection"] = "ABrush.ColourfulGlyph.Selected",
                ["ListBox.Static.Background"] = "ABrush.Tone4.Background.Static",
                ["ListBox.Static.Border"] = "ABrush.Tone4.Border.Static",
                ["ListBox.Disabled.Background"] = "ABrush.Tone4.Background.Disabled",
                ["ListBox.Disabled.Border"] = "ABrush.Tone4.Border.Disabled",
                ["Item.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["Item.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["Item.SelectedInactive.Background"] = "ABrush.Tone5.Background.Selected.Inactive",
                ["Item.SelectedInactive.Border"] = "ABrush.Tone5.Border.Selected.Inactive",
                ["Item.SelectedActive.Background"] = "ABrush.Tone5.Background.Selected",
                ["Item.SelectedActive.Border"] = "ABrush.Tone5.Border.Selected",
                ["CheckBox.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["CheckBox.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["CheckBox.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["CheckBox.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["CheckBox.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["CheckBox.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["CheckBox.Pressed.Background"] = "ABrush.Tone5.Background.MouseDown",
                ["CheckBox.Pressed.Border"] = "ABrush.Tone5.Border.MouseDown",
                ["CheckBox.Static.Glyph"] = "ABrush.Glyph.Static",
                ["CheckBox.Pressed.Glyph"] = "ABrush.Glyph.MouseDown",
                ["CheckBox.MouseOver.Glyph"] = "ABrush.Glyph.MouseOver",
                ["CheckBox.Disabled.Glyph"] = "ABrush.Glyph.Disabled",
                ["RadioButton.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["RadioButton.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["RadioButton.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["RadioButton.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["RadioButton.Pressed.Background"] = "ABrush.Tone5.Background.MouseDown",
                ["RadioButton.Pressed.Border"] = "ABrush.Tone5.Border.MouseDown",
                ["RadioButton.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["RadioButton.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["RadioButton.Static.Glyph"] = "ABrush.Glyph.Static",
                ["RadioButton.MouseOver.Glyph"] = "ABrush.Glyph.MouseOver",
                ["RadioButton.Pressed.Glyph"] = "ABrush.Glyph.MouseDown",
                ["RadioButton.Disabled.Glyph"] = "ABrush.Glyph.Disabled",
                ["ScrollBar.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["ScrollBar.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["ScrollBarButton.Static.Background"] = "ABrush.Tone6.Background.Static",
                ["ScrollBarButton.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["ScrollBar.Pressed.Glyph"] = "ABrush.Glyph.MouseDown",
                ["ScrollBar.MouseOver.Glyph"] = "ABrush.Glyph.MouseOver",
                ["ScrollBar.Disabled.Glyph"] = "ABrush.Glyph.Disabled",
                ["ScrollBar.Static.Glyph"] = "ABrush.Glyph.Static",
                ["ScrollBarSink.MouseOver.Background"] = "ABrush.Tone1.Background.Static",
                ["ScrollBar.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["ScrollBar.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["ScrollBar.Pressed.Background"] = "ABrush.Tone5.Background.MouseDown",
                ["ScrollBar.Pressed.Border"] = "ABrush.Tone5.Border.MouseDown",
                ["ScrollBar.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["ScrollBar.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["ScrollBar.MouseOver.Thumb"] = "ABrush.Tone7.Background.MouseOver",
                ["ScrollBar.Pressed.Thumb"] = "ABrush.Tone7.Background.MouseDown",
                ["ScrollBar.Static.Thumb"] = "ABrush.Tone7.Background.Static",
                ["TabItem.Selected.Background"] = "ABrush.Tone6.Background.Static",
                ["TabItem.Selected.Border"] = "ABrush.Tone5.Border.Selected",
                ["TabItem.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["TabItem.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["TabItem.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["TabItem.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["TabItem.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["TabItem.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["Expander.MouseOver.Circle.Fill"] = "ABrush.Tone5.Background.MouseOver",
                ["Expander.MouseOver.Circle.Stroke"] = "ABrush.Tone5.Border.MouseOver",
                ["Expander.Pressed.Circle.Fill"] = "ABrush.Tone5.Background.MouseDown",
                ["Expander.Pressed.Circle.Stroke"] = "ABrush.Tone5.Border.MouseDown",
                ["Expander.Disabled.Circle.Fill"] = "ABrush.Tone5.Background.Disabled",
                ["Expander.Disabled.Circle.Stroke"] = "ABrush.Tone5.Border.Disabled",
                ["Expander.Static.Circle.Fill"] = "ABrush.Tone5.Background.Static",
                ["Expander.Static.Circle.Stroke"] = "ABrush.Tone5.Border.Static",
                ["Expander.Static.Arrow.Stroke"] = "ABrush.Glyph.Static",
                ["Expander.MouseOver.Arrow.Stroke"] = "ABrush.Glyph.MouseOver",
                ["Expander.Pressed.Arrow.Stroke"] = "ABrush.Glyph.MouseDown",
                ["Expander.Disabled.Arrow.Stroke"] = "ABrush.Glyph.Disabled",
                ["GridSplitter.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["GroupBox.Static.Border"] = "ABrush.Tone6.Border.Static",
                ["GroupBox.Header.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["GroupBox.Header.Static.Border"] = "ABrush.Tone6.Border.Static",
                ["GroupBox.Header.Foreground"] = "ABrush.Foreground.Static",
                ["GroupBox.Header.Background.Disabled"] = "ABrush.Tone5.Background.Disabled",
                ["GroupBox.Header.Static.Border.Disabled"] = "ABrush.Tone6.Border.Disabled",
                ["GroupBox.Header.Foreground.Disabled"] = "ABrush.Foreground.Disabled",
                ["MenuItem.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["MenuItem.IconBar.Static.Background"] = "ABrush.Tone6.Background.Static",
                ["MenuItem.IconBar.Static.Border"] = "ABrush.Tone6.Border.Static",
                ["MenuItem.Square.Static.Background"] = "ABrush.Tone6.Background.Static",
                ["MenuItem.Square.Static.Border"] = "ABrush.Tone8.Border.Static",
                ["MenuItem.MouseOver.Background"] = "ABrush.AccentTone2.Background.Static",
                ["MenuItem.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["MenuItem.Disabled.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["Popup.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["Popup.Static.Border"] = "ABrush.Tone7.Border.Static",
                ["Popup.Static.Foreground"] = "ABrush.Foreground.Static",
                ["Popup.Disabled.Background"] = "ABrush.Tone4.Background.Disabled",
                ["Popup.Disabled.Border"] = "ABrush.Tone4.Border.Disabled",
                ["Popup.Disabled.Foreground"] = "ABrush.Foreground.Disabled",
                ["MenuItem.Static.Glyph"] = "ABrush.Glyph.Static",
                ["MenuItem.Disabled.Glyph"] = "ABrush.Glyph.Disabled",
                ["SliderThumb.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["SliderThumb.MouseOver.Border"] = "ABrush.Tone5.Border.MouseOver",
                ["SliderThumb.Pressed.Background"] = "ABrush.Tone5.Background.MouseDown",
                ["SliderThumb.Pressed.Border"] = "ABrush.Tone5.Border.MouseDown",
                ["SliderThumb.Disabled.Background"] = "ABrush.Tone5.Background.Disabled",
                ["SliderThumb.Disabled.Border"] = "ABrush.Tone5.Border.Disabled",
                ["SliderThumb.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["SliderThumb.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["SliderThumb.Track.Background"] = "ABrush.Tone5.Background.Static",
                ["SliderThumb.Track.Border"] = "ABrush.Tone5.Border.Static",
                ["SliderThumb.Static.Foreground"] = "ABrush.Glyph.Disabled",
                ["SliderThumb.Track.Progress.Background"] = "ABrush.ColourfulGlyph.Static",
                ["SliderThumb.Track.Progress.Border"] = "ABrush.Tone5.Border.Static",
                ["ProgressBar.Progress"] = "ABrush.ColourfulGlyph.Static",
                ["ProgressBar.Background"] = "ABrush.Tone5.Background.Static",
                ["ProgressBar.Border"] = "ABrush.Tone5.Border.Static",
                ["TreeView.Static.Background"] = "ABrush.Tone4.Background.Static",
                ["TreeView.Static.Border"] = "ABrush.Tone4.Border.Static",
                ["TreeView.Disabled.Background"] = "ABrush.Tone4.Background.Disabled",
                ["TreeView.Disabled.Border"] = "ABrush.Tone4.Border.Disabled",
                ["TreeViewItem.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["TreeViewItem.Selected.Background"] = "ABrush.Tone7.Background.Selected",
                ["TreeViewItem.Selected.Border"] = "ABrush.Tone7.Border.Selected",
                ["TreeViewItem.Selected.Inactive.Background"] = "ABrush.Tone6.Background.Selected.Inactive",
                ["TreeViewItem.Selected.Inactive.Border"] = "ABrush.Tone6.Border.Selected.Inactive",
                ["TreeViewItem.MouseOver.Background"] = "ABrush.Tone6.Background.MouseOver",
                ["TreeViewItem.MouseOver.Border"] = "ABrush.Tone6.Border.MouseOver",
                ["TreeViewItem.MouseDown.Background"] = "ABrush.Tone6.Background.MouseDown",
                ["TreeViewItem.MouseDown.Border"] = "ABrush.Tone6.Border.MouseDown",
                ["DataGrid.Static.Background"] = "ABrush.Tone4.Background.Static",
                ["DataGrid.Static.Border"] = "ABrush.Tone4.Border.Static",
                ["DataGrid.HorizontalSeparatorBrush"] = "ABrush.Tone7.Background.Static",
                ["DataGrid.VerticalSeparatorBrush"] = "ABrush.Tone7.Background.Static",
                ["DataGrid.HeaderItem.Static.Background"] = "ABrush.Tone6.Background.Static",
                ["DataGrid.HeaderItem.MouseOver.Background"] = "ABrush.Tone6.Background.MouseOver",
                ["DataGrid.HeaderItem.MouseDown.Background"] = "ABrush.Tone6.Background.MouseDown",
                ["DataGrid.RowHeader.Static.Background"] = "ABrush.Tone6.Background.Static",
                ["DataGrid.Row.Selected.Background"] = "ABrush.Tone5.Background.Selected",
                ["DataGrid.CellItem.Static.Background"] = "ABrush.Tone5.Background.Static",
                ["DataGrid.CellItem.MouseOver.Background"] = "ABrush.Tone5.Background.MouseOver",
                ["Window.Static.Background"] = "ABrush.Tone3.Background.Static",
                ["Window.Static.Border"] = "ABrush.Tone5.Border.Static",
                ["Window.Static.Foreground"] = "ABrush.Foreground.Static",
                ["Window.Static.Title.Background"] = "ABrush.Tone6.Background.Static",
            };

            this.SetInheritanceFromMap(inheritMap);
        }
    }
}
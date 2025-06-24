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
        if (manager.GetBuiltInThemes().FirstOrDefault(x => x.Name.Equals("dark", StringComparison.OrdinalIgnoreCase)) is ThemeImpl darkTheme) {
            darkTheme.SetInheritance(BaseDarkThemeControlColourMappings.Inheritance.Select(x => new KeyValuePair<string, string?>(x.Item1, x.Item2)).ToList());
        }
        
        if (manager.GetBuiltInThemes().FirstOrDefault(x => x.Name.Equals("light", StringComparison.OrdinalIgnoreCase)) is ThemeImpl lightTheme) {
            lightTheme.SetInheritance(BaseLightThemeControlColourMappings.Inheritance.Select(x => new KeyValuePair<string, string?>(x.Item1, x.Item2)).ToList());
        }

        ThemeConfigurationPage p = manager.ThemeConfigurationPage;
        foreach ((string path, string theme) item in ControlColourMappings.PathToThemeKey) {
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
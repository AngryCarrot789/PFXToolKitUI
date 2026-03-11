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

using System.Diagnostics;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Shortcuts.Keymapping;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;
using PFXToolKitUI.Shortcuts.Keymapping;
using PFXToolKitUI.Shortcuts.Usage;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Shortcuts.Avalonia;

public sealed class AvaloniaKeyMapManager : KeyMapManager {
    public const int BUTTON_WHEEL_UP = 143; // Away from the user
    public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
    public const string DEFAULT_USAGE_ID = "DEF";

    public static AvaloniaKeyMapManager AvaloniaInstance => (AvaloniaKeyMapManager) Instance ?? throw new Exception("No WPF shortcut manager available");

    static AvaloniaKeyMapManager() {
        KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
        KeyStroke.ModifierToStringProvider = (x, s) => {
            StringJoiner joiner = new StringJoiner(s);
            KeyModifiers keys = (KeyModifiers) x;
            if ((keys & KeyModifiers.Control) != 0)
                joiner.Append("Ctrl");
            if ((keys & KeyModifiers.Alt) != 0)
                joiner.Append("Alt");
            if ((keys & KeyModifiers.Shift) != 0)
                joiner.Append("Shift");
            if ((keys & KeyModifiers.Meta) != 0)
                joiner.Append(OperatingSystem.IsMacOS() ? "Cmd" : "Win");
            return joiner.ToString();
        };

        MouseStroke.MouseButtonToStringProvider = (x) => {
            switch (x) {
                case (int) MouseButton.Left:     return "LMB";
                case (int) MouseButton.Middle:   return "MMB";
                case (int) MouseButton.Right:    return "RMB";
                case (int) MouseButton.XButton1: return "X1";
                case (int) MouseButton.XButton2: return "X2";
                case BUTTON_WHEEL_UP:            return "MWU";
                case BUTTON_WHEEL_DOWN:          return "MWD";
                default:                         return $"?{x}?";
            }
        };
    }

    public AvaloniaKeyMapManager() { }

    public override KeyMapInputProcessor NewProcessor() => new AvaloniaKeyMapInputProcessor(this);

    public override string? CurrentFocusPath => UIInputManager.Instance.FocusedPath;

    public override void ReloadFromStream(Stream stream) {
        this.InvalidateShortcutCache();
        Keymap map = KeyMapSerializer.Instance.Deserialise(this, stream);
        this.Root = map.Root; // invalidates cache automatically
        try {
            this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
        }
        catch (Exception e) {
            this.InvalidateShortcutCache();
            this.Root = KeyMapGroupEntry.CreateRoot(this);
            throw new Exception("Failed to process keymap and built caches", e);
        }
    }

    protected override void OnSecondShortcutUsagesProgressed(KeyMapInputProcessor inputProcessor) {
        base.OnSecondShortcutUsagesProgressed(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, KeyMapEntry> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected override void OnShortcutUsagesCreated(KeyMapInputProcessor inputProcessor) {
        base.OnShortcutUsagesCreated(inputProcessor);
        StringJoiner joiner = new StringJoiner(", ");
        foreach (KeyValuePair<IShortcutUsage, KeyMapEntry> pair in inputProcessor.ActiveUsages) {
            joiner.Append(pair.Key.CurrentStroke.ToString());
        }

        BroadcastShortcutActivity("Waiting for next input: " + joiner);
    }

    protected override void OnCancelUsageForNoSuchNextMouseStroke(KeyMapInputProcessor inputProcessor, IShortcutUsage usage, KeyMapEntry keyMapEntry, MouseStroke stroke) {
        base.OnCancelUsageForNoSuchNextMouseStroke(inputProcessor, usage, keyMapEntry, stroke);
        BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
    }

    protected override void OnCancelUsageForNoSuchNextKeyStroke(KeyMapInputProcessor inputProcessor, IShortcutUsage usage, KeyMapEntry keyMapEntry, KeyStroke stroke) {
        base.OnCancelUsageForNoSuchNextKeyStroke(inputProcessor, usage, keyMapEntry, stroke);
        BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
    }

    protected override void OnNoSuchShortcutForMouseStroke(KeyMapInputProcessor inputProcessor, string? group, MouseStroke stroke) {
        base.OnNoSuchShortcutForMouseStroke(inputProcessor, group, stroke);
        if (Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
        }
    }

    protected override void OnNoSuchShortcutForKeyStroke(KeyMapInputProcessor inputProcessor, string? group, KeyStroke stroke) {
        base.OnNoSuchShortcutForKeyStroke(inputProcessor, group, stroke);
        if (stroke.IsKeyDown && Debugger.IsAttached) {
            BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
        }
    }

    protected override bool OnShortcutActivatedOverride(KeyMapInputProcessor inputProcessor, KeyMapEntry keyMapEntry) {
        string str;

        if (Debugger.IsAttached) {
            str = $"shortcut command: {keyMapEntry} -> {(string.IsNullOrWhiteSpace(keyMapEntry.CommandId) ? "<none>" : keyMapEntry.CommandId)}";
        }
        else {
            str = $"shortcut command: {(string.IsNullOrWhiteSpace(keyMapEntry.CommandId) ? "<none>" : keyMapEntry.CommandId)}";
        }

        BroadcastShortcutActivity($"Activating {str}...");
        bool result = base.OnShortcutActivatedOverride(inputProcessor, keyMapEntry);
        BroadcastShortcutActivity($"Activated {str}!");
        return result;
    }

    private static void BroadcastShortcutActivity(string msg) {
    }
}
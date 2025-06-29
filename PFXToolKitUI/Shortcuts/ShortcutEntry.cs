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

using System.Text;

namespace PFXToolKitUI.Shortcuts;

public delegate void ShortcutEntryShortcutChangedEventHandler(ShortcutEntry sender, IShortcut oldShortcut, IShortcut newShortcut);

/// <summary>
/// A class used to store a reference to a <see cref="Shortcut"/> and its
/// owning <see cref="ShortcutGroupEntry"/>, and also other shortcut data
/// </summary>
public sealed class ShortcutEntry : IKeyMapEntry {
    private IShortcut shortcut;

    public ShortcutManager? Manager => this.Parent?.Manager;

    public ShortcutGroupEntry? Parent { get; }

    /// <summary>
    /// The name of the shortcut. This will not be null or empty and will not consist of only whitespaces;
    /// this is always some sort of valid string (even if only 1 character)
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// This group's display name, which is a more readable and user-friendly version of <see cref="Name"/>
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// A description of what the shortcut is used for
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this shortcut can run globally (across the entire window). When false, the parent group must be focused in order for this to be fun
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// Whether this shortcut can be targeted when the focused group is deeper in the focus tree but somewhere up the tree is a group which contains this shortcut
    /// <para>
    /// For example, this shortcut's path is <c>app/main-window/OpenFile</c> and the focused group is <c>app/main-window/content/text-editor</c>, if no
    /// shortcuts in that specific group with this shortcut's input combination exists and no other shortcuts could be found via inheritance apart from
    /// this instance, then this instance can be targeted. The first shortcut to be found via inheritance is the one that gets targeted; others are ignored.
    /// This means the order of shortcuts in the XML document can play a part in which shortcut is finally executed
    /// </para>
    /// </summary>
    public bool IsInherited { get; set; }

    /// <summary>
    /// The ID for an optional command that this shortcut will trigger when activated
    /// </summary>
    public string? CommandId { get; set; }

    /// <summary>
    /// This shortcut's full path (the parent's path if it's not the root object, and this shortcut's name). Will not be null and will always containing valid characters
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Whether this shortcut is triggered by non-repeated key strokes, repeated key strokes, or if repeats are ignored
    /// </summary>
    public RepeatMode RepeatMode { get; set; }

    /// <summary>
    /// The shortcut itself. Will not be null
    /// </summary>
    public IShortcut Shortcut {
        get => this.shortcut;
        set {
            IShortcut oldShortcut = this.shortcut;
            if (ReferenceEquals(oldShortcut, value)) {
                return;
            }

            this.shortcut = value ?? throw new ArgumentNullException(nameof(value), "Shortcut cannot be null");
            this.ShortcutChanged?.Invoke(this, oldShortcut, value);
        }
    }

    public event ShortcutEntryShortcutChangedEventHandler? ShortcutChanged;

    public ShortcutEntry(ShortcutGroupEntry groupEntry, string name, IShortcut shortcut, bool isGlobal = false) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null, empty, or consist of only whitespaces");
        this.Parent = groupEntry ?? throw new ArgumentNullException(nameof(groupEntry), "Collection cannot be null");
        this.Name = name;
        this.shortcut = shortcut ?? throw new ArgumentNullException(nameof(shortcut));
        this.FullPath = groupEntry.GetPathForName(name);
        this.IsGlobal = isGlobal;
        this.IsInherited = true;
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(nameof(ShortcutEntry)).Append(" (").Append(this.Shortcut.IsEmpty ? "Empty/No Shortcut" : this.Shortcut.ToString()).Append(" -> ").Append(this.FullPath);
        if (!string.IsNullOrWhiteSpace(this.Description)) {
            sb.Append(" (").Append(this.Description).Append(")");
        }

        return sb.Append(')').ToString();
    }
}
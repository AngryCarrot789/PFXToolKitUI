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

namespace PFXToolKitUI.Avalonia.Themes.Configurations;

public interface IThemeConfigEntryTreeOrNode {
    ThemeConfigTreeView? ThemeConfigTree { get; }

    ThemeConfigTreeViewItem? ParentNode { get; }

    IThemeTreeEntry? Entry { get; }

    void InsertGroup(ThemeConfigEntryGroup entry, int index);

    void InsertEntry(ThemeConfigEntry entry, int index);

    void RemoveGroup(int index, bool canCache = true);

    void RemoveEntry(int index, bool canCache = true);
}
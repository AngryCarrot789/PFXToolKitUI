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

using PFXToolKitUI.Interactivity;
using PFXToolKitUI.PropertyEditing;
using PFXToolKitUI.PropertyEditing.Core;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

public static class TestPropertyEditor {
    public static PropertyEditor DummyEditor { get; } = new PropertyEditor();
    public static BasePropertyEditorGroup DummyGroup { get; }

    static TestPropertyEditor() {
        DummyEditor.Root.AddItem(new SimplePropertyEditorGroup(typeof(TestObject)) {
            DisplayName = "Group 1"
        });

        SimplePropertyEditorGroup group = new SimplePropertyEditorGroup(typeof(TestObject)) {
            DisplayName = "Group 2", IsExpanded = true
        };

        group.AddItem(new DisplayNamePropertyEditorSlot());
        DummyEditor.Root.AddItem(group);
        DummyEditor.Root.SetupHierarchyState([new TestObject()]);
        DummyGroup = DummyEditor.Root;
    }

    public class TestObject : IDisplayName {
        public string? DisplayName { get; set; }

        public event DisplayNameChangedEventHandler? DisplayNameChanged;
    }
}
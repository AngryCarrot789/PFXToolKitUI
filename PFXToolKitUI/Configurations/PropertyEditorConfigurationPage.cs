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

using PFXToolKitUI.PropertyEditing;

namespace PFXToolKitUI.Configurations;

/// <summary>
/// A configuration page that consists entirely of a property editor
/// </summary>
public abstract class PropertyEditorConfigurationPage : ConfigurationPage {
    public PropertyEditor PropertyEditor { get; }

    public PropertyEditorConfigurationPage() : this(new PropertyEditor()) {
    }

    public PropertyEditorConfigurationPage(PropertyEditor propertyEditor) {
        ArgumentNullException.ThrowIfNull(propertyEditor);
        this.PropertyEditor = propertyEditor;
    }
}
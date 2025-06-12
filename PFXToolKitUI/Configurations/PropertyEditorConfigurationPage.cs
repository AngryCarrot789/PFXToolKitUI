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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing;

namespace PFXToolKitUI.Configurations;

/// <summary>
/// A configuration page that consists entirely of a property editor
/// </summary>
public abstract class PropertyEditorConfigurationPage : ConfigurationPage, ITransferableData {
    public PropertyEditor PropertyEditor { get; }

    public TransferableData TransferableData { get; }

    public PropertyEditorConfigurationPage() : this(new PropertyEditor()) {
    }

    public PropertyEditorConfigurationPage(PropertyEditor propertyEditor) {
        ArgumentNullException.ThrowIfNull(propertyEditor);
        this.TransferableData = new TransferableData(this);
        this.PropertyEditor = propertyEditor;
    }

    private static void MarkModifiedHandler(DataParameter parameter, ITransferableData owner) => ((PropertyEditorConfigurationPage) owner).IsModified = true;

    protected static void AffectsModifiedState(DataParameter parameter) {
        parameter.ValueChanged += MarkModifiedHandler;
    }

    protected static void AffectsModifiedState(DataParameter p1, DataParameter p2) {
        p1.ValueChanged += MarkModifiedHandler;
        p2.ValueChanged += MarkModifiedHandler;
    }

    protected static void AffectsModifiedState(DataParameter p1, DataParameter p2, DataParameter p3) {
        p1.ValueChanged += MarkModifiedHandler;
        p2.ValueChanged += MarkModifiedHandler;
        p3.ValueChanged += MarkModifiedHandler;
    }

    protected static void AffectsModifiedState(params DataParameter[] parameters) {
        foreach (DataParameter p in parameters) {
            p.ValueChanged += MarkModifiedHandler;
        }
    }
}
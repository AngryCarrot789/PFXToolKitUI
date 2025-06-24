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

using Avalonia;
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

public static class ExpanderHelper {
    public static readonly AttachedProperty<bool> IsPointerOverExProperty = AvaloniaProperty.RegisterAttached<Expander, bool>("IsPointerOverEx", typeof(ExpanderHelper));

    public static void SetIsPointerOverEx(Expander obj, bool value) => obj.SetValue(IsPointerOverExProperty, value);
    public static bool GetIsPointerOverEx(Expander obj) => obj.GetValue(IsPointerOverExProperty);
}
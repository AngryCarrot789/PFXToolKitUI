// 
// Copyright (c) 2024-2025 REghZy
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

using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.Utils.Accessing;

namespace PFXToolKitUI.Avalonia.Bindings;

public static class Binders {
    public static AvaloniaPropertyToEventPropertyAccessorBinder<TModel, TValue> AccessorAEDPLinq<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, string propertyOrFieldName) where TModel : class {
        // Uses cached accessor
        return AccessorAEDP<TModel, TValue>(property, eventName, ValueAccessors.LinqExpression<TValue>(typeof(TModel), propertyOrFieldName, true));
    }

    public static AvaloniaPropertyToEventPropertyAccessorBinder<TModel, TValue> AccessorAEDPFastStartup<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, string propertyOrFieldName) where TModel : class {
        // Uses cached accessor
        return AccessorAEDP<TModel, TValue>(property, eventName, ValueAccessors.FastStartupAccessor<TValue>(typeof(TModel), propertyOrFieldName));
    }

    public static AvaloniaPropertyToEventPropertyAccessorBinder<TModel, TValue> AccessorAEDP<TModel, TValue>(AvaloniaProperty<TValue> property, string eventName, ValueAccessor<TValue> accessor) where TModel : class {
        return new AvaloniaPropertyToEventPropertyAccessorBinder<TModel, TValue>(property, eventName, accessor);
    }

    public static AvaloniaPropertyToEventPropertyBinder<TModel> AutoUpdateAndEvent<TModel>(AvaloniaProperty property, string eventName, Action<IBinder<TModel>>? updateControl, Action<IBinder<TModel>>? updateModel) where TModel : class {
        return new AvaloniaPropertyToEventPropertyBinder<TModel>(property, eventName, updateControl, updateModel);
    }

    public static void AttachControls<TModel>(Control control, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.AttachControl(control);
        }
    }

    public static void AttachModels<TModel>(TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.AttachModel(model);
        }
    }

    public static void DetachModels<TModel>(params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.DetachModel();
        }
    }
}
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

using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Bindings;

public static class Binders {
    public static void Attach<TModel>(Control control, TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.Attach(control, model);
    }
    
    public static void Attach<TModel>(Control control, TModel model, params ReadOnlySpan<IBinder<TModel>> binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.Attach(control, model);
    }
    
    public static void AttachControls<TModel>(Control control, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.AttachControl(control);
    }
    
    public static void AttachControls<TModel>(Control control, params ReadOnlySpan<IBinder<TModel>> binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.AttachControl(control);
    }
    
    public static void AttachModels<TModel>(TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.AttachModel(model);
    }
    
    public static void AttachModels<TModel>(TModel model, params ReadOnlySpan<IBinder<TModel>> binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.AttachModel(model);
    }
    
    public static void Detach(params IBinder[] binders) {
        foreach (IBinder b in binders)
            b.Detach();
    }
    
    public static void Detach(params ReadOnlySpan<IBinder> binders) {
        foreach (IBinder b in binders)
            b.Detach();
    }
    
    public static void DetachControls(params IBinder[] binders) {
        foreach (IBinder b in binders)
            b.DetachControl();
    }
    
    public static void DetachControls(params ReadOnlySpan<IBinder> binders) {
        foreach (IBinder b in binders)
            b.DetachControl();
    }
    
    public static void DetachModels(params IBinder[] binders) {
        foreach (IBinder b in binders)
            b.DetachModel();
    }
    
    public static void DetachModels(params ReadOnlySpan<IBinder> binders) {
        foreach (IBinder b in binders)
            b.DetachModel();
    }
}
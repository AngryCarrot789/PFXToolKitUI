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
    public static void AttachControls<TModel>(Control control, IBinder<TModel> binder1) where TModel : class {
        binder1.AttachControl(control);
    }
    
    public static void AttachControls<TModel>(Control control, IBinder<TModel> binder1, IBinder<TModel> binder2) where TModel : class {
        binder1.AttachControl(control);
        binder2.AttachControl(control);
    }
    
    public static void AttachControls<TModel>(Control control, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3) where TModel : class {
        binder1.AttachControl(control);
        binder2.AttachControl(control);
        binder3.AttachControl(control);
    }
    
    public static void AttachControls<TModel>(Control control, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3, IBinder<TModel> binder4) where TModel : class {
        binder1.AttachControl(control);
        binder2.AttachControl(control);
        binder3.AttachControl(control);
        binder4.AttachControl(control);
    }
    
    public static void AttachControls<TModel>(Control control, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.AttachControl(control);
    }
    
    public static void AttachModels<TModel>(TModel model, IBinder<TModel> binder1) where TModel : class {
        binder1.AttachModel(model);
    }
    
    public static void AttachModels<TModel>(TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2) where TModel : class {
        binder1.AttachModel(model);
        binder2.AttachModel(model);
    }
    
    public static void AttachModels<TModel>(TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3) where TModel : class {
        binder1.AttachModel(model);
        binder2.AttachModel(model);
        binder3.AttachModel(model);
    }
    
    public static void AttachModels<TModel>(TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3, IBinder<TModel> binder4) where TModel : class {
        binder1.AttachModel(model);
        binder2.AttachModel(model);
        binder3.AttachModel(model);
        binder4.AttachModel(model);
    }

    public static void AttachModels<TModel>(TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.AttachModel(model);
        }
    }
    
    public static void Attach<TModel>(Control control, TModel model, IBinder<TModel> binder1) where TModel : class {
        binder1.Attach(control, model);
    }
    
    public static void Attach<TModel>(Control control, TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2) where TModel : class {
        binder1.Attach(control, model);
        binder2.Attach(control, model);
    }
    
    public static void Attach<TModel>(Control control, TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3) where TModel : class {
        binder1.Attach(control, model);
        binder2.Attach(control, model);
        binder3.Attach(control, model);
    }
    
    public static void Attach<TModel>(Control control, TModel model, IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3, IBinder<TModel> binder4) where TModel : class {
        binder1.Attach(control, model);
        binder2.Attach(control, model);
        binder3.Attach(control, model);
        binder4.Attach(control, model);
    }
    
    public static void Attach<TModel>(Control control, TModel model, params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders)
            b.Attach(control, model);
    }
    
    public static void DetachModels<TModel>(IBinder<TModel> binder1) where TModel : class {
        binder1.DetachModel();
    }
    
    public static void DetachModels<TModel>(IBinder<TModel> binder1, IBinder<TModel> binder2) where TModel : class {
        binder1.DetachModel();
        binder2.DetachModel();
    }
    
    public static void DetachModels<TModel>(IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3) where TModel : class {
        binder1.DetachModel();
        binder2.DetachModel();
        binder3.DetachModel();
    }
    
    public static void DetachModels<TModel>(IBinder<TModel> binder1, IBinder<TModel> binder2, IBinder<TModel> binder3, IBinder<TModel> binder4) where TModel : class {
        binder1.DetachModel();
        binder2.DetachModel();
        binder3.DetachModel();
        binder4.DetachModel();
    }

    public static void DetachModels<TModel>(params IBinder<TModel>[] binders) where TModel : class {
        foreach (IBinder<TModel> b in binders) {
            b.DetachModel();
        }
    }
}
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
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// The base class for general binders, which are used to create a "bind" between model and event.
/// <para>
/// The typical behaviour is to add an event handler in user code and call <see cref="UpdateControl"/>
/// which will cause <see cref="UpdateControlOverride"/> to be called, allowing you to update the control's value. An internal bool
/// will stop a stack overflow when the control's value ends up calling <see cref="UpdateModel"/> which ignores
/// the call if that bool is true
/// </para>
/// <para>
/// Then, an event handler should be added for the control and it should call <see cref="UpdateModel"/>, which causes
/// <see cref="UpdateModelOverride"/>. As before, an internal bool stops a stack overflow when the value changes ends up
/// calling <see cref="UpdateControl"/>
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of model</typeparam>
public abstract class BaseBinder<TModel> : IBinder<TModel> where TModel : class {
    protected Control? myControl;
    protected TModel? myModel;
    private int reentrancyUpdateControl;

    public Control Control => this.myControl ?? throw new InvalidOperationException("No control is attached");

    public TModel Model => this.myModel ?? throw new InvalidOperationException("No model is attached");

    Control? IBinder.Debug_Control => this.myControl;

    object? IBinder.Debug_Model => this.myModel;

    public bool HasControl => this.myControl != null;

    public bool HasModel => this.myModel != null;

    public bool IsFullyAttached { get; private set; }

    public bool IsUpdatingControl => this.reentrancyUpdateControl > 0;

    /// <summary>
    /// A unique name for this instance to identify it while debugging
    /// </summary>
    public string? DebugName { get; set; }

    public event BinderEventHandler<TModel>? UpdateControlWithoutModel;
    public event BinderEventHandler<TModel>? ControlUpdated;
    public event BinderEventHandler<TModel>? ModelUpdated;
    public event BinderModelChangedEventHandler<TModel>? ModelChanged;
    public event BinderControlChangedEventHandler<TModel>? ControlChanged;

    protected BaseBinder() {
    }

    public void UpdateControl() {
        if (this.IsFullyAttached) {
            this.UpdateControlInternal(false);
        }
        else if (this.myControl != null) {
            this.UpdateControlWithoutModel?.Invoke(this);
        }
    }

    private void UpdateControlInternal(bool isFirstUpdateControl) {
        try {
            // We don't check if we are updating the control, just in case the model
            // decided to coerce its own value which is different from the UI control

            this.reentrancyUpdateControl++;
            this.UpdateControlOverride(isFirstUpdateControl);
            this.ControlUpdated?.Invoke(this);
        }
        finally {
            this.reentrancyUpdateControl--;
        }
    }

    public void UpdateModel() {
        if (!this.IsUpdatingControl && this.IsFullyAttached) {
            this.UpdateModelOverride();
            this.ModelUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// This method should be overridden to update the model's value using the element's value
    /// </summary>
    protected abstract void UpdateModelOverride();

    /// <summary>
    /// This method should be overridden to update the control's value using the model's value
    /// </summary>
    /// <param name="hasJustAttached">True when <see cref="IsFullyAttached"/> became true in the call frame, false otherwise</param>
    protected abstract void UpdateControlOverride(bool hasJustAttached);

    public void Attach(Control control, TModel model) {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(control);
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myControl != null)
            throw new InvalidOperationException("A control is already attached");
        if (this.myModel != null)
            throw new InvalidOperationException("A model is already attached");

        this.CheckAttachControl(control);
        this.CheckAttachModel(model);

        this.myModel = model;
        this.ModelChanged?.Invoke(this, null, model);

        this.myControl = control;
        this.ControlChanged?.Invoke(this, null, control);

        this.AttachInternal();
    }

    public void AttachControl(Control control) {
        ArgumentNullException.ThrowIfNull(control);
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myControl != null)
            throw new InvalidOperationException("A control is already attached");

        this.CheckAttachControl(control);

        this.myControl = control;
        this.ControlChanged?.Invoke(this, null, control);
        if (this.myModel != null) {
            this.AttachInternal();
        }
        else {
            this.UpdateControlWithoutModel?.Invoke(this);
        }
    }

    public void AttachModel(TModel model) {
        ArgumentNullException.ThrowIfNull(model);
        if (this.IsFullyAttached)
            throw new Exception("Already fully attached");
        if (this.myModel != null)
            throw new InvalidOperationException("A model is already attached");

        this.CheckAttachModel(model);

        this.myModel = model;
        this.ModelChanged?.Invoke(this, null, model);
        
        if (this.myControl != null) {
            this.AttachInternal();
        }
    }

    public void Detach() {
        if (!this.IsFullyAttached)
            throw new Exception("Not fully attached");

        TModel? oldModel = this.myModel;
        Control? oldControl = this.myControl;
        Debug.Assert(oldControl != null && oldModel != null);

        this.IsFullyAttached = false;
        this.OnDetached();

        this.myModel = null;
        this.ModelChanged?.Invoke(this, oldModel, null);

        this.myControl = null;
        this.ControlChanged?.Invoke(this, oldControl, null);
    }

    public void DetachControl() {
        Control? oldControl = this.myControl;
        if (oldControl == null)
            throw new InvalidOperationException("No control is attached");

        this.TryDetachInternal();
        this.myControl = null;
        this.ControlChanged?.Invoke(this, oldControl, null);
    }

    public void DetachModel() {
        if (this.myModel == null)
            throw new InvalidOperationException("No model is attached");

        this.DetachModelInternal();
        if (this.myControl != null) {
            this.UpdateControlWithoutModel?.Invoke(this);
        }
    }

    private void DetachModelInternal() {
        this.TryDetachInternal();
        TModel? oldModel = this.myModel;
        Debug.Assert(oldModel != null);
        
        this.myModel = null;
        this.ModelChanged?.Invoke(this, oldModel, null);
    }

    public void SwitchControl(Control? newControl) {
        if (this.myControl != null)
            this.DetachControl();

        if (newControl != null)
            this.AttachControl(newControl);
    }

    public void SwitchModel(TModel? newModel) {
        if (this.myModel != null)
            this.DetachModelInternal();

        if (newModel != null) {
            this.AttachModel(newModel);
        }
        else if (this.myControl != null) {
            this.UpdateControlWithoutModel?.Invoke(this);
        }
    }

    /// <summary>
    /// A method that can be overridden to throw an exception if the control cannot be attached for whatever reason
    /// </summary>
    /// <param name="control">The control being attached</param>
    protected virtual void CheckAttachControl(Control control) {
    }

    /// <summary>
    /// A method that can be overridden to throw an exception if the model cannot be attached for whatever reason
    /// </summary>
    /// <param name="model">The model being attached</param>
    protected virtual void CheckAttachModel(TModel model) {
    }

    protected abstract void OnAttached();

    protected abstract void OnDetached();

    private void AttachInternal() {
        this.IsFullyAttached = true;
        this.OnAttached();
        this.UpdateControlInternal(true);
    }

    private void TryDetachInternal() {
        if (this.IsFullyAttached) {
            this.IsFullyAttached = false;
            this.OnDetached();
        }
    }
}
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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Utils;

/// <summary>
/// A class which helps with managing an attached and detached state based
/// on a mutable model property and the mutable "attached to visual tree" state
/// </summary>
public sealed class AttachedModelHelper<TModel> : IDisposable {
    private readonly Control control;
    private readonly Action<TModel, bool> isAttachedChangedHandler;
    
    private Optional<TModel> model;
    private bool isAttached;

    public Optional<TModel> Model {
        get => this.model;
        set {
            Optional<TModel> oldValue = this.model;
            if (!oldValue.Equals(value)) {
                if (oldValue.HasValue && this.isAttached)
                    this.isAttachedChangedHandler(oldValue.Value, false);

                this.model = value;
                if (value.HasValue && this.isAttached)
                    this.isAttachedChangedHandler(value.Value, true);
            }
        }
    }

    private bool IsAttached {
        get => this.isAttached;
        set {
            bool oldIsAttached = this.isAttached;
            if (value != oldIsAttached) {
                if (oldIsAttached && this.model.HasValue)
                    this.isAttachedChangedHandler(this.model.Value, false);

                if ((this.isAttached = value) && this.model.HasValue)
                    this.isAttachedChangedHandler(this.model.Value, true);
            }
        }
    }

    public AttachedModelHelper(Control control, Action<TModel, bool> isAttachedChangedHandler) {
        this.control = control;
        this.isAttachedChangedHandler = isAttachedChangedHandler;
        
        this.control.AttachedToVisualTree += this.OnAttachedToVisualTree;
        this.control.DetachedFromVisualTree += this.OnDetachedFromVisualTree;
    }

    public bool TryGetModel([MaybeNullWhen(false)] out TModel theModel) {
        if (this.isAttached && this.model.HasValue) {
            theModel = this.model.Value;
            return true;
        }

        theModel = default;
        return false;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.IsAttached = true;
    }
    
    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        this.IsAttached = false;
    }

    public void Dispose() {
        this.control.AttachedToVisualTree -= this.OnAttachedToVisualTree;
        this.control.DetachedFromVisualTree -= this.OnDetachedFromVisualTree;
        this.IsAttached = false;
        this.Model = default;
    }
}
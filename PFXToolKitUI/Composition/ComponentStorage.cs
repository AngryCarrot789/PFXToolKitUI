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

namespace PFXToolKitUI.Composition;

public sealed class ComponentStorage {
    private readonly Dictionary<Type, object> myComponents = new();
    private readonly IComponentManager componentManager;

    public ComponentStorage(IComponentManager componentManager) {
        this.componentManager = componentManager;
    }

    public object GetComponent(Type serviceType) {
        object? value = this.myComponents.GetValueOrDefault(serviceType);
        if (value != null)
            return value;
        if (this.componentManager.ParentComponentManager is IComponentManager parent)
            return parent.GetComponent(serviceType);

        throw new Exception("Component not found");
    }

    public T GetComponent<T>() where T : class {
        object? value = this.myComponents.GetValueOrDefault(typeof(T));
        if (value != null)
            return (T) value;
        if (this.componentManager.ParentComponentManager is IComponentManager parent)
            return parent.GetComponent<T>();

        throw new Exception("Component not found");
    }

    public bool TryGetComponent(Type serviceType, [NotNullWhen(true)] out object? component) {
        component = this.myComponents.GetValueOrDefault(serviceType);
        if (component != null)
            return true;
        if (this.componentManager.ParentComponentManager is IComponentManager parent)
            return parent.TryGetComponent(serviceType, out component);

        component = null;
        return false;
    }

    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class {
        component = this.myComponents.GetValueOrDefault(typeof(T)) as T;
        if (component != null)
            return true;
        if (this.componentManager.ParentComponentManager is IComponentManager parent)
            return parent.TryGetComponent(out component);

        component = null;
        return false;
    }

    public void AddService<TComponent>(TComponent component) where TComponent : class {
        this.myComponents[typeof(TComponent)] = component;
    }

    public T GetOrCreateComponent<T>(Func<IComponentManager, T> factory) where T : class {
        if (this.TryGetComponent(out T? component)) {
            return component;
        }

        T newValue = factory(this.componentManager);
        this.myComponents[typeof(T)] = newValue;
        return newValue;
    }
}
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
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Composition;

public sealed class ComponentStorage {
    private readonly Dictionary<Type, ComponentEntry> myComponents = new();
    private readonly IComponentManager componentManager;

    public ComponentStorage(IComponentManager componentManager) {
        this.componentManager = componentManager;
    }

    public bool HasComponent(Type componentType) {
        ArgumentNullException.ThrowIfNull(componentType);
        return this.TryGetOrInitializeComponent(componentType, out _, false);
    }
    
    public bool HasComponent<T>() {
        return this.TryGetOrInitializeComponent(typeof(T), out _, false);
    }

    public object GetComponent(Type componentType) {
        ArgumentNullException.ThrowIfNull(componentType);
        if (!this.TryGetOrInitializeComponent(componentType, out object? component))
            throw new Exception($"No component registered with type: {componentType}");
        return component!;
    }

    public T GetComponent<T>() where T : class => (T) this.GetComponent(typeof(T));

    public bool TryGetComponent(Type componentType, [NotNullWhen(true)] out object? component) {
        return this.TryGetOrInitializeComponent(componentType, out component);
    }

    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class {
        bool result = this.TryGetOrInitializeComponent(typeof(T), out object? comp);
        component = (T?) comp;
        return result;
    }

    public void AddComponent<TComponent>(TComponent component) where TComponent : class {
        if (this.myComponents.ContainsKey(typeof(TComponent)))
            throw new InvalidOperationException("Component type already registered: " + typeof(TComponent));

        this.myComponents[typeof(TComponent)] = new ComponentEntry(false, component);
    }

    public void AddLazyComponent<TComponent>(Func<IComponentManager, TComponent> factory) where TComponent : class {
        ArgumentNullException.ThrowIfNull(factory);
        if (this.myComponents.ContainsKey(typeof(TComponent)))
            throw new InvalidOperationException("Component type already registered: " + typeof(TComponent));

        this.myComponents[typeof(TComponent)] = new ComponentEntry(true, new Func<IComponentManager, object>(factory));
    }

    public T GetOrCreateComponent<T>(Func<IComponentManager, T> factory) where T : class {
        if (this.TryGetComponent(out T? component)) {
            return component;
        }

        T newValue = factory(this.componentManager);
        this.myComponents[typeof(T)] = new ComponentEntry(false, newValue);
        return newValue;
    }
    
    private bool TryGetOrInitializeComponent(Type componentType, out object? component, bool initializeIfLazy = true) {
        ArgumentNullException.ThrowIfNull(componentType);
        if (!this.myComponents.TryGetValue(componentType, out ComponentEntry entry)) {
            component = null;
            return false;
        }

        if (entry.isLazyEntry) {
            if (!initializeIfLazy) {
                component = null;
                return true;
            }

            component = ((Func<IComponentManager, object>) entry.value)(this.componentManager);
            Debug.Assert(componentType.IsInstanceOfType(component), "New component instance is incompatible with target type");
            this.myComponents[componentType] = new ComponentEntry(false, component);
        }
        else {
            component = entry.value;
        }

        return true;
    }

    private readonly struct ComponentEntry(bool isLazyEntry, object value) {
        public readonly bool isLazyEntry = isLazyEntry;
        public readonly object value = value;
    }
}
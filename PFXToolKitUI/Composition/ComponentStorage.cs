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

    /// <summary>
    /// Checks if we have a component of the specified type
    /// </summary>
    /// <param name="componentType">The type of component to check</param>
    /// <returns>True if we have a component of the specified type</returns>
    public bool HasComponent(Type componentType) {
        ArgumentNullException.ThrowIfNull(componentType);
        return this.TryGetOrInitializeComponent(componentType, out _, false);
    }
    
    /// <summary>
    /// Checks if we have a component of the specified type
    /// </summary>
    /// <typeparam name="T">The type of component to check</typeparam>
    /// <returns>True if we have a component of the specified type</returns>
    public bool HasComponent<T>() {
        return this.TryGetOrInitializeComponent(typeof(T), out _, false);
    }

    /// <summary>
    /// Gets a component of the specified type
    /// </summary>
    /// <param name="componentType">The type of component to get</param>
    /// <returns>The component</returns>
    /// <exception cref="Exception">No such component of the specified type</exception>
    public object GetComponent(Type componentType) {
        ArgumentNullException.ThrowIfNull(componentType);
        if (!this.TryGetOrInitializeComponent(componentType, out object? component))
            throw new Exception($"No component registered with type: {componentType}");
        return component!;
    }

    /// <summary>
    /// Gets a component of the specified type
    /// </summary>
    /// <typeparam name="T">The type of component to get</typeparam>
    /// <returns>The component</returns>
    /// <exception cref="Exception">No such component of the specified type</exception>
    public T GetComponent<T>() where T : class => (T) this.GetComponent(typeof(T));

    /// <summary>
    /// Tries to get a component of the specified type
    /// </summary>
    /// <param name="componentType">The type of component to get</param>
    /// <param name="component">The component, or null, if we didn't have it</param>
    /// <returns>True if we have a component of the specified type</returns>
    public bool TryGetComponent(Type componentType, [NotNullWhen(true)] out object? component) {
        return this.TryGetOrInitializeComponent(componentType, out component);
    }

    /// <summary>
    /// Tries to get a component of the specified type
    /// </summary>
    /// <param name="component">The component, or null, if we didn't have it</param>
    /// <typeparam name="T">The type of component to get</typeparam>
    /// <returns>True if we have a component of the specified type</returns>
    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class {
        bool result = this.TryGetOrInitializeComponent(typeof(T), out object? comp);
        component = (T?) comp;
        return result;
    }
    
    /// <summary>
    /// Either gets an existing component of the specified type, or creates it and registers it using type <see cref="T"/>
    /// </summary>
    /// <param name="factory">The factory used to create an instance of the component</param>
    /// <typeparam name="T">The type of component to get or register the new component instance with</typeparam>
    /// <returns>The component, either pre-existing or newly created</returns>
    public T GetOrCreateComponent<T>(Func<IComponentManager, T> factory) where T : class {
        if (this.TryGetComponent(out T? component)) {
            return component;
        }

        T newValue = factory(this.componentManager);
        this.myComponents[typeof(T)] = new ComponentEntry(false, newValue);
        return newValue;
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
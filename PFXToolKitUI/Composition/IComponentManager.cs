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

/// <summary>
/// An object that stores composite objects (components)
/// </summary>
public interface IComponentManager {
    /// <summary>
    /// Gets the component container object
    /// </summary>
    ComponentStorage ComponentStorage { get; }

    /// <summary>
    /// Checks if our <see cref="ComponentStorage"/> contains a component of the specified type
    /// </summary>
    /// <param name="componentType">The type of component to check</param>
    /// <returns>True if we have a component of the specified type</returns>
    sealed bool HasComponent(Type componentType) => this.ComponentStorage.HasComponent(componentType);
    
    /// <summary>
    /// Checks if our <see cref="ComponentStorage"/> contains a component of the specified type
    /// </summary>
    /// <typeparam name="T">The type of component to check</typeparam>
    /// <returns>True if we have a component of the specified type</returns>
    sealed bool HasComponent<T>() => this.ComponentStorage.HasComponent<T>();

    /// <summary>
    /// Gets a component of the specified type from our <see cref="ComponentStorage"/>
    /// </summary>
    /// <param name="componentType">The type of component to get</param>
    /// <returns>The component</returns>
    /// <exception cref="Exception">No such component of the specified type</exception>
    sealed object GetComponent(Type componentType) => this.ComponentStorage.GetComponent(componentType);

    /// <summary>
    /// Gets a component of the specified type
    /// </summary>
    /// <typeparam name="T">The type of component to get</typeparam>
    /// <returns>The component</returns>
    /// <exception cref="Exception">No such component of the specified type</exception>
    sealed T GetComponent<T>() where T : class => this.ComponentStorage.GetComponent<T>();

    /// <summary>
    /// Tries to get a component of the specified type
    /// </summary>
    /// <param name="componentType">The type of component to get</param>
    /// <param name="component">The component, or null, if we didn't have it</param>
    /// <returns>True if we have a component of the specified type</returns>
    sealed bool TryGetComponent(Type componentType, [NotNullWhen(true)] out object? component) => this.ComponentStorage.TryGetComponent(componentType, out component);

    /// <summary>
    /// Tries to get a component of the specified type
    /// </summary>
    /// <param name="component">The component, or null, if we didn't have it</param>
    /// <typeparam name="T">The type of component to get</typeparam>
    /// <returns>True if we have a component of the specified type</returns>
    sealed bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class => this.ComponentStorage.TryGetComponent(out component);

    /// <summary>
    /// Either gets an existing component of the specified type, or creates it and registers it using
    /// type <see cref="T"/>, from our <see cref="ComponentStorage"/>
    /// </summary>
    /// <param name="factory">The factory used to create an instance of the component</param>
    /// <typeparam name="T">The type of component to get or register the new component instance with</typeparam>
    /// <returns>The component, either pre-existing or newly created</returns>
    sealed T GetOrCreateComponent<T>(Func<IComponentManager, T> factory) where T : class => this.ComponentStorage.GetOrCreateComponent(factory);
}
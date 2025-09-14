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
/// An interface for an object that has composite components associated with it
/// </summary>
public interface IComponentManager {
    /// <summary>
    /// Gets the component container object
    /// </summary>
    ComponentStorage ComponentStorage { get; }

    sealed bool HasComponent(Type componentType) => this.ComponentStorage.HasComponent(componentType);
    
    sealed bool HasComponent<T>() => this.ComponentStorage.HasComponent<T>();

    sealed object GetComponent(Type componentType) => this.ComponentStorage.GetComponent(componentType);

    sealed T GetComponent<T>() where T : class => this.ComponentStorage.GetComponent<T>();

    sealed bool TryGetComponent(Type componentType, [NotNullWhen(true)] out object? component) => this.ComponentStorage.TryGetComponent(componentType, out component);

    sealed bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class => this.ComponentStorage.TryGetComponent(out component);

    sealed T GetOrCreateComponent<T>(Func<IComponentManager, T> factory) where T : class => this.ComponentStorage.GetOrCreateComponent(factory);
}
// 
// Copyright (c) 2026-2026 REghZy
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
using PFXToolKitUI.Composition;

namespace PFXToolKitUI.Interactivity.Windowing;

public sealed class TopLevelDataMap {
    private readonly Dictionary<TopLevelIdentifier, Dictionary<Type, object>> dataMap;

    public IComponentManager ComponentManager { get; }

    private TopLevelDataMap(IComponentManager componentManager) {
        this.dataMap = new Dictionary<TopLevelIdentifier, Dictionary<Type, object>>();
        this.ComponentManager = componentManager;
    }

    private Dictionary<Type, object> GetOrCreateTopLevelMap(TopLevelIdentifier topLevelIdentifier) {
        if (!this.dataMap.TryGetValue(topLevelIdentifier, out Dictionary<Type, object>? map)) {
            this.dataMap[topLevelIdentifier] = map = new Dictionary<Type, object>();
        }

        return map;
    }

    public bool TryGetValue<TValue>(TopLevelIdentifier topLevel, [NotNullWhen(true)] out TValue? value) where TValue : class {
        if (!this.dataMap.TryGetValue(topLevel, out Dictionary<Type, object>? data) || !data.TryGetValue(typeof(TValue), out object? objVal)) {
            value = null;
            return false;
        }

        value = (TValue) objVal;
        return true;
    }

    public void Set<TValue>(TopLevelIdentifier topLevel, TValue value) where TValue : class {
        this.GetOrCreateTopLevelMap(topLevel)[typeof(TValue)] = value;
    }

    public TValue GetOrCreate<TValue>(TopLevelIdentifier topLevel, object? factoryState, Func<object?, TopLevelIdentifier, TValue> factory) where TValue : class {
        Dictionary<Type, object> map = this.GetOrCreateTopLevelMap(topLevel);
        if (!map.TryGetValue(typeof(TValue), out object? objVal)) {
            map[typeof(TValue)] = objVal = factory(factoryState, topLevel);
        }

        return (TValue) objVal;
    }

    public TValue? Remove<TValue>(TopLevelIdentifier topLevel) where TValue : class {
        if (this.dataMap.TryGetValue(topLevel, out Dictionary<Type, object>? data)) {
            if (data.Remove(typeof(TValue), out object? objVal)) {
                return (TValue) objVal;
            }
        }

        return null;
    }

    public static TopLevelDataMap GetInstance(IComponentManager componentManager) {
        return componentManager.GetOrCreateComponent((m) => new TopLevelDataMap(m));
    }
}
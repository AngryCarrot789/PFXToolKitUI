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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.Composition;

namespace PFXToolKitUI.Interactivity.Windowing;

public sealed class TopLevelDataMap {
    private readonly Dictionary<TopLevelIdentifier, Dictionary<Type, object>> dataMap;

    public IComponentManager ComponentManager { get; }

    /// <summary>
    /// Gets an enumerable of <see cref="TopLevelIdentifier"/> instances this data map stores values for
    /// </summary>
    public IEnumerable<TopLevelIdentifier> TopLevels => this.dataMap.Keys;
    
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

    public bool TryGetValue(TopLevelIdentifier topLevel, Type valueType, [NotNullWhen(true)] out object? value) {
        ArgumentNullException.ThrowIfNull(valueType);
        if (this.dataMap.TryGetValue(topLevel, out Dictionary<Type, object>? data) && data.TryGetValue(valueType, out value)) {
            return true;
        }

        value = null;
        return false;
    }
    
    public bool TryGetValue<TValue>(TopLevelIdentifier topLevel, [NotNullWhen(true)] out TValue? value) where TValue : class {
        if (!this.TryGetValue(topLevel, typeof(TValue), out object? objVal)) {
            value = null;
            return false;
        }

        value = (TValue) objVal;
        return true;
    }

    public void Set<TValue>(TopLevelIdentifier topLevel, TValue value) where TValue : class {
        this.GetOrCreateTopLevelMap(topLevel)[typeof(TValue)] = value;
    }
    
    public void Set(TopLevelIdentifier topLevel, Type valueType, object value) {
        if (!valueType.IsInstanceOfType(value)) {
            ThrowInvalidType(valueType, value.GetType());
            return;

            [DoesNotReturn]
            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowInvalidType(Type expectedType, Type actualType) {
                throw new ArgumentException($"Expected value of type {expectedType}, instead got {actualType}");
            }
        }
        
        this.GetOrCreateTopLevelMap(topLevel)[valueType] = value;
    }

    public TValue GetOrCreate<TValue>(TopLevelIdentifier topLevel, object? factoryState, Func<object?, TopLevelIdentifier, TValue> factory) where TValue : class {
        Dictionary<Type, object> map = this.GetOrCreateTopLevelMap(topLevel);
        if (!map.TryGetValue(typeof(TValue), out object? objVal)) {
            map[typeof(TValue)] = objVal = factory(factoryState, topLevel);
        }

        return (TValue) objVal;
    }

    public TValue? Remove<TValue>(TopLevelIdentifier topLevel) where TValue : class {
        return (TValue?) this.Remove(topLevel, typeof(TValue));
    }
    
    public object? Remove(TopLevelIdentifier topLevel, Type valueType) {
        if (this.dataMap.TryGetValue(topLevel, out Dictionary<Type, object>? data)) {
            if (data.Remove(valueType, out object? objVal)) {
                if (data.Count < 1) {
                    bool removed = this.dataMap.Remove(topLevel);
                    Debug.Assert(removed);
                }
                
                return objVal;
            }
        }

        return null;
    }

    public IEnumerable<TValue> GetValues<TValue>(string topLevelId) where TValue : class {
        return this.GetValues(topLevelId, typeof(TValue)).Cast<TValue>();
    }
    
    public IEnumerable<object> GetValues(string topLevelId, Type valueType) {
        foreach (KeyValuePair<TopLevelIdentifier, Dictionary<Type, object>> entry in this.dataMap) {
            if (entry.Key.Id == topLevelId) {
                if (entry.Value.TryGetValue(valueType, out object? objVal)) {
                    yield return objVal;
                }
            }
        }
    }
    
    public IEnumerable<object> GetValues(string topLevelId) {
        return this.dataMap.Where(x => x.Key.Id == topLevelId).SelectMany(x => x.Value.Values);
    }

    public static TopLevelDataMap GetInstance(IComponentManager componentManager) {
        return componentManager.GetOrCreateComponent((m) => new TopLevelDataMap(m));
    }
}
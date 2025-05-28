// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;

namespace PFXToolKitUI.Avalonia.Utils;

public readonly struct MultiControlEntry : IEquatable<MultiControlEntry> {
    /// <summary>
    /// Gets the type we were scanning when iterating the type hierarchy of a source model.
    /// When <see cref="RegisteredType"/> is an interface, this value will be the object-type
    /// that has the interface implemented, unless the user passed an interface type to
    /// <see cref="ModelTypeMultiControlRegistry.GetControlTypes"/> in which case
    /// this value will also be an interface.
    /// <para>
    /// This value is not typically used
    /// </para>
    /// </summary>
    public readonly Type HierarchicalType;

    /// <summary>
    /// Returns the type that is used as the key in the <see cref="ModelTypeMultiControlRegistry{object}.RegisterType"/> method.
    /// </summary>
    public readonly Type RegisteredType;

    public MultiControlEntry(Type hierarchicalType, Type registeredType) {
        this.RegisteredType = registeredType;
        this.HierarchicalType = hierarchicalType;
    }

    public bool Equals(MultiControlEntry other) {
        return this.RegisteredType == other.RegisteredType && this.HierarchicalType == other.HierarchicalType;
    }

    public override bool Equals(object? obj) {
        return obj is MultiControlEntry other && this.Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.RegisteredType, this.HierarchicalType);
    }

    public static bool operator ==(MultiControlEntry left, MultiControlEntry right) => left.Equals(right);

    public static bool operator !=(MultiControlEntry left, MultiControlEntry right) => !(left == right);
}

/// <summary>
/// A model-control registry that supports creating a list of controls by iterating the type hierarchy of the model
/// </summary>
/// <typeparam name="T">The control type</typeparam>
public class ModelTypeMultiControlRegistry<T> where T : AvaloniaObject {
    public const int MaxCacheSize = 32;

    private readonly Dictionary<Type, List<Func<T>>> registeredConstructors;

    // Since calculating the control hierarchy is fairly expensive, we want to cache it
    // This gets invalidated whenever ANYTHING is registered, because otherwise if for example
    // an interface was registered, we'd have to figure out every controls that implements it
    // which will be really expensive as well
    private readonly Dictionary<Type, List<MultiControlEntry>> modelTypeToEntryListCache;
    private readonly Dictionary<Type, IList<T>> controlCache; // RegisteredType to control list
    private ulong cacheIteration; // The amount of times the cache was invalidated due to RegisterType

    /// <summary>
    /// Creates an instance of the ModelTypeMultiControlRegistry
    /// </summary>
    /// <param name="cacheSize">The size of the cache. Set to 0 to disable. Potential performance issues when disabled!</param>
    public ModelTypeMultiControlRegistry() {
        this.registeredConstructors = new Dictionary<Type, List<Func<T>>>();
        this.modelTypeToEntryListCache = new Dictionary<Type, List<MultiControlEntry>>();
        this.controlCache = new Dictionary<Type, IList<T>>();
    }

    public void RegisterType(Type modelType, Func<T> constructor) {
        // Clear cache
        foreach (List<MultiControlEntry> theCachedList in this.modelTypeToEntryListCache.Values)
            theCachedList.Clear(); // helps GC :D hopefully
        this.modelTypeToEntryListCache.Clear();
        this.controlCache.Clear();

        if (!this.registeredConstructors.TryGetValue(modelType, out List<Func<T>>? list))
            this.registeredConstructors[modelType] = list = [];

        list.Add(constructor);
        this.cacheIteration++;
    }

    public int AddItemsToCache(ModelTypeMultiControlList<T> list) {
        if (list.cacheIteration != this.cacheIteration)
            return 0; // list is out of data with our cache
        if (this.controlCache.Count >= MaxCacheSize)
            return 0; // cache is full

        int count = 0;
        for (int i = 0; i < list.ControlMap.Count; i++, count++) {
            (MultiControlEntry, IList<T>) entry = list.ControlMap[i];
            this.controlCache[entry.Item1.RegisteredType] = entry.Item2;
            if (this.controlCache.Count == MaxCacheSize) {
                break;
            }
        }

        return count;
    }

    public ModelTypeMultiControlList<T> GetControlInstances(object model) => this.GetControlInstances(model.GetType());

    public ModelTypeMultiControlList<T> GetControlInstances(Type modelType) {
        IList<MultiControlEntry> typeEntries = this.GetControlTypes(modelType);
        List<(MultiControlEntry, IList<T>)> controlMap = new List<(MultiControlEntry, IList<T>)>(typeEntries.Count);
        List<T> allControls = new List<T>(typeEntries.Count + 8);
        foreach (MultiControlEntry entry in typeEntries) {
            if (this.controlCache.Remove(entry.RegisteredType, out IList<T>? cachedControls)) {
                allControls.AddRange(cachedControls);
                controlMap.Add((entry, cachedControls));
            }
            else if (this.registeredConstructors.TryGetValue(entry.RegisteredType, out List<Func<T>>? ctorList)) {
                IList<T> controls = new List<T>(ctorList.Count);
                foreach (Func<T> ctor in ctorList) {
                    controls.Add(ctor());
                }

                controls = controls.AsReadOnly();
                allControls.AddRange(controls);
                controlMap.Add((entry, controls));
            }
        }

        return new ModelTypeMultiControlList<T>(allControls, controlMap.AsReadOnly()) {
            cacheIteration = this.cacheIteration
        };
    }

    /// <summary>
    /// Gets a list of every single request for a control instance for the given model type.
    /// Interfaces are appended AFTER the objective type.
    /// </summary>
    /// <param name="modelType">The model type</param>
    /// <param name="unique">True to generate a new list instead of using a cached list</param>
    /// <returns></returns>
    public IList<MultiControlEntry> GetControlTypes(Type modelType, bool unique = false) {
        ArgumentNullException.ThrowIfNull(modelType);
        if (!unique && this.modelTypeToEntryListCache.TryGetValue(modelType, out List<MultiControlEntry>? cachedList)) {
            return cachedList.AsReadOnly();
        }

        List<MultiControlEntry> finalList;
        List<Func<T>>? ctorList;
        List<Type> interfaces = modelType.GetInterfaces().ToList();
        if (modelType.IsInterface) {
            finalList = new List<MultiControlEntry>(interfaces.Count);
            for (int j = 0; j < interfaces.Count; j++) {
                Type itf = interfaces[j];
                if (this.registeredConstructors.TryGetValue(itf, out ctorList) && ctorList.Count > 0) {
                    finalList.Add(new MultiControlEntry(modelType, itf));
                }
            }
        }
        else {
            List<Type> typeHierarchy = new List<Type>();
            for (Type? type = modelType; type != null; type = type.BaseType) {
                typeHierarchy.Add(type);
            }

            int totalEntries = 0;
            List<List<MultiControlEntry>> hierarchyLists = new List<List<MultiControlEntry>>();

            // First scan bottom to top (object to modelType)
            for (int i = typeHierarchy.Count - 1; i >= 0; i--) {
                List<MultiControlEntry> subList = new List<MultiControlEntry>();

                Type type = typeHierarchy[i];
                for (int j = 0; j < interfaces.Count; j++) {
                    Type itf = interfaces[j];
                    if (itf.IsAssignableFrom(type)) {
                        // We found an interface in use, so remove it so that derived types cannot see it
                        interfaces.RemoveAt(j);
                        if (this.registeredConstructors.TryGetValue(itf, out ctorList) && ctorList.Count > 0) {
                            subList.Add(new MultiControlEntry(type, itf));
                        }
                    }
                }

                if (this.registeredConstructors.TryGetValue(type, out ctorList) && ctorList.Count > 0) {
                    subList.Insert(0, new MultiControlEntry(type, type));
                }

                hierarchyLists.Add(subList);
                totalEntries += subList.Count;
            }

            finalList = new List<MultiControlEntry>(totalEntries);
            for (int i = hierarchyLists.Count - 1; i >= 0; i--) {
                finalList.AddRange(hierarchyLists[i]);
            }
        }

        if (!unique)
            this.modelTypeToEntryListCache[modelType] = finalList;
        return finalList.AsReadOnly();
    }
}

public class ModelTypeMultiControlList<T> where T : AvaloniaObject {
    internal ulong cacheIteration;

    /// <summary>
    /// Gets the controls that were generated
    /// </summary>
    public IList<T> Controls { get; }

    /// <summary>
    /// Gets the ordered list of items that maps an entry to a list of controls.
    /// This is what was originally used to generate <see cref="Controls"/> list
    /// </summary>
    public IList<(MultiControlEntry, IList<T>)> ControlMap { get; }

    public ModelTypeMultiControlList(IList<T> controls, IList<(MultiControlEntry, IList<T>)> controlMap) {
        this.Controls = controls;
        this.ControlMap = controlMap;
    }
}
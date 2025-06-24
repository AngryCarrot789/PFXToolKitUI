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

using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Logging;

namespace PFXToolKitUI.Avalonia;

/// <summary>
/// A class that exposes avalonia internals that ideally should be public
/// </summary>
public static class AvUtils {
    private static AvaloniaLocator Locator;
    private static MethodInfo GetServiceMethod;

    public static void OnApplicationInitialised() {
        Locator = (AvaloniaLocator) GetProperty<AvaloniaLocator, IAvaloniaDependencyResolver>(null, "Current", true)!;
        GetServiceMethod = typeof(AvaloniaLocator).GetMethod("GetService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(Type)], null) ?? throw new Exception("Could not find GetService method");

        // Test that the above code works
        GetService(typeof(object));
        RuntimeHelpers.RunClassConstructor(typeof(AutoTemplate).TypeHandle);
    }

    public static void OnFrameworkInitialised() {
    }

    /// <summary>
    /// Gets an Avalonia application service
    /// </summary>
    /// <param name="type">The service type</param>
    /// <returns>The service, or null, if no service was found</returns>
    public static object? GetService(Type type) => GetServiceMethod.Invoke(Locator, [type]);

    public static T? GetService<T>() => (T?) GetServiceMethod.Invoke(Locator, [typeof(T)]);

    /// <summary>
    /// Tries to get a service of the generic type
    /// </summary>
    /// <param name="value">The found service</param>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>Whether the service was found</returns>
    public static bool TryGetService<T>(out T value) where T : class => (value = (GetService(typeof(T)) as T)!) != null;

    private static TValue? GetProperty<TOwner, TValue>(object? instance, string name, bool isStatic, bool allowNull = false) {
        Type owner = typeof(TOwner);
        BindingFlags initialFlags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
        PropertyInfo? property;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.Public)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.NonPublic)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.Public | BindingFlags.FlattenHierarchy)) != null)
            goto found;
        if ((property = owner.GetProperty(name, initialFlags | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)) == null)
            throw new Exception("No such property: " + owner.Name + "." + name);

        found:
        object? theValue = property.GetValue(instance);

        if (allowNull && theValue == null)
            return default;

        if (theValue is TValue value)
            return value;

        throw new Exception("Property value is incompatible with type");
    }

    public static Point GetRelativePosition(Visual relativeTo, Point point) {
        IRenderRoot? root = relativeTo.GetVisualRoot();
        if (root == null) {
            return point;
        }

        PixelPoint screen = root.PointToScreen(point);
        return relativeTo.PointToClient(screen);
    }

    public static RDMultiChange BeginMultiChange(ResourceDictionary resourceDictionary) {
        return new RDMultiChange(resourceDictionary);
    }

    public sealed class RDMultiChange : IDisposable {
        private static readonly MethodInfo? RaiseResourcesChangedMethod;
        private static readonly PropertyInfo? InnerProperty;

        private readonly Dictionary<object, object?>? dict_map;
        private Dictionary<object, object?>? map;
        private HashSet<object>? removedKeys;

        public ResourceDictionary Dictionary { get; }

        public object? this[object key] {
            get {
                if (this.map == null)
                    return null;
                this.map.TryGetValue(key, out object? value); // same behaviour as ResourceDictionary
                return value;
            }
            set => (this.map ??= new Dictionary<object, object?>())[key] = value;
        }

        internal RDMultiChange(ResourceDictionary dictionary) {
            this.Dictionary = dictionary;
            this.dict_map = (Dictionary<object, object?>?) InnerProperty?.GetValue(dictionary);
        }

        static RDMultiChange() {
            RaiseResourcesChangedMethod = typeof(ResourceProvider).GetMethod("RaiseResourcesChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            InnerProperty = typeof(ResourceDictionary).GetProperty("Inner", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void Remove(object key) => (this.removedKeys ??= []).Add(key);

        public void Dispose() {
            // Fallback to slow version -- need to update for new avalonia versions
            if (this.dict_map == null || RaiseResourcesChangedMethod == null) {
                AppLogger.Instance.WriteLine("Warning: internal ResourceDictionary API changed. Cannot fast change");
                if (this.removedKeys != null)
                    foreach (object key in this.removedKeys)
                        this.Dictionary.Remove(key);
                if (this.map != null)
                    this.Dictionary.SetItems(this.map);
            }
            else {
                bool anyChanges = false;
                if (this.map?.Count > 0) {
                    anyChanges = true;
                    foreach (KeyValuePair<object, object?> entry in this.map)
                        this.dict_map[entry.Key] = entry.Value;
                }

                if (this.removedKeys?.Count > 0) {
                    anyChanges = true;
                    foreach (object key in this.removedKeys)
                        this.dict_map.Remove(key);
                }

                if (anyChanges)
                    RaiseResourcesChangedMethod.Invoke(this.Dictionary, []);
            }
        }
    }
}
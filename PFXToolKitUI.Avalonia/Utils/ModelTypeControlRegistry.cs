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
using Avalonia.Controls;

namespace PFXToolKitUI.Avalonia.Utils;

public delegate void ModelTypeControlRegistryTypeRegisteredEventHandler(object sender, Type modelType, Delegate constructor);

/// <summary>
/// A model type to control instance registry
/// </summary>
/// <typeparam name="TControl">The base class for the controls</typeparam>
public class ModelTypeControlRegistry<TControl> where TControl : Control {
    private readonly Dictionary<Type, Func<TControl>> constructors;

    public event ModelTypeControlRegistryTypeRegisteredEventHandler? TypeRegistered;
    
    public ModelTypeControlRegistry() {
        this.constructors = new Dictionary<Type, Func<TControl>>();
    }

    public void RegisterType(Type modelType, Func<TControl> constructor) {
        this.constructors[modelType] = constructor;
        this.TypeRegistered?.Invoke(this, modelType, constructor);
    }

    // public TControl NewInstance(Type modelType) {
    //     if (modelType == null) {
    //         throw new ArgumentNullException(nameof(modelType));
    //     }
    //
    //     // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
    //     bool hasLogged = false;
    //     for (Type? type = modelType; type != null; type = type.BaseType) {
    //         if (this.constructors.TryGetValue(type, out Func<TControl>? func)) {
    //             return func();
    //         }
    //
    //         if (!hasLogged) {
    //             hasLogged = true;
    //             Debugger.Break();
    //             Debug.WriteLine("Could not find control for resource type on first try. Scanning base types");
    //         }
    //     }
    //
    //     throw new Exception("No such content control for resource type: " + modelType.Name);
    // }
    
    public bool TryGetNewInstance(Type modelType, [NotNullWhen(true)] out TControl? control) {
        return (control = this.NewInstanceInternal(modelType, false)) != null;
    }

    public TControl NewInstance(Type modelType, bool logBaseTypeScan = true) {
        TControl? control = this.NewInstanceInternal(modelType, logBaseTypeScan);
        return control ?? throw new Exception("No registered control for model type: " + modelType.Name);
    }

    private TControl? NewInstanceInternal(Type modelType, bool logBaseTypeScan) {
        ArgumentNullException.ThrowIfNull(modelType);
        bool hasLogged = false;
        // Just try to find a base control type. It should be found first try unless I forgot to register a new control type
        for (Type? type = modelType; type != null; type = type.BaseType) {
            if (this.constructors.TryGetValue(type, out Func<TControl>? func)) {
                return func();
            }

            if (logBaseTypeScan && !hasLogged) {
                hasLogged = true;
                Debugger.Break();
                Debug.WriteLine("Could not find control for model type on first try. Scanning base types");
            }
        }

        return null;
    }
}
// 
// Copyright (c) 2023-2025 REghZy
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
using System.Runtime.CompilerServices;

namespace PFXToolKitUI;

/// <summary>
/// A model-control map that casts the runtime instances of the source models and controls to the destination models and controls
/// </summary>
public static class CastingModelControlMap {
    /// <summary>
    /// Creates an unsafe casting map that uses <see cref="Unsafe.As{T}"/> to cast the source models and controls to the destination models and controls
    /// </summary>
    public static IModelControlMap<TDstModel, TDstControl> CreateUnsafe<TSrcModel, TDstModel, TSrcControl, TDstControl>(IModelControlMap<TSrcModel, TSrcControl> source) where TSrcModel : class where TDstModel : class where TSrcControl : class where TDstControl : class {
        return new UnsafeModelControlCastingMap<TSrcModel, TDstModel, TSrcControl, TDstControl>(source);
    }

    private class UnsafeModelControlCastingMap<TSrcModel, TDstModel, TSrcControl, TDstControl> : IModelControlMap<TDstModel, TDstControl> where TSrcModel : class where TDstModel : class where TSrcControl : class where TDstControl : class {
        private readonly IModelControlMap<TSrcModel, TSrcControl> source;

        public IEnumerable<TDstModel> Models {
            [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
            get {
                return typeof(TSrcModel) == typeof(TDstModel)
                    ? Unsafe.As<IEnumerable<TDstModel>>(this.source.Models)
                    : this.source.Models.Select(src => Unsafe.As<TDstModel>(src));
            }
        }

        public IEnumerable<TDstControl> Controls {
            [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
            get {
                return typeof(TSrcControl) == typeof(TDstControl)
                    ? Unsafe.As<IEnumerable<TDstControl>>(this.source.Controls)
                    : this.source.Controls.Select(src => Unsafe.As<TDstControl>(src));
            }
        }

        public IEnumerable<KeyValuePair<TDstModel, TDstControl>> Entries {
            get {
                if (typeof(TSrcModel) == typeof(TDstModel) && typeof(TSrcControl) == typeof(TDstControl))
                    return Unsafe.As<IEnumerable<KeyValuePair<TDstModel, TDstControl>>>(this.source.Entries);
                return this.source.Entries.Select(pair => new KeyValuePair<TDstModel, TDstControl>(Unsafe.As<TDstModel>(pair.Key), Unsafe.As<TDstControl>(pair.Value)));
            }
        }

        public UnsafeModelControlCastingMap(IModelControlMap<TSrcModel, TSrcControl> source) {
            this.source = source;
        }

        public TDstControl GetControl(TDstModel model) {
            return Unsafe.As<TDstControl>(this.source.GetControl(Unsafe.As<TSrcModel>(model)));
        }

        public TDstModel GetModel(TDstControl control) {
            return Unsafe.As<TDstModel>(this.source.GetModel(Unsafe.As<TSrcControl>(control)));
        }

        public bool TryGetControl(TDstModel model, [NotNullWhen(true)] out TDstControl? control) {
            if (this.source.TryGetControl(Unsafe.As<TSrcModel>(model), out TSrcControl? theControl)) {
                control = Unsafe.As<TDstControl>(theControl);
                return true;
            }
            else {
                control = null;
                return false;
            }
        }

        public bool TryGetModel(TDstControl control, [NotNullWhen(true)] out TDstModel? model) {
            if (this.source.TryGetModel(Unsafe.As<TSrcControl>(control), out TSrcModel? theModel)) {
                model = Unsafe.As<TDstModel>(theModel);
                return true;
            }
            else {
                model = null;
                return false;
            }
        }
    }
}
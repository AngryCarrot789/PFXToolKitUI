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

using System.Collections.ObjectModel;

namespace PFXToolKitUI.Utils.Collections.ObservableEx;

public class ReadOnlyObservableListEx<T> : ReadOnlyCollection<T>, IObservableListEx<T> {
    public event ObservableListExChangedEventHandler<T>? CollectionChanged;

    public ReadOnlyObservableListEx(IObservableListEx<T> list) : base(list) {
        list.CollectionChanged += this.HandleCollectionChanged;
    }

    private void HandleCollectionChanged(IObservableListEx<T> list, ObservableListChangedEventArgs<T> e) {
        this.CollectionChanged?.Invoke(this, e);
    }
}
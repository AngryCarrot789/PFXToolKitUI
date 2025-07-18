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

using System.Collections;

namespace PFXToolKitUI.Utils;

public class SingletonList<T> : IList<T>, IList {
    private readonly T value;

    public int Count => 1;

    public bool IsReadOnly => true;

    public T this[int index] {
        get => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);
        set => throw new NotImplementedException("Read-only list");
    }

    object? IList.this[int index] {
        get => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);
        set => throw new NotImplementedException("Read-only list");
    }

    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public bool IsFixedSize => true;

    public SingletonList(T value) {
        this.value = value;
    }

    public void Add(T item) => throw new NotImplementedException("Read-only list");

    public void Clear() => throw new NotImplementedException("Read-only list");

    public bool Remove(T item) => throw new NotImplementedException("Read-only list");

    public void Insert(int index, T item) => throw new NotImplementedException("Read-only list");

    public void RemoveAt(int index) => throw new NotImplementedException("Read-only list");


    public bool Contains(T item) {
        return this.IndexOf(item) == 0;
    }

    public void CopyTo(T[] array, int arrayIndex) {
        array[arrayIndex] = this.value;
    }

    public int IndexOf(T item) {
        return EqualityComparer<T>.Default.Equals(item, this.value) ? 0 : -1;
    }

    public IEnumerator<T> GetEnumerator() {
        yield return this.value;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        yield return this.value;
    }

    public void CopyTo(Array array, int index) => array.SetValue(this.value, index);

    public int Add(object? value) => throw new NotImplementedException("Read-only list");

    public bool Contains(object? val) => EqualityComparer<object>.Default.Equals(val, this.value);

    public int IndexOf(object? val) => this.Contains(val) ? 0 : -1;

    public void Insert(int index, object? value) => throw new NotImplementedException("Read-only list");

    public void Remove(object? value) => throw new NotImplementedException("Read-only list");
}

public class SingletonReadOnlyList<T> : IReadOnlyList<T> {
    private readonly T value;

    public int Count => 1;

    public T this[int index] => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);

    public SingletonReadOnlyList(T value) {
        this.value = value;
    }

    public IEnumerator<T> GetEnumerator() {
        yield return this.value;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        yield return this.value;
    }
}
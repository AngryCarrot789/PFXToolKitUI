// 
// Copyright (c) 2025-2025 REghZy
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

namespace PFXToolKitUI.Interactivity.Contexts.Observables;

/// <summary>
/// An implementation of <see cref="IMutableContextData"/> that stores entries in a dictionary
/// </summary>
public abstract class BaseObservableContextData : IObservableMutableContextData {
    private int batchCounter;
    private List<ModificationEntry>? myBatchModifications;

    /// <summary>
    /// The number of entries in our internal map
    /// </summary>
    public abstract int Count { get; }

    public abstract IEnumerable<KeyValuePair<string, object>> Entries { get; }

    /// <summary>
    /// Gets or creates the dictionary instance
    /// </summary>
    protected abstract IDictionary<string, object> InternalDictionary { get; }

    public event EventHandler? ContextChanged;

    protected BaseObservableContextData() {
    }

    protected void RaiseContextChanged() {
        this.ContextChanged?.Invoke(this, EventArgs.Empty);
    }

    void IMutableContextData.OnEnterBatchScope() {
        this.batchCounter++;
    }

    void IMutableContextData.OnExitBatchScope() {
        if (this.batchCounter == 0)
            Debug.Fail("Excessive calls to OnExitBatchScope");

        if (--this.batchCounter == 0)
            this.ProcessBatches();
    }

    // We hide the interface impl so that we can overwrite it with the builder-style version
    void IMutableContextData.SetUnsafe(string key, object? value) {
        if (value == null) {
            this.HandleRemove(key);
        }
        else {
            this.HandleAdd(key, value);
        }
    }

    public virtual bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        if (this.Count > 0 && this.InternalDictionary.TryGetValue(key, out value!))
            return true;
        value = null!;
        return false;
    }

    public virtual bool ContainsKey(string key) => this.Count > 0 && this.InternalDictionary.ContainsKey(key);

    public override string ToString() {
        string details = "empty";
        if (this.Count > 0) {
            details = string.Join(", ", this.InternalDictionary.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
        }

        return $"{this.GetType().Name}[" + details + "]";
    }

    private void HandleAdd(string key, object value) {
        if (this.batchCounter > 0) {
            if (this.FindExistingBatchEntry(key, out int index)) {
                this.myBatchModifications![index] = ModificationEntry.Add(key, value);
                return;
            }

            (this.myBatchModifications ??= new List<ModificationEntry>()).Add(ModificationEntry.Add(key, value));
        }
        else {
            IDictionary<string, object>? dict = null;
            if (this.Count < 1 || !(dict = this.InternalDictionary).TryGetValue(key, out object? existing) || !Equals(existing, value)) {
                (dict ?? this.InternalDictionary)[key] = value;
                this.RaiseContextChanged();
            }
        }
    }

    private void HandleRemove(string key) {
        if (this.batchCounter > 0) {
            if (this.FindExistingBatchEntry(key, out int index)) {
                this.myBatchModifications![index] = ModificationEntry.Remove(key);
                return;
            }

            (this.myBatchModifications ??= new List<ModificationEntry>()).Add(ModificationEntry.Remove(key));
        }
        else if (this.Count > 0 && this.InternalDictionary.Remove(key)) {
            this.RaiseContextChanged();
        }
    }

    private bool FindExistingBatchEntry(string key, out int index) {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        List<ModificationEntry>? list = this.myBatchModifications;
        if (list != null) {
            for (index = 0; index < list.Count; index++) {
                if (list[index].Key == key) {
                    return true;
                }
            }
        }

        index = -1;
        return false;
    }

    private void ProcessBatches() {
        Debug.Assert(this.batchCounter <= 0);
        List<ModificationEntry>? list = this.myBatchModifications;
        if (list == null || list.Count < 1) {
            return;
        }

        int added = 0, removed = 0;
        IDictionary<string, object> map = this.InternalDictionary;
        foreach (ModificationEntry entry in list) {
            if (entry.IsAdding) {
                Debug.Assert(entry.Value != null, "Insertion entry's value should not be null");
                if (map.TryGetValue(entry.Key, out object? existing) && Equals(existing, entry.Value)) {
                    continue;
                }

                map[entry.Key] = entry.Value;
                added++;
            }
            else if (map.Remove(entry.Key)) {
                removed++;
            }
        }

        if (added > 0 || removed > 0) {
            this.RaiseContextChanged();
        }

        this.myBatchModifications = null;
    }

    /// <summary>
    /// Gets the last added modification entry associated with the given key.
    /// Modification entries are only created when inserting or removing data during batching
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="entry">The found entry, or default</param>
    /// <returns>True if an entry is found with the given key</returns>
    private bool FindLatestModificationEntryByKey(string key, out ModificationEntry entry) {
        if (this.myBatchModifications != null) {
            for (int i = this.myBatchModifications.Count - 1; i >= 0; i--) {
                if ((entry = this.myBatchModifications[i]).Key == key) {
                    return true;
                }
            }
        }

        entry = default;
        return false;
    }

    private readonly struct ModificationEntry {
        public readonly bool IsAdding;
        public readonly string Key;
        public readonly object? Value;

        public static ModificationEntry Add(string key, object value) => new ModificationEntry(key, value);
        public static ModificationEntry Remove(string key) => new ModificationEntry(key);

        private ModificationEntry(string key, object value) {
            this.IsAdding = true;
            this.Key = key;
            this.Value = value;
        }

        private ModificationEntry(string key) {
            this.Key = key;
        }
    }
}
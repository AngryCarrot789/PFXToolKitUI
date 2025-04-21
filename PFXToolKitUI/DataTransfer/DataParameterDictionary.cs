using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PFXToolKitUI.DataTransfer;

internal struct DataParameterDictionary<TValue> {
    private const int DefaultInitialCapacity = 4;
    private Entry[]? _entries;
    private int _entryCount;

    public DataParameterDictionary() {
        this._entries = null;
        this._entryCount = 0;
    }

    public DataParameterDictionary(int capactity) {
        this._entries = new Entry[capactity];
        this._entryCount = 0;
    }

    public int Count => this._entryCount;

    public TValue this[DataParameter property] {
        get {
            int index = this.FindEntry(property.GlobalIndex);
            if (index < 0)
                ThrowNotFound();
            return this.UnsafeGetEntryRef(index).Value;
        }
        set {
            int index = this.FindEntry(property.GlobalIndex);
            if (index >= 0)
                this.UnsafeGetEntryRef(index) = new Entry(property, value);
            else
                this.InsertEntry(new Entry(property, value), ~index);
        }
    }

    public TValue this[int index] {
        get {
            if (index >= this._entryCount)
                ThrowOutOfRange();
            return this.UnsafeGetEntryRef(index).Value;
        }
    }

    public void Add(DataParameter property, TValue value) {
        int index = this.FindEntry(property.GlobalIndex);
        if (index >= 0)
            ThrowDuplicate();
        this.InsertEntry(new Entry(property, value), ~index);
    }

    public void Clear() {
        if (this._entries is not null) {
            Array.Clear(this._entries, 0, this._entries.Length);
            this._entryCount = 0;
        }
    }

    public bool ContainsKey(DataParameter property) => this.FindEntry(property.GlobalIndex) >= 0;

    public TValue GetValue(int index) {
        if (index >= this._entryCount)
            ThrowOutOfRange();
        ref Entry entry = ref this.UnsafeGetEntryRef(index);
        return entry.Value;
    }

    public bool Remove(DataParameter property) {
        int index = this.FindEntry(property.GlobalIndex);
        if (index >= 0) {
            this.RemoveAt(index);
            return true;
        }

        return false;
    }

    public bool Remove(DataParameter property, [MaybeNullWhen(false)] out TValue value) {
        int index = this.FindEntry(property.GlobalIndex);
        if (index >= 0) {
            value = this.UnsafeGetEntryRef(index).Value;
            this.RemoveAt(index);
            return true;
        }

        value = default;
        return false;
    }

    public void RemoveAt(int index) {
        if (this._entries is null)
            ThrowOutOfRange();

        Array.Copy(this._entries, index + 1, this._entries, index, this._entryCount - index - 1);
        this._entryCount--;
        this.UnsafeGetEntryRef(this._entryCount) = default;
    }

    public bool TryAdd(DataParameter property, TValue value) {
        int index = this.FindEntry(property.GlobalIndex);
        if (index >= 0)
            return false;
        this.InsertEntry(new Entry(property, value), ~index);
        return true;
    }

    public bool TryGetValue(DataParameter property, [MaybeNullWhen(false)] out TValue value) {
        int lo = 0;
        int hi = this._entryCount - 1;

        if (hi >= 0) {
            int propertyId = property.GlobalIndex;
            ref Entry entry0 = ref this.UnsafeGetEntryRef(0);

            do {
                // hi and lo are never negative: there's no overflow using unsigned math
                int i = (int) (((uint) hi + (uint) lo) >> 1);

#if NET6_0_OR_GREATER
                // nuint cast to force zero extend instead of sign extend
                ref Entry entry = ref Unsafe.Add(ref entry0, (nuint) i);
#else
                    ref var entry = ref Unsafe.Add(ref entry0, i);
#endif

                int entryId = entry.GlobalIndex;
                if (entryId == propertyId) {
                    value = entry.Value;
                    return true;
                }

                if (entryId < propertyId) {
                    lo = i + 1;
                }
                else {
                    hi = i - 1;
                }
            } while (lo <= hi);
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindEntry(int propertyId) {
        int lo = 0;
        int hi = this._entryCount - 1;

        if (hi >= 0) {
            ref Entry entry0 = ref this.UnsafeGetEntryRef(0);

            do {
                // hi and lo are never negative: there's no overflow using unsigned math
                int i = (int) (((uint) hi + (uint) lo) >> 1);

#if NET6_0_OR_GREATER
                // nuint cast to force zero extend instead of sign extend
                ref Entry entry = ref Unsafe.Add(ref entry0, (nuint) i);
#else
                    ref var entry = ref Unsafe.Add(ref entry0, i);
#endif

                int entryId = entry.GlobalIndex;
                if (entryId == propertyId) {
                    return i;
                }

                if (entryId < propertyId) {
                    lo = i + 1;
                }
                else {
                    hi = i - 1;
                }
            } while (lo <= hi);
        }

        return ~lo;
    }

    [MemberNotNull(nameof(_entries))]
    private void InsertEntry(Entry entry, int entryIndex) {
        if (this._entryCount > 0) {
            if (this._entryCount == this._entries!.Length) {
                int newSize = this._entryCount == DefaultInitialCapacity ? DefaultInitialCapacity * 2 : (int) (this._entryCount * 1.5);

                Entry[] destEntries = new Entry[newSize];

                Array.Copy(this._entries, 0, destEntries, 0, entryIndex);

                destEntries[entryIndex] = entry;

                Array.Copy(this._entries, entryIndex, destEntries, entryIndex + 1, this._entryCount - entryIndex);

                this._entries = destEntries;
            }
            else {
                Array.Copy(this._entries,
                    entryIndex, this._entries,
                    entryIndex + 1, this._entryCount - entryIndex);

                this.UnsafeGetEntryRef(entryIndex) = entry;
            }
        }
        else {
            this._entries ??= new Entry[DefaultInitialCapacity];
            this.UnsafeGetEntryRef(0) = entry;
        }

        this._entryCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry UnsafeGetEntryRef(int index) {
#if NET6_0_OR_GREATER && !DEBUG
            // This type is performance critical: in release mode, skip any bound check the JIT compiler couldn't elide.
            // The index parameter should always be correct when calling this method: no unchecked user input should get here.
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this._entries!), (uint)index);
#else
        return ref this._entries![index];
#endif
    }

    [DoesNotReturn]
    private static void ThrowOutOfRange() => throw new IndexOutOfRangeException();

    [DoesNotReturn]
    private static void ThrowDuplicate() =>
        throw new ArgumentException("An item with the same key has already been added.");

    [DoesNotReturn]
    private static void ThrowNotFound() => throw new KeyNotFoundException();

    private readonly struct Entry {
        public readonly int GlobalIndex;
        public readonly TValue Value;

        public Entry(DataParameter property, TValue value) {
            this.GlobalIndex = property.GlobalIndex;
            this.Value = value;
        }
    }
}
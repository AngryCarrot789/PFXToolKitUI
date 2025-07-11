using System.Collections.ObjectModel;

namespace PFXToolKitUI.Utils.Collections;

/// <summary>
/// An extension to
/// </summary>
/// <typeparam name="T"></typeparam>
public class CollectionEx<T> : Collection<T> {
    public CollectionEx() {
    }

    public CollectionEx(IList<T> list) : base(list) {
    }

    public int FindIndex(Predicate<T> match) {
        for (int i = 0; i < this.Items.Count; i++) {
            if (match(this.Items[i])) {
                return i;
            }
        }

        return -1;
    }
}
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Interactivity.Windowing;

/// <summary>
/// Maps bidirectional dictionary that maps top level identifiers to top level objects, and vice versa
/// </summary>
public class TopLevelDictionary<T> where T : class, ITopLevel {
    private readonly List<KeyValuePair<TopLevelIdentifier, T>> topLevels;

    public IEnumerable<KeyValuePair<TopLevelIdentifier, T>> TopLevels => this.topLevels;

    public TopLevelDictionary() {
        this.topLevels = new List<KeyValuePair<TopLevelIdentifier, T>>();
    }

    public void AddTopLevel(TopLevelIdentifier topLevelIdentifier, T topLevel) {
        TopLevelIdentifier.ThrowIfInvalid(topLevelIdentifier);
        ArgumentNullException.ThrowIfNull(topLevel);
        foreach (KeyValuePair<TopLevelIdentifier, T> e in this.topLevels) {
            if (e.Key.Equals(topLevelIdentifier))
                throw new InvalidOperationException($"Identifier '{topLevelIdentifier}' already in use");
            if (ReferenceEquals(e.Value, topLevel))
                throw new InvalidOperationException($"TopLevel object already in use with identifier '{e.Key}'");
        }

        this.topLevels.Add(new KeyValuePair<TopLevelIdentifier, T>(topLevelIdentifier, topLevel));
        this.OnTopLevelAdded(topLevelIdentifier, topLevel);
    }

    public bool RemoveTopLevel(KeyValuePair<TopLevelIdentifier, T> entry) {
        TopLevelIdentifier.ThrowIfInvalid(entry.Key);
        ArgumentNullException.ThrowIfNull(entry.Value);
        
        int index = this.IndexOf(entry);
        if (index == -1)
            return false;

        this.topLevels.RemoveAt(index);
        this.OnTopLevelRemoved(entry.Key, entry.Value);
        return true;
    }

    public bool Contains(TopLevelIdentifier topLevelIdentifier) {
        TopLevelIdentifier.ThrowIfInvalid(topLevelIdentifier);
        foreach (KeyValuePair<TopLevelIdentifier, T> entry in this.topLevels) {
            if (entry.Key == topLevelIdentifier)
                return true;
        }

        return false;
    }

    public bool Contains(T topLevel) {
        ArgumentNullException.ThrowIfNull(topLevel);
        foreach (KeyValuePair<TopLevelIdentifier, T> entry in this.topLevels) {
            if (ReferenceEquals(entry.Value, topLevel))
                return true;
        }

        return false;
    }

    public bool Contains(KeyValuePair<TopLevelIdentifier, T> entry) {
        TopLevelIdentifier.ThrowIfInvalid(entry.Key);
        ArgumentNullException.ThrowIfNull(entry.Value);
        return this.IndexOf(entry) >= 0;
    }

    public bool TryGetTopLevel(TopLevelIdentifier topLevelIdentifier, [NotNullWhen(true)] out T? topLevel) {
        foreach (KeyValuePair<TopLevelIdentifier, T> entry in this.topLevels) {
            if (entry.Key.Equals(topLevelIdentifier)) {
                topLevel = entry.Value;
                return true;
            }
        }

        topLevel = null;
        return false;
    }

    public bool TryGetIdentifier(T topLevel, out TopLevelIdentifier topLevelIdentifier) {
        foreach (KeyValuePair<TopLevelIdentifier, T> entry in this.topLevels) {
            if (entry.Value.Equals(topLevel)) {
                topLevelIdentifier = entry.Key;
                return true;
            }
        }

        topLevelIdentifier = default;
        return false;
    }

    public ITopLevel GetTopLevel(TopLevelIdentifier topLevelIdentifier) {
        TopLevelIdentifier.ThrowIfInvalid(topLevelIdentifier);
        if (!this.TryGetTopLevel(topLevelIdentifier, out T? topLevel))
            throw new Exception("Identifier not in use with this dictionary");

        return topLevel;
    }

    public TopLevelIdentifier GetIdentifier(T topLevel) {
        ArgumentNullException.ThrowIfNull(topLevel);
        if (!this.TryGetIdentifier(topLevel, out TopLevelIdentifier topLevelIdentifier))
            throw new Exception("TopLevel not in use with this dictionary");

        return topLevelIdentifier;
    }

    protected virtual void OnTopLevelAdded(TopLevelIdentifier identifier, T topLevel) {
    }

    protected virtual void OnTopLevelRemoved(TopLevelIdentifier identifier, T topLevel) {
    }

    private int IndexOf(KeyValuePair<TopLevelIdentifier, T> entry) {
        for (int i = 0; i < this.topLevels.Count; i++) {
            KeyValuePair<TopLevelIdentifier, T> e = this.topLevels[i];
            if (e.Key.Equals(entry.Key) && ReferenceEquals(e.Value, entry.Value)) {
                return i;
            }
        }

        return -1;
    }
}
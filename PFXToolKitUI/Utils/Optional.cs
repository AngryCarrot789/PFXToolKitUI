namespace PFXToolKitUI.Utils;

/// <summary>
/// A wrapper for a value and a presence state
/// </summary>
/// <typeparam name="T">The type of value to store</typeparam>
public readonly struct Optional<T> : IEquatable<Optional<T>> {
    private readonly T value;

    /// <summary>
    /// Returns an <see cref="Optional{T}"/> without a value.
    /// </summary>
    public static Optional<T> Empty => default;
    
    /// <summary>
    /// Whether we have a valid value
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="HasValue"/> is false</exception>
    public T Value => this.HasValue ? this.value : throw new InvalidOperationException("Optional has no value.");

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> struct with value.
    /// </summary>
    /// <param name="value">The value.</param>
    public Optional(T value) {
        this.value = value;
        this.HasValue = true;
    }

    public override bool Equals(object? obj) {
        return obj is Optional<T> o && this.Equals(o);
    }

    public bool Equals(Optional<T> other) {
        if (!this.HasValue && !other.HasValue)
            return true;
        return this.HasValue && other.HasValue &&
               EqualityComparer<T>.Default.Equals(this.Value, other.Value);
    }

    public override int GetHashCode() {
        return this.HasValue
            ? this.value != null ? EqualityComparer<T>.Default.GetHashCode(this.value) : 0
            : 0;
    }

    public override string ToString() {
        return this.HasValue ? (this.value?.ToString() ?? "") : "Empty";
    }

    public T? GetValueOrDefault() => this.HasValue ? this.value : default;

    public T? GetValueOrDefault(T defaultValue) => this.HasValue ? this.value : defaultValue;

    public static implicit operator Optional<T>(T value) => new(value);

    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}

public static class OptionalExtensions {
    /// <summary>
    /// Casts the type of an <see cref="Optional{T}"/> using only the C# cast operator.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The binding value.</param>
    /// <returns>The cast value.</returns>
    public static Optional<T> Cast<T>(this Optional<object?> value) {
        return value.HasValue ? new Optional<T>((T) value.Value!) : Optional<T>.Empty;
    }
}
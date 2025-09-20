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
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Tasks;

/// <summary>
/// A result-exception pair
/// </summary>
/// <typeparam name="T">The type of value to store</typeparam>
[DebuggerDisplay("{ToString()}")]
public readonly struct Result<T> {
    private readonly T? value;

    /// <summary>
    /// Gets the value or throws
    /// </summary>
    /// <exception cref="Exception">The exception that caused the creation of this <see cref="Result{T}"/></exception>
    public T Value {
        get {
            if (this.Exception != null)
                throw this.Exception; // maybe ExceptionDispatchInfo.Throw()?
            return this.value!;
        }
    }

    /// <summary>
    /// Gets the exception
    /// </summary>
    public Exception? Exception { get; }

    public bool HasException => this.Exception != null;

    private Result(T? value, Exception? exception) {
        this.value = value;
        this.Exception = exception;
    }

    public T? GetValueOrDefault() => this.HasException ? default : this.value;
    
    public static Result<T> FromException(Exception exception) => new Result<T>(default, exception);

    public static Result<T> FromValue(T value) => new Result<T>(value, null);

    public static Result<T> Run(Func<T> factory) {
        T result;
        try {
            result = factory();
        }
        catch (Exception e) {
            return FromException(e);
        }

        return FromValue(result);
    }

    public static async Task<Result<T>> RunAsync(Func<Task<T>> factory) {
        T result;
        try {
            result = await factory();
        }
        catch (Exception e) {
            return FromException(e);
        }

        return FromValue(result);
    }

    /// <summary>
    /// Maps our current value into a new value via the mapping function.
    /// If this result has an exception, we return a new result with that exception.
    /// </summary>
    /// <param name="mapper">The mapping function to convert <see cref="T"/> into <see cref="V"/></param>
    /// <typeparam name="V">The new value type</typeparam>
    /// <returns>A new result</returns>
    public Result<V> Map<V>(Func<T, V> mapper) {
        if (this.Exception != null)
            return Result<V>.FromException(this.Exception);

        V result;
        try {
            result = mapper(this.value!);
        }
        catch (Exception e) {
            return Result<V>.FromException(e);
        }

        return Result<V>.FromValue(result);
    }

    /// <summary>
    /// Maps our current value into a new value via the mapping function as an async operation.
    /// If this result has an exception, we return a new result with that exception.
    /// </summary>
    /// <param name="mapper">The mapping function to convert <see cref="T"/> into a task that produces <see cref="V"/></param>
    /// <typeparam name="V">The new value type</typeparam>
    /// <returns>A new result</returns>
    public async Task<Result<V>> MapAsync<V>(Func<T, Task<V>> mapper) {
        if (this.Exception != null)
            return Result<V>.FromException(this.Exception);

        V result;
        try {
            result = await mapper(this.value!);
        }
        catch (Exception e) {
            return Result<V>.FromException(e);
        }

        return Result<V>.FromValue(result);
    }

    public override string ToString() {
        return this.HasException
            ? $"{nameof(Result<T>)} (error = {this.Exception})"
            : $"{nameof(Result<T>)} (value = {this.value!})";
    }
}

/// <summary>
/// A result-error pair
/// </summary>
/// <typeparam name="T">The type of value to store</typeparam>
/// <typeparam name="TError">The type of error value to store</typeparam>
[DebuggerDisplay("{ToString()}")]
public readonly struct Result<T, TError> {
    private readonly T? value;

    /// <summary>
    /// Gets the value or throws
    /// </summary>
    /// <exception cref="Error">The exception that caused the creation of this <see cref="Result{T}"/></exception>
    public T Value {
        get {
            if (this.Error.HasValue)
                throw new InvalidOperationException("Result contains only an error. Cannot access value");
            return this.value!;
        }
    }

    /// <summary>
    /// Gets the error value
    /// </summary>
    public Optional<TError> Error { get; }

    public bool HasError => this.Error.HasValue;

    private Result(T? value, Optional<TError> error) {
        this.value = value;
        this.Error = error;
    }

    public T? GetValueOrDefault() => this.HasError ? default : this.value;
    
    public static Result<T, TError> FromError(TError error) => new Result<T, TError>(default, new Optional<TError>(error));

    public static Result<T, TError> FromValue(T value) => new Result<T, TError>(value, default);

    public static Result<T, TError> Run(Func<T> factory, Func<Exception, TError> exceptionToError) {
        T result;
        try {
            result = factory();
        }
        catch (Exception e) {
            return FromError(exceptionToError(e));
        }

        return FromValue(result);
    }

    public static async Task<Result<T, TError>> RunAsync(Func<Task<T>> factory, Func<Exception, TError> exceptionToError) {
        T result;
        try {
            result = await factory();
        }
        catch (Exception e) {
            return FromError(exceptionToError(e));
        }

        return FromValue(result);
    }

    public override string ToString() {
        return this.HasError
            ? $"{nameof(Result<T, TError>)} (error = {this.Error})"
            : $"{nameof(Result<T, TError>)} (value = {this.value!})";
    }
}
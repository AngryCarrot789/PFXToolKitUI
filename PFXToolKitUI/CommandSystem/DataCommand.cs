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

using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// A helper command that automatically pulls the value keyed by a data key from the context data
/// When the data key is missing, the executability is <see cref="Executability.Invalid"/>
/// </summary>
/// <typeparam name="A">The data value type</typeparam>
public abstract class DataCommand<A> : Command {
    private readonly DataKey<A> dataKeyA;

    public DataCommand(DataKey<A> dataKeyA, bool allowMultipleExecutions = false) : base(allowMultipleExecutions) {
        this.dataKeyA = dataKeyA;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Executability.Invalid;

        return this.CanExecuteOverride(a, e);
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Task.CompletedTask;

        return this.ExecuteCommandAsync(a, e);
    }

    protected virtual Executability CanExecuteOverride(A a, CommandEventArgs e) => Executability.Valid;

    protected abstract Task ExecuteCommandAsync(A a, CommandEventArgs e);
}

/// <summary>
/// A helper command that automatically pulls two values keyed by data keys from the context data.
/// Missing data keys make the executability <see cref="Executability.Invalid"/>
/// </summary>
/// <typeparam name="A">The first value type</typeparam>
/// <typeparam name="B">The second value type</typeparam>
public abstract class DataCommand<A, B> : Command {
    private readonly DataKey<A> dataKeyA;
    private readonly DataKey<B> dataKeyB;

    public DataCommand(DataKey<A> dataKeyA, DataKey<B> dataKeyB, bool allowMultipleExecutions = false) : base(allowMultipleExecutions) {
        this.dataKeyA = dataKeyA;
        this.dataKeyB = dataKeyB;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Executability.Invalid;
        if (!this.dataKeyB.TryGetContext(e.ContextData, out B? b))
            return Executability.Invalid;

        return this.CanExecuteOverride(a, b, e);
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Task.CompletedTask;
        if (!this.dataKeyB.TryGetContext(e.ContextData, out B? b))
            return Task.CompletedTask;

        return this.ExecuteCommandAsync(a, b, e);
    }

    protected virtual Executability CanExecuteOverride(A a, B b, CommandEventArgs e) => Executability.Valid;

    protected abstract Task ExecuteCommandAsync(A a, B b, CommandEventArgs e);
}

/// <summary>
/// A helper command that automatically pulls three values keyed by data keys from the context data.
/// Missing data keys make the executability <see cref="Executability.Invalid"/>
/// </summary>
/// <typeparam name="A">The first value type</typeparam>
/// <typeparam name="B">The second value type</typeparam>
/// <typeparam name="C">The third value type</typeparam>
public abstract class DataCommand<A, B, C> : Command {
    private readonly DataKey<A> dataKeyA;
    private readonly DataKey<B> dataKeyB;
    private readonly DataKey<C> dataKeyC;

    public DataCommand(DataKey<A> dataKeyA, DataKey<B> dataKeyB, DataKey<C> dataKeyC, bool allowMultipleExecutions = false) : base(allowMultipleExecutions) {
        this.dataKeyA = dataKeyA;
        this.dataKeyB = dataKeyB;
        this.dataKeyC = dataKeyC;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Executability.Invalid;
        if (!this.dataKeyB.TryGetContext(e.ContextData, out B? b))
            return Executability.Invalid;
        if (!this.dataKeyC.TryGetContext(e.ContextData, out C? c))
            return Executability.Invalid;

        return this.CanExecuteOverride(a, b, c, e);
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!this.dataKeyA.TryGetContext(e.ContextData, out A? a))
            return Task.CompletedTask;
        if (!this.dataKeyB.TryGetContext(e.ContextData, out B? b))
            return Task.CompletedTask;
        if (!this.dataKeyC.TryGetContext(e.ContextData, out C? c))
            return Task.CompletedTask;

        return this.ExecuteCommandAsync(a, b, c, e);
    }

    protected virtual Executability CanExecuteOverride(A a, B b, C c, CommandEventArgs e) => Executability.Valid;

    protected abstract Task ExecuteCommandAsync(A a, B b, C value, CommandEventArgs e);
}
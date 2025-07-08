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
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PFXToolKitUI.Avalonia;

/// <summary>
/// A delegate around the avalonia dispatcher so that core projects can access it, since features RateLimitedDispatchAction require it
/// </summary>
public class AvaloniaDispatcherDelegate : IDispatcher {
    private static readonly Action EmptyAction = () => {
    };

    private readonly Dispatcher dispatcher;

    public AvaloniaDispatcherDelegate(Dispatcher dispatcher) {
        this.dispatcher = dispatcher;
    }

    public bool CheckAccess() => this.dispatcher.CheckAccess();

    public void VerifyAccess() => this.dispatcher.VerifyAccess();

    public void Invoke(Action action, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
            action();
        }
        else {
            this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
        }
    }

    public T Invoke<T>(Func<T> function, DispatchPriority priority) {
        if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
            return function();
        return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
    }

    public async Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
        await this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token);
    }

    public async Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
        return await this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token);
    }

    public void Post(Action action, DispatchPriority priority = DispatchPriority.Default) {
        this.dispatcher.Post(action, ToAvaloniaPriority(priority));
    }

    public Task Process(DispatchPriority priority) => this.InvokeAsync(EmptyAction, priority);

    public void InvokeShutdown() => this.dispatcher.InvokeShutdown();

    private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
        return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
    }
}
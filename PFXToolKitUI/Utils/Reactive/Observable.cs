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

namespace PFXToolKitUI.Utils.Reactive;

public static class Observable {
    /// <summary>
    /// Creates an observable object that can add and remove event handlers
    /// </summary>
    /// <param name="addHandler">Callback to add an event handler</param>
    /// <param name="removeHandler">Callback to remove an event handler</param>
    /// <typeparam name="T">The type of sender object</typeparam>
    /// <returns>The observable</returns>
    public static IEventObservable<T> ForEvent<T>(Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler) where T : class {
        return new EventObservableImpl<T>(addHandler, removeHandler);
    }

    /// <summary>
    /// Creates an observable object that can add and remove event handlers
    /// </summary>
    /// <param name="addHandler">Callback to add an event handler</param>
    /// <param name="removeHandler">Callback to remove an event handler</param>
    /// <typeparam name="T">The type of sender object</typeparam>
    /// <returns>The observable</returns>
    public static IEventObservable<T> ForEvent<T, TEventArgs>(Action<T, EventHandler<TEventArgs>> addHandler, Action<T, EventHandler<TEventArgs>> removeHandler) where T : class where TEventArgs : allows ref struct {
        return new EventObservableWithArgsImpl<T, TEventArgs>(addHandler, removeHandler);
    }

    /// <summary>
    /// Creates an observable object that can observable multiple other observables
    /// </summary>
    /// <param name="observables">The observables we can subscribe to, to observe</param>
    /// <typeparam name="T">The type of owner</typeparam>
    /// <returns>The observable</returns>
    public static IEventObservable<T> ForCombinedObservables<T>(params IEventObservable<T>[] observables) where T : class {
        return new EventObservableCombinedImpl<T>(observables);
    }

    private sealed class EventObservableImpl<T>(Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler) : IEventObservable<T> {
        private readonly Action<T, EventHandler> addHandler = addHandler;
        private readonly Action<T, EventHandler> removeHandler = removeHandler;

        public IDisposable Subscribe(T owner, object? state, EventHandler<T, object?> callback, bool invokeImmediately = true) {
            return new Subscriber(this, owner, state, callback, invokeImmediately);
        }

        private sealed class Subscriber : IDisposable {
            private readonly EventObservableImpl<T> impl;
            private readonly T owner;
            private readonly object? state;
            private readonly EventHandler<T, object?> callback;
            private readonly EventHandler myHandler;

            public Subscriber(EventObservableImpl<T> impl, T owner, object? state, EventHandler<T, object?> callback, bool initialCallback) {
                this.impl = impl;
                this.owner = owner;
                this.state = state;
                this.callback = callback;
                this.impl.addHandler(this.owner, this.myHandler = this.OnEvent);
                if (initialCallback)
                    callback(owner, state);
            }

            private void OnEvent(object? sender, EventArgs e) {
                this.callback(this.owner, this.state);
            }

            public void Dispose() {
                this.impl.removeHandler(this.owner, this.myHandler);
            }
        }
    }

    private sealed class EventObservableWithArgsImpl<T, TEventArgs>(Action<T, EventHandler<TEventArgs>> addHandler, Action<T, EventHandler<TEventArgs>> removeHandler) : IEventObservable<T> where TEventArgs : allows ref struct where T : class {
        private readonly Action<T, EventHandler<TEventArgs>> addHandler = addHandler;
        private readonly Action<T, EventHandler<TEventArgs>> removeHandler = removeHandler;

        public IDisposable Subscribe(T owner, object? state, EventHandler<T, object?> callback, bool invokeImmediately = true) {
            return new Subscriber(this, owner, state, callback, invokeImmediately);
        }

        private sealed class Subscriber : IDisposable {
            private readonly EventObservableWithArgsImpl<T, TEventArgs> impl;
            private readonly T owner;
            private readonly object? state;
            private readonly EventHandler<T, object?> callback;
            private readonly EventHandler<TEventArgs> myHandler;

            public Subscriber(EventObservableWithArgsImpl<T, TEventArgs> impl, T owner, object? state, EventHandler<T, object?> callback, bool initialCallback) {
                this.impl = impl;
                this.owner = owner;
                this.state = state;
                this.callback = callback;
                this.impl.addHandler(this.owner, this.myHandler = this.OnEvent);
                if (initialCallback)
                    callback(owner, state);
            }

            private void OnEvent(object? sender, TEventArgs eventArgs) {
                this.callback(this.owner, this.state);
            }

            public void Dispose() {
                this.impl.removeHandler(this.owner, this.myHandler);
            }
        }
    }

    private sealed class EventObservableCombinedImpl<T>(IEventObservable<T>[] observable) : IEventObservable<T> {
        private readonly IEventObservable<T>[] observable = observable;

        public IDisposable Subscribe(T owner, object? state, EventHandler<T, object?> callback, bool invokeImmediately = true) {
            return new Subscriber(this, owner, state, callback, invokeImmediately);
        }

        private sealed class Subscriber : IDisposable {
            private readonly IDisposable?[] subscriptions;
            private readonly T owner;
            private readonly object? state;
            private readonly EventHandler<T, object?> callback;

            public Subscriber(EventObservableCombinedImpl<T> impl, T owner, object? state, EventHandler<T, object?> callback, bool initialCallback) {
                this.owner = owner;
                this.state = state;
                this.callback = callback;
                this.subscriptions = new IDisposable[impl.observable.Length];
                for (int i = 0; i < impl.observable.Length; i++) {
                    this.subscriptions[i] = impl.observable[i].Subscribe(owner, this, static (_, s) => ((Subscriber) s!).OnEvent());
                }

                if (initialCallback)
                    callback(owner, state);
            }

            private void OnEvent() {
                this.callback(this.owner, this.state);
            }

            public void Dispose() {
                for (int i = 0; i < this.subscriptions.Length; i++) {
                    this.subscriptions[i]?.Dispose();
                    this.subscriptions[i] = null;
                }
            }
        }
    }
}
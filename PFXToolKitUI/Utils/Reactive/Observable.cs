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

namespace PFXToolKitUI.Utils.Reactive;

public static class Observable {
    public static IDisposable ForEvent<T>(T owner, Action<T> callback, Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler, bool initialCallback = true) where T : class {
        return new SimpleEventObserver<T>(owner, callback, addHandler, removeHandler, initialCallback);
    }

    public static IEventObservable<T> FromEvent<T>(Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler) where T : class {
        return new EventObservableImpl<T>(addHandler, removeHandler);
    }

    private sealed class EventObservableImpl<T>(Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler) : IEventObservable<T> {
        private readonly Action<T, EventHandler> addHandler = addHandler;
        private readonly Action<T, EventHandler> removeHandler = removeHandler;

        public IDisposable Subscribe(T owner, Action<T> callback, bool initialCallback = true) {
            return new Subscriber(this, owner, callback, initialCallback);
        }

        private sealed class Subscriber : IDisposable {
            private readonly EventObservableImpl<T> impl;
            private readonly T owner;
            private readonly Action<T> callback;
            private readonly EventHandler myHandler;

            public Subscriber(EventObservableImpl<T> impl, T owner, Action<T> callback, bool initialCallback) {
                this.impl = impl;
                this.owner = owner;
                this.callback = callback;
                this.impl.addHandler(this.owner, this.myHandler = this.OnEvent);
                if (initialCallback)
                    this.OnEvent(null, EventArgs.Empty);
            }

            private void OnEvent(object? sender, EventArgs e) {
                this.callback(this.owner);
            }

            public void Dispose() {
                this.impl.removeHandler(this.owner, this.myHandler);
            }
        }
    }

    private sealed class SimpleEventObserver<T> : IDisposable where T : class {
        private readonly T owner;
        private readonly Action<T> callback;
        private readonly Action<T, EventHandler> removeHandler;
        private readonly EventHandler myHandler;

        public SimpleEventObserver(T owner, Action<T> callback, Action<T, EventHandler> addHandler, Action<T, EventHandler> removeHandler, bool doInitialEvent) {
            this.owner = owner;
            this.callback = callback;
            this.removeHandler = removeHandler;
            this.myHandler = this.OnEvent;
            addHandler(owner, this.myHandler);
            if (doInitialEvent)
                this.OnEvent(owner, EventArgs.Empty);
        }

        private void OnEvent(object? theSender, EventArgs _) {
            Debug.Assert(theSender == this.owner);
            this.callback(this.owner);
        }

        public void Dispose() {
            this.removeHandler(this.owner, this.myHandler);
        }
    }
}
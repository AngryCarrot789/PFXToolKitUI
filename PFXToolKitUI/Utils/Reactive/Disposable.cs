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

public static class Disposable {
    /// <summary>
    /// Gets a disposable that does nothing when disposed
    /// </summary>
    public static IDisposable Empty => EmptyDisposable.Instance;

    /// <summary>
    /// Creates a disposable that invokes the given action
    /// </summary>
    public static IDisposable Create(Action dispose) {
        ArgumentNullException.ThrowIfNull(dispose);
        return new AnonymousDisposable(dispose);
    }

    /// <summary>
    /// Creates a disposable that invokes the given action
    /// </summary>
    public static IDisposable Create<TState>(TState state, Action<TState> dispose) {
        ArgumentNullException.ThrowIfNull(dispose);
        return new AnonymousDisposable<TState>(state, dispose);
    }

    public static IDisposable CreateComposite(IEnumerable<IDisposable> disposables) {
        ArgumentNullException.ThrowIfNull(disposables);
        return new CompositeDisposable(disposables.ToArray());
    }
    
    public static IDisposable CreateComposite(IDisposable[] disposables) {
        ArgumentNullException.ThrowIfNull(disposables);
        return new CompositeDisposable(disposables.ToArray());
    }

    /// <summary>
    /// Represents a disposable that does nothing on disposal.
    /// </summary>
    private sealed class EmptyDisposable : IDisposable {
        public static readonly EmptyDisposable Instance = new EmptyDisposable();

        public void Dispose() {
        }
    }

    internal sealed class AnonymousDisposable(Action dispose) : IDisposable {
        private volatile Action? dispose = dispose;

        public void Dispose() {
            Interlocked.Exchange(ref this.dispose, null)?.Invoke();
        }
    }

    internal sealed class AnonymousDisposable<TState>(TState state, Action<TState> dispose) : IDisposable {
        private TState state = state;
        private Action<TState>? dispose = dispose;

        public void Dispose() {
            Interlocked.Exchange(ref this.dispose, null)?.Invoke(this.state);
            this.state = default!;
        }
    }
    
    internal sealed class CompositeDisposable(IDisposable[] disposables) : IDisposable {
        private IDisposable[]? disposables = disposables;

        public void Dispose() {
            IDisposable[]? array = Interlocked.Exchange(ref this.disposables, null);
            if (array != null) {
                foreach (IDisposable d in array) {
                    d.Dispose();
                }
            }
        }
    }
}
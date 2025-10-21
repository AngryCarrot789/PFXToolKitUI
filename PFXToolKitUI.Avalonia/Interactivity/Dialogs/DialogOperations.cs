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

using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.Interactivity.Dialogs;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Interactivity.Dialogs;

public static class DialogOperations {
    /// <summary>
    /// Creates a dialog operation object from a desktop window.
    /// </summary>
    public static IDesktopDialogOperation<T> WrapDesktopWindow<T>(IDesktopWindow window) => new DesktopDialogOperation<T>(window);

    /// <summary>
    /// Creates a dialog operation object from an overlay window
    /// </summary>
    public static IDialogOperation<T> WrapOverlayWindow<T>(IOverlayWindow window) => new OverlayDialogOperation<T>(window);

    private class BaseDialogOperation<T> : IDialogOperation<T> {
        protected readonly IWindowBase window;
        private readonly TaskCompletionSource<T> tcs;

        public ITopLevel TopLevel => this.window;

        public Optional<T> Result { get; private set; }

        public bool IsCompleted => this.tcs.Task.IsCompleted;

        public BaseDialogOperation(IWindowBase window) {
            this.window = window;
            this.tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void SetResult(T result) {
            if (this.IsCompleted)
                throw new InvalidOperationException("Result already set");

            this.Result = result;
            this.tcs.SetResult(result);
            if (this.window.OpenState == OpenState.Open) {
                this.window.RequestClose(result);
            }
        }

        public void SetCancelled() {
            if (this.IsCompleted)
                throw new InvalidOperationException("Result already set");

            this.Result = Optional<T>.Empty;
            this.tcs.SetCanceled();
            if (this.window.OpenState == OpenState.Open) {
                this.window.RequestClose();
            }
        }

        public Task<T> WaitForResultAsync(CancellationToken cancellationToken = default) {
            return this.tcs.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class DesktopDialogOperation<T> : BaseDialogOperation<T>, IDesktopDialogOperation<T> {
        public DesktopDialogOperation(IDesktopWindow window) : base(window) {
            window.Closing += this.WindowOnClosing;
        }

        private void WindowOnClosing(IDesktopWindow sender, WindowCloseEventArgs e) {
            if (!this.IsCompleted)
                this.SetCancelled();
        }

        public void Activate() {
            ((IDesktopWindow) this.window).Activate();
        }
    }

    private sealed class OverlayDialogOperation<T> : BaseDialogOperation<T> {
        public OverlayDialogOperation(IOverlayWindow window) : base(window) {
            window.Closing += this.WindowOnClosing;
        }

        private void WindowOnClosing(IOverlayWindow sender, OverlayWindowCloseEventArgs e) {
            if (!this.IsCompleted)
                this.SetCancelled();
        }
    }
}
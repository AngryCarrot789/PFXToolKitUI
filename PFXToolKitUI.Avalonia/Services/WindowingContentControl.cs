// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of FramePFX.
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
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Utils.Destroying;

namespace PFXToolKitUI.Avalonia.Services;

/// <summary>
/// An abstract <see cref="UserControl"/> which is the base for any control placed in a window
/// </summary>
public abstract class WindowingContentControl : UserControl {
    public static readonly StyledProperty<IBrush?> WindowTitleBarBrushProperty = AvaloniaProperty.Register<WindowingContentControl, IBrush?>(nameof(WindowTitleBarBrush));
    public static readonly StyledProperty<IBrush?> WindowBorderBrushProperty = AvaloniaProperty.Register<WindowingContentControl, IBrush?>(nameof(WindowBorderBrush));
    public static readonly StyledProperty<string?> WindowTitleProperty = AvaloniaProperty.Register<WindowingContentControl, string?>(nameof(WindowTitle));
    public static readonly StyledProperty<TextAlignment> WindowTitleBarTextAlignmentProperty = AvaloniaProperty.Register<WindowingContentControl, TextAlignment>(nameof(WindowTitleBarTextAlignment));

    /// <summary>
    /// Gets or sets the brush that is bound to the title bar brush of the connected window
    /// </summary>
    public IBrush? WindowTitleBarBrush {
        get => this.GetValue(WindowTitleBarBrushProperty);
        set => this.SetValue(WindowTitleBarBrushProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the brush that is bound to the border brush of the connected window
    /// </summary>
    public IBrush? WindowBorderBrush {
        get => this.GetValue(WindowBorderBrushProperty);
        set => this.SetValue(WindowBorderBrushProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text that is bound to the title of the connected window
    /// </summary>
    public string? WindowTitle {
        get => this.GetValue(WindowTitleProperty);
        set => this.SetValue(WindowTitleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text alignment that is bound to the title bar's text alignment of the connected window
    /// </summary>
    public TextAlignment WindowTitleBarTextAlignment {
        get => this.GetValue(WindowTitleBarTextAlignmentProperty);
        set => this.SetValue(WindowTitleBarTextAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets the window this control is currently open in
    /// </summary>
    public IWindow? Window { get; private set; }

    private readonly IDisposable?[] bindingDisposables = new IDisposable?[4];

    private bool isWindowClosing;

    protected WindowingContentControl() {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        if (this.Window != null) {
            throw new InvalidOperationException(this.GetType().Name + $" was attached to VT when window was already open '{this.Window.Title}'");
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        if (this.Window != null && !this.isWindowClosing) {
            throw new InvalidOperationException(this.GetType().Name + $" was detached from VT when window was already open '{this.Window.Title}'");
        }
    }

    internal void OnOpened(IWindow window) {
        if (this.Window != null) {
            throw new InvalidOperationException("This control is already open in another window");
        }
        
        this.Window = window;
        this.Window.WindowClosing += this.OnWindowClosingAsync;
        this.OnWindowOpenedInternal();
        this.OnWindowOpened();
    }

    private Task<bool> OnWindowClosingAsync(IWindow sender, WindowCloseReason reason, bool isCancelled) {
        this.isWindowClosing = true;
        return Task.FromResult(false);
    }

    internal void OnClosed(IWindow window) {
        if (this.Window == null) {
            throw new InvalidOperationException("This control is not open in a window");
        }

        this.OnWindowClosedInternal();
        
        try {
            this.OnWindowClosed();
        }
        finally {
            this.Window = null;
        }
    }

    private void OnWindowOpenedInternal() {
        this.bindingDisposables[0] = this.GetObservable(WindowTitleBarBrushProperty).Subscribe(new AnonymousObserver<IBrush?>(brush => this.Window!.TitleBarBrush = brush));
        this.bindingDisposables[1] = this.GetObservable(WindowBorderBrushProperty).Subscribe(new AnonymousObserver<IBrush?>(brush => this.Window!.BorderBrush = brush));
        this.bindingDisposables[2] = this.GetObservable(WindowTitleProperty).Subscribe(new AnonymousObserver<string?>(text => this.Window!.Title = text));
        this.bindingDisposables[3] = this.GetObservable(WindowTitleBarTextAlignmentProperty).Subscribe(new AnonymousObserver<TextAlignment>(text => this.Window!.TitleBarTextAlignment = text));
    }
    
    private void OnWindowClosedInternal() {
        this.isWindowClosing = false;
        DisposableUtils.DisposeArray(this.bindingDisposables);
    }
    
    protected virtual void OnWindowOpened() {
        
    }
    
    protected virtual void OnWindowClosed() {
        
    }
}
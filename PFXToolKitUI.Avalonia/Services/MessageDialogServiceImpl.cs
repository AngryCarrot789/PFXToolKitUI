// 
// Copyright (c) 2023-2025 REghZy
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

using Avalonia.Controls;
using Avalonia.Input;
using PFXToolKitUI.Avalonia.Interactivity.Windowing;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Desktop;
using PFXToolKitUI.Avalonia.Interactivity.Windowing.Overlays;
using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.Messaging.Configurations;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Services;

public class MessageDialogServiceImpl : IMessageDialogService {
    public async Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null) {
        MessageBoxInfo info = new MessageBoxInfo(caption, message) { Buttons = buttons, DefaultButton = defaultButton, PersistentDialogName = persistentDialogName };
        info.SetDefaultButtonText();
        return await this.ShowMessage(info);
    }

    public async Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null) {
        MessageBoxInfo info = new MessageBoxInfo(caption, header, message) { Buttons = buttons, DefaultButton = defaultButton, PersistentDialogName = persistentDialogName };
        info.SetDefaultButtonText();
        return await this.ShowMessage(info);
    }

    public async Task<MessageBoxResult> ShowMessage(MessageBoxInfo info) {
        ArgumentNullException.ThrowIfNull(info);
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            return await ShowMessageMainThread(info);
        }

        return await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowMessageMainThread(info)).Unwrap();
    }

    private static bool IsPersistentButtonValidFor(MessageBoxResult button, MessageBoxButton buttons) {
        switch (button) {
            case MessageBoxResult.None:   return false;
            case MessageBoxResult.OK:     return buttons == MessageBoxButton.OK || buttons == MessageBoxButton.OKCancel;
            case MessageBoxResult.Cancel: return buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel;
            case MessageBoxResult.Yes:
            case MessageBoxResult.No:
                return buttons == MessageBoxButton.YesNoCancel || buttons == MessageBoxButton.YesNo;
            default: throw new ArgumentOutOfRangeException(nameof(button), button, null);
        }
    }

    private static async Task<MessageBoxResult> ShowMessageMainThread(MessageBoxInfo info) {
        ArgumentNullException.ThrowIfNull(info);
        if (!string.IsNullOrWhiteSpace(info.PersistentDialogName)) {
            PersistentDialogResult persistent = PersistentDialogResult.GetInstance(info.PersistentDialogName);
            if (persistent.Button is MessageBoxResult result) {
                if (IsPersistentButtonValidFor(result, info.Buttons)) {
                    return result;
                }

                // Someone tinkered with the config file maybe
                persistent.SetButton(null);
                AppLogger.Instance.WriteLine("Warning: PersistentDialogResult's button is not valid for the message box.");
                AppLogger.Instance.WriteLine($"BTN = {result}, Buttons = {info.Buttons}, DialogName = {info.PersistentDialogName}");
            }
        }

        ITopLevel? parentTopLevel = TopLevelContextUtils.GetTopLevelFromContext();
        if (parentTopLevel != null) {
            MessageBoxView view = new MessageBoxView() {
                MessageBoxData = info
            };
            
            Optional<MessageBoxResult> result = await WindowContextUtils.UseWindowAsync(parentTopLevel, (w) => ShowMessageBoxInWindow(info, view, w), (m, w) => ShowMessageBoxInOverlay(info, view, m , w));
            return result.GetValueOrDefault();
        }

        AppLogger.Instance.WriteLine(parentTopLevel == null
            ? "Could not find top level to show message dialog in"
            : $"Unsupported top level '{parentTopLevel.GetType()}' type for message box");
        Console.WriteLine($"[{info.Caption}] {info.Header}");
        Console.WriteLine($"  {info.Message}");
        return MessageBoxResult.None;
    }

    private static async Task<MessageBoxResult> ShowMessageBoxInWindow(MessageBoxInfo info, MessageBoxView view, IDesktopWindow parentWindow) {
        IDesktopWindow window = parentWindow.WindowManager.CreateWindow(new WindowBuilder() {
            Title = info.Caption,
            Parent = parentWindow,
            Content = view,
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 300, MinHeight = 100,
            MaxWidth = 800, MaxHeight = 800,
            TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue)
        });

        window.Control.AddHandler(InputElement.KeyDownEvent, WindowOnKeyDown);
        window.Opening += WindowOnWindowOpening;
        window.Opened += WindowOnWindowOpened;
        window.Closed += WindowOnWindowClosed;

        MessageBoxResult mbResult = await window.ShowDialogAsync() as MessageBoxResult? ?? MessageBoxResult.None;
        view.MessageBoxData = null; // unhook models' event handlers

        if (!string.IsNullOrWhiteSpace(info.PersistentDialogName) && info.AlwaysUseThisResult && mbResult != MessageBoxResult.None) {
            PersistentDialogResult.GetInstance(info.PersistentDialogName).SetButton(mbResult, info.AlwaysUseThisResultUntilAppCloses);
        }

        return mbResult;

        void WindowOnWindowOpening(IDesktopWindow s, EventArgs e) => view.OnWindowOpening(s);
        void WindowOnWindowOpened(IDesktopWindow s, EventArgs e) => view.OnWindowOpened(s);
        void WindowOnWindowClosed(IDesktopWindow s, WindowCloseEventArgs e) => view.OnWindowClosed(s);

        void WindowOnKeyDown(object? s, KeyEventArgs e) {
            if (!e.Handled && e.Key == Key.Escape) {
                if (view.OwnerWindow != null && view.OwnerWindow.OpenState == OpenState.Open) {
                    view.Close(MessageBoxResult.None);
                }
            }
        }
    }

    private static async Task<MessageBoxResult> ShowMessageBoxInOverlay(MessageBoxInfo info, MessageBoxView view, IOverlayWindowManager manager, IOverlayWindow? parentWindow) {
        IOverlayWindow window = manager.CreateWindow(new OverlayWindowBuilder() {
            TitleBar = new OverlayWindowTitleBarInfo() {
                Title = info.Caption,
                TitleBarBrush = BrushManager.Instance.GetDynamicThemeBrush("ABrush.Tone4.Background.Static"),
            },
            Parent = parentWindow,
            Content = view,
            MinWidth = 300, MinHeight = 100,
            MaxWidth = 800, MaxHeight = 800,
            SizeToContent = SizeToContent.WidthAndHeight,
            BorderBrush = BrushManager.Instance.CreateConstant(SKColors.DodgerBlue)
        });

        window.Control.AddHandler(InputElement.KeyDownEvent, WindowOnKeyDown);
        window.Opening += WindowOnWindowOpening;
        window.Opened += WindowOnWindowOpened;
        window.Closed += WindowOnWindowClosed;

        MessageBoxResult mbResult = await window.ShowDialogAsync() as MessageBoxResult? ?? MessageBoxResult.None;
        view.MessageBoxData = null; // unhook models' event handlers

        if (!string.IsNullOrWhiteSpace(info.PersistentDialogName) && info.AlwaysUseThisResult && mbResult != MessageBoxResult.None) {
            PersistentDialogResult.GetInstance(info.PersistentDialogName).SetButton(mbResult, info.AlwaysUseThisResultUntilAppCloses);
        }

        return mbResult;

        void WindowOnWindowOpening(IOverlayWindow s, EventArgs e) => view.OnWindowOpening(s);
        void WindowOnWindowOpened(IOverlayWindow s, EventArgs e) => view.OnWindowOpened(s);
        void WindowOnWindowClosed(IOverlayWindow s, OverlayWindowCloseEventArgs e) => view.OnWindowClosed(s);

        void WindowOnKeyDown(object? s, KeyEventArgs e) {
            if (!e.Handled && e.Key == Key.Escape) {
                if (view.OwnerWindow != null && view.OwnerWindow.OpenState == OpenState.Open) {
                    view.Close(MessageBoxResult.None);
                }
            }
        }
    }
}
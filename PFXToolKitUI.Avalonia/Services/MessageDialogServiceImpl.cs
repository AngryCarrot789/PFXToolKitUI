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
    public MessageDialogServiceImpl() {
    }

    public async Task<MessageBoxResult> ShowMessage(MessageBoxInfo info) {
        ArgumentNullException.ThrowIfNull(info);
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            return await ShowMessageBoxInMainThread(info);
        }

        // Capture the current context, since the caller is probably
        // from an activity started by a command which has top-level info
        return await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowMessageBoxInMainThread(info), captureContext: true).Unwrap();
    }

    private static async Task<MessageBoxResult> ShowMessageBoxInMainThread(MessageBoxInfo info) {
        ArgumentNullException.ThrowIfNull(info);
        if (!string.IsNullOrWhiteSpace(info.PersistentDialogName)) {
            PersistentDialogResult persistent = PersistentDialogResult.GetInstance(info.PersistentDialogName);
            if (persistent.Button is MessageBoxResult result) {
                if (result.IsValidResultOf(info.Buttons)) {
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
            IWindowBase? window = WindowContextUtils.CreateWindow(parentTopLevel, (w) => ShowMessageBoxInWindow(info, w), (m, w) => ShowMessageBoxInOverlay(info, m, w));
            if (window != null) {
                MessageBoxView view = (MessageBoxView) window.Content!;

                // Register cancellation to force-close dialog even if myTask is not completed
                await using CancellationTokenRegistration register = info.DialogCancellation.Register(static t => {
                    ApplicationPFX.Instance.Dispatcher.Post(static void (winRef) => {
                        IWindowBase? win = (IWindowBase?) ((WeakReference) winRef!).Target;
                        if (win != null && win.OpenState == OpenState.Open) {
                            win.RequestClose();
                        }
                    }, t);
                }, new WeakReference(window));

                MessageBoxResult result = await window.ShowDialogAsync() as MessageBoxResult? ?? MessageBoxResult.None;
                view.MessageBoxData = null; // unhook models' event handlers

                if (!string.IsNullOrWhiteSpace(info.PersistentDialogName) && info.AlwaysUseThisResult && result != MessageBoxResult.None) {
                    PersistentDialogResult.GetInstance(info.PersistentDialogName).SetButton(result, info.AlwaysUseThisResultUntilAppCloses);
                }

                return result;
            }
        }

        AppLogger.Instance.WriteLine(parentTopLevel == null
            ? "Could not find top level to show message dialog in"
            : $"Unsupported top level '{parentTopLevel.GetType()}' type for message box");
        Console.WriteLine($"[{info.Caption}] {info.Header}");
        Console.WriteLine($"  {info.Message}");
        return MessageBoxResult.None;
    }

    private static IDesktopWindow ShowMessageBoxInWindow(MessageBoxInfo info, IDesktopWindow parentWindow) {
        MessageBoxView view = new MessageBoxView() {
            MessageBoxData = info
        };

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

        window.Control.AddHandler(InputElement.KeyDownEvent, (s, e) => {
            if (!e.Handled && e.Key == Key.Escape) {
                if (view.OwnerWindow != null) {
                    view.RequestClose(MessageBoxResult.None);
                }
            }
        });

        window.Opening += (s, e) => view.OnWindowOpening(s);
        window.Opened += (s, e) => view.OnWindowOpened(s);
        window.Closed += (s, e) => view.OnWindowClosed(s);
        return window;
    }

    private static IOverlayWindow ShowMessageBoxInOverlay(MessageBoxInfo info, IOverlayWindowManager manager, IOverlayWindow? parentWindow) {
        MessageBoxView view = new MessageBoxView() {
            MessageBoxData = info
        };

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

        window.Control.AddHandler(InputElement.KeyDownEvent, (s, e) => {
            if (!e.Handled && e.Key == Key.Escape) {
                if (view.OwnerWindow != null) {
                    view.RequestClose(MessageBoxResult.None);
                }
            }
        });

        window.Opening += (s, e) => view.OnWindowOpening(s);
        window.Opened += (s, e) => view.OnWindowOpened(s);
        window.Closed += (s, e) => view.OnWindowClosed(s);
        return window;
    }
}
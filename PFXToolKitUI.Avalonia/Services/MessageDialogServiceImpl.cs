// 
// Copyright (c) 2023-2025 REghZy
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

using PFXToolKitUI.Avalonia.Services.Messages.Windows;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.Messaging.Configurations;
using PFXToolKitUI.Utils;

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
        Validate.NotNull(info);
        if (ApplicationPFX.Instance.Dispatcher.CheckAccess()) {
            return await ShowMessageMainThread(info);
        }
        else {
            return await ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => ShowMessageMainThread(info)).Unwrap();
        }
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
        Validate.NotNull(info);
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

        if (WindowingSystem.TryGetInstance(out WindowingSystem? system) && system.TryGetActiveWindow(out DesktopWindow? activeWindow)) {
            MessageBoxWindow window = new MessageBoxWindow() { MessageBoxData = info };
            MessageBoxResult? result = await system.Register(window).ShowDialog<MessageBoxResult?>(activeWindow);
            window.MessageBoxData = null;

            MessageBoxResult trueResult = result ?? MessageBoxResult.None;
            if (!string.IsNullOrWhiteSpace(info.PersistentDialogName) && info.AlwaysUseThisResult) {
                PersistentDialogResult.GetInstance(info.PersistentDialogName).SetButton(trueResult, info.AlwaysUseThisResultUntilAppCloses);
            }

            return trueResult;
        }
        else {
            Console.WriteLine("Warning: no message box library available");
            Console.WriteLine($"[{info.Caption}] {info.Header}");
            Console.WriteLine($"  {info.Message}");
        }

        return MessageBoxResult.None;
    }
}
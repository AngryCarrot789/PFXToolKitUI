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

using PFXToolKitUI.Avalonia.Services.Messages.Controls;
using PFXToolKitUI.Avalonia.Services.Windowing;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.Services;

public class MessageDialogServiceImpl : IMessageDialogService {
    public async Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None) {
        MessageBoxInfo info = new MessageBoxInfo(caption, message) { Buttons = buttons, DefaultButton = defaultButton };
        info.SetDefaultButtonText();
        return await this.ShowMessage(info);
    }

    public async Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None) {
        MessageBoxInfo info = new MessageBoxInfo(caption, header, message) { Buttons = buttons, DefaultButton = defaultButton };
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

    private static async Task<MessageBoxResult> ShowMessageMainThread(MessageBoxInfo info) {
        Validate.NotNull(info);
        if (WindowingSystem.TryGetInstance(out WindowingSystem? system) && system.TryGetActiveWindow(out IWindow? activeWindow)) {
            MessageBoxControl control = new MessageBoxControl() { MessageBoxData = info };
            
            IWindow dialog = system.CreateWindow(control);
            MessageBoxResult? result = await dialog.ShowDialog<MessageBoxResult?>(activeWindow);
            control.MessageBoxData = null;
            
            return result ?? MessageBoxResult.None;
        }
        else {
            Console.WriteLine("Warning: no message box library available");
            Console.WriteLine($"[{info.Caption}] {info.Header}");
            Console.WriteLine($"  {info.Message}");
        }

        return MessageBoxResult.None;
    }
}
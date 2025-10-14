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

using PFXToolKitUI.Logging;

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// An interface for the message dialog service for the application. The ShowMessage methods can be
/// called from any thread, but it's best to call from the main thread so that the active window is predicable
/// </summary>
public interface IMessageDialogService {
    public static IMessageDialogService Instance {
        get {
            if (!ApplicationPFX.TryGetComponent(out IMessageDialogService? service)) {
                return EmptyMessageDialogService.Instance;
            }

            return service;
        }
    }

    /// <summary>
    /// Shows a dialog message
    /// </summary>
    /// <param name="caption">The window titlebar message</param>
    /// <param name="message">The main message content</param>
    /// <param name="buttons">The buttons to show</param>
    /// <param name="defaultButton">The default selected button. Default is none</param>
    /// <param name="persistentDialogName">
    /// A unique ID for this type of message dialog that allows the user to specify to remember 
    /// their selection and use the same result next time without showing the dialog
    /// </param>
    /// <param name="dialogCancellation">
    /// A cancellation token that notifies the dialog to close, causing this method
    /// to produce <see cref="MessageBoxResult.None"/>
    /// </param>
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null, CancellationToken dialogCancellation = default) {
        return this.ShowMessage(new MessageBoxInfo(caption, message) {
            Buttons = buttons,
            DefaultButton = defaultButton,
            PersistentDialogName = persistentDialogName,
            DialogCancellation = dialogCancellation
        }.SetDefaultButtonText());
    }

    /// <summary>
    /// Shows a dialog message
    /// </summary>
    /// <param name="caption">The window titlebar message</param>
    /// <param name="header">A message presented in bold above the message, a less concise caption but still short</param>
    /// <param name="message">The main message content</param>
    /// <param name="buttons">The buttons to show</param>
    /// <param name="defaultButton">The default selected button. Default is none</param>
    /// <param name="persistentDialogName">
    /// A unique ID for this type of message dialog that allows the user to specify to remember 
    /// their selection and use the same result next time without showing the dialog
    /// </param>
    /// <param name="dialogCancellation">
    /// A cancellation token that notifies the dialog to close, causing this method
    /// to produce <see cref="MessageBoxResult.None"/>
    /// </param>
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null, CancellationToken dialogCancellation = default) {
        return this.ShowMessage(new MessageBoxInfo(caption, header, message) {
            Buttons = buttons,
            DefaultButton = defaultButton,
            PersistentDialogName = persistentDialogName,
            DialogCancellation = dialogCancellation
        }.SetDefaultButtonText());
    }

    /// <summary>
    /// Shows a message box dialog that is dynamically customisable; 3 buttons, caption, header and message
    /// </summary>
    /// <param name="info">The data for the message box</param>
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(MessageBoxInfo info);
}

public sealed class EmptyMessageDialogService : IMessageDialogService {
    /// <summary>
    /// Gets a singleton global instance
    /// </summary>
    public static EmptyMessageDialogService Instance { get; } = new EmptyMessageDialogService();

    public Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null) {
        PrintToLogs(caption, message, buttons);
        return Task.FromResult(MessageBoxResult.None);
    }

    public Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null) {
        PrintToLogs(caption, message, buttons);
        return Task.FromResult(MessageBoxResult.None);
    }

    public Task<MessageBoxResult> ShowMessage(MessageBoxInfo info) {
        PrintToLogs(info.Caption ?? "(no caption)", info.Message ?? "(no message)", info.Buttons);
        return Task.FromResult(MessageBoxResult.None);
    }

    private static void PrintToLogs(string caption, string message, MessageBoxButtons buttons) {
        AppLogger.Instance.WriteLine("No message dialog service available");
        AppLogger.Instance.WriteLine("# " + caption);
        AppLogger.Instance.WriteLine("#   " + message);
        AppLogger.Instance.WriteLine("#   (" + buttons + ")");
    }
}
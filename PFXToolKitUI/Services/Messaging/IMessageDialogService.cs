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

namespace PFXToolKitUI.Services.Messaging;

/// <summary>
/// An interface for the message dialog service for the application. The ShowMessage methods can be
/// called from any thread, but it's best to call from the main thread so that the active window is predicable
/// </summary>
public interface IMessageDialogService {
    public static IMessageDialogService Instance => ApplicationPFX.GetService<IMessageDialogService>();

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
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null);

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
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultButton = MessageBoxResult.None, string? persistentDialogName = null);

    /// <summary>
    /// Shows a message box dialog that is dynamically customisable; 3 buttons, caption, header and message
    /// </summary>
    /// <param name="info">The data for the message box</param>
    /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
    Task<MessageBoxResult> ShowMessage(MessageBoxInfo info);
}
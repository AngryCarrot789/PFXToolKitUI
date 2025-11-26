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

using PFXToolKitUI.Logging;
using PFXToolKitUI.Services.Messaging;

namespace PFXToolKitUI.Utils;

/// <summary>
/// A helper class for handling exceptions thrown from a command
/// </summary>
public static class LogExceptionHelper {
    public static Task ShowExceptionMessage(this IMessageDialogService service, string caption, Exception exception, bool printToLogger = true) {
        return service.ShowExceptionMessage(caption, "An exception occurred while executing command", exception, printToLogger);
    }

    /// <summary>
    /// Shows a message box with the caption and message, and specifies the exception as the extra details. Optionally prints the exception to the app logger
    /// </summary>
    /// <param name="service">Message dialog service</param>
    /// <param name="caption">Dialog caption</param>
    /// <param name="message">Dialog message</param>
    /// <param name="exception">Exception</param>
    /// <param name="printToLogger">True to print the exception to the logger too</param>
    public static async Task ShowExceptionMessage(this IMessageDialogService service, string caption, string message, Exception exception, bool printToLogger = true) {
        string exceptionText = exception.GetToString();

        if (printToLogger) {
            AppLogger.Instance.WriteLine(caption + " - " + message);
            AppLogger.Instance.WriteLine(exceptionText);
        }

        await service.ShowMessage(new MessageBoxInfo(caption, message) {
            Buttons = MessageBoxButtons.OK,
            Icon = MessageBoxIcons.ErrorIcon,
            ExtraDetails = exceptionText
        });
    }
}
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
    public static Task ShowMessageAndPrintToLogs(string caption, Exception ex) {
        return ShowMessageAndPrintToLogs(caption, "An exception occurred while executing command", ex);
    }
    
    public static async Task ShowMessageAndPrintToLogs(string caption, string message, Exception ex) {
        AppLogger.Instance.WriteLine(caption + " - " + message);
        AppLogger.Instance.WriteLine(ex.GetToString());
        
        await IMessageDialogService.Instance.ShowMessage(caption, message + Environment.NewLine + Environment.NewLine + "See logs for more info");
    }
}
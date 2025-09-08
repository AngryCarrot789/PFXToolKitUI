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

using PFXToolKitUI.CommandSystem;

namespace PFXToolKitUI.History.Commands;

public abstract class BaseHistoryCommand : Command {
    protected sealed override Executability CanExecuteCore(CommandEventArgs e) {
        if (!HistoryManager.DataKey.TryGetContext(e.ContextData, out HistoryManager? manager))
            return Executability.Invalid;

        return this.CanExecuteHistory(manager, e);
    }

    protected virtual Executability CanExecuteHistory(HistoryManager manager, CommandEventArgs e) {
        return Executability.Valid;
    }

    protected sealed override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!HistoryManager.DataKey.TryGetContext(e.ContextData, out HistoryManager? manager))
            return Task.CompletedTask;

        return this.ExecuteHistoryCommandAsync(manager, e);
    }

    protected abstract Task ExecuteHistoryCommandAsync(HistoryManager manager, CommandEventArgs e);
}
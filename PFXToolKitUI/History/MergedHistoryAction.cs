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

namespace PFXToolKitUI.History;

public class MergedHistoryAction : IHistoryAction {
    private readonly IHistoryAction[] actions;

    public MergedHistoryAction(IHistoryAction[] actions) {
        this.actions = actions ?? throw new ArgumentNullException(nameof(actions));
    }

    public async Task<bool> Undo() {
        for (int i = this.actions.Length - 1; i >= 0; i--) {
            if (!await this.actions[i].Undo()) {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> Redo() {
        for (int i = 0; i < this.actions.Length; i++) {
            if (!await this.actions[i].Redo()) {
                return false;
            }
        }

        return true;
    }

    public void Dispose() {
        foreach (IHistoryAction t in this.actions) {
            t.Dispose();
        }
    }
}
// 
// Copyright (c) 2024-2025 REghZy
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

namespace PFXToolKitUI;

public class LambdaStartupManager : IStartupManager {
    private readonly object action;

    public LambdaStartupManager(Action action) {
        this.action = action;
    }

    public LambdaStartupManager(Action<string[]> actionWithArgs) {
        this.action = actionWithArgs;
    }

    public LambdaStartupManager(Func<Task> asyncAction) {
        this.action = asyncAction;
    }

    public LambdaStartupManager(Func<string[], Task> asyncActionWithArgs) {
        this.action = asyncActionWithArgs;
    }

    public async Task OnApplicationStartupWithArgs(string[] args) {
        switch (this.action) {
            case Action<string[]> a:
                a(args);
                return;
            case Action a:
                a();
                return;
            case Func<string[], Task> a: await a(args); break;
            case Func<Task> a:           await a(); break;
            default:                     return;
        }
    }
}
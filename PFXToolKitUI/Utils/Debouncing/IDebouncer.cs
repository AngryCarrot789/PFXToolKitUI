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

namespace PFXToolKitUI.Utils.Debouncing;

/// <summary>
/// A debouncer is used to prevent an action from being invoked until enough time (or enough of something) has elapsed.
/// </summary>
public interface IDebouncer {
    /// <summary>
    /// Gets whether it is appropriate to call <see cref="Run"/>.
    /// <para>
    /// For example, this might true when enough time has passed since <see cref="Run"/> was last invoked
    /// </para> 
    /// </summary>
    /// <value></value>
    bool CanRun { get; }
    
    /// <summary>
    /// Returns true when <see cref="Run"/> has been invoked at least once
    /// </summary>
    /// <value></value>
    bool HasRunOnce { get; }

    /// <summary>
    /// Runs the operation, regardless of <see cref="CanRun"/>
    /// </summary>
    void Run();

    /// <summary>
    /// Runs the operation if <see cref="CanRun"/> if true
    /// </summary>
    void RunIf() => this.GetThenRunIf();

    /// <summary>
    /// Gets the state of <see cref="CanRun"/> and then calls <see cref="Run"/>
    /// </summary>
    /// <returns>The result of <see cref="CanRun"/> before invoking <see cref="Run"/></returns>
    bool GetThenRun() {
        var canTrigger = this.CanRun;
        this.Run();
        return canTrigger;
    }

    /// <summary>
    /// Gets the state of <see cref="CanRun"/>, and if it's true, we invoke <see cref="Run"/>
    /// </summary>
    /// <returns>The result of <see cref="CanRun"/> before invoking <see cref="Run"/></returns>
    bool GetThenRunIf() {
        if (!this.CanRun)
            return false;

        this.Run();
        return true;
    }

    /// <summary>
    /// Resets the debouncing state. This should make <see cref="CanRun"/> return true
    /// </summary>
    void Reset();
}
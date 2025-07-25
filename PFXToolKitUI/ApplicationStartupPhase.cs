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

namespace PFXToolKitUI;

public enum ApplicationStartupPhase {
    // TODO: do we even need this many phases???

    /// <summary>
    /// The application is in its default state; the instance just exists but nothing else has happened
    /// </summary>
    Default,

    /// <summary>
    /// The application is in the pre-init stage, which is where services and commands are being created
    /// </summary>
    PreLoad,

    /// <summary>
    /// The application is initialising core components and is loading plugins
    /// </summary>
    Loading,

    /// <summary>
    /// The application is fully initialised and is about to enter the running state
    /// </summary>
    FullyLoaded,

    /// <summary>
    /// The application is in its running state.
    /// </summary>
    Running,

    /// <summary>
    /// The application is in the process of shutting down (e.g. the last editor window was closed)
    /// </summary>
    Stopping,

    /// <summary>
    /// The application is stopped and the process will exit soon
    /// </summary>
    Stopped
}
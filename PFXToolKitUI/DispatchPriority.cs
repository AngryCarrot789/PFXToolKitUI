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

namespace PFXToolKitUI;

/// <summary>
/// A priority for a dispatcher operation. A lower value means a lower priority and will be processed after higher priority operations.
/// </summary>
public enum DispatchPriority {
    /*
     * Example: You set a control's IsVisible property to true.
     *
     *   If you then InvokeAsync an operation using a priority lower than Loaded,
     *   the control's Loaded event is fired first, then your operation is run
     *
     *   But if you InvokeAsync an operation using a higher priority (above Loaded),
     *   your operation runs first, then Loaded will be fired some point after your operation completes
     *   (sooner or later depending on how many extra operations are scheduled above Loaded)
     *
     *   If you InvokeAsync on the Loaded priority, the behaviour can be complicated
     */
    
    /// <summary>The lowest foreground dispatcher priority</summary>
    Default = 0,

    /// <summary>The job will be processed with the same priority as input</summary>
    Input = Default - 1,

    /// <summary>The job will be processed after other non-idle operations have completed</summary>
    Background = Input - 1,

    /// <summary>The job will be processed after background operations have completed</summary>
    ContextIdle = Background - 1,

    /// <summary>The job will be processed when the application is idle</summary>
    ApplicationIdle = ContextIdle - 1,

    /// <summary>The job will be processed when the system is idle. This is the minimum possible priority that's actually dispatched</summary>
    SystemIdle = ApplicationIdle - 1,

    /// <summary>The job will be processed after layout and render but before input</summary>
    Loaded = Default + 1,

    /// <summary>A special priority for platforms with UI render timer or for forced full rasterization requests</summary>
    TimedOrForcedRender = Loaded + 1,

    /// <summary>A special priority for jobs to run after rendering, usually to synchronize native control host positions, IME, etc.</summary>
    AfterRender = TimedOrForcedRender + 1,

    /// <summary>The job will be processed with the same priority as render</summary>
    Render = AfterRender + 1,
    
    /// <summary>The job will be processed before render operations</summary>
    BeforeRender = Render + 1,
    
    /// <summary>The job will be processed with normal priority. This is the same as <see cref="BeforeRender"/></summary>
    Normal = BeforeRender,

    /// <summary>The job will be processed before other asynchronous operations. This is the highest foreground priority</summary>
    Send = Normal + 1,
}
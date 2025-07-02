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

// #define MEASURE_INHERITANCE_CACHE_HITS_MISSES

using Avalonia.Controls;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.ToolTips;

/// <summary>
/// An interface for an object that can receive open/close notifications from a control when its tooltip opens.
/// <para>
/// This interface is only used by the <see cref="ToolTipEx"/> service; you simply specify the control type (which
/// may implement this interface) as the value for <see cref="ToolTipEx.TipTypeProperty"/>
/// </para>
/// <para>
/// When a control attempts to open its tooltip, <see cref="OnOpened"/> is invoked. When the tooltip closes,
/// <see cref="OnClosed"/> is invoked. The opened callback is never called more than once in a row without close being called.
/// </para>
/// </summary>
public interface IToolTipControl {
    /// <summary>
    /// Invoked when the tool tip opens on the owner control. The context data is provided via <see cref="DataManager.GetFullContextData"/>
    /// </summary>
    /// <param name="owner">The owner of the tooltip</param>
    /// <param name="data">The inherited context data of the tooltip's owner control</param>
    void OnOpened(Control owner, IContextData data);

    /// <summary>
    /// Invoked when the tool tip closes.
    /// </summary>
    /// <param name="owner">The owner of the tooltip</param>
    void OnClosed(Control owner);

    // Practically zero point since the odds of context data changing while a tool tip is open is extremely rare
    // void OnContextChanged(IContextData data);
}
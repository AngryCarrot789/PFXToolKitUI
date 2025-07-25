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

using Avalonia.Controls.Primitives;
using PFXToolKitUI.Configurations;

namespace PFXToolKitUI.Avalonia.Configurations.Pages;

/// <summary>
/// The base class for a page control
/// </summary>
public class BaseConfigurationPageControl : TemplatedControl {
    public ConfigurationPage? Page { get; private set; }

    public void Connect(ConfigurationPage page) {
        ArgumentNullException.ThrowIfNull(page);
        if (this.Page != null)
            throw new InvalidOperationException("Already connected");

        this.Page = page;
        this.OnConnected();
    }

    public void Disconnect() {
        if (this.Page == null)
            throw new InvalidOperationException("Not connected");

        this.OnDisconnected();
        this.Page = null;
    }

    public virtual void OnConnected() {
    }

    public virtual void OnDisconnected() {
    }
}
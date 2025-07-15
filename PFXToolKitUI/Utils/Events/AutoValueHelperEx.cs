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

using System.Diagnostics;

namespace PFXToolKitUI.Utils.Events;

public class AutoValueHelperEx<T> : IRelayEventHandler where T : class {
    private readonly Action<T> update;
    private readonly EventRelayStorage storage;
    private readonly SenderEventRelay eventRelay;
    private T? value;

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    public T? Value {
        get => this.value;
        set {
            if (!ReferenceEquals(this.value, value)) {
                if (this.value != null) {
                    this.storage.RemoveHandler(this.value, this, this.eventRelay);
                }

                if ((this.value = value) != null) {
                    this.storage.AddHandler(value!, this, this.eventRelay);
                    this.update(value!);
                }
            }
        }
    }

    public AutoValueHelperEx(Action<T> update, string eventName, EventRelayStorage storage) {
        this.storage = storage;
        this.eventRelay = storage.GetEventRelay(typeof(T), eventName);
        this.update = update;
    }

    void IRelayEventHandler.OnEvent(object sender) => this.Update();

    /// <summary>
    /// Invokes the update callback manually.
    /// </summary>
    public void Update() {
        Debug.Assert(this.value != null);
        this.update(this.value!);
    }
}
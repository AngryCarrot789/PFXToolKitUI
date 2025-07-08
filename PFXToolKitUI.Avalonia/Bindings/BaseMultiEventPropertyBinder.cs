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
using PFXToolKitUI.Utils.Events;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base binder class which implements an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseMultiEventPropertyBinder<TModel> : BaseBinder<TModel>, IRelayEventHandler where TModel : class {
    private readonly SenderEventRelay[] autoEventHelpers;
    private readonly IDispatcher dispatcher;
    private volatile RapidDispatchActionEx? rdaUpdateControl;
    private volatile int rdaLock;

    /// <summary>
    /// Gets or sets (init only) if the event can be fired from any thread. Default is true
    /// <para>
    /// When true, we will use a <see cref="RapidDispatchActionEx"/> to dispatch the <see cref="BaseBinder{TModel}.UpdateControl"/> signal
    /// back to the main thread. When false, if the event is fired outside of the main thread, we throw an exception
    /// </para> 
    /// </summary>
    public bool AllowEventFiredOnAnyThread { get; init; } = true;

    /// <summary>
    /// Gets or sets (init only) the dispatch priority for a <see cref="RapidDispatchActionEx"/> when <see cref="AllowEventFiredOnAnyThread"/> is true
    /// </summary>
    public DispatchPriority DispatchPriority { get; init; } = DispatchPriority.Normal;

    protected BaseMultiEventPropertyBinder(params string[] eventNames) {
        this.autoEventHelpers = new SenderEventRelay[eventNames.Length];
        for (int i = 0; i < eventNames.Length; i++) {
            this.autoEventHelpers[i] = EventRelayBinderUtils.GetEventRelay(typeof(TModel), eventNames[i]);
        }

        this.dispatcher = ApplicationPFX.Instance.Dispatcher;
    }
    
    void IRelayEventHandler.OnEventFired() => this.OnAnyEventFired();

    /// <summary>
    /// Invoked by the model's value changed event handler. By default this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnAnyEventFired() {
        if (!this.dispatcher.CheckAccess()) {
            if (this.AllowEventFiredOnAnyThread) {
                if (this.rdaUpdateControl == null) {
                    while (Interlocked.CompareExchange(ref this.rdaLock, 1, 0) != 0)
                        Thread.Yield();

                    try {
                        if (this.rdaUpdateControl == null)
                            this.rdaUpdateControl = RapidDispatchActionEx.ForSync(this.UpdateControl, this.dispatcher, this.DispatchPriority, "BaseMultiEventPropertyBinder");
                    }
                    finally {
                        this.rdaLock = 0;
                    }
                }

                this.rdaUpdateControl!.InvokeAsync();
                return;
            }

            Debugger.Break();
            throw new InvalidOperationException("This property binder requires the event be fired on the main thread");
        }

        this.UpdateControl();
    }

    protected override void OnAttached() {
        foreach (SenderEventRelay aeh in this.autoEventHelpers)
            EventRelayBinderUtils.OnAttached(this.myModel!, this, aeh);
    }

    protected override void OnDetached() {
        foreach (SenderEventRelay aeh in this.autoEventHelpers)
            EventRelayBinderUtils.OnDetached(this.myModel!, this, aeh);
    }
}
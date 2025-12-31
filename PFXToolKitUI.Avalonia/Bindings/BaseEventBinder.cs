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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PFXToolKitUI.EventHelpers;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base binder class which implements an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseEventBinder<TModel> : BaseBinder<TModel>, IRelayEventHandler where TModel : class {
    private readonly EventWrapper[] eventRelays;
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

    protected BaseEventBinder(string eventName) {
        this.eventRelays = [EventRelayStorage.UIStorage.GetEventRelay(typeof(TModel), eventName)];
        this.dispatcher = ApplicationPFX.Instance.Dispatcher;
    }

    protected BaseEventBinder(string[] eventNames) {
        this.eventRelays = new EventWrapper[eventNames.Length];
        for (int i = 0; i < eventNames.Length; i++) {
            this.eventRelays[i] = EventRelayStorage.UIStorage.GetEventRelay(typeof(TModel), eventNames[i]);
        }

        this.dispatcher = ApplicationPFX.Instance.Dispatcher;
    }

    void IRelayEventHandler.OnEvent(object sender) {
        if (this.dispatcher.CheckAccess()) {
            this.UpdateControl();
        }
        else {
            this.OnModelValueChangedOffMainThread();
        }
    }

    private void OnModelValueChangedOffMainThread() {
        if (this.AllowEventFiredOnAnyThread) {
            RapidDispatchActionEx? rda = this.rdaUpdateControl;
            if (rda == null) {
                while (Interlocked.CompareExchange(ref this.rdaLock, 1, 0) != 0)
                    Thread.Yield();

                try {
                    if ((rda = this.rdaUpdateControl) == null)
                        this.rdaUpdateControl = rda = RapidDispatchActionEx.ForSync(this.UpdateControl, this.dispatcher, this.DispatchPriority, "BaseEventPropertyBinder");
                }
                finally {
                    this.rdaLock = 0;
                }
            }

            rda.InvokeAsync();
            return;
        }

        Debugger.Break();
        ThrowInvalidOperationException();
        return;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowInvalidOperationException() {
            throw new InvalidOperationException("This property binder requires the event be fired on the main thread");
        }
    }

    protected override void OnAttached() {
        foreach (EventWrapper aeh in this.eventRelays)
            EventRelayStorage.UIStorage.AddHandler(this.Model, this, aeh);
    }

    protected override void OnDetached() {
        foreach (EventWrapper aeh in this.eventRelays)
            EventRelayStorage.UIStorage.RemoveHandler(this.Model, this, aeh);
    }
}
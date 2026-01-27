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
using PFXToolKitUI.Utils.RDA;
using PFXToolKitUI.Utils.Reactive;

namespace PFXToolKitUI.Avalonia.Bindings;

/// <summary>
/// A base binder class which implements an event handler for the model which fires the <see cref="IBinder.UpdateControl"/> method
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public abstract class BaseObservableBinder<TModel> : BaseBinder<TModel> where TModel : class {
    private readonly IDispatcher dispatcher;
    private volatile RapidDispatchActionEx? rdaUpdateControl;
    private volatile int rdaLock;
    private readonly IEventObservable<TModel>[] observables;
    private readonly IDisposable?[] subscriptions;
    private EventHandler<TModel, object?>? m_OnEvent;

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

    protected BaseObservableBinder(IEventObservable<TModel> observable) {
        this.observables = [observable];
        this.subscriptions = new IDisposable[1];
        this.dispatcher = ApplicationPFX.Instance.Dispatcher;
    }

    protected BaseObservableBinder(IEventObservable<TModel>[] observables) {
        this.observables = observables;
        this.subscriptions = new IDisposable[observables.Length];
        this.dispatcher = ApplicationPFX.Instance.Dispatcher;
    }

    /// <summary>
    /// Invoked by the model's value changed event handler. By default this method invokes <see cref="IBinder.UpdateControl"/>
    /// </summary>
    protected virtual void OnModelValueChanged() {
        if (!this.dispatcher.CheckAccess()) {
            if (this.AllowEventFiredOnAnyThread) {
                if (this.rdaUpdateControl == null) {
                    while (Interlocked.CompareExchange(ref this.rdaLock, 1, 0) != 0)
                        Thread.Yield();

                    try {
                        if (this.rdaUpdateControl == null)
                            this.rdaUpdateControl = RapidDispatchActionEx.ForSync(this.UpdateControl, this.dispatcher, this.DispatchPriority, "BaseEventPropertyBinder");
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
        EventHandler<TModel, object?> handler = this.m_OnEvent ??= this.OnEvent;
        for (int i = 0; i < this.observables.Length; i++) {
            this.subscriptions[i] = this.observables[i].Subscribe(this.Model, null, handler, invokeImmediately: false /* BaseBinder calls UpdateControl after OnAttached */);
        }
    }

    protected override void OnDetached() {
        for (int i = 0; i < this.subscriptions.Length; i++) {
            this.subscriptions[i]!.Dispose();
            this.subscriptions[i] = null;
        }
    }

    private void OnEvent(TModel sender, object? state) => this.OnModelValueChanged();
}
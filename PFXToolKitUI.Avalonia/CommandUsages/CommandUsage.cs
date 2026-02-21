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

using Avalonia;
using Avalonia.Interactivity;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils.RDA;

namespace PFXToolKitUI.Avalonia.CommandUsages;

/// <summary>
/// A command usage is a ui-place-specific usage of a command, e.g. a push or toggle button, a menu or context item.
/// These accept a connected <see cref="AvaloniaObject"/>, in which events can be attached and detached in order to
/// things like execute the command.
/// <para>
/// This class automatically listens for contextual data changes, which triggers the
/// executability state to be re-queried from the command based on the new contextual data
/// </para>
/// </summary>
public abstract class CommandUsage {
    // Since its invoke method is only called from the main thread,
    // there's no need for the extended version
    private RapidDispatchActionEx? delayedContextUpdate;
    private EventHandler<RoutedEventArgs>? d_OnInheritedContextChangedImmediately;

    /// <summary>
    /// Gets the target command ID for this usage instance. This is not null, not empty and
    /// does not consist of only whitespaces; it's a fully valid command ID
    /// </summary>
    public string CommandId { get; }

    public AvaloniaObject? Control { get; private set; }

    /// <summary>
    /// Gets whether this usage is currently connected to a control. When disconnecting, this is set
    /// to false while <see cref="Control"/> remains non-null, until <see cref="OnDisconnected"/> has returned
    /// </summary>
    public bool IsConnected { get; private set; }

    protected CommandUsage(string commandId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        this.CommandId = commandId;
    }

    /// <summary>
    /// Gets the current available context for our connected control. Returns null if disconnected
    /// </summary>
    /// <returns>The context data</returns>
    public IContextData? GetContextData() => this.Control != null && this.IsConnected ? DataManager.GetFullContextData(this.Control) : null;

    /// <summary>
    /// Connects to the given object control
    /// </summary>
    /// <param name="control">Control to connect to</param>
    /// <exception cref="ArgumentNullException">Control is null</exception>
    public void Connect(AvaloniaObject control) {
        this.Control = control ?? throw new ArgumentNullException(nameof(control));
        try {
            this.OnConnecting();
        }
        catch (Exception e) {
            this.Control = null;
            throw new InvalidOperationException("Attempt to connect with invalid control", e);
        }

        this.IsConnected = true;
        DataManager.AddInheritedContextChangedHandler(control, this.d_OnInheritedContextChangedImmediately ??= this.OnInheritedContextChangedImmediately);
        this.OnConnected();
    }

    /// <summary>
    /// Disconnects from this control
    /// </summary>
    /// <exception cref="InvalidCastException">Not connected</exception>
    public void Disconnect() {
        if (this.Control == null)
            throw new InvalidCastException("Not connected");

        DataManager.RemoveInheritedContextChangedHandler(this.Control, this.d_OnInheritedContextChangedImmediately!);
        this.IsConnected = false;
        this.OnDisconnected();
        this.Control = null;
    }

    private void OnInheritedContextChangedImmediately(object? sender, RoutedEventArgs e) {
        this.OnContextChanged();
    }

    protected virtual void OnConnecting() {
        // check shit before actually connecting
    }

    protected virtual void OnConnected() => this.OnContextChanged();

    protected virtual void OnDisconnected() => this.OnContextChanged();

    /// <summary>
    /// Called immediately when our inherited context changes
    /// </summary>
    protected virtual void OnContextChanged() {
        this.UpdateCanExecuteLater();
    }

    /// <summary>
    /// Schedules the <see cref="UpdateCanExecute"/> method to be invoked later. This is called by <see cref="OnContextChanged"/>
    /// </summary>
    public void UpdateCanExecuteLater() {
        RapidDispatchActionEx guard = this.delayedContextUpdate ??= RapidDispatchActionEx.ForSync(this.UpdateCanExecute, DispatchPriority.Loaded, "UpdateCanExecute");
        guard.InvokeAsync();
    }

    private void SignalOnCanExecuteChanged(object? sender, EventArgs e) {
        this.UpdateCanExecuteLater();
    }
    
    /// <summary>
    /// Adds a handler to the signal's <see cref="CommandUsageSignal.CanExecuteChanged"/> event that invokes <see cref="UpdateCanExecuteLater"/>.
    /// </summary>
    /// <param name="signal">The signal</param>
    public void AddCommandSignalHandler(CommandUsageSignal signal) {
        signal.CanExecuteChanged += this.SignalOnCanExecuteChanged;
    }

    /// <summary>
    /// Removes a handler from the signal's <see cref="CommandUsageSignal.CanExecuteChanged"/> event that invokes <see cref="UpdateCanExecuteLater"/>.
    /// </summary>
    /// <param name="signal">The signal</param>
    public void RemoveCommandSignalHandler(CommandUsageSignal signal) {
        signal.CanExecuteChanged -= this.SignalOnCanExecuteChanged;
    }

    public virtual void UpdateCanExecute() {
        IContextData? ctx = this.GetContextData();
        this.OnUpdateForCanExecuteState(
            ctx != null
                ? CommandManager.Instance.CanExecute(this.CommandId, ctx, null, this.GetContextRegistryForCommand())
                : Executability.Invalid);
    }

    protected virtual void OnUpdateForCanExecuteState(Executability state) {
    }

    /// <summary>
    /// Gets the context registry associated with this command usage. Should only return a non-null value when used within a menu, of course
    /// </summary>
    /// <returns></returns>
    protected virtual ContextRegistry? GetContextRegistryForCommand() => null;
}
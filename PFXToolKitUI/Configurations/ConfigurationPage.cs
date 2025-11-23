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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Events;

namespace PFXToolKitUI.Configurations;

/// <summary>
/// The base class for a configuration page model. A page is what is presented
/// on the right side of the settings dialog
/// <para>
/// A custom page may wish to implement their own sub-page or sub-section system that implement similar behaviour
/// to these pages (such as <see cref="Apply"/>), or it may be an entirely custom page (e.g. shortcut editor tree)
/// </para>
/// </summary>
public abstract class ConfigurationPage : ITransferableData {
    private bool ignoreDataParamChange;

    /// <summary>
    /// Gets the configuration context currently applicable to this page. This is updated when the page
    /// is being viewed by the user. An instance of a page cannot be concurrently viewed multiple times,
    /// hence why this property is not a list of configuration contexts
    /// </summary>
    public ConfigurationContext? ActiveContext { get; private set; }

    /// <summary>
    /// Gets or sets if this page has been modified since being loaded. This determines if it requires saving
    /// </summary>
    public bool IsModified {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.IsModifiedChanged);
    }

    public TransferableData TransferableData { get; }

    public event EventHandler? IsModifiedChanged;

    protected ConfigurationPage() {
        this.TransferableData = new TransferableData(this);
    }

    private static void MarkModifiedOnDataParameterChanged(object? sender, DataParameterValueChangedEventArgs e) {
        if (!((ConfigurationPage) e.Owner).ignoreDataParamChange)
            ((ConfigurationPage) e.Owner).IsModified = true;
    }

    protected static void AffectsIsModified(DataParameter parameter) {
        parameter.ValueChanged += MarkModifiedOnDataParameterChanged;
    }

    protected static void AffectsIsModified(DataParameter p1, DataParameter p2) {
        p1.ValueChanged += MarkModifiedOnDataParameterChanged;
        p2.ValueChanged += MarkModifiedOnDataParameterChanged;
    }

    protected static void AffectsIsModified(DataParameter p1, DataParameter p2, DataParameter p3) {
        p1.ValueChanged += MarkModifiedOnDataParameterChanged;
        p2.ValueChanged += MarkModifiedOnDataParameterChanged;
        p3.ValueChanged += MarkModifiedOnDataParameterChanged;
    }

    protected static void AffectsIsModified(DataParameter p1, DataParameter p2, DataParameter p3, DataParameter p4) {
        p1.ValueChanged += MarkModifiedOnDataParameterChanged;
        p2.ValueChanged += MarkModifiedOnDataParameterChanged;
        p3.ValueChanged += MarkModifiedOnDataParameterChanged;
        p4.ValueChanged += MarkModifiedOnDataParameterChanged;
    }

    protected static void AffectsIsModified(params DataParameter[] parameters) {
        DataParameter.AddMultipleHandlers(MarkModifiedOnDataParameterChanged, parameters);
    }

    /// <summary>
    /// Applies the current data into the application. This is invoked when the user clicks
    /// the Apply (which just applies) or Save button (which applies then closes the dialog)
    /// </summary>
    /// <param name="errors">
    /// A list of errors encountered while applying changes (e.g. bugs or conflicting
    /// values, maybe a and b cannot both be true)
    /// </param>
    public abstract ValueTask Apply(List<ApplyChangesFailureEntry>? errors);

    /// <summary>
    /// Reverts any changes this page made to the application outside the standard apply/cancel behaviour.
    /// For example, the theme manager page modifies the colours of the UI in real time,
    /// but uses this to reset the colours back to their original state, but if you click apply and then
    /// make changes again, this method will only revert those changes after <see cref="Apply"/>
    /// <para>
    /// This is invoked when the Cancel button is clicked in the UI
    /// </para>
    /// </summary>
    /// <param name="errors">
    /// A list of errors encountered while applying changes (e.g. bugs or conflicting
    /// values, maybe a and b cannot both be true)
    /// </param>
    public virtual ValueTask RevertLiveChanges(List<ApplyChangesFailureEntry>? errors) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when a new configuration context is created for use with a configuration manager
    /// in which this page exists in. This is typically invoked recursively
    /// <para>
    /// At this point, the settings dialog will not be fully loaded, and so,
    /// we are free to modify our internal state without notifying of changes
    /// </para>
    /// <para>
    /// Basically, this is where you load data from the application in preparation for the UI.
    /// </para>
    /// </summary>
    /// <param name="context">The context that was created</param>
    /// <returns></returns>
    protected virtual ValueTask OnContextCreated(ConfigurationContext context) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when the context is no longer in use, meaning the settings dialog was closed.
    /// This method is always called and can be used to for example unregistered global event handlers.
    /// </summary>
    /// <param name="context">The context that was destroyed</param>
    /// <returns></returns>
    protected virtual ValueTask OnContextDestroyed(ConfigurationContext context) {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Invoked when our active context changes. One of the parameters will be null, unless I forget
    /// to update this comment. This happens when the page is no longer being viewed (either by the user clicking
    /// another page, or closing the dialog), and so maybe the page shouldn't listen to intense application updates
    /// <para>
    /// If this method is ever called, it will always be at least AFTER <see cref="OnContextCreated"/> and BEFORE <see cref="OnContextDestroyed"/>.
    /// Basically, it will always be called either zero times or at least two times during page lifetime
    /// </para>
    /// </summary>
    /// <param name="oldContext">The previous context</param>
    /// <param name="newContext">The new context</param>
    protected virtual void OnIsViewingChanged(ConfigurationContext? oldContext, ConfigurationContext? newContext) {
    }

    internal static void InternalSetContext(ConfigurationPage page, ConfigurationContext? context) {
        ConfigurationContext? oldContext = page.ActiveContext;
        if (ReferenceEquals(oldContext, context)) {
            return;
        }

        page.ActiveContext = context;
        if (context != null)
            page.ignoreDataParamChange = true;

        page.OnIsViewingChanged(oldContext, context);

        if (context != null)
            page.ignoreDataParamChange = false;
    }

    internal static ValueTask InternalOnContextCreated(ConfigurationPage page, ConfigurationContext context) => page.OnContextCreated(context);
    internal static ValueTask InternalOnContextDestroyed(ConfigurationPage page, ConfigurationContext context) => page.OnContextDestroyed(context);
}
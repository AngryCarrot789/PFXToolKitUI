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
using System.Text;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Interactivity;

/// <summary>
/// A class that is used to store and extract contextual information from WPF components.
/// <para>
/// This class generates inherited-merged contextual data for the logical tree, that is, all contextual data
/// is accumulated and cached in each element from logical parents, and the <see cref="InheritedContextChangedEvent"/> is fired
/// on the element and all of its visual children when that parent's <see cref="ContextDataProperty"/> changes,
/// allowing listeners to do anything they want (e.g. re-query command executability based on available context)
/// </para>
/// </summary>
public class DataManager {
    private static int totalSuspensionCount; // used for performance reasons

    /// <summary>
    /// The context data property, used to store contextual information relative to a specific avalonia object.
    /// <para>
    /// The underlying context data object must not be modified (as in, it must stay immutable), because inherited
    /// context does not reflect the changes made. Invoke <see cref="SetContextData"/> to publish inheritable changes,
    /// or just call <see cref="InvalidateInheritedContext"/> when it is mutated
    /// </para>
    /// </summary>
    private static readonly AttachedProperty<IControlContextData?> ContextDataProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, IControlContextData?>("ContextData");

    private static readonly AttachedProperty<IContextData?> InheritedContextDataProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, IContextData?>("InheritedContextData");

    public static readonly AttachedProperty<int> SuspendedInvalidationCountProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, int>("SuspendedInvalidationCount");

    public static readonly AttachedProperty<DataKey?> DataContextDataKeyProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, DataKey?>("DataContextDataKey");

    public static readonly AttachedProperty<DataKey?> SelfDataKeyProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, DataKey?>("SelfDataKey", coerce: (o, key) => {
            if (key != null && !key.DataType.IsInstanceOfType(o)) {
                StringBuilder sb = new StringBuilder(128);
                sb.AppendLine("Attempt to set SelfDataKey to a key whose data type is incompatible with the instance type.");
                sb.AppendLine($"  Instance Type = {o.GetType()}");
                sb.AppendLine($"  DataKey ID = {key.Id}, Data Type = {key.DataType}");
                throw new InvalidOperationException(sb.ToString());
            }
            
            return key;
        });

    /// <summary>
    /// An event that gets raised on every single visual child (similar to tunnelling) when its inherited context
    /// becomes invalid (caused by either manual invalidation or when the context data is modified for any parent element).
    /// </summary>
    public static readonly RoutedEvent InheritedContextChangedEvent =
        RoutedEvent.Register<DataManager, RoutedEventArgs>("InheritedContextChanged", RoutingStrategies.Direct);

    static DataManager() {
        Visual.VisualParentProperty.Changed.AddClassHandler<Visual, Visual?>(OnVisualParentChanged);
        DataContextDataKeyProperty.Changed.AddClassHandler<AvaloniaObject, DataKey?>(OnDataContextDataKeyPropertyChanged);
        SelfDataKeyProperty.Changed.AddClassHandler<AvaloniaObject, DataKey?>(OnSelfDataKeyPropertyChanged);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void OnVisualParentChanged(Visual sender, AvaloniaPropertyChangedEventArgs<Visual?> e) {
        if (sender.IsAttachedToVisualTree()) {
            InvalidateInheritedContext(sender);
        }
        // else {
        //     Debug.WriteLine($"[{nameof(OnVisualParentChanged)}] Skipped InvalidateInheritedContext for {sender} (not attached to VT)");
        // }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void OnDataContextDataKeyPropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs<DataKey?> e) {
        if (sender is StyledElement element) {
            if (!e.NewValue.HasValue)
                element.DataContextChanged -= OnDataContextChanged;
            else if (!e.OldValue.HasValue)
                element.DataContextChanged += OnDataContextChanged;
        }
        
        if (!(sender is Visual visual) || visual.IsAttachedToVisualTree()) {
            InvalidateInheritedContext(sender);
        }
    }

    private static void OnDataContextChanged(object? sender, EventArgs e) {
        Debug.Assert(((AvaloniaObject) sender!).GetValue(DataContextDataKeyProperty) != null);
        if (!(sender is Visual visual) || visual.IsAttachedToVisualTree()) {
            InvalidateInheritedContext((StyledElement) sender!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void OnSelfDataKeyPropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs<DataKey?> e) {
        if (!(sender is Visual visual) || visual.IsAttachedToVisualTree()) {
            InvalidateInheritedContext(sender);
        }
    }

    /// <summary>
    /// Adds a handler for <see cref="InheritedContextChangedEvent"/> to the given target
    /// </summary>
    /// <param name="target">The target object</param>
    /// <param name="handler">The event handler</param>
    /// <exception cref="ArgumentException">The target is not <see cref="IInputElement"/> and therefore cannot accept event handlers</exception>
    public static void AddInheritedContextChangedHandler(AvaloniaObject target, EventHandler<RoutedEventArgs> handler) {
        if (!(target is IInputElement input))
            throw new ArgumentException("Target is not an instance of " + nameof(IInputElement));
        input.AddHandler(InheritedContextChangedEvent, handler);
    }

    /// <summary>
    /// Removes a handler for <see cref="InheritedContextChangedEvent"/> from the given target
    /// </summary>
    /// <param name="target">The target object</param>
    /// <param name="handler">The event handler</param>
    /// <exception cref="ArgumentException">The target is not <see cref="IInputElement"/> and therefore cannot accept event handlers</exception>
    public static void RemoveInheritedContextChangedHandler(AvaloniaObject target, EventHandler<RoutedEventArgs> handler) {
        if (!(target is IInputElement input))
            throw new ArgumentException("Target is not an instance of " + nameof(IInputElement));
        input.RemoveHandler(InheritedContextChangedEvent, handler);
    }

    /// <summary>
    /// Invalidates the inherited-merged contextual data for the element and its entire visual child
    /// tree, firing the <see cref="InheritedContextChangedEvent"/> for each visual child, allowing
    /// them to re-query their new valid contextual data.
    /// <para>
    /// This is the same method called when an element is removed from the visual tree or an element's context data changes
    /// </para>
    /// </summary>
    /// <param name="element">The element to invalidate the inherited context data of, along with its visual tree</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void InvalidateInheritedContext(AvaloniaObject element) {
        if (totalSuspensionCount > 0 && GetSuspendedInvalidationCount(element) > 0) {
            return;
        }

        // For MemoryEngine360, this takes about 0.8ms for EngineWindow, which uses a fairly
        // dense control tree and quite a few handlers of the invalidation events
        // long a = Time.GetSystemTicks();

        InvalidateInheritedContextAndChildren(element);
        RaiseInheritedContextChanged(element);

        // long b = Time.GetSystemTicks() - a;
        // Debug.WriteLine($"{nameof(InvalidateInheritedContext)} on {element.GetType().Name} = {(b / Time.TICK_PER_MILLIS_D).ToString()}");
    }

    /// <summary>
    /// Raises the <see cref="InheritedContextChangedEvent"/> event for the element's visual tree (self and all children recursively).
    /// <para>
    /// This does not affect the return value of <see cref="GetFullContextData"/>. Use <see cref="InvalidateInheritedContext"/> instead
    /// </para>
    /// </summary>
    /// <param name="element">The element to raise the event for, along with its visual tree</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void RaiseInheritedContextChanged(AvaloniaObject element) {
        RaiseEventRecursive(element, new RoutedEventArgs(InheritedContextChangedEvent, element));
        // Debug.WriteLine($"Context invalidated: {element.GetType().Name}");
    }

    /// <summary>
    /// Clears the context data for the given element
    /// </summary>
    /// <param name="element">The element</param>
    public static void ClearContextData(AvaloniaObject element, bool invalidate = true) {
        if (element.IsSet(ContextDataProperty)) {
            element.ClearValue(ContextDataProperty);
            if (invalidate) {
                InvalidateInheritedContext(element);
            }
        }
    }

    /// <summary>
    /// Gets or creates this control's context data
    /// </summary>
    /// <param name="element">The element to get the context data from</param>
    /// <returns>The context data</returns>
    public static IControlContextData GetContextData(AvaloniaObject element) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data == null) {
            element.SetValue(ContextDataProperty, data = new ControlContextData(element));
        }

        return data;
    }

    public static bool TryGetContextData(AvaloniaObject element, [NotNullWhen(true)] out IControlContextData? data) {
        return (data = element.GetValue(ContextDataProperty)) != null;
    }

    /// <summary>
    /// Sets the element's context data to a delegating instance. Inherited context data can be stacked 
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="inherited">The inherited context data</param>
    public static void SetDelegateContextData(AvaloniaObject element, IContextData inherited) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        element.SetValue(ContextDataProperty, data == null ? new InheritingControlContextData(element, inherited) : data.CreateInherited(inherited));
        InvalidateInheritedContext(element);
    }

    /// <summary>
    /// Makes the element's context data no longer inherit from anything. Only does anything if the current context is actually inheriting
    /// </summary>
    /// <param name="element">The element</param>
    public static void ClearDelegateContextData(AvaloniaObject element) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data is InheritingControlContextData inheriting) {
            element.SetValue(ContextDataProperty, new ControlContextData(element, inheriting));
            InvalidateInheritedContext(element);
        }
    }

    public static void SwapDelegateContextData(AvaloniaObject element, IContextData newDelegate) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data is InheritingControlContextData inheriting) {
            data = new ControlContextData(element, inheriting).CreateInherited(newDelegate);
        }
        else {
            data = new InheritingControlContextData(element, newDelegate);
        }

        element.SetValue(ContextDataProperty, data);
        InvalidateInheritedContext(element);
    }

    /// <summary>
    /// Gets the full inherited data context, which is the merged results of the entire visual tree
    /// starting from the root to the given component.
    /// <para>
    /// See <see cref="EvaluateContextDataRaw"/> for more info on how this works
    /// </para>
    /// <para>
    /// The returned data should not be modified (despite being potentially mutable), since changes will not be reflected down the visual tree
    /// </para>
    /// </summary>
    /// <param name="component">The target object</param>
    /// <returns>The fully inherited and merged context data. Will always be non-null</returns>
    public static IContextData GetFullContextData(AvaloniaObject component) {
        IContextData? value = component.GetValue(InheritedContextDataProperty);
        if (value == null) {
            component.SetValue(InheritedContextDataProperty, value = EvaluateContextDataRaw(component));
        }

        return value;
    }

    // private static readonly PropertyInfo TreeLevelPropertyInfo = typeof(Visual).GetProperty("TreeLevel", BindingFlagsInstPrivDeclared) ?? throw new Exception("Could not find TreeLevel property");

    /// <summary>
    /// Does bottom-to-top scan of the element's visual tree, and then merged all of the data keys associated
    /// with each object from top to bottom, ensuring the bottom of the visual tree has the most power over
    /// the final data context key values. <see cref="GetFullContextData"/> should be preferred over this
    /// method, however, that method calls this one anyway (and invalidates the results for every visual child
    /// when the <see cref="InheritedContextChangedEvent"/> is about to be fired)
    /// </summary>
    /// <param name="source">The element to get the full context of</param>
    /// <returns>The context</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static IContextData EvaluateContextDataRaw(AvaloniaObject source) {
        ProviderContextData ctx = new ProviderContextData();

        // EvaluateContextDataRaw with about 26 elements takes about 20 microseconds
        List<AvaloniaObject> hierarchy = new List<AvaloniaObject>(32) { source };
        for (StyledElement? parent = (source as StyledElement)?.Parent; parent != null; parent = parent.Parent) {
            hierarchy.Add(parent);
        }

        // Scan top-down in order to allow deeper objects' entries to override higher up entries
        for (int i = hierarchy.Count - 1; i >= 0; i--) {
            AvaloniaObject control = hierarchy[i];
            IControlContextData? data = control.GetBaseValue(ContextDataProperty).GetValueOrDefault();
            if (data != null && data.Count > 0) {
                ctx.Merge(data);
            }

            DataKey? key;
            if (control is StyledElement element && (key = GetDataContextDataKey(control)) != null) {
                object? dc = element.DataContext;
                if (dc == null || key.DataType.IsInstanceOfType(dc)) {
                    ctx.SetValueRaw(key.Id, element.DataContext);
                }
                else {
                    Debug.WriteLine($"DataContextDataKey of {control.GetType().Name} was invalid for its actual data context.");
                    Debug.WriteLine($"  DataContext = {dc.GetType()}");
                    Debug.WriteLine($"  DataKey ID = {key.Id}, Data Type = {key.DataType}");
                }
            }

            if ((key = GetSelfDataKey(control)) != null) {
                ctx.SetValueRaw(key.Id, control);
            }
        }

        return ctx;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void InvalidateInheritedContextAndChildren(AvaloniaObject obj) {
        // Debug.WriteLine($"InvalidateInheritedContext for {obj}");

        // SetValue is around 2x faster than ClearValue, and either way, ClearValue isn't
        // very useful here since inheritance isn't used, and the value will most
        // likely be re-calculated very near in the future possibly via dispatcher on background priority

        // Checking there is a value before setting generally improves runtime performance, since SetValue is fairly intensive
        if (obj.GetValue((AvaloniaProperty) InheritedContextDataProperty) != null)
            obj.SetValue(InheritedContextDataProperty, null);

        if (obj is Visual) {
            AvaloniaList<Visual> list = Unsafe.As<AvaloniaList<Visual>>(Unsafe.As<AvaloniaObject, Visual>(ref obj).GetVisualChildren());
            foreach (Visual child in list)
                InvalidateInheritedContextAndChildren(child);
        }
    }

    // Minimize stack usage as much as possible by using 'as' cast
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void RaiseEventRecursive(AvaloniaObject target, RoutedEventArgs args) {
        if (target is InputElement element) {
            element.RaiseEvent(args);
            AvaloniaList<Visual> list = Unsafe.As<AvaloniaList<Visual>>(element.GetVisualChildren());
            foreach (Visual child in list)
                RaiseEventRecursive(child, args);
        }
        else {
            Debugger.Break();
        }
    }

    // Not sure if this will work as well as the above...
    // private static void WalkVisualTreeForParentContextInvalidated(AvaloniaObject obj, RoutedEventArgs args) {
    //     obj.SetValue(InheritedContextDataProperty, null);
    //     (obj as IInputElement)?.RaiseEvent(args);
    //     for (int i = 0, count = VisualTreeHelper.GetChildrenCount(obj); i < count; i++) {
    //         WalkVisualTreeForParentContextInvalidated(VisualTreeHelper.GetChild(obj, i), args);
    //     }
    // }

    // Until this is actually useful, not gonna implement it.
    // May have to implement it if the performance of invalidating the visual tree
    // becomes a problem (e.g. context data changes many times during an operation)

    /// <summary>
    /// Can be used to suspend the automatic merged context invalidation of the visual tree when an element's context changes, for performance reasons.
    /// <para>
    /// Failure to dispose the returned reference will permanently disable merged context invalidation
    /// </para>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="autoInvalidateOnUnsuspended"></param>
    /// <returns></returns>
    public static IDisposable SuspendMergedContextInvalidation(AvaloniaObject obj, bool autoInvalidateOnUnsuspended = true) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        totalSuspensionCount++;
        return new SuspendInvalidation(obj, autoInvalidateOnUnsuspended);
    }

    public static int GetSuspendedInvalidationCount(AvaloniaObject element) {
        return element.GetValue(SuspendedInvalidationCountProperty);
    }

    /// <summary>
    /// Sets the data key used to key the data context object
    /// </summary>
    public static void SetDataContextDataKey(AvaloniaObject obj, DataKey? value) => obj.SetValue(DataContextDataKeyProperty, value);

    /// <summary>
    /// Gets the data key used to key the data context object
    /// </summary>
    public static DataKey? GetDataContextDataKey(AvaloniaObject obj) => obj.GetValue(DataContextDataKeyProperty);

    /// <summary>
    /// Sets the data key used to key the control instance itself
    /// </summary>
    public static void SetSelfDataKey(AvaloniaObject obj, DataKey? value) => obj.SetValue(SelfDataKeyProperty, value);

    /// <summary>
    /// Gets the data key used to key the control instance itself
    /// </summary>
    public static DataKey? GetSelfDataKey(AvaloniaObject obj) => obj.GetValue(SelfDataKeyProperty);

    private class SuspendInvalidation : IDisposable {
        private AvaloniaObject? target;
        private readonly bool autoInvalidateOnUnsuspended;

        public SuspendInvalidation(AvaloniaObject target, bool autoInvalidateOnUnsuspended) {
            this.target = target;
            this.autoInvalidateOnUnsuspended = autoInvalidateOnUnsuspended;
            target.SetValue(SuspendedInvalidationCountProperty, target.GetValue(SuspendedInvalidationCountProperty) + 1);
        }

        public void Dispose() {
            ApplicationPFX.Instance.Dispatcher.VerifyAccess();
            if (this.target == null) {
                return;
            }

            totalSuspensionCount--;
            int count = GetSuspendedInvalidationCount(this.target);
            if (count < 0) {
                Debugger.Break();
                return;
            }

            if (count == 1) {
                this.target.SetValue(SuspendedInvalidationCountProperty, SuspendedInvalidationCountProperty.GetDefaultValue(this.target.GetType()));
                if (this.autoInvalidateOnUnsuspended) {
                    InvalidateInheritedContext(this.target);
                }
            }
            else {
                this.target.SetValue(SuspendedInvalidationCountProperty, count - 1);
            }

            this.target = null;
        }
    }
}
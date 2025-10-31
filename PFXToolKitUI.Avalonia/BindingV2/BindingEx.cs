// 
// Copyright (c) 2025-2025 REghZy
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

// using Avalonia.Controls;
// using Avalonia.Controls.Primitives;
//
// namespace PFXToolKitUI.Avalonia.BindingV2;
//
// public static class BindingEx {
//     private static readonly Dictionary<Type, ControlInfo> controlTypeToInfoMap = new Dictionary<Type, ControlInfo>();
//
//     /// <summary>
//     /// Marks the control as 'attached' when the template is applied
//     /// </summary>
//     /// <param name="control">'this'</param>
//     /// <exception cref="InvalidOperationException">No binding layout</exception>
//     public static void AttachTemplated(TemplatedControl control) {
//         if (!controlTypeToInfoMap.TryGetValue(control.GetType(), out ControlInfo? info)) {
//             throw new InvalidOperationException("Binding layout not registered");
//         }
//
//         info.AttachOnTemplateApplied(control);
//     }
//
//     /// <summary>
//     /// Immediately sets up the control's bindings
//     /// </summary>
//     /// <param name="control">'this'</param>
//     /// <exception cref="InvalidOperationException">No binding layout</exception>
//     public static void Init(Control control) {
//         if (!controlTypeToInfoMap.TryGetValue(control.GetType(), out ControlInfo? info)) {
//             throw new InvalidOperationException("Binding layout not registered");
//         }
//
//         info.DoAttach(control);
//     }
//
//     public static BindingLayoutBuilder BeginLayout(Type controlType) {
//         if (controlTypeToInfoMap.ContainsKey(controlType))
//             throw new InvalidOperationException($"Control type already has a binding layout registered: {controlType}");
//
//         return new BindingLayoutBuilder(controlType);
//     }
//
//     internal static void CompleteLayout(BindingLayoutBuilder builder) {
//         if (controlTypeToInfoMap.ContainsKey(builder.controlType))
//             throw new InvalidOperationException($"Control type already has a binding layout registered: {builder.controlType}");
//
//         controlTypeToInfoMap[builder.controlType] = ControlInfo.Build(builder);
//     }
//
//     internal class ControlInfo {
//         private readonly Type type;
//
//         public ControlInfo(Type type) {
//             this.type = type;
//         }
//         
//         public static ControlInfo Build(BindingLayoutBuilder builder) {
//             ControlInfo info = new ControlInfo(builder.controlType);
//             return info;
//         }
//
//         public void AttachOnTemplateApplied(TemplatedControl control) {
//             // assume template not applied yet
//             control.TemplateApplied += this.OnTemplateApplied;
//         }
//
//         public void DoAttach(Control control) {
//             this.DetachInternal();
//             INameScope? ns = control.FindNameScope();
//             if (ns != null) {
//                 this.AttachInternal(ns);
//             }
//         }
//
//         private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) {
//             this.DetachInternal();
//             this.AttachInternal(e.NameScope);
//         }
//
//         private void DetachInternal() {
//         }
//
//         private void AttachInternal(INameScope scope) {
//         }
//     }
// }
//
// /// <summary>
// /// A builder used to setup the binding structure of a control type
// /// </summary>
// /// <typeparam name="TControl"></typeparam>
// public sealed class BindingLayoutBuilder : IDisposable {
//     internal readonly Type controlType;
//     internal readonly List<InternalEventUpdateBinderInfo> eventUpdaters;
//     
//     internal BindingLayoutBuilder(Type controlType) {
//         this.controlType = controlType;
//         this.eventUpdaters = new List<InternalEventUpdateBinderInfo>();
//     }
//
//     public void AddEventUpdateBinder(string controlName, string eventName, Action<object, object> updateControl) {
//         this.eventUpdaters.Add(new InternalEventUpdateBinderInfo(controlName, eventName, updateControl));
//     }
//
//     public void Dispose() {
//         BindingEx.CompleteLayout(this);
//     }
// }
//
// internal struct InternalEventUpdateBinderInfo(string controlName, string eventName, Action<object, object> updateControl) {
//     public readonly string ControlName = controlName;
//     public readonly string EventName = eventName;
//     public readonly Action<object, object> updateControl = updateControl;
// }
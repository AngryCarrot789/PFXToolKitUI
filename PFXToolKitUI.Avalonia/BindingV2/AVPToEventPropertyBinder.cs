// using System.Runtime.CompilerServices;
// using Avalonia;
// using Avalonia.Controls;
// using PFXToolKitUI.Avalonia.Bindings;
// using PFXToolKitUI.Avalonia.Utils;
//
// namespace PFXToolKitUI.Avalonia.BindingV2;
//
// public class AVPToEventPropertyBinder {
//     private static readonly Dictionary<Type, OwnerInfo> controlInfo = new Dictionary<Type, OwnerInfo>();
//
//     /// <summary>
//     /// Binds the property of the control name to a model and vice versa by use of the updateControl and updateModel callbacks
//     /// </summary>
//     /// <param name="controlName">The name of the control</param>
//     /// <param name="property">The property whose value must change for <see cref="updateModel"/> to be fired</param>
//     /// <param name="modelEventName">The name of the event in the model class</param>
//     /// <param name="updateControl">Invoked when the model's event is fired, to update the control</param>
//     /// <param name="updateModel">Invoked when the property on the control changes, to update the model</param>
//     public static void Bind<TOwner, TControl, TModel, TValue>(string controlName, StyledProperty<TValue> property, string modelEventName, Action<TControl, TModel>? updateControl, Action<TControl, TModel>? updateModel) where TOwner : AvaloniaObject where TControl : AvaloniaObject where TModel : class {
//         if (!controlInfo.TryGetValue(typeof(TOwner), out OwnerInfo? ownerInfo)) {
//             controlInfo[typeof(TOwner)] = ownerInfo = new OwnerInfo(typeof(TOwner));
//         }
//
//         ownerInfo.AssignControl(controlName, property, modelEventName, updateControl, updateModel);
//     }
//
//     private class OwnerInfo {
//         private readonly Type type;
//         private readonly Dictionary<string, Dictionary<AvaloniaProperty, ControlToModelBindingInfo>> bindingMap;
//
//         public OwnerInfo(Type type) {
//             this.type = type;
//         }
//
//         public void AssignControl<TControl, TModel, TValue>(string controlName, StyledProperty<TValue> property, string modelEventName, Action<TControl, TModel>? updateControl, Action<TControl, TModel>? updateModel) where TControl : AvaloniaObject where TModel : class {
//             if (!this.bindingMap.TryGetValue(controlName, out Dictionary<AvaloniaProperty, ControlToModelBindingInfo>? map)) {
//                 this.bindingMap[controlName] = map = new Dictionary<AvaloniaProperty, ControlToModelBindingInfo>();
//             }
//
//             if (map.ContainsKey(property))
//                 throw new InvalidOperationException("Property already bound");
//
//             map[property] = new ControlToModelBindingInfo(controlName, (AvaloniaProperty) property, modelEventName, updateControl, updateModel);
//         }
//
//         private class ControlToModelBindingInfo {
//             public ControlToModelBindingInfo(string controlName, AvaloniaProperty property, string modelEventName, Action<object,object>? updateControl, Action<object,object>? updateModel) {
//                 
//             }
//         }
//
//         public void Attach(INameScope nameScope, object model) {
//             foreach (KeyValuePair<string, Dictionary<AvaloniaProperty, string>> entry in this.bindingMap) {
//                 object? obj = nameScope.Find(entry.Key);
//                 if (!(obj is AvaloniaObject control)) {
//                     continue;
//                 }
//                 
//                 
//             }
//         }
//     }
//
//     public static void Attach<TOwner>(INameScope nameScope, object model) {
//         if (controlInfo.TryGetValue(typeof(TOwner), out OwnerInfo? info)) {
//             info.Attach(nameScope, model);
//         }
//     }
//
//     private class ModelTypeInfo {
//     }
// }